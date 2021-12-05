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
    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Saitek;

    public class RadioPanelPZ69AJS37 : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private CurrentAJS37RadioMode _currentUpperRadioMode = CurrentAJS37RadioMode.FR22;
        private CurrentAJS37RadioMode _currentLowerRadioMode = CurrentAJS37RadioMode.FR22;

        /*AJS-37 FR22 COM1*/
        // Large dial freqs can't be read from DCS as it is now (15.11.2017)
        // Small dial 
        /*private ClickSpeedDetector _bigFreqFR22IncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _bigFreqFR22DecreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _smallFreqFR22IncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _smallFreqFR22DecreaseChangeMonitor = new ClickSpeedDetector(20);*/
        // const int ChangeValue = 10;
        private enum FR22DialSideSelected
        {
            Right,
            Left
        }
        private FR22DialSideSelected _fr22DialSideSelected = FR22DialSideSelected.Right;
        private const string FR22_LEFT_BIG_DIAL_INCREASE_COMMAND = "FR22_OUTER_LEFT_KNOB +8000\n";
        private const string FR22_LEFT_SMALL_DIAL_INCREASE_COMMAND = "FR22_INNER_LEFT_KNOB +8000\n";
        private const string FR22_LEFT_BIG_DIAL_DECREASE_COMMAND = "FR22_OUTER_LEFT_KNOB -8000\n";
        private const string FR22_LEFT_SMALL_DIAL_DECREASE_COMMAND = "FR22_INNER_LEFT_KNOB -8000\n";
        private const string FR22_RIGHT_BIG_DIAL_INCREASE_COMMAND = "FR22_OUTER_RIGHT_KNOB +8000\n";
        private const string FR22_RIGHT_SMALL_DIAL_INCREASE_COMMAND = "FR22_INNER_RIGHT_KNOB +8000\n";
        private const string FR22_RIGHT_BIG_DIAL_DECREASE_COMMAND = "FR22_OUTER_RIGHT_KNOB -8000\n";
        private const string FR22_RIGHT_SMALL_DIAL_DECREASE_COMMAND = "FR22_INNER_RIGHT_KNOB -8000\n";


        /*AJS-37 FR24 COM2*/
        // Large dial Presets
        // Small dial Radio Volume

        /*AJS-37 TILS NAV1*/
        // Large dial TILS Selector 1-10 11-20 TILS_CHANNEL_SELECT
        // 1   2  3  4  5  6  7  8  9 10
        // 11 12 13 14 15 16 17 18 19 20
        // Small dial Master Mode Selector
        // ACT/STBY Toggles the ranges
        private volatile uint _tilsChannelCockpitValue;
        private volatile uint _tilsChannelLayerSelectorCockpitValue;
        private volatile uint _masterModeSelectorCockpitValue;
        private readonly object _lockTilsChannelSelectorDialObject1 = new object();
        private readonly object _lockTilsChannelLayerSelectorObject2 = new object();
        private readonly object _lockMasterModeSelectorObject = new object();
        private DCSBIOSOutput _tilsChannelSelectorDcsbiosOutput;
        private DCSBIOSOutput _tilsChannelLayerSelectorDcsbiosOutput;
        private DCSBIOSOutput _masterModeSelectorDcsbiosOutput;
        private const string TILS_CHANNEL_DIAL_COMMAND_INC = "TILS_CHANNEL_SELECT INC\n";
        private const string TILS_CHANNEL_DIAL_COMMAND_DEC = "TILS_CHANNEL_SELECT DEC\n";
        private const string TILS_CHANNEL_LAYER_DIAL_COMMAND_TOGGLE = "TILS_CHANNEL_LAYER TOGGLE\n";
        private const string MASTER_MODE_SELECTOR_COMMAND_INC = "MASTER_MODE_SELECT INC\n";
        private const string MASTER_MODE_SELECTOR_COMMAND_DEC = "MASTER_MODE_SELECT DEC\n";
        private int _tilsChannelDialSkipper;
        private int _masterModeSelectorDialSkipper;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69AJS37(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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
                Common.ShowErrorMessageBox( ex, "DCSBIOSStringReceived()");
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

                // TILS Channel Selector
                if (e.Address == _tilsChannelSelectorDcsbiosOutput.Address)
                {
                    lock (_lockTilsChannelSelectorDialObject1)
                    {
                        var tmp = _tilsChannelCockpitValue;
                        _tilsChannelCockpitValue = _tilsChannelSelectorDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _tilsChannelCockpitValue)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // TILS Channel Mode
                if (e.Address == _tilsChannelLayerSelectorDcsbiosOutput.Address)
                {
                    lock (_lockTilsChannelLayerSelectorObject2)
                    {
                        var tmp = _tilsChannelLayerSelectorCockpitValue;
                        _tilsChannelLayerSelectorCockpitValue = _tilsChannelLayerSelectorDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _tilsChannelLayerSelectorCockpitValue)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // Master Mode Selector
                if (e.Address == _masterModeSelectorDcsbiosOutput.Address)
                {
                    lock (_lockMasterModeSelectorObject)
                    {
                        var tmp = _masterModeSelectorCockpitValue;
                        _masterModeSelectorCockpitValue = _masterModeSelectorDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _masterModeSelectorCockpitValue)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAJS37 knob)
        {
            try
            {

                if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH))
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
                                        DCSBIOS.Send(TILS_CHANNEL_LAYER_DIAL_COMMAND_TOGGLE);
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
                                        DCSBIOS.Send(TILS_CHANNEL_LAYER_DIAL_COMMAND_TOGGLE);
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
                Common.ShowErrorMessageBox( ex);
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

                            case RadioPanelPZ69KnobsAJS37.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsAJS37.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsAJS37.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsAJS37.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsAJS37.LOWER_NO_USE3:
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
                                    // Ignore
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
                                        DCSBIOS.Send(TILS_CHANNEL_LAYER_DIAL_COMMAND_TOGGLE);
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
                                        DCSBIOS.Send(TILS_CHANNEL_LAYER_DIAL_COMMAND_TOGGLE);
                                    }
                                    else if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH);
                                    }

                                    break;
                                }
                        }
                        
                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(ProfileHandler.SelectedProfile().Description, HIDInstanceId, (int)PluginGamingPanelEnum.PZ69RadioPanel, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }

                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_BIG_DIAL_INCREASE_COMMAND : FR22_RIGHT_BIG_DIAL_INCREASE_COMMAND);
                                                break;
                                            }

                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }

                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTilsChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILS_CHANNEL_DIAL_COMMAND_INC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_BIG_DIAL_DECREASE_COMMAND : FR22_RIGHT_BIG_DIAL_DECREASE_COMMAND);
                                                break;
                                            }

                                        case CurrentAJS37RadioMode.FR24:
                                            {
                                                break;
                                            }

                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTilsChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILS_CHANNEL_DIAL_COMMAND_DEC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_SMALL_DIAL_INCREASE_COMMAND : FR22_RIGHT_SMALL_DIAL_INCREASE_COMMAND);
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
                                                    DCSBIOS.Send(MASTER_MODE_SELECTOR_COMMAND_INC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_SMALL_DIAL_DECREASE_COMMAND : FR22_RIGHT_SMALL_DIAL_DECREASE_COMMAND);
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
                                                    DCSBIOS.Send(MASTER_MODE_SELECTOR_COMMAND_DEC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_BIG_DIAL_INCREASE_COMMAND : FR22_RIGHT_BIG_DIAL_INCREASE_COMMAND);
                                                break;
                                            }

                                        case CurrentAJS37RadioMode.FR24:
                                            {

                                                break;
                                            }

                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTilsChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILS_CHANNEL_DIAL_COMMAND_INC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_BIG_DIAL_DECREASE_COMMAND : FR22_RIGHT_BIG_DIAL_DECREASE_COMMAND);
                                                break;
                                            }

                                        case CurrentAJS37RadioMode.FR24:
                                            {

                                                break;
                                            }

                                        case CurrentAJS37RadioMode.TILS:
                                            {
                                                if (!SkipTilsChannelDialChange())
                                                {
                                                    DCSBIOS.Send(TILS_CHANNEL_DIAL_COMMAND_DEC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_SMALL_DIAL_INCREASE_COMMAND : FR22_RIGHT_SMALL_DIAL_INCREASE_COMMAND);
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
                                                    DCSBIOS.Send(MASTER_MODE_SELECTOR_COMMAND_INC);
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
                                                DCSBIOS.Send(_fr22DialSideSelected == FR22DialSideSelected.Left ? FR22_LEFT_SMALL_DIAL_DECREASE_COMMAND : FR22_RIGHT_SMALL_DIAL_DECREASE_COMMAND);
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
                                                    DCSBIOS.Send(MASTER_MODE_SELECTOR_COMMAND_DEC);
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
                Common.ShowErrorMessageBox( ex);
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
                                lock (_lockTilsChannelSelectorDialObject1)
                                {
                                    channelSelector = _tilsChannelCockpitValue;
                                }

                                lock (_lockTilsChannelLayerSelectorObject2)
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
                                        channelSelector += 10;
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
                                lock (_lockTilsChannelSelectorDialObject1)
                                {
                                    channelSelector = _tilsChannelCockpitValue;
                                }

                                lock (_lockTilsChannelLayerSelectorObject2)
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
                                        channelSelector += 10;
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
                Common.ShowErrorMessageBox( ex);
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

                // COM2

                // NAV1
                _tilsChannelSelectorDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("TILS_CHANNEL_SELECT");
                _tilsChannelLayerSelectorDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("TILS_CHANNEL_LAYER");
                _masterModeSelectorDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("MASTER_MODE_SELECT");

                // NAV2

                // ADF

                // XPDR
                StartListeningForHidPanelChanges();

                // IsAttached = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        
        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55
            {
                DCSBiosOutputLED = dcsBiosOutput,
                LEDColor = panelLEDColor,
                SaitekLEDPosition = saitekPanelLEDPosition
            };
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobAJS37.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentAJS37RadioMode currentAJS37RadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentAJS37RadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentAJS37RadioMode currentAJS37RadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentAJS37RadioMode;

                // If NOUSE then send next round of e.Data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool SkipTilsChannelDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentAJS37RadioMode.TILS || _currentLowerRadioMode == CurrentAJS37RadioMode.TILS)
                {
                    if (_tilsChannelDialSkipper > 2)
                    {
                        _tilsChannelDialSkipper = 0;
                        return false;
                    }

                    _tilsChannelDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipMasterModeSelectorChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentAJS37RadioMode.TILS || _currentLowerRadioMode == CurrentAJS37RadioMode.TILS)
                {
                    if (_masterModeSelectorDialSkipper > 2)
                    {
                        _masterModeSelectorDialSkipper = 0;
                        return false;
                    }

                    _masterModeSelectorDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
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

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
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
