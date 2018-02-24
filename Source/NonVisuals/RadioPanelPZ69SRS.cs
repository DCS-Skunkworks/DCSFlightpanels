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


        /*Radio1 COM1*/
        /*Radio2 COM2*/
        /*Radio3 NAV1*/
        /*Radio4 NAV2*/
        /*Radio5 ADF*/
        /*Radio6 DME*/
        /*Radio7 XPDR*/
        //Large dial
        //Small dial
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
                            case RadioPanelPZ69KnobsSRS.UPPER_COM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.COM1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_COM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.COM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NAV1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NAV1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NAV2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NAV2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_DME:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.DME);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_XPDR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.XPDR);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_COM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.COM1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_COM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.COM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NAV1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NAV1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NAV2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NAV2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_DME:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.DME);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_XPDR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.XPDR);
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
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH:
                                {
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                                        case CurrentSRSRadioMode.COM1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.COM2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV1:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.NAV2:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.ADF:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.DME:
                                            {
                                                break;
                                            }
                                        case CurrentSRSRadioMode.XPDR:
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
                        case CurrentSRSRadioMode.COM1:
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
                        case CurrentSRSRadioMode.COM2:
                        case CurrentSRSRadioMode.NAV1:
                        case CurrentSRSRadioMode.NAV2:
                        case CurrentSRSRadioMode.ADF:
                        case CurrentSRSRadioMode.DME:
                        case CurrentSRSRadioMode.XPDR:
                            {
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentSRSRadioMode.COM1:
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
                        case CurrentSRSRadioMode.COM2:
                        case CurrentSRSRadioMode.NAV1:
                        case CurrentSRSRadioMode.NAV2:
                        case CurrentSRSRadioMode.ADF:
                        case CurrentSRSRadioMode.DME:
                        case CurrentSRSRadioMode.XPDR:
                            {
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
                
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
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
        /*
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
        */

        public override String SettingsVersion()
        {
            return "0X";
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
            }
            catch (Exception ex)
            {
                Common.LogError(82001, ex);
            }
        }

    }
}
