using System.Diagnostics;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.Helpers;

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
    using NonVisuals.Panels.Saitek.Panels;




    /// <summary>
    /// Pre-programmed radio panel for the Yak-52. 
    /// </summary>
    public class RadioPanelPZ69Yak52 : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentYak52RadioMode
        {
            VHF,
            ADF_FRONT,
            ADF_REAR,
            GMK,
            NO_USE
        }

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        private CurrentYak52RadioMode _currentUpperRadioMode = CurrentYak52RadioMode.VHF;
        private CurrentYak52RadioMode _currentLowerRadioMode = CurrentYak52RadioMode.VHF;

        /*
        *  VHF RADIO 
        *  COM1 Large Mhz
        *  COM1 Small Khz
        *  Freq. Squelch on off
         * Freq. + Small Dial : Volume
        */
        private readonly object _lockVHFRadioSquelchSwitchObject1 = new();
        private readonly object _lockVHFRadioFrequencyStringObject1 = new();
        private DCSBIOSOutput _vhfRadioSquelchSwitchDcsbiosOutput;
        private volatile uint _vhfRadioSquelchSwitchCockpitPosition;

        private DCSBIOSOutput _vhfRadioFrequencyDcsbiosOutput;
        private string _vhfRadioFrequencyString = "";

        private readonly uint _vhfRadioMhzDialChangeValue = 8000; //Values gotten from ctrlref while clicking the dial in cockpit
        private readonly uint _vhfRadioKhzDialChangeValue = 8000;
        private readonly uint _vhfRadioVolumeDialChangeValue = 1000;
        private const string VHF_RADIO_SQUELCH_TOGGLE = "FRONT_VHF_RADIO_SQ";
        private const string VHF_RADIO_SQUELCH_TOGGLE_COMMAND = "FRONT_VHF_RADIO_SQ TOGGLE\n";
        private const string VHF_RADIO_MHZ_DIAL = "FRONT_VHF_RADIO_MHZ";
        private const string VHF_RADIO_KHZ_DIAL = "FRONT_VHF_RADIO_KHZ";
        private const string VHF_RADIO_VOLUME_DIAL = "FRONT_VHF_RADIO_VOL";
        private const string VHF_RADIO_FREQUENCY = "BAKLAN5_FREQ";

        /*
         *  ADF
         *  COM2 Large : Volume Dial
         *  COM2 Small : ADF Channel
         *  ACT/STBY + Dial => Rear ADF
         */
        private readonly object _lockADFFrontChannelObject1 = new();
        private DCSBIOSOutput _adfFrontChannelDialDcsbiosOutput;
        private volatile uint _adfFrontChannelCockpitPosition;
        private readonly object _lockADFRearChannelObject1 = new();
        private DCSBIOSOutput _adfRearChannelDialDcsbiosOutput;
        private volatile uint _adfRearChannelCockpitPosition;
        private const string ADF_FRONT_CHANNEL = "FRONT_RDF_CHANNEL";
        private const string ADF_FRONT_CHANNEL_INC = "FRONT_RDF_CHANNEL INC\n";
        private const string ADF_FRONT_CHANNEL_DEC = "FRONT_RDF_CHANNEL DEC\n";
        private const string ADF_FRONT_VOLUME_INC = "FRONT_RDF_VOLUME +5000\n";
        private const string ADF_FRONT_VOLUME_DEC = "FRONT_RDF_VOLUME -5000\n";
        private const string ADF_REAR_CHANNEL = "REAR_RDF_CHANNEL";
        private const string ADF_REAR_CHANNEL_INC = "REAR_RDF_CHANNEL INC\n";
        private const string ADF_REAR_CHANNEL_DEC = "REAR_RDF_CHANNEL DEC\n";
        private const string ADF_REAR_VOLUME_INC = "REAR_RDF_VOLUME +5000\n";
        private const string ADF_REAR_VOLUME_DEC = "REAR_RDF_VOLUME -5000\n";
        private readonly ClickSkipper _adfFrontClickSkipper = new ClickSkipper(2);
        private readonly ClickSkipper _adfRearClickSkipper = new ClickSkipper(2);

        /*
         * GMK-1А directional heading system
         * NAV1 Large : Heading/Course Selector Switch
         * NAV1 Small : Latitude Selector Knob
         * NAV1 ACT/STBY + Large : Hemisphere Selector Switch
         * NAV1 ACT/STBY + Small : Mode Switch
         * 
         */
        private readonly object _lockGMKLatitudeObject1 = new();
        private DCSBIOSOutput _gmkLatitudeDialDcsbiosOutput;
        private volatile uint _gmkLatitudeCockpitPosition;
        private const string GMK_LATITUDE_SELECTOR = "FRONT_SDG_LAT";
        private const string GMK_LATITUDE_SELECTOR_INC = "FRONT_SDG_LAT +2000\n";
        private const string GMK_LATITUDE_SELECTOR_DEC = "FRONT_SDG_LAT -2000\n";
        private readonly object _lockGMKHeadingSelectorObject1 = new();
        private DCSBIOSOutput _gmkHeadingSelectorDialDcsbiosOutput;
        private volatile uint _gmkHeadingSelectorCockpitPosition;
        private const string GMK_HEADING_SELECTOR = "FRONT_SDG_COURSE";
        private const string GMK_HEADING_SELECTOR_CCW = "FRONT_SDG_COURSE 0\n";
        private const string GMK_HEADING_SELECTOR_OFF = "FRONT_SDG_COURSE 1\n";
        private const string GMK_HEADING_SELECTOR_CC = "FRONT_SDG_COURSE 2\n";
        private readonly object _lockGMKHemisphereSelectorObject1 = new();
        private DCSBIOSOutput _gmkHemisphereSelectorDialDcsbiosOutput;
        private volatile uint _gmkHemisphereSelectorCockpitPosition;
        private const string GMK_HEMISPHERE_SELECTOR = "FRONT_SDG_HEMI";
        private const string GMK_HEMISPHERE_SELECTOR_INC = "FRONT_SDG_HEMI INC\n";
        private const string GMK_HEMISPHERE_SELECTOR_DEC = "FRONT_SDG_HEMI DEC\n";
        private readonly object _lockGMKModeSelectorObject1 = new();
        private DCSBIOSOutput _gmkModeSelectorDialDcsbiosOutput;
        private volatile uint _gmkModeSelectorCockpitPosition;
        private const string GMK_MODE_SELECTOR = "FRONT_SDG_MODE";
        private const string GMK_MODE_SELECTOR_INC = "FRONT_SDG_MODE INC\n";
        private const string GMK_MODE_SELECTOR_DEC = "FRONT_SDG_MODE DEC\n";


        private readonly ClickSkipper _gmkClickSkipper = new ClickSkipper(2);





        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69Yak52(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);
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
                    BIOSEventHandler.DetachStringListener(this);
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
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (_vhfRadioFrequencyDcsbiosOutput.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockVHFRadioFrequencyStringObject1)
                    {
                        _vhfRadioFrequencyString = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }
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

                // VHF Radio Squelch Switch
                if (_vhfRadioSquelchSwitchDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioSquelchSwitchObject1)
                    {
                        _vhfRadioSquelchSwitchCockpitPosition = _vhfRadioSquelchSwitchDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // Front ADF Channel
                if (_adfFrontChannelDialDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockADFFrontChannelObject1)
                    {
                        _adfFrontChannelCockpitPosition = _adfFrontChannelDialDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // Rear ADF Channel
                if (_adfRearChannelDialDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockADFRearChannelObject1)
                    {
                        _adfRearChannelCockpitPosition = _adfRearChannelDialDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // GMK Latitude
                if (_gmkLatitudeDialDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockGMKLatitudeObject1)
                    {
                        _gmkLatitudeCockpitPosition = _gmkLatitudeDialDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // GMK Mode
                if (_gmkModeSelectorDialDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockGMKModeSelectorObject1)
                    {
                        _gmkModeSelectorCockpitPosition = _gmkModeSelectorDialDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // GMK Hemisphere
                if (_gmkHemisphereSelectorDialDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockGMKHemisphereSelectorObject1)
                    {
                        _gmkHemisphereSelectorCockpitPosition = _gmkHemisphereSelectorDialDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // GMK Heading Selector
                if (_gmkHeadingSelectorDialDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockGMKHeadingSelectorObject1)
                    {
                        _gmkHeadingSelectorCockpitPosition = _gmkHeadingSelectorDialDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                        var radioPanelKnob = (RadioPanelKnobYak52)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsYak52.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.VHF);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.UPPER_ADF_FRONT:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.ADF_FRONT);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.UPPER_ADF_REAR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.ADF_REAR);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.UPPER_GMK:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.GMK);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE3:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_ADF_FRONT:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.ADF_FRONT);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_ADF_REAR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.ADF_REAR);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_GMK:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.GMK);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE3:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.NO_USE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_FREQ_SWITCH:
                                {
                                    _upperButtonPressed = radioPanelKnob.IsOn;

                                    if (_currentUpperRadioMode == CurrentYak52RadioMode.VHF)
                                    {
                                        if (!radioPanelKnob.IsOn)
                                        {
                                            if (!_upperButtonPressedAndDialRotated)
                                            {
                                                // Do not synch if user has pressed the button to configure the radio
                                                // Do when user releases button
                                                DCSBIOS.Send(VHF_RADIO_SQUELCH_TOGGLE_COMMAND);
                                            }

                                            _upperButtonPressedAndDialRotated = false;
                                        }
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_FREQ_SWITCH:
                                {
                                    _lowerButtonPressed = radioPanelKnob.IsOn;

                                    if (_currentLowerRadioMode == CurrentYak52RadioMode.VHF)
                                    {
                                        if (!radioPanelKnob.IsOn)
                                        {
                                            if (!_lowerButtonPressedAndDialRotated)
                                            {
                                                // Do not synch if user has pressed the button to configure the radio
                                                // Do when user releases button
                                                DCSBIOS.Send(VHF_RADIO_SQUELCH_TOGGLE_COMMAND);
                                            }

                                            _lowerButtonPressedAndDialRotated = false;
                                        }
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                DCSAircraft.SelectedAircraft.Description,
                                HIDInstance,
                                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_Yak52,
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
                Logger.Error(ex);
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
                    var radioPanelKnobYak52 = (RadioPanelKnobYak52)o;
                    if (radioPanelKnobYak52.IsOn)
                    {
                        switch (radioPanelKnobYak52.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(true);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                DCSBIOS.Send(ADF_FRONT_VOLUME_INC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                DCSBIOS.Send(ADF_REAR_VOLUME_INC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                        {
                                            DCSBIOS.Send(_upperButtonPressed ? GMK_HEMISPHERE_SELECTOR_INC : GetGMKHeadingSelectorCommand(true));
                                            break;
                                        }
                                        case CurrentYak52RadioMode.NO_USE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(false);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                DCSBIOS.Send(ADF_FRONT_VOLUME_DEC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                DCSBIOS.Send(ADF_REAR_VOLUME_DEC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                DCSBIOS.Send(_upperButtonPressed ? GMK_HEMISPHERE_SELECTOR_INC : GetGMKHeadingSelectorCommand(false));
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(true);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(true);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                if (_adfFrontClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_FRONT_CHANNEL_INC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                if (_adfRearClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_REAR_CHANNEL_INC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    DCSBIOS.Send(GMK_MODE_SELECTOR_INC);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(GMK_LATITUDE_SELECTOR_INC);
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(false);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(false);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                if (_adfFrontClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_FRONT_CHANNEL_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                if (_adfRearClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_REAR_CHANNEL_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    DCSBIOS.Send(GMK_MODE_SELECTOR_DEC);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(GMK_LATITUDE_SELECTOR_DEC);
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(true);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                DCSBIOS.Send(ADF_FRONT_VOLUME_INC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                DCSBIOS.Send(ADF_REAR_VOLUME_INC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                DCSBIOS.Send(_upperButtonPressed ? GMK_HEMISPHERE_SELECTOR_INC : GetGMKHeadingSelectorCommand(true));
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(false);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                DCSBIOS.Send(ADF_FRONT_VOLUME_DEC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                DCSBIOS.Send(ADF_REAR_VOLUME_DEC);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                DCSBIOS.Send(_upperButtonPressed ? GMK_HEMISPHERE_SELECTOR_INC : GetGMKHeadingSelectorCommand(false));
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(true);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(true);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                if (_adfFrontClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_FRONT_CHANNEL_INC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                if (_adfRearClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_REAR_CHANNEL_INC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    DCSBIOS.Send(GMK_MODE_SELECTOR_INC);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(GMK_LATITUDE_SELECTOR_INC);
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(false);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(false);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_FRONT:
                                            {
                                                if (_adfFrontClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_FRONT_CHANNEL_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF_REAR:
                                            {
                                                if (_adfRearClickSkipper.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(ADF_REAR_CHANNEL_DEC);
                                                }

                                                break;
                                            }
                                        case CurrentYak52RadioMode.GMK:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    DCSBIOS.Send(GMK_MODE_SELECTOR_INC);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(GMK_LATITUDE_SELECTOR_INC);
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
                Logger.Error(ex);
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
                        case CurrentYak52RadioMode.VHF:
                            {
                                if (_upperButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    break;
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, _vhfRadioFrequencyString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioSquelchSwitchCockpitPosition, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentYak52RadioMode.ADF_FRONT:
                            {
                                lock (_lockADFFrontChannelObject1)
                                {
                                    lock (_lockADFRearChannelObject1)
                                    {
                                        SetPZ69DisplayBytesUnsignedInteger(ref bytes, _adfFrontChannelCockpitPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    }
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.ADF_REAR:
                            {
                                lock (_lockADFFrontChannelObject1)
                                {
                                    lock (_lockADFRearChannelObject1)
                                    {
                                        SetPZ69DisplayBytesUnsignedInteger(ref bytes, _adfRearChannelCockpitPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    }
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.GMK:
                            {
                                lock (_lockGMKHeadingSelectorObject1)
                                {
                                    lock (_lockGMKHemisphereSelectorObject1)
                                    {
                                        lock (_lockGMKLatitudeObject1)
                                        {
                                            lock (_lockGMKModeSelectorObject1)
                                            {
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _upperButtonPressed ? _gmkHemisphereSelectorCockpitPosition : _gmkHeadingSelectorCockpitPosition, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _upperButtonPressed ? _gmkModeSelectorCockpitPosition : _gmkLatitudeCockpitPosition, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.NO_USE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentYak52RadioMode.VHF:
                            {
                                if (_lowerButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    break;
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, _vhfRadioFrequencyString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioSquelchSwitchCockpitPosition, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentYak52RadioMode.ADF_FRONT:
                            {
                                lock (_lockADFFrontChannelObject1)
                                {
                                    lock (_lockADFRearChannelObject1)
                                    {
                                        SetPZ69DisplayBytesUnsignedInteger(ref bytes, _adfFrontChannelCockpitPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    }
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.ADF_REAR:
                            {
                                lock (_lockADFFrontChannelObject1)
                                {
                                    lock (_lockADFRearChannelObject1)
                                    {
                                        SetPZ69DisplayBytesUnsignedInteger(ref bytes, _adfRearChannelCockpitPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    }
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.GMK:
                            {
                                lock (_lockGMKHeadingSelectorObject1)
                                {
                                    lock (_lockGMKHemisphereSelectorObject1)
                                    {
                                        lock (_lockGMKLatitudeObject1)
                                        {
                                            lock (_lockGMKModeSelectorObject1)
                                            {
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lowerButtonPressed ? _gmkHemisphereSelectorCockpitPosition : _gmkHeadingSelectorCockpitPosition, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lowerButtonPressed ? _gmkModeSelectorCockpitPosition : _gmkLatitudeCockpitPosition, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.NO_USE:
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
                Logger.Error(ex);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }


        public sealed override void Startup()
        {
            try
            {
                // VHF
                _vhfRadioSquelchSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(VHF_RADIO_SQUELCH_TOGGLE);
                _vhfRadioFrequencyDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(VHF_RADIO_FREQUENCY);
                DCSBIOSStringManager.AddListeningAddress(_vhfRadioFrequencyDcsbiosOutput);

                // ADF
                _adfFrontChannelDialDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(ADF_FRONT_CHANNEL);
                _adfRearChannelDialDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(ADF_REAR_CHANNEL);

                // GMK
                _gmkHeadingSelectorDialDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(GMK_HEADING_SELECTOR);
                _gmkHemisphereSelectorDialDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(GMK_HEMISPHERE_SELECTOR);
                _gmkModeSelectorDialDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(GMK_MODE_SELECTOR);
                _gmkLatitudeDialDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(GMK_LATITUDE_SELECTOR);

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobYak52.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentYak52RadioMode currentYak52RadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentYak52RadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentYak52RadioMode currentYak52RadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentYak52RadioMode;
                // If NO_USE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendVHFMhzCommand(bool increase)
        {
            var s = GetVHFRadioMhzDialStringCommand(increase);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            DCSBIOS.Send(s);
        }

        private void SendVHFKhzCommand(bool increase)
        {
            var s = GetVHFRadioKhzDialStringCommand(increase);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            DCSBIOS.Send(s);
        }
        private void SendVHFVolumeCommand(bool increase)
        {
            var s = GetVHFRadioVolumeDialStringCommand(increase);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            DCSBIOS.Send(s);
        }

        private string GetVHFRadioMhzDialStringCommand(bool moveUp)
        {
            if (moveUp)
            {

                return $"{VHF_RADIO_MHZ_DIAL} {_vhfRadioMhzDialChangeValue} \n";
            }
            return $"{VHF_RADIO_MHZ_DIAL} -{_vhfRadioMhzDialChangeValue} \n";
        }

        private string GetVHFRadioKhzDialStringCommand(bool moveUp)
        {
            if (moveUp)
            {

                return $"{VHF_RADIO_KHZ_DIAL} {_vhfRadioKhzDialChangeValue}\n";
            }
            return $"{VHF_RADIO_KHZ_DIAL} -{_vhfRadioKhzDialChangeValue} \n";
        }

        private string GetVHFRadioVolumeDialStringCommand(bool moveUp)
        {
            if (moveUp)
            {

                return $"{VHF_RADIO_VOLUME_DIAL} {_vhfRadioVolumeDialChangeValue} \n";
            }
            return $"{VHF_RADIO_VOLUME_DIAL} -{_vhfRadioVolumeDialChangeValue} \n";
        }

        private string GetGMKHeadingSelectorCommand(bool moveUp)
        {
            /*
             * Self returning toggle in cockpit
             * 0 => 1 <= 2
             * Must emulate same here
             */
            if (moveUp)
            {
                return _gmkHeadingSelectorCockpitPosition switch
                {
                    0 => GMK_HEADING_SELECTOR_OFF,
                    1 => GMK_HEADING_SELECTOR_CC,
                    2 => GMK_HEADING_SELECTOR_CC,
                    _ => ""
                };
            }

            return _gmkHeadingSelectorCockpitPosition switch
            {
                2 => GMK_HEADING_SELECTOR_OFF,
                1 => GMK_HEADING_SELECTOR_CCW,
                0 => GMK_HEADING_SELECTOR_CCW,
                _ => ""
            };
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}
