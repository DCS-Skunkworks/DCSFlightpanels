using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;


namespace NonVisuals.Radios
{
    public class RadioPanelPZ69M2000C : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private CurrentM2000CRadioMode _currentUpperRadioMode = CurrentM2000CRadioMode.VUHF;
        private CurrentM2000CRadioMode _currentLowerRadioMode = CurrentM2000CRadioMode.VUHF;

        /*M-2000C V/UHF PRESETS COM1*/
        //Large dial PRESETS [step of 1]
        //Small dial Volume
        private readonly object _lockVUHFPresetDialObject = new object();
        private DCSBIOSOutput _vuhfDcsbiosOutputPresetDial;
        private volatile uint _vuhfPresetCockpitDialPos = 1;
        private const string VUHF_PRESET_COMMAND_INC = "UVHF_PRESET_KNOB INC\n";
        private const string VUHF_PRESET_COMMAND_DEC = "UVHF_PRESET_KNOB DEC\n";
        private int _vuhfPresetDialSkipper;
        private const string VUHF_VOLUME_COMMAND_INC = "VUHF_RADIO_VOL_KNOB +3200\n";
        private const string VUHF_VOLUME_COMMAND_DEC = "VUHF_RADIO_VOL_KNOB -3200\n";

        /*M2000C UHF PRESETS COM2*/
        //Large dial PRESETS [step of 1]
        //Small dial Volume
        private readonly object _lockUHFPresetDialObject = new object();
        private DCSBIOSOutput _uhfDcsbiosOutputPresetDial;
        private volatile uint _uhfPresetCockpitDialPos = 1;
        private const string UHF_PRESET_COMMAND_INC = "UHF_PRESET_KNOB INC\n";
        private const string UHF_PRESET_COMMAND_DEC = "UHF_PRESET_KNOB DEC\n";
        private int _uhfPresetDialSkipper;
        private const string UHF_VOLUME_COMMAND_INC = "UHF_RADIO_VOL_KNOB +3200\n";
        private const string UHF_VOLUME_COMMAND_DEC = "UHF_RADIO_VOL_KNOB -3200\n";

        //definePotentiometer("UHF_RADIO_VOL_KNOB", 16, 3706, 706, { 0, 1}, "AUDIO PANEL", "I - UHF - Radio Volume Knob")

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

        private bool _upperFreqSwitchPressedDown = false;
        private bool _lowerFreqSwitchPressedDown = false;

        public RadioPanelPZ69M2000C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                }*/
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
            }
            //ShowFrequenciesOnPanel();
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
                if (e.Address == _vuhfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockVUHFPresetDialObject)
                    {
                        var tmp = _vuhfPresetCockpitDialPos;
                        _vuhfPresetCockpitDialPos = _vuhfDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        _vuhfPresetCockpitDialPos++;
                        if (tmp != _vuhfPresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                // UHF Preset Channel Dial
                if (e.Address == _uhfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockUHFPresetDialObject)
                    {
                        var tmp = _uhfPresetCockpitDialPos;
                        _uhfPresetCockpitDialPos = _uhfDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        _uhfPresetCockpitDialPos++;
                        if (tmp != _uhfPresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                // VOR Ones
                if (e.Address == _vorDcsbiosOutputDialOnes.Address)
                {
                    lock (_lockVoRialObject)
                    {
                        var tmp = _vorOnesCockpitDialPos;
                        _vorOnesCockpitDialPos = _vorDcsbiosOutputDialOnes.GetUIntValue(e.Data) - 1;
                        if (tmp != _vorOnesCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                Common.LogError(78002, ex);
            }
            Common.DebugP("Leaving M2000C Radio SendFrequencyToDCSBIOS()");
        }

    */
        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                                    //Ignore
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
                    }
                    AdjustFrequency(hashSet);
                    ShowFrequenciesOnPanel();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78006, ex);
            }
            Common.DebugP("Leaving M2000C Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio AdjustFrequency()");

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
                                                    DCSBIOS.Send(VUHF_PRESET_COMMAND_INC);
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
                                                    DCSBIOS.Send(VUHF_PRESET_COMMAND_DEC);
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
                                                DCSBIOS.Send(VUHF_VOLUME_COMMAND_INC);
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
                                                DCSBIOS.Send(VUHF_VOLUME_COMMAND_DEC);
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
                                                    DCSBIOS.Send(VUHF_PRESET_COMMAND_INC);
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
                                                    DCSBIOS.Send(VUHF_PRESET_COMMAND_DEC);
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
                                                DCSBIOS.Send(VUHF_VOLUME_COMMAND_INC);
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
                                                DCSBIOS.Send(VUHF_VOLUME_COMMAND_DEC);
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
                Common.LogError(78007, ex);
            }
            Common.DebugP("Leaving M2000C Radio AdjustFrequency()");
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

                    Common.DebugP("Entering M2000C Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentM2000CRadioMode.VUHF:
                            {
                                var channelAsString = "";
                                lock (_lockVUHFPresetDialObject)
                                {
                                    channelAsString = (_vuhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.UHF:
                            {
                                var channelAsString = "";
                                lock (_lockUHFPresetDialObject)
                                {
                                    channelAsString = (_uhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.TACAN:
                            {
                                var channelAsString = "";
                                var modeAsString = "";
                                lock (_lockTACANDialObject)
                                {
                                    channelAsString = (_tacanXYSelectCockpitDialPos).ToString().PadRight(3, ' ') + (_tacanTensCockpitDialPos).ToString() + (_tacanOnesCockpitDialPos).ToString();
                                    modeAsString = _tacanModeSelectCockpitDialPos.ToString().PadLeft(5, ' ');
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, channelAsString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, modeAsString, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.VOR:
                            {
                                var channelAsString = "";
                                var settingsAsString = "";
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
                                var channelAsString = "";
                                lock (_lockVUHFPresetDialObject)
                                {
                                    channelAsString = (_vuhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.UHF:
                            {
                                var channelAsString = "";
                                lock (_lockUHFPresetDialObject)
                                {
                                    channelAsString = (_uhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.TACAN:
                            {
                                var channelAsString = "";
                                var modeAsString = "";
                                lock (_lockTACANDialObject)
                                {
                                    channelAsString = (_tacanXYSelectCockpitDialPos).ToString().PadRight(3, ' ') + (_tacanTensCockpitDialPos).ToString() + (_tacanOnesCockpitDialPos).ToString();
                                    modeAsString = _tacanModeSelectCockpitDialPos.ToString().PadLeft(5, ' ');
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, channelAsString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, modeAsString, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.VOR:
                            {
                                var channelAsString = "";
                                var settingsAsString = "";
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
                Common.LogError(78011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving M2000C Radio ShowFrequenciesOnPanel()");
        }


        protected override void GamingPanelKnobChanged(IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(hashSet);
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("M2000C");

                //COM1
                _vuhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("UVHF_PRESET_KNOB");

                //COM2
                _uhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_KNOB");

                //NAV1
                _tacanDcsbiosOutputDialTens = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_CH_10_SEL");
                _tacanDcsbiosOutputDialOnes = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_CH_1_SEL");
                _tacanDcsbiosOutputDialModeSelect = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_MODE_SEL");
                _tacanDcsbiosOutputDialXYSelect = DCSBIOSControlLocator.GetDCSBIOSOutput("TAC_X_Y_SEL");

                //NAV2
                _vorDcsbiosOutputDialDecimals = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_FREQ_DECIMAL");
                _vorDcsbiosOutputDialOnes = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_FREQ_WHOLE");
                _vorDcsbiosOutputDialPower = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_PWR_DIAL");
                _vorDcsbiosOutputDialTest = DCSBIOSControlLocator.GetDCSBIOSOutput("VORILS_TEST_DIAL");

                StartListeningForPanelChanges();
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69M2000C.StartUp() : " + ex.Message);
                Common.LogError(321654, ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering M2000C Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving M2000C Radio Shutdown()");
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
            SaitekPanelKnobs = RadioPanelKnobM2000C.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentM2000CRadioMode);
                _currentUpperRadioMode = currentM2000CRadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(78014, ex);
            }
            Common.DebugP("Leaving M2000C Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentM2000CRadioMode);
                _currentLowerRadioMode = currentM2000CRadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            Common.DebugP("Leaving M2000C Radio SetLowerRadioMode()");
        }


        private bool SkipVUHFPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SkipVUHFPresetDialChange()");
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.VUHF || _currentLowerRadioMode == CurrentM2000CRadioMode.VUHF)
                {
                    if (_vuhfPresetDialSkipper > 2)
                    {
                        _vuhfPresetDialSkipper = 0;
                        Common.DebugP("Leaving M2000C Radio SkipVUHFPresetDialChange()");
                        return false;
                    }
                    _vuhfPresetDialSkipper++;
                    Common.DebugP("Leaving M2000C Radio SkipVUHFPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving M2000C Radio SkipVUHFPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        private bool SkipUHFPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SkipUHFPresetDialChange()");
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.UHF || _currentLowerRadioMode == CurrentM2000CRadioMode.UHF)
                {
                    if (_uhfPresetDialSkipper > 2)
                    {
                        _uhfPresetDialSkipper = 0;
                        Common.DebugP("Leaving M2000C Radio SkipUHFPresetDialChange()");
                        return false;
                    }
                    _uhfPresetDialSkipper++;
                    Common.DebugP("Leaving M2000C Radio SkipUHFPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving M2000C Radio SkipUHFPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
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
                Common.LogError(78009, ex);
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
