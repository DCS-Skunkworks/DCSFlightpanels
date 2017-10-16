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
    public class RadioPanelPZ69Mi8 : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobMi8> _radioPanelKnobs = new HashSet<RadioPanelKnobMi8>();
        private CurrentMi8RadioMode _currentUpperRadioMode = CurrentMi8RadioMode.R863_MANUAL;
        private CurrentMi8RadioMode _currentLowerRadioMode = CurrentMi8RadioMode.R863_MANUAL;

        /*Mi-8 VHF/UHF R-863 MANUAL COM1*/
        //Large dial 100-149  -> 220 - 399 [step of 1]
        //Small dial 0 - 95
        private ClickSpeedDetector _bigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _bigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        const int ChangeValue = 10;
        //private int[] _r863ManualFreq1DialValues = { 10, 11, 12, 13, 14, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 };
        private volatile uint _r863ManualBigFrequencyStandby = 108;
        private volatile uint _r863ManualSmallFrequencyStandby;
        private volatile uint _r863ManualSavedCockpitBigFrequency;
        private volatile uint _r863ManualSavedCockpitSmallFrequency;
        private object _lockR863ManualDialsObject1 = new object();
        private object _lockR863ManualDialsObject2 = new object();
        private object _lockR863ManualDialsObject3 = new object();
        private object _lockR863ManualDialsObject4 = new object();
        private volatile uint _r863ManualCockpitFreq1DialPos = 1;
        private volatile uint _r863ManualCockpitFreq2DialPos = 1;
        private volatile uint _r863ManualCockpitFreq3DialPos = 1;
        private volatile uint _r863ManualCockpitFreq4DialPos = 1;
        private double _r863ManualCockpitFrequency = 100.000;
        private DCSBIOSOutput _r863ManualDcsbiosOutputCockpitFrequency;
        private const string R863ManualFreq1DialCommand = "R863_FREQ1 ";
        private const string R863ManualFreq2DialCommand = "R863_FREQ2 ";
        private const string R863ManualFreq3DialCommand = "R863_FREQ3 ";
        private const string R863ManualFreq4DialCommand = "R863_FREQ4 ";
        private Thread _r863ManualSyncThread;
        private long _r863ManualThreadNowSynching;
        private long _r863ManualDial1WaitingForFeedback;
        private long _r863ManualDial2WaitingForFeedback;
        private long _r863ManualDial3WaitingForFeedback;
        private long _r863ManualDial4WaitingForFeedback;

        /*Mi-8 VHF/UHF R-863 PRESETS COM2*/
        //Large dial 1-10 [step of 1]
        //Small dial volume control
        private readonly object _lockR863Preset1DialObject1 = new object();
        private DCSBIOSOutput _r863Preset1DcsbiosOutputPresetDial;
        private volatile uint _r863PresetCockpitDialPos = 1;
        private const string R863PresetCommandInc = "R863_CNL_SEL INC\n";
        private const string R863PresetCommandDec = "R863_CNL_SEL DEC\n";
        private int _r863PresetDialSkipper;
        private const string R863PresetVolumeKnobCommandInc = "R863_VOL +2500\n";
        private const string R863PresetVolumeKnobCommandDec = "R863_VOL -2500\n";

        /*Mi-8 YaDRO 1A NAV1*/
        //Large dial 100-149  -> 20 - 179 [step of 1]
        //Small dial 0 - 99
        private readonly ClickSpeedDetector _yadro1aBigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _yadro1aBigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private volatile uint _yadro1aBigFrequencyStandby = 100;
        private volatile uint _yadro1aSmallFrequencyStandby;
        private volatile uint _yadro1aSavedCockpitBigFrequency;
        private volatile uint _yadro1aSavedCockpitSmallFrequency;
        private object _lockYADRO1ADialsObject1 = new object();
        private object _lockYADRO1ADialsObject2 = new object();
        private object _lockYADRO1ADialsObject3 = new object();
        private object _lockYADRO1ADialsObject4 = new object();
        private volatile uint _yadro1aCockpitFreq1DialPos = 1;
        private volatile uint _yadro1aCockpitFreq2DialPos = 1;
        private volatile uint _yadro1aCockpitFreq3DialPos = 1;
        private volatile uint _yadro1aCockpitFreq4DialPos = 1;
        private double _yadro1aCockpitFrequency = 100;
        private DCSBIOSOutput _yadro1aDcsbiosOutputCockpitFrequency;
        private const string YADRO1AFreq1DialCommand = "YADRO1A_FREQ1 ";
        private const string YADRO1AFreq2DialCommand = "YADRO1A_FREQ2 ";
        private const string YADRO1AFreq3DialCommand = "YADRO1A_FREQ3 ";
        private const string YADRO1AFreq4DialCommand = "YADRO1A_FREQ4 ";
        private Thread _yadro1aSyncThread;
        private long _yadro1aThreadNowSynching;
        private long _yadro1aDial1WaitingForFeedback;
        private long _yadro1aDial2WaitingForFeedback;
        private long _yadro1aDial3WaitingForFeedback;
        private long _yadro1aDial4WaitingForFeedback;

        /*Mi-8 R-828 FM Radio PRESETS NAV2*/
        //Large dial 1-10 [step of 1]
        //Small dial volume control
        private readonly object _lockR828Preset1DialObject1 = new object();
        private DCSBIOSOutput _r828Preset1DcsbiosOutputDial;
        private volatile uint _r828PresetCockpitDialPos = 1;
        private const string R828PresetCommandInc = "R828_PRST_CHAN_SEL INC\n";
        private const string R828PresetCommandDec = "R828_PRST_CHAN_SEL DEC\n";
        private int _r828PresetDialSkipper;
        private const string R828PresetVolumeKnobCommandInc = "R828_VOL +2500\n";
        private const string R828PresetVolumeKnobCommandDec = "R828_VOL -2500\n";

        /*Mi-8 ARK-9 ADF*/
        //Large 100KHz 01 -> 12
        //Small 10Khz 00 -> 90 (10 steps)
        private readonly object _lockADFDialObject1 = new object();
        private readonly object _lockADFDialObject2 = new object();
        private DCSBIOSOutput _adfDcsbiosOutputPresetDial1;
        private DCSBIOSOutput _adfDcsbiosOutputPresetDial2;
        private volatile uint _adfCockpitPresetDial1Pos = 1;
        private volatile uint _adfCockpitPresetDial2Pos = 1;
        private const string ADF100KhzPresetCommandInc = "ARC_MAIN_100KHZ INC\n";
        private const string ADF100KhzPresetCommandDec = "ARC_MAIN_100KHZ DEC\n";
        private const string ADF10KhzPresetCommandInc = "ARC_MAIN_10KHZ INC\n";
        private const string ADF10KhzPresetCommandDec = "ARC_MAIN_10KHZ DEC\n";
        private int _adfPresetDial1Skipper;
        private int _adfPresetDial2Skipper;
        private const string ADFModeSwitchAntenna = "ADF_CMPS_ANT INC\n";
        private const string ADFModeSwitchCompass = "ADF_CMPS_ANT DEC\n";
        private string _adfModeSwitchLastSent = "";

        /*Mi-8 ARK-9 ADF (DME)*/
        //Large Tuning
        //Radio Volume
        private const string ADFTuneKnobCommandInc = "ARC9_MAIN_TUNE +500\n";
        private const string ADFTuneKnobCommandDec = "ARC9_MAIN_TUNE -500\n";
        private const string ADFVolumeKnobCommandInc = "ADF_VOLUME +2500\n";
        private const string ADFVolumeKnobCommandDec = "ADF_VOLUME -2500\n";
        private readonly object _lockADFTuneDialObject = new object();
        private DCSBIOSOutput _adfTuneDcsbiosOutputDial;
        private volatile uint _adfTuneCockpitDialPos = 1;
        //XPDR
        /*Mi-8 SPU-7 XPDR*/
        //Large dial 0-5 [step of 1]
        //Small dial volume control
        private readonly object _lockSPU7DialObject1 = new object();
        private DCSBIOSOutput _spu7DcsbiosOutputPresetDial;
        private volatile uint _spu7CockpitDialPos = 0;
        private int _spu7DialSkipper;
        private const string SPU7CommandInc = "RADIO_SEL_R INC\n";
        private const string SPU7CommandDec = "RADIO_SEL_R DEC\n";
        private const string SPU7VolumeKnobCommandInc = "LST_VOL_KNOB_L +2500\n";
        private const string SPU7VolumeKnobCommandDec = "LST_VOL_KNOB_L -2500\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69Mi8(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69Mi8()
        {
            if (_r863ManualSyncThread != null)
            {
                _r863ManualSyncThread.Abort();
            }
        }

        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(stringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + stringData);
                    return;
                }
                if (address.Equals(_yadro1aDcsbiosOutputCockpitFrequency.Address))
                {
                    // "02000.0" - "17999.9"
                    // Last digit not used in panel


                    var tmpFreq = Double.Parse(stringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_yadro1aCockpitFrequency))
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                    if (tmpFreq.Equals(_yadro1aCockpitFrequency))
                    {
                        //No need to process same data over and over
                        return;
                    }
                    _yadro1aCockpitFrequency = tmpFreq;
                    lock (_lockYADRO1ADialsObject1)
                    {
                        // "02000.0" - "*17*999.9"
                        var tmp = _yadro1aCockpitFreq1DialPos;
                        _yadro1aCockpitFreq1DialPos = uint.Parse(stringData.Substring(0, 2));
                        Common.DebugP("Just read YaDRO-1A dial 1 position: " + _yadro1aCockpitFreq1DialPos + "  " + Environment.TickCount);
                        if (tmp != _yadro1aCockpitFreq1DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _yadro1aDial1WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockYADRO1ADialsObject2)
                    {
                        // "02000.0" - "17*9*99.9"  
                        var tmp = _yadro1aCockpitFreq2DialPos;
                        _yadro1aCockpitFreq2DialPos = uint.Parse(stringData.Substring(2, 1));
                        Common.DebugP("Just read YaDRO-1A dial 2 position: " + _yadro1aCockpitFreq2DialPos + "  " + Environment.TickCount);
                        if (tmp != _yadro1aCockpitFreq2DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _yadro1aDial2WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockYADRO1ADialsObject3)
                    {
                        // "02000.0" - "179*9*9.9"  
                        var tmp = _yadro1aCockpitFreq3DialPos;
                        _yadro1aCockpitFreq3DialPos = uint.Parse(stringData.Substring(3, 1));
                        Common.DebugP("Just read YaDRO-1A dial 3 position: " + _yadro1aCockpitFreq3DialPos + "  " + Environment.TickCount);
                        if (tmp != _yadro1aCockpitFreq3DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _yadro1aDial3WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockYADRO1ADialsObject4)
                    {
                        // "02000.0" - "1799*9*.9"  
                        var tmp = _yadro1aCockpitFreq4DialPos;
                        _yadro1aCockpitFreq4DialPos = uint.Parse(stringData.Substring(4, 1));
                        Common.DebugP("Just read YaDRO-1A dial 4 position: " + _yadro1aCockpitFreq4DialPos + "  " + Environment.TickCount);
                        if (tmp != _yadro1aCockpitFreq4DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _yadro1aDial4WaitingForFeedback, 0);
                        }
                    }
                }
                if (address.Equals(_r863ManualDcsbiosOutputCockpitFrequency.Address))
                {
                    // "100.000" - "399.975"
                    // Last digit not used in panel


                    var tmpFreq = Double.Parse(stringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_r863ManualCockpitFrequency))
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                    if (tmpFreq.Equals(_r863ManualCockpitFrequency))
                    {
                        //No need to process same data over and over
                        return;
                    }
                    _r863ManualCockpitFrequency = tmpFreq;
                    lock (_lockR863ManualDialsObject1)
                    {
                        // "100.000" - "*39*9.975"
                        var tmp = _r863ManualCockpitFreq1DialPos;
                        _r863ManualCockpitFreq1DialPos = uint.Parse(stringData.Substring(0, 2));
                        Common.DebugP("Just read R-863 dial 1 position: " + _r863ManualCockpitFreq1DialPos + "  " + Environment.TickCount);
                        if (tmp != _r863ManualCockpitFreq1DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r863ManualDial1WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockR863ManualDialsObject2)
                    {
                        // "100.000" - "39*9*.975"
                        var tmp = _r863ManualCockpitFreq2DialPos;
                        _r863ManualCockpitFreq2DialPos = uint.Parse(stringData.Substring(2, 1));
                        Common.DebugP("Just read R-863 dial 2 position: " + _r863ManualCockpitFreq2DialPos + "  " + Environment.TickCount);
                        if (tmp != _r863ManualCockpitFreq2DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r863ManualDial2WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockR863ManualDialsObject3)
                    {
                        // "100.000" - "399.*9*75"
                        var tmp = _r863ManualCockpitFreq3DialPos;
                        _r863ManualCockpitFreq3DialPos = uint.Parse(stringData.Substring(4, 1));
                        Common.DebugP("Just read R-863 dial 3 position: " + _r863ManualCockpitFreq3DialPos + "  " + Environment.TickCount);
                        if (tmp != _r863ManualCockpitFreq3DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r863ManualDial3WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockR863ManualDialsObject4)
                    {
                        // "100.000" - "399.9*75*"
                        //Read only the first char
                        var tmp = _r863ManualCockpitFreq4DialPos;
                        _r863ManualCockpitFreq4DialPos = uint.Parse(stringData.Substring(5, 1));
                        Common.DebugP("Just read R-863 dial 4 position: " + _r863ManualCockpitFreq4DialPos + "  " + Environment.TickCount);
                        if (tmp != _r863ManualCockpitFreq4DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.LogError(78030, e, "DCSBIOSStringReceived()");
            }
            ShowFrequenciesOnPanel();
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            try
            {
                //Common.DebugP("PZ69 Mi8 READ ENTERING");
                UpdateCounter(address, data);
                /*
                 * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
                 * Once a dial has been deemed to be "off" position and needs to be changed
                 * a change command is sent to DCS-BIOS.
                 * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
                 * reset. Reading the dial's position with no change in value will not reset.
                 */



                //R-863 Preset Channel Dial
                if (address == _r863Preset1DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockR863Preset1DialObject1)
                    {
                        var tmp = _r863PresetCockpitDialPos;
                        _r863PresetCockpitDialPos = _r863Preset1DcsbiosOutputPresetDial.GetUIntValue(data);
                        if (tmp != _r863PresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //YaDRO-1A

                //R-828 Preset Channel Dial
                if (address == _r828Preset1DcsbiosOutputDial.Address)
                {
                    lock (_lockR828Preset1DialObject1)
                    {
                        var tmp = _r828PresetCockpitDialPos;
                        _r828PresetCockpitDialPos = _r828Preset1DcsbiosOutputDial.GetUIntValue(data);
                        if (tmp != _r828PresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Preset Dial 1
                if (address == _adfDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockADFDialObject1)
                    {
                        var tmp = _adfCockpitPresetDial1Pos;
                        _adfCockpitPresetDial1Pos = _adfDcsbiosOutputPresetDial1.GetUIntValue(data);
                        if (tmp != _adfCockpitPresetDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Preset Dial 2
                if (address == _adfDcsbiosOutputPresetDial2.Address)
                {
                    lock (_lockADFDialObject1)
                    {
                        var tmp = _adfCockpitPresetDial2Pos;
                        _adfCockpitPresetDial2Pos = _adfDcsbiosOutputPresetDial2.GetUIntValue(data);
                        if (tmp != _adfCockpitPresetDial2Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }
                
                //ADF Tune
                if (address == _adfTuneDcsbiosOutputDial.Address)
                {
                    lock (_lockADFTuneDialObject)
                    {
                        var tmp = _adfTuneCockpitDialPos;
                        _adfTuneCockpitDialPos = _adfTuneDcsbiosOutputDial.GetUIntValue(data);
                        if (tmp != _adfTuneCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //SPU-7 Dial
                if (address == _spu7DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockSPU7DialObject1)
                    {
                        var tmp = _spu7CockpitDialPos;
                        _spu7CockpitDialPos = _spu7DcsbiosOutputPresetDial.GetUIntValue(data);
                        if (tmp != _spu7CockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
                //Common.DebugP("PZ69 Mi8 READ EXITING");
            }
            catch (Exception ex)
            {
                Common.LogError(78001, ex);
            }
        }


        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsMi8 knob)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SendFrequencyToDCSBIOS()");
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }
                switch (knob)
                {
                    case RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentMi8RadioMode.R863_PRESET:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.R863_MANUAL:
                                    {
                                        SendR863ManualToDCSBIOS();
                                        break;
                                    }
                                case CurrentMi8RadioMode.YADRO1A:
                                    {
                                        SendYaDRO1AToDCSBIOS();
                                        break;
                                    }
                                case CurrentMi8RadioMode.R828_PRESETS:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.ADF_ARK9:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.SPU7:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentMi8RadioMode.R863_PRESET:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.R863_MANUAL:
                                    {
                                        SendR863ManualToDCSBIOS();
                                        break;
                                    }
                                case CurrentMi8RadioMode.YADRO1A:
                                    {
                                        SendYaDRO1AToDCSBIOS();
                                        break;
                                    }
                                case CurrentMi8RadioMode.R828_PRESETS:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.ADF_ARK9:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.SPU7:
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
            Common.DebugP("Leaving Mi-8 Radio SendFrequencyToDCSBIOS()");
        }


        private void SendR863ManualToDCSBIOS()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SendR863ManualToDCSBIOS()");
                if (R863ManualNowSyncing())
                {
                    return;
                }
                SaveCockpitFrequencyR863Manual();


                if (_r863ManualSyncThread != null)
                {
                    _r863ManualSyncThread.Abort();
                }
                _r863ManualSyncThread = new Thread(() => R863ManualSynchThreadMethod());
                _r863ManualSyncThread.Start();

            }
            catch (Exception ex)
            {
                Common.LogError(78003, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SendR863ManualToDCSBIOS()");
        }

        private void R863ManualSynchThreadMethod()
        {
            try
            {
                try
                {
                    try
                    {   /*
                     * Mi-8 R-863 COM1
                     */
                        Common.DebugP("Entering Mi-8 Radio R863ManualSynchThreadMethod()");
                        string str;
                        Interlocked.Exchange(ref _r863ManualThreadNowSynching, 1);
                        long dial1Timeout = DateTime.Now.Ticks;
                        long dial2Timeout = DateTime.Now.Ticks;
                        long dial3Timeout = DateTime.Now.Ticks;
                        long dial4Timeout = DateTime.Now.Ticks;
                        long dial1OkTime = 0;
                        long dial2OkTime = 0;
                        long dial3OkTime = 0;
                        long dial4OkTime = 0;
                        var dial1SendCount = 0;
                        var dial2SendCount = 0;
                        var dial3SendCount = 0;
                        var dial4SendCount = 0;

                        var frequencyAsString = _r863ManualBigFrequencyStandby.ToString() + "." + _r863ManualSmallFrequencyStandby.ToString().PadLeft(2, '0');
                        frequencyAsString = frequencyAsString.PadRight(6, '0');
                        Common.DebugP("Standby frequencyAsString is " + frequencyAsString);
                        //Frequency selector 1      R863_FREQ1
                        //      "10" "11" "12" "13" "14" "22" "23" "24" "25" "26" "27" "28" "29" "30" "31" "32" "33" "34" "35" "36" "37" "38" "39"
                        //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15   16   17   18   19   20   21   22

                        //Frequency selector 2      R863_FREQ2
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 3      R863_FREQ3
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 4      R863_FREQ4
                        //      "00" "25" "50" "75", only "00" and "50" used.
                        //Pos     0    1    2    3

                        //Reason for this is to separate the standby frequency from the sync loop
                        //If not the sync would pick up any changes made by the user during the
                        //sync process
                        var desiredPositionDial1X = 0;
                        var desiredPositionDial2X = 0;
                        var desiredPositionDial3X = 0;
                        var desiredPositionDial4X = 0;

                        //151.95
                        //#1 = 15  (position = value - 3)
                        //#2 = 1   (position = value)
                        //#3 = 9   (position = value)
                        //#4 = 5
                        desiredPositionDial1X = int.Parse(frequencyAsString.Substring(0, 2));//Array.IndexOf(_r863ManualFreq1DialValues, int.Parse(frequencyAsString.Substring(0, 2)));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(4, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(5, 1));

                        do
                        {
                            if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "R-863 dial1Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r863ManualDial1WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-863 1");
                            }
                            if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "R-863 dial2Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r863ManualDial2WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-863 2");
                            }
                            if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "R-863 dial3Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r863ManualDial3WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-863 3");
                            }
                            if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "R-863 dial4Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-863 4");
                            }
                            if (Interlocked.Read(ref _r863ManualDial1WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject1)
                                {

                                    Common.DebugP("_r863ManualCockpitFreq1DialPos is " + _r863ManualCockpitFreq1DialPos + " and should be " + desiredPositionDial1X);
                                    if (_r863ManualCockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq1DialCommand + GetCommandDirectionForR863ManualDial1(desiredPositionDial1X, _r863ManualCockpitFreq1DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial1SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial1WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial1Timeout);
                                }
                            }
                            else
                            {
                                dial1OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _r863ManualDial2WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject2)
                                {
                                    Common.DebugP("_r863ManualCockpitFreq2DialPos is " + _r863ManualCockpitFreq2DialPos + " and should be " + desiredPositionDial2X);
                                    if (_r863ManualCockpitFreq2DialPos != desiredPositionDial2X)
                                    {
                                        dial2OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq2DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial2X, _r863ManualCockpitFreq2DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial2SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial2WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial2Timeout);
                                }
                            }
                            else
                            {
                                dial2OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _r863ManualDial3WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject3)
                                {
                                    Common.DebugP("_r863ManualCockpitFreq3DialPos is " + _r863ManualCockpitFreq3DialPos + " and should be " + desiredPositionDial3X);
                                    if (_r863ManualCockpitFreq3DialPos != desiredPositionDial3X)
                                    {
                                        dial3OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq3DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial3X, _r863ManualCockpitFreq3DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial3SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial3WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial3Timeout);
                                }
                            }
                            else
                            {
                                dial3OkTime = DateTime.Now.Ticks;
                            }
                            var desiredPositionDial4 = 0;
                            if (Interlocked.Read(ref _r863ManualDial4WaitingForFeedback) == 0)
                            {
                                if (desiredPositionDial4X == 0)
                                {
                                    desiredPositionDial4 = 0;
                                }
                                else if (desiredPositionDial4X == 2)
                                {
                                    desiredPositionDial4 = 5;
                                }
                                else if (desiredPositionDial4X == 5)
                                {
                                    desiredPositionDial4 = 5;
                                }
                                else if (desiredPositionDial4X == 7)
                                {
                                    desiredPositionDial4 = 0;
                                }
                                //      "00" "25" "50" "75", only "00" and "50" used.
                                //Pos     0    1    2    3

                                lock (_lockR863ManualDialsObject4)
                                {
                                    Common.DebugP("_r863ManualCockpitFreq4DialPos is " + _r863ManualCockpitFreq4DialPos + " and should be " + desiredPositionDial4);
                                    if (_r863ManualCockpitFreq4DialPos < desiredPositionDial4)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq4DialCommand + "INC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 1);
                                    }
                                    else if (_r863ManualCockpitFreq4DialPos > desiredPositionDial4)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq4DialCommand + "DEC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial4Timeout);
                                }

                            }
                            else
                            {
                                dial4OkTime = DateTime.Now.Ticks;
                            }
                            if (dial1SendCount > 12 || dial2SendCount > 10 || dial3SendCount > 10 || dial4SendCount > 5)
                            {
                                //"Race" condition detected?
                                dial1SendCount = 0;
                                dial2SendCount = 0;
                                dial3SendCount = 0;
                                dial4SendCount = 0;
                                Thread.Sleep(5000);
                            }
                            Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                        }
                        while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime));
                        SwapCockpitStandbyFrequencyR863Manual();
                        ShowFrequenciesOnPanel();
                    }
                    catch (ThreadAbortException)
                    { }
                    catch (Exception ex)
                    {
                        Common.LogError(56443, ex);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _r863ManualThreadNowSynching, 0);
                }

            }
            catch (Exception ex)
            {
                Common.LogError(78004, ex);
            }
            //Refresh panel once this debacle is finished
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            Common.DebugP("Leaving Mi-8 Radio R863ManualSynchThreadMethod()");
        }

        private void SwapCockpitStandbyFrequencyR863Manual()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SwapCockpitStandbyFrequencyR863Manual()");
                Common.DebugP("_r863ManualBigFrequencyStandby  " + _r863ManualBigFrequencyStandby);
                Common.DebugP("_r863ManualSavedCockpitBigFrequency  " + _r863ManualSavedCockpitBigFrequency);
                Common.DebugP("_r863ManualSmallFrequencyStandby  " + _r863ManualSmallFrequencyStandby);
                Common.DebugP("_r863ManualSavedCockpitSmallFrequency  " + _r863ManualSavedCockpitSmallFrequency);
                _r863ManualBigFrequencyStandby = _r863ManualSavedCockpitBigFrequency;
                _r863ManualSmallFrequencyStandby = _r863ManualSavedCockpitSmallFrequency;
            }
            catch (Exception ex)
            {
                Common.LogError(78005, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SwapCockpitStandbyFrequencyR863Manual()");
        }

        private void SendYaDRO1AToDCSBIOS()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SendYaDRO1AToDCSBIOS()");
                if (YaDRO1ANowSyncing())
                {
                    return;
                }
                SaveCockpitFrequencyYaDRO1A();


                if (_yadro1aSyncThread != null)
                {
                    _yadro1aSyncThread.Abort();
                }
                _yadro1aSyncThread = new Thread(() => YaDRO1ASynchThreadMethod());
                _yadro1aSyncThread.Start();

            }
            catch (Exception ex)
            {
                Common.LogError(78003, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SendYaDRO1AToDCSBIOS()");
        }

        private void YaDRO1ASynchThreadMethod()
        {
            try
            {
                try
                {
                    try
                    {   /*
                     * Mi-8 YaDRO-1A
                     */
                        Common.DebugP("Entering Mi-8 Radio YaDRO1ASynchThreadMethod()");
                        string str;
                        Interlocked.Exchange(ref _yadro1aThreadNowSynching, 1);
                        long dial1Timeout = DateTime.Now.Ticks;
                        long dial2Timeout = DateTime.Now.Ticks;
                        long dial3Timeout = DateTime.Now.Ticks;
                        long dial4Timeout = DateTime.Now.Ticks;
                        long dial1OkTime = 0;
                        long dial2OkTime = 0;
                        long dial3OkTime = 0;
                        long dial4OkTime = 0;
                        var dial1SendCount = 0;
                        var dial2SendCount = 0;
                        var dial3SendCount = 0;
                        var dial4SendCount = 0;

                        var frequencyAsString = _yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0');
                        frequencyAsString = frequencyAsString.PadRight(6, '0');
                        //Frequency selector 1      YADRO1A_FREQ1
                        //      "02" "03" "04" "05" "06" "07" "08" "09" "10" "11" "12" "13" "14" "15" "16" "17"
                        //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15

                        //Frequency selector 2      YADRO1A_FREQ2
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 3      YADRO1A_FREQ3
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 4      YADRO1A_FREQ4
                        //0 1 2 3 4 5 6 7 8 9

                        //Reason for this is to separate the standby frequency from the sync loop
                        //If not the sync would pick up any changes made by the user during the
                        //sync process
                        var desiredPositionDial1X = 0;
                        var desiredPositionDial2X = 0;
                        var desiredPositionDial3X = 0;
                        var desiredPositionDial4X = 0;

                        //02000
                        //17999
                        //#1 = 17  (position = value)
                        //#2 = 9   (position = value)
                        //#3 = 9   (position = value)
                        //#4 = 9   (position = value)
                        desiredPositionDial1X = int.Parse(frequencyAsString.Substring(0, 2));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(3, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(4, 1));

                        do
                        {
                            if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "YaDRO-1A dial1Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _yadro1aDial1WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for YaDRO-1A 1");
                            }
                            if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "YaDRO-1A dial2Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _yadro1aDial2WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for YaDRO-1A 2");
                            }
                            if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "YaDRO-1A dial3Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _yadro1aDial3WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for YaDRO-1A 3");
                            }
                            if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "YaDRO-1A dial4Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _yadro1aDial4WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for YaDRO-1A 4");
                            }
                            if (Interlocked.Read(ref _yadro1aDial1WaitingForFeedback) == 0)
                            {
                                lock (_lockYADRO1ADialsObject1)
                                {

                                    Common.DebugP("_yadro1aCockpitFreq1DialPos is " + _yadro1aCockpitFreq1DialPos + " and should be " + desiredPositionDial1X);
                                    if (_yadro1aCockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        if (_yadro1aCockpitFreq1DialPos < desiredPositionDial1X)
                                        {
                                            str = YADRO1AFreq1DialCommand + "INC\n";
                                        }
                                        else
                                        {
                                            str = YADRO1AFreq1DialCommand + "DEC\n";
                                        }
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial1SendCount++;
                                        Interlocked.Exchange(ref _yadro1aDial1WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial1Timeout);
                                }
                            }
                            else
                            {
                                dial1OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _yadro1aDial2WaitingForFeedback) == 0)
                            {
                                lock (_lockYADRO1ADialsObject2)
                                {
                                    Common.DebugP("_yadro1aCockpitFreq2DialPos is " + _yadro1aCockpitFreq2DialPos + " and should be " + desiredPositionDial2X);
                                    if (_yadro1aCockpitFreq2DialPos != desiredPositionDial2X)
                                    {
                                        dial2OkTime = DateTime.Now.Ticks;
                                        str = YADRO1AFreq2DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial2X, _yadro1aCockpitFreq2DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial2SendCount++;
                                        Interlocked.Exchange(ref _yadro1aDial2WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial2Timeout);
                                }
                            }
                            else
                            {
                                dial2OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _yadro1aDial3WaitingForFeedback) == 0)
                            {
                                lock (_lockYADRO1ADialsObject3)
                                {
                                    Common.DebugP("_yadro1aCockpitFreq3DialPos is " + _yadro1aCockpitFreq3DialPos + " and should be " + desiredPositionDial3X);
                                    if (_yadro1aCockpitFreq3DialPos != desiredPositionDial3X)
                                    {
                                        dial3OkTime = DateTime.Now.Ticks;
                                        str = YADRO1AFreq3DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial3X, _yadro1aCockpitFreq3DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial3SendCount++;
                                        Interlocked.Exchange(ref _yadro1aDial3WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial3Timeout);
                                }
                            }
                            else
                            {
                                dial3OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _yadro1aDial4WaitingForFeedback) == 0)
                            {
                                lock (_lockYADRO1ADialsObject4)
                                {
                                    Common.DebugP("_yadro1aCockpitFreq4DialPos is " + _yadro1aCockpitFreq4DialPos + " and should be " + desiredPositionDial4X);
                                    if (_yadro1aCockpitFreq4DialPos != desiredPositionDial4X)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = YADRO1AFreq4DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial4X, _yadro1aCockpitFreq4DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _yadro1aDial4WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial4Timeout);
                                }
                            }
                            else
                            {
                                dial4OkTime = DateTime.Now.Ticks;
                            }

                            Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                        }
                        while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime));
                        SwapCockpitStandbyFrequencyYaDRO1A();
                        ShowFrequenciesOnPanel();
                    }
                    catch (ThreadAbortException)
                    { }
                    catch (Exception ex)
                    {
                        Common.LogError(56443, ex);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _yadro1aThreadNowSynching, 0);
                }

            }
            catch (Exception ex)
            {
                Common.LogError(78704, ex);
            }
            //Refresh panel once this debacle is finished
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            Common.DebugP("Leaving Mi-8 Radio YaDRO1ASynchThreadMethod()");
        }

        private void SwapCockpitStandbyFrequencyYaDRO1A()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SwapCockpitStandbyFrequencyYaDRO1A()");
                _yadro1aBigFrequencyStandby = _yadro1aSavedCockpitBigFrequency;
                _yadro1aSmallFrequencyStandby = _yadro1aSavedCockpitSmallFrequency;
            }
            catch (Exception ex)
            {
                Common.LogError(78055, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SwapCockpitStandbyFrequencyYaDRO1A()");
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (_lockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobMi8)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsMi8.UPPER_R863_MANUAL:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.R863_MANUAL);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_R863_PRESET:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.R863_PRESET);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_YADRO1A:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.YADRO1A);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_R828:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.R828_PRESETS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_ADF_ARK9:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.ADF_ARK9);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_SPU7:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.SPU7);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_R863_MANUAL:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.R863_MANUAL);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_R863_PRESET:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.R863_PRESET);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_YADRO1A:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.YADRO1A);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_R828:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.R828_PRESETS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_ADF_ARK9:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.ADF_ARK9);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_SPU7:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.SPU7);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_ADF_TUNE:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.ADF_TUNE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_ADF_TUNE:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.ADF_TUNE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentMi8RadioMode.R863_PRESET)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            //DCSBIOS.Send(Vhf1TunerButtonPress);
                                        }
                                        else
                                        {
                                            //DCSBIOS.Send(Vhf1TunerButtonRelease);
                                        }
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 && radioPanelKnob.IsOn)
                                    {
                                        if (_adfModeSwitchLastSent.Equals(ADFModeSwitchAntenna))
                                        {
                                            //DCSBIOS.Send(ADFModeSwitchCompass);
                                            //_adfModeSwitchLastSent = ADFModeSwitchCompass;
                                        }
                                        else
                                        {
                                            //DCSBIOS.Send(ADFModeSwitchAntenna);
                                            //_adfModeSwitchLastSent = ADFModeSwitchAntenna;
                                        }
                                    }
                                    else if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentMi8RadioMode.R863_PRESET)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            //DCSBIOS.Send(Vhf1TunerButtonPress);
                                        }
                                        else
                                        {
                                            //DCSBIOS.Send(Vhf1TunerButtonRelease);
                                        }
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9 && radioPanelKnob.IsOn)
                                    {
                                        if (_adfModeSwitchLastSent.Equals(ADFModeSwitchAntenna))
                                        {
                                            DCSBIOS.Send(ADFModeSwitchCompass);
                                            _adfModeSwitchLastSent = ADFModeSwitchCompass;
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(ADFModeSwitchAntenna);
                                            _adfModeSwitchLastSent = ADFModeSwitchAntenna;
                                        }
                                    }
                                    else if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH);
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
            Common.DebugP("Leaving Mi-8 Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobMi8 = (RadioPanelKnobMi8)o;
                    if (radioPanelKnobMi8.IsOn)
                    {
                        switch (radioPanelKnobMi8.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                if (_bigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (_r863ManualBigFrequencyStandby.Equals(399))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby + ChangeValue;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby++;
                                                }
                                                if (_r863ManualBigFrequencyStandby > 149 && _r863ManualBigFrequencyStandby < 220)
                                                {
                                                    _r863ManualBigFrequencyStandby = 220;
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1aBigFreqIncreaseChangeMonitor.Click();
                                                if (_yadro1aBigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    _yadro1aBigFrequencyStandby = _yadro1aBigFrequencyStandby + ChangeValue;
                                                }
                                                if (_yadro1aBigFrequencyStandby >= 179)
                                                {
                                                    //@ max value
                                                    _yadro1aBigFrequencyStandby = 179;
                                                    break;
                                                }
                                                _yadro1aBigFrequencyStandby++;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(ADF100KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFTuneKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSPU7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7CommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                if (_bigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (_r863ManualBigFrequencyStandby.Equals(100))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby - ChangeValue;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby--;
                                                }
                                                if (_r863ManualBigFrequencyStandby < 220 && _r863ManualBigFrequencyStandby > 149)
                                                {
                                                    _r863ManualBigFrequencyStandby = 149;
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1aBigFreqDecreaseChangeMonitor.Click();
                                                if (_yadro1aBigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    _yadro1aBigFrequencyStandby = _yadro1aBigFrequencyStandby - ChangeValue;
                                                }
                                                if (_yadro1aBigFrequencyStandby <= 20)
                                                {
                                                    //@ max value
                                                    _yadro1aBigFrequencyStandby = 20;
                                                    break;
                                                }
                                                _yadro1aBigFrequencyStandby--;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(ADF100KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFTuneKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSPU7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7CommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby >= 95)
                                                {
                                                    //At max value
                                                    _r863ManualSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby + 5;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863PresetVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1aSmallFrequencyStandby >= 99)
                                                {
                                                    //At max value
                                                    _yadro1aSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _yadro1aSmallFrequencyStandby = _yadro1aSmallFrequencyStandby + 1;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828PresetVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(ADF10KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7VolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _r863ManualSmallFrequencyStandby = 95;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby - 5;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863PresetVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1aSmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _yadro1aSmallFrequencyStandby = 99;
                                                    break;
                                                }
                                                _yadro1aSmallFrequencyStandby = _yadro1aSmallFrequencyStandby - 1;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828PresetVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(ADF10KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7VolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                if (_bigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (_r863ManualBigFrequencyStandby.Equals(399))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby + ChangeValue;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby++;
                                                }
                                                if (_r863ManualBigFrequencyStandby > 149 && _r863ManualBigFrequencyStandby < 220)
                                                {
                                                    _r863ManualBigFrequencyStandby = 220;
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1aBigFreqIncreaseChangeMonitor.Click();
                                                if (_yadro1aBigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    _yadro1aBigFrequencyStandby = _yadro1aBigFrequencyStandby + ChangeValue;
                                                }
                                                if (_yadro1aBigFrequencyStandby >= 179)
                                                {
                                                    //@ max value
                                                    _yadro1aBigFrequencyStandby = 179;
                                                    break;
                                                }
                                                _yadro1aBigFrequencyStandby++;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(ADF100KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFTuneKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSPU7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7CommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                if (_bigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (_r863ManualBigFrequencyStandby.Equals(100))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby - ChangeValue;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby--;
                                                }
                                                if (_r863ManualBigFrequencyStandby < 220 && _r863ManualBigFrequencyStandby > 149)
                                                {
                                                    _r863ManualBigFrequencyStandby = 149;
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1aBigFreqDecreaseChangeMonitor.Click();
                                                if (_yadro1aBigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                if (changeFaster)
                                                {
                                                    _yadro1aBigFrequencyStandby = _yadro1aBigFrequencyStandby - ChangeValue;
                                                }
                                                if (_yadro1aBigFrequencyStandby <= 20)
                                                {
                                                    //@ max value
                                                    _yadro1aBigFrequencyStandby = 20;
                                                    break;
                                                }
                                                _yadro1aBigFrequencyStandby--;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(ADF100KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFTuneKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSPU7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7CommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby >= 95)
                                                {
                                                    //At max value
                                                    _r863ManualSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby + 5;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863PresetVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1aSmallFrequencyStandby >= 99)
                                                {
                                                    //At max value
                                                    _yadro1aSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _yadro1aSmallFrequencyStandby = _yadro1aSmallFrequencyStandby + 1;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828PresetVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(ADF10KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7VolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _r863ManualSmallFrequencyStandby = 95;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby - 5;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863PresetVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1aSmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _yadro1aSmallFrequencyStandby = 99;
                                                    break;
                                                }
                                                _yadro1aSmallFrequencyStandby = _yadro1aSmallFrequencyStandby - 1;
                                                break;
                                            }
                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828PresetVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(ADF10KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ADF_TUNE:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7VolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentMi8RadioMode.NOUSE:
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
            Common.DebugP("Leaving Mi-8 Radio AdjustFrequency()");
        }


        private void CheckFrequenciesForValidity()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio CheckFrequenciesForValidity()");
                //Crude fix if any freqs are outside the valid boundaries

                //R-800L VHF 2
                //100.00 - 149.00
                //220.00 - 399.00
                if (_r863ManualBigFrequencyStandby < 100)
                {
                    _r863ManualBigFrequencyStandby = 100;
                }
                if (_r863ManualBigFrequencyStandby > 399)
                {
                    _r863ManualBigFrequencyStandby = 399;
                }
                if (_r863ManualBigFrequencyStandby == 399 && _r863ManualSmallFrequencyStandby > 0)
                {
                    _r863ManualSmallFrequencyStandby = 0;
                }
                if (_r863ManualBigFrequencyStandby == 149 && _r863ManualSmallFrequencyStandby > 0)
                {
                    _r863ManualSmallFrequencyStandby = 0;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78008, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio CheckFrequenciesForValidity()");
        }


        private void ShowFrequenciesOnPanel()
        {
            try
            {
                lock (_lockShowFrequenciesOnPanelObject)
                {
                    if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                    {
                        //Common.DebugP("Leaving Mi-8 Radio ShowFrequenciesOnPanel() NO KNOBS/FREQS changed");
                        return;
                    }
                    //Common.DebugP("ShowFrequenciesOnPanel " + id);
                    if (!FirstReportHasBeenRead)
                    {
                        //Common.DebugP("Leaving Mi-8 Radio ShowFrequenciesOnPanel()");
                        return;
                    }

                    Common.DebugP("Entering Mi-8 Radio ShowFrequenciesOnPanel()");
                    CheckFrequenciesForValidity();
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentMi8RadioMode.R863_MANUAL:
                            {
                                var frequencyAsString = "";
                                lock (_lockR863ManualDialsObject1)
                                {
                                    frequencyAsString = _r863ManualCockpitFreq1DialPos.ToString();//_r863ManualFreq1DialValues[_r863ManualCockpitFreq1DialPos].ToString();
                                }
                                lock (_lockR863ManualDialsObject2)
                                {

                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq2DialPos;
                                }
                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR863ManualDialsObject3)
                                {

                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq3DialPos;
                                }
                                lock (_lockR863ManualDialsObject4)
                                {

                                    frequencyAsString = frequencyAsString + GetR863ManualDialFrequencyForPosition(_r863ManualCockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                                break;
                            }
                        case CurrentMi8RadioMode.R863_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                var channelAsString = "";
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = (_r863PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.YADRO1A:
                            {
                                lock (_lockYADRO1ADialsObject1)
                                {
                                    lock (_lockYADRO1ADialsObject2)
                                    {
                                        lock (_lockYADRO1ADialsObject3)
                                        {
                                            lock (_lockYADRO1ADialsObject4)
                                            {
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)_yadro1aCockpitFrequency, PZ69LCDPosition.UPPER_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0')), PZ69LCDPosition.UPPER_RIGHT);
                                                //SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                                            }
                                        }
                                    }
                                }
                                break;
                            }

                        case CurrentMi8RadioMode.R828_PRESETS:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10 

                                var channelAsString = "";
                                lock (_lockR828Preset1DialObject1)
                                {
                                    channelAsString = (_r828PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ADF_ARK9:
                            {
                                //Dial1 XX00
                                //Dial2 00XX

                                var channelAsString = "";
                                lock (_lockADFDialObject1)
                                {
                                    channelAsString = (_adfCockpitPresetDial1Pos + 1).ToString();
                                }
                                lock (_lockADFDialObject2)
                                {
                                    channelAsString = channelAsString + _adfCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ADF_TUNE:
                        {
                            //Dial1 XX00
                            //Dial2 00XX

                            var channelAsString = "";
                            var tuneValueAsString = "";
                            lock (_lockADFDialObject1)
                            {
                                channelAsString = (_adfCockpitPresetDial1Pos + 1).ToString();
                            }
                            lock (_lockADFDialObject2)
                            {
                                channelAsString = channelAsString + _adfCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                            }
                            lock (_lockADFTuneDialObject)
                            {
                                tuneValueAsString = _adfTuneCockpitDialPos.ToString();
                            }
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(tuneValueAsString), PZ69LCDPosition.UPPER_RIGHT);
                            break;
                        }
                        case CurrentMi8RadioMode.SPU7:
                            {
                                //0-5
                                var channelAsString = "";
                                lock (_lockSPU7DialObject1)
                                {
                                    channelAsString = (_spu7CockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentMi8RadioMode.R863_MANUAL:
                            {
                                var frequencyAsString = "";
                                lock (_lockR863ManualDialsObject1)
                                {
                                    frequencyAsString = _r863ManualCockpitFreq1DialPos.ToString(); //_r863ManualFreq1DialValues[_r863ManualCockpitFreq1DialPos].ToString();
                                }
                                lock (_lockR863ManualDialsObject2)
                                {

                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq2DialPos;
                                }
                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR863ManualDialsObject3)
                                {

                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq3DialPos;
                                }
                                lock (_lockR863ManualDialsObject4)
                                {

                                    frequencyAsString = frequencyAsString + GetR863ManualDialFrequencyForPosition(_r863ManualCockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_RIGHT);
                                break;
                            }
                        case CurrentMi8RadioMode.R863_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10  

                                var channelAsString = "";
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = (_r863PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.YADRO1A:
                            {
                                lock (_lockYADRO1ADialsObject1)
                                {
                                    lock (_lockYADRO1ADialsObject2)
                                    {
                                        lock (_lockYADRO1ADialsObject3)
                                        {
                                            lock (_lockYADRO1ADialsObject4)
                                            {
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)_yadro1aCockpitFrequency, PZ69LCDPosition.LOWER_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0')), PZ69LCDPosition.LOWER_RIGHT);
                                                //SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case CurrentMi8RadioMode.R828_PRESETS:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10 

                                var channelAsString = "";
                                lock (_lockR828Preset1DialObject1)
                                {
                                    channelAsString = (_r828PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ADF_ARK9:
                            {
                                //Dial1 XX00
                                //Dial2 00XX

                                var channelAsString = "";
                                lock (_lockADFDialObject1)
                                {
                                    channelAsString = (_adfCockpitPresetDial1Pos + 1).ToString();
                                }
                                lock (_lockADFDialObject2)
                                {
                                    channelAsString = channelAsString + _adfCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ADF_TUNE:
                        {
                            //Dial1 XX00
                            //Dial2 00XX

                            var channelAsString = "";
                            var tuneValueAsString = "";
                            lock (_lockADFDialObject1)
                            {
                                channelAsString = (_adfCockpitPresetDial1Pos + 1).ToString();
                            }
                            lock (_lockADFDialObject2)
                            {
                                channelAsString = channelAsString + _adfCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                            }
                            lock (_lockADFTuneDialObject)
                            {
                                tuneValueAsString = _adfTuneCockpitDialPos.ToString();
                            }
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(tuneValueAsString), PZ69LCDPosition.LOWER_RIGHT);
                            break;
                        }
                        case CurrentMi8RadioMode.SPU7:
                            {
                                //0-5
                                var channelAsString = "";
                                lock (_lockSPU7DialObject1)
                                {
                                    channelAsString = (_spu7CockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.NOUSE:
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
                Common.LogError(78011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving Mi-8 Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering Mi-8 Radio OnReport()");
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
                                    var knob = (RadioPanelKnobMi8)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobMi8)radioPanelKnob));
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
            Common.DebugP("Leaving Mi-8 Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering Mi-8 Radio GetHashSetOfChangedKnobs()");
                //Common.DebugP("Old: " + Convert.ToString(oldValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[2], 2).PadLeft(8, '0'));
                //Common.DebugP("New: " + Convert.ToString(newValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[2], 2).PadLeft(8, '0'));
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
                            //Common.DebugP("Following knob has changed : " + radioPanelKnob.RadioPanelPZ69Knob + " isOn? : " + radioPanelKnob.IsOn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78013, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("Mi-8");

                //COM1
                _r863ManualDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("R863_FREQ");
                DCSBIOSStringListenerHandler.AddAddress(_r863ManualDcsbiosOutputCockpitFrequency.Address, 7, this);

                //COM2
                _r863Preset1DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R863_CNL_SEL");

                //NAV1
                _yadro1aDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("YADRO1A_FREQ");
                DCSBIOSStringListenerHandler.AddAddress(_yadro1aDcsbiosOutputCockpitFrequency.Address, 7, this);

                //NAV2
                _r828Preset1DcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R828_PRST_CHAN_SEL");

                //ADF
                _adfDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_MAIN_100KHZ");
                _adfDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_MAIN_10KHZ");

                //DME
                _adfTuneDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC9_MAIN_TUNE");

                //XPDR
                _spu7DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RADIO_SEL_L");

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69Mi8.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving Mi-8 Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobMi8.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobMi8 radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentMi8RadioMode currentMi8RadioMode)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentMi8RadioMode);
                _currentUpperRadioMode = currentMi8RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(78014, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentMi8RadioMode currentMi8RadioMode)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentMi8RadioMode);
                _currentLowerRadioMode = currentMi8RadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SetLowerRadioMode()");
        }

        private bool R863ManualNowSyncing()
        {
            return Interlocked.Read(ref _r863ManualThreadNowSynching) > 0;
        }

        private bool YaDRO1ANowSyncing()
        {
            return Interlocked.Read(ref _yadro1aThreadNowSynching) > 0;
        }

        private void SaveCockpitFrequencyR863Manual()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SaveCockpitFrequencyR863Manual()");
                /*
                 * Dial 1
                 *      10 11 12 13 14 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39
                 * Pos  0   1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22
                 * 
                 * Dial 2
                 * 0 - 9
                 * 
                 * "."
                 * 
                 * Dial 3
                 * 0 - 9
                 * 
                 * Dial 4
                 * 00/50
                 */

                lock (_lockR863ManualDialsObject1)
                {
                    lock (_lockR863ManualDialsObject2)
                    {
                        lock (_lockR863ManualDialsObject3)
                        {
                            lock (_lockR863ManualDialsObject4)
                            {
                                _r863ManualSavedCockpitBigFrequency = uint.Parse(_r863ManualCockpitFreq1DialPos.ToString() + _r863ManualCockpitFreq2DialPos.ToString());//uint.Parse(_r863ManualFreq1DialValues[_r863ManualCockpitFreq1DialPos].ToString() + _r863ManualCockpitFreq2DialPos.ToString());
                                _r863ManualSavedCockpitSmallFrequency = uint.Parse(_r863ManualCockpitFreq3DialPos.ToString() + _r863ManualCockpitFreq4DialPos.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78016, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SaveCockpitFrequencyR863Manual()");
        }

        private void SaveCockpitFrequencyYaDRO1A()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SaveCockpitFrequencyYaDRO1A()");
                /*
                 * 02000
                 * 17999
                 * 
                 * Dial 1
                 * 02 - 17
                 * 
                 * Dial 2
                 * 0 - 9
                 * 
                 * "."
                 * 
                 * Dial 3
                 * 0 - 9
                 * 
                 * Dial 4
                 * 0 - 9
                 */

                lock (_lockYADRO1ADialsObject1)
                {
                    lock (_lockYADRO1ADialsObject2)
                    {
                        lock (_lockYADRO1ADialsObject3)
                        {
                            lock (_lockYADRO1ADialsObject4)
                            {
                                _yadro1aSavedCockpitBigFrequency = uint.Parse(_yadro1aCockpitFreq1DialPos.ToString() + _yadro1aCockpitFreq2DialPos.ToString());
                                _yadro1aSavedCockpitSmallFrequency = uint.Parse(_yadro1aCockpitFreq3DialPos.ToString() + _yadro1aCockpitFreq4DialPos.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78036, ex);
            }
            Common.DebugP("Leaving Mi-8 Radio SaveCockpitFrequencyYaDRO1A()");
        }

        private string GetCommandDirectionForR863ManualDial1(int desiredDialPosition, uint actualDialPosition)
        {
            var inc = "INC\n";
            var dec = "DEC\n";
            try
            {
                /*
                 * Min is 10
                 * Max is 39
                 */


                Common.DebugP("Entering Mi-8 Radio GetCommandDirectionForR863ManualDial1()");
                //count up
                var tmpActualDialPositionUp = actualDialPosition;
                var upCount = actualDialPosition;
                do
                {
                    Common.DebugP("tmpActualDialPositionUp " + tmpActualDialPositionUp + " desiredDialPosition " + desiredDialPosition);
                    if (tmpActualDialPositionUp == 39)
                    {
                        tmpActualDialPositionUp = 10;
                    }
                    else if (tmpActualDialPositionUp == 14)
                    {
                        tmpActualDialPositionUp = 22;
                    }
                    else
                    {
                        tmpActualDialPositionUp++;
                    }
                    upCount++;
                } while (tmpActualDialPositionUp != desiredDialPosition);

                //down up
                var tmpActualDialPositionDown = actualDialPosition;
                var downCount = actualDialPosition;
                do
                {
                    Common.DebugP("tmpActualDialPositionDown " + tmpActualDialPositionDown + " desiredDialPosition " + desiredDialPosition);
                    if (tmpActualDialPositionDown == 10)
                    {
                        tmpActualDialPositionDown = 39;
                    }
                    else if (tmpActualDialPositionDown == 22)
                    {
                        tmpActualDialPositionDown = 14;
                    }
                    else
                    {
                        tmpActualDialPositionDown--;
                    }
                    downCount++;
                } while (tmpActualDialPositionDown != desiredDialPosition);


                if (upCount < downCount)
                {
                    Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionForR863ManualDial1()");
                    return inc;
                }
                Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionForR863ManualDial1()");
                return dec;
            }
            catch (Exception ex)
            {
                Common.LogError(78017, ex);
            }
            return inc;
        }

        private string GetCommandDirectionFor0To9Dials(int desiredDialPosition, uint actualDialPosition)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                var inc = "INC\n";
                var dec = "DEC\n";
                switch (desiredDialPosition)
                {
                    case 0:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //-4 DEC
                                        return dec;
                                    }
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //5 INC
                                        return inc;
                                    }
                            }
                            break;
                        }
                    case 1:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 1:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                            }
                            break;
                        }
                    case 2:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 2:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                            }
                            break;
                        }
                    case 3:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                case 2:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 3:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                            }
                            break;
                        }
                    case 4:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 4:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                            }
                            break;
                        }
                    case 5:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 5:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                            }
                            break;
                        }
                    case 6:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 6:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                            }
                            break;
                        }
                    case 7:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 7:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                            }
                            break;
                        }
                    case 8:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                case 2:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 8:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                            }
                            break;
                        }
                    case 9:
                        {
                            switch (actualDialPosition)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 9:
                                    {
                                        Common.DebugP("Leaving Mi-8 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78018, ex);
            }
            throw new Exception("Should not reach this code. private String GetCommandDirectionFor0To9Dials(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        private string GetR863ManualDialFrequencyForPosition(uint position)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                //        "00"  "25" "50" "75"
                //          0    2    5    7  
                switch (position)
                {
                    case 0:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "0";
                        }
                    case 2:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "5";
                        }
                    case 5:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "5";
                        }
                    case 7:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "0";
                        }
                }
                Common.DebugP("ERROR!!! Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
            }
            catch (Exception ex)
            {
                Common.LogError(78019, ex);
            }
            return "";
        }

        private bool SkipR863PresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipR863PresetDialChange()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.R863_PRESET || _currentLowerRadioMode == CurrentMi8RadioMode.R863_PRESET)
                {
                    if (_r863PresetDialSkipper > 2)
                    {
                        _r863PresetDialSkipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipR863PresetDialChange()");
                        return false;
                    }
                    _r863PresetDialSkipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipR863PresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipR863PresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        private bool SkipR828PresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipR828PresetDialChange()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.R828_PRESETS || _currentLowerRadioMode == CurrentMi8RadioMode.R828_PRESETS)
                {
                    if (_r828PresetDialSkipper > 2)
                    {
                        _r828PresetDialSkipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipR828PresetDialChange()");
                        return false;
                    }
                    _r828PresetDialSkipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipR828PresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipR828PresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            return false;
        }

        private bool SkipADFPresetDial1Change()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipADFPresetDial1Change()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 || _currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9)
                {
                    if (_adfPresetDial1Skipper > 2)
                    {
                        _adfPresetDial1Skipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipADFPresetDial1Change()");
                        return false;
                    }
                    _adfPresetDial1Skipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipADFPresetDial1Change()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipADFPresetDial1Change()");
            }
            catch (Exception ex)
            {
                Common.LogError(78010, ex);
            }
            return false;
        }

        private bool SkipADFPresetDial2Change()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipADFPresetDial2Change()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 || _currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9)
                {
                    if (_adfPresetDial2Skipper > 2)
                    {
                        _adfPresetDial2Skipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipADFPresetDial2Change()");
                        return false;
                    }
                    _adfPresetDial2Skipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipADFPresetDial2Change()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipADFPresetDial2Change()");
            }
            catch (Exception ex)
            {
                Common.LogError(78010, ex);
            }
            return false;
        }

        private bool SkipSPU7PresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipSPU7PresetDialChange()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.SPU7 || _currentLowerRadioMode == CurrentMi8RadioMode.SPU7)
                {
                    if (_spu7DialSkipper > 2)
                    {
                        _spu7DialSkipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipSPU7PresetDialChange()");
                        return false;
                    }
                    _spu7DialSkipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipSPU7PresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipSPU7PresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78110, ex);
            }
            return false;
        }

    }
}
