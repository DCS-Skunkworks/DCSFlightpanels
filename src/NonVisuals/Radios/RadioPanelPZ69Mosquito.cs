using ClassLibraryCommon;
using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;

    using MEF;
    using Plugin;
    using Knobs;
    using Panels.Saitek;
    using HID;
    using Helpers;
    using DCS_BIOS.Serialized;
    using DCS_BIOS.ControlLocator;




    /// <summary>
    /// Pre-programmed radio panel for the Mosquito. 
    /// </summary>
    public class RadioPanelPZ69Mosquito : RadioPanelPZ69Base
    {
        private enum CurrentMosquitoRadioMode
        {
            VHF,
            NO_USE
        }

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        private CurrentMosquitoRadioMode _currentUpperRadioMode = CurrentMosquitoRadioMode.VHF;
        private CurrentMosquitoRadioMode _currentLowerRadioMode = CurrentMosquitoRadioMode.VHF;

        /*Mosquito VHF Presets 1-4*/
        // Large dial 1-4 [step of 1]
        // Small dial volume control
        private readonly object _lockVhf1DialObject1 = new();
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton0;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton1;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton2;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton3;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton4;
        private volatile uint _vhf1CockpitPresetActiveButton;
        private const string VHF1_VOLUME_KNOB_COMMAND_INC = "RADIO_VOL +2000\n";
        private const string VHF1_VOLUME_KNOB_COMMAND_DEC = "RADIO_VOL -2000\n";
        private const string VHF1_RADIO_LIGHT_SWITCH_COMMAND = "RADIO_L_DIM TOGGLE\n";
        private readonly object _lockHFRadioModeDialObject1 = new();
        private volatile uint _vhfRadioModeCockpitPosition = 1;
        private DCSBIOSOutput _vhfRadioModeDcsbiosOutput;
        private readonly ClickSkipper _vhfDialChangeSkipper = new(2);

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69Mosquito(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
            _vhf1DcsbiosOutputPresetButton0 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_OFF");
            _vhf1DcsbiosOutputPresetButton1 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_A");
            _vhf1DcsbiosOutputPresetButton2 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_B");
            _vhf1DcsbiosOutputPresetButton3 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_C");
            _vhf1DcsbiosOutputPresetButton4 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_D");
            _vhfRadioModeDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_T_MODE");
            
            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                UpdateCounter(e.Address, e.Data);

                /*
                * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
                * Once a dial has been deemed to be "off" position and needs to be changed
                * a change command is sent to DCS-BIOS.
                * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
                * reset. Reading the dial's position with no change in value will not reset.
                */

                // VHF On Off
                if (_vhf1DcsbiosOutputPresetButton0.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhf1DcsbiosOutputPresetButton0.LastUIntValue == 1)
                        {
                            // Radio is off
                            _vhf1CockpitPresetActiveButton = 0;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF A
                if (_vhf1DcsbiosOutputPresetButton1.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhf1DcsbiosOutputPresetButton1.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhf1CockpitPresetActiveButton = 1;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF B
                if (_vhf1DcsbiosOutputPresetButton2.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhf1DcsbiosOutputPresetButton2.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhf1CockpitPresetActiveButton = 2;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF C
                if (_vhf1DcsbiosOutputPresetButton3.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhf1DcsbiosOutputPresetButton3.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhf1CockpitPresetActiveButton = 3;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF D
                if (_vhf1DcsbiosOutputPresetButton4.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhf1DcsbiosOutputPresetButton4.GetUIntValue(e.Data) == 1)
                        {
                            // Radio is on A
                            _vhf1CockpitPresetActiveButton = 4;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Mode
                if (_vhfRadioModeDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioModeDialObject1)
                    {
                        _vhfRadioModeCockpitPosition = _vhfRadioModeDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                        var radioPanelKnob = (RadioPanelKnobMosquito)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelKnobsMosquito.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMosquitoRadioMode.VHF);
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.UPPER_NO_USE0:
                            case RadioPanelKnobsMosquito.UPPER_NO_USE1:
                            case RadioPanelKnobsMosquito.UPPER_NO_USE2:
                            case RadioPanelKnobsMosquito.UPPER_NO_USE3:
                            case RadioPanelKnobsMosquito.UPPER_NO_USE4:
                            case RadioPanelKnobsMosquito.UPPER_NO_USE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMosquitoRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMosquitoRadioMode.VHF);
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_NO_USE0:
                            case RadioPanelKnobsMosquito.LOWER_NO_USE1:
                            case RadioPanelKnobsMosquito.LOWER_NO_USE2:
                            case RadioPanelKnobsMosquito.LOWER_NO_USE3:
                            case RadioPanelKnobsMosquito.LOWER_NO_USE4:
                            case RadioPanelKnobsMosquito.LOWER_NO_USE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMosquitoRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelKnobsMosquito.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelKnobsMosquito.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelKnobsMosquito.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelKnobsMosquito.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelKnobsMosquito.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelKnobsMosquito.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelKnobsMosquito.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelKnobsMosquito.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentMosquitoRadioMode.VHF)
                                    {
                                        _upperButtonPressed = radioPanelKnob.IsOn;
                                        if (!radioPanelKnob.IsOn)
                                        {
                                            if (!_upperButtonPressedAndDialRotated)
                                            {
                                                // Do not synch if user has pressed the button to configure the radio
                                                // Do when user releases button
                                                DCSBIOS.Send(VHF1_RADIO_LIGHT_SWITCH_COMMAND);
                                            }

                                            _upperButtonPressedAndDialRotated = false;
                                        }
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentMosquitoRadioMode.VHF)
                                    {
                                        _lowerButtonPressed = radioPanelKnob.IsOn;
                                        if (!radioPanelKnob.IsOn)
                                        {
                                            if (!_lowerButtonPressedAndDialRotated)
                                            {
                                                // Do not synch if user has pressed the button to configure the radio
                                                // Do when user releases button
                                                DCSBIOS.Send(VHF1_RADIO_LIGHT_SWITCH_COMMAND);
                                            }

                                            _lowerButtonPressedAndDialRotated = false;
                                        }
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_Mosquito, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                    var RadioPanelKnobMosquito = (RadioPanelKnobMosquito)o;
                    if (RadioPanelKnobMosquito.IsOn)
                    {
                        switch (RadioPanelKnobMosquito.RadioPanelPZ69Knob)
                        {
                            case RadioPanelKnobsMosquito.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    _vhfDialChangeSkipper.Click(GetHFRadioModeStringCommand(true));
                                    break;
                                }

                            case RadioPanelKnobsMosquito.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _vhfDialChangeSkipper.Click(GetHFRadioModeStringCommand(false));
                                    break;
                                }

                            case RadioPanelKnobsMosquito.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMosquitoRadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
                                                }
                                                else if (!_vhfDialChangeSkipper.ShouldSkip())
                                                {
                                                    SendIncVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMosquitoRadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
                                                }
                                                else if (!_vhfDialChangeSkipper.ShouldSkip())
                                                {
                                                    SendDecVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    // MODE
                                    _vhfDialChangeSkipper.Click(GetHFRadioModeStringCommand(true));
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    // MODE
                                    _vhfDialChangeSkipper.Click(GetHFRadioModeStringCommand(false));
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMosquitoRadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
                                                }
                                                else if (!_vhfDialChangeSkipper.ShouldSkip()) 
                                                {
                                                    SendIncVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelKnobsMosquito.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMosquitoRadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
                                                }
                                                else if (!_vhfDialChangeSkipper.ShouldSkip()) 
                                                {
                                                    SendDecVHFPresetCommand();
                                                }
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
                Logger.Error(ex);
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

                    if (!FirstReportHasBeenRead)
                    {

                        return;
                    }

                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentMosquitoRadioMode.VHF:
                            {
                                if (_upperButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    break;
                                }

                                // Pos     0    1    2    3    4
                                string channelAsString;
                                lock (_lockVhf1DialObject1)
                                {
                                    channelAsString = _vhf1CockpitPresetActiveButton.ToString();
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioModeCockpitPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMosquitoRadioMode.NO_USE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentMosquitoRadioMode.VHF:
                            {
                                if (_lowerButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    break;
                                }

                                // Pos     0    1    2    3    4
                                string channelAsString;
                                lock (_lockVhf1DialObject1)
                                {
                                    channelAsString = _vhf1CockpitPresetActiveButton.ToString();
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioModeCockpitPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMosquitoRadioMode.NO_USE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                    }
                    SendLCDData(bytes);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        private string GetHFRadioModeStringCommand(bool moveUp)
        {
            lock (_lockHFRadioModeDialObject1)
            {
                if (moveUp)
                {
                    return "RADIO_T_MODE " + (_vhfRadioModeCockpitPosition + 1) + "\n";
                }
                return "RADIO_T_MODE " + (_vhfRadioModeCockpitPosition - 1) + "\n";
            }
        }

        private void SendIncVHFPresetCommand()
        {
            Interlocked.Increment(ref _doUpdatePanelLCD);
            lock (_lockVhf1DialObject1)
            {
                switch (_vhf1CockpitPresetActiveButton)
                {
                    case 0:
                        {
                            DCSBIOS.Send("RADIO_A 1\n");
                            break;
                        }

                    case 1:
                        {
                            DCSBIOS.Send("RADIO_B 1\n");
                            break;
                        }

                    case 2:
                        {
                            DCSBIOS.Send("RADIO_C 1\n");
                            break;
                        }

                    case 3:
                        {
                            DCSBIOS.Send("RADIO_D 1\n");
                            break;
                        }

                    case 4:
                        {
                            break;
                        }
                }
            }
        }

        private void SendDecVHFPresetCommand()
        {
            Interlocked.Increment(ref _doUpdatePanelLCD);
            lock (_lockVhf1DialObject1)
            {
                switch (_vhf1CockpitPresetActiveButton)
                {
                    case 0:
                        {
                            break;
                        }

                    case 1:
                        {
                            DCSBIOS.Send("RADIO_OFF 1\n");
                            break;
                        }

                    case 2:
                        {
                            DCSBIOS.Send("RADIO_A 1\n");
                            break;
                        }

                    case 3:
                        {
                            DCSBIOS.Send("RADIO_B 1\n");
                            break;
                        }

                    case 4:
                        {
                            DCSBIOS.Send("RADIO_C 1\n");
                            break;
                        }
                }
            }
        }

        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobMosquito.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentMosquitoRadioMode currentMosquitoRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentMosquitoRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentMosquitoRadioMode currentMosquitoRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentMosquitoRadioMode;

                // If NO_USE then send next round of e.Data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}
