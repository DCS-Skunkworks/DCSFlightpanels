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
    using NonVisuals.Radios.RadioControls;


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

        /*COM1*/
        /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
        private readonly object _lockCOM1Object = new();
        private ARC210 _com1Radio;
        private DCSBIOSOutput _com1RadioControl;

        /*COM2*/
        /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
        private readonly object _lockCOM2Object = new();
        private ARC210 _com2Radio;
        private DCSBIOSOutput _com2RadioControl;

        private long _doUpdatePanelLCD;
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private const uint QUART_FREQ_CHANGE_VALUE = 25;

        public RadioPanelPZ69JF17(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
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
            lock (_lockCOM1Object)
            {
                _com1Radio = new ARC210("COMM1",
                    ARC210FrequencyBand.VHF2,
                    new[] { ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF });
                _com1Radio.InitRadio();
            }
            lock (_lockCOM2Object)
            {
                _com2Radio = new ARC210("COMM2",
                    ARC210FrequencyBand.VHF2,
                    new[] { ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF });
                _com2Radio.InitRadio();
            }

            // COM1
            _com1RadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("COMM1");

            // COM2
            _com2RadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("COMM2");

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

                if (_com1RadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockCOM1Object)
                    {
                        _com1Radio.SetCockpitFrequency(e.StringData);
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_com2RadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockCOM2Object)
                    {
                        _com2Radio.SetCockpitFrequency(e.StringData);
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
                                    SendCOM1ToDCSBIOS();
                                    break;
                                }
                            case CurrentJF17RadioMode.COM2:
                                {
                                    SendCOM2ToDCSBIOS();
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
                                    SendCOM1ToDCSBIOS();
                                    break;
                                }
                            case CurrentJF17RadioMode.COM2:
                                {
                                    SendCOM2ToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendCOM1ToDCSBIOS()
        {
            try
            {
                DCSBIOS.Send(_com1Radio.GetDCSBIOSCommand());
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendCOM2ToDCSBIOS()
        {
            try
            {
                DCSBIOS.Send(_com2Radio.GetDCSBIOSCommand());
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
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
                            lock (_lockCOM1Object)
                            {
                                if (_upperButtonPressed || _lowerButtonPressed)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com1Radio.GetTemporaryFrequencyBandId(), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com1Radio.GetStandbyFrequency(), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _com1Radio.GetCockpitFrequency(), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentJF17RadioMode.COM2:
                        {
                            lock (_lockCOM2Object)
                            {
                                if (_upperButtonPressed || _lowerButtonPressed)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com2Radio.GetTemporaryFrequencyBandId(), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com2Radio.GetStandbyFrequency(), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _com2Radio.GetCockpitFrequency(), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                            lock (_lockCOM1Object)
                            {
                                if (_upperButtonPressed || _lowerButtonPressed)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com1Radio.GetTemporaryFrequencyBandId(), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com1Radio.GetStandbyFrequency(), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _com1Radio.GetCockpitFrequency(), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentJF17RadioMode.COM2:
                        {
                            lock (_lockCOM2Object)
                            {
                                if (_upperButtonPressed || _lowerButtonPressed)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com2Radio.GetTemporaryFrequencyBandId(), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, _com2Radio.GetStandbyFrequency(), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _com2Radio.GetCockpitFrequency(), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                _com1Radio.TemporaryFrequencyBandUp();
                                            }
                                            else
                                            {
                                                _com1Radio.BigFrequencyUp();
                                            }
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                _com2Radio.TemporaryFrequencyBandUp();
                                            }
                                            else
                                            {
                                                _com2Radio.BigFrequencyUp();
                                            }
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
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                _com1Radio.TemporaryFrequencyBandDown();
                                            }
                                            else
                                            {
                                                _com1Radio.BigFrequencyDown();
                                            }
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                _com2Radio.TemporaryFrequencyBandDown();
                                            }
                                            else
                                            {
                                                _com2Radio.BigFrequencyDown();
                                            }
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
                                            _com1Radio.SmallFrequencyUp();
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            _com2Radio.SmallFrequencyUp();
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
                                            _com1Radio.SmallFrequencyDown();
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            _com2Radio.SmallFrequencyDown();
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
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                _com1Radio.TemporaryFrequencyBandUp();
                                            }
                                            else
                                            {
                                                _com1Radio.BigFrequencyUp();
                                            }
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                _com2Radio.TemporaryFrequencyBandUp();
                                            }
                                            else
                                            {
                                                _com2Radio.BigFrequencyUp();
                                            }
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
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                _com1Radio.TemporaryFrequencyBandDown();
                                            }
                                            else
                                            {
                                                _com1Radio.BigFrequencyDown();
                                            }
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                _com2Radio.TemporaryFrequencyBandDown();
                                            }
                                            else
                                            {
                                                _com2Radio.BigFrequencyDown();
                                            }
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
                                            _com1Radio.SmallFrequencyUp();
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            _com2Radio.SmallFrequencyUp();
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
                                            _com1Radio.SmallFrequencyDown();
                                            break;
                                        }
                                    case CurrentJF17RadioMode.COM2:
                                        {
                                            _com2Radio.SmallFrequencyDown();
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
                                _upperButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Sync when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelKnobsJF17.UPPER_FREQ_SWITCH);
                                    }
                                    else
                                    {
                                        /* We must say that the user now has stopped rotating */
                                        switch (_currentUpperRadioMode)
                                        {
                                            case CurrentJF17RadioMode.COM1:
                                                {

                                                    _com1Radio.SwitchFrequencyBand();
                                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                                    break;
                                                }
                                            case CurrentJF17RadioMode.COM2:
                                                {

                                                    _com2Radio.SwitchFrequencyBand();
                                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                                    break;
                                                }
                                        }
                                    }

                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                        case RadioPanelKnobsJF17.LOWER_FREQ_SWITCH:
                            {
                                _lowerButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Sync when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelKnobsJF17.LOWER_FREQ_SWITCH);
                                    }
                                    else
                                    {
                                        /* We must say that the user now has stopped rotating */
                                        switch (_currentLowerRadioMode)
                                        {
                                            case CurrentJF17RadioMode.COM1:
                                                {

                                                    _com1Radio.SwitchFrequencyBand();
                                                    break;
                                                }
                                            case CurrentJF17RadioMode.COM2:
                                                {

                                                    _com2Radio.SwitchFrequencyBand();
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

