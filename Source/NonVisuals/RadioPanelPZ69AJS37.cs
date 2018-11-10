using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69AJS37 : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobAJS37> _radioPanelKnobs = new HashSet<RadioPanelKnobAJS37>();
        private CurrentAJS37RadioMode _currentUpperRadioMode = CurrentAJS37RadioMode.FR22;
        private CurrentAJS37RadioMode _currentLowerRadioMode = CurrentAJS37RadioMode.FR22;

        /*AJS-37 FR22 COM1*/
        //Large dial freqs can't be read from DCS as it is now (15.11.2017)
        //Small dial 
        /*private ClickSpeedDetector _bigFreqFR22IncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _bigFreqFR22DecreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _smallFreqFR22IncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _smallFreqFR22DecreaseChangeMonitor = new ClickSpeedDetector(20);*/
        //const int ChangeValue = 10;
        private enum FR22DialSideSelected
        {
            Right,
            Left
        }
        private FR22DialSideSelected _fr22DialSideSelected = FR22DialSideSelected.Right;
        private const string FR22LeftBigDialIncreaseCommand = "FR22_OUTER_LEFT_KNOB +8000\n";
        private const string FR22LeftSmallDialIncreaseCommand = "FR22_INNER_LEFT_KNOB +8000\n";
        private const string FR22LeftBigDialDecreaseCommand = "FR22_OUTER_LEFT_KNOB -8000\n";
        private const string FR22LeftSmallDialDecreaseCommand = "FR22_INNER_LEFT_KNOB -8000\n";
        private const string FR22RightBigDialIncreaseCommand = "FR22_OUTER_RIGHT_KNOB +8000\n";
        private const string FR22RightSmallDialIncreaseCommand = "FR22_INNER_RIGHT_KNOB +8000\n";
        private const string FR22RightBigDialDecreaseCommand = "FR22_OUTER_RIGHT_KNOB -8000\n";
        private const string FR22RightSmallDialDecreaseCommand = "FR22_INNER_RIGHT_KNOB -8000\n";


        /*AJS-37 FR24 COM2*/
        //Large dial Presets
        //Small dial Radio Volume

        /*AJS-37 TILS NAV1*/
        //Large dial TILS Selector 1-10 11-20 TILS_CHANNEL_SELECT
        //1   2  3  4  5  6  7  8  9 10
        //11 12 13 14 15 16 17 18 19 20
        //Small dial Master Mode Selector
        //ACT/STBY Toggles the ranges
        private volatile uint _tilsChannelCockpitValue;
        private volatile uint _tilsChannelLayerSelectorCockpitValue;
        private volatile uint _masterModeSelectorCockpitValue;
        private readonly object _lockTILSChannelSelectorDialObject1 = new object();
        private readonly object _lockTILSChannelLayerSelectorObject2 = new object();
        private readonly object _lockMasterModeSelectorObject = new object();
        private DCSBIOSOutput _tilsChannelSelectorDcsbiosOutput;
        private DCSBIOSOutput _tilsChannelLayerSelectorDcsbiosOutput;
        private DCSBIOSOutput _masterModeSelectorDcsbiosOutput;
        private const string TILSChannelDialCommandInc = "TILS_CHANNEL_SELECT INC\n";
        private const string TILSChannelDialCommandDec = "TILS_CHANNEL_SELECT DEC\n";
        private const string TILSChannelLayerDialCommandToggle = "TILS_CHANNEL_LAYER TOGGLE\n";
        private const string MasterModeSelectorCommandInc = "MASTER_MODE_SELECT INC\n";
        private const string MasterModeSelectorCommandDec = "MASTER_MODE_SELECT DEC\n";
        private int _tilsChannelDialSkipper;
        private int _masterModeSelectorDialSkipper;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69AJS37(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                /*
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }

                */
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
            }
            ShowFrequenciesOnPanel();
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



                //TILS Channel Selector
                if (e.Address == _tilsChannelSelectorDcsbiosOutput.Address)
                {
                    lock (_lockTILSChannelSelectorDialObject1)
                    {
                        var tmp = _tilsChannelCockpitValue;
                        _tilsChannelCockpitValue = _tilsChannelSelectorDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _tilsChannelCockpitValue)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //TILS Channel Mode
                if (e.Address == _tilsChannelLayerSelectorDcsbiosOutput.Address)
                {
                    lock (_lockTILSChannelLayerSelectorObject2)
                    {
                        var tmp = _tilsChannelLayerSelectorCockpitValue;
                        _tilsChannelLayerSelectorCockpitValue = _tilsChannelLayerSelectorDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _tilsChannelLayerSelectorCockpitValue)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //Master Mode Selector
                if (e.Address == _masterModeSelectorDcsbiosOutput.Address)
                {
                    lock (_lockMasterModeSelectorObject)
                    {
                        var tmp = _masterModeSelectorCockpitValue;
                        _masterModeSelectorCockpitValue = _masterModeSelectorDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _masterModeSelectorCockpitValue)
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
                Common.LogError(78001, ex);
            }
        }


        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAJS37 knob)
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio SendFrequencyToDCSBIOS()");
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }
                switch (knob)
                {
                    case RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentAJS37RadioMode.FR22:
                                    {
                                        break;
                                    }
                                case CurrentAJS37RadioMode.FR24:
                                    {
                                        break;
                                    }
                                case CurrentAJS37RadioMode.TILS:
                                    {
                                        DCSBIOS.Send(TILSChannelLayerDialCommandToggle);
                                        break;
                                    }
                                case CurrentAJS37RadioMode.NOUSE:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentAJS37RadioMode.FR22:
                                    {
                                        break;
                                    }
                                case CurrentAJS37RadioMode.FR24:
                                    {
                                        break;
                                    }
                                case CurrentAJS37RadioMode.TILS:
                                    {
                                        DCSBIOS.Send(TILSChannelLayerDialCommandToggle);
                                        break;
                                    }
                                case CurrentAJS37RadioMode.NOUSE:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78002, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio SendFrequencyToDCSBIOS()");
        }


        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobAJS37)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsAJS37.UPPER_FR22:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAJS37RadioMode.FR22);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_FR24:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAJS37RadioMode.FR24);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_TILS:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAJS37RadioMode.TILS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_FR22:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAJS37RadioMode.FR22);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_FR24:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAJS37RadioMode.FR24);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_TILS:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAJS37RadioMode.TILS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_NOUSE0:
                            case RadioPanelPZ69KnobsAJS37.UPPER_NOUSE1:
                            case RadioPanelPZ69KnobsAJS37.UPPER_NOUSE2:
                            case RadioPanelPZ69KnobsAJS37.UPPER_NOUSE3:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NOUSE0:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NOUSE1:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NOUSE2:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NOUSE3:
                                {
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentAJS37RadioMode.FR22)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            _fr22DialSideSelected = _fr22DialSideSelected == FR22DialSideSelected.Left ? FR22DialSideSelected.Right : FR22DialSideSelected.Left;
                                        }
                                    }
                                    else if (_currentUpperRadioMode == CurrentAJS37RadioMode.FR24 && radioPanelKnob.IsOn)
                                    {

                                    }
                                    else if (_currentUpperRadioMode == CurrentAJS37RadioMode.TILS && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(TILSChannelLayerDialCommandToggle);
                                    }
                                    else if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentAJS37RadioMode.FR22)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            _fr22DialSideSelected = _fr22DialSideSelected == FR22DialSideSelected.Left ? FR22DialSideSelected.Right : FR22DialSideSelected.Left;
                                        }
                                    }
                                    else if (_currentLowerRadioMode == CurrentAJS37RadioMode.FR24 && radioPanelKnob.IsOn)
                                    {

                                    }
                                    else if (_currentLowerRadioMode == CurrentAJS37RadioMode.TILS && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(TILSChannelLayerDialCommandToggle);
                                    }
                                    else if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH);
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
                Common.LogError(78006, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobAJS37 = (RadioPanelKnobAJS37)o;
                    if (radioPanelKnobAJS37.IsOn)
                    {
                        switch (radioPanelKnobAJS37.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftBigDialIncreaseCommand : FR22RightBigDialIncreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTILSChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILSChannelDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftBigDialDecreaseCommand : FR22RightBigDialDecreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTILSChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILSChannelDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftSmallDialIncreaseCommand : FR22RightSmallDialIncreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipMasterModeSelectorChange())
                                                {
                                                    DCSBIOS.Send(MasterModeSelectorCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftSmallDialDecreaseCommand : FR22RightSmallDialDecreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipMasterModeSelectorChange())
                                                {
                                                    DCSBIOS.Send(MasterModeSelectorCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftBigDialIncreaseCommand : FR22RightBigDialIncreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {

                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTILSChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILSChannelDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftBigDialDecreaseCommand : FR22RightBigDialDecreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {

                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTILSChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILSChannelDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftSmallDialIncreaseCommand : FR22RightSmallDialIncreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipMasterModeSelectorChange())
                                                {
                                                    DCSBIOS.Send(MasterModeSelectorCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAJS37RadioMode.FR22:
                                        {
                                            DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22LeftSmallDialDecreaseCommand : FR22RightSmallDialDecreaseCommand);
                                            break;
                                        }
                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipMasterModeSelectorChange())
                                                {
                                                    DCSBIOS.Send(MasterModeSelectorCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentAJS37RadioMode.NOUSE:
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
                Common.LogError(78007, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio AdjustFrequency()");
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

                    Common.DebugP("Entering AJS-37 Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentAJS37RadioMode.FR22:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentAJS37RadioMode.FR24:
                            {
                                break;
                            }
                        case CurrentAJS37RadioMode.TILS:
                            {
                                uint layerSelector = 0;
                                uint channelSelector = 0;
                                uint masterModeSelector = 0;
                                lock (_lockTILSChannelSelectorDialObject1)
                                {
                                    channelSelector = _tilsChannelCockpitValue;
                                }
                                lock (_lockTILSChannelLayerSelectorObject2)
                                {
                                    layerSelector = _tilsChannelLayerSelectorCockpitValue;
                                }
                                lock (_lockMasterModeSelectorObject)
                                {
                                    masterModeSelector = _masterModeSelectorCockpitValue;
                                }
                                if (channelSelector == 0)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, 0, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    if (layerSelector == 1)
                                    {
                                        channelSelector = channelSelector + 10;
                                    }
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, channelSelector, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, masterModeSelector, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentAJS37RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentAJS37RadioMode.FR22:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentAJS37RadioMode.FR24:
                            {
                                break;
                            }
                        case CurrentAJS37RadioMode.TILS:
                            {
                                uint layerSelector = 0;
                                uint channelSelector = 0;
                                uint masterModeSelector = 0;
                                lock (_lockTILSChannelSelectorDialObject1)
                                {
                                    channelSelector = _tilsChannelCockpitValue;
                                }
                                lock (_lockTILSChannelLayerSelectorObject2)
                                {
                                    layerSelector = _tilsChannelLayerSelectorCockpitValue;
                                }
                                lock (_lockMasterModeSelectorObject)
                                {
                                    masterModeSelector = _masterModeSelectorCockpitValue;
                                }
                                if (channelSelector == 0)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, 0, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    if (layerSelector == 1)
                                    {
                                        channelSelector = channelSelector + 10;
                                    }
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, channelSelector, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, masterModeSelector, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentAJS37RadioMode.NOUSE:
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
                Common.LogError(78011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving AJS-37 Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering AJS-37 Radio OnReport()");
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
                                    var knob = (RadioPanelKnobAJS37)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobAJS37)radioPanelKnob));
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
                Common.LogError(78012, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering AJS-37 Radio GetHashSetOfChangedKnobs()");


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
                Common.LogError(78013, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("AJS-37");

                //COM1


                //COM2


                //NAV1
                _tilsChannelSelectorDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("TILS_CHANNEL_SELECT");
                _tilsChannelLayerSelectorDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("TILS_CHANNEL_LAYER");
                _masterModeSelectorDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("MASTER_MODE_SELECT");

                //NAV2


                //ADF
                //TODO


                //XPDR

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69AJS37.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving AJS-37 Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobAJS37.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobAJS37 radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentAJS37RadioMode currentAJS37RadioMode)
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentAJS37RadioMode);
                _currentUpperRadioMode = currentAJS37RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(78014, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentAJS37RadioMode currentAJS37RadioMode)
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentAJS37RadioMode);
                _currentLowerRadioMode = currentAJS37RadioMode;
                //If NOUSE then send next round of e.Data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            Common.DebugP("Leaving AJS-37 Radio SetLowerRadioMode()");
        }

        private bool SkipTILSChannelDialChange()
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio SkipR863PresetDialChange()");
                if (_currentUpperRadioMode == CurrentAJS37RadioMode.TILS || _currentLowerRadioMode == CurrentAJS37RadioMode.TILS)
                {
                    if (_tilsChannelDialSkipper > 2)
                    {
                        _tilsChannelDialSkipper = 0;
                        Common.DebugP("Leaving AJS-37 Radio SkipTILSChannelDialChange()");
                        return false;
                    }
                    _tilsChannelDialSkipper++;
                    Common.DebugP("Leaving AJS-37 Radio SkipTILSChannelDialChange()");
                    return true;
                }
                Common.DebugP("Leaving AJS-37 Radio SkipTILSChannelDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        private bool SkipMasterModeSelectorChange()
        {
            try
            {
                Common.DebugP("Entering AJS-37 Radio SkipMasterModeSelectorChange()");
                if (_currentUpperRadioMode == CurrentAJS37RadioMode.TILS || _currentLowerRadioMode == CurrentAJS37RadioMode.TILS)
                {
                    if (_masterModeSelectorDialSkipper > 2)
                    {
                        _masterModeSelectorDialSkipper = 0;
                        Common.DebugP("Leaving AJS-37 Radio SkipMasterModeSelectorChange()");
                        return false;
                    }
                    _masterModeSelectorDialSkipper++;
                    Common.DebugP("Leaving AJS-37 Radio SkipMasterModeSelectorChange()");
                    return true;
                }
                Common.DebugP("Leaving AJS-37 Radio SkipMasterModeSelectorChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }

    }
}
