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
    using DCS_BIOS.Serialized;
    using DCS_BIOS.ControlLocator;


    /// <summary>
    /// Pre-programmed radio panel for the F/A-18C. 
    /// </summary>
    public class RadioPanelPZ69C101 : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentC101RadioMode
        {
            VHF,
            UHF,
            NO_USE,
        }

        private CurrentC101RadioMode _currentUpperRadioMode = CurrentC101RadioMode.VHF;
        private CurrentC101RadioMode _currentLowerRadioMode = CurrentC101RadioMode.VHF;
        /*private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;*/

        /*VHF*/
        /* 116.000 to 149.975 */
        private readonly object _lockVHFObject = new();
        private uint _vhfBigFrequencyStandby = 108;
        private uint _vhfSmallFrequencyStandby;
        private string _vhfCockpitFrequency = "108.000";
        private DCSBIOSOutput _vhfRadioControl;
        private const string VHF_RADIO_COMMAND = "COMM_RADIO ";

        /*UHF*/
        /* 225.000 to 339.975 */
        private readonly object _lockUHFObject = new();
        private uint _uhfBigFrequencyStandby = 225;
        private uint _uhfSmallFrequencyStandby;
        private string _uhfCockpitFrequency = "225.000";
        private DCSBIOSOutput _uhfRadioControl;
        private const string UHF_RADIO_COMMAND = "VUHF_RADIO ";

        private long _doUpdatePanelLCD;
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private const uint QUART_FREQ_CHANGE_VALUE = 25;

        public RadioPanelPZ69C101(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {}

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            TurnOffAllDisplays();
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

            // VHF
            _vhfRadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("COMM_RADIO");

            // UHF
            _uhfRadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("VUHF_RADIO");
            
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);
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

                if (_vhfRadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockVHFObject)
                    {
                        _vhfCockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_uhfRadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockUHFObject)
                    {
                        _uhfCockpitFrequency = e.StringData;
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

        private void SendFrequencyToDCSBIOS(RadioPanelKnobsC101 knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelKnobsC101.UPPER_FREQ_SWITCH || knob == RadioPanelKnobsC101.LOWER_FREQ_SWITCH))
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
                case RadioPanelKnobsC101.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentC101RadioMode.VHF:
                                {
                                    SendVHFToDCSBIOS();
                                    break;
                                }
                            case CurrentC101RadioMode.UHF:
                                {
                                    SendUHFToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelKnobsC101.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {

                            case CurrentC101RadioMode.VHF:
                                {
                                    SendVHFToDCSBIOS();
                                    break;
                                }
                            case CurrentC101RadioMode.UHF:
                                {
                                    SendUHFToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendVHFToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _vhfCockpitFrequency;
                DCSBIOS.SendAsync($"{VHF_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentC101RadioMode.VHF)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _vhfBigFrequencyStandby = uint.Parse(array[0]);
                _vhfSmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendUHFToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _uhfCockpitFrequency;
                DCSBIOS.SendAsync($"{UHF_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentC101RadioMode.UHF)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _uhfBigFrequencyStandby = uint.Parse(array[0]);
                _uhfSmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private string GetStandbyFrequencyString(CurrentC101RadioMode radio)
        {
            return radio switch
            {
                CurrentC101RadioMode.VHF => _vhfBigFrequencyStandby + "." + _vhfSmallFrequencyStandby.ToString().PadLeft(3, '0'),
                CurrentC101RadioMode.UHF => _uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(3, '0'),
                _ => throw new ArgumentOutOfRangeException(nameof(radio), radio, "C-101.GetFrequencyString()")
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
                    case CurrentC101RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            break;
                        }
                    case CurrentC101RadioMode.VHF:
                        {
                            lock (_lockVHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentC101RadioMode.VHF), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _vhfCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentC101RadioMode.UHF:
                        {
                            lock (_lockUHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentC101RadioMode.UHF), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _uhfCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                }

                switch (_currentLowerRadioMode)
                {
                    case CurrentC101RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            break;
                        }
                    case CurrentC101RadioMode.VHF:
                        {
                            lock (_lockVHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentC101RadioMode.VHF), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _vhfCockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentC101RadioMode.UHF:
                        {
                            lock (_lockUHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentC101RadioMode.UHF), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _uhfCockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                var radioPanelKnobC101 = (RadioPanelKnobC101)o;
                if (radioPanelKnobC101.IsOn)
                {
                    switch (radioPanelKnobC101.RadioPanelPZ69Knob)
                    {
                        case RadioPanelKnobsC101.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby >= 151)
                                            {
                                                // @ max value
                                                _vhfBigFrequencyStandby = 151;
                                                break;
                                            }
                                            _vhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby >= 399)
                                            {
                                                _uhfBigFrequencyStandby = 399;
                                                // @ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby <= 108)
                                            {
                                                _vhfBigFrequencyStandby = 108;
                                                // @ min value
                                                break;
                                            }
                                            _vhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby <= 225)
                                            {
                                                _uhfBigFrequencyStandby = 225;
                                                // @ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby >= 975)
                                            {
                                                _vhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }

                                            _vhfSmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby >= 975)
                                            {
                                                _uhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby == 0)
                                            {
                                                _vhfSmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _vhfSmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby == 0)
                                            {
                                                _uhfSmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby > 151)
                                            {
                                                // @ max value
                                                _vhfBigFrequencyStandby = 151;
                                                break;
                                            }
                                            _vhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby > 399)
                                            {
                                                _uhfBigFrequencyStandby = 399;
                                                // @ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby <= 108)
                                            {
                                                _vhfBigFrequencyStandby = 108;
                                                // @ min value
                                                break;
                                            }
                                            _vhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby <= 225)
                                            {
                                                _uhfBigFrequencyStandby = 225;
                                                // @ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby >= 975)
                                            {
                                                _vhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }

                                            _vhfSmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby >= 975)
                                            {
                                                _uhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby += QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelKnobsC101.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentC101RadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby == 0)
                                            {
                                                _vhfSmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _vhfSmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
                                            break;
                                        }
                                    case CurrentC101RadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby == 0)
                                            {
                                                _uhfSmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby -= QUART_FREQ_CHANGE_VALUE;
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
                    var radioPanelKnob = (RadioPanelKnobC101)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelKnobsC101.UPPER_VHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentC101RadioMode.VHF;
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentC101RadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.UPPER_NAV1:
                        case RadioPanelKnobsC101.UPPER_NAV2:
                        case RadioPanelKnobsC101.UPPER_ADF:
                        case RadioPanelKnobsC101.UPPER_DME:
                        case RadioPanelKnobsC101.UPPER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentC101RadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.LOWER_VHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentC101RadioMode.VHF;
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentC101RadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.LOWER_NAV1:
                        case RadioPanelKnobsC101.LOWER_NAV2:
                        case RadioPanelKnobsC101.LOWER_ADF:
                        case RadioPanelKnobsC101.LOWER_DME:
                        case RadioPanelKnobsC101.LOWER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentC101RadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.UPPER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelKnobsC101.UPPER_FREQ_SWITCH);
                                }
                                break;
                            }
                        case RadioPanelKnobsC101.LOWER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelKnobsC101.LOWER_FREQ_SWITCH);
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
            SaitekPanelKnobs = RadioPanelKnobC101.GetRadioPanelKnobs();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}

