using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69F86F : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobF86F> _radioPanelKnobs = new HashSet<RadioPanelKnobF86F>();
        private CurrentF86FRadioMode _currentUpperRadioMode = CurrentF86FRadioMode.ARC27_PRESET;
        private CurrentF86FRadioMode _currentLowerRadioMode = CurrentF86FRadioMode.ARC27_PRESET;

        /*F-86F UHF ARC-27 PRESETS COM1*/
        //Large dial 1-18 [step of 1]
        //Small dial Power/Mode control
        private readonly object _lockARC27PresetDialObject1 = new object();
        private DCSBIOSOutput _arc27PresetDcsbiosOutputPresetDial;
        private volatile uint _arc27PresetCockpitDialPos = 1;
        private const string ARC27PresetCommandInc = "ARC27_CHAN_SEL INC\n";
        private const string ARC27PresetCommandDec = "ARC27_CHAN_SEL DEC\n";
        private int _arc27PresetDialSkipper;
        private readonly object _lockARC27ModeDialObject1 = new object();
        private DCSBIOSOutput _arc27ModeDcsbiosOutputDial;
        private volatile uint _arc27ModeCockpitDialPos = 1;
        private const string ARC27ModeCommandInc = "ARC27_PWR_SEL INC\n";
        private const string ARC27ModeCommandDec = "ARC27_PWR_SEL DEC\n";
        private int _arc27ModeDialSkipper;

        /*F-86F ARC-27 PRESETS COM2*/
        //Small dial Volume Control
        private const string ARC27VolumeKnobCommandInc = "ARC_27_VOL +2500\n";
        private const string ARC27VolumeKnobCommandDec = "ARC_27_VOL -2500\n";

        /*F-86F ARN-6 MANUAL NAV1*/
        //Large dial -> tuning
        //Small dial -> bands
        private ClickSpeedDetector _bigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _bigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        const int ChangeValue = 10;
        private object _lockARN6FrequencyObject = new object();
        private object _lockARN6BandObject = new object();
        private volatile uint _arn6CockpitFrequency = 108;
        private volatile uint _arn6CockpitBand;
        private DCSBIOSOutput _arn6ManualDcsbiosOutputCockpitFrequency;
        private DCSBIOSOutput _arn6BandDcsbiosOutputCockpit;
        private const string ARN6FrequencyCommandMoreInc = "ARN_6_TUNE +1000\n";
        private const string ARN6FrequencyCommandMoreDec = "ARN_6_TUNE -1000\n";
        private const string ARN6FrequencyCommandInc = "ARN_6_TUNE +50\n";
        private const string ARN6FrequencyCommandDec = "ARN_6_TUNE -50\n";
        private const string ARN6BandDialCommandInc = "ARN6_CHAN_SEL INC\n";
        private const string ARN6BandDialCommandDec = "ARN6_CHAN_SEL DEC\n";
        private int _arn6BandDialSkipper;

        /*F-86F ARN-6 MODES NAV2*/
        //Large dial MODES
        //Small dial volume control
        private object _lockARN6ModeObject = new object();
        private DCSBIOSOutput _arn6ModeDcsbiosOutputPresetDial;
        private volatile uint _arn6ModeCockpitDialPos = 1;
        private const string ARN6ModeCommandInc = "ARN6_FUNC_SEL INC\n";
        private const string ARN6ModeCommandDec = "ARN6_FUNC_SEL DEC\n";
        private int _arn6ModeDialSkipper;
        private const string ARN6VolumeKnobCommandInc = "ARN_6_VOL +2500\n";
        private const string ARN6VolumeKnobCommandDec = "ARN_6_VOL -2500\n";

        /*F-86F APX-6 ADF*/
        //Large dial MODES
        //Small - No Use
        //ACT-STBY, Toggles IFF Dial Stop Button, button must be depressed to go into Emergency Mode.
        private volatile uint _apx6ModeCockpitDialPos = 1;
        private object _lockAPX6ModeObject = new object();
        private int _apx6ModeDialSkipper;
        private DCSBIOSOutput _apx6ModeDcsbiosOutputCockpit;
        private const string APX6ModeDialCommandInc = "APX6_MASTER INC\n";
        private const string APX6ModeDialCommandDec = "APX6_MASTER DEC\n";
        private const string APX6DialStopToggleCommand = "APX_6_IFF_DIAL_STOP TOGGLE\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69F86F(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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

                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }
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



                //ARC-27 Preset Channel Dial
                if (e.Address == _arc27PresetDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockARC27PresetDialObject1)
                    {
                        var tmp = _arc27PresetCockpitDialPos;
                        _arc27PresetCockpitDialPos = _arc27PresetDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _arc27PresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARC-27 Mode Dial
                if (e.Address == _arc27ModeDcsbiosOutputDial.Address)
                {
                    lock (_lockARC27ModeDialObject1)
                    {
                        var tmp = _arc27ModeCockpitDialPos;
                        _arc27ModeCockpitDialPos = _arc27ModeDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _arc27ModeCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARN-6 Frequency
                if (e.Address == _arn6ManualDcsbiosOutputCockpitFrequency.Address)
                {
                    lock (_lockARN6FrequencyObject)
                    {
                        var tmp = _arn6CockpitFrequency;
                        _arn6CockpitFrequency = _arn6ManualDcsbiosOutputCockpitFrequency.GetUIntValue(e.Data);
                        if (tmp != _arn6CockpitFrequency)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARN-6 Band
                if (e.Address == _arn6BandDcsbiosOutputCockpit.Address)
                {
                    lock (_lockARN6BandObject)
                    {
                        var tmp = _arn6CockpitBand;
                        _arn6CockpitBand = _arn6BandDcsbiosOutputCockpit.GetUIntValue(e.Data);
                        if (tmp != _arn6CockpitBand)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARN-6 Modes
                if (e.Address == _arn6ModeDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockARN6ModeObject)
                    {
                        var tmp = _arn6ModeCockpitDialPos;
                        _arn6ModeCockpitDialPos = _arn6ModeDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _arn6ModeCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //APX-6 Modes
                if (e.Address == _apx6ModeDcsbiosOutputCockpit.Address)
                {
                    lock (_lockAPX6ModeObject)
                    {
                        var tmp = _apx6ModeCockpitDialPos;
                        _apx6ModeCockpitDialPos = _apx6ModeDcsbiosOutputCockpit.GetUIntValue(e.Data);
                        if (tmp != _apx6ModeCockpitDialPos)
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


        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF86F knob)
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SendFrequencyToDCSBIOS()");
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
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
                                        DCSBIOS.Send(APX6DialStopToggleCommand);
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
                                        DCSBIOS.Send(APX6DialStopToggleCommand);
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
            Common.DebugP("Leaving F-86F Radio SendFrequencyToDCSBIOS()");
        }


        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering F-86F Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (_lockLCDUpdateObject)
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
                            case RadioPanelPZ69KnobsF86F.UPPER_NOUSE1:
                            case RadioPanelPZ69KnobsF86F.UPPER_NOUSE2:
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
                            case RadioPanelPZ69KnobsF86F.LOWER_NOUSE1:
                            case RadioPanelPZ69KnobsF86F.LOWER_NOUSE2:
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
                                    //Ignore
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
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78006, ex);
            }
            Common.DebugP("Leaving F-86F Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering F-86F Radio AdjustFrequency()");

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
                                                    DCSBIOS.Send(ARC27PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                if (_bigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandMoreInc);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6ModeCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6ModeDialCommandInc);
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
                                                    DCSBIOS.Send(ARC27PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                if (_bigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandMoreDec);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6ModeCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6ModeDialCommandDec);
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
                                                    DCSBIOS.Send(ARC27ModeCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27VolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6BandDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6VolumeKnobCommandInc);
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
                                                    DCSBIOS.Send(ARC27ModeCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27VolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6BandDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6VolumeKnobCommandDec);
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
                                                    DCSBIOS.Send(ARC27PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                if (_bigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandMoreInc);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6ModeCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6ModeDialCommandInc);
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
                                                    DCSBIOS.Send(ARC27PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                if (_bigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandMoreDec);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARN6FrequencyCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6ModeCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ADF_APX6:
                                            {
                                                if (!SkipAPX6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(APX6ModeDialCommandDec);
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
                                                    DCSBIOS.Send(ARC27ModeCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27VolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6BandDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6VolumeKnobCommandInc);
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
                                                    DCSBIOS.Send(ARC27ModeCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARC27_VOL:
                                            {
                                                DCSBIOS.Send(ARC27VolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6:
                                            {
                                                if (!SkipARN6BandDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6BandDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentF86FRadioMode.ARN6_MODES:
                                            {
                                                if (!SkipARN6ModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARN6VolumeKnobCommandDec);
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
                Common.LogError(78007, ex);
            }
            Common.DebugP("Leaving F-86F Radio AdjustFrequency()");
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

                    Common.DebugP("Entering F-86F Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentF86FRadioMode.ARC27_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 1" -> "18"
                                //Pos     0 .. 17

                                var channelAsString = "";
                                lock (_lockARC27PresetDialObject1)
                                {
                                    channelAsString = (_arc27PresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                var modeAsString = "";
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
                                //Frequency
                                //Band

                                var frequencyAsString = "";
                                lock (_lockARN6FrequencyObject)
                                {
                                    frequencyAsString = (_arn6CockpitFrequency).ToString().PadLeft(4, ' ');
                                }
                                var bandAsString = "";
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
                                //Modes
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
                                //Modes
                                //Emergency ON OFF
                                //Modes

                                var modeAsString = "";
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
                                //Preset Channel Selector
                                //      " 1" -> "18"
                                //Pos     0 .. 17

                                var channelAsString = "";
                                lock (_lockARC27PresetDialObject1)
                                {
                                    channelAsString = (_arc27PresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                var modeAsString = "";
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
                                //Frequency
                                //Band

                                var frequencyAsString = "";
                                lock (_lockARN6FrequencyObject)
                                {
                                    frequencyAsString = (_arn6CockpitFrequency).ToString().PadLeft(4, ' ');
                                }
                                var bandAsString = "";
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
                                //Modes
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
                                //Modes
                                //Emergency ON OFF
                                //Modes

                                var modeAsString = "";
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
                Common.LogError(78011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving F-86F Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering F-86F Radio OnReport()");
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
                                    var knob = (RadioPanelKnobF86F)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobF86F)radioPanelKnob));
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
            Common.DebugP("Leaving F-86F Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering F-86F Radio GetHashSetOfChangedKnobs()");


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
            Common.DebugP("Leaving F-86F Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("F-86F");

                //COM1
                _arc27PresetDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC27_CHAN_SEL");
                _arc27ModeDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC27_PWR_SEL");

                //COM2
                // nada

                //NAV1
                _arn6ManualDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("ARN6_FREQUENCY");
                _arn6BandDcsbiosOutputCockpit = DCSBIOSControlLocator.GetDCSBIOSOutput("ARN6_CHAN_SEL");

                //NAV2
                _arn6ModeDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARN6_FUNC_SEL");

                //ADF
                _apx6ModeDcsbiosOutputCockpit = DCSBIOSControlLocator.GetDCSBIOSOutput("APX6_MASTER");


                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69F86F.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering F-86F Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving F-86F Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobF86F.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobF86F radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentF86FRadioMode currentF86FRadioMode)
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentF86FRadioMode);
                _currentUpperRadioMode = currentF86FRadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(78014, ex);
            }
            Common.DebugP("Leaving F-86F Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentF86FRadioMode currentF86FRadioMode)
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentF86FRadioMode);
                _currentLowerRadioMode = currentF86FRadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            Common.DebugP("Leaving F-86F Radio SetLowerRadioMode()");
        }


        private bool SkipARC27PresetDialChange()
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SkipARC27PresetDialChange()");
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARC27_PRESET || _currentLowerRadioMode == CurrentF86FRadioMode.ARC27_PRESET)
                {
                    if (_arc27PresetDialSkipper > 2)
                    {
                        _arc27PresetDialSkipper = 0;
                        Common.DebugP("Leaving F-86F Radio SkipARC27PresetDialChange()");
                        return false;
                    }
                    _arc27PresetDialSkipper++;
                    Common.DebugP("Leaving F-86F Radio SkipARC27PresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving F-86F Radio SkipARC27PresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        private bool SkipARC27ModeDialChange()
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SkipARC27ModeDialChange()");
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARC27_PRESET || _currentLowerRadioMode == CurrentF86FRadioMode.ARC27_PRESET)
                {
                    if (_arc27ModeDialSkipper > 2)
                    {
                        _arc27ModeDialSkipper = 0;
                        Common.DebugP("Leaving F-86F Radio SkipARC27ModeDialChange()");
                        return false;
                    }
                    _arc27ModeDialSkipper++;
                    Common.DebugP("Leaving F-86F Radio SkipARC27ModeDialChange()");
                    return true;
                }
                Common.DebugP("Leaving F-86F Radio SkipARC27ModeDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        private bool SkipARN6BandDialChange()
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SkipARN6BandDialChange()");
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARN6 || _currentLowerRadioMode == CurrentF86FRadioMode.ARN6)
                {
                    if (_arn6BandDialSkipper > 2)
                    {
                        _arn6BandDialSkipper = 0;
                        Common.DebugP("Leaving F-86F Radio SkipARN6BandDialChange()");
                        return false;
                    }
                    _arn6BandDialSkipper++;
                    Common.DebugP("Leaving F-86F Radio SkipARN6BandDialChange()");
                    return true;
                }
                Common.DebugP("Leaving F-86F Radio SkipARN6BandDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            return false;
        }

        private bool SkipARN6ModeDialChange()
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SkipARN6ModeDialChange()");
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ARN6_MODES || _currentLowerRadioMode == CurrentF86FRadioMode.ARN6_MODES)
                {
                    if (_arn6ModeDialSkipper > 2)
                    {
                        _arn6ModeDialSkipper = 0;
                        Common.DebugP("Leaving F-86F Radio SkipARN6ModeDialChange()");
                        return false;
                    }
                    _arn6ModeDialSkipper++;
                    Common.DebugP("Leaving F-86F Radio SkipARN6ModeDialChange()");
                    return true;
                }
                Common.DebugP("Leaving F-86F Radio SkipARN6ModeDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            return false;
        }

        private bool SkipAPX6ModeDialChange()
        {
            try
            {
                Common.DebugP("Entering F-86F Radio SkipAPX6ModeDialChange()");
                if (_currentUpperRadioMode == CurrentF86FRadioMode.ADF_APX6 || _currentLowerRadioMode == CurrentF86FRadioMode.ADF_APX6)
                {
                    if (_apx6ModeDialSkipper > 2)
                    {
                        _apx6ModeDialSkipper = 0;
                        Common.DebugP("Leaving F-86F Radio SkipAPX6ModeDialChange()");
                        return false;
                    }
                    _apx6ModeDialSkipper++;
                    Common.DebugP("Leaving F-86F Radio SkipAPX6ModeDialChange()");
                    return true;
                }
                Common.DebugP("Leaving F-86F Radio SkipAPX6ModeDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78010, ex);
            }
            return false;
        }

        public override String SettingsVersion()
        {
            return "0X";
        }

    }
}
