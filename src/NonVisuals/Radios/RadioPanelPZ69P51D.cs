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
    using DCS_BIOS.misc;




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
        private DCSBIOSOutput _vhfOffOutput;
        private DCSBIOSCommand _vhfOffCommand;
        private DCSBIOSOutput _vhfChannelAOutput;
        private DCSBIOSCommand _vhfChannelACommand;
        private DCSBIOSOutput _vhfChannelBOutput;
        private DCSBIOSCommand _vhfChannelBCommand;
        private DCSBIOSOutput _vhfChannelCOutput;
        private DCSBIOSCommand _vhfChannelCCommand;
        private DCSBIOSOutput _vhfChannelDOutput;
        private DCSBIOSCommand _vhfChannelDCommand;
        private volatile uint _vhfCockpitPresetActiveButton;
        private DCSBIOSCommand _vhfVolumeCommand;
        private const int VHF_VOLUME_CHANGE_VALUE = 2000;
        private readonly object _lockVHFRadioModeDialObject1 = new();
        private DCSBIOSOutput _vhfMode2Output;
        private DCSBIOSCommand _vhfMode2Command;
        private volatile uint _vhfMode2CockpitDialPosition = 1;
        private DCSBIOSOutput _vhfMode3Output;
        private DCSBIOSCommand _vhfMode3Command;
        private volatile uint _vhfMode3CockpitDialPosition = 1;
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
        private DCSBIOSCommand _lfFrequencyCommand;
        private DCSBIOSOutput _lfRadioVolumeDcsbiosOutput;
        private DCSBIOSCommand _lfVolumeCommand;
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
            (_vhfOffCommand,_vhfOffOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("VHF_RADIO_ON_OFF");
            (_vhfChannelACommand,_vhfChannelAOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("VHF_RADIO_CHAN_A");
            (_vhfChannelBCommand, _vhfChannelBOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("VHF_RADIO_CHAN_B");
            (_vhfChannelCCommand, _vhfChannelCOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("VHF_RADIO_CHAN_C");
            (_vhfChannelDCommand, _vhfChannelDOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("VHF_RADIO_CHAN_D");
            (_vhfMode2Command, _vhfMode2Output) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RADIO_MODE2");
            (_vhfMode3Command, _vhfMode3Output) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RADIO_MODE3");
            _vhfVolumeCommand = DCSBIOSControlLocator.GetCommand("RADIO_VOLUME");

            // LF DETROLA
            (_lfFrequencyCommand,_lfRadioFrequencyDcsbiosOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("DETROLA_FREQUENCY");
            (_lfVolumeCommand, _lfRadioVolumeDcsbiosOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("DETROLA_VOLUME");

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
                if (_vhfOffOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhfOffOutput.LastUIntValue == 1)
                        {
                            // Radio is off
                            _vhfCockpitPresetActiveButton = 0;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF A
                if (_vhfChannelAOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhfChannelAOutput.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhfCockpitPresetActiveButton = 1;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF B
                if (_vhfChannelBOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhfChannelBOutput.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhfCockpitPresetActiveButton = 2;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF C
                if (_vhfChannelCOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhfChannelCOutput.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhfCockpitPresetActiveButton = 3;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF D
                if (_vhfChannelDOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        if (_vhfChannelDOutput.LastUIntValue == 1)
                        {
                            // Radio is on A
                            _vhfCockpitPresetActiveButton = 4;
                        }
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF MODE 1
                if (_vhfMode2Output.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        _vhfMode2CockpitDialPosition = _vhfMode2Output.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF MODE 2
                if (_vhfMode3Output.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVhf1DialObject1)
                    {
                        _vhfMode3CockpitDialPosition = _vhfMode3Output.LastUIntValue;
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
                                                // Do not sync if user has pressed the button to configure the radio
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
                                                // Do not sync if user has pressed the button to configure the radio
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
                                                    DCSBIOS.Send(_vhfVolumeCommand.GetVariableCommand(VHF_VOLUME_CHANGE_VALUE));
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
                                                    DCSBIOS.Send(_vhfVolumeCommand.GetVariableCommand(VHF_VOLUME_CHANGE_VALUE, true));
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
                                                    DCSBIOS.Send(_vhfVolumeCommand.GetVariableCommand(VHF_VOLUME_CHANGE_VALUE));
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
                                                    DCSBIOS.Send(_vhfVolumeCommand.GetVariableCommand(VHF_VOLUME_CHANGE_VALUE, true));
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
                                    channelAsString = _vhfCockpitPresetActiveButton.ToString();
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfMode2CockpitDialPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                                    channelAsString = _vhfCockpitPresetActiveButton.ToString();
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfMode2CockpitDialPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                switch (_vhfCockpitPresetActiveButton)
                {
                    case 0:
                        {
                            DCSBIOS.Send(_vhfChannelACommand.GetIncCommand());
                            break;
                        }

                    case 1:
                        {
                            DCSBIOS.Send(_vhfChannelBCommand.GetIncCommand());
                            break;
                        }

                    case 2:
                        {
                            DCSBIOS.Send(_vhfChannelCCommand.GetIncCommand());
                            break;
                        }

                    case 3:
                        {
                            DCSBIOS.Send(_vhfChannelDCommand.GetIncCommand());
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
                switch (_vhfCockpitPresetActiveButton)
                {
                    case 0:
                    {
                        break;
                    }

                    case 1:
                    {
                        DCSBIOS.Send(_vhfOffCommand.GetIncCommand());
                        break;
                    }

                    case 2:
                    {
                        DCSBIOS.Send(_vhfChannelACommand.GetIncCommand());
                        break;
                    }

                    case 3:
                    {
                        DCSBIOS.Send(_vhfChannelBCommand.GetIncCommand());
                        break;
                    }

                    case 4:
                    {
                        DCSBIOS.Send(_vhfChannelCCommand.GetIncCommand());
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
                    return _lfFrequencyCommand.GetSetStateCommand(newValue);
                }

                newValue = _lfRadioFrequencyDCSBIOSValue < changeValue ? 0 : _lfRadioFrequencyDCSBIOSValue - changeValue;
                return _lfFrequencyCommand.GetSetStateCommand(newValue);
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
                    return _lfVolumeCommand.GetSetStateCommand(newValue);
                }

                newValue = _lfRadioVolumeDCSBIOSValue < changeValue ? 0 : _lfRadioVolumeDCSBIOSValue - changeValue;
                return _lfVolumeCommand.GetSetStateCommand(newValue);
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
                    return _vhfMode2CockpitDialPosition switch
                    {
                        0 => _vhfMode3Command.GetDecCommand(),
                        1 => _vhfMode2Command.GetIncCommand(),
                        2 => null,
                        _ => null
                    };
                }

                return _vhfMode2CockpitDialPosition switch
                {
                    0 => null,
                    1 => _vhfMode3Command.GetDecCommand(),
                    2 => _vhfMode3Command.GetDecCommand() + _vhfMode3Command.GetDecCommand(),
                    _ => null
                };
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

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}
