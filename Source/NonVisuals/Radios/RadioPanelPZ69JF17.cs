using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using MEF;
    using Plugin;
    using Knobs;
    using Panels.Saitek;
    using HID;


    /// <summary>
    /// Pre-programmed radio panel for the F-15E. 
    /// </summary>
    public class RadioPanelPZ69JF17 : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentJF17RadioMode
        {
            COM1,
            COM2,
            NO_USE,
        }

        private CurrentJF17RadioMode _currentUpperRadioMode = CurrentJF17RadioMode.COM1;
        private CurrentJF17RadioMode _currentLowerRadioMode = CurrentJF17RadioMode.COM1;
        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        /*COMM1*/
        /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
        private readonly object _lockCOMM1Object = new();
        private uint _comm1BigFrequencyStandby = 108;
        private uint _comm1SmallFrequencyStandby;
        private string _comm1CockpitFrequency = "108.000";
        private DCSBIOSOutput _comm1RadioControl;
        private const string COMM1_RADIO_COMMAND = "COMM1 ";

        /*COMM2*/
        /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
        private readonly object _lockCOMM2Object = new();
        private uint _comm2BigFrequencyStandby = 108;
        private uint _comm2SmallFrequencyStandby;
        private string _comm2CockpitFrequency = "108.000";
        private DCSBIOSOutput _comm2RadioControl;
        private const string COMM2_RADIO_COMMAND = "COMM2 ";

        private long _doUpdatePanelLCD;
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private const uint QUART_FREQ_CHANGE_VALUE = 25;

        public RadioPanelPZ69JF17(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {}

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void InitPanel()
        {
            CreateRadioKnobs();

            // COMM1
            _comm1RadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("COMM1");

            // COMM2
            _comm2RadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("COMM2");
            
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
        }
        
        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateCounter(e.Address, e.Data);

            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (_comm1RadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockCOMM1Object)
                    {
                        _comm1CockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_comm2RadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockCOMM2Object)
                    {
                        _comm2CockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelKnobsJF17 knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelKnobsJF17.UPPER_FREQ_SWITCH || knob == RadioPanelKnobsJF17.LOWER_FREQ_SWITCH))
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

            switch (knob)
            {
                case RadioPanelKnobsJF17.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentJF17RadioMode.COM1:
                                {
                                    SendCOMM1ToDCSBIOS();
                                    break;
                                }
                            case CurrentJF17RadioMode.COM2:
                                {
                                    SendCOMM2ToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelKnobsJF17.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {

                            case CurrentJF17RadioMode.COM1:
                                {
                                    SendCOMM1ToDCSBIOS();
                                    break;
                                }
                            case CurrentJF17RadioMode.COM2:
                                {
                                    SendCOMM2ToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendCOMM1ToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _comm1CockpitFrequency;
                DCSBIOS.Send($"{COMM1_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentJF17RadioMode.COM1)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _comm1BigFrequencyStandby = uint.Parse(array[0]);
                _comm1SmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendCOMM2ToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _comm2CockpitFrequency;
                DCSBIOS.Send($"{COMM2_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentJF17RadioMode.COM2)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _comm2BigFrequencyStandby = uint.Parse(array[0]);
                _comm2SmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private string GetStandbyFrequencyString(CurrentJF17RadioMode radio)
        {
            return radio switch
            {
                CurrentJF17RadioMode.COM1 => _comm1BigFrequencyStandby + "." + _comm1SmallFrequencyStandby.ToString().PadLeft(3, '0'),
                CurrentJF17RadioMode.COM2 => _comm2BigFrequencyStandby + "." + _comm2SmallFrequencyStandby.ToString().PadLeft(3, '0'),
                _ => throw new ArgumentOutOfRangeException(nameof(radio), radio, "JF-17.GetFrequencyString()")
            };
        }

        private void ShowFrequenciesOnPanel()
        {
            lock (_lockShowFrequenciesOnPanelObject)
            {
                if (!FirstReportHasBeenRead)
                {
                    return;
                }

                if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                {
                    return;
                }

                var bytes = new byte[21];
                bytes[0] = 0x0;

                switch (_currentUpperRadioMode)
                {
                    case CurrentJF17RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            break;
                        }
                    case CurrentJF17RadioMode.COM1:
                        {
                            lock (_lockCOMM1Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentJF17RadioMode.COM1), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _comm1CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentJF17RadioMode.COM2:
                        {
                            lock (_lockCOMM2Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentJF17RadioMode.COM2), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _comm2CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                }

                switch (_currentLowerRadioMode)
                {
                    case CurrentJF17RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            break;
                        }
                    case CurrentJF17RadioMode.COM1:
                        {
                            lock (_lockCOMM1Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentJF17RadioMode.COM1), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _comm1CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentJF17RadioMode.COM2:
                        {
                            lock (_lockCOMM2Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentJF17RadioMode.COM2), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _comm2CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                }

                SendLCDData(bytes);
            }
            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            if (SkipCurrentFrequencyChange())
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var radioPanelKnobC101 = (RadioPanelKnobJF17)o;
                if (radioPanelKnobC101.IsOn)
                {
                    switch (radioPanelKnobC101.RadioPanelPZ69Knob)
                    {
                        case RadioPanelKnobsJF17.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1BigFrequencyStandby >= 173)
                                            {
                                                // @ max value
                                                _comm1BigFrequencyStandby = 225;
                                                break;
                                            }
                                            _comm1BigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2BigFrequencyStandby >= 173)
                                            {
                                                _comm2BigFrequencyStandby = 225;
                                                // @ max value
                                                break;
                                            }
                                            _comm2BigFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1BigFrequencyStandby <= 225 && _comm1BigFrequencyStandby > 173)
                                            {
                                                _comm1BigFrequencyStandby = 173;
                                                // @ min value
                                                break;
                                            }
                                            if (_comm1BigFrequencyStandby <= 108)
                                            {
                                                _comm1BigFrequencyStandby = 399;
                                                // @ min value
                                                break;
                                            }
                                            _comm1BigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2BigFrequencyStandby <= 225 && _comm2BigFrequencyStandby > 173)
                                            {
                                                _comm2BigFrequencyStandby = 173;
                                                // @ min value
                                                break;
                                            }
                                            if (_comm2BigFrequencyStandby <= 108)
                                            {
                                                _comm2BigFrequencyStandby = 399;
                                                // @ min value
                                                break;
                                            }
                                            _comm2BigFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1SmallFrequencyStandby >= 975)
                                            {
                                                _comm1SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }

                                            _comm1SmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2SmallFrequencyStandby >= 975)
                                            {
                                                _comm2SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _comm2SmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1SmallFrequencyStandby == 0)
                                            {
                                                _comm1SmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _comm1SmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2SmallFrequencyStandby == 0)
                                            {
                                                _comm2SmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _comm2SmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1BigFrequencyStandby >= 173)
                                            {
                                                // @ max value
                                                _comm1BigFrequencyStandby = 225;
                                                break;
                                            }
                                            _comm1BigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2BigFrequencyStandby >= 173)
                                            {
                                                _comm2BigFrequencyStandby = 225;
                                                // @ max value
                                                break;
                                            }
                                            _comm2BigFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1BigFrequencyStandby <= 225 && _comm1BigFrequencyStandby > 173)
                                            {
                                                _comm1BigFrequencyStandby = 173;
                                                // @ min value
                                                break;
                                            }
                                            if (_comm1BigFrequencyStandby <= 108)
                                            {
                                                _comm1BigFrequencyStandby = 399;
                                                // @ min value
                                                break;
                                            }
                                            _comm1BigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2BigFrequencyStandby <= 225 && _comm2BigFrequencyStandby > 173)
                                            {
                                                _comm2BigFrequencyStandby = 173;
                                                // @ min value
                                                break;
                                            }
                                            if (_comm2BigFrequencyStandby <= 108)
                                            {
                                                _comm2BigFrequencyStandby = 399;
                                                // @ min value
                                                break;
                                            }
                                            _comm2BigFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1SmallFrequencyStandby >= 975)
                                            {
                                                _comm1SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }

                                            _comm1SmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2SmallFrequencyStandby >= 975)
                                            {
                                                _comm2SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _comm2SmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsJF17.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentJF17RadioMode.COM1:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm1SmallFrequencyStandby == 0)
                                            {
                                                _comm1SmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _comm1SmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
                                            if (_comm2SmallFrequencyStandby == 0)
                                            {
                                                _comm2SmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _comm2SmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
            }

            ShowFrequenciesOnPanel();
        }

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            Interlocked.Increment(ref _doUpdatePanelLCD);
            lock (LockLCDUpdateObject)
            {
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobJF17)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelKnobsJF17.UPPER_COM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentJF17RadioMode.COM1;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.UPPER_COM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentJF17RadioMode.COM2;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.UPPER_NAV1:
                        case RadioPanelKnobsJF17.UPPER_NAV2:
                        case RadioPanelKnobsJF17.UPPER_ADF:
                        case RadioPanelKnobsJF17.UPPER_DME:
                        case RadioPanelKnobsJF17.UPPER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentJF17RadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.LOWER_COM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentJF17RadioMode.COM1;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.LOWER_COM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentJF17RadioMode.COM2;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.LOWER_NAV1:
                        case RadioPanelKnobsJF17.LOWER_NAV2:
                        case RadioPanelKnobsJF17.LOWER_ADF:
                        case RadioPanelKnobsJF17.LOWER_DME:
                        case RadioPanelKnobsJF17.LOWER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentJF17RadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.UPPER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelKnobsJF17.UPPER_FREQ_SWITCH);
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.LOWER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelKnobsJF17.LOWER_FREQ_SWITCH);
                                }
                                break;
                            }
                    }

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(
                            DCSAircraft.SelectedAircraft.Description,
                            HIDInstance,
                            PluginGamingPanelEnum.PZ69RadioPanel_PreProg_AH64D,
                            (int)radioPanelKnob.RadioPanelPZ69Knob,
                            radioPanelKnob.IsOn,
                            null);
                    }
                }
                AdjustFrequency(hashSet);
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobJF17.GetRadioPanelKnobs();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}

