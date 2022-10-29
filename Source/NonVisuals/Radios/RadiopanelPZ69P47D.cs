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
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Saitek;

    public class RadioPanelPZ69P47D : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentP47DRadioMode
        {
            HFRADIO,
            HFRADIO2,
            //IFF,
            NOUSE
        }

        private CurrentP47DRadioMode _currentUpperRadioMode = CurrentP47DRadioMode.HFRADIO;
        private CurrentP47DRadioMode _currentLowerRadioMode = CurrentP47DRadioMode.HFRADIO;

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
        private DCSBIOSOutput _hfRadioOffDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelAPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelBPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelCPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelDPresetDcsbiosOutput;
        private volatile uint _hfRadioOffCockpitButton = 1;
        private volatile uint _hfRadioChannelACockpitButton;
        private volatile uint _hfRadioChannelBCockpitButton;
        private volatile uint _hfRadioChannelCCockpitButton;
        private volatile uint _hfRadioChannelDCockpitButton;
        private int _hfRadioChannelPresetDialSkipper;
        private const string HF_RADIO_LIGHT_SWITCH_COMMAND = "RCTRL_DIM TOGGLE\n";
        private readonly object _lockHFRadioModeDialObject1 = new();
        private volatile uint _hfRadioModeCockpitDialPosition = 1;
        private DCSBIOSOutput _hfRadioModeDialPresetDcsbiosOutput;
        private int _hfRadioModePresetDialSkipper;
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

                // HF Radio Off Button
                if (e.Address == _hfRadioOffDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioOffCockpitButton;
                        _hfRadioOffCockpitButton = _hfRadioOffDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioOffCockpitButton)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // HF Radio Channel A Button
                if (e.Address == _hfRadioChannelAPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelACockpitButton;
                        _hfRadioChannelACockpitButton = _hfRadioChannelAPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelACockpitButton)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // HF Radio Channel B Button
                if (e.Address == _hfRadioChannelBPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelBCockpitButton;
                        _hfRadioChannelBCockpitButton = _hfRadioChannelBPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelBCockpitButton)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // HF Radio Channel C Button
                if (e.Address == _hfRadioChannelCPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelCCockpitButton;
                        _hfRadioChannelCCockpitButton = _hfRadioChannelCPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelCCockpitButton)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // HF Radio Channel B Button
                if (e.Address == _hfRadioChannelDPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelDCockpitButton;
                        _hfRadioChannelDCockpitButton = _hfRadioChannelDPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelDCockpitButton)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // HF Radio Mode
                if (e.Address == _hfRadioModeDialPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioModeDialObject1)
                    {
                        var tmp = _hfRadioModeCockpitDialPosition;
                        _hfRadioModeCockpitDialPosition = _hfRadioModeDialPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioModeCockpitDialPosition)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // IFF B
                /*if (e.Address == _iffBiffDcsbiosOutputDial.Address)
                {
                    lock (_lockIFFDialObject1)
                    {
                        var tmp = _iffBiffCockpitDialPos;
                        _iffBiffCockpitDialPos = _iffBiffDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _iffBiffCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //HF Radio Channel B Button
                if (e.Address == _iffDiffDcsbiosOutputDial.Address)
                {
                    lock (_lockIFFDialObject1)
                    {
                        var tmp = _iffDiffCockpitDialPos;
                        _iffDiffCockpitDialPos = _iffDiffDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _iffDiffCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }*/

                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (isFirstReport)
            {
                return;
            }

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
                                        SetUpperRadioMode(CurrentP47DRadioMode.HFRADIO);
                                    }

                                    break;
                                }

                            /*case RadioPanelPZ69KnobsP47D.UPPER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP47DRadioMode.IFF);
                                    }
                                    break;
                                }*/
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE4:
                            case RadioPanelPZ69KnobsP47D.UPPER_NO_USE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP47DRadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_HFRADIO:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP47DRadioMode.HFRADIO);
                                    }
                                    break;
                                }

                            /*case RadioPanelPZ69KnobsP47D.LOWER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP47DRadioMode.IFF);
                                    }
                                    break;
                                }*/
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE4:
                            case RadioPanelPZ69KnobsP47D.LOWER_NO_USE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP47DRadioMode.NOUSE);
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
                                    if (_currentLowerRadioMode == CurrentP47DRadioMode.HFRADIO)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(HF_RADIO_LIGHT_SWITCH_COMMAND);
                                        }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentP47DRadioMode.HFRADIO)
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
                                DCSFPProfile.SelectedProfile.Description,
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
                logger.Error(ex);
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
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // MODE
                                                if (!SkipHFRadioModeDialChange())
                                                {
                                                    var s = GetHFRadioModeStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffdDialChange())
                                                {
                                                    DCSBIOS.Send(IFFD_COMMAND_INC);
                                                }
                                                break;
                                            }*/
                                        case CurrentP47DRadioMode.NOUSE:
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
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // MODE
                                                if (!SkipHFRadioModeDialChange())
                                                {
                                                    var s = GetHFRadioModeStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffdDialChange())
                                                {
                                                    DCSBIOS.Send(IFFD_COMMAND_DEC);
                                                }
                                                break;
                                            }*/
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // CHANNEL
                                                if (!SkipHFRadioChannelPresetDialChange())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffbDialChange())
                                                {
                                                    DCSBIOS.Send(IFFB_COMMAND_INC);
                                                }
                                                break;
                                            }*/
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // CHANNEL
                                                if (!SkipHFRadioChannelPresetDialChange())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffbDialChange())
                                                {
                                                    DCSBIOS.Send(IFFB_COMMAND_DEC);
                                                }
                                                break;
                                            }*/
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // MODE
                                                if (!SkipHFRadioModeDialChange())
                                                {
                                                    var s = GetHFRadioModeStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffdDialChange())
                                                {
                                                    DCSBIOS.Send(IFFD_COMMAND_INC);
                                                }
                                                break;
                                            }*/
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // MODE
                                                if (!SkipHFRadioModeDialChange())
                                                {
                                                    var s = GetHFRadioModeStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffdDialChange())
                                                {
                                                    DCSBIOS.Send(IFFD_COMMAND_DEC);
                                                }
                                                break;
                                            }*/
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // CHANNEL
                                                if (!SkipHFRadioChannelPresetDialChange())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(true);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffbDialChange())
                                                {
                                                    DCSBIOS.Send(IFFB_COMMAND_INC);
                                                }
                                                break;
                                            }*/
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP47DRadioMode.HFRADIO:
                                            {
                                                // CHANNEL
                                                if (!SkipHFRadioChannelPresetDialChange())
                                                {
                                                    var s = GetHFRadioChannelStringCommand(false);
                                                    if (!string.IsNullOrEmpty(s))
                                                    {
                                                        DCSBIOS.Send(s);
                                                    }
                                                }
                                                break;
                                            }

                                        /*case CurrentP47DRadioMode.IFF:
                                            {
                                                if (!SkipIffbDialChange())
                                                {
                                                    DCSBIOS.Send(IFFB_COMMAND_DEC);
                                                }
                                                break;
                                            }*/
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
                logger.Error(ex);
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
                        case CurrentP47DRadioMode.HFRADIO:
                            {
                                // 0-4
                                uint channel = 0;
                                lock (_lockHFRadioPresetDialObject1)
                                {
                                    if (_hfRadioOffCockpitButton == 1)
                                    {
                                        channel = 0;
                                    }
                                    else if (_hfRadioChannelACockpitButton == 1)
                                    {
                                        channel = 1;
                                    }
                                    else if (_hfRadioChannelBCockpitButton == 1)
                                    {
                                        channel = 2;
                                    }
                                    else if (_hfRadioChannelCCockpitButton == 1)
                                    {
                                        channel = 3;
                                    }
                                    else if (_hfRadioChannelDCockpitButton == 1)
                                    {
                                        channel = 4;
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _hfRadioModeCockpitDialPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }

                        /*case CurrentP47DRadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                uint bChannel = 0;
                                uint dChannel = 0;
                                lock (_lockIFFDialObject1)
                                {
                                    bChannel = _iffBiffCockpitDialPos;
                                    dChannel = _iffDiffCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, dChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, bChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }*/
                        case CurrentP47DRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentP47DRadioMode.HFRADIO:
                            {
                                // 0-4
                                uint channel = 0;
                                lock (_lockHFRadioPresetDialObject1)
                                {
                                    if (_hfRadioOffCockpitButton == 1)
                                    {
                                        channel = 0;
                                    }
                                    else if (_hfRadioChannelACockpitButton == 1)
                                    {
                                        channel = 1;
                                    }
                                    else if (_hfRadioChannelBCockpitButton == 1)
                                    {
                                        channel = 2;
                                    }
                                    else if (_hfRadioChannelCCockpitButton == 1)
                                    {
                                        channel = 3;
                                    }
                                    else if (_hfRadioChannelDCockpitButton == 1)
                                    {
                                        channel = 4;
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _hfRadioModeCockpitDialPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }

                        /*case CurrentP47DRadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                uint bChannel = 0;
                                uint dChannel = 0;
                                lock (_lockIFFDialObject1)
                                {
                                    bChannel = _iffBiffCockpitDialPos;
                                    dChannel = _iffDiffCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, dChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, bChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }*/
                        case CurrentP47DRadioMode.NOUSE:
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
                logger.Error(ex);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        public sealed override void Startup()
        {
            try
            {
                // COM1
                _hfRadioOffDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_OFF");
                _hfRadioChannelAPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_A");
                _hfRadioChannelBPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_B");
                _hfRadioChannelCPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_C");
                _hfRadioChannelDPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_D");
                _hfRadioModeDialPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_T_MODE");
                // COM2
                // _iffBiffDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("IFF_B");
                // _iffDiffDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("IFF_D");
                StartListeningForHidPanelChanges();
                // IsAttached = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        
        public override void ClearSettings(bool setIsDirty = false)
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            DcsOutputAndColorBindingPZ55 dcsOutputAndColorBinding = new()
            {
                DCSBiosOutputLED = dcsBiosOutput,
                LEDColor = panelLEDColor,
                SaitekLEDPosition = saitekPanelLEDPosition
            };
            return dcsOutputAndColorBinding;
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
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentP47DRadioMode currentP47DRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentP47DRadioMode;
                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool SkipHFRadioChannelPresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentP47DRadioMode.HFRADIO || _currentLowerRadioMode == CurrentP47DRadioMode.HFRADIO)
                {
                    if (_hfRadioChannelPresetDialSkipper > 2)
                    {
                        _hfRadioChannelPresetDialSkipper = 0;
                        return false;
                    }
                    _hfRadioChannelPresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        /*
        private bool SkipIffdDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentP47DRadioMode.IFF || _currentLowerRadioMode == CurrentP47DRadioMode.IFF)
                {
                    if (_iffDiffDialSkipper > 2)
                    {
                        _iffDiffDialSkipper = 0;
                        return false;
                    }
                    _iffDiffDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private bool SkipIffbDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentP47DRadioMode.IFF || _currentLowerRadioMode == CurrentP47DRadioMode.IFF)
                {
                    if (_iffBiffDialSkipper > 2)
                    {
                        _iffBiffDialSkipper = 0;
                        return false;
                    }
                    _iffBiffDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }
        */
        private bool SkipHFRadioModeDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentP47DRadioMode.HFRADIO || _currentLowerRadioMode == CurrentP47DRadioMode.HFRADIO)
                {
                    if (_hfRadioModePresetDialSkipper > 2)
                    {
                        _hfRadioModePresetDialSkipper = 0;
                        return false;
                    }
                    _hfRadioModePresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private string GetHFRadioChannelStringCommand(bool moveUp)
        {
            lock (_lockHFRadioPresetDialObject1)
            {
                if (moveUp)
                {
                    if ((_hfRadioOffCockpitButton == 1 || _hfRadioOffCockpitButton == 0) && _hfRadioChannelACockpitButton == 0 && _hfRadioChannelBCockpitButton == 0
                        && _hfRadioChannelCCockpitButton == 0 && _hfRadioChannelDCockpitButton == 0)
                    {
                        return "RCTRL_A INC\n";
                    }

                    if (_hfRadioChannelACockpitButton == 1)
                    {
                        return "RCTRL_B INC\n";
                    }

                    if (_hfRadioChannelBCockpitButton == 1)
                    {
                        return "RCTRL_C INC\n";
                    }

                    if (_hfRadioChannelCCockpitButton == 1)
                    {
                        return "RCTRL_D INC\n";
                    }
                }
                else
                {
                    if (_hfRadioChannelDCockpitButton == 1)
                    {
                        return "RCTRL_C INC\n";
                    }

                    if (_hfRadioChannelCCockpitButton == 1)
                    {
                        return "RCTRL_B INC\n";
                    }

                    if (_hfRadioChannelBCockpitButton == 1)
                    {
                        return "RCTRL_A INC\n";
                    }

                    if (_hfRadioChannelACockpitButton == 1)
                    {
                        return "RCTRL_OFF INC\n";
                    }
                }
            }
            return null;
        }

        private string GetHFRadioModeStringCommand(bool moveUp)
        {
            lock (_lockHFRadioModeDialObject1)
            {
                if (moveUp)
                {
                    return "RCTRL_T_MODE " + (_hfRadioModeCockpitDialPosition + 1) + "\n";
                }
                return "RCTRL_T_MODE " + (_hfRadioModeCockpitDialPosition - 1) + "\n";
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

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }
    }
}
