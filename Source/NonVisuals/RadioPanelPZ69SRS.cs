using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69SRS : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobSRS> _radioPanelKnobs = new HashSet<RadioPanelKnobSRS>();
        private CurrentSRSRadioMode _currentUpperRadioMode = CurrentSRSRadioMode.COM1;
        private CurrentSRSRadioMode _currentLowerRadioMode = CurrentSRSRadioMode.COM1;
        

        /*FuG 16ZY COM1*/
        //Large dial 0-3 [step of 1]
        //Small dial Fine tuning
        private ClickSpeedDetector _fineTuneIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _fineTuneDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly object _lockFug16ZyPresetDialObject1 = new object();
        private DCSBIOSOutput _fug16ZyPresetDcsbiosOutputPresetDial;
        private volatile uint _fug16ZyPresetCockpitDialPos = 1;
        private const string Fug16ZyPresetCommandInc = "RADIO_MODE INC\n";
        private const string Fug16ZyPresetCommandDec = "RADIO_MODE DEC\n";
        private int _fug16ZyPresetDialSkipper;
        private readonly object _lockFug16ZyFineTuneDialObject1 = new object();
        private DCSBIOSOutput _fug16ZyFineTuneDcsbiosOutputDial;
        private volatile uint _fug16ZyFineTuneCockpitDialPos = 1;
        private const string Fug16ZyFineTuneCommandInc = "FUG16_TUNING +300\n";
        private const string Fug16ZyFineTuneCommandDec = "FUG16_TUNING -300\n";
        private const string Fug16ZyFineTuneCommandIncMore = "FUG16_TUNING +3000\n";
        private const string Fug16ZyFineTuneCommandDecMore = "FUG16_TUNING -3000\n";

        /*SRS FuG 25a IFF COM2*/
        //Large dial 0-1 [step of 1]
        //Small dial Volume control
        //ACT/STBY IFF Test Button
        private readonly object _lockFUG25AIFFDialObject1 = new object();
        private DCSBIOSOutput _fug25aIFFDcsbiosOutputDial;
        private volatile uint _fug25aIFFCockpitDialPos = 1;
        private const string FUG25AIFFCommandInc = "FUG25_MODE INC\n";
        private const string FUG25AIFFCommandDec = "FUG25_MODE DEC\n";
        private int _fug25aIFFDialSkipper;
        private const string RadioVolumeKnobCommandInc = "FUG16_VOLUME +2500\n";
        private const string RadioVolumeKnobCommandDec = "FUG16_VOLUME -2500\n";
        private const string FuG25ATestCommandInc = "FUG25_TEST INC\n";
        private const string FuG25ATestCommandDec = "FUG25_TEST DEC\n";

        /*SRS FuG 16ZY Homing Switch NAV1*/
        //Large dial N/A
        //Small dial N/A
        //ACT/STBY Homing Switch
        private readonly object _lockHomingDialObject1 = new object();
        private DCSBIOSOutput _homingDcsbiosOutputPresetDial;
        private volatile uint _homingCockpitDialPos = 1;
        private const string HomingCommandInc = "FT_ZF_SWITCH INC\n";
        private const string HomingCommandDec = "FT_ZF_SWITCH DEC\n";
        private int _homingDialSkipper;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SRS(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }


        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {
            }
            catch (Exception e)
            {
                Common.LogError(78030, e, "DCSBIOSStringReceived()");
            }
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            try
            {

                UpdateCounter(address, data);
                /*
                 * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
                 * Once a dial has been deemed to be "off" position and needs to be changed
                 * a change command is sent to DCS-BIOS.
                 * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
                 * reset. Reading the dial's position with no change in value will not reset.
                 */



                //FuG 16ZY Preset Channel Dial
                if (address == _fug16ZyPresetDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockFug16ZyPresetDialObject1)
                    {
                        var tmp = _fug16ZyPresetCockpitDialPos;
                        _fug16ZyPresetCockpitDialPos = _fug16ZyPresetDcsbiosOutputPresetDial.GetUIntValue(data);
                        if (tmp != _fug16ZyPresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //FuG 16ZY Fine Tune Dial
                if (address == _fug16ZyFineTuneDcsbiosOutputDial.Address)
                {
                    lock (_lockFug16ZyFineTuneDialObject1)
                    {
                        var tmp = _fug16ZyFineTuneCockpitDialPos;
                        _fug16ZyFineTuneCockpitDialPos = _fug16ZyFineTuneDcsbiosOutputDial.GetUIntValue(data);
                        if (tmp != _fug16ZyFineTuneCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //FuG 25A IFF Channel Dial
                if (address == _fug25aIFFDcsbiosOutputDial.Address)
                {
                    lock (_lockFUG25AIFFDialObject1)
                    {
                        var tmp = _fug25aIFFCockpitDialPos;
                        _fug25aIFFCockpitDialPos = _fug25aIFFDcsbiosOutputDial.GetUIntValue(data);
                        if (tmp != _fug25aIFFCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //FuG 16ZY Homing Switch
                if (address == _homingDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockHomingDialObject1)
                    {
                        var tmp = _homingCockpitDialPos;
                        _homingCockpitDialPos = _homingDcsbiosOutputPresetDial.GetUIntValue(data);
                        if (tmp != _homingCockpitDialPos)
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
                Common.LogError(82001, ex);
            }
        }


        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering SRS Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (_lockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobSRS)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSRS.UPPER_FUG16ZY:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.FUG16ZY);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.IFF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_HOMING:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.HOMING);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NOUSE1:
                            case RadioPanelPZ69KnobsSRS.UPPER_NOUSE2:
                            case RadioPanelPZ69KnobsSRS.UPPER_NOUSE3:
                            case RadioPanelPZ69KnobsSRS.UPPER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_FUG16ZY:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.FUG16ZY);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.IFF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_HOMING:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.HOMING);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NOUSE1:
                            case RadioPanelPZ69KnobsSRS.LOWER_NOUSE2:
                            case RadioPanelPZ69KnobsSRS.LOWER_NOUSE3:
                            case RadioPanelPZ69KnobsSRS.LOWER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentSRSRadioMode.IFF)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(FuG25ATestCommandInc);
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(FuG25ATestCommandDec);
                                        }
                                    }
                                    if (_currentUpperRadioMode == CurrentSRSRadioMode.HOMING)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            lock (_lockHomingDialObject1)
                                            {
                                                if (_homingCockpitDialPos == 1)
                                                {
                                                    DCSBIOS.Send(HomingCommandDec);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(HomingCommandInc);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentSRSRadioMode.IFF)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(FuG25ATestCommandInc);
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(FuG25ATestCommandDec);
                                        }
                                    }
                                    if (_currentLowerRadioMode == CurrentSRSRadioMode.HOMING)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            lock (_lockHomingDialObject1)
                                            {
                                                if (_homingCockpitDialPos == 1)
                                                {
                                                    DCSBIOS.Send(HomingCommandDec);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(HomingCommandInc);
                                                }
                                            }
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
                Common.LogError(82006, ex);
            }
            Common.DebugP("Leaving SRS Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering SRS Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobSRS = (RadioPanelKnobSRS)o;
                    if (radioPanelKnobSRS.IsOn)
                    {
                        switch (radioPanelKnobSRS.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneIncreaseChangeMonitor.Click();
                                                if (_fineTuneIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandIncMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneDecreaseChangeMonitor.Click();
                                                if (_fineTuneDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandDecMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneIncreaseChangeMonitor.Click();
                                                if (_fineTuneIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandIncMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentSRSRadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneDecreaseChangeMonitor.Click();
                                                if (_fineTuneDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandDecMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(Fug16ZyFineTuneCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentSRSRadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentSRSRadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NOUSE:
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
                Common.LogError(82007, ex);
            }
            Common.DebugP("Leaving SRS Radio AdjustFrequency()");
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

                    Common.DebugP("Entering SRS Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentSRSRadioMode.FUG16ZY:
                            {
                                //1-4
                                var modeDialPostionAsString = "";
                                var fineTunePositionAsString = "";
                                lock (_lockFug16ZyPresetDialObject1)
                                {
                                    modeDialPostionAsString = (_fug16ZyPresetCockpitDialPos + 1).ToString();
                                }
                                lock (_lockFug16ZyFineTuneDialObject1)
                                {

                                    fineTunePositionAsString = (_fug16ZyFineTuneCockpitDialPos / 10).ToString();
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeDialPostionAsString), PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(fineTunePositionAsString), PZ69LCDPosition.UPPER_RIGHT);
                                break;
                            }
                        case CurrentSRSRadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                var positionAsString = "";
                                lock (_lockFUG25AIFFDialObject1)
                                {
                                    positionAsString = (_fug25aIFFCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }

                        case CurrentSRSRadioMode.HOMING:
                            {
                                //Switch
                                //0-1

                                var positionAsString = "";
                                lock (_lockHomingDialObject1)
                                {
                                    positionAsString = (_homingCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentSRSRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentSRSRadioMode.FUG16ZY:
                            {
                                //1-4
                                var modeDialPostionAsString = "";
                                var fineTunePositionAsString = "";
                                lock (_lockFug16ZyPresetDialObject1)
                                {
                                    modeDialPostionAsString = (_fug16ZyPresetCockpitDialPos + 1).ToString();
                                }
                                lock (_lockFug16ZyFineTuneDialObject1)
                                {

                                    fineTunePositionAsString = (_fug16ZyFineTuneCockpitDialPos / 10).ToString();
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeDialPostionAsString), PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(fineTunePositionAsString), PZ69LCDPosition.LOWER_RIGHT);
                                break;
                            }
                        case CurrentSRSRadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                var positionAsString = "";
                                lock (_lockFUG25AIFFDialObject1)
                                {
                                    positionAsString = (_fug25aIFFCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }

                        case CurrentSRSRadioMode.HOMING:
                            {
                                //Switch
                                //0-1

                                var positionAsString = "";
                                lock (_lockHomingDialObject1)
                                {
                                    positionAsString = (_homingCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentSRSRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_RIGHT);
                                break;
                            }
                    }
                    SendLCDData(bytes);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving SRS Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering SRS Radio OnReport()");
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
                                    var knob = (RadioPanelKnobSRS)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobSRS)radioPanelKnob));
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
                Common.LogError(82012, ex);
            }
            Common.DebugP("Leaving SRS Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering SRS Radio GetHashSetOfChangedKnobs()");


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
                Common.LogError(82013, ex);
            }
            Common.DebugP("Leaving SRS Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("SRS");

                //COM1
                _fug16ZyPresetDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RADIO_MODE");
                _fug16ZyFineTuneDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("FUG16_TUNING");

                //COM2
                _fug25aIFFDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("FUG25_MODE");

                //NAV1
                _homingDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("FT_ZF_SWITCH");


                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69SRS.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering SRS Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving SRS Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobSRS.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobSRS radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering SRS Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentBf109RadioMode);
                _currentUpperRadioMode = currentBf109RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(82014, ex);
            }
            Common.DebugP("Leaving SRS Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering SRS Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentBf109RadioMode);
                _currentLowerRadioMode = currentBf109RadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(82015, ex);
            }
            Common.DebugP("Leaving SRS Radio SetLowerRadioMode()");
        }

        private bool SkipFuG16ZYPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipFuG16ZYPresetDialChange()");
                if (_currentUpperRadioMode == CurrentSRSRadioMode.FUG16ZY || _currentLowerRadioMode == CurrentSRSRadioMode.FUG16ZY)
                {
                    if (_fug16ZyPresetDialSkipper > 2)
                    {
                        _fug16ZyPresetDialSkipper = 0;
                        Common.DebugP("Leaving SRS Radio SkipFuG16ZYPresetDialChange()");
                        return false;
                    }
                    _fug16ZyPresetDialSkipper++;
                    Common.DebugP("Leaving SRS Radio SkipFuG16ZYPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving SRS Radio SkipFuG16ZYPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(82009, ex);
            }
            return false;
        }

        private bool SkipIFFDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipIFFDialChange()");
                if (_currentUpperRadioMode == CurrentSRSRadioMode.IFF || _currentLowerRadioMode == CurrentSRSRadioMode.IFF)
                {
                    if (_fug25aIFFDialSkipper > 2)
                    {
                        _fug25aIFFDialSkipper = 0;
                        Common.DebugP("Leaving SRS Radio SkipIFFDialChange()");
                        return false;
                    }
                    _fug25aIFFDialSkipper++;
                    Common.DebugP("Leaving SRS Radio SkipIFFDialChange()");
                    return true;
                }
                Common.DebugP("Leaving SRS Radio SkipIFFDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(82015, ex);
            }
            return false;
        }

        private bool SkipHomingPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipHomingPresetDialChange()");
                if (_currentUpperRadioMode == CurrentSRSRadioMode.HOMING || _currentLowerRadioMode == CurrentSRSRadioMode.HOMING)
                {
                    if (_homingDialSkipper > 2)
                    {
                        _homingDialSkipper = 0;
                        Common.DebugP("Leaving SRS Radio SkipHomingPresetDialChange()");
                        return false;
                    }
                    _homingDialSkipper++;
                    Common.DebugP("Leaving SRS Radio SkipHomingPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving SRS Radio SkipHomingPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(82110, ex);
            }
            return false;
        }

        public override String SettingsVersion()
        {
            return "0X";
        }

    }
}
