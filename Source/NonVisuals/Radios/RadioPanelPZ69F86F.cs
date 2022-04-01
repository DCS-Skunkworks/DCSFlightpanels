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

    public class RadioPanelPZ69F86F : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private CurrentF86FRadioMode _currentUpperRadioMode = CurrentF86FRadioMode.ARC27_PRESET;
        private CurrentF86FRadioMode _currentLowerRadioMode = CurrentF86FRadioMode.ARC27_PRESET;

        /*F-86F UHF ARC-27 PRESETS COM1*/
        // Large dial 1-18 [step of 1]
        // Small dial Power/Mode control
        private readonly object _lockARC27PresetDialObject1 = new object();
        private DCSBIOSOutput _arc27PresetDcsbiosOutputPresetDial;
        private volatile uint _arc27PresetCockpitDialPos = 1;
        private const string ARC27_PRESET_COMMAND_INC = "ARC27_CHAN_SEL INC\n";
        private const string ARC27_PRESET_COMMAND_DEC = "ARC27_CHAN_SEL DEC\n";
        private int _arc27PresetDialSkipper;
        private readonly object _lockARC27ModeDialObject1 = new object();
        private DCSBIOSOutput _arc27ModeDcsbiosOutputDial;
        private volatile uint _arc27ModeCockpitDialPos = 1;
        private const string ARC27_MODE_COMMAND_INC = "ARC27_PWR_SEL INC\n";
        private const string ARC27_MODE_COMMAND_DEC = "ARC27_PWR_SEL DEC\n";
        private int _arc27ModeDialSkipper;

        /*F-86F ARC-27 PRESETS COM2*/
        // Small dial Volume Control
        private const string ARC27_VOLUME_KNOB_COMMAND_INC = "ARC_27_VOL +2500\n";
        private const string ARC27_VOLUME_KNOB_COMMAND_DEC = "ARC_27_VOL -2500\n";

        /*F-86F ARN-6 MANUAL NAV1*/
        // Large dial -> tuning
        // Small dial -> bands
        private readonly ClickSpeedDetector _bigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _bigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly object _lockARN6FrequencyObject = new object();
        private readonly object _lockARN6BandObject = new object();
        private volatile uint _arn6CockpitFrequency = 108;
        private volatile uint _arn6CockpitBand;
        private DCSBIOSOutput _arn6ManualDcsbiosOutputCockpitFrequency;
        private DCSBIOSOutput _arn6BandDcsbiosOutputCockpit;
        private const string ARN6_FREQUENCY_COMMAND_MORE_INC = "ARN_6_TUNE +1000\n";
        private const string ARN6_FREQUENCY_COMMAND_MORE_DEC = "ARN_6_TUNE -1000\n";
        private const string ARN6_FREQUENCY_COMMAND_INC = "ARN_6_TUNE +50\n";
        private const string ARN6_FREQUENCY_COMMAND_DEC = "ARN_6_TUNE -50\n";
        private const string ARN6_BAND_DIAL_COMMAND_INC = "ARN6_CHAN_SEL INC\n";
        private const string ARN6_BAND_DIAL_COMMAND_DEC = "ARN6_CHAN_SEL DEC\n";
        private int _arn6BandDialSkipper;

        /*F-86F ARN-6 MODES NAV2*/
        // Large dial MODES
        // Small dial volume control
        private readonly object _lockARN6ModeObject = new object();
        private DCSBIOSOutput _arn6ModeDcsbiosOutputPresetDial;
        private volatile uint _arn6ModeCockpitDialPos = 1;
        private const string ARN6_MODE_COMMAND_INC = "ARN6_FUNC_SEL INC\n";
        private const string ARN6_MODE_COMMAND_DEC = "ARN6_FUNC_SEL DEC\n";
        private int _arn6ModeDialSkipper;
        private const string ARN6_VOLUME_KNOB_COMMAND_INC = "ARN_6_VOL +2500\n";
        private const string ARN6_VOLUME_KNOB_COMMAND_DEC = "ARN_6_VOL -2500\n";

        /*F-86F APX-6 ADF*/
        // Large dial MODES
        // Small - No Use
        // ACT-STBY, Toggles IFF Dial Stop Button, button must be depressed to go into Emergency Mode.
        private volatile uint _apx6ModeCockpitDialPos = 1;
        private readonly object _lockAPX6ModeObject = new object();
        private int _apx6ModeDialSkipper;
        private DCSBIOSOutput _apx6ModeDcsbiosOutputCockpit;
        private const string APX6_MODE_DIAL_COMMAND_INC = "APX6_MASTER INC\n";
        private const string APX6_MODE_DIAL_COMMAND_DEC = "APX6_MASTER DEC\n";
        private const string APX_6DIAL_STOP_TOGGLE_COMMAND = "APX_6_IFF_DIAL_STOP TOGGLE\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69F86F(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                /*
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }*/
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

                // ARC-27 Preset Channel Dial
                if (e.Address == _arc27PresetDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockARC27PresetDialObject1)
                    {
                        var tmp = _arc27PresetCockpitDialPos;
                        _arc27PresetCockpitDialPos = _arc27PresetDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _arc27PresetCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // ARC-27 Mode Dial
                if (e.Address == _arc27ModeDcsbiosOutputDial.Address)
                {
                    lock (_lockARC27ModeDialObject1)
                    {
                        var tmp = _arc27ModeCockpitDialPos;
                        _arc27ModeCockpitDialPos = _arc27ModeDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _arc27ModeCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // ARN-6 Frequency
                if (e.Address == _arn6ManualDcsbiosOutputCockpitFrequency.Address)
                {
                    lock (_lockARN6FrequencyObject)
                    {
                        var tmp = _arn6CockpitFrequency;
                        _arn6CockpitFrequency = _arn6ManualDcsbiosOutputCockpitFrequency.GetUIntValue(e.Data);
                        if (tmp != _arn6CockpitFrequency)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // ARN-6 Band
                if (e.Address == _arn6BandDcsbiosOutputCockpit.Address)
                {
                    lock (_lockARN6BandObject)
                    {
                        var tmp = _arn6CockpitBand;
                        _arn6CockpitBand = _arn6BandDcsbiosOutputCockpit.GetUIntValue(e.Data);
                        if (tmp != _arn6CockpitBand)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // ARN-6 Modes
                if (e.Address == _arn6ModeDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockARN6ModeObject)
                    {
                        var tmp = _arn6ModeCockpitDialPos;
                        _arn6ModeCockpitDialPos = _arn6ModeDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _arn6ModeCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // APX-6 Modes
                if (e.Address == _apx6ModeDcsbiosOutputCockpit.Address)
                {
                    lock (_lockAPX6ModeObject)
                    {
                        var tmp = _apx6ModeCockpitDialPos;
                        _apx6ModeCockpitDialPos = _apx6ModeDcsbiosOutputCockpit.GetUIntValue(e.Data);
                        if (tmp != _apx6ModeCockpitDialPos)
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


        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF86F knob)
        {
            try
            {

                if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsF86F.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsF86F.LOWER_FREQ_SWITCH))
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
                    case RadioPanelPZ69KnobsF86F.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentF86FRadioMode.ARC27_PRESET:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ARC27_VOL:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ARN6:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ARN6_MODES:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ADF_APX6:
                                    {
                                        DCSBIOS.Send(APX_6DIAL_STOP_TOGGLE_COMMAND);
                                        break;
                                    }
                            }
                            break;
                        }

                    case RadioPanelPZ69KnobsF86F.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentF86FRadioMode.ARC27_PRESET:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ARC27_VOL:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ARN6:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ARN6_MODES:
                                    {
                                        break;
                                    }

                                case CurrentF86FRadioMode.ADF_APX6:
                                    {
                                        DCSBIOS.Send(APX_6DIAL_STOP_TOGGLE_COMMAND);
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
                        var radioPanelKnob = (RadioPanelKnobF86F)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsF86F.UPPER_ARC27_PRESET:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF86FRadioMode.ARC27_PRESET);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_ARC27_VOL:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF86FRadioMode.ARC27_VOL);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_ARN6:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF86FRadioMode.ARN6);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_ARN6_MODES:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF86FRadioMode.ARN6_MODES);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_ADF_APX6:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF86FRadioMode.ADF_APX6);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsF86F.UPPER_NO_USE2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF86FRadioMode.NOUSE);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_ARC27_PRESET:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF86FRadioMode.ARC27_PRESET);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_ARC27_VOL:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF86FRadioMode.ARC27_VOL);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_ARN6:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF86FRadioMode.ARN6);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_ARN6_MODES:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF86FRadioMode.ARN6_MODES);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_ADF_APX6:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF86FRadioMode.ADF_APX6);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsF86F.LOWER_NO_USE2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF86FRadioMode.NOUSE);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentF86FRadioMode.ADF_APX6)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF86F.UPPER_FREQ_SWITCH);
                                        }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentF86FRadioMode.ADF_APX6)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF86F.LOWER_FREQ_SWITCH);
                                        }
                                    }

                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSFPProfile.SelectedModule.Description, HIDInstance, (int)PluginGamingPanelEnum.PZ69RadioPanel, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
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
                    var radioPanelKnobF86F = (RadioPanelKnobF86F)o;
                    if (radioPanelKnobF86F.IsOn)
                    {
                        switch (radioPanelKnobF86F.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27PresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_bigFreqIncreaseChangeMonitor.ClickThresholdReached() ? ARN6_FREQUENCY_COMMAND_MORE_INC : ARN6_FREQUENCY_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_MODE_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6_MODE_DIAL_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27PresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_bigFreqDecreaseChangeMonitor.ClickThresholdReached() ? ARN6_FREQUENCY_COMMAND_MORE_DEC : ARN6_FREQUENCY_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_MODE_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6_MODE_DIAL_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_MODE_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_BAND_DIAL_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_VOLUME_KNOB_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_MODE_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_BAND_DIAL_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_VOLUME_KNOB_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27PresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_bigFreqIncreaseChangeMonitor.ClickThresholdReached() ? ARN6_FREQUENCY_COMMAND_MORE_INC : ARN6_FREQUENCY_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_MODE_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6_MODE_DIAL_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27PresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_bigFreqDecreaseChangeMonitor.ClickThresholdReached() ? ARN6_FREQUENCY_COMMAND_MORE_DEC : ARN6_FREQUENCY_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_MODE_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6_MODE_DIAL_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_MODE_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_BAND_DIAL_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_VOLUME_KNOB_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF86FRadioMode.ARC27_PRESET:
                                            {
                                                if (!SkipARC27ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARC27_MODE_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_BAND_DIAL_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6_VOLUME_KNOB_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NOUSE:
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
                        case CurrentF86FRadioMode.ARC27_PRESET:
                            {
                                // Preset Channel Selector
                                // " 1" -> "18"
                                // Pos     0 .. 17
                                var channelAsString = string.Empty;
                                lock (_lockARC27PresetDialObject1)
                                {
                                    channelAsString = (_arc27PresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                var modeAsString = string.Empty;
                                lock (_lockARC27ModeDialObject1)
                                {
                                    modeAsString = (_arc27ModeCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ARC27_VOL:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ARN6:
                            {
                                // Frequency
                                // Band
                                var frequencyAsString = string.Empty;
                                lock (_lockARN6FrequencyObject)
                                {
                                    frequencyAsString = (_arn6CockpitFrequency).ToString().PadLeft(4, ' ');
                                }

                                var bandAsString = string.Empty;
                                lock (_lockARN6BandObject)
                                {
                                    bandAsString = (_arn6CockpitBand + 1).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(bandAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(frequencyAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ARN6_MODES:
                            {
                                // Modes
                                uint mode = 0;
                                lock (_lockARN6ModeObject)
                                {
                                    switch (_arn6ModeCockpitDialPos)
                                    {
                                        case 2:
                                            {
                                                mode = 1;
                                                break;
                                            }

                                        case 3:
                                            {
                                                mode = 2;
                                                break;
                                            }

                                        case 0:
                                            {
                                                mode = 3;
                                                break;
                                            }

                                        case 1:
                                            {
                                                mode = 4;
                                                break;
                                            }
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, mode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ADF_APX6:
                            {
                                // Modes
                                // Emergency ON OFF
                                // Modes
                                var modeAsString = string.Empty;
                                lock (_lockAPX6ModeObject)
                                {
                                    modeAsString = (_apx6ModeCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentF86FRadioMode.ARC27_PRESET:
                            {
                                // Preset Channel Selector
                                // " 1" -> "18"
                                // Pos     0 .. 17
                                var channelAsString = string.Empty;
                                lock (_lockARC27PresetDialObject1)
                                {
                                    channelAsString = (_arc27PresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                var modeAsString = string.Empty;
                                lock (_lockARC27ModeDialObject1)
                                {
                                    modeAsString = (_arc27ModeCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ARC27_VOL:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ARN6:
                            {
                                // Frequency
                                // Band
                                var frequencyAsString = string.Empty;
                                lock (_lockARN6FrequencyObject)
                                {
                                    frequencyAsString = (_arn6CockpitFrequency).ToString().PadLeft(4, ' ');
                                }

                                var bandAsString = string.Empty;
                                lock (_lockARN6BandObject)
                                {
                                    bandAsString = (_arn6CockpitBand + 1).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(bandAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(frequencyAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ARN6_MODES:
                            {
                                // Modes
                                uint mode = 0;
                                lock (_lockARN6ModeObject)
                                {
                                    switch (_arn6ModeCockpitDialPos)
                                    {
                                        case 2:
                                            {
                                                mode = 1;
                                                break;
                                            }

                                        case 3:
                                            {
                                                mode = 2;
                                                break;
                                            }

                                        case 0:
                                            {
                                                mode = 3;
                                                break;
                                            }

                                        case 1:
                                            {
                                                mode = 4;
                                                break;
                                            }
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, mode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.ADF_APX6:
                            {
                                // Modes
                                // Emergency ON OFF
                                // Modes
                                var modeAsString = string.Empty;
                                lock (_lockAPX6ModeObject)
                                {
                                    modeAsString = (_apx6ModeCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.NOUSE:
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
                _arc27PresetDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC27_CHAN_SEL");
                _arc27ModeDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC27_PWR_SEL");

                // COM2

                // NAV1
                _arn6ManualDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("ARN6_FREQUENCY");
                _arn6BandDcsbiosOutputCockpit = DCSBIOSControlLocator.GetDCSBIOSOutput("ARN6_CHAN_SEL");

                // NAV2
                _arn6ModeDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARN6_FUNC_SEL");

                // ADF
                _apx6ModeDcsbiosOutputCockpit = DCSBIOSControlLocator.GetDCSBIOSOutput("APX6_MASTER");

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
            SaitekPanelKnobs = RadioPanelKnobF86F.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentF86FRadioMode currentF86FRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentF86FRadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentF86FRadioMode currentF86FRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentF86FRadioMode;

                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }


        private bool SkipARC27PresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARC27_PRESET || _currentLowerRadioMode == CurrentF86FRadioMode.ARC27_PRESET)
                {
                    if (_arc27PresetDialSkipper > 2)
                    {
                        _arc27PresetDialSkipper = 0;
                        return false;
                    }

                    _arc27PresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipARC27ModeDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARC27_PRESET || _currentLowerRadioMode == CurrentF86FRadioMode.ARC27_PRESET)
                {
                    if (_arc27ModeDialSkipper > 2)
                    {
                        _arc27ModeDialSkipper = 0;
                        return false;
                    }

                    _arc27ModeDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipARN6BandDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARN6 || _currentLowerRadioMode == CurrentF86FRadioMode.ARN6)
                {
                    if (_arn6BandDialSkipper > 2)
                    {
                        _arn6BandDialSkipper = 0;
                        return false;
                    }

                    _arn6BandDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipARN6ModeDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARN6_MODES || _currentLowerRadioMode == CurrentF86FRadioMode.ARN6_MODES)
                {
                    if (_arn6ModeDialSkipper > 2)
                    {
                        _arn6ModeDialSkipper = 0;
                        return false;
                    }

                    _arn6ModeDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipAPX6ModeDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ADF_APX6 || _currentLowerRadioMode == CurrentF86FRadioMode.ADF_APX6)
                {
                    if (_apx6ModeDialSkipper > 2)
                    {
                        _apx6ModeDialSkipper = 0;
                        return false;
                    }

                    _apx6ModeDialSkipper++;
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
