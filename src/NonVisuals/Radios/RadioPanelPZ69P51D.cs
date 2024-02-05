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
    using NonVisuals.Helpers;




    /// <summary>
    /// Pre-programmed radio panel for the P-51D. 
    /// </summary>
    public class RadioPanelPZ69P51D : RadioPanelPZ69Base
    {
        private enum CurrentP51DRadioMode
        {
            VHF,
            DETROLA,
            NO_USE
        }

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        private CurrentP51DRadioMode _currentUpperRadioMode = CurrentP51DRadioMode.VHF;
        private CurrentP51DRadioMode _currentLowerRadioMode = CurrentP51DRadioMode.VHF;

        /*P-51D VHF Presets 1-4*/
        // Large dial : Radio Mode
        // Small dial : Channel
        private readonly object _lockVhf1DialObject1 = new();
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton0;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton1;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton2;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton3;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton4;
        private volatile uint _vhf1CockpitPresetActiveButton;
        private const string VHF1_VOLUME_KNOB_COMMAND_INC = "RADIO_VOLUME +2000\n";
        private const string VHF1_VOLUME_KNOB_COMMAND_DEC = "RADIO_VOLUME -2000\n";
        private readonly object _lockVHFRadioModeDialObject1 = new();
        private volatile uint _vhfRadioModeCockpitDial1Position = 1;
        private DCSBIOSOutput _vhf1RadioModeDial1PresetDcsbiosOutput;
        private volatile uint _vhfRadioModeCockpitDial2Position = 1;
        private DCSBIOSOutput _vhf1RadioModeDial2PresetDcsbiosOutput;
        private readonly ClickSkipper _vhfRadioDialSkipper = new(2);
        private const string VHF_RADIO_LIGHT_SWITCH_COMMAND = "RADIO_LIGHTS_DIMMER TOGGLE\n";

        /*
         *  LF DETROLA RADIO
         *  COM1 Large : Volume Dial
         *  COM1 Small : Frequency Dial        
         */
        private readonly object _lockLFRadioFrequencyDialObject1 = new();
        private readonly object _lockLFRadioVolumeDialObject1 = new();
        private DCSBIOSOutput _lfRadioFrequencyDcsbiosOutput;
        private DCSBIOSOutput _lfRadioVolumeDcsbiosOutput;
        private volatile uint _lfRadioFrequencyDCSBIOSValue = 1;
        private volatile uint _lfRadioVolumeDCSBIOSValue;
        private volatile uint _lfRadioFrequencyCockpitValue = 1;
        private volatile uint _lfRadioVolumeCockpitValue;
        private readonly uint _lfFrequencyChangeValue = 50;
        private readonly uint _lfVolumeChangeValue = 200;
        private readonly ClickSpeedDetector _lfFrequencyDialChangeMonitor = new(15);
        private readonly ClickSpeedDetector _lfVolumeDialChangeMonitor = new(15);

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69P51D(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
            _vhf1DcsbiosOutputPresetButton0 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VHF_RADIO_ON_OFF");
            _vhf1DcsbiosOutputPresetButton1 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VHF_RADIO_CHAN_A");
            _vhf1DcsbiosOutputPresetButton2 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VHF_RADIO_CHAN_B");
            _vhf1DcsbiosOutputPresetButton3 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VHF_RADIO_CHAN_C");
            _vhf1DcsbiosOutputPresetButton4 = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VHF_RADIO_CHAN_D");
            _vhf1RadioModeDial1PresetDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_MODE2");
            _vhf1RadioModeDial2PresetDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RADIO_MODE3");
            // LF DETROLA
            _lfRadioFrequencyDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("DETROLA_FREQUENCY");
            _lfRadioVolumeDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("DETROLA_VOLUME");

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
                        if (_vhf1DcsbiosOutputPresetButton4.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhf1CockpitPresetActiveButton = 4;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF MODE 1
                if (_vhf1RadioModeDial1PresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        _vhfRadioModeCockpitDial1Position = _vhf1RadioModeDial1PresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF MODE 2
                if (_vhf1RadioModeDial2PresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        _vhfRadioModeCockpitDial2Position = _vhf1RadioModeDial2PresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // LF DETROLA Frequency
                if (_lfRadioFrequencyDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockLFRadioFrequencyDialObject1)
                    {
                        _lfRadioFrequencyDCSBIOSValue = _lfRadioFrequencyDcsbiosOutput.LastUIntValue;
                        //Range is 200 - 400kHz (DCS-BIOS value 0 - 65535)
                        _lfRadioFrequencyCockpitValue = Convert.ToUInt32(Convert.ToDouble(_lfRadioFrequencyDCSBIOSValue) / DCSBIOSConstants.MAX_VALUE * 200 + 200);
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // LF DETROLA Volume
                if (_lfRadioVolumeDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockLFRadioVolumeDialObject1)
                    {
                        _lfRadioVolumeDCSBIOSValue = _lfRadioVolumeDcsbiosOutput.LastUIntValue;
                        //0 - 100
                        _lfRadioVolumeCockpitValue = Convert.ToUInt32(Convert.ToDouble(_lfRadioVolumeDCSBIOSValue) / DCSBIOSConstants.MAX_VALUE * 100);
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
                        var radioPanelKnob = (RadioPanelKnobP51D)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsP51D.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP51DRadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_LF_RADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP51DRadioMode.DETROLA);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE4:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP51DRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP51DRadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_LF_RADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP51DRadioMode.DETROLA);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE4:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP51DRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentP51DRadioMode.VHF)
                                    {
                                        _upperButtonPressed = radioPanelKnob.IsOn;
                                        if (!radioPanelKnob.IsOn)
                                        {
                                            if (!_upperButtonPressedAndDialRotated)
                                            {
                                                // Do not synch if user has pressed the button to configure the radio
                                                // Do when user releases button
                                                DCSBIOS.Send(VHF_RADIO_LIGHT_SWITCH_COMMAND);
                                            }

                                            _upperButtonPressedAndDialRotated = false;
                                        }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentP51DRadioMode.VHF)
                                    {
                                        _lowerButtonPressed = radioPanelKnob.IsOn;
                                        if (!radioPanelKnob.IsOn)
                                        {
                                            if (!_lowerButtonPressedAndDialRotated)
                                            {
                                                // Do not synch if user has pressed the button to configure the radio
                                                // Do when user releases button
                                                DCSBIOS.Send(VHF_RADIO_LIGHT_SWITCH_COMMAND);
                                            }

                                            _lowerButtonPressedAndDialRotated = false;
                                        }
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_P51D, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
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
                    var radioPanelKnobP51D = (RadioPanelKnobP51D)o;
                    if (radioPanelKnobP51D.IsOn)
                    {
                        switch (radioPanelKnobP51D.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                _vhfRadioDialSkipper.Click(GetHFRadioModeStringCommand(true));
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                _vhfRadioDialSkipper.Click(GetHFRadioModeStringCommand(false));
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
                                                }
                                                else if (!_vhfRadioDialSkipper.ShouldSkip())
                                                {
                                                    SendIncVHFPresetCommand();
                                                }
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
                                                }
                                                else if (!_vhfRadioDialSkipper.ShouldSkip())
                                                {
                                                    SendDecVHFPresetCommand();
                                                }
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                _vhfRadioDialSkipper.Click(GetHFRadioModeStringCommand(true));
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                _vhfRadioDialSkipper.Click(GetHFRadioModeStringCommand(false));
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
                                                }
                                                else if (!_vhfRadioDialSkipper.ShouldSkip())
                                                {
                                                    SendIncVHFPresetCommand();
                                                }
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
                                                }
                                                else if (!_vhfRadioDialSkipper.ShouldSkip())
                                                {
                                                    SendDecVHFPresetCommand();
                                                }
                                                break;
                                            }
                                        case CurrentP51DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(false);
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
                        case CurrentP51DRadioMode.VHF:
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

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioModeCockpitDial1Position, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentP51DRadioMode.DETROLA:
                            {
                                lock (_lockLFRadioFrequencyDialObject1)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioFrequencyCockpitValue, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioVolumeCockpitValue, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                break;
                            }
                        case CurrentP51DRadioMode.NO_USE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentP51DRadioMode.VHF:
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

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioModeCockpitDial1Position, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentP51DRadioMode.DETROLA:
                            {
                                lock (_lockLFRadioFrequencyDialObject1)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioFrequencyCockpitValue, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioVolumeCockpitValue, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                break;
                            }

                        case CurrentP51DRadioMode.NO_USE:
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

        private void SendIncVHFPresetCommand()
        {
            Interlocked.Increment(ref _doUpdatePanelLCD);
            lock (_lockVhf1DialObject1)
            {
                switch (_vhf1CockpitPresetActiveButton)
                {
                    case 0:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_A 1\n");
                            break;
                        }

                    case 1:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_B 1\n");
                            break;
                        }

                    case 2:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_C 1\n");
                            break;
                        }

                    case 3:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_D 1\n");
                            break;
                        }

                    case 4:
                        {
                            break;
                        }
                }
            }
        }


        private void SendLFFrequencyCommand(bool increase)
        {
            DCSBIOS.Send(GetDetrolaFrequencyStringCommand(increase, _lfFrequencyDialChangeMonitor.ClickAndCheck() ? _lfFrequencyChangeValue * 10 : _lfFrequencyChangeValue));
        }

        private void SendLFVolumeCommand(bool increase)
        {
            DCSBIOS.Send(GetDetrolaVolumeStringCommand(increase, _lfVolumeDialChangeMonitor.ClickAndCheck() ? _lfVolumeChangeValue * 10 : _lfVolumeChangeValue));
        }


        private string GetDetrolaFrequencyStringCommand(bool moveUp, uint changeValue)
        {
            lock (_lockLFRadioFrequencyDialObject1)
            {
                uint newValue;
                if (moveUp)
                {
                    newValue = _lfRadioFrequencyDCSBIOSValue + changeValue > 0xFFFF ? 0xFFFF : _lfRadioFrequencyDCSBIOSValue + changeValue;
                    return $"DETROLA_FREQUENCY {newValue}\n";
                }

                newValue = _lfRadioFrequencyDCSBIOSValue < changeValue ? 0 : _lfRadioFrequencyDCSBIOSValue - changeValue;
                return $"DETROLA_FREQUENCY {newValue}\n";
            }
        }

        private string GetDetrolaVolumeStringCommand(bool moveUp, uint changeValue)
        {
            lock (_lockLFRadioVolumeDialObject1)
            {
                uint newValue;
                if (moveUp)
                {
                    newValue = _lfRadioVolumeDCSBIOSValue + changeValue > 0xFFFF ? 0xFFFF : _lfRadioVolumeDCSBIOSValue + changeValue;
                    return $"DETROLA_VOLUME {newValue}\n";
                }

                newValue = _lfRadioVolumeDCSBIOSValue < changeValue ? 0 : _lfRadioVolumeDCSBIOSValue - changeValue;
                return $"DETROLA_VOLUME {newValue}\n";
            }
        }

        private string GetHFRadioModeStringCommand(bool moveUp)
        {
            lock (_lockVHFRadioModeDialObject1)
            {
                /*
                 * Either DCS model or DCS-BIOS is broken. 16.07.2023
                 * 2 => 0 : RADIO_MODE2 DEC
                 * 0 => 2 : RADIO_MODE2 INC
                 * 1 => 2 : RADIO_MODE2 INC
                 * 0 => 1 : RADIO_MODE3 DEC
                 * 1 => 0 : RADIO_MODE3 DEC
                 */
                if (moveUp)
                {
                    return _vhfRadioModeCockpitDial1Position switch
                    {
                        0 => "RADIO_MODE3 DEC\n",
                        1 => "RADIO_MODE2 INC\n",
                        2 => null,
                        _ => null
                    };
                }

                return _vhfRadioModeCockpitDial1Position switch
                {
                    0 => null,
                    1 => "RADIO_MODE3 DEC\n",
                    2 => "RADIO_MODE3 DEC\n RADIO_MODE3 DEC\n",
                    _ => null
                };
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
                            DCSBIOS.Send("VHF_RADIO_ON_OFF 1\n");
                            break;
                        }

                    case 2:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_A 1\n");
                            break;
                        }

                    case 3:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_B 1\n");
                            break;
                        }

                    case 4:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_C 1\n");
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
            SaitekPanelKnobs = RadioPanelKnobP51D.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentP51DRadioMode currentP51DRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentP51DRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentP51DRadioMode currentP51DRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentP51DRadioMode;

                // If NO_USE then send next round of e.Data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
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
