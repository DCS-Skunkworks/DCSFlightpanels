namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using MEF;
    using NonVisuals.EventArgs;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Saitek;

    public class RadioPanelPZ69M2000C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private CurrentM2000CRadioMode _currentUpperRadioMode = CurrentM2000CRadioMode.VUHF;
        private CurrentM2000CRadioMode _currentLowerRadioMode = CurrentM2000CRadioMode.VUHF;

        /*M-2000C VHF PRESETS COM1*/
        // Large dial PRESETS [step of 1]
        // Small dial Volume
        private readonly object _lockVUHFPresetFreqObject = new object();
        private readonly object _lockVUHFPresetDialObject = new object();
        private DCSBIOSOutput _vhfDcsbiosOutputPresetFreqString;
        private DCSBIOSOutput _vhfDcsbiosOutputPresetDial;
        private string _vhfPresetCockpitFrequency = string.Empty;
        private volatile uint _vhfPresetCockpitDialPos = 1;
        private const string VHFPresetCommandInc = "VHF_CH_SEL INC\n";
        private const string VHFPresetCommandDec = "VHF_CH_SEL DEC\n";
        private int _vhfPresetDialSkipper;
        private const string VHFVolumeCommandInc = "VUHF_RADIO_VOL_KNOB +3200\n";
        private const string VHFVolumeCommandDec = "VUHF_RADIO_VOL_KNOB -3200\n";


        /*M2000C UHF PRESETS COM2*/
        // Large dial PRESETS [step of 1]
        // Small dial Volume
        private readonly object _lockUHFPresetFreqObject = new object();
        private readonly object _lockUHFPresetDialObject = new object();
        private DCSBIOSOutput _uhfDcsbiosOutputPresetFreqString;
        private DCSBIOSOutput _uhfDcsbiosOutputPresetDial;
        private string _uhfPresetCockpitFrequency = string.Empty;
        private volatile uint _uhfPresetCockpitDialPos = 1;
        private const string UHF_PRESET_COMMAND_INC = "UHF_PRESET_KNOB INC\n";
        private const string UHF_PRESET_COMMAND_DEC = "UHF_PRESET_KNOB DEC\n";
        private int _uhfPresetDialSkipper;
        private const string UHF_VOLUME_COMMAND_INC = "UHF_RADIO_VOL_KNOB +3200\n";
        private const string UHF_VOLUME_COMMAND_DEC = "UHF_RADIO_VOL_KNOB -3200\n";

        // definePotentiometer("UHF_RADIO_VOL_KNOB", 16, 3706, 706, { 0, 1}, "AUDIO PANEL", "I - UHF - Radio Volume Knob")

        /*M-2000C TACAN NAV1*/
        private readonly object _lockTACANDialObject = new object();
        private DCSBIOSOutput _tacanDcsbiosOutputDialTens;
        private DCSBIOSOutput _tacanDcsbiosOutputDialOnes;
        private volatile uint _tacanTensCockpitDialPos = 1;
        private volatile uint _tacanOnesCockpitDialPos = 1;
        private const string TACAN_TENS_COMMAND_INC = "TAC_CH_10_SEL INC\n";
        private const string TACAN_TENS_COMMAND_DEC = "TAC_CH_10_SEL DEC\n";
        private const string TACAN_ONES_COMMAND_INC = "TAC_CH_1_SEL INC\n";
        private const string TACAN_ONES_COMMAND_DEC = "TAC_CH_1_SEL DEC\n";
        private int _tacanDialSkipper;
        private DCSBIOSOutput _tacanDcsbiosOutputDialModeSelect;
        private DCSBIOSOutput _tacanDcsbiosOutputDialXYSelect;
        private volatile uint _tacanModeSelectCockpitDialPos = 1;
        private volatile uint _tacanXYSelectCockpitDialPos = 1;
        private const string TACAN_MODE_SELECT_COMMAND_INC = "TAC_MODE_SEL INC\n";
        private const string TACAN_MODE_SELECT_COMMAND_DEC = "TAC_MODE_SEL DEC\n";
        private const string TACANXY_SELECT_COMMAND_INC = "TAC_X_Y_SEL INC\n";
        private const string TACANXY_SELECT_COMMAND_DEC = "TAC_X_Y_SEL DEC\n";

        /*M-2000C VOR.ILS NAV2*/
        private readonly object _lockVoRialObject = new object();
        private DCSBIOSOutput _vorDcsbiosOutputDialDecimals;
        private DCSBIOSOutput _vorDcsbiosOutputDialOnes;
        private volatile uint _vorDecimalsCockpitDialPos = 1;
        private volatile uint _vorOnesCockpitDialPos = 1;
        private const string VOR_DECIMALS_COMMAND_INC = "VORILS_FREQ_DECIMAL INC\n";
        private const string VOR_DECIMALS_COMMAND_DEC = "VORILS_FREQ_DECIMAL DEC\n";
        private const string VOR_ONES_COMMAND_INC = "VORILS_FREQ_WHOLE INC\n";
        private const string VOR_ONES_COMMAND_DEC = "VORILS_FREQ_WHOLE DEC\n";
        private int _vorDialSkipper;
        private DCSBIOSOutput _vorDcsbiosOutputDialPower;
        private DCSBIOSOutput _vorDcsbiosOutputDialTest;
        private volatile uint _vorPowerCockpitDialPos = 1;
        private volatile uint _vorTestCockpitDialPos = 1;
        private const string VOR_POWER_COMMAND_INC = "VORILS_PWR_DIAL INC\n";
        private const string VOR_POWER_COMMAND_DEC = "VORILS_PWR_DIAL DEC\n";
        private const string VOR_TEST_COMMAND_INC = "VORILS_TEST_DIAL INC\n";
        private const string VOR_TEST_COMMAND_DEC = "VORILS_TEST_DIAL DEC\n";
        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        private bool _upperFreqSwitchPressedDown;
        private bool _lowerFreqSwitchPressedDown;

        public RadioPanelPZ69M2000C(HIDSkeleton hidSkeleton, AppEventHandler appEventHandler) 
            : base(hidSkeleton, appEventHandler)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachStringListener(this);
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
                    BIOSEventHandler.DetachStringListener(this);
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
                // VHF Preset Channel Frequency
                if (e.Address == _vhfDcsbiosOutputPresetFreqString.Address && !string.IsNullOrEmpty(e.StringData) && double.TryParse(e.StringData, out var tmpValue))
                {
                    lock (_lockVUHFPresetFreqObject)
                    {
                        var tmp = _vhfPresetCockpitFrequency;
                        _vhfPresetCockpitFrequency = (double.Parse(e.StringData) / 100).ToString(NumberFormatInfoFullDisplay);
                        if (!string.IsNullOrEmpty(_vhfPresetCockpitFrequency) && tmp != _vhfPresetCockpitFrequency)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        }
                    }
                }

                // UHF Preset Channel Frequency
                if (e.Address == _uhfDcsbiosOutputPresetFreqString.Address && !string.IsNullOrEmpty(e.StringData) && double.TryParse(e.StringData, out var tmpValue2))
                {
                    lock (_lockUHFPresetFreqObject)
                    {
                        var tmp = _uhfPresetCockpitFrequency;
                        _uhfPresetCockpitFrequency = (double.Parse(e.StringData) / 100).ToString(NumberFormatInfoFullDisplay);
                        if (!string.IsNullOrEmpty(_uhfPresetCockpitFrequency) && tmp != _uhfPresetCockpitFrequency)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }

            // ShowFrequenciesOnPanel();
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

                // V/UHF Preset Channel Dial
                if (e.Address == _vhfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockVUHFPresetDialObject)
                    {
                        var tmp = _vhfPresetCockpitDialPos;
                        _vhfPresetCockpitDialPos = _vhfDcsbiosOutputPresetDial.GetUIntValue(e.Data) + 2;
                        if (_vhfPresetCockpitDialPos == 21)
                        {
                            _vhfPresetCockpitDialPos = 1; // something weird with this
                        }

                        if (tmp != _vhfPresetCockpitDialPos)
                        {
                            Debug.WriteLine(_vhfDcsbiosOutputPresetDial.GetUIntValue(e.Data));
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // UHF Preset Channel Dial
                if (e.Address == _uhfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockUHFPresetDialObject)
                    {
                        var tmp = _uhfPresetCockpitDialPos;
                        _uhfPresetCockpitDialPos = _uhfDcsbiosOutputPresetDial.GetUIntValue(e.Data) + 1;
                        if (tmp != _uhfPresetCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // TACAN Tens
                if (e.Address == _tacanDcsbiosOutputDialTens.Address)
                {
                    lock (_lockTACANDialObject)
                    {
                        var tmp = _tacanTensCockpitDialPos;
                        _tacanTensCockpitDialPos = _tacanDcsbiosOutputDialTens.GetUIntValue(e.Data);
                        if (tmp != _tacanTensCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // TACAN Ones
                if (e.Address == _tacanDcsbiosOutputDialOnes.Address)
                {
                    lock (_lockTACANDialObject)
                    {
                        var tmp = _tacanOnesCockpitDialPos;
                        _tacanOnesCockpitDialPos = _tacanDcsbiosOutputDialOnes.GetUIntValue(e.Data);
                        if (tmp != _tacanOnesCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // TACAN Mode Select
                if (e.Address == _tacanDcsbiosOutputDialModeSelect.Address)
                {
                    lock (_lockTACANDialObject)
                    {
                        var tmp = _tacanModeSelectCockpitDialPos;
                        _tacanModeSelectCockpitDialPos = _tacanDcsbiosOutputDialModeSelect.GetUIntValue(e.Data);
                        if (tmp != _tacanModeSelectCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // TACAN XY Select
                if (e.Address == _tacanDcsbiosOutputDialXYSelect.Address)
                {
                    lock (_lockTACANDialObject)
                    {
                        var tmp = _tacanXYSelectCockpitDialPos;
                        _tacanXYSelectCockpitDialPos = _tacanDcsbiosOutputDialXYSelect.GetUIntValue(e.Data);
                        if (tmp != _tacanXYSelectCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // VOR Tens
                if (e.Address == _vorDcsbiosOutputDialDecimals.Address)
                {
                    lock (_lockVoRialObject)
                    {
                        var tmp = _vorDecimalsCockpitDialPos;
                        _vorDecimalsCockpitDialPos = _vorDcsbiosOutputDialDecimals.GetUIntValue(e.Data);
                        if (tmp != _vorDecimalsCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // VOR Ones
                if (e.Address == _vorDcsbiosOutputDialOnes.Address)
                {
                    lock (_lockVoRialObject)
                    {
                        var tmp = _vorOnesCockpitDialPos;
                        _vorOnesCockpitDialPos = _vorDcsbiosOutputDialOnes.GetUIntValue(e.Data);
                        if (tmp != _vorOnesCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // VOR Power
                if (e.Address == _vorDcsbiosOutputDialPower.Address)
                {
                    lock (_lockVoRialObject)
                    {
                        var tmp = _vorPowerCockpitDialPos;
                        _vorPowerCockpitDialPos = _vorDcsbiosOutputDialPower.GetUIntValue(e.Data);
                        if (tmp != _vorPowerCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // VOR Test
                if (e.Address == _vorDcsbiosOutputDialTest.Address)
                {
                    lock (_lockVoRialObject)
                    {
                        var tmp = _vorTestCockpitDialPos;
                        _vorTestCockpitDialPos = _vorDcsbiosOutputDialTest.GetUIntValue(e.Data);
                        if (tmp != _vorTestCockpitDialPos)
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        /*
        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsM2000C knob)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SendFrequencyToDCSBIOS()");
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }
                switch (knob)
                {
                    case RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentM2000CRadioMode.VUHF:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.UHF:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.TACAN:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.VOR:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.NOUSE:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentM2000CRadioMode.VUHF:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.UHF:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.TACAN:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.VOR:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.NOUSE:
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
            Common.DebugP("Leaving M2000C Radio SendFrequencyToDCSBIOS()");
        }

    */
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
                        var radioPanelKnob = (RadioPanelKnobM2000C)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsM2000C.UPPER_VUHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.VUHF);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.UHF);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_TACAN:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.TACAN);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_VOR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.VOR);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.NOUSE);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_VUHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.VUHF);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.UHF);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_TACAN:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.TACAN);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_VOR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.VOR);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.NOUSE);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH:
                                {
                                    _upperFreqSwitchPressedDown = radioPanelKnob.IsOn;
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH:
                                {
                                    _lowerFreqSwitchPressedDown = radioPanelKnob.IsOn;
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSFPProfile.SelectedProfile.Description, HIDInstance, (int)PluginGamingPanelEnum.PZ69RadioPanel, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }

                    AdjustFrequency(hashSet);
                    ShowFrequenciesOnPanel();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                    var radioPanelKnobM2000C = (RadioPanelKnobM2000C)o;
                    if (radioPanelKnobM2000C.IsOn)
                    {
                        switch (radioPanelKnobM2000C.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                            {
                                                if (!SkipVUHFPresetDialChange())
                                                {

                                                    DCSBIOS.Send(VHFPresetCommandInc);

                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_INC : TACAN_TENS_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? VOR_POWER_COMMAND_INC : VOR_DECIMALS_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                            {
                                                if (!SkipVUHFPresetDialChange())
                                                {

                                                    DCSBIOS.Send(VHFPresetCommandDec);

                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_DEC : TACAN_TENS_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? VOR_POWER_COMMAND_DEC : VOR_DECIMALS_COMMAND_DEC);
                                                }

                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                            {

                                                DCSBIOS.Send(VHFVolumeCommandInc);

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                DCSBIOS.Send(UHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_INC : TACAN_ONES_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? VOR_TEST_COMMAND_INC : VOR_ONES_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                DCSBIOS.Send(UHF_VOLUME_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VUHF:
                                            {

                                                DCSBIOS.Send(VHFVolumeCommandDec);

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_DEC : TACAN_ONES_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_upperFreqSwitchPressedDown ? VOR_TEST_COMMAND_DEC : VOR_ONES_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                            {
                                                if (!SkipVUHFPresetDialChange())
                                                {

                                                    DCSBIOS.Send(VHFPresetCommandInc);

                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_INC : TACAN_TENS_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? VOR_POWER_COMMAND_INC : VOR_DECIMALS_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                            {
                                                if (!SkipVUHFPresetDialChange())
                                                {

                                                    DCSBIOS.Send(VHFPresetCommandDec);

                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_DEC : TACAN_TENS_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? VOR_POWER_COMMAND_DEC : VOR_DECIMALS_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                DCSBIOS.Send(UHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VUHF:
                                            {
                                                DCSBIOS.Send(VHFVolumeCommandInc);

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_INC : TACAN_ONES_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? VOR_TEST_COMMAND_INC : VOR_ONES_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                DCSBIOS.Send(UHF_VOLUME_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VUHF:
                                            {

                                                DCSBIOS.Send(VHFVolumeCommandDec);

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                if (!SkipTACANDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_DEC : TACAN_ONES_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                if (!SkipVORDialChange())
                                                {
                                                    DCSBIOS.Send(_lowerFreqSwitchPressedDown ? VOR_TEST_COMMAND_DEC : VOR_ONES_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NOUSE:
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
                        case CurrentM2000CRadioMode.VUHF:
                            {
                                var channelAsString = string.Empty;
                                lock (_lockVUHFPresetDialObject)
                                {
                                    channelAsString = (_vhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);

                                lock (_vhfPresetCockpitFrequency)
                                {
                                    if (!string.IsNullOrEmpty(_vhfPresetCockpitFrequency))
                                    {
                                        SetPZ69DisplayBytes(ref bytes, double.Parse(_vhfPresetCockpitFrequency, NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    }
                                    else
                                    {
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    }
                                }

                                break;
                            }

                        case CurrentM2000CRadioMode.UHF:
                            {
                                var channelAsString = string.Empty;
                                lock (_lockUHFPresetDialObject)
                                {
                                    channelAsString = (_uhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);

                                lock (_uhfPresetCockpitFrequency)
                                {
                                    if (!string.IsNullOrEmpty(_uhfPresetCockpitFrequency))
                                    {
                                        SetPZ69DisplayBytes(ref bytes, double.Parse(_uhfPresetCockpitFrequency, NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    }
                                    else
                                    {
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    }
                                }

                                break;
                            }

                        case CurrentM2000CRadioMode.TACAN:
                            {
                                var channelAsString = string.Empty;
                                var modeAsString = string.Empty;
                                lock (_lockTACANDialObject)
                                {
                                    channelAsString = (_tacanXYSelectCockpitDialPos).ToString().PadRight(3, ' ') + (_tacanTensCockpitDialPos) + (_tacanOnesCockpitDialPos);
                                    modeAsString = _tacanModeSelectCockpitDialPos.ToString().PadLeft(5, ' ');
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, channelAsString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, modeAsString, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }

                        case CurrentM2000CRadioMode.VOR:
                            {
                                var channelAsString = string.Empty;
                                var settingsAsString = string.Empty;
                                lock (_lockTACANDialObject)
                                {
                                    var ones = _vorOnesCockpitDialPos + 8;
                                    var decimals = _vorDecimalsCockpitDialPos * 5;
                                    channelAsString = "1" + ones.ToString().PadLeft(2, '0') + "." + decimals.ToString().PadLeft(2, '0');
                                    settingsAsString = _vorPowerCockpitDialPos + "   " + _vorTestCockpitDialPos;
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, channelAsString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, settingsAsString, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }

                        case CurrentM2000CRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentM2000CRadioMode.VUHF:
                            {
                                var channelAsString = string.Empty;
                                lock (_lockVUHFPresetDialObject)
                                {
                                    channelAsString = (_vhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);

                                lock (_vhfPresetCockpitFrequency)
                                {
                                    if (!string.IsNullOrEmpty(_vhfPresetCockpitFrequency))
                                    {
                                        SetPZ69DisplayBytes(ref bytes, double.Parse(_vhfPresetCockpitFrequency, NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    }
                                    else
                                    {
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    }
                                }

                                break;
                            }

                        case CurrentM2000CRadioMode.UHF:
                            {
                                var channelAsString = string.Empty;
                                lock (_lockUHFPresetDialObject)
                                {
                                    channelAsString = (_uhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);

                                lock (_uhfPresetCockpitFrequency)
                                {
                                    if (!string.IsNullOrEmpty(_uhfPresetCockpitFrequency))
                                    {
                                        SetPZ69DisplayBytes(ref bytes, double.Parse(_uhfPresetCockpitFrequency, NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    }
                                    else
                                    {
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    }
                                }

                                break;
                            }

                        case CurrentM2000CRadioMode.TACAN:
                            {
                                var channelAsString = string.Empty;
                                var modeAsString = string.Empty;
                                lock (_lockTACANDialObject)
                                {
                                    channelAsString = (_tacanXYSelectCockpitDialPos).ToString().PadRight(3, ' ') + (_tacanTensCockpitDialPos) + (_tacanOnesCockpitDialPos);
                                    modeAsString = _tacanModeSelectCockpitDialPos.ToString().PadLeft(5, ' ');
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, channelAsString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, modeAsString, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }

                        case CurrentM2000CRadioMode.VOR:
                            {
                                var channelAsString = string.Empty;
                                var settingsAsString = string.Empty;
                                lock (_lockTACANDialObject)
                                {
                                    var ones = _vorOnesCockpitDialPos + 8;
                                    var decimals = _vorDecimalsCockpitDialPos * 5;
                                    channelAsString = "1" + ones.ToString().PadLeft(2, '0') + "." + decimals.ToString().PadLeft(2, '0');
                                    settingsAsString = _vorPowerCockpitDialPos + "   " + _vorTestCockpitDialPos;
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, channelAsString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, settingsAsString, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }

                        case CurrentM2000CRadioMode.NOUSE:
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
                Common.ShowErrorMessageBox(ex);
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
                _vhfDcsbiosOutputPresetFreqString = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_FREQUENCY");
                DCSBIOSStringManager.AddListeningAddress(_vhfDcsbiosOutputPresetFreqString);
                _vhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_CH_SEL");

                // COM2
                _uhfDcsbiosOutputPresetFreqString = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FREQUENCY");
                DCSBIOSStringManager.AddListeningAddress(_uhfDcsbiosOutputPresetFreqString);
                _uhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_KNOB");

                // NAV1
                _tacanDcsbiosOutputDialTens = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_CH_10_SEL");
                _tacanDcsbiosOutputDialOnes = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_CH_1_SEL");
                _tacanDcsbiosOutputDialModeSelect = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_MODE_SEL");
                _tacanDcsbiosOutputDialXYSelect = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_X_Y_SEL");

                // NAV2
                _vorDcsbiosOutputDialDecimals = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_FREQ_DECIMAL");
                _vorDcsbiosOutputDialOnes = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_FREQ_WHOLE");
                _vorDcsbiosOutputDialPower = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_PWR_DIAL");
                _vorDcsbiosOutputDialTest = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_TEST_DIAL");

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
            SaitekPanelKnobs = RadioPanelKnobM2000C.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentM2000CRadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentM2000CRadioMode;

                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }


        private bool SkipVUHFPresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.VUHF || _currentLowerRadioMode == CurrentM2000CRadioMode.VUHF)
                {
                    if (_vhfPresetDialSkipper > 2)
                    {
                        _vhfPresetDialSkipper = 0;
                        return false;
                    }

                    _vhfPresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipUHFPresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.UHF || _currentLowerRadioMode == CurrentM2000CRadioMode.UHF)
                {
                    if (_uhfPresetDialSkipper > 2)
                    {
                        _uhfPresetDialSkipper = 0;
                        return false;
                    }

                    _uhfPresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipTACANDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.TACAN || _currentLowerRadioMode == CurrentM2000CRadioMode.TACAN)
                {
                    if (_tacanDialSkipper > 2)
                    {
                        _tacanDialSkipper = 0;
                        return false;
                    }

                    _tacanDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipVORDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.VOR || _currentLowerRadioMode == CurrentM2000CRadioMode.VOR)
                {
                    if (_vorDialSkipper > 2)
                    {
                        _vorDialSkipper = 0;
                        return false;
                    }

                    _vorDialSkipper++;
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
