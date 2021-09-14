namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Saitek;

    public class RadioPanelPZ69AV8BNA : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private CurrentAV8BNARadioMode _currentUpperRadioMode = CurrentAV8BNARadioMode.COMM1;
        private CurrentAV8BNARadioMode _currentLowerRadioMode = CurrentAV8BNARadioMode.COMM1;
        
        /* COMM1 V/UHF AN/ARC-210 */
        // Large dial xxx-xxx [step of 1]
        // Small dial 0.00-0.97 [step of x.x[0 2 5 7]
        private const string COMM1_CHANNEL_INC = "UFC_COM1_SEL +1000\n";
        private const string COMM1_CHANNEL_DEC = "UFC_COM1_SEL -1000\n";
        private const string COMM1_VOL_INC = "UFC_COM1_VOL +4000\n";
        private const string COMM1_VOL_DEC = "UFC_COM1_VOL -4000\n";
        private const string COMM1_PULL_PRESS = "UFC_COM1_PULL INC\n";
        private const string COMM1_PULL_RELEASE = "UFC_COM1_PULL DEC\n";
        private readonly object _lockCOMM1DialsObject = new object();
        private DCSBIOSOutput _comm1DcsbiosOutputFreq;
        private string _comm1Frequency = "225.000";
        private readonly ClickSpeedDetector _comm1ChannelClickSpeedDetector = new ClickSpeedDetector(8);

        /* COMM2 V/UHF AN/ARC-210 */
        // Large dial xxx-xxx [step of 1]
        // Small dial 0.00-0.97 [step of x.x[0 2 5 7]
        private const string COMM2_CHANNEL_INC = "UFC_COM2_SEL +3200\n";
        private const string COMM2_CHANNEL_DEC = "UFC_COM2_SEL -3200\n";
        private const string COMM2_VOL_INC = "UFC_COM2_VOL +4000\n";
        private const string COMM2_VOL_DEC = "UFC_COM2_VOL -4000\n";
        private const string COMM2_PULL_PRESS = "UFC_COM2_PULL INC\n";
        private const string COMM2_PULL_RELEASE = "UFC_COM2_PULL DEC\n";
        private readonly object _lockCOMM2DialsObject = new object();
        private DCSBIOSOutput _comm2DcsbiosOutputFreq;
        private string _comm2Frequency = "225.000";
        private readonly ClickSpeedDetector _comm2ChannelClickSpeedDetector = new ClickSpeedDetector(8);

        private readonly object _lockShowFrequenciesOnPanelObject = new object();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69AV8BNA(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }
        
        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateCounter(e.Address, e.Data);


            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */

            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (e.Address == _comm1DcsbiosOutputFreq.Address)
                {
                    lock (_lockCOMM1DialsObject)
                    {
                        var tmp = _comm1Frequency;
                        _comm1Frequency = e.StringData;
                        if (tmp != _comm1Frequency)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                if (e.Address == _comm2DcsbiosOutputFreq.Address)
                {
                    lock (_lockCOMM2DialsObject)
                    {
                        var tmp = _comm2Frequency;
                        _comm2Frequency = e.StringData;
                        if (tmp != _comm2Frequency)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelKnobAV8BNA knob)
        {

            if (IgnoreSwitchButtonOnce() && (knob.RadioPanelPZ69Knob == RadioPanelPZ69KnobsAV8BNA.UPPER_FREQ_SWITCH || knob.RadioPanelPZ69Knob == RadioPanelPZ69KnobsAV8BNA.LOWER_FREQ_SWITCH))
            {
                // Don't do anything on the very first button press as the panel sends ALL
                // switches when it is manipulated the first time
                // This would cause unintended sync.
                return;
            }

            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                // Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }

            switch (knob.RadioPanelPZ69Knob)
            {
                case RadioPanelPZ69KnobsAV8BNA.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentAV8BNARadioMode.COMM1:
                                {
                                    DCSBIOS.Send(knob.IsOn ? COMM1_PULL_PRESS : COMM1_PULL_RELEASE);
                                    ShowFrequenciesOnPanel();
                                    break;
                                }

                            case CurrentAV8BNARadioMode.COMM2:
                                {
                                    DCSBIOS.Send(knob.IsOn ? COMM2_PULL_PRESS : COMM2_PULL_RELEASE);
                                    ShowFrequenciesOnPanel();
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelPZ69KnobsAV8BNA.LOWER_FREQ_SWITCH:
                    {

                        switch (_currentLowerRadioMode)
                        {
                            case CurrentAV8BNARadioMode.COMM1:
                                {
                                    DCSBIOS.Send(knob.IsOn ? COMM1_PULL_PRESS : COMM1_PULL_RELEASE);
                                    ShowFrequenciesOnPanel();
                                    break;
                                }

                            case CurrentAV8BNARadioMode.COMM2:
                                {
                                    DCSBIOS.Send(knob.IsOn ? COMM2_PULL_PRESS : COMM2_PULL_RELEASE);
                                    ShowFrequenciesOnPanel();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void ShowFrequenciesOnPanel()
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

                var bytes = new byte[21];
                bytes[0] = 0x0;

                switch (_currentUpperRadioMode)
                {
                    case CurrentAV8BNARadioMode.COMM1:
                        {
                            var frequencyAsString = string.Empty;
                            lock (_lockCOMM1DialsObject)
                            {
                                frequencyAsString = _comm1Frequency;
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);

                        }

                        break;
                    case CurrentAV8BNARadioMode.COMM2:
                        {
                            var frequencyAsString = string.Empty;
                            lock (_lockCOMM2DialsObject)
                            {
                                frequencyAsString = _comm2Frequency;
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);

                        }

                        break;
                    case CurrentAV8BNARadioMode.NOUSE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentAV8BNARadioMode.COMM1:
                        {
                            var frequencyAsString = string.Empty;
                            lock (_lockCOMM1DialsObject)
                            {
                                frequencyAsString = _comm1Frequency;
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);

                        }

                        break;
                    case CurrentAV8BNARadioMode.COMM2:
                        {
                            var frequencyAsString = string.Empty;
                            lock (_lockCOMM2DialsObject)
                            {
                                frequencyAsString = _comm2Frequency;
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);

                        }

                        break;
                    case CurrentAV8BNARadioMode.NOUSE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                }
                SendLCDData(bytes);
            }

            Interlocked.Add(ref _doUpdatePanelLCD, -1);
        }

        private string GetUHFCockpitFrequencyAsString()
        {
            /*var frequencyAsString = string.Empty;
            lock (_lockUhfBigFreqObject1)
            {
                lock (_lockUhfDial3FreqObject2)
                {
                    lock (_lockUhfDial4FreqObject2)
                    {
                        frequencyAsString = _uhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
                        frequencyAsString = frequencyAsString + ".";
                        frequencyAsString = frequencyAsString + _uhfCockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString = frequencyAsString + _uhfCockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');
                        //225.000 7 characters
                    }
                }
            }
            return frequencyAsString;*/
            return string.Empty;
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            if (SkipCurrentFrequencyChange())
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var radioPanelKnobAV8BNA = (RadioPanelKnobAV8BNA)o;
                if (radioPanelKnobAV8BNA.IsOn)
                {
                    switch (radioPanelKnobAV8BNA.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            if (_comm1ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM1_CHANNEL_INC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM1_CHANNEL_INC);
                                            }

                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            if (_comm2ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM2_CHANNEL_INC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM2_CHANNEL_INC);
                                            }

                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            if (_comm1ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM1_CHANNEL_DEC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM1_CHANNEL_DEC);
                                            }

                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            if (_comm2ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM2_CHANNEL_DEC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM2_CHANNEL_DEC);
                                            }

                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            DCSBIOS.Send(COMM1_VOL_INC);
                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_INC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            DCSBIOS.Send(COMM1_VOL_DEC);
                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_DEC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            if (_comm1ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM1_CHANNEL_INC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM1_CHANNEL_INC);
                                            }

                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            if (_comm2ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM2_CHANNEL_INC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM2_CHANNEL_INC);
                                            }

                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            if (_comm1ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM1_CHANNEL_DEC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM1_CHANNEL_DEC);
                                            }

                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            if (_comm2ChannelClickSpeedDetector.ClickAndCheck())
                                            {
                                                // No need for turbo
                                                DCSBIOS.Send(COMM2_CHANNEL_DEC);
                                            }
                                            else
                                            {
                                                DCSBIOS.Send(COMM2_CHANNEL_DEC);
                                            }

                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {

                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            DCSBIOS.Send(COMM1_VOL_INC);
                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_INC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAV8BNARadioMode.COMM1:
                                        {
                                            DCSBIOS.Send(COMM1_VOL_DEC);
                                            break;
                                        }

                                    case CurrentAV8BNARadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_DEC);
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
            }

            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            ShowFrequenciesOnPanel();
        }


        public void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobAV8BNA)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsAV8BNA.UPPER_COMM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAV8BNARadioMode.COMM1;
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_COMM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAV8BNARadioMode.COMM2;
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_NAV1:
                        case RadioPanelPZ69KnobsAV8BNA.UPPER_NAV2:
                        case RadioPanelPZ69KnobsAV8BNA.UPPER_ADF:
                        case RadioPanelPZ69KnobsAV8BNA.UPPER_DME:
                        case RadioPanelPZ69KnobsAV8BNA.UPPER_XPDR:
                        {
                            if (radioPanelKnob.IsOn)
                            {
                                _currentUpperRadioMode = CurrentAV8BNARadioMode.NOUSE;
                            }

                            break;
                        }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_COMM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAV8BNARadioMode.COMM1;
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_COMM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAV8BNARadioMode.COMM2;
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_NAV1:
                        case RadioPanelPZ69KnobsAV8BNA.LOWER_NAV2:
                        case RadioPanelPZ69KnobsAV8BNA.LOWER_ADF:
                        case RadioPanelPZ69KnobsAV8BNA.LOWER_DME:
                        case RadioPanelPZ69KnobsAV8BNA.LOWER_XPDR:
                        {
                            if (radioPanelKnob.IsOn)
                            {
                                _currentLowerRadioMode = CurrentAV8BNARadioMode.NOUSE;
                            }

                            break;
                        }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.UPPER_FREQ_SWITCH:
                            {
                                SendFrequencyToDCSBIOS(radioPanelKnob);
                                break;
                            }

                        case RadioPanelPZ69KnobsAV8BNA.LOWER_FREQ_SWITCH:
                            {
                                SendFrequencyToDCSBIOS(radioPanelKnob);
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

        public sealed override void Startup()
        {
            try
            {
                StartupBase("AV-8B NA");

                // V/UHF COMM1
                _comm1DcsbiosOutputFreq = DCSBIOSControlLocator.GetDCSBIOSOutput("COMM1_STRING_FREQ");
                DCSBIOSStringManager.AddListener(_comm1DcsbiosOutputFreq, this);

                // V/UHF COMM2
                _comm2DcsbiosOutputFreq = DCSBIOSControlLocator.GetDCSBIOSOutput("COMM2_STRING_FREQ");
                DCSBIOSStringManager.AddListener(_comm2DcsbiosOutputFreq, this);

                StartListeningForPanelChanges();

                // IsAttached = true;
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public override void Dispose()
        {
            try
            {
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
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

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobAV8BNA.GetRadioPanelKnobs();
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

