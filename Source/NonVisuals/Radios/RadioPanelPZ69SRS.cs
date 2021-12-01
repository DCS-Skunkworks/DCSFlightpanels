namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;

    using MEF;
    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Radios.SRS;
    using NonVisuals.Saitek;

    public struct SRSRadioSmallFreqStepping
    {
        public const double One = 0.001;
        public const double Five = 0.005;
        public const double Ten = 0.010;
        public const double Fifty = 0.050;
    }

    public class RadioPanelPZ69SRS : RadioPanelPZ69Base, ISRSDataListener
    {
        private CurrentSRSRadioMode _currentUpperRadioMode = CurrentSRSRadioMode.COM1;
        private CurrentSRSRadioMode _currentLowerRadioMode = CurrentSRSRadioMode.COM1;

        // private List<double> _listMainFrequencies = new List<double>(7) { 0, 0, 0, 0, 0, 0, 0 };
        // private List<double> _listGuardFrequencies = new List<double>(7) { 0, 0, 0, 0, 0, 0, 0 };
        private readonly object _freqListLockObject = new object();

        /*Radio1 COM1*/
        /*Radio2 COM2*/
        /*Radio3 NAV1*/
        /*Radio4 NAV2*/
        /*Radio5 ADF*/
        /*Radio6 DME*/
        /*Radio7 XPDR*/
        // Large dial
        // Small dial
        private double _upperMainFreq;
        private double _lowerMainFreq;
        private double _upperGuardFreq;
        private double _lowerGuardFreq;
        private long _upperFreqSwitchPressed;
        private long _lowerFreqSwitchPressed;
        private int _largeDialSkipper;
        private int _smallDialSkipper;
        private readonly ClickSpeedDetector _largeDialIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _largeDialDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _firstSmallDialIncreaseChangeMonitor = new ClickSpeedDetector(30);
        private readonly ClickSpeedDetector _firstSmallDialDecreaseChangeMonitor = new ClickSpeedDetector(30);
        private readonly ClickSpeedDetector _secondSmallDialIncreaseChangeMonitor = new ClickSpeedDetector(36);
        private readonly ClickSpeedDetector _secondSmallDialDecreaseChangeMonitor = new ClickSpeedDetector(36);

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;
        private double _smallFreqStepping = 0.001;

        public RadioPanelPZ69SRS(int portFrom, string ipAddressTo, int portTo, HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            SRSListenerFactory.SetParams(portFrom, ipAddressTo, portTo);
            SRSListenerFactory.GetSRSListener().Attach(this);
            CreateRadioKnobs();
            Startup();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SRSListenerFactory.GetSRSListener().Detach(this);
                    SRSListenerFactory.Shutdown();
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public sealed override void Startup()
        {
            try
            {
                StartupBase("SRS");
                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }
        
        public double SmallFreqStepping
        {
            get => _smallFreqStepping;
            set => _smallFreqStepping = value;
        }

        public void SRSDataReceived(object sender)
        {
            try
            {
                _upperMainFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentUpperRadioMode);
                _upperGuardFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentUpperRadioMode, true);
                _lowerMainFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentLowerRadioMode);
                _lowerGuardFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentLowerRadioMode, true);
                Interlocked.Increment(ref _doUpdatePanelLCD);
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            try
            {
                lock (LockLCDUpdateObject)
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
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        _upperFreqSwitchPressed = DateTime.Now.Ticks;
                                    }

                                    if (!radioPanelKnob.IsOn)
                                    {
                                        var timeSpan = new TimeSpan(DateTime.Now.Ticks - _upperFreqSwitchPressed);
                                        if (timeSpan.Seconds <= 2)
                                        {
                                            SRSListenerFactory.GetSRSListener().ToggleBetweenGuardAndFrequency(_currentUpperRadioMode);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        _lowerFreqSwitchPressed = DateTime.Now.Ticks;
                                    }

                                    if (!radioPanelKnob.IsOn)
                                    {
                                        var timeSpan = new TimeSpan(DateTime.Now.Ticks - _lowerFreqSwitchPressed);
                                        if (timeSpan.Seconds <= 2)
                                        {
                                            SRSListenerFactory.GetSRSListener().ToggleBetweenGuardAndFrequency(_currentLowerRadioMode);
                                        }
                                    }

                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(ProfileHandler.SelectedProfile().Description, HIDInstanceId, (int)PluginGamingPanelEnum.PZ69RadioPanel, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }

                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
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
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentUpperRadioMode, true);
                                        }
                                        else
                                        {
                                            var changeValue = 1.0;
                                            if (_largeDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = 5.0;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _largeDialDecreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentUpperRadioMode, false);
                                        }
                                        else
                                        {
                                            var changeValue = -1.0;
                                            if (_largeDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = -5.0;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    _firstSmallDialIncreaseChangeMonitor.Click();
                                    _secondSmallDialIncreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentUpperRadioMode, true);
                                        }
                                        else
                                        {
                                            var changeValue = _smallFreqStepping;
                                            if (_firstSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 10;
                                            }

                                            if (_secondSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 100;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    _firstSmallDialDecreaseChangeMonitor.Click();
                                    _secondSmallDialDecreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentUpperRadioMode, false);
                                        }
                                        else
                                        {
                                            var changeValue = _smallFreqStepping * -1;
                                            if (_firstSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 10;
                                            }

                                            if (_secondSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 100;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    _largeDialIncreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentLowerRadioMode, true);
                                        }
                                        else
                                        {
                                            var changeValue = 1.0;
                                            if (_largeDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = 10.0;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _largeDialDecreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentLowerRadioMode, false);
                                        }
                                        else
                                        {
                                            var changeValue = -1.0;
                                            if (_largeDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = -10.0;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    _firstSmallDialIncreaseChangeMonitor.Click();
                                    _secondSmallDialIncreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentLowerRadioMode, true);
                                        }
                                        else
                                        {
                                            var changeValue = _smallFreqStepping;
                                            if (_firstSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 10;
                                            }

                                            if (_secondSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 100;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    _firstSmallDialDecreaseChangeMonitor.Click();
                                    _secondSmallDialDecreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().ChangeChannel(_currentLowerRadioMode, false);
                                        }
                                        else
                                        {
                                            var changeValue = _smallFreqStepping * -1;
                                            if (_firstSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 10;
                                            }

                                            if (_secondSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = changeValue * 100;
                                            }

                                            SRSListenerFactory.GetSRSListener().ChangeFrequency(_currentUpperRadioMode, changeValue);
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
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

                    var bytes = new byte[21];
                    bytes[0] = 0x0;
                    lock (_freqListLockObject)
                    {
                        if (_upperMainFreq > 0)
                        {
                            if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)Math.Floor(_upperMainFreq), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, (_upperMainFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                        }

                        if (_upperGuardFreq > 0)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, (_upperGuardFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                        }

                        if (_lowerMainFreq > 0)
                        {
                            if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)Math.Floor(_lowerMainFreq), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, (_lowerMainFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }

                        if (_lowerGuardFreq > 0)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, (_lowerGuardFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }

                        SendLCDData(bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }


        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobSRS.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentBf109RadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentBf109RadioMode;

                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool SkipSmallDialDialChange()
        {
            try
            {
                if (_smallDialSkipper > 2)
                {
                    _smallDialSkipper = 0;
                    return false;
                }

                _smallDialSkipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipLargeDialDialChange()
        {
            try
            {
                if (_largeDialSkipper > 2)
                {
                    _largeDialSkipper = 0;
                    return false;
                }

                _largeDialSkipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }


        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex, "DCSBIOSStringReceived()");
            }
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }
    }
}
