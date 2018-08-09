using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69SpitfireLFMkIX : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobSpitfireLFMkIX> _radioPanelKnobs = new HashSet<RadioPanelKnobSpitfireLFMkIX>();
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
        private readonly object _lockHFRadioPresetDialObject1 = new object();
        private DCSBIOSOutput _hfRadioOffDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelAPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelBPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelCPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioChannelDPresetDcsbiosOutput;
        private volatile uint _hfRadioOffCockpitButton = 1;
        private volatile uint _hfRadioChannelACockpitButton = 0;
        private volatile uint _hfRadioChannelBCockpitButton = 0;
        private volatile uint _hfRadioChannelCCockpitButton = 0;
        private volatile uint _hfRadioChannelDCockpitButton = 0;
        private int _hfRadioChannelPresetDialSkipper;
        private const string _hfRadioLightSwitchCommand = "RCTRL_DIM TOGGLE\n";
        private readonly object _lockHFRadioModeDialObject1 = new object();
        private volatile uint _hfRadio1ModeCockpitDialPosition = 1;
        private volatile uint _hfRadio2ModeCockpitDialPosition = 1;
        private DCSBIOSOutput _hfRadioMode1DialPresetDcsbiosOutput;
        private DCSBIOSOutput _hfRadioMode2DialPresetDcsbiosOutput;
        private int _hfRadioModePresetDialSkipper;
        /* 
        *  COM2 Large IFF Circuit D
        *  COM2 Small IFF Circuit B
        *  COM2 ACT/STBY IFF Destruction
        */
        private readonly object _lockIFFDialObject1 = new object();
        private DCSBIOSOutput _iffBIFFDcsbiosOutputDial;
        private DCSBIOSOutput _iffDIFFDcsbiosOutputDial;
        private volatile uint _iffBIFFCockpitDialPos = 1;
        private volatile uint _iffDIFFCockpitDialPos = 0;
        private int _iffBIFFDialSkipper;
        private int _iffDIFFDialSkipper;
        private const string IFFBCommandInc = "IFF_B INC\n";
        private const string IFFBCommandDec = "IFF_B DEC\n";
        private const string IFFDCommandInc = "IFF_D INC\n";
        private const string IFFDCommandDec = "IFF_D DEC\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SpitfireLFMkIX(HIDSkeleton hidSkeleton, bool enableDCSBIOS = true) : base(hidSkeleton, enableDCSBIOS)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }


        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
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



                //HF Radio Off Button
                if (e.Address == _hfRadioOffDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioOffCockpitButton;
                        _hfRadioOffCockpitButton = _hfRadioOffDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioOffCockpitButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Channel A Button
                if (e.Address == _hfRadioChannelAPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelACockpitButton;
                        _hfRadioChannelACockpitButton = _hfRadioChannelAPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelACockpitButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Channel B Button
                if (e.Address == _hfRadioChannelBPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelBCockpitButton;
                        _hfRadioChannelBCockpitButton = _hfRadioChannelBPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelBCockpitButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Channel C Button
                if (e.Address == _hfRadioChannelCPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelCCockpitButton;
                        _hfRadioChannelCCockpitButton = _hfRadioChannelCPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelCCockpitButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Channel B Button
                if (e.Address == _hfRadioChannelDPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioPresetDialObject1)
                    {
                        var tmp = _hfRadioChannelDCockpitButton;
                        _hfRadioChannelDCockpitButton = _hfRadioChannelDPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadioChannelDCockpitButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Mode 1
                if (e.Address == _hfRadioMode1DialPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioModeDialObject1)
                    {
                        var tmp = _hfRadio1ModeCockpitDialPosition;
                        _hfRadio1ModeCockpitDialPosition = _hfRadioMode1DialPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadio1ModeCockpitDialPosition)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Mode 2
                if (e.Address == _hfRadioMode2DialPresetDcsbiosOutput.Address)
                {
                    lock (_lockHFRadioModeDialObject1)
                    {
                        var tmp = _hfRadio2ModeCockpitDialPosition;
                        _hfRadio2ModeCockpitDialPosition = _hfRadioMode2DialPresetDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _hfRadio2ModeCockpitDialPosition)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //IFF B
                if (e.Address == _iffBIFFDcsbiosOutputDial.Address)
                {
                    lock (_lockIFFDialObject1)
                    {
                        var tmp = _iffBIFFCockpitDialPos;
                        _iffBIFFCockpitDialPos = _iffBIFFDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _iffBIFFCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //HF Radio Channel B Button
                if (e.Address == _iffDIFFDcsbiosOutputDial.Address)
                {
                    lock (_lockIFFDialObject1)
                    {
                        var tmp = _iffDIFFCockpitDialPos;
                        _iffDIFFCockpitDialPos = _iffDIFFDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _iffDIFFCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();

            }
            catch (Exception ex)
            {
                Common.LogError(83001, ex);
            }
        }


        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NOUSE0:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NOUSE1:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NOUSE2:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NOUSE3:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSpitfireLFMkIXRadioMode.NOUSE);
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
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NOUSE0:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NOUSE1:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NOUSE2:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NOUSE3:
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSpitfireLFMkIXRadioMode.NOUSE);
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
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(_hfRadioLightSwitchCommand);
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
                                            DCSBIOS.Send(_hfRadioLightSwitchCommand);
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(83006, ex);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio AdjustFrequency()");

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
                                                //MODE
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFDDialChange())
                                                {
                                                    DCSBIOS.Send(IFFDCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSpitfireLFMkIXRadioMode.NOUSE:
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
                                                //MODE
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFDDialChange())
                                                {
                                                    DCSBIOS.Send(IFFDCommandDec);
                                                }
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
                                                //CHANNEL
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFBDialChange())
                                                {
                                                    DCSBIOS.Send(IFFBCommandInc);
                                                }
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
                                                //CHANNEL
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFBDialChange())
                                                {
                                                    DCSBIOS.Send(IFFBCommandDec);
                                                }
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
                                                //MODE
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFDDialChange())
                                                {
                                                    DCSBIOS.Send(IFFDCommandInc);
                                                }
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
                                                //MODE
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFDDialChange())
                                                {
                                                    DCSBIOS.Send(IFFDCommandDec);
                                                }
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
                                                //CHANNEL
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFBDialChange())
                                                {
                                                    DCSBIOS.Send(IFFBCommandInc);
                                                }
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
                                                //CHANNEL
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
                                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                                            {
                                                if (!SkipIFFBDialChange())
                                                {
                                                    DCSBIOS.Send(IFFBCommandDec);
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
                Common.LogError(83007, ex);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio AdjustFrequency()");
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

                    Common.DebugP("Entering Spitfire LF Mk. IX Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentSpitfireLFMkIXRadioMode.HFRADIO:
                            {
                                //0-2
                                uint mode = 0;
                                //0-4
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
                                lock (_lockHFRadioModeDialObject1)
                                {
                                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 0)
                                    {
                                        mode = 0;
                                    }
                                    else
                                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 1)
                                    {
                                        mode = 1;
                                    }
                                    else
                                    if (_hfRadio1ModeCockpitDialPosition == 1 && _hfRadio2ModeCockpitDialPosition == 1)
                                    {
                                        mode = 2;
                                    }
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, mode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                uint bChannel = 0;
                                uint dChannel = 0;
                                lock (_lockIFFDialObject1)
                                {
                                    bChannel = _iffBIFFCockpitDialPos;
                                    dChannel = _iffDIFFCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, dChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, bChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentSpitfireLFMkIXRadioMode.NOUSE:
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
                                //0-2
                                uint mode = 0;
                                //0-4
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
                                lock (_lockHFRadioModeDialObject1)
                                {
                                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 0)
                                    {
                                        mode = 0;
                                    }
                                    else
                                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 1)
                                    {
                                        mode = 1;
                                    }
                                    else
                                    if (_hfRadio1ModeCockpitDialPosition == 1 && _hfRadio2ModeCockpitDialPosition == 1)
                                    {
                                        mode = 2;
                                    }
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, mode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, channel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentSpitfireLFMkIXRadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                uint bChannel = 0;
                                uint dChannel = 0;
                                lock (_lockIFFDialObject1)
                                {
                                    bChannel = _iffBIFFCockpitDialPos;
                                    dChannel = _iffDIFFCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, dChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, bChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentSpitfireLFMkIXRadioMode.NOUSE:
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
                Common.LogError(83011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering Spitfire LF Mk. IX Radio OnReport()");
                    //if (IsAttached == false) { return; }

                    if (report.Data.Length == 3)
                    {
                        Array.Copy(NewRadioPanelValue, OldRadioPanelValue, 3);
                        Array.Copy(report.Data, NewRadioPanelValue, 3);
                        var hashSet = GetHashSetOfChangedKnobs(OldRadioPanelValue, NewRadioPanelValue);
                        PZ69KnobChanged(hashSet);
                        OnSwitchesChanged(hashSet);
                        FirstReportHasBeenRead = true;
                        if (1 == 2 && Common.DebugOn)
                        {
                            var stringBuilder = new StringBuilder();
                            for (var i = 0; i < report.Data.Length; i++)
                            {
                                stringBuilder.Append(Convert.ToString(report.Data[i], 2).PadLeft(8, '0') + "  ");
                            }
                            Common.DebugP(stringBuilder.ToString());
                            if (hashSet.Count > 0)
                            {
                                Common.DebugP("\nFollowing knobs has been changed:\n");
                                foreach (var radioPanelKnob in hashSet)
                                {
                                    var knob = (RadioPanelKnobSpitfireLFMkIX)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobSpitfireLFMkIX)radioPanelKnob));
                                }
                            }
                        }
                        Common.DebugP("\r\nDone!\r\n");
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message + "\n" + ex.StackTrace);
                    SetLastException(ex);
                }
                try
                {
                    if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                    {
                        Common.DebugP("Adding callback " + TypeOfSaitekPanel + " " + GuidString);
                        HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message + "\n" + ex.StackTrace);
                    SetLastException(ex);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(83012, ex);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio GetHashSetOfChangedKnobs()");


                for (var i = 0; i < 3; i++)
                {
                    var oldByte = oldValue[i];
                    var newByte = newValue[i];

                    foreach (var radioPanelKnob in _radioPanelKnobs)
                    {
                        if (radioPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, radioPanelKnob.Mask) || !FirstReportHasBeenRead))
                        {
                            radioPanelKnob.IsOn = FlagValue(newValue, radioPanelKnob);
                            result.Add(radioPanelKnob);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(83013, ex);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("Spitfire LF Mk. IX");

                //COM1
                _hfRadioOffDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_OFF");
                _hfRadioChannelAPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_A");
                _hfRadioChannelBPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_B");
                _hfRadioChannelCPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_C");
                _hfRadioChannelDPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_D");
                _hfRadioMode1DialPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_T_MODE1");
                _hfRadioMode2DialPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RCTRL_T_MODE2");

                //COM2
                _iffBIFFDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("IFF_B");
                _iffDIFFDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("IFF_D");


                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69SpitfireLFMkIX.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio Shutdown()");
        }

        public override void ClearSettings()
        {
            //todo
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            //todo
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            _radioPanelKnobs = RadioPanelKnobSpitfireLFMkIX.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobSpitfireLFMkIX radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentSpitfireLFMkIXRadioMode currentSpitfireLFMkIXRadioMode)
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentSpitfireLFMkIXRadioMode);
                _currentUpperRadioMode = currentSpitfireLFMkIXRadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(83014, ex);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentSpitfireLFMkIXRadioMode currentSpitfireLFMkIXRadioMode)
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentSpitfireLFMkIXRadioMode);
                _currentLowerRadioMode = currentSpitfireLFMkIXRadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(83015, ex);
            }
            Common.DebugP("Leaving Spitfire LF Mk. IX Radio SetLowerRadioMode()");
        }

        private bool SkipHFRadioChannelPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio SkipHFRadioChannelPresetDialChange()");
                if (_currentUpperRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO || _currentLowerRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO)
                {
                    if (_hfRadioChannelPresetDialSkipper > 2)
                    {
                        _hfRadioChannelPresetDialSkipper = 0;
                        Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipHFRadioChannelPresetDialChange()");
                        return false;
                    }
                    _hfRadioChannelPresetDialSkipper++;
                    Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipHFRadioChannelPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipHFRadioChannelPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(83009, ex);
            }
            return false;
        }

        private bool SkipIFFDDialChange()
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio SkipIFFDDialChange()");
                if (_currentUpperRadioMode == CurrentSpitfireLFMkIXRadioMode.IFF || _currentLowerRadioMode == CurrentSpitfireLFMkIXRadioMode.IFF)
                {
                    if (_iffDIFFDialSkipper > 2)
                    {
                        _iffDIFFDialSkipper = 0;
                        Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipIFFDDialChange()");
                        return false;
                    }
                    _iffDIFFDialSkipper++;
                    Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipIFFDDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipIFFDDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(83015, ex);
            }
            return false;
        }

        private bool SkipIFFBDialChange()
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio SkipIFFBDialChange()");
                if (_currentUpperRadioMode == CurrentSpitfireLFMkIXRadioMode.IFF || _currentLowerRadioMode == CurrentSpitfireLFMkIXRadioMode.IFF)
                {
                    if (_iffBIFFDialSkipper > 2)
                    {
                        _iffBIFFDialSkipper = 0;
                        Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipIFFBDialChange()");
                        return false;
                    }
                    _iffBIFFDialSkipper++;
                    Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipIFFBDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipIFFBDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(83015, ex);
            }
            return false;
        }

        private bool SkipHFRadioModeDialChange()
        {
            try
            {
                Common.DebugP("Entering Spitfire LF Mk. IX Radio SkipHFRadioModeDialChange()");
                if (_currentUpperRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO || _currentLowerRadioMode == CurrentSpitfireLFMkIXRadioMode.HFRADIO)
                {
                    if (_hfRadioModePresetDialSkipper > 2)
                    {
                        _hfRadioModePresetDialSkipper = 0;
                        Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipHFRadioModeDialChange()");
                        return false;
                    }
                    _hfRadioModePresetDialSkipper++;
                    Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipHFRadioModeDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Spitfire LF Mk. IX Radio SkipHFRadioModeDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(83110, ex);
            }
            return false;
        }

        private string GetHFRadioChannelStringCommand(bool moveUp)
        {
            lock (_lockHFRadioPresetDialObject1)
            {
                if (moveUp)
                {
                    if ((_hfRadioOffCockpitButton == 1 || _hfRadioOffCockpitButton == 0) && _hfRadioChannelACockpitButton == 0 && _hfRadioChannelBCockpitButton == 0 && _hfRadioChannelCCockpitButton == 0 && _hfRadioChannelDCockpitButton == 0)
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
                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 0)
                    {
                        return "RCTRL_T_MODE1 INC\n";
                    }
                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 1)
                    {
                        return "RCTRL_T_MODE1 INC\n";
                    }
                }
                else
                {

                    if (_hfRadio1ModeCockpitDialPosition == 1 && _hfRadio2ModeCockpitDialPosition == 1)
                    {
                        return "RCTRL_T_MODE1 DEC\n";
                    }
                    if (_hfRadio1ModeCockpitDialPosition == 0 && _hfRadio2ModeCockpitDialPosition == 1)
                    {
                        return "RCTRL_T_MODE2 DEC\n";
                    }
                }
            }
            return null;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }

    }
}
