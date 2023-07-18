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
    using NonVisuals.Helpers;




    /// <summary>
    /// Pre-programmed radio panel for the P-47D. 
    /// </summary>
    public class RadioPanelPZ69P47D : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentP47DRadioMode
        {
            VHF,
            DETROLA,
            NO_USE
        }

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        private CurrentP47DRadioMode _currentUpperRadioMode = CurrentP47DRadioMode.VHF;
        private CurrentP47DRadioMode _currentLowerRadioMode = CurrentP47DRadioMode.VHF;

        /*
        *  VHF RADIO
        *  COM1 Large Freq Mode
        *  COM1 Small Fine Channel/OFF 0 => 4
        *  Freq. Selector Light Switch        
        */
        private readonly object _lockVHFRadioPresetDialObject1 = new();
        private DCSBIOSOutput _vhfRadioOffDcsbiosOutput;
        private DCSBIOSOutput _vhfRadioChannelAPresetDcsbiosOutput;
        private DCSBIOSOutput _vhfRadioChannelBPresetDcsbiosOutput;
        private DCSBIOSOutput _vhfRadioChannelCPresetDcsbiosOutput;
        private DCSBIOSOutput _vhfRadioChannelDPresetDcsbiosOutput;
        private volatile uint _vhfRadioOffCockpitButton = 1;
        private volatile uint _vhfRadioChannelACockpitButton;
        private volatile uint _vhfRadioChannelBCockpitButton;
        private volatile uint _vhfRadioChannelCCockpitButton;
        private volatile uint _vhfRadioChannelDCockpitButton;
        private readonly ClickSkipper _vhfRadioChannelPresetDialSkipper = new(2);
        private const string VHF_RADIO_LIGHT_SWITCH_COMMAND = "RCTRL_DIM TOGGLE\n";
        private const string VHF_VOLUME_KNOB_COMMAND_INC = "RCTRL_VOL +2000\n";
        private const string VHF_VOLUME_KNOB_COMMAND_DEC = "RCTRL_VOL -2000\n";
        private readonly object _lockVHFRadioModeDialObject1 = new();
        private volatile uint _vhfRadioModeCockpitDialPosition = 1;
        private DCSBIOSOutput _vhfRadioModeDialPresetDcsbiosOutput;
        private readonly ClickSkipper _vhfRadioModePresetDialSkipper = new(2);

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

        public RadioPanelPZ69P47D(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachDataListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
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

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
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

                // VHF Radio Off Button
                if (_vhfRadioOffDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioPresetDialObject1)
                    {
                        _vhfRadioOffCockpitButton = _vhfRadioOffDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF Radio Channel A Button
                if (_vhfRadioChannelAPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioPresetDialObject1)
                    {
                        _vhfRadioChannelACockpitButton = _vhfRadioChannelAPresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF Radio Channel B Button
                if (_vhfRadioChannelBPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioPresetDialObject1)
                    {
                        _vhfRadioChannelBCockpitButton = _vhfRadioChannelBPresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF Radio Channel C Button
                if (_vhfRadioChannelCPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioPresetDialObject1)
                    {
                        _vhfRadioChannelCCockpitButton = _vhfRadioChannelCPresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF Radio Channel B Button
                if (_vhfRadioChannelDPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioPresetDialObject1)
                    {
                        _vhfRadioChannelDCockpitButton = _vhfRadioChannelDPresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VHF Radio Mode
                if (_vhfRadioModeDialPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioModeDialObject1)
                    {
                        _vhfRadioModeCockpitDialPosition = _vhfRadioModeDialPresetDcsbiosOutput.LastUIntValue;
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
                        _lfRadioFrequencyCockpitValue = Convert.ToUInt32(Convert.ToDouble(_lfRadioFrequencyDCSBIOSValue) / 65535 * 200 + 200);
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
                        _lfRadioVolumeCockpitValue = Convert.ToUInt32(Convert.ToDouble(_lfRadioVolumeDCSBIOSValue) / 65535 * 100);
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
                        var radioPanelKnob = (RadioPanelKnobP47D)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsP47D.UPPER_HFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP47DRadioMode.VHF);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsP47D.UPPER_LFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP47DRadioMode.DETROLA);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP47DRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_HFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP47DRadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP47D.LOWER_LFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP47DRadioMode.DETROLA);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP47DRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP47D.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode != CurrentP47DRadioMode.VHF)
                                    {
                                        break;
                                    }
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
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode != CurrentP47DRadioMode.VHF)
                                    {
                                        break;
                                    }
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
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                DCSAircraft.SelectedAircraft.Description,
                                HIDInstance,
                                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_P47D,
                                (int)radioPanelKnob.RadioPanelPZ69Knob,
                                radioPanelKnob.IsOn,
                                null);
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
                    var radioPanelKnobP47D = (RadioPanelKnobP47D)o;
                    if (radioPanelKnobP47D.IsOn)
                    {
                        switch (radioPanelKnobP47D.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsP47D.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                // MODE
                                                if (!_vhfRadioModePresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioModeStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(true);
                                                break;
                                            }
                                        case CurrentP47DRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                // MODE
                                                if (!_vhfRadioModePresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioModeStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF_VOLUME_KNOB_COMMAND_INC);
                                                }
                                                else if (!_vhfRadioChannelPresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF_VOLUME_KNOB_COMMAND_DEC);
                                                }
                                                else if (!_vhfRadioChannelPresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                // MODE
                                                if (!_vhfRadioModePresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioModeStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                // MODE
                                                if (!_vhfRadioModePresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioModeStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFVolumeCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF_VOLUME_KNOB_COMMAND_INC);
                                                }
                                                else if (!_vhfRadioChannelPresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
                                            {
                                                SendLFFrequencyCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    DCSBIOS.Send(VHF_VOLUME_KNOB_COMMAND_DEC);
                                                }
                                                else if (!_vhfRadioChannelPresetDialSkipper.ShouldSkip())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }
                                        case CurrentP47DRadioMode.DETROLA:
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
                        case CurrentP47DRadioMode.VHF:
                            {
                                if (_upperButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    break;
                                }

                                // 0-4
                                uint channel = 0;
                                lock (_lockVHFRadioPresetDialObject1)
                                {
                                    if (_vhfRadioOffCockpitButton == 1)
                                    {
                                        channel = 0;
                                    }
                                    else if (_vhfRadioChannelACockpitButton == 1)
                                    {
                                        channel = 1;
                                    }
                                    else if (_vhfRadioChannelBCockpitButton == 1)
                                    {
                                        channel = 2;
                                    }
                                    else if (_vhfRadioChannelCCockpitButton == 1)
                                    {
                                        channel = 3;
                                    }
                                    else if (_vhfRadioChannelDCockpitButton == 1)
                                    {
                                        channel = 4;
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioModeCockpitDialPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentP47DRadioMode.DETROLA:
                            {
                                lock (_lockLFRadioFrequencyDialObject1)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioFrequencyCockpitValue, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioVolumeCockpitValue, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                break;
                            }
                        case CurrentP47DRadioMode.NO_USE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentP47DRadioMode.VHF:
                            {
                                if (_lowerButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    break;
                                }

                                // 0-4
                                uint channel = 0;
                                lock (_lockVHFRadioPresetDialObject1)
                                {
                                    if (_vhfRadioOffCockpitButton == 1)
                                    {
                                        channel = 0;
                                    }
                                    else if (_vhfRadioChannelACockpitButton == 1)
                                    {
                                        channel = 1;
                                    }
                                    else if (_vhfRadioChannelBCockpitButton == 1)
                                    {
                                        channel = 2;
                                    }
                                    else if (_vhfRadioChannelCCockpitButton == 1)
                                    {
                                        channel = 3;
                                    }
                                    else if (_vhfRadioChannelDCockpitButton == 1)
                                    {
                                        channel = 4;
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioModeCockpitDialPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentP47DRadioMode.DETROLA:
                            {
                                lock (_lockLFRadioFrequencyDialObject1)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioFrequencyCockpitValue, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioVolumeCockpitValue, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                break;
                            }
                        case CurrentP47DRadioMode.NO_USE:
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
        
        public sealed override void Startup()
        {
            try
            {
                // VHF
                _vhfRadioOffDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_OFF");
                _vhfRadioChannelAPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_A");
                _vhfRadioChannelBPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_B");
                _vhfRadioChannelCPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_C");
                _vhfRadioChannelDPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_D");
                _vhfRadioModeDialPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_T_MODE");
                // LF DETROLA
                _lfRadioFrequencyDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("DETROLA_FREQU_SEL");
                _lfRadioVolumeDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("DETROLA_VOL");

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
            SaitekPanelKnobs = RadioPanelKnobP47D.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentP47DRadioMode currentP47DRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentP47DRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentP47DRadioMode currentP47DRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentP47DRadioMode;
                // If NO_USE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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

        private string GetHFRadioChannelStringCommand(bool moveUp)
        {
            lock (_lockVHFRadioPresetDialObject1)
            {
                if (moveUp)
                {
                    if ((_vhfRadioOffCockpitButton == 1 || _vhfRadioOffCockpitButton == 0) && _vhfRadioChannelACockpitButton == 0 && _vhfRadioChannelBCockpitButton == 0
                        && _vhfRadioChannelCCockpitButton == 0 && _vhfRadioChannelDCockpitButton == 0)
                    {
                        return "RCTRL_A INC\n";
                    }

                    if (_vhfRadioChannelACockpitButton == 1)
                    {
                        return "RCTRL_B INC\n";
                    }

                    if (_vhfRadioChannelBCockpitButton == 1)
                    {
                        return "RCTRL_C INC\n";
                    }

                    if (_vhfRadioChannelCCockpitButton == 1)
                    {
                        return "RCTRL_D INC\n";
                    }
                }
                else
                {
                    if (_vhfRadioChannelDCockpitButton == 1)
                    {
                        return "RCTRL_C INC\n";
                    }

                    if (_vhfRadioChannelCCockpitButton == 1)
                    {
                        return "RCTRL_B INC\n";
                    }

                    if (_vhfRadioChannelBCockpitButton == 1)
                    {
                        return "RCTRL_A INC\n";
                    }

                    if (_vhfRadioChannelACockpitButton == 1)
                    {
                        return "RCTRL_OFF INC\n";
                    }
                }
            }
            return null;
        }

        private string GetHFRadioModeStringCommand(bool moveUp)
        {
            lock (_lockVHFRadioModeDialObject1)
            {
                if (moveUp)
                {
                    return "RCTRL_T_MODE " + (_vhfRadioModeCockpitDialPosition + 1) + "\n";
                }
                return "RCTRL_T_MODE " + (_vhfRadioModeCockpitDialPosition - 1) + "\n";
            }
        }

        private string GetDetrolaFrequencyStringCommand(bool moveUp, uint changeValue)
        {
            lock (_lockLFRadioFrequencyDialObject1)
            {
                uint newValue;
                if (moveUp)
                {
                    newValue = _lfRadioFrequencyDCSBIOSValue + changeValue > 0xFFFF ? 0xFFFF : _lfRadioFrequencyDCSBIOSValue + changeValue;
                    return $"DETROLA_FREQU_SEL {newValue}\n";
                }

                newValue = _lfRadioFrequencyDCSBIOSValue < changeValue ? 0 : _lfRadioFrequencyDCSBIOSValue - changeValue;
                return $"DETROLA_FREQU_SEL {newValue}\n";
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
                    return $"DETROLA_VOL {newValue}\n";
                }

                newValue = _lfRadioVolumeDCSBIOSValue < changeValue ? 0 : _lfRadioVolumeDCSBIOSValue - changeValue;
                return $"DETROLA_VOL {newValue}\n";
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
