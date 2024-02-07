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
    using DCS_BIOS.Serialized;
    using DCS_BIOS.ControlLocator;


    /// <summary>
    /// Pre-programmed radio panel for the M2000C. 
    /// </summary>
    public class RadioPanelPZ69M2000C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentM2000CRadioMode
        {
            VUHF,
            UHF,
            TACAN,
            VOR,
            NO_USE
        }

        private CurrentM2000CRadioMode _currentUpperRadioMode = CurrentM2000CRadioMode.VUHF;
        private CurrentM2000CRadioMode _currentLowerRadioMode = CurrentM2000CRadioMode.VUHF;

        /*M-2000C VHF PRESETS COM1*/
        // Large dial PRESETS [step of 1]
        // Small dial Volume
        private readonly object _lockVUHFPresetFreqObject = new();
        private readonly object _lockVUHFPresetDialObject = new();
        private DCSBIOSOutput _vhfDcsbiosOutputPresetFreqString;
        private DCSBIOSOutput _vhfDcsbiosOutputPresetDial;
        private string _vhfPresetCockpitFrequency = string.Empty;
        private volatile uint _vhfPresetCockpitDialPos = 1;
        private const string VHF_PRESET_COMMAND_INC = "VHF_CH_SEL INC\n";
        private const string VHF_PRESET_COMMAND_DEC = "VHF_CH_SEL DEC\n";
        private readonly ClickSkipper _vhfPresetDialSkipper = new(2);
        private const string VHF_VOLUME_COMMAND_INC = "VUHF_RADIO_VOL_KNOB +3200\n";
        private const string VHF_VOLUME_COMMAND_DEC = "VUHF_RADIO_VOL_KNOB -3200\n";

        /*M2000C UHF PRESETS COM2*/
        // Large dial PRESETS [step of 1]
        // Small dial Volume
        private readonly object _lockUHFPresetFreqObject = new();
        private readonly object _lockUHFPresetDialObject = new();
        private DCSBIOSOutput _uhfDcsbiosOutputPresetFreqString;
        private DCSBIOSOutput _uhfDcsbiosOutputPresetDial;
        private string _uhfPresetCockpitFrequency = string.Empty;
        private volatile uint _uhfPresetCockpitDialPos = 1;
        private const string UHF_PRESET_COMMAND_INC = "UHF_PRESET_KNOB INC\n";
        private const string UHF_PRESET_COMMAND_DEC = "UHF_PRESET_KNOB DEC\n";
        private readonly ClickSkipper _uhfPresetDialSkipper = new(2);
        private const string UHF_VOLUME_COMMAND_INC = "UHF_RADIO_VOL_KNOB +3200\n";
        private const string UHF_VOLUME_COMMAND_DEC = "UHF_RADIO_VOL_KNOB -3200\n";

        // definePotentiometer("UHF_RADIO_VOL_KNOB", 16, 3706, 706, { 0, 1}, "AUDIO PANEL", "I - UHF - Radio Volume Knob")

        /*M-2000C TACAN NAV1*/
        private readonly object _lockTACANDialObject = new();
        private DCSBIOSOutput _tacanDcsbiosOutputDialTens;
        private DCSBIOSOutput _tacanDcsbiosOutputDialOnes;
        private volatile uint _tacanTensCockpitDialPos = 1;
        private volatile uint _tacanOnesCockpitDialPos = 1;
        private const string TACAN_TENS_COMMAND_INC = "TAC_CH_10_SEL INC\n";
        private const string TACAN_TENS_COMMAND_DEC = "TAC_CH_10_SEL DEC\n";
        private const string TACAN_ONES_COMMAND_INC = "TAC_CH_1_SEL INC\n";
        private const string TACAN_ONES_COMMAND_DEC = "TAC_CH_1_SEL DEC\n";
        private readonly ClickSkipper _tacanDialSkipper = new(2);
        private DCSBIOSOutput _tacanDcsbiosOutputDialModeSelect;
        private DCSBIOSOutput _tacanDcsbiosOutputDialXYSelect;
        private volatile uint _tacanModeSelectCockpitDialPos = 1;
        private volatile uint _tacanXYSelectCockpitDialPos = 1;
        private const string TACAN_MODE_SELECT_COMMAND_INC = "TAC_MODE_SEL INC\n";
        private const string TACAN_MODE_SELECT_COMMAND_DEC = "TAC_MODE_SEL DEC\n";
        private const string TACANXY_SELECT_COMMAND_INC = "TAC_X_Y_SEL INC\n";
        private const string TACANXY_SELECT_COMMAND_DEC = "TAC_X_Y_SEL DEC\n";

        /*M-2000C VOR.ILS NAV2*/
        private readonly object _lockVoRialObject = new();
        private DCSBIOSOutput _vorDcsbiosOutputDialDecimals;
        private DCSBIOSOutput _vorDcsbiosOutputDialOnes;
        private volatile uint _vorDecimalsCockpitDialPos = 1;
        private volatile uint _vorOnesCockpitDialPos = 1;
        private const string VOR_DECIMALS_COMMAND_INC = "VORILS_FREQ_DECIMAL INC\n";
        private const string VOR_DECIMALS_COMMAND_DEC = "VORILS_FREQ_DECIMAL DEC\n";
        private const string VOR_ONES_COMMAND_INC = "VORILS_FREQ_WHOLE INC\n";
        private const string VOR_ONES_COMMAND_DEC = "VORILS_FREQ_WHOLE DEC\n";
        private readonly ClickSkipper _vorDialSkipper = new(2);
        private DCSBIOSOutput _vorDcsbiosOutputDialPower;
        private DCSBIOSOutput _vorDcsbiosOutputDialTest;
        private volatile uint _vorPowerCockpitDialPos = 1;
        private volatile uint _vorTestCockpitDialPos = 1;
        private const string VOR_POWER_COMMAND_INC = "VORILS_PWR_DIAL INC\n";
        private const string VOR_POWER_COMMAND_DEC = "VORILS_PWR_DIAL DEC\n";
        private const string VOR_TEST_COMMAND_INC = "VORILS_TEST_DIAL INC\n";
        private const string VOR_TEST_COMMAND_DEC = "VORILS_TEST_DIAL DEC\n";
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        private bool _upperFreqSwitchPressedDown;
        private bool _lowerFreqSwitchPressedDown;

        public RadioPanelPZ69M2000C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                    BIOSEventHandler.DetachStringListener(this);
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
            _vhfDcsbiosOutputPresetFreqString = DCSBIOSControlLocator.GetStringDCSBIOSOutput("VHF_FREQUENCY");
            _vhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VHF_CH_SEL");

            // COM2
            _uhfDcsbiosOutputPresetFreqString = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UHF_FREQUENCY");
            _uhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("UHF_PRESET_KNOB");

            // NAV1
            _tacanDcsbiosOutputDialTens = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("TAC_CH_10_SEL");
            _tacanDcsbiosOutputDialOnes = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("TAC_CH_1_SEL");
            _tacanDcsbiosOutputDialModeSelect = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("TAC_MODE_SEL");
            _tacanDcsbiosOutputDialXYSelect = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("TAC_X_Y_SEL");

            // NAV2
            _vorDcsbiosOutputDialDecimals = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VORILS_FREQ_DECIMAL");
            _vorDcsbiosOutputDialOnes = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VORILS_FREQ_WHOLE");
            _vorDcsbiosOutputDialPower = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VORILS_PWR_DIAL");
            _vorDcsbiosOutputDialTest = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("VORILS_TEST_DIAL");

            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                // VHF Preset Channel Frequency
                if (e.Address == _vhfDcsbiosOutputPresetFreqString.Address && !string.IsNullOrEmpty(e.StringData) && double.TryParse(e.StringData, out _))
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
                if (e.Address == _uhfDcsbiosOutputPresetFreqString.Address && !string.IsNullOrEmpty(e.StringData) && double.TryParse(e.StringData, out _))
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
                if (_vhfDcsbiosOutputPresetDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVUHFPresetDialObject)
                    {
                        _vhfPresetCockpitDialPos = _vhfDcsbiosOutputPresetDial.LastUIntValue + 2;
                        if (_vhfPresetCockpitDialPos == 21)
                        {
                            _vhfPresetCockpitDialPos = 1; // something weird with this
                        }

                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // UHF Preset Channel Dial
                if (_uhfDcsbiosOutputPresetDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockUHFPresetDialObject)
                    {
                        _uhfPresetCockpitDialPos = _uhfDcsbiosOutputPresetDial.LastUIntValue + 1;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // TACAN Tens
                if (_tacanDcsbiosOutputDialTens.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockTACANDialObject)
                    {
                        _tacanTensCockpitDialPos = _tacanDcsbiosOutputDialTens.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // TACAN Ones
                if (_tacanDcsbiosOutputDialOnes.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockTACANDialObject)
                    {
                        _tacanOnesCockpitDialPos = _tacanDcsbiosOutputDialOnes.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // TACAN Mode Select
                if (_tacanDcsbiosOutputDialModeSelect.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockTACANDialObject)
                    {
                        _tacanModeSelectCockpitDialPos = _tacanDcsbiosOutputDialModeSelect.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // TACAN XY Select
                if (_tacanDcsbiosOutputDialXYSelect.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockTACANDialObject)
                    {
                        _tacanXYSelectCockpitDialPos = _tacanDcsbiosOutputDialXYSelect.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VOR Tens
                if (_vorDcsbiosOutputDialDecimals.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVoRialObject)
                    {
                        _vorDecimalsCockpitDialPos = _vorDcsbiosOutputDialDecimals.LastUIntValue; ;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VOR Ones
                if (_vorDcsbiosOutputDialOnes.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVoRialObject)
                    {
                        _vorOnesCockpitDialPos = _vorDcsbiosOutputDialOnes.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VOR Power
                if (_vorDcsbiosOutputDialPower.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVoRialObject)
                    {
                        _vorPowerCockpitDialPos = _vorDcsbiosOutputDialPower.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // VOR Test
                if (_vorDcsbiosOutputDialTest.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVoRialObject)
                    {
                        _vorTestCockpitDialPos = _vorDcsbiosOutputDialTest.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
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

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
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
                                        SetUpperRadioMode(CurrentM2000CRadioMode.NO_USE);
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
                                        SetLowerRadioMode(CurrentM2000CRadioMode.NO_USE);
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
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_M2000C, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
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
                                                _vhfPresetDialSkipper.Click(VHF_PRESET_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                _uhfPresetDialSkipper.Click(UHF_PRESET_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_upperFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_INC : TACAN_TENS_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_upperFreqSwitchPressedDown ? VOR_POWER_COMMAND_INC : VOR_DECIMALS_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                                _vhfPresetDialSkipper.Click(VHF_PRESET_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                _uhfPresetDialSkipper.Click(UHF_PRESET_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_upperFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_DEC : TACAN_TENS_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_upperFreqSwitchPressedDown ? VOR_POWER_COMMAND_DEC : VOR_DECIMALS_COMMAND_DEC);
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
                                                DCSBIOS.Send(VHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                DCSBIOS.Send(UHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_upperFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_INC : TACAN_ONES_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_upperFreqSwitchPressedDown ? VOR_TEST_COMMAND_INC : VOR_ONES_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                                DCSBIOS.Send(VHF_VOLUME_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_upperFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_DEC : TACAN_ONES_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_upperFreqSwitchPressedDown ? VOR_TEST_COMMAND_DEC : VOR_ONES_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                                _vhfPresetDialSkipper.Click(VHF_PRESET_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                _uhfPresetDialSkipper.Click(UHF_PRESET_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_lowerFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_INC : TACAN_TENS_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_lowerFreqSwitchPressedDown ? VOR_POWER_COMMAND_INC : VOR_DECIMALS_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                                _vhfPresetDialSkipper.Click(VHF_PRESET_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                _uhfPresetDialSkipper.Click(UHF_PRESET_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_lowerFreqSwitchPressedDown ? TACANXY_SELECT_COMMAND_DEC : TACAN_TENS_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_lowerFreqSwitchPressedDown ? VOR_POWER_COMMAND_DEC : VOR_DECIMALS_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                                DCSBIOS.Send(VHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_lowerFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_INC : TACAN_ONES_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_lowerFreqSwitchPressedDown ? VOR_TEST_COMMAND_INC : VOR_ONES_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                                DCSBIOS.Send(VHF_VOLUME_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.TACAN:
                                            {
                                                _tacanDialSkipper.Click(_lowerFreqSwitchPressedDown ? TACAN_MODE_SELECT_COMMAND_DEC : TACAN_ONES_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.VOR:
                                            {
                                                _vorDialSkipper.Click(_lowerFreqSwitchPressedDown ? VOR_TEST_COMMAND_DEC : VOR_ONES_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentM2000CRadioMode.NO_USE:
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
                                string channelAsString;
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
                                string channelAsString;
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
                                string channelAsString;
                                string modeAsString;
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
                                string channelAsString;
                                string settingsAsString;
                                lock (_lockVoRialObject)
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

                        case CurrentM2000CRadioMode.NO_USE:
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
                                string channelAsString;
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
                                string channelAsString;
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
                                string channelAsString;
                                string modeAsString;
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
                                string channelAsString;
                                string settingsAsString;
                                lock (_lockVoRialObject)
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

                        case CurrentM2000CRadioMode.NO_USE:
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

        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
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
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentM2000CRadioMode;

                // If NO_USE then send next round of data to the panel in order to clear the LCD.
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
