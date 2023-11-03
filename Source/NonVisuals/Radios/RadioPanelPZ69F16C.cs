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
    using NonVisuals.Radios.RadioSettings;
    
    /*
     * Pre-programmed radio panel for the F-16 C Block 50.
     */
    public class RadioPanelPZ69F16C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentF16CRadioMode
        {
            ARC164_UHF,
            ARC222_VHF,
            NO_USE
        }

        private CurrentF16CRadioMode _currentUpperRadioMode = CurrentF16CRadioMode.ARC164_UHF;
        private CurrentF16CRadioMode _currentLowerRadioMode = CurrentF16CRadioMode.ARC164_UHF;

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;


        /*UHF  AN/ARC-164  COM1*/
        /* 225.000 - 399.975 MHz */
        private readonly object _lockArc164Object = new();
        private FlightRadio _arc164Radio;
        private DCSBIOSOutput _arc164RadioDCSBIOSControl;


        /*VHF  AN/ARC-222  COM2*/
        /*108.000 - 151.975 MHz*/
        private readonly object _lockArc222Object = new();
        private FlightRadio _arc222Radio;
        private DCSBIOSOutput _arc222RadioDCSBIOSControl;

        public RadioPanelPZ69F16C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
            lock (_lockArc164Object)
            {
                _arc164Radio = new FlightRadio(new ARC164Settings("UHF_RADIO").RadioSettings);
                _arc164Radio.InitRadio();
            }
            lock (_lockArc222Object)
            {
                _arc222Radio = new FlightRadio(new ARC222Settings("VHF_RADIO").RadioSettings);
                _arc222Radio.InitRadio();
            }

            // UHF
            _arc164RadioDCSBIOSControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UHF_RADIO");
            _arc222RadioDCSBIOSControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("VHF_RADIO");


            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
        }
        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                UpdateCounter(e.Address, e.Data);

                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (_arc164RadioDCSBIOSControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockArc164Object)
                    {
                        _arc164Radio.SetCockpitFrequency(e.StringData);
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_arc222RadioDCSBIOSControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockArc222Object)
                    {
                        _arc222Radio.SetCockpitFrequency(e.StringData);
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                Interlocked.Increment(ref _doUpdatePanelLCD);
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }
       
        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobF16C)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsF16C.UPPER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.ARC164_UHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.ARC222_VHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.ARC164_UHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.ARC222_VHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_FREQ_SWITCH:
                                {
                                    if (!radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF16C.UPPER_FREQ_SWITCH);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_FREQ_SWITCH:
                                {
                                    if (!radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF16C.LOWER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F16C, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF16C knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsF16C.UPPER_FREQ_SWITCH ||  knob == RadioPanelPZ69KnobsF16C.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsF16C.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentF16CRadioMode.ARC164_UHF:
                                {
                                    DCSBIOS.Send(_arc164Radio.GetDCSBIOSCommand());
                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                    break;
                                }
                            case CurrentF16CRadioMode.ARC222_VHF:
                                {
                                    DCSBIOS.Send(_arc222Radio.GetDCSBIOSCommand());
                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelPZ69KnobsF16C.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentF16CRadioMode.ARC164_UHF:
                                {
                                    DCSBIOS.Send(_arc164Radio.GetDCSBIOSCommand());
                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                    break;
                                }
                            case CurrentF16CRadioMode.ARC222_VHF:
                                {
                                    DCSBIOS.Send(_arc222Radio.GetDCSBIOSCommand());
                                    Interlocked.Increment(ref _doUpdatePanelLCD);
                                    break;
                                }
                        }
                        break;
                    }
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
                    var radioPanelKnobF16C = (RadioPanelKnobF16C)o;
                    if (radioPanelKnobF16C.IsOn)
                    {
                        switch (radioPanelKnobF16C.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.IntegerFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.IntegerFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.IntegerFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.IntegerFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.DecimalFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.DecimalFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.DecimalFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.DecimalFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.IntegerFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.IntegerFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.IntegerFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.IntegerFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.DecimalFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.DecimalFrequencyUp();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.ARC164_UHF:
                                            {
                                                _arc164Radio.DecimalFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.ARC222_VHF:
                                            {
                                                _arc222Radio.DecimalFrequencyDown();
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NO_USE:
                                            {
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
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                    case CurrentF16CRadioMode.ARC164_UHF:
                        {
                            lock (_lockArc164Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, _arc164Radio.CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, _arc164Radio.StandbyFrequency, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            break;
                        }
                    case CurrentF16CRadioMode.ARC222_VHF:
                        {
                            lock (_lockArc222Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, _arc222Radio.CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, _arc222Radio.StandbyFrequency, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            break;
                        }
                    case CurrentF16CRadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentF16CRadioMode.ARC164_UHF:
                        {
                            lock (_lockArc164Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, _arc164Radio.CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, _arc164Radio.StandbyFrequency, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            break;
                        }
                    case CurrentF16CRadioMode.ARC222_VHF:
                        {
                            lock (_lockArc222Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, _arc222Radio.CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, _arc222Radio.StandbyFrequency, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            break;
                        }
                    case CurrentF16CRadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                }
                SendLCDData(bytes);
            }
            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }
        
        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobF16C.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentF16CRadioMode currentF16CRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentF16CRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentF16CRadioMode currentF16CRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentF16CRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }
    }
}
