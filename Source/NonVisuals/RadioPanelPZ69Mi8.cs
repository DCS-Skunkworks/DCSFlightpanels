using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69Mi8 : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private CurrentMi8RadioMode _currentUpperRadioMode = CurrentMi8RadioMode.R863_MANUAL;
        private CurrentMi8RadioMode _currentLowerRadioMode = CurrentMi8RadioMode.R863_MANUAL;

        /*Mi-8 VHF/UHF R-863 MANUAL COM1*/
        //Large dial 100-149  -> 220 - 399 [step of 1]
        //Small dial 0 - 95
        private readonly ClickSpeedDetector _bigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _bigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        const int ChangeValue = 10;
        //private int[] _r863ManualFreq1DialValues = { 10, 11, 12, 13, 14, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 };
        private volatile uint _r863ManualBigFrequencyStandby = 108;
        private volatile uint _r863ManualSmallFrequencyStandby;
        private volatile uint _r863ManualSavedCockpitBigFrequency;
        private volatile uint _r863ManualSavedCockpitSmallFrequency;
        private readonly object _lockR863ManualDialsObject1 = new object();
        private readonly object _lockR863ManualDialsObject2 = new object();
        private readonly object _lockR863ManualDialsObject3 = new object();
        private readonly object _lockR863ManualDialsObject4 = new object();
        private volatile uint _r863ManualCockpitFreq1DialPos = 1;
        private volatile uint _r863ManualCockpitFreq2DialPos = 1;
        private volatile uint _r863ManualCockpitFreq3DialPos = 1;
        private volatile uint _r863ManualCockpitFreq4DialPos = 0;
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
        //ACT/STBY Unit Switch, DIAL/MEMORY
        private readonly object _lockR863Preset1DialObject1 = new object();
        private DCSBIOSOutput _r863Preset1DcsbiosOutputPresetDial;
        private volatile uint _r863PresetCockpitDialPos = 1;
        private const string R863PresetCommandInc = "R863_CNL_SEL INC\n";
        private const string R863PresetCommandDec = "R863_CNL_SEL DEC\n";
        private int _r863PresetDialSkipper;
        private const string R863PresetVolumeKnobCommandInc = "R863_VOL +2500\n";
        private const string R863PresetVolumeKnobCommandDec = "R863_VOL -2500\n";
        private readonly object _lockR863UnitSwitchObject = new object();
        private DCSBIOSOutput _r863UnitSwitchDcsbiosOutput;
        private volatile uint _r863UnitSwitchCockpitPos = 1;
        private const string R863UnitSwitchCommandToggle = "R863_UNIT_SWITCH TOGGLE\n";

        /*Mi-8 YaDRO 1A NAV1*/
        //Large dial 100-149  -> 20 - 179 [step of 1]
        //Small dial 0 - 99
        private readonly ClickSpeedDetector _yadro1aBigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _yadro1aBigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private volatile uint _yadro1aBigFrequencyStandby = 100;
        private volatile uint _yadro1aSmallFrequencyStandby;
        private volatile uint _yadro1aSavedCockpitBigFrequency;
        private volatile uint _yadro1aSavedCockpitSmallFrequency;
        private readonly object _lockYADRO1ADialsObject1 = new object();
        private readonly object _lockYADRO1ADialsObject2 = new object();
        private readonly object _lockYADRO1ADialsObject3 = new object();
        private readonly object _lockYADRO1ADialsObject4 = new object();
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
        //ACT/STBY AGC, automatic gain control
        private readonly object _lockR828Preset1DialObject1 = new object();
        private DCSBIOSOutput _r828Preset1DcsbiosOutputDial;
        private volatile uint _r828PresetCockpitDialPos = 1;
        private const string R828PresetCommandInc = "R828_PRST_CHAN_SEL INC\n";
        private const string R828PresetCommandDec = "R828_PRST_CHAN_SEL DEC\n";
        private int _r828PresetDialSkipper;
        private const string R828PresetVolumeKnobCommandInc = "R828_VOL +2500\n";
        private const string R828PresetVolumeKnobCommandDec = "R828_VOL -2500\n";
        private const string R828GainControlCommandOn = "R828_TUNER INC\n";
        private const string R828GainControlCommandOff = "R828_TUNER DEC\n";

        /*Mi-8 ARK-9 ADF MAIN*/
        //Large 100KHz 01 -> 12
        //Small 10Khz 00 -> 90 (10 steps)
        private readonly object _lockADFMainDialObject1 = new object();
        private readonly object _lockADFMainDialObject2 = new object();
        private DCSBIOSOutput _adfMainDcsbiosOutputPresetDial1;
        private DCSBIOSOutput _adfMainDcsbiosOutputPresetDial2;
        private volatile uint _adfMainCockpitPresetDial1Pos = 1;
        private volatile uint _adfMainCockpitPresetDial2Pos = 1;
        private const string ADFMain100KhzPresetCommandInc = "ARC_MAIN_100KHZ INC\n";
        private const string ADFMain100KhzPresetCommandDec = "ARC_MAIN_100KHZ DEC\n";
        private const string ADFMain10KhzPresetCommandInc = "ARC_MAIN_10KHZ INC\n";
        private const string ADFMain10KhzPresetCommandDec = "ARC_MAIN_10KHZ DEC\n";
        /*
         *  ADF BACKUP
         */
        private readonly object _lockADFBackupDialObject1 = new object();
        private readonly object _lockADFBackupDialObject2 = new object();
        private DCSBIOSOutput _adfBackupDcsbiosOutputPresetDial1;
        private DCSBIOSOutput _adfBackupDcsbiosOutputPresetDial2;
        private volatile uint _adfBackupCockpitPresetDial1Pos = 1;
        private volatile uint _adfBackupCockpitPresetDial2Pos = 1;
        private const string ADFBackup100KhzPresetCommandInc = "ARC_BCK_100KHZ INC\n";
        private const string ADFBackup100KhzPresetCommandDec = "ARC_BCK_100KHZ DEC\n";
        private const string ADFBackup10KhzPresetCommandInc = "ARC_BCK_10KHZ INC\n";
        private const string ADFBackup10KhzPresetCommandDec = "ARC_BCK_10KHZ DEC\n";
        private int _adfPresetDial1Skipper;
        private int _adfPresetDial2Skipper;
        //0 = Backup ADF
        //1 = Main ADF
        private readonly object _lockADFBackupMainDialObject = new object();
        private DCSBIOSOutput _adfBackupMainDcsbiosOutputPresetDial;
        private volatile uint _adfBackupMainCockpitDial1Pos = 0;
        private const string ADFBackupMainSwitchToggleCommand = "ARC9_MAIN_BACKUP TOGGLE\n";

        /*Mi-8 ARK-9 ADF (DME)*/
        //Large Tuning
        //Radio Volume
        /*private const string ADFTuneKnobCommandInc = "ARC9_MAIN_TUNE +500\n";
        private const string ADFTuneKnobCommandDec = "ARC9_MAIN_TUNE -500\n";
        private const string ADFVolumeKnobCommandInc = "ARC9_VOL +2500\n";
        private const string ADFVolumeKnobCommandDec = "ARC9_VOL -2500\n";*/

        /*
         *  ACT/STBY Toggling ADF mode
         */
        /*private readonly object _lockADFModeDialObject = new object();
        private DCSBIOSOutput _adfModeDcsbiosOutputPresetDial;
        private volatile uint _adfModeCockpitDial1Pos = 0;
        private const string ADFModeCommandInc = "ARC9_MODE INC\n";
        private const string ADFModeCommandDec = "ARC9_MODE DEC\n";
        private bool _adfModeSwitchUpwards = false;*/

        /*Mi-8 ARK-UD VHF Homing (DME)*/
        //Large Frequency 1-6
        //Small Mode
        //ACT/STBY   VHF/UHF
        private readonly object _lockARKUDPresetDialObject = new object();
        private DCSBIOSOutput _arkUDPresetDcsbiosOutputPresetDial;
        private volatile uint _arkUDPresetCockpitDial1Pos = 0;
        private const string ARKUDPresetCommandInc = "ARCUD_CHL INC\n";
        private const string ARKUDPresetCommandDec = "ARCUD_CHL DEC\n";
        private int _arkUDPresetDialSkipper;

        private readonly object _lockARKUDModeDialObject = new object();
        private DCSBIOSOutput _arkUDModeDcsbiosOutputDial;
        private volatile uint _arkUDModeCockpitDial1Pos = 0;
        private const string ARKUDModeCommandInc = "ARCUD_MODE INC\n";
        private const string ARKUDModeCommandDec = "ARCUD_MODE DEC\n";
        private int _arkUDModeDialSkipper;

        private readonly object _lockARKUDVhfUhfModeDialObject = new object();
        private DCSBIOSOutput _arkUDVhfUhfModeDcsbiosOutputDial;
        private volatile uint _arkUDVhfUhfModeCockpitDial1Pos = 0;
        private const string ARKUDVhfUhfModeCommandToggle = "ARCUD_WAVE TOGGLE\n";


        //XPDR
        /*Mi-8 SPU-7 XPDR*/
        //Large dial 0-5 [step of 1]
        //Small dial volume control
        //ACT/STBY Toggle Radio/ICS Switch
        private readonly object _lockSPU7DialObject1 = new object();
        private DCSBIOSOutput _spu7DcsbiosOutputPresetDial;
        private volatile uint _spu7CockpitDialPos = 0;
        private int _spu7DialSkipper;
        private const string SPU7CommandInc = "RADIO_SEL_R INC\n";
        private const string SPU7CommandDec = "RADIO_SEL_R DEC\n";
        private const string SPU7VolumeKnobCommandInc = "LST_VOL_KNOB_L +2500\n";
        private const string SPU7VolumeKnobCommandDec = "LST_VOL_KNOB_L -2500\n";
        private readonly object _lockSPU7ICSSwitchObject = new object();
        private DCSBIOSOutput _spu7ICSSwitchDcsbiosOutput;
        private volatile uint _spu7ICSSwitchCockpitDialPos = 0;
        private const string SPU7ICSSwitchToggleCommand = "SPU7_L_ICS TOGGLE\n";

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
            _r863ManualSyncThread?.Abort();
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

                if (e.Address.Equals(_yadro1aDcsbiosOutputCockpitFrequency.Address))
                {
                    // "02000.0" - "17999.9"
                    // Last digit not used in panel


                    var tmpFreq = double.Parse(e.StringData, NumberFormatInfoFullDisplay);
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
                        _yadro1aCockpitFreq1DialPos = uint.Parse(e.StringData.Substring(0, 2));
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
                        _yadro1aCockpitFreq2DialPos = uint.Parse(e.StringData.Substring(2, 1));
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
                        _yadro1aCockpitFreq3DialPos = uint.Parse(e.StringData.Substring(3, 1));
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
                        _yadro1aCockpitFreq4DialPos = uint.Parse(e.StringData.Substring(4, 1));
                        Common.DebugP("Just read YaDRO-1A dial 4 position: " + _yadro1aCockpitFreq4DialPos + "  " + Environment.TickCount);
                        if (tmp != _yadro1aCockpitFreq4DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _yadro1aDial4WaitingForFeedback, 0);
                        }
                    }
                }
                if (e.Address.Equals(_r863ManualDcsbiosOutputCockpitFrequency.Address))
                {
                    // "100.000" - "399.975"
                    // Last digit not used in panel


                    var tmpFreq = double.Parse(e.StringData, NumberFormatInfoFullDisplay);
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
                        _r863ManualCockpitFreq1DialPos = uint.Parse(e.StringData.Substring(0, 2));
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
                        _r863ManualCockpitFreq2DialPos = uint.Parse(e.StringData.Substring(2, 1));
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
                        _r863ManualCockpitFreq3DialPos = uint.Parse(e.StringData.Substring(4, 1));
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
                        _r863ManualCockpitFreq4DialPos = uint.Parse(e.StringData.Substring(5, 2));
                        Common.DebugP("Just read R-863 dial 4 position: " + _r863ManualCockpitFreq4DialPos + "  " + Environment.TickCount);
                        if (tmp != _r863ManualCockpitFreq4DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
            }
            ShowFrequenciesOnPanel();
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



                //R-863 Preset Channel Dial
                if (e.Address == _r863Preset1DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockR863Preset1DialObject1)
                    {
                        var tmp = _r863PresetCockpitDialPos;
                        _r863PresetCockpitDialPos = _r863Preset1DcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _r863PresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //R-863 Unit Switch
                if (e.Address == _r863UnitSwitchDcsbiosOutput.Address)
                {
                    lock (_lockR863UnitSwitchObject)
                    {
                        var tmp = _r863UnitSwitchCockpitPos;
                        _r863UnitSwitchCockpitPos = _r863UnitSwitchDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _r863UnitSwitchCockpitPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //YaDRO-1A

                //R-828 Preset Channel Dial
                if (e.Address == _r828Preset1DcsbiosOutputDial.Address)
                {
                    lock (_lockR828Preset1DialObject1)
                    {
                        var tmp = _r828PresetCockpitDialPos;
                        _r828PresetCockpitDialPos = _r828Preset1DcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _r828PresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Main Preset Dial 1
                if (e.Address == _adfMainDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockADFMainDialObject1)
                    {
                        var tmp = _adfMainCockpitPresetDial1Pos;
                        _adfMainCockpitPresetDial1Pos = _adfMainDcsbiosOutputPresetDial1.GetUIntValue(e.Data);
                        if (tmp != _adfMainCockpitPresetDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Main Preset Dial 2
                if (e.Address == _adfMainDcsbiosOutputPresetDial2.Address)
                {
                    lock (_lockADFMainDialObject2)
                    {
                        var tmp = _adfMainCockpitPresetDial2Pos;
                        _adfMainCockpitPresetDial2Pos = _adfMainDcsbiosOutputPresetDial2.GetUIntValue(e.Data);
                        if (tmp != _adfMainCockpitPresetDial2Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Backup Preset Dial 1
                if (e.Address == _adfBackupDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockADFBackupDialObject1)
                    {
                        var tmp = _adfBackupCockpitPresetDial1Pos;
                        _adfBackupCockpitPresetDial1Pos = _adfBackupDcsbiosOutputPresetDial1.GetUIntValue(e.Data);
                        if (tmp != _adfBackupCockpitPresetDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Backup Preset Dial 2
                if (e.Address == _adfBackupDcsbiosOutputPresetDial2.Address)
                {
                    lock (_lockADFBackupDialObject2)
                    {
                        var tmp = _adfBackupCockpitPresetDial2Pos;
                        _adfBackupCockpitPresetDial2Pos = _adfBackupDcsbiosOutputPresetDial2.GetUIntValue(e.Data);
                        if (tmp != _adfBackupCockpitPresetDial2Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Backup or Main
                if (e.Address == _adfBackupMainDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockADFBackupMainDialObject)
                    {
                        var tmp = _adfBackupMainCockpitDial1Pos;
                        _adfBackupMainCockpitDial1Pos = _adfBackupMainDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _adfBackupMainCockpitDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARK-UD  VHF Homing Preset Channels
                if (e.Address == _arkUDPresetDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockARKUDPresetDialObject)
                    {
                        var tmp = _arkUDPresetCockpitDial1Pos;
                        _arkUDPresetCockpitDial1Pos = _arkUDPresetDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _arkUDPresetCockpitDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARK-UD  Mode 
                if (e.Address == _arkUDModeDcsbiosOutputDial.Address)
                {
                    lock (_lockARKUDModeDialObject)
                    {
                        var tmp = _arkUDModeCockpitDial1Pos;
                        _arkUDModeCockpitDial1Pos = _arkUDModeDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _arkUDModeCockpitDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ARK-UD  VHF/UHF
                if (e.Address == _arkUDVhfUhfModeDcsbiosOutputDial.Address)
                {
                    lock (_lockARKUDVhfUhfModeDialObject)
                    {
                        var tmp = _arkUDVhfUhfModeCockpitDial1Pos;
                        _arkUDVhfUhfModeCockpitDial1Pos = _arkUDVhfUhfModeDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _arkUDVhfUhfModeCockpitDial1Pos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //SPU-7 Dial
                if (e.Address == _spu7DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockSPU7DialObject1)
                    {
                        var tmp = _spu7CockpitDialPos;
                        _spu7CockpitDialPos = _spu7DcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _spu7CockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //SPU-7 Radio/ICS
                if (e.Address == _spu7ICSSwitchDcsbiosOutput.Address)
                {
                    lock (_lockSPU7ICSSwitchObject)
                    {
                        var tmp = _spu7ICSSwitchCockpitDialPos;
                        _spu7ICSSwitchCockpitDialPos = _spu7ICSSwitchDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _spu7ICSSwitchCockpitDialPos)
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


        private void SendFrequencyToDCSBIOS(bool knobIsOn, RadioPanelPZ69KnobsMi8 knob)
        {
            try
            {
                if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH))
                {
                    //Don't do anything on the very first button press as the panel sends ALL
                    //switches when it is manipulated the first time
                    //This would cause unintended sync.
                    return;
                }
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
                                        DCSBIOS.Send(knobIsOn ? R828GainControlCommandOn : R828GainControlCommandOff);
                                        break;
                                    }
                                case CurrentMi8RadioMode.ADF_ARK9:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.SPU7:
                                    {
                                        if (knobIsOn)
                                        {
                                            DCSBIOS.Send(SPU7ICSSwitchToggleCommand);
                                        }
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
                                        DCSBIOS.Send(knobIsOn ? R828GainControlCommandOn : R828GainControlCommandOff);
                                        break;
                                    }
                                case CurrentMi8RadioMode.ADF_ARK9:
                                    {
                                        break;
                                    }
                                case CurrentMi8RadioMode.SPU7:
                                    {
                                        if (knobIsOn)
                                        {
                                            DCSBIOS.Send(SPU7ICSSwitchToggleCommand);
                                        }
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


                _r863ManualSyncThread?.Abort();
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

                        var frequencyAsString = _r863ManualBigFrequencyStandby.ToString() + "." + _r863ManualSmallFrequencyStandby.ToString().PadLeft(3, '0');
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
                        desiredPositionDial1X = int.Parse(frequencyAsString.Substring(0, 2));//Array.IndexOf(_r863ManualFreq1DialValues, int.Parse(xfrequencyAsString.Substring(0, 2)));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(4, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(5, 2));
                        Debug.WriteLine(" frequencyAsString : " + frequencyAsString);
                        Debug.WriteLine("Desired position Dial 1 : " + desiredPositionDial1X);
                        Debug.WriteLine("Desired position Dial 2 : " + desiredPositionDial2X);
                        Debug.WriteLine("Desired position Dial 3 : " + desiredPositionDial3X);
                        Debug.WriteLine("Desired position Dial 4 : " + desiredPositionDial4X);
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

                            string str;
                            if (Interlocked.Read(ref _r863ManualDial1WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject1)
                                {
                                    Common.DebugP("_r863ManualCockpitFreq1DialPos is " + _r863ManualCockpitFreq1DialPos + " and should be " + desiredPositionDial1X);
                                    if (_r863ManualCockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq1DialCommand + "DEC\n";//TODO is this still a problem? 30.7.2018 Went into loop GetCommandDirectionForR863ManualDial1(desiredPositionDial1X, _r863ManualCockpitFreq1DialPos);
                                        /*
                                            25.7.2018
                                            10	0.22999967634678
                                            39	0.21999971568584

                                            10Mhz  Rotary Knob (Mi-8MT/R863_FREQ1)
                                            Changing the dial (in cockpit) 39 => 10 does not show in DCS-BIOS in the CTRL-Ref page. But going down from 10 => 39 is OK.
                                            I have gotten the values above from DCS using the Lua console so I can see that they do actually change but there is something not working in DCS-BIOS
                                            So only go down with it
                                         */

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

                            if (Interlocked.Read(ref _r863ManualDial4WaitingForFeedback) == 0)
                            {

                                lock (_lockR863ManualDialsObject4)
                                {
                                    Common.DebugP("_r863ManualCockpitFreq4DialPos is " + _r863ManualCockpitFreq4DialPos + " and should be " + desiredPositionDial4X);
                                    Debug.WriteLine("_r863ManualCockpitFreq4DialPos is " + _r863ManualCockpitFreq4DialPos + " and should be " + desiredPositionDial4X);
                                    if (_r863ManualCockpitFreq4DialPos < desiredPositionDial4X)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R863ManualFreq4DialCommand + "INC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 1);
                                    }
                                    else if (_r863ManualCockpitFreq4DialPos > desiredPositionDial4X)
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
                            //if (dial1SendCount > 12 || dial2SendCount > 10 || dial3SendCount > 10 || dial4SendCount > 5)
                            /* ATTN ! 12.05.2019
                             * This radio is problematic, sometimes it goes bonkers, DCS-BIOS doesn't see the update from DCS
                             * and everything goes wrong after that. Been like this from start.
                             * Added big send counts here just to make sure it has time to find the correct position.
                             */
                            if (dial1SendCount > 40 || dial2SendCount > 20 || dial3SendCount > 20 || dial4SendCount > 10)
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
                    catch (ThreadAbortException ex)
                    {
                        Common.LogError(56442, ex);
                    }
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
                Common.DebugP("x_r863ManualSmallFrequencyStandby  " + _r863ManualSmallFrequencyStandby);
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


                _yadro1aSyncThread?.Abort();
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

                            string str;
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
                lock (LockLCDUpdateObject)
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
                            case RadioPanelPZ69KnobsMi8.UPPER_ARK_UD:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.ARK_UD);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_ARK_UD:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.ARK_UD);
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
                                            DCSBIOS.Send(R863UnitSwitchCommandToggle);
                                        }
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ADFBackupMainSwitchToggleCommand);
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi8RadioMode.ARK_UD && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ARKUDVhfUhfModeCommandToggle);
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentMi8RadioMode.R863_PRESET)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(R863UnitSwitchCommandToggle);
                                        }
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9 && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ADFBackupMainSwitchToggleCommand);
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi8RadioMode.ARK_UD && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ARKUDVhfUhfModeCommandToggle);
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH);
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain100KhzPresetCommandInc : ADFBackup100KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDPresetCommandInc);
                                                }
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain100KhzPresetCommandDec : ADFBackup100KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDPresetCommandDec);
                                                }
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
                                                if (_r863ManualSmallFrequencyStandby >= 975)
                                                {
                                                    //At max value
                                                    _r863ManualSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby + 25;
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain10KhzPresetCommandInc : ADFBackup10KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDModeCommandInc);
                                                }
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
                                                    _r863ManualSmallFrequencyStandby = 975;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby - 25;
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain10KhzPresetCommandDec : ADFBackup10KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDModeCommandDec);
                                                }
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain100KhzPresetCommandInc : ADFBackup100KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDPresetCommandInc);
                                                }
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain100KhzPresetCommandDec : ADFBackup100KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDPresetCommandDec);
                                                }
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
                                                if (_r863ManualSmallFrequencyStandby >= 975)
                                                {
                                                    //At max value
                                                    _r863ManualSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby + 25;
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain10KhzPresetCommandInc : ADFBackup10KhzPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDModeCommandInc);
                                                }
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
                                                    _r863ManualSmallFrequencyStandby = 975;
                                                    break;
                                                }
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby - 25;
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
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADFMain10KhzPresetCommandDec : ADFBackup10KhzPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipARKUDModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUDModeCommandDec);
                                                }
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

                        return;
                    }

                    if (!FirstReportHasBeenRead)
                    {

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
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq4DialPos.ToString().PadLeft(2, '0');
                                    //GetR863ManualDialFrequencyForPosition(_r863ManualCockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentMi8RadioMode.R863_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                var channelAsString = "";
                                uint unitSwitch = 0;
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = (_r863PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                lock (_lockR863UnitSwitchObject)
                                {
                                    unitSwitch = _r863UnitSwitchCockpitPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, unitSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)_yadro1aCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0')), PZ69LCDPosition.UPPER_STBY_RIGHT);
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ADF_ARK9:
                            {
                                //Dial1 XX00
                                //Dial2 00XX

                                var channelAsString = "";
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject1)
                                        {
                                            channelAsString = (_adfMainCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            channelAsString = (_adfBackupCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfMainCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfBackupCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ARK_UD:
                            {
                                var stringToBeShownLeft = "";
                                uint arkPreset = 0;
                                uint arkMode = 0;
                                uint arkBand = 0;
                                lock (_lockARKUDPresetDialObject)
                                {
                                    arkPreset = _arkUDPresetCockpitDial1Pos + 1;
                                }
                                lock (_lockARKUDModeDialObject)
                                {
                                    arkMode = _arkUDModeCockpitDial1Pos;
                                }
                                lock (_lockARKUDVhfUhfModeDialObject)
                                {
                                    arkBand = _arkUDVhfUhfModeCockpitDial1Pos;
                                }
                                //1 4 5
                                //12345
                                stringToBeShownLeft = arkBand + "   " + arkMode;
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, arkPreset, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, stringToBeShownLeft, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.SPU7:
                            {
                                //0-5
                                var channelAsString = "";
                                uint spuICSSwitch = 0;
                                lock (_lockSPU7DialObject1)
                                {
                                    channelAsString = (_spu7CockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                lock (_lockSPU7ICSSwitchObject)
                                {
                                    spuICSSwitch = _spu7ICSSwitchCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, spuICSSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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

                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq4DialPos.ToString().PadLeft(2, '0');//GetR863ManualDialFrequencyForPosition(_r863ManualCockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentMi8RadioMode.R863_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                var channelAsString = "";
                                uint unitSwitch = 0;
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = (_r863PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                lock (_lockR863UnitSwitchObject)
                                {
                                    unitSwitch = _r863UnitSwitchCockpitPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, unitSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)_yadro1aCockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0')), PZ69LCDPosition.LOWER_STBY_RIGHT);
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ADF_ARK9:
                            {
                                //Dial1 XX00
                                //Dial2 00XX

                                var channelAsString = "";
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject1)
                                        {
                                            channelAsString = (_adfMainCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            channelAsString = (_adfBackupCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfMainCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfBackupCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.ARK_UD:
                            {
                                var stringToBeShownLeft = "";
                                uint arkPreset = 0;
                                uint arkMode = 0;
                                uint arkBand = 0;
                                lock (_lockARKUDPresetDialObject)
                                {
                                    arkPreset = _arkUDPresetCockpitDial1Pos + 1;
                                }
                                lock (_lockARKUDModeDialObject)
                                {
                                    arkMode = _arkUDModeCockpitDial1Pos;
                                }
                                lock (_lockARKUDVhfUhfModeDialObject)
                                {
                                    arkBand = _arkUDVhfUhfModeCockpitDial1Pos;
                                }
                                //1 4 5
                                //12345
                                stringToBeShownLeft = arkBand + "   " + arkMode;
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, arkPreset, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, stringToBeShownLeft, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.SPU7:
                            {
                                //0-5
                                var channelAsString = "";
                                uint spuICSSwitch = 0;
                                lock (_lockSPU7DialObject1)
                                {
                                    channelAsString = (_spu7CockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                lock (_lockSPU7ICSSwitchObject)
                                {
                                    spuICSSwitch = _spu7ICSSwitchCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, spuICSSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi8RadioMode.NOUSE:
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
            Common.DebugP("Leaving Mi-8 Radio ShowFrequenciesOnPanel()");
        }


        protected override void SaitekPanelKnobChanged(IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(hashSet);
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
                _r863UnitSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("R863_UNIT_SWITCH");

                //NAV1
                _yadro1aDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("YADRO1A_FREQ");
                DCSBIOSStringListenerHandler.AddAddress(_yadro1aDcsbiosOutputCockpitFrequency.Address, 7, this);

                //NAV2
                _r828Preset1DcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R828_PRST_CHAN_SEL");

                //ADF
                _adfMainDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_MAIN_100KHZ");
                _adfMainDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_MAIN_10KHZ");
                _adfBackupDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_BCK_100KHZ");
                _adfBackupDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_BCK_10KHZ");
                _adfBackupMainDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC9_MAIN_BACKUP");

                //DME
                _arkUDPresetDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARCUD_CHL");
                _arkUDModeDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARCUD_MODE");
                _arkUDVhfUhfModeDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARCUD_WAVE");

                //XPDR
                _spu7DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RADIO_SEL_L");
                _spu7ICSSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("SPU7_L_ICS");


                StartListeningForPanelChanges();
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69Mi8.StartUp() : " + ex.Message);
                Common.LogError(321654, ex);
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
            SaitekPanelKnobs = RadioPanelKnobMi8.GetRadioPanelKnobs();
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
            const string inc = "INC\n";
            const string dec = "DEC\n";
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

                //count down
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
                const string inc = "INC\n";
                const string dec = "DEC\n";

                var tmpActualDialPositionUp = actualDialPosition;
                var upCount = actualDialPosition;
                do
                {
                    Common.DebugP("tmpActualDialPositionUp " + tmpActualDialPositionUp + " desiredDialPosition " + desiredDialPosition);

                    if (tmpActualDialPositionUp == 9)
                    {
                        tmpActualDialPositionUp = 0;
                    }
                    else
                    {
                        tmpActualDialPositionUp++;
                    }

                    upCount++;
                } while (tmpActualDialPositionUp != desiredDialPosition);

                tmpActualDialPositionUp = actualDialPosition;
                var downCount = actualDialPosition;
                do
                {
                    Common.DebugP("tmpActualDialPositionUp " + tmpActualDialPositionUp + " desiredDialPosition " + desiredDialPosition);

                    if (tmpActualDialPositionUp == 0)
                    {
                        tmpActualDialPositionUp = 9;
                    }
                    else
                    {
                        tmpActualDialPositionUp--;
                    }

                    downCount++;
                } while (tmpActualDialPositionUp != desiredDialPosition);


                Common.DebugP("GetCommandDirectionFor0To9Dials()");
                if (upCount < downCount)
                {
                    return inc;
                }

                return dec;
            }
            catch (Exception ex)
            {
                Common.LogError(78018, ex);
            }
            throw new Exception("Should not reach this code. private String GetCommandDirectionFor0To9Dials(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }
        /*
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
                Common.LogError(1111, "ERROR!!! Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition() Position : [" + position + "]");
            }
            catch (Exception ex)
            {
                Common.LogError(78019, ex);
            }
            return "0";
        }
        */
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

        private bool SkipARKUDPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipARKUDPresetDialChange()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ARK_UD || _currentLowerRadioMode == CurrentMi8RadioMode.ARK_UD)
                {
                    if (_arkUDPresetDialSkipper > 2)
                    {
                        _arkUDPresetDialSkipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipARKUDPresetDialChange()");
                        return false;
                    }
                    _arkUDPresetDialSkipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipARKUDPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipARKUDPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78110, ex);
            }
            return false;
        }

        private bool SkipARKUDModeDialChange()
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio SkipARKUDModeDialChange()");
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ARK_UD || _currentLowerRadioMode == CurrentMi8RadioMode.ARK_UD)
                {
                    if (_arkUDModeDialSkipper > 2)
                    {
                        _arkUDModeDialSkipper = 0;
                        Common.DebugP("Leaving Mi-8 Radio SkipARKUDModeDialChange()");
                        return false;
                    }
                    _arkUDModeDialSkipper++;
                    Common.DebugP("Leaving Mi-8 Radio SkipARKUDModeDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Mi-8 Radio SkipARKUDModeDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78110, ex);
            }
            return false;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }

    }
}
