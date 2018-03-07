﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DCS_BIOS;
using Newtonsoft.Json;

namespace NonVisuals
{
    public interface ISRSDataListener
    {
        void SRSDataReceived();
    }

    public static class SRSListenerFactory
    {
        private static SRSListener _srsListener = null;
        //private string _srsReceiveFromIPUdp = "127.0.0.1";
        private static string _srsSendToIPUdp = "127.0.0.1";
        private static int _srsReceivePortUdp = 7082;
        private static int _srsSendPortUdp = 9086;

        public static void SetParams(int portFrom, string ipAddressTo, int portTo)
        {
            //_srsReceiveFromIPUdp = ipAddressFrom;
            var restart = _srsReceivePortUdp != portFrom || _srsSendPortUdp != portTo || _srsSendToIPUdp != ipAddressTo;

            _srsSendToIPUdp = ipAddressTo;
            _srsReceivePortUdp = portFrom;
            _srsSendPortUdp = portTo;
            if (restart)
            {
                _srsListener?.ShutdownRP();
                var srsListener = GetSRSListener();
            }
        }

        public static void Shutdown()
        {
            _srsListener?.ShutdownRP();
            _srsListener = null;
        }

        public static SRSListener GetSRSListener()
        {
            if (_srsListener == null)
            {
                _srsListener = new SRSListener(_srsReceivePortUdp, _srsSendToIPUdp, _srsSendPortUdp);
            }
            return _srsListener;
        }
    }

    public class SRSListener
    {
        public delegate void SRSDataReceivedEventHandler();
        public event SRSDataReceivedEventHandler OnSRSDataReceived;

        public void Attach(ISRSDataListener srsDataListener)
        {
            OnSRSDataReceived += srsDataListener.SRSDataReceived;
        }

        public void Detach(ISRSDataListener srsDataListener)
        {
            OnSRSDataReceived -= srsDataListener.SRSDataReceived;
        }

        private UdpClient _udpReceiveClient;
        private UdpClient _udpSendClient;
        private Thread _srsListeningThread;
        //private string _srsReceiveFromIPUdp = "127.0.0.1";
        private readonly string _srsSendToIPUdp;
        private readonly int _srsReceivePortUdp;
        private readonly int _srsSendPortUdp;
        //private IPEndPoint _ipEndPointReceiverUdp;
        //private IPEndPoint _ipEndPointSenderUdp;
        private SRSPlayerRadioInfo _srsPlayerRadioInfo = null;
        private bool _shutdown;
        private bool _started;
        private readonly object _sendSRSDataLockObject = new object();
        private readonly object _readSRSDataLockObject = new object();

        public SRSListener(int portFrom, string ipAddressTo, int portTo)
        {
            _srsSendToIPUdp = ipAddressTo;
            _srsReceivePortUdp = portFrom;
            _srsSendPortUdp = portTo;
            StartupRP();
        }

        private void ReceiveDataUdp()
        {
            try
            {
                Common.DebugP("SRSListener entering threaded receive data loop");
                while (!_shutdown)
                {
                    var ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, _srsReceivePortUdp);
                    var byteData = _udpReceiveClient.Receive(ref ipEndPointReceiverUdp);
                    try
                    {
                        var message = Encoding.UTF8.GetString(byteData, 0, byteData.Length);
                        //Console.WriteLine(HIDSkeletonBase.InstanceId + " Message received on UDP");
                        var srsCombinedRadioState = JsonConvert.DeserializeObject<SRSCombinedRadioState>(message);
                        var srsPlayerRadioInfo = srsCombinedRadioState.RadioInfo;
                        if (srsPlayerRadioInfo != null)
                        {
                            lock (_readSRSDataLockObject)
                            {
                                _srsPlayerRadioInfo = srsPlayerRadioInfo;
                            }
                            OnSRSDataReceived?.Invoke();
                        }
                    }
                    catch (Exception e)
                    {
                        Common.LogError(242352375, e, "SRSListener.ReceiveDataUdp()");
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Common.LogError(94413, e, "SRSListener.ReceiveDataUdp()");
            }
            Common.DebugP("SRSListener exiting threaded receive data loop");
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
                    result = _udpSendClient.Send(asciiBytes.ToArray(), asciiBytes.ToArray().Length, ipEndPointSenderUdp);
                }
                catch (Exception e)
                {
                    Common.DebugP("Error sending data to SRS. " + e.Message + Environment.NewLine + e.StackTrace);
                    Common.LogError(9216101, e, "Error sending data to SRS. " + e.Message + Environment.NewLine + e.StackTrace);
                }
            }
            return result;
        }

        private void StartupRP()
        {
            try
            {
                if (_started)
                {
                    return;
                }
                _shutdown = false;
                Common.DebugP("SRSListener STARTING UP");

                var ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, _srsReceivePortUdp);
                var ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(_srsSendToIPUdp), _srsSendPortUdp);

                _udpReceiveClient?.Close();
                _udpReceiveClient = new UdpClient();
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpReceiveClient.ExclusiveAddressUse = false;
                _udpReceiveClient.Client.Bind(ipEndPointReceiverUdp);
                //_udpReceiveClient.JoinMulticastGroup(IPAddress.Parse(_srsReceiveFromIPUdp));

                _udpSendClient?.Close();
                _udpSendClient = new UdpClient();
                _udpSendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSendClient.EnableBroadcast = true;

                _srsListeningThread?.Abort();
                _srsListeningThread = new Thread(ReceiveDataUdp);
                _srsListeningThread.Start();

                _started = true;
            }
            catch (Exception e)
            {
                Common.LogError(9211101, e, "SRSListener.StartupRP()");
                if (_udpReceiveClient != null && _udpReceiveClient.Client.Connected)
                {
                    _udpReceiveClient.Close();
                    _udpReceiveClient = null;
                }
                if (_udpSendClient != null && _udpSendClient.Client.Connected)
                {
                    _udpSendClient.Close();
                    _udpSendClient = null;
                }
            }
        }

        public double GetFrequency(CurrentSRSRadioMode currentSRSRadioMode, bool guard = false)
        {
            lock (_readSRSDataLockObject)
            {
                switch (currentSRSRadioMode)
                {
                    case CurrentSRSRadioMode.COM1:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[1].freq;
                            }

                            return _srsPlayerRadioInfo.radios[1].secFreq;
                        }
                    case CurrentSRSRadioMode.COM2:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[2].freq;
                            }

                            return _srsPlayerRadioInfo.radios[2].secFreq;
                        }
                    case CurrentSRSRadioMode.NAV1:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[3].freq;
                            }

                            return _srsPlayerRadioInfo.radios[3].secFreq;
                        }
                    case CurrentSRSRadioMode.NAV2:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[4].freq;
                            }

                            return _srsPlayerRadioInfo.radios[4].secFreq;
                        }
                    case CurrentSRSRadioMode.ADF:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[5].freq;
                            }

                            return _srsPlayerRadioInfo.radios[5].secFreq;
                        }
                    case CurrentSRSRadioMode.DME:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[6].freq;
                            }

                            return _srsPlayerRadioInfo.radios[6].secFreq;
                        }
                    case CurrentSRSRadioMode.XPDR:
                        {
                            if (!guard)
                            {
                                return _srsPlayerRadioInfo.radios[7].freq;
                            }

                            return _srsPlayerRadioInfo.radios[7].secFreq;
                        }
                }
            }
            return -1;
        }

        public void ShutdownRP()
        {
            try
            {
                try
                {
                    _shutdown = true;
                    Common.DebugP("SRSListener RP is SHUTTING DOWN");
                    _srsListeningThread?.Abort();
                }
                catch (Exception)
                {
                }
                try
                {
                    _udpReceiveClient?.Close();
                }
                catch (Exception)
                {
                }
                try
                {
                    _udpSendClient?.Close();
                }
                catch (Exception)
                {
                }
                _started = false;
            }
            catch (Exception ex)
            {
                Common.LogError(9212, ex, "SRSListener.ShutdownRP()");
            }
        }


    }
}
