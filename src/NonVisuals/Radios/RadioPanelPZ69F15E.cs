using System.Diagnostics;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.Radios.RadioControls;
using NonVisuals.Radios.RadioSettings;

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
    /// Pre-programmed radio panel for the F-15E. 
    /// </summary>
    public class RadioPanelPZ69F15E : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentF15ERadioMode
        {
            ARC210,
            UHF,
            NO_USE,
        }

        private CurrentF15ERadioMode _currentUpperRadioMode = CurrentF15ERadioMode.ARC210;
        private CurrentF15ERadioMode _currentLowerRadioMode = CurrentF15ERadioMode.ARC210;
        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        /*ARC210*/
        /* (FM) 30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        private readonly object _lockARC210Object = new();
        private FlightRadio _arc210Radio;
        private DCSBIOSOutput _arc210RadioDCSBIOSControl;

        /*UHF*/
        /* 225.000 to 339.975 */
        private readonly object _lockUHFObject = new();
        private uint _uhfBigFrequencyStandby = 225;
        private uint _uhfSmallFrequencyStandby;
        private string _uhfCockpitFrequency = "225.000";
        private DCSBIOSOutput _uhfRadioControl;
        private const string UHF_RADIO_COMMAND = "UHF_RADIO ";

        private long _doUpdatePanelLCD;
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private const uint QUART_FREQ_CHANGE_VALUE = 25;

        public RadioPanelPZ69F15E(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        { }

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
            lock (_lockARC210Object)
            {
                _arc210Radio = new FlightRadio(new ARC210Settings("ARC_210_RADIO").RadioSettings);
                _arc210Radio.InitRadio();
            }

            // VHF
            _arc210RadioDCSBIOSControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("ARC_210_RADIO");

            // UHF
            _uhfRadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UHF_RADIO");

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

                if (_arc210RadioDCSBIOSControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockARC210Object)
                    {
                        Debug.WriteLine($"Received {e.StringData}");
                        _arc210Radio.SetCockpitFrequency(e.StringData);
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

        private void SendFrequencyToDCSBIOS(RadioPanelKnobsF15E knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelKnobsF15E.UPPER_FREQ_SWITCH || knob == RadioPanelKnobsF15E.LOWER_FREQ_SWITCH))
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
                case RadioPanelKnobsF15E.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentF15ERadioMode.ARC210:
                                {
                                    DCSBIOS.SendAsync(_arc210Radio.GetDCSBIOSCommand());
                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                    break;
                                }
                            case CurrentF15ERadioMode.UHF:
                                {
                                    SendUHFToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelKnobsF15E.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {

                            case CurrentF15ERadioMode.ARC210:
                                {
                                    DCSBIOS.SendAsync(_arc210Radio.GetDCSBIOSCommand());
                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                    break;
                                }
                            case CurrentF15ERadioMode.UHF:
                                {
                                    SendUHFToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendUHFToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _uhfCockpitFrequency;
                DCSBIOS.SendAsync($"{UHF_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentF15ERadioMode.UHF)}\n");
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

        private string GetStandbyFrequencyString(CurrentF15ERadioMode radio)
        {
            return radio switch
            {
                CurrentF15ERadioMode.UHF => _uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(3, '0'),
                _ => throw new ArgumentOutOfRangeException(nameof(radio), radio, "F-15E.GetFrequencyString()")
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
                    case CurrentF15ERadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            break;
                        }
                    case CurrentF15ERadioMode.ARC210:
                        {
                            lock (_lockARC210Object)
                            {
                                if (_upperButtonPressed || _lowerButtonPressed)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _arc210Radio.TemporaryFrequencyBandId, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _arc210Radio.StandbyFrequency, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _arc210Radio.CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentF15ERadioMode.UHF:
                        {
                            lock (_lockUHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentF15ERadioMode.UHF), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _uhfCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                }

                switch (_currentLowerRadioMode)
                {
                    case CurrentF15ERadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            break;
                        }
                    case CurrentF15ERadioMode.ARC210:
                        {
                            lock (_lockARC210Object)
                            {
                                if (_upperButtonPressed || _lowerButtonPressed)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _arc210Radio.TemporaryFrequencyBandId, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _arc210Radio.StandbyFrequency, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _arc210Radio.CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentF15ERadioMode.UHF:
                        {
                            lock (_lockUHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentF15ERadioMode.UHF), PZ69LCDPosition.LOWER_STBY_RIGHT);
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
                var radioPanelKnobC101 = (RadioPanelKnobF15E)o;
                if (radioPanelKnobC101.IsOn)
                {
                    switch (radioPanelKnobC101.RadioPanelPZ69Knob)
                    {
                        case RadioPanelKnobsF15E.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                _arc210Radio.TemporaryFrequencyBandUp();
                                            }
                                            else
                                            {
                                                _arc210Radio.IntegerFrequencyUp();
                                            }

                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                _arc210Radio.TemporaryFrequencyBandDown();
                                            }
                                            else
                                            {
                                                _arc210Radio.IntegerFrequencyDown();
                                            }
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            _arc210Radio.DecimalFrequencyUp();
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            _arc210Radio.DecimalFrequencyDown();
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                _arc210Radio.TemporaryFrequencyBandUp();
                                            }
                                            else
                                            {
                                                _arc210Radio.IntegerFrequencyUp();
                                            }
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                _arc210Radio.TemporaryFrequencyBandDown();
                                            }
                                            else
                                            {
                                                _arc210Radio.IntegerFrequencyDown();
                                            }
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            _arc210Radio.DecimalFrequencyUp();
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

                        case RadioPanelKnobsF15E.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF15ERadioMode.ARC210:
                                        {
                                            _arc210Radio.DecimalFrequencyDown();
                                            break;
                                        }
                                    case CurrentF15ERadioMode.UHF:
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

            Interlocked.Increment(ref _doUpdatePanelLCD);
            ShowFrequenciesOnPanel();
        }

        protected override void PZ69KnobChangedAsync(IEnumerable<object> hashSet)
        {
            Interlocked.Increment(ref _doUpdatePanelLCD);
            lock (LockLCDUpdateObject)
            {
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobF15E)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelKnobsF15E.UPPER_ARC210:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF15ERadioMode.ARC210;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF15ERadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.UPPER_NAV1:
                        case RadioPanelKnobsF15E.UPPER_NAV2:
                        case RadioPanelKnobsF15E.UPPER_ADF:
                        case RadioPanelKnobsF15E.UPPER_DME:
                        case RadioPanelKnobsF15E.UPPER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF15ERadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.LOWER_ARC210:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF15ERadioMode.ARC210;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF15ERadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.LOWER_NAV1:
                        case RadioPanelKnobsF15E.LOWER_NAV2:
                        case RadioPanelKnobsF15E.LOWER_ADF:
                        case RadioPanelKnobsF15E.LOWER_DME:
                        case RadioPanelKnobsF15E.LOWER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF15ERadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.UPPER_FREQ_SWITCH:
                            {
                                _upperButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Sync when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelKnobsF15E.UPPER_FREQ_SWITCH);
                                    }
                                    else
                                    {
                                        /* We must say that the user now has stopped rotating */
                                        switch (_currentUpperRadioMode)
                                        {
                                            case CurrentF15ERadioMode.ARC210:
                                                {
                                                    _arc210Radio.SwitchFrequencyBand();
                                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                                    break;
                                                }
                                            case CurrentF15ERadioMode.UHF:
                                                {
                                                    break;
                                                }
                                        }
                                    }

                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                        case RadioPanelKnobsF15E.LOWER_FREQ_SWITCH:
                            {
                                _lowerButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Sync when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelKnobsF15E.LOWER_FREQ_SWITCH);
                                    }
                                    else
                                    {
                                        /* We must say that the user now has stopped rotating */
                                        switch (_currentLowerRadioMode)
                                        {
                                            case CurrentF15ERadioMode.ARC210:
                                            {
                                                _arc210Radio.SwitchFrequencyBand();
                                                Interlocked.Increment(ref _doUpdatePanelLCD);
                                                break;
                                            }
                                            case CurrentF15ERadioMode.UHF:
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    _lowerButtonPressedAndDialRotated = false;
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
            SaitekPanelKnobs = RadioPanelKnobF15E.GetRadioPanelKnobs();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}

