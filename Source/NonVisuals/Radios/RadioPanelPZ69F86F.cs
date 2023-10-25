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
    /// Pre-programmed radio panel for the F-86F. 
    /// </summary>
    public class RadioPanelPZ69F86F : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentF86FRadioMode
        {
            ARC27_PRESET,
            ARC27_VOL,
            ARN6,
            ARN6_MODES,
            ADF_APX6,
            NO_USE
        }

        private CurrentF86FRadioMode _currentUpperRadioMode = CurrentF86FRadioMode.ARC27_PRESET;
        private CurrentF86FRadioMode _currentLowerRadioMode = CurrentF86FRadioMode.ARC27_PRESET;

        /*F-86F UHF ARC-27 PRESETS COM1*/
        // Large dial 1-18 [step of 1]
        // Small dial Power/Mode control
        private readonly object _lockARC27PresetDialObject1 = new();
        private DCSBIOSOutput _arc27PresetDcsbiosOutputPresetDial;
        private volatile uint _arc27PresetCockpitDialPos = 1;
        private const string ARC27_PRESET_COMMAND_INC = "ARC27_CHAN_SEL INC\n";
        private const string ARC27_PRESET_COMMAND_DEC = "ARC27_CHAN_SEL DEC\n";
        private readonly ClickSkipper _arc27PresetDialSkipper = new(2);
        private readonly object _lockARC27ModeDialObject1 = new();
        private DCSBIOSOutput _arc27ModeDcsbiosOutputDial;
        private volatile uint _arc27ModeCockpitDialPos = 1;
        private const string ARC27_MODE_COMMAND_INC = "ARC27_PWR_SEL INC\n";
        private const string ARC27_MODE_COMMAND_DEC = "ARC27_PWR_SEL DEC\n";
        private readonly ClickSkipper _arc27ModeDialSkipper = new(2);

        /*F-86F ARC-27 PRESETS COM2*/
        // Small dial Volume Control
        private const string ARC27_VOLUME_KNOB_COMMAND_INC = "ARC_27_VOL +2500\n";
        private const string ARC27_VOLUME_KNOB_COMMAND_DEC = "ARC_27_VOL -2500\n";

        /*F-86F ARN-6 MANUAL NAV1*/
        // Large dial -> tuning
        // Small dial -> bands
        private readonly ClickSpeedDetector _bigFreqIncreaseChangeMonitor = new(20);
        private readonly ClickSpeedDetector _bigFreqDecreaseChangeMonitor = new(20);
        private readonly object _lockARN6FrequencyObject = new();
        private readonly object _lockARN6BandObject = new();
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
        private readonly ClickSkipper _arn6BandDialSkipper = new(2);

        /*F-86F ARN-6 MODES NAV2*/
        // Large dial MODES
        // Small dial volume control
        private readonly object _lockARN6ModeObject = new();
        private DCSBIOSOutput _arn6ModeDcsbiosOutputPresetDial;
        private volatile uint _arn6ModeCockpitDialPos = 1;
        private const string ARN6_MODE_COMMAND_INC = "ARN6_FUNC_SEL INC\n";
        private const string ARN6_MODE_COMMAND_DEC = "ARN6_FUNC_SEL DEC\n";
        private readonly ClickSkipper _arn6ModeDialSkipper = new(2);
        private const string ARN6_VOLUME_KNOB_COMMAND_INC = "ARN_6_VOL +2500\n";
        private const string ARN6_VOLUME_KNOB_COMMAND_DEC = "ARN_6_VOL -2500\n";

        /*F-86F APX-6 ADF*/
        // Large dial MODES
        // Small - No Use
        // ACT-STBY, Toggles IFF Dial Stop Button, button must be depressed to go into Emergency Mode.
        private volatile uint _apx6ModeCockpitDialPos = 1;
        private readonly object _lockAPX6ModeObject = new();
        private readonly ClickSkipper _apx6ModeDialSkipper = new(2);
        private DCSBIOSOutput _apx6ModeDcsbiosOutputCockpit;
        private const string APX6_MODE_DIAL_COMMAND_INC = "APX6_MASTER INC\n";
        private const string APX6_MODE_DIAL_COMMAND_DEC = "APX6_MASTER DEC\n";
        private const string APX_6DIAL_STOP_TOGGLE_COMMAND = "APX_6_IFF_DIAL_STOP TOGGLE\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69F86F(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {}

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

        public override void InitPanel()
        {
            CreateRadioKnobs();

            // COM1
            _arc27PresetDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("ARC27_CHAN_SEL");
            _arc27ModeDcsbiosOutputDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("ARC27_PWR_SEL");

            // COM2

            // NAV1
            _arn6ManualDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("ARN6_FREQUENCY");
            _arn6BandDcsbiosOutputCockpit = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("ARN6_CHAN_SEL");

            // NAV2
            _arn6ModeDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("ARN6_FUNC_SEL");

            // ADF
            _apx6ModeDcsbiosOutputCockpit = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("APX6_MASTER");
            
            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
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

                // ARC-27 Preset Channel Dial
                if (_arc27PresetDcsbiosOutputPresetDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockARC27PresetDialObject1)
                    {
                        _arc27PresetCockpitDialPos = _arc27PresetDcsbiosOutputPresetDial.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // ARC-27 Mode Dial
                if (_arc27ModeDcsbiosOutputDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockARC27ModeDialObject1)
                    {
                        _arc27ModeCockpitDialPos = _arc27ModeDcsbiosOutputDial.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // ARN-6 Frequency
                if (_arn6ManualDcsbiosOutputCockpitFrequency.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockARN6FrequencyObject)
                    {
                        _arn6CockpitFrequency = _arn6ManualDcsbiosOutputCockpitFrequency.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // ARN-6 Band
                if (_arn6BandDcsbiosOutputCockpit.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockARN6BandObject)
                    {
                        _arn6CockpitBand = _arn6BandDcsbiosOutputCockpit.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // ARN-6 Modes
                if (_arn6ModeDcsbiosOutputPresetDial.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockARN6ModeObject)
                    {
                        _arn6ModeCockpitDialPos = _arn6ModeDcsbiosOutputPresetDial.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // APX-6 Modes
                if (_apx6ModeDcsbiosOutputCockpit.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockAPX6ModeObject)
                    {
                        _apx6ModeCockpitDialPos = _apx6ModeDcsbiosOutputCockpit.LastUIntValue;
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
                                        SetUpperRadioMode(CurrentF86FRadioMode.NO_USE);
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
                                        SetLowerRadioMode(CurrentF86FRadioMode.NO_USE);
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
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F86F, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }

                    AdjustFrequency(hashSet);
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
                                                _arc27PresetDialSkipper.Click(ARC27_PRESET_COMMAND_INC);
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
                                                _arn6ModeDialSkipper.Click(ARN6_MODE_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                _apx6ModeDialSkipper.Click(APX6_MODE_DIAL_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27PresetDialSkipper.Click(ARC27_PRESET_COMMAND_DEC);
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
                                                _arn6ModeDialSkipper.Click(ARN6_MODE_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                _apx6ModeDialSkipper.Click(APX6_MODE_DIAL_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27ModeDialSkipper.Click(ARC27_MODE_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _arn6BandDialSkipper.Click(ARN6_BAND_DIAL_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                _arn6ModeDialSkipper.Click(ARN6_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27ModeDialSkipper.Click(ARC27_MODE_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _arn6BandDialSkipper.Click(ARN6_BAND_DIAL_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                _arn6ModeDialSkipper.Click(ARN6_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27PresetDialSkipper.Click(ARC27_PRESET_COMMAND_INC);
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
                                                _arn6ModeDialSkipper.Click(ARN6_MODE_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                _apx6ModeDialSkipper.Click(APX6_MODE_DIAL_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27PresetDialSkipper.Click(ARC27_PRESET_COMMAND_DEC);
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
                                                _arn6ModeDialSkipper.Click(ARN6_MODE_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                _apx6ModeDialSkipper.Click(APX6_MODE_DIAL_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27ModeDialSkipper.Click(ARC27_MODE_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _arn6BandDialSkipper.Click(ARN6_BAND_DIAL_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                _arn6ModeDialSkipper.Click(ARN6_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NO_USE:
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
                                                _arc27ModeDialSkipper.Click(ARC27_MODE_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _arn6BandDialSkipper.Click(ARN6_BAND_DIAL_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                _arn6ModeDialSkipper.Click(ARN6_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentF86FRadioMode.ADF_APX6:
                                        case CurrentF86FRadioMode.NO_USE:
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
                        case CurrentF86FRadioMode.ARC27_PRESET:
                            {
                                // Preset Channel Selector
                                // " 1" -> "18"
                                // Pos     0 .. 17
                                string channelAsString;
                                lock (_lockARC27PresetDialObject1)
                                {
                                    channelAsString = (_arc27PresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                string modeAsString;
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
                                string frequencyAsString;
                                lock (_lockARN6FrequencyObject)
                                {
                                    frequencyAsString = (_arn6CockpitFrequency).ToString().PadLeft(4, ' ');
                                }

                                string bandAsString;
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
                                uint mode;
                                lock (_lockARN6ModeObject)
                                {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                                    mode = _arn6ModeCockpitDialPos switch
                                    {
                                        2 => 1,
                                        3 => 2,
                                        0 => 3,
                                        1 => 4
                                    };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
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
                                string modeAsString;
                                lock (_lockAPX6ModeObject)
                                {
                                    modeAsString = (_apx6ModeCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.NO_USE:
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
                                string channelAsString;
                                lock (_lockARC27PresetDialObject1)
                                {
                                    channelAsString = (_arc27PresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                string modeAsString;
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
                                string frequencyAsString;
                                lock (_lockARN6FrequencyObject)
                                {
                                    frequencyAsString = (_arn6CockpitFrequency).ToString().PadLeft(4, ' ');
                                }

                                string bandAsString;
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
                                uint mode;
                                lock (_lockARN6ModeObject)
                                {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                                    mode = _arn6ModeCockpitDialPos switch
                                    {
                                        2 => 1,
                                        3 => 2,
                                        0 => 3,
                                        1 => 4
                                    };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
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
                                string modeAsString;
                                lock (_lockAPX6ModeObject)
                                {
                                    modeAsString = (_apx6ModeCockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentF86FRadioMode.NO_USE:
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
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentF86FRadioMode currentF86FRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentF86FRadioMode;
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
