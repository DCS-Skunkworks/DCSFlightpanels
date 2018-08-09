using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69Bf109 : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobBf109> _radioPanelKnobs = new HashSet<RadioPanelKnobBf109>();
        private CurrentBf109RadioMode _currentUpperRadioMode = CurrentBf109RadioMode.FUG16ZY;
        private CurrentBf109RadioMode _currentLowerRadioMode = CurrentBf109RadioMode.FUG16ZY;

        /*FuG 16ZY*/
        /*38.4 and 42.4 MHz*/
        /*
         *  COM1 Large Freq Sel
         *  COM1 Small Fine Tune
         *  Freq. Selector
         *  Fine Tuning
         *  I Management frequency Withing Squadron
         *  II Group Order frequency Different squadrons
         *  Δ Air Traffic Control frequency
         *  □ Reich Fighter Defense Frequency (Country Wide)
         *  Homing:
         *  FT FT
         *  Y ZF
         */

        /*FuG 25a*/
        /*125 +/-1.8 MHz*/

        /* 
        * 
        *  COM1 Large Freq Sel
        *  COM1 Small Fine Tune
        *  COM2 Large IFF Control Switch
        *  COM2 Small Volume
        *  COM2 ACT/STBY IFF Test Button
        *  IFF Control Switch
        *  IFF Test Button
        *  Volume
        *  NAV1
        *  Homing Switch         
        */

        /*FuG 16ZY COM1*/
        //Large dial 0-3 [step of 1]
        //Small dial Fine tuning
        private readonly ClickSpeedDetector _fineTuneIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _fineTuneDecreaseChangeMonitor = new ClickSpeedDetector(20);
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

        /*Bf 109 FuG 25a IFF COM2*/
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

        /*Bf 109 FuG 16ZY Homing Switch NAV1*/
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

        public RadioPanelPZ69Bf109(HIDSkeleton hidSkeleton, bool enableDCSBIOS = true) : base(hidSkeleton, enableDCSBIOS)
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
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
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



                //FuG 16ZY Preset Channel Dial
                if (e.Address == _fug16ZyPresetDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockFug16ZyPresetDialObject1)
                    {
                        var tmp = _fug16ZyPresetCockpitDialPos;
                        _fug16ZyPresetCockpitDialPos = _fug16ZyPresetDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _fug16ZyPresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //FuG 16ZY Fine Tune Dial
                if (e.Address == _fug16ZyFineTuneDcsbiosOutputDial.Address)
                {
                    lock (_lockFug16ZyFineTuneDialObject1)
                    {
                        var tmp = _fug16ZyFineTuneCockpitDialPos;
                        _fug16ZyFineTuneCockpitDialPos = _fug16ZyFineTuneDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _fug16ZyFineTuneCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //FuG 25A IFF Channel Dial
                if (e.Address == _fug25aIFFDcsbiosOutputDial.Address)
                {
                    lock (_lockFUG25AIFFDialObject1)
                    {
                        var tmp = _fug25aIFFCockpitDialPos;
                        _fug25aIFFCockpitDialPos = _fug25aIFFDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _fug25aIFFCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //FuG 16ZY Homing Switch
                if (e.Address == _homingDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockHomingDialObject1)
                    {
                        var tmp = _homingCockpitDialPos;
                        _homingCockpitDialPos = _homingDcsbiosOutputPresetDial.GetUIntValue(e.Data);
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
                Common.DebugP("Entering Bf 109 Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobBf109)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsBf109.UPPER_FUG16ZY:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentBf109RadioMode.FUG16ZY);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentBf109RadioMode.IFF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_HOMING:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentBf109RadioMode.HOMING);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_NOUSE1:
                            case RadioPanelPZ69KnobsBf109.UPPER_NOUSE2:
                            case RadioPanelPZ69KnobsBf109.UPPER_NOUSE3:
                            case RadioPanelPZ69KnobsBf109.UPPER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentBf109RadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_FUG16ZY:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentBf109RadioMode.FUG16ZY);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_IFF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentBf109RadioMode.IFF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_HOMING:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentBf109RadioMode.HOMING);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_NOUSE1:
                            case RadioPanelPZ69KnobsBf109.LOWER_NOUSE2:
                            case RadioPanelPZ69KnobsBf109.LOWER_NOUSE3:
                            case RadioPanelPZ69KnobsBf109.LOWER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentBf109RadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentBf109RadioMode.IFF)
                                    {
                                        DCSBIOS.Send(radioPanelKnob.IsOn ? FuG25ATestCommandInc : FuG25ATestCommandDec);
                                    }
                                    if (_currentUpperRadioMode == CurrentBf109RadioMode.HOMING)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            lock (_lockHomingDialObject1)
                                            {
                                                DCSBIOS.Send(_homingCockpitDialPos == 1 ? HomingCommandDec : HomingCommandInc);
                                            }
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentBf109RadioMode.IFF)
                                    {
                                        DCSBIOS.Send(radioPanelKnob.IsOn ? FuG25ATestCommandInc : FuG25ATestCommandDec);
                                    }
                                    if (_currentLowerRadioMode == CurrentBf109RadioMode.HOMING)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            lock (_lockHomingDialObject1)
                                            {
                                                DCSBIOS.Send(_homingCockpitDialPos == 1 ? HomingCommandDec : HomingCommandInc);
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
            Common.DebugP("Leaving Bf 109 Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Bf 109 Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobBf109 = (RadioPanelKnobBf109)o;
                    if (radioPanelKnobBf109.IsOn)
                    {
                        switch (radioPanelKnobBf109.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneIncreaseChangeMonitor.Click();
                                                if (_fineTuneIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                DCSBIOS.Send(changeFaster ? Fug16ZyFineTuneCommandIncMore : Fug16ZyFineTuneCommandInc);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneDecreaseChangeMonitor.Click();
                                                if (_fineTuneDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }

                                                DCSBIOS.Send(changeFaster ? Fug16ZyFineTuneCommandDecMore : Fug16ZyFineTuneCommandDec);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Presets
                                                if (!SkipFuG16ZYPresetDialChange())
                                                {
                                                    DCSBIOS.Send(Fug16ZyPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                if (!SkipIFFDialChange())
                                                {
                                                    DCSBIOS.Send(FUG25AIFFCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneIncreaseChangeMonitor.Click();
                                                if (_fineTuneIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }

                                                DCSBIOS.Send(changeFaster ? Fug16ZyFineTuneCommandIncMore : Fug16ZyFineTuneCommandInc);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentBf109RadioMode.FUG16ZY:
                                            {
                                                //Fine tuning
                                                var changeFaster = false;
                                                _fineTuneDecreaseChangeMonitor.Click();
                                                if (_fineTuneDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }

                                                DCSBIOS.Send(changeFaster ? Fug16ZyFineTuneCommandDecMore : Fug16ZyFineTuneCommandDec);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.IFF:
                                            {
                                                DCSBIOS.Send(RadioVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentBf109RadioMode.HOMING:
                                            {
                                                break;
                                            }
                                        case CurrentBf109RadioMode.NOUSE:
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
            Common.DebugP("Leaving Bf 109 Radio AdjustFrequency()");
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

                    Common.DebugP("Entering Bf 109 Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentBf109RadioMode.FUG16ZY:
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeDialPostionAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(fineTunePositionAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentBf109RadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                var positionAsString = "";
                                lock (_lockFUG25AIFFDialObject1)
                                {
                                    positionAsString = (_fug25aIFFCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentBf109RadioMode.HOMING:
                            {
                                //Switch
                                //0-1

                                var positionAsString = "";
                                lock (_lockHomingDialObject1)
                                {
                                    positionAsString = (_homingCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentBf109RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentBf109RadioMode.FUG16ZY:
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(modeDialPostionAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(fineTunePositionAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentBf109RadioMode.IFF:
                            {
                                //Preset Channel Selector
                                //0-1

                                var positionAsString = "";
                                lock (_lockFUG25AIFFDialObject1)
                                {
                                    positionAsString = (_fug25aIFFCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentBf109RadioMode.HOMING:
                            {
                                //Switch
                                //0-1

                                var positionAsString = "";
                                lock (_lockHomingDialObject1)
                                {
                                    positionAsString = (_homingCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(positionAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentBf109RadioMode.NOUSE:
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
                Common.LogError(82011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving Bf 109 Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering Bf 109 Radio OnReport()");
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
                                    var knob = (RadioPanelKnobBf109)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobBf109)radioPanelKnob));
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
            Common.DebugP("Leaving Bf 109 Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering Bf 109 Radio GetHashSetOfChangedKnobs()");


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
            Common.DebugP("Leaving Bf 109 Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("Bf 109");

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
                Common.DebugP("RadioPanelPZ69Bf109.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering Bf 109 Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving Bf 109 Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobBf109.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobBf109 radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentBf109RadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering Bf 109 Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentBf109RadioMode);
                _currentUpperRadioMode = currentBf109RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(82014, ex);
            }
            Common.DebugP("Leaving Bf 109 Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentBf109RadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering Bf 109 Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentBf109RadioMode);
                _currentLowerRadioMode = currentBf109RadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(82015, ex);
            }
            Common.DebugP("Leaving Bf 109 Radio SetLowerRadioMode()");
        }

        private bool SkipFuG16ZYPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Bf 109 Radio SkipFuG16ZYPresetDialChange()");
                if (_currentUpperRadioMode == CurrentBf109RadioMode.FUG16ZY || _currentLowerRadioMode == CurrentBf109RadioMode.FUG16ZY)
                {
                    if (_fug16ZyPresetDialSkipper > 2)
                    {
                        _fug16ZyPresetDialSkipper = 0;
                        Common.DebugP("Leaving Bf 109 Radio SkipFuG16ZYPresetDialChange()");
                        return false;
                    }
                    _fug16ZyPresetDialSkipper++;
                    Common.DebugP("Leaving Bf 109 Radio SkipFuG16ZYPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Bf 109 Radio SkipFuG16ZYPresetDialChange()");
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
                Common.DebugP("Entering Bf 109 Radio SkipIFFDialChange()");
                if (_currentUpperRadioMode == CurrentBf109RadioMode.IFF || _currentLowerRadioMode == CurrentBf109RadioMode.IFF)
                {
                    if (_fug25aIFFDialSkipper > 2)
                    {
                        _fug25aIFFDialSkipper = 0;
                        Common.DebugP("Leaving Bf 109 Radio SkipIFFDialChange()");
                        return false;
                    }
                    _fug25aIFFDialSkipper++;
                    Common.DebugP("Leaving Bf 109 Radio SkipIFFDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Bf 109 Radio SkipIFFDialChange()");
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
                Common.DebugP("Entering Bf 109 Radio SkipHomingPresetDialChange()");
                if (_currentUpperRadioMode == CurrentBf109RadioMode.HOMING || _currentLowerRadioMode == CurrentBf109RadioMode.HOMING)
                {
                    if (_homingDialSkipper > 2)
                    {
                        _homingDialSkipper = 0;
                        Common.DebugP("Leaving Bf 109 Radio SkipHomingPresetDialChange()");
                        return false;
                    }
                    _homingDialSkipper++;
                    Common.DebugP("Leaving Bf 109 Radio SkipHomingPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Bf 109 Radio SkipHomingPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(82110, ex);
            }
            return false;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }

    }
}
