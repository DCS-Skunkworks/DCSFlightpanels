using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

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
    /// Pre-programmed radio panel for the Spitfire LF MK IX. 
    /// </summary>
    public class RadioPanelPZ69SpitfireLFMkIX : RadioPanelPZ69Base
    {
        private enum CurrentSpitfireLFMkIXRadioMode
        {
            HFRADIO,
            HFRADIO2,
            IFF,
            NO_USE
        }

        private CurrentSpitfireLFMkIXRadioMode _currentUpperRadioMode = CurrentSpitfireLFMkIXRadioMode.HFRADIO;
        private CurrentSpitfireLFMkIXRadioMode _currentLowerRadioMode = CurrentSpitfireLFMkIXRadioMode.HFRADIO;

        /*
         *  COM1 Large Freq Mode  0 => 4
         *  COM1 Small Fine Channel/OFF
         *  Freq. Selector Light Switch
         *  
         *  COM2 Large IFF Circuit D
         *  COM2 Small IFF Circuit B
         *  COM2 ACT/STBY NOT IMPL IFF Destruction
         *     
         */

        /*
        *  HF RADIO
        *  COM1 Large Freq Mode
        *  COM1 Small Fine Channel/OFF 0 => 4
        *  Freq. Selector Light Switch        
        */
        private readonly object _lockHFRadioPresetDialObject1 = new();
        private DCSBIOSOutput _hfOffDcsbiosOutput;
        private DCSBIOSCommand _hfOffCommand;
        private DCSBIOSOutput _hfChannelAPresetOutput;
        private DCSBIOSCommand _hfChannelACommand;
        private DCSBIOSOutput _hfChannelBPresetOutput;
        private DCSBIOSCommand _hfChannelBCommand;
        private DCSBIOSOutput _hfChannelCPresetOutput;
        private DCSBIOSCommand _hfChannelCCommand;
        private DCSBIOSOutput _hfChannelDPresetOutput;
        private DCSBIOSCommand _hfChannelDCommand;
        private volatile uint _hfOffCockpitButton = 1;
        private volatile uint _hfChannelACockpitButton;
        private volatile uint _hfChannelBCockpitButton;
        private volatile uint _hfChannelCCockpitButton;
        private volatile uint _hfChannelDCockpitButton;
        private readonly ClickSkipper _hfChannelPresetDialSkipper = new(2);
        private const string HF_RADIO_LIGHT_SWITCH_COMMAND = "RCTRL_DIM TOGGLE\n";
        private readonly object _lockHFRadioModeDialObject1 = new();
        private volatile uint _hfModeCockpitDialPosition = 1;
        private DCSBIOSOutput _hfModeDialPresetOutput;
        private DCSBIOSCommand _hfModeCommand;
        private readonly ClickSkipper _hfModePresetDialSkipper = new(2);

        /* 
        *  COM2 Large IFF Circuit D
        *  COM2 Small IFF Circuit B
        *  COM2 ACT/STBY IFF Destruction
        */
        private readonly object _lockIFFDialObject1 = new();
        private DCSBIOSOutput _iffBiffDcsbiosOutputDial;
        private DCSBIOSCommand _iffBDialCommand;
        private DCSBIOSOutput _iffDiffDcsbiosOutputDial;
        private DCSBIOSCommand _iffDDialCommand;
        private volatile uint _iffBiffCockpitDialPos = 1;
        private volatile uint _iffDiffCockpitDialPos;
        private readonly ClickSkipper _iffBiffDialSkipper = new(2);
        private readonly ClickSkipper _iffDiffDialSkipper = new(2);
        private const string IFFB_COMMAND_INC = "IFF_B INC\n";
        private const string IFFB_COMMAND_DEC = "IFF_B DEC\n";
        private const string IFFD_COMMAND_INC = "IFF_D INC\n";
        private const string IFFD_COMMAND_DEC = "IFF_D DEC\n";
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SpitfireLFMkIX(HIDSkeleton hidSkeleton)
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

            // COM1
            (_hfOffCommand ,_hfOffDcsbiosOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RCTRL_OFF");
            (_hfChannelACommand, _hfChannelAPresetOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RCTRL_A");
            (_hfChannelBCommand, _hfChannelBPresetOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RCTRL_B");
            (_hfChannelCCommand, _hfChannelCPresetOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RCTRL_C");
            (_hfChannelDCommand, _hfChannelDPresetOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RCTRL_D");
            (_hfModeCommand, _hfModeDialPresetOutput) = DCSBIOSControlLocator.GetUIntCommandAndOutput("RCTRL_T_MODE");
            // COM2
            (_iffBDialCommand, _iffBiffDcsbiosOutputDial) = DCSBIOSControlLocator.GetUIntCommandAndOutput("IFF_B");
            (_iffDDialCommand, _iffDiffDcsbiosOutputDial) = DCSBIOSControlLocator.GetUIntCommandAndOutput("IFF_D");

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

                // HF Radio Off Button
                if (_hfOffDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        _hfOffCockpitButton = _hfOffDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Channel A Button
                if (_hfChannelAPresetOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        _hfChannelACockpitButton = _hfChannelAPresetOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Channel B Button
                if (_hfChannelBPresetOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        _hfChannelBCockpitButton = _hfChannelBPresetOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Channel C Button
                if (_hfChannelCPresetOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        _hfChannelCCockpitButton = _hfChannelCPresetOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Channel B Button
                if (_hfChannelDPresetOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        _hfChannelDCockpitButton = _hfChannelDPresetOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Mode
                if (_hfModeDialPresetOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockHFRadioModeDialObject1)
                    {
                        _hfModeCockpitDialPosition = _hfModeDialPresetOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // IFF B
                if (_iffBiffDcsbiosOutputDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockIFFDialObject1)
                    {
                        _iffBiffCockpitDialPos = _iffBiffDcsbiosOutputDial.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // HF Radio Channel B Button
                if (_iffDiffDcsbiosOutputDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockIFFDialObject1)
                    {
                        _iffDiffCockpitDialPos = _iffDiffDcsbiosOutputDial.LastUIntValue;
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
                        var radioPanelKnob = (RadioPanelKnobSpitfireLFMkIX)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_HFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSpitfireLFMkIXRadioMode.HFRADIO);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSpitfireLFMkIXRadioMode.IFF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSpitfireLFMkIXRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_HFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSpitfireLFMkIXRadioMode.HFRADIO);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSpitfireLFMkIXRadioMode.IFF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSpitfireLFMkIXRadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(HF_RADIO_LIGHT_SWITCH_COMMAND);
                                        }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(HF_RADIO_LIGHT_SWITCH_COMMAND);
                                        }
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                DCSAircraft.SelectedAircraft.Description,
                                HIDInstance,
                                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_SPITFIRELFMKIX,
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
                    var radioPanelKnobSpitfireLFMkIX = (RadioPanelKnobSpitfireLFMkIX)o;
                    if (radioPanelKnobSpitfireLFMkIX.IsOn)
                    {
                        switch (radioPanelKnobSpitfireLFMkIX.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                _hfModePresetDialSkipper.Click(GetHFRadioModeStringCommand(true));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffDiffDialSkipper.Click(IFFD_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                // MODE
                                                _hfModePresetDialSkipper.Click(GetHFRadioModeStringCommand(false));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffDiffDialSkipper.Click(IFFD_COMMAND_DEC);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                // CHANNEL
                                                _hfChannelPresetDialSkipper.Click(GetHFRadioChannelStringCommand(true));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffBiffDialSkipper.Click(IFFB_COMMAND_INC);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                _hfChannelPresetDialSkipper.Click(GetHFRadioChannelStringCommand(false));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffBiffDialSkipper.Click(IFFB_COMMAND_DEC);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                _hfModePresetDialSkipper.Click(GetHFRadioModeStringCommand(true));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffDiffDialSkipper.Click(IFFD_COMMAND_INC);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                _hfModePresetDialSkipper.Click(GetHFRadioModeStringCommand(false));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffDiffDialSkipper.Click(IFFD_COMMAND_DEC);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                _hfChannelPresetDialSkipper.Click(GetHFRadioChannelStringCommand(true));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffBiffDialSkipper.Click(IFFB_COMMAND_INC);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                                            {
                                                _hfChannelPresetDialSkipper.Click(GetHFRadioChannelStringCommand(false));
                                                break;
                                            }

                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                _iffBiffDialSkipper.Click(IFFB_COMMAND_DEC);
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
                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                            {
                                // 0-4
                                uint channel = 0;
                                lock (_lockHFRadioPresetDialObject1)
                                {
                                    if (_hfOffCockpitButton == 1)
                                    {
                                        channel = 0;
                                    }
                                    else if (_hfChannelACockpitButton == 1)
                                    {
                                        channel = 1;
                                    }
                                    else if (_hfChannelBCockpitButton == 1)
                                    {
                                        channel = 2;
                                    }
                                    else if (_hfChannelCCockpitButton == 1)
                                    {
                                        channel = 3;
                                    }
                                    else if (_hfChannelDCockpitButton == 1)
                                    {
                                        channel = 4;
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _hfModeCockpitDialPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }

                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                            {
                                // Preset Channel Selector
                                // 0-1
                                uint bChannel;
                                uint dChannel;
                                lock (_lockIFFDialObject1)
                                {
                                    bChannel = _iffBiffCockpitDialPos;
                                    dChannel = _iffDiffCockpitDialPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, dChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, bChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentSpitfireLFMkIXRadioMode.NO_USE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                            {
                                // 0-4
                                uint channel = 0;
                                lock (_lockHFRadioPresetDialObject1)
                                {
                                    if (_hfOffCockpitButton == 1)
                                    {
                                        channel = 0;
                                    }
                                    else if (_hfChannelACockpitButton == 1)
                                    {
                                        channel = 1;
                                    }
                                    else if (_hfChannelBCockpitButton == 1)
                                    {
                                        channel = 2;
                                    }
                                    else if (_hfChannelCCockpitButton == 1)
                                    {
                                        channel = 3;
                                    }
                                    else if (_hfChannelDCockpitButton == 1)
                                    {
                                        channel = 4;
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _hfModeCockpitDialPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }

                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                            {
                                // Preset Channel Selector
                                // 0-1
                                uint bChannel;
                                uint dChannel;
                                lock (_lockIFFDialObject1)
                                {
                                    bChannel = _iffBiffCockpitDialPos;
                                    dChannel = _iffDiffCockpitDialPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, dChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, bChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentSpitfireLFMkIXRadioMode.NO_USE:
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

        public override void ClearSettings(bool setIsDirty = false)
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobSpitfireLFMkIX.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentSpitfireLFMkIXRadioMode currentSpitfireLFMkIXRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentSpitfireLFMkIXRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentSpitfireLFMkIXRadioMode currentSpitfireLFMkIXRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentSpitfireLFMkIXRadioMode;
                // If NO_USE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private string GetHFRadioChannelStringCommand(bool moveUp)
        {
            lock (_lockHFRadioPresetDialObject1)
            {
                if (moveUp)
                {
                    if ((_hfOffCockpitButton == 1 || _hfOffCockpitButton == 0) && _hfChannelACockpitButton == 0 && _hfChannelBCockpitButton == 0
                        && _hfChannelCCockpitButton == 0 && _hfChannelDCockpitButton == 0)
                    {
                        return _hfChannelACommand.GetIncCommand();
                    }

                    if (_hfChannelACockpitButton == 1)
                    {
                        return _hfChannelBCommand.GetIncCommand();
                    }

                    if (_hfChannelBCockpitButton == 1)
                    {
                        return _hfChannelCCommand.GetIncCommand();
                    }

                    if (_hfChannelCCockpitButton == 1)
                    {
                        return _hfChannelDCommand.GetIncCommand();
                    }
                }
                else
                {
                    if (_hfChannelDCockpitButton == 1)
                    {
                        return _hfChannelCCommand.GetIncCommand();
                    }

                    if (_hfChannelCCockpitButton == 1)
                    {
                        return _hfChannelBCommand.GetIncCommand();
                    }

                    if (_hfChannelBCockpitButton == 1)
                    {
                        return _hfChannelACommand.GetIncCommand();
                    }

                    if (_hfChannelACockpitButton == 1)
                    {
                        return _hfOffCommand.GetIncCommand();
                    }
                }
            }
            return null;
        }

        private string GetHFRadioModeStringCommand(bool moveUp)
        {
            lock (_lockHFRadioModeDialObject1)
            {
                return _hfModeCommand.GetSetStateCommand(moveUp ? _hfModeCockpitDialPosition + 1 : _hfModeCockpitDialPosition - 1);
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
