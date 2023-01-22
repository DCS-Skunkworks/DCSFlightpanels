﻿namespace NonVisuals.Radios.SRS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using Newtonsoft.Json;
    using NLog;
    using NonVisuals.Radios.Knobs;

    public enum SRSRadioMode
    {
        Frequency,
        Channel
    }

    public interface ISRSDataListener
    {
        void SRSDataReceived(object sender);
    }

    public class SRSRadio
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private UdpClient _udpReceiveClient;
        private UdpClient _udpSendClient;
        private Thread _srsListeningThread;
        private readonly string _srsSendToIPUdp;
        private readonly int _srsReceivePortUdp;
        private readonly int _srsSendPortUdp;
        private SRSPlayerRadioInfo _srsPlayerRadioInfo;
        private volatile bool _shutdownThread;
        private bool _started;
        private readonly object _sendSRSDataLockObject = new();
        private readonly object _readSRSDataLockObject = new();
        private IPEndPoint _ipEndPointReceiverUdp;
        private IPEndPoint _ipEndPointSenderUdp;
        private System.Timers.Timer _udpReceiveThrottleTimer = new(10) { AutoReset = true }; //Throttle UDP receive every 10 ms in case nothing is available
        private AutoResetEvent _udpReceiveThrottleAutoResetEvent = new(false);

        public SRSRadio(int portFrom, string ipAddressTo, int portTo)
        {
            _srsSendToIPUdp = ipAddressTo;
            _srsReceivePortUdp = portFrom;
            _srsSendPortUdp = portTo;
            Startup();
        }

        private void ReceiveDataUdp()
        {
            try
            {
                while (!_shutdownThread)
                {
                    try
                    {
                        if (_udpReceiveClient.Available > 0)
                        {
                            var byteData = _udpReceiveClient.Receive(ref _ipEndPointReceiverUdp);
                            var message = Encoding.UTF8.GetString(byteData, 0, byteData.Length);

                            var srsCombinedRadioState = JsonConvert.DeserializeObject<SRSCombinedRadioState>(message);
                            var srsPlayerRadioInfo = srsCombinedRadioState.RadioInfo;
                            if (srsPlayerRadioInfo != null)
                            {
                                lock (_readSRSDataLockObject)
                                {
                                    _srsPlayerRadioInfo = srsPlayerRadioInfo;
                                }

                                OnSRSDataReceived?.Invoke(this);
                            }
                        }
                        _udpReceiveThrottleAutoResetEvent.WaitOne(); // Minimizes CPU hit
                    }
                    catch (SocketException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "SRSListener.ReceiveDataUdp()");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SRSListener.ReceiveDataUdp()");
            }
        }

        public int SendDataFunction(string stringData)
        {
            var result = 0;
            lock (_sendSRSDataLockObject)
            {
                try
                {
                    var ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(_srsSendToIPUdp), _srsSendPortUdp);
                    var unicodeBytes = Encoding.Unicode.GetBytes(stringData);
                    var asciiBytes = new List<byte>(stringData.Length);
                    asciiBytes.AddRange(Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes));
                    result = _udpSendClient.Send(asciiBytes.ToArray(), asciiBytes.ToArray().Length, _ipEndPointSenderUdp);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error sending data to SRS. {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }

            return result;
        }

        private void Startup()
        {
            try
            {
                if (_started)
                {
                    return;
                }

                _shutdownThread = true;

                _ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, _srsReceivePortUdp);
                _ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(_srsSendToIPUdp), _srsSendPortUdp);
                _udpReceiveClient?.Close();
                _udpReceiveClient = new UdpClient();
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 200);
                _udpReceiveClient.ExclusiveAddressUse = false;
                _udpReceiveClient.Client.Bind(_ipEndPointReceiverUdp);
                
                lock (_sendSRSDataLockObject)
                {
                    _udpSendClient?.Close();
                    _udpSendClient = new UdpClient();
                    _udpSendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _udpSendClient.EnableBroadcast = true;
                }

                Thread.Sleep(Constants.ThreadShutDownWaitTime);

                _udpReceiveThrottleTimer.Elapsed += UdpReceiveThrottleTimer_Elapsed;
                _udpReceiveThrottleTimer.Start();
                _shutdownThread = false;
                _srsListeningThread = new Thread(ReceiveDataUdp);
                _srsListeningThread.Start();

                _started = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SRSListener.StartupRP()");
                if (_udpReceiveClient != null && _udpReceiveClient.Client.Connected)
                {
                    _udpReceiveClient.Close();
                    _udpReceiveClient = null;
                }

                lock (_sendSRSDataLockObject)
                {
                    if (_udpSendClient != null && _udpSendClient.Client.Connected)
                    {
                        _udpSendClient.Close();
                        _udpSendClient = null;
                    }
                }
            }
        }
        private void UdpReceiveThrottleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _udpReceiveThrottleAutoResetEvent.Set();
        }

        private CurrentSRSRadioMode TranslateSRSRadioMode(int radioNumber)
        {
            CurrentSRSRadioMode currentSRSRadioMode;

            switch (radioNumber)
            {
                case 1:
                    currentSRSRadioMode = CurrentSRSRadioMode.COM1;
                    break;
                case 2:
                    currentSRSRadioMode = CurrentSRSRadioMode.COM2;
                    break;
                case 3:
                    currentSRSRadioMode = CurrentSRSRadioMode.NAV1;
                    break;
                case 4:
                    currentSRSRadioMode = CurrentSRSRadioMode.NAV2;
                    break;
                case 5:
                    currentSRSRadioMode = CurrentSRSRadioMode.ADF;
                    break;
                case 6:
                    currentSRSRadioMode = CurrentSRSRadioMode.DME;
                    break;
                case 7:
                    currentSRSRadioMode = CurrentSRSRadioMode.XPDR;
                    break;
                default:
                    currentSRSRadioMode = CurrentSRSRadioMode.COM1;
                    break;
            }

            return currentSRSRadioMode;
        }

        public SRSRadioMode GetRadioMode(int radioNumber)
        {
            var currentSRSRadioMode = TranslateSRSRadioMode(radioNumber);
            return GetRadioMode(currentSRSRadioMode);
        }

        public SRSRadioMode GetRadioMode(CurrentSRSRadioMode currentSRSRadioMode)
        {
            lock (_readSRSDataLockObject)
            {
                switch (currentSRSRadioMode)
                {
                    case CurrentSRSRadioMode.COM1:
                        {
                            if (_srsPlayerRadioInfo.radios[1].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }

                    case CurrentSRSRadioMode.COM2:
                        {
                            if (_srsPlayerRadioInfo.radios[2].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }

                    case CurrentSRSRadioMode.NAV1:
                        {
                            if (_srsPlayerRadioInfo.radios[3].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }

                    case CurrentSRSRadioMode.NAV2:
                        {
                            if (_srsPlayerRadioInfo.radios[4].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }

                    case CurrentSRSRadioMode.ADF:
                        {
                            if (_srsPlayerRadioInfo.radios[5].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }

                    case CurrentSRSRadioMode.DME:
                        {
                            if (_srsPlayerRadioInfo.radios[6].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }

                    case CurrentSRSRadioMode.XPDR:
                        {
                            if (_srsPlayerRadioInfo.radios[7].channel == -1)
                            {
                                return SRSRadioMode.Frequency;
                            }

                            return SRSRadioMode.Channel;
                        }
                }
            }

            return SRSRadioMode.Frequency;
        }

        public double GetFrequencyOrChannel(int radioNumber, bool guard = false)
        {
            var currentSRSRadioMode = TranslateSRSRadioMode(radioNumber);
            return GetFrequencyOrChannel(currentSRSRadioMode, guard);
        }

        public double GetFrequencyOrChannel(CurrentSRSRadioMode currentSRSRadioMode, bool guard = false)
        {
            lock (_readSRSDataLockObject)
            {
                switch (currentSRSRadioMode)
                {
                    case CurrentSRSRadioMode.COM1:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[1].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[1].freq;
                                }

                                return _srsPlayerRadioInfo.radios[1].channel;
                            }

                            return _srsPlayerRadioInfo.radios[1].secFreq;
                        }

                    case CurrentSRSRadioMode.COM2:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[2].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[2].freq;
                                }

                                return _srsPlayerRadioInfo.radios[2].channel;
                            }

                            return _srsPlayerRadioInfo.radios[2].secFreq;
                        }

                    case CurrentSRSRadioMode.NAV1:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[3].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[3].freq;
                                }

                                return _srsPlayerRadioInfo.radios[3].channel;
                            }

                            return _srsPlayerRadioInfo.radios[3].secFreq;
                        }

                    case CurrentSRSRadioMode.NAV2:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[4].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[4].freq;
                                }

                                return _srsPlayerRadioInfo.radios[4].channel;
                            }

                            return _srsPlayerRadioInfo.radios[4].secFreq;
                        }

                    case CurrentSRSRadioMode.ADF:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[5].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[5].freq;
                                }

                                return _srsPlayerRadioInfo.radios[5].channel;
                            }

                            return _srsPlayerRadioInfo.radios[5].secFreq;
                        }

                    case CurrentSRSRadioMode.DME:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[6].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[6].freq;
                                }

                                return _srsPlayerRadioInfo.radios[6].channel;
                            }

                            return _srsPlayerRadioInfo.radios[6].secFreq;
                        }

                    case CurrentSRSRadioMode.XPDR:
                        {
                            if (!guard)
                            {
                                if (_srsPlayerRadioInfo.radios[7].channel == -1)
                                {
                                    return _srsPlayerRadioInfo.radios[7].freq;
                                }

                                return _srsPlayerRadioInfo.radios[7].channel;
                            }

                            return _srsPlayerRadioInfo.radios[7].secFreq;
                        }
                }
            }

            return -1;
        }






        public void ChangeFrequency(int radioNumber, double value)
        {
            var currentSRSRadioMode = TranslateSRSRadioMode(radioNumber);
            ChangeFrequency(currentSRSRadioMode, value);
        }

        public void ChangeFrequency(CurrentSRSRadioMode currentSRSRadioMode, double value)
        {
            int radioId;
            switch (currentSRSRadioMode)
            {
                case CurrentSRSRadioMode.COM1:
                    {
                        radioId = 1;
                        break;
                    }

                case CurrentSRSRadioMode.COM2:
                    {
                        radioId = 2;
                        break;
                    }

                case CurrentSRSRadioMode.NAV1:
                    {
                        radioId = 3;
                        break;
                    }

                case CurrentSRSRadioMode.NAV2:
                    {
                        radioId = 4;
                        break;
                    }

                case CurrentSRSRadioMode.ADF:
                    {
                        radioId = 5;
                        break;
                    }

                case CurrentSRSRadioMode.DME:
                    {
                        radioId = 6;
                        break;
                    }

                case CurrentSRSRadioMode.XPDR:
                    {
                        radioId = 7;
                        break;
                    }

                default:
                    {
                        radioId = 1;
                        break;
                    }
            }
            var result = "{ \"Command\": 0,\"RadioId\":" + radioId + ",\"Frequency\": " + value.ToString("0.000", CultureInfo.InvariantCulture) + " }\n";
            SendDataFunction(result);
        }






        public void ToggleGuard(int radioNumber)
        {
            var currentSRSRadioMode = TranslateSRSRadioMode(radioNumber);
            ToggleBetweenGuardAndFrequency(currentSRSRadioMode);
        }

        public void ToggleBetweenGuardAndFrequency(CurrentSRSRadioMode currentSRSRadioMode)
        {
            var radioId = 0;
            switch (currentSRSRadioMode)
            {
                case CurrentSRSRadioMode.COM1:
                    {
                        radioId = 1;
                        break;
                    }

                case CurrentSRSRadioMode.COM2:
                    {
                        radioId = 2;
                        break;
                    }

                case CurrentSRSRadioMode.NAV1:
                    {
                        radioId = 3;
                        break;
                    }

                case CurrentSRSRadioMode.NAV2:
                    {
                        radioId = 4;
                        break;
                    }

                case CurrentSRSRadioMode.ADF:
                    {
                        radioId = 5;
                        break;
                    }

                case CurrentSRSRadioMode.DME:
                    {
                        radioId = 6;
                        break;
                    }

                case CurrentSRSRadioMode.XPDR:
                    {
                        radioId = 7;
                        break;
                    }
            }

            var result = "{\"Command\": 2,\"RadioId\":" + radioId + "}\n";

            // { "Command": 2,"RadioId":2} 
            SendDataFunction(result);
        }

        public void ChangeChannel(int radioNumber, bool increase)
        {
            var currentSRSRadioMode = TranslateSRSRadioMode(radioNumber);
            ChangeChannel(currentSRSRadioMode, increase);
        }

        public void ChangeChannel(CurrentSRSRadioMode currentSRSRadioMode, bool increase)
        {
            var radioId = 0;
            switch (currentSRSRadioMode)
            {
                case CurrentSRSRadioMode.COM1:
                    {
                        radioId = 1;
                        break;
                    }

                case CurrentSRSRadioMode.COM2:
                    {
                        radioId = 2;
                        break;
                    }

                case CurrentSRSRadioMode.NAV1:
                    {
                        radioId = 3;
                        break;
                    }

                case CurrentSRSRadioMode.NAV2:
                    {
                        radioId = 4;
                        break;
                    }

                case CurrentSRSRadioMode.ADF:
                    {
                        radioId = 5;
                        break;
                    }

                case CurrentSRSRadioMode.DME:
                    {
                        radioId = 6;
                        break;
                    }

                case CurrentSRSRadioMode.XPDR:
                    {
                        radioId = 7;
                        break;
                    }
            }

            /*{ "Command": 3,"RadioId":1}
                        --channel up(if channels have been configured)
            
                        { "Command": 4,"RadioId":1}
                        --channel down(if channels have been configured)*/
            string result;
            if (increase)
            {
                result = "{\"Command\": 3,\"RadioId\":" + radioId + "}\n";
            }
            else
            {
                result = "{\"Command\": 4,\"RadioId\":" + radioId + "}\n";
            }

            SendDataFunction(result);
        }

        public void Shutdown()
        {
            try
            {
                try
                {
                    _shutdownThread = true;
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    _udpReceiveClient?.Close();
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    lock (_sendSRSDataLockObject)
                    {
                        _udpSendClient?.Close();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                _started = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SRSListener.ShutdownRP()");
            }
        }


        public delegate void SRSDataReceivedEventHandler(object sender);
        public event SRSDataReceivedEventHandler OnSRSDataReceived;

        public void Attach(ISRSDataListener srsDataListener)
        {
            OnSRSDataReceived += srsDataListener.SRSDataReceived;
        }

        public void Detach(ISRSDataListener srsDataListener)
        {
            OnSRSDataReceived -= srsDataListener.SRSDataReceived;
        }
    }
}
