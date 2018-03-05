using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DCS_BIOS;
using HidLibrary;
using Newtonsoft;
using Newtonsoft.Json;

namespace NonVisuals
{
    public class RadioPanelPZ69SRS : RadioPanelPZ69Base, IRadioPanel
    {
        private HashSet<RadioPanelKnobSRS> _radioPanelKnobs = new HashSet<RadioPanelKnobSRS>();
        private CurrentSRSRadioMode _currentUpperRadioMode = CurrentSRSRadioMode.COM1;
        private CurrentSRSRadioMode _currentLowerRadioMode = CurrentSRSRadioMode.COM1;


        private UdpClient _udpReceiveClient;
        private UdpClient _udpSendClient;
        private Thread _srsListeningThread;
        private string _srsReceiveFromIPUdp = "127.0.0.1";
        private string _srsSendToIPUdp = "127.0.0.1";
        private int _srsReceivePortUdp = 7082;
        private int _srsSendPortUdp = 9086;
        private IPEndPoint _ipEndPointReceiverUdp;
        private IPEndPoint _ipEndPointSenderUdp;
        private string _receivedDataUdp = null;
        private readonly object _sendSRSDataLockObject = new object();
        private bool _shutdown;
        private bool _started;
        private List<double> _listMainFrequencies = new List<double>(7) { 0, 0, 0, 0, 0, 0, 0 };
        private List<double> _listGuardFrequencies = new List<double>(7) { 0, 0, 0, 0, 0, 0, 0 };
        private readonly object _freqListLockObject = new object();

        /*Radio1 COM1*/
        /*Radio2 COM2*/
        /*Radio3 NAV1*/
        /*Radio4 NAV2*/
        /*Radio5 ADF*/
        /*Radio6 DME*/
        /*Radio7 XPDR*/
        //Large dial
        //Small dial

        private int _largeDialSkipper;
        private int _smallDialSkipper;
        private ClickSpeedDetector _largeDialIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _largeDialDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _firstSmallDialIncreaseChangeMonitor = new ClickSpeedDetector(30);
        private ClickSpeedDetector _firstSmallDialDecreaseChangeMonitor = new ClickSpeedDetector(30);
        private ClickSpeedDetector _secondSmallDialIncreaseChangeMonitor = new ClickSpeedDetector(36);
        private ClickSpeedDetector _secondSmallDialDecreaseChangeMonitor = new ClickSpeedDetector(36);

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SRS(string ipAddressFrom, int portFrom, string ipAddressTo, int portTo, HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            _srsReceiveFromIPUdp = ipAddressFrom;
            _srsSendToIPUdp = ipAddressTo;
            _srsReceivePortUdp = portFrom;
            _srsSendPortUdp = portTo;
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
            StartupRP();
        }

        public sealed override void Startup()
        {
            try
            {
                StartupBase("SRS");
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69SRS.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        private void StartupRP()
        {
            try
            {
                ShutdownRP();
                if (_started)
                {
                    return;
                }
                _shutdown = false;
                Common.DebugP("SRS Radio Panel RP is STARTING UP");

                _ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, _srsReceivePortUdp);
                _ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(_srsSendToIPUdp), _srsSendPortUdp);

                _udpReceiveClient?.Close();
                _udpReceiveClient = new UdpClient();
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpReceiveClient.ExclusiveAddressUse = false;
                _udpReceiveClient.Client.Bind(_ipEndPointReceiverUdp);
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
                SetLastException(e);
                Common.LogError(9211101, e, "RadioPanelPZ69SRS.StartupRP()");
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

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering SRS Radio Shutdown()");
                ShutdownBase();
                ShutdownRP();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving SRS Radio Shutdown()");
        }

        private void ShutdownRP()
        {
            try
            {
                try
                {
                    _shutdown = true;
                    Common.DebugP("SRS Radio Panel RP is SHUTTING DOWN");
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
                SetLastException(ex);
                Common.LogError(9212, ex, "RadioPanelPZ69SRS.ShutdownRP()");
            }
        }


        private void ReceiveDataUdp()
        {
            try
            {
                Common.DebugP("SRS Radio Panel entering threaded receive data loop");
                while (!_shutdown)
                {
                    var byteData = _udpReceiveClient.Receive(ref _ipEndPointReceiverUdp);
                    try
                    {
                        var message = Encoding.UTF8.GetString(byteData, 0, byteData.Length);

                        /*var combinedState = new CombinedRadioState()
                        {
                            RadioInfo = new SRSPlayerRadioInfo(),
                            RadioSendingState = new RadioSendingState(),
                            RadioReceivingState = new RadioReceivingState[11]
                        };*/

                        var srsCombinedRadioState = JsonConvert.DeserializeObject<SRSCombinedRadioState>(message);
                        var srsPlayerRadioInfo = srsCombinedRadioState.RadioInfo;
                        lock (_freqListLockObject)
                        {
                            if (srsPlayerRadioInfo != null)
                            {
                                var update = false;
                                for (var i = 0; i < 7; i++)
                                {
                                    if (Math.Abs(_listMainFrequencies[i] - srsPlayerRadioInfo.radios[i + 1].freq) > 0)
                                    {
                                        _listMainFrequencies[i] = srsPlayerRadioInfo.radios[i].freq;
                                        update = true;
                                    }
                                    if (Math.Abs(_listGuardFrequencies[i] - srsPlayerRadioInfo.radios[i + 1].secFreq) > 0)
                                    {
                                        _listGuardFrequencies[i] = srsPlayerRadioInfo.radios[i].secFreq;
                                        update = true;
                                    }
                                }
                                if (update)
                                {
                                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Common.LogError(242352375, e, "RadioPanelPZ69SRS.ReceiveDataUdp()");
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                SetLastException(e);
                Common.LogError(94413, e, "RadioPanelPZ69SRS.ReceiveDataUdp()");
            }
            Common.DebugP("SRS Radio Panel exiting threaded receive data loop");
        }

        private int SendDataFunction(string stringData)
        {
            var result = 0;
            lock (_sendSRSDataLockObject)
            {
                try
                {
                    var unicodeBytes = Encoding.Unicode.GetBytes(stringData);
                    var asciiBytes = new List<byte>(stringData.Length);
                    asciiBytes.AddRange(Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes));
                    result = _udpSendClient.Send(asciiBytes.ToArray(), asciiBytes.ToArray().Length, _ipEndPointSenderUdp);
                }
                catch (Exception e)
                {
                    DBCommon.DebugP("Error sending data to SRS. " + e.Message + Environment.NewLine + e.StackTrace);
                    SetLastException(e);
                    DBCommon.LogError(9216101, e, "RadioPanelPZ69SRS.SendDataFunction()");
                }
            }
            return result;
        }

        private void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering SRS Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (_lockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobSRS)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSRS.UPPER_COM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.COM1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_COM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.COM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NAV1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NAV1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NAV2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NAV2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_DME:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.DME);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_XPDR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.XPDR);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_COM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.COM1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_COM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.COM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NAV1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NAV1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NAV2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NAV2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_DME:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.DME);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_XPDR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.XPDR);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH:
                                {
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH:
                                {
                                    break;
                                }
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82006, ex);
            }
            Common.DebugP("Leaving SRS Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering SRS Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobSRS = (RadioPanelKnobSRS)o;
                    if (radioPanelKnobSRS.IsOn)
                    {
                        switch (radioPanelKnobSRS.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    _largeDialIncreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        var changeValue = 1.0;
                                        if (_largeDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 10.0;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _largeDialDecreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        var changeValue = -1.0;
                                        if (_largeDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -10.0;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    _firstSmallDialIncreaseChangeMonitor.Click();
                                    _secondSmallDialIncreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = 0.001;
                                        if (_firstSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.01;
                                        }
                                        if (_secondSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    _firstSmallDialDecreaseChangeMonitor.Click();
                                    _secondSmallDialDecreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = -0.001;
                                        if (_firstSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.01;
                                        }
                                        if (_secondSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    _largeDialIncreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        var changeValue = 1.0;
                                        if (_largeDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 10.0;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _largeDialDecreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        var changeValue = -1.0;
                                        if (_largeDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -10.0;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    _firstSmallDialIncreaseChangeMonitor.Click();
                                    _secondSmallDialIncreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = 0.001;
                                        if (_firstSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.01;
                                        }
                                        if (_secondSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    _firstSmallDialDecreaseChangeMonitor.Click();
                                    _secondSmallDialDecreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = -0.001;
                                        if (_firstSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.01;
                                        }
                                        if (_secondSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                        SendDataFunction(command);
                                    }
                                    break;
                                }
                        }
                    }
                }
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.LogError(82007, ex);
            }
            Common.DebugP("Leaving SRS Radio AdjustFrequency()");
        }

        private string GetFreqChangeSendCommand(CurrentSRSRadioMode currentSRSRadioMode, double value)
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
            var result = "{ \"Command\": 0,\"RadioId\":" + radioId + ",\"Frequency\": " + value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " }\n";
            Common.DebugP(result);
            return result;
        }

        private void ShowFrequenciesOnPanel()
        {
            try
            {
                lock (_lockShowFrequenciesOnPanelObject)
                {
                    if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                    {
                        return;
                    }

                    if (!FirstReportHasBeenRead)
                    {
                        return;
                    }

                    Common.DebugP("Entering SRS Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;
                    double mainUpperFrequency = 0;
                    double guardUpperFrequency = 0;
                    double mainLowerFrequency = 0;
                    double guardLowerFrequency = 0;
                    lock (_freqListLockObject)
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentSRSRadioMode.COM1:
                                {
                                    mainUpperFrequency = _listMainFrequencies[0];
                                    guardUpperFrequency = _listGuardFrequencies[0];
                                    break;
                                }
                            case CurrentSRSRadioMode.COM2:
                                {
                                    mainUpperFrequency = _listMainFrequencies[1];
                                    guardUpperFrequency = _listGuardFrequencies[1];
                                    break;
                                }
                            case CurrentSRSRadioMode.NAV1:
                                {
                                    mainUpperFrequency = _listMainFrequencies[2];
                                    guardUpperFrequency = _listGuardFrequencies[2];
                                    break;
                                }
                            case CurrentSRSRadioMode.NAV2:
                                {
                                    mainUpperFrequency = _listMainFrequencies[3];
                                    guardUpperFrequency = _listGuardFrequencies[3];
                                    break;
                                }
                            case CurrentSRSRadioMode.ADF:
                                {
                                    mainUpperFrequency = _listMainFrequencies[4];
                                    guardUpperFrequency = _listGuardFrequencies[4];
                                    break;
                                }
                            case CurrentSRSRadioMode.DME:
                                {
                                    mainUpperFrequency = _listMainFrequencies[5];
                                    guardUpperFrequency = _listGuardFrequencies[5];
                                    break;
                                }
                            case CurrentSRSRadioMode.XPDR:
                                {
                                    mainUpperFrequency = _listMainFrequencies[6];
                                    guardUpperFrequency = _listGuardFrequencies[6];
                                    break;
                                }
                        }

                        switch (_currentLowerRadioMode)
                        {
                            case CurrentSRSRadioMode.COM1:
                                {
                                    mainLowerFrequency = _listMainFrequencies[0];
                                    guardLowerFrequency = _listGuardFrequencies[0];
                                    break;
                                }
                            case CurrentSRSRadioMode.COM2:
                                {
                                    mainLowerFrequency = _listMainFrequencies[1];
                                    guardLowerFrequency = _listGuardFrequencies[1];
                                    break;
                                }
                            case CurrentSRSRadioMode.NAV1:
                                {
                                    mainLowerFrequency = _listMainFrequencies[2];
                                    guardLowerFrequency = _listGuardFrequencies[2];
                                    break;
                                }
                            case CurrentSRSRadioMode.NAV2:
                                {
                                    mainLowerFrequency = _listMainFrequencies[3];
                                    guardLowerFrequency = _listGuardFrequencies[3];
                                    break;
                                }
                            case CurrentSRSRadioMode.ADF:
                                {
                                    mainLowerFrequency = _listMainFrequencies[4];
                                    guardLowerFrequency = _listGuardFrequencies[4];
                                    break;
                                }
                            case CurrentSRSRadioMode.DME:
                                {
                                    mainLowerFrequency = _listMainFrequencies[5];
                                    guardLowerFrequency = _listGuardFrequencies[5];
                                    break;
                                }
                            case CurrentSRSRadioMode.XPDR:
                                {
                                    mainLowerFrequency = _listMainFrequencies[6];
                                    guardLowerFrequency = _listGuardFrequencies[6];
                                    break;
                                }
                        }
                    }

                    if (mainUpperFrequency > 0)
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, mainUpperFrequency, PZ69LCDPosition.UPPER_LEFT);
                    }
                    else
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                    }
                    if (guardUpperFrequency > 0)
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, guardUpperFrequency, PZ69LCDPosition.UPPER_RIGHT);
                    }
                    else
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_RIGHT);
                    }
                    if (mainLowerFrequency > 0)
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, mainLowerFrequency, PZ69LCDPosition.LOWER_LEFT);
                    }
                    else
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                    }
                    if (guardLowerFrequency > 0)
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, guardLowerFrequency, PZ69LCDPosition.LOWER_RIGHT);
                    }
                    else
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_RIGHT);
                    }
                    SendLCDData(bytes);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving SRS Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering SRS Radio OnReport()");
                    //if (IsAttached == false) { return; }

                    if (report.Data.Length == 3)
                    {
                        Array.Copy(NewRadioPanelValue, OldRadioPanelValue, 3);
                        Array.Copy(report.Data, NewRadioPanelValue, 3);
                        var hashSet = GetHashSetOfChangedKnobs(OldRadioPanelValue, NewRadioPanelValue);
                        PZ69KnobChanged(hashSet);
                        OnSwitchesChanged(hashSet);
                        FirstReportHasBeenRead = true;
                        if (1 == 2 && Common.DebugOn)
                        {
                            var stringBuilder = new StringBuilder();
                            for (var i = 0; i < report.Data.Length; i++)
                            {
                                stringBuilder.Append(Convert.ToString(report.Data[i], 2).PadLeft(8, '0') + "  ");
                            }
                            Common.DebugP(stringBuilder.ToString());
                            if (hashSet.Count > 0)
                            {
                                Common.DebugP("\nFollowing knobs has been changed:\n");
                                foreach (var radioPanelKnob in hashSet)
                                {
                                    var knob = (RadioPanelKnobSRS)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobSRS)radioPanelKnob));
                                }
                            }
                        }
                        Common.DebugP("\r\nDone!\r\n");
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message + "\n" + ex.StackTrace);
                    SetLastException(ex);
                }
                try
                {
                    if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                    {
                        Common.DebugP("Adding callback " + TypeOfSaitekPanel + " " + GuidString);
                        HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message + "\n" + ex.StackTrace);
                    SetLastException(ex);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82012, ex);
            }
            Common.DebugP("Leaving SRS Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering SRS Radio GetHashSetOfChangedKnobs()");


                for (var i = 0; i < 3; i++)
                {
                    var oldByte = oldValue[i];
                    var newByte = newValue[i];

                    foreach (var radioPanelKnob in _radioPanelKnobs)
                    {
                        if (radioPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, radioPanelKnob.Mask) || !FirstReportHasBeenRead))
                        {
                            radioPanelKnob.IsOn = FlagValue(newValue, radioPanelKnob);
                            result.Add(radioPanelKnob);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82013, ex);
            }
            Common.DebugP("Leaving SRS Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override void ClearSettings()
        {
            //todo
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            //todo
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            _radioPanelKnobs = RadioPanelKnobSRS.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobSRS radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering SRS Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentBf109RadioMode);
                _currentUpperRadioMode = currentBf109RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(82014, ex);
            }
            Common.DebugP("Leaving SRS Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering SRS Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentBf109RadioMode);
                _currentLowerRadioMode = currentBf109RadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(82015, ex);
            }
            Common.DebugP("Leaving SRS Radio SetLowerRadioMode()");
        }

        private bool SkipSmallDialDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipSmallDialDialChange()");
                if (_smallDialSkipper > 2)
                {
                    _smallDialSkipper = 0;
                    Common.DebugP("Leaving SRS Radio SkipSmallDialDialChange()");
                    return false;
                }
                _smallDialSkipper++;
                Common.DebugP("Leaving SRS Radio SkipSmallDialDialChange()");
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(82009, ex);
            }
            return false;
        }

        private bool SkipLargeDialDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipLargeDialDialChange()");
                if (_largeDialSkipper > 2)
                {
                    _largeDialSkipper = 0;
                    Common.DebugP("Leaving SRS Radio SkipLargeDialDialChange()");
                    return false;
                }
                _largeDialSkipper++;
                Common.DebugP("Leaving SRS Radio SkipLargeDialDialChange()");
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(82009, ex);
            }
            return false;
        }


        public override String SettingsVersion()
        {
            return "0X";
        }


        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {
            }
            catch (Exception e)
            {
                Common.LogError(78030, e, "DCSBIOSStringReceived()");
            }
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.LogError(82001, ex);
            }
        }

        public string ReceiveFromIpUdp
        {
            get => _srsReceiveFromIPUdp;
            set => _srsReceiveFromIPUdp = value;
        }

        public string SendToIpUdp
        {
            get => _srsSendToIPUdp;
            set => _srsSendToIPUdp = value;
        }

        public int ReceivePortUdp
        {
            get => _srsReceivePortUdp;
            set => _srsReceivePortUdp = value;
        }

        public int SendPortUdp
        {
            get => _srsSendPortUdp;
            set => _srsSendPortUdp = value;
        }
    }
}
