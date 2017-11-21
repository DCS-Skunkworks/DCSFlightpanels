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
    public class RadioPanelPZ69Ka50 : RadioPanelPZ69Base, IRadioPanel
    {
        private HashSet<RadioPanelKnobKa50> _radioPanelKnobs = new HashSet<RadioPanelKnobKa50>();
        private CurrentKa50RadioMode _currentUpperRadioMode = CurrentKa50RadioMode.VHF1_R828;
        private CurrentKa50RadioMode _currentLowerRadioMode = CurrentKa50RadioMode.VHF1_R828;

        /*Ka-50 VHF 1 R-828*/
        //Large dial 1-10 [step of 1]
        //Small dial volume control
        private readonly object _lockVhf1DialObject1 = new object();
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetDial;
        private volatile uint _vhf1CockpitPresetDialPos = 1;
        private const string Vhf1PresetCommandInc = "R828_CHANNEL INC\n";
        private const string Vhf1PresetCommandDec = "R828_CHANNEL DEC\n";
        private int _vhf1PresetDialSkipper;
        //private DCSBIOSOutput _vhf1DcsbiosOutputVolumeDial;
        private const string Vhf1VolumeKnobCommandInc = "R828_VOLUME +2500\n";
        private const string Vhf1VolumeKnobCommandDec = "R828_VOLUME -2500\n";
        private const string Vhf1TunerButtonPress = "R828_TUNER INC\n";
        private const string Vhf1TunerButtonRelease = "R828_TUNER DEC\n";

        /*Ka-50 VHF 2 R-800L1*/
        //Large dial 100-149  -> 220 - 399 [step of 1]
        //Small dial 0 - 95
        /*private int[] _r800l1UsedBigFrequencyValues =
        {   //These are currently used by DCS, to get more efficient usage by disregarding freqs not used in game (51 positions)
            108, 109, 110, 111, 113, 114, 115, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130,131, 132, 133, 134, 135, 136, 138, 139, 140, 141, 250, 251, 252, 253, 254, 255, 256,257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 344, 365, 385
            //
        };*/
        private ClickSpeedDetector _r800l1BigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _r800l1BigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        const int ChangeValue = 10;
        //private long _changesWithinLastticksSinceLastChangeLargeDial;
        private int[] _r800l1Freq1DialValues = { 10, 11, 12, 13, 14, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 };
        private volatile uint _r800l1BigFrequencyStandby = 108;
        private volatile uint _r800l1SmallFrequencyStandby;
        private volatile uint _r800l1SavedCockpitBigFrequency;
        private volatile uint _r800l1SavedCockpitSmallFrequency;
        private object _lockR800L1DialsObject1 = new object();
        private object _lockR800L1DialsObject2 = new object();
        private object _lockR800L1DialsObject3 = new object();
        private object _lockR800L1DialsObject4 = new object();
        private DCSBIOSOutput _r800l1DcsbiosOutputFreqDial1;
        private DCSBIOSOutput _r800l1DcsbiosOutputFreqDial2;
        private DCSBIOSOutput _r800l1DcsbiosOutputFreqDial3;
        private DCSBIOSOutput _r800l1DcsbiosOutputFreqDial4;
        private volatile uint _r800l1CockpitFreq1DialPos = 1;
        private volatile uint _r800l1CockpitFreq2DialPos = 1;
        private volatile uint _r800l1CockpitFreq3DialPos = 1;
        private volatile uint _r800l1CockpitFreq4DialPos = 1;
        private const string R800L1Freq1DialCommand = "R800_FREQ1 ";
        private const string R800L1Freq2DialCommand = "R800_FREQ2 ";
        private const string R800L1Freq3DialCommand = "R800_FREQ3 ";
        private const string R800L1Freq4DialCommand = "R800_FREQ4 ";
        private Thread _r800l1SyncThread;
        private long _r800l1ThreadNowSynching;
        private long _r800l1Dial1WaitingForFeedback;
        private long _r800l1Dial2WaitingForFeedback;
        private long _r800l1Dial3WaitingForFeedback;
        private long _r800l1Dial4WaitingForFeedback;


        /*Ka-50 ARK-22 ADF*/
        //Large dial 0-9 [step of 1]
        //Small dial volume control
        //ACT/STBY Switch between ADF Modes Inner Auto Outer
        private readonly object _lockADFDialObject1 = new object();
        private DCSBIOSOutput _adfDcsbiosOutputPresetDial;
        private volatile uint _adfCockpitPresetDialPos = 1;
        private const string ADFPresetCommandInc = "ADF_CHANNEL INC\n";
        private const string ADFPresetCommandDec = "ADF_CHANNEL DEC\n";
        private int _adfPresetDialSkipper;
        private const string ADFVolumeKnobCommandInc = "ADF_VOLUME +2500\n";
        private const string ADFVolumeKnobCommandDec = "ADF_VOLUME -2500\n";
        /*private const string ADFModeSwitchAntenna = "ADF_CMPS_ANT INC\n";
        private const string ADFModeSwitchCompass = "ADF_CMPS_ANT DEC\n";
        private string _adfModeSwitchLastSent = "";*/
        private readonly object _lockADFModeDialObject = new object();
        private DCSBIOSOutput _adfModeDcsbiosOutput;
        private volatile uint _adfModeCockpitPos = 1;
        private const string ADFModeInc = "ADF_NDB_MODE INC\n";
        private const string ADFModeDec = "ADF_NDB_MODE DEC\n";
        private bool _adfModeSwitchDirectionUp = false;



        /*Ka-50 ARBIS NAV1 (Not radio but programmed as there are so few radio systems on the KA-50*/
        //Large ARBIS Left Dial
        //Small ARBIS Right Dial
        //ACT/STBY Push Right ARBIS Dial IN/OUT
        private readonly ClickSpeedDetector _arbisLeftDialIncreaseChangeMonitor = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _arbisLeftDialDecreaseChangeMonitor = new ClickSpeedDetector(10);
        private const string ARBISLeftDialCommandIncMore = "ABRIS_BRIGHTNESS +2500\n";
        private const string ARBISLeftDialCommandDecMore = "ABRIS_BRIGHTNESS -2500\n";
        private const string ARBISLeftDialCommandInc = "ABRIS_BRIGHTNESS +1000\n";
        private const string ARBISLeftDialCommandDec = "ABRIS_BRIGHTNESS -1000\n";
        private readonly ClickSpeedDetector _arbisRightDialIncreaseChangeMonitor = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _arbisRightDialDecreaseChangeMonitor = new ClickSpeedDetector(10);
        private const string ARBISRightDialCommandIncMore = "ABRIS_CURSOR_ROT +2500\n";
        private const string ARBISRightDialCommandDecMore = "ABRIS_CURSOR_ROT -2500\n";
        private const string ARBISRightDialCommandInc = "ABRIS_CURSOR_ROT +5000\n";
        private const string ARBISRightDialCommandDec = "ABRIS_CURSOR_ROT -5000\n";
        private const string ARBISRightDialPushToggleOnCommand = "ABRIS_CURSOR_BTN 1\n";
        private const string ARBISRightDialPushToggleOffCommand = "ABRIS_CURSOR_BTN 0\n";


        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69Ka50(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69Ka50()
        {
            if (_r800l1SyncThread != null)
            {
                _r800l1SyncThread.Abort();
            }
        }


        public override void DcsBiosDataReceived(uint address, uint data)
        {
            try
            {
                //Common.DebugP("PZ69 Ka50 READ ENTERING");
                UpdateCounter(address, data);
                /*
                 * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
                 * Once a dial has been deemed to be "off" position and needs to be changed
                 * a change command is sent to DCS-BIOS.
                 * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
                 * reset. Reading the dial's position with no change in value will not reset.
                 */

                //VHF1 Preset Channel Dial
                if (address == _vhf1DcsbiosOutputPresetDial.Address)
                {
                    //Common.DebugP("VHF1 Preset Dial, waiting for lock." + Environment.TickCount);
                    lock (_lockVhf1DialObject1)
                    {
                        //Common.DebugP("Just read VHF1 Preset Dial Position: " + _vhf1CockpitPreset1DialPos + "  " + +Environment.TickCount);
                        var tmp = _vhf1CockpitPresetDialPos;
                        _vhf1CockpitPresetDialPos = _vhf1DcsbiosOutputPresetDial.GetUIntValue(data);
                        if (tmp != _vhf1CockpitPresetDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //VHF2 Dial 1 R-800L1
                if (address == _r800l1DcsbiosOutputFreqDial1.Address)
                {
                    //Common.DebugP("R-800L1 freq dial 1 position arrived, waiting for lock." + Environment.TickCount);
                    lock (_lockR800L1DialsObject1)
                    {
                        //Common.DebugP("Just read R-800L1 freq dial 1 position: " + _r800l1CockpitFreq1DialPos + "  " + +Environment.TickCount);
                        var tmp = _r800l1CockpitFreq1DialPos;
                        _r800l1CockpitFreq1DialPos = _r800l1DcsbiosOutputFreqDial1.GetUIntValue(data);
                        if (tmp != _r800l1CockpitFreq1DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            //Common.DebugP("R-800L1 freq dial 1 Before : " + tmp + "  now: " + _r800l1CockpitFreq1DialPos);
                            Interlocked.Exchange(ref _r800l1Dial1WaitingForFeedback, 0);
                        }
                    }
                }

                //VHF2 Dial 2 R-800L1
                if (address == _r800l1DcsbiosOutputFreqDial2.Address)
                {
                    //Common.DebugP("R-800L1 freq dial 2 position arrived, waiting for lock." + Environment.TickCount);
                    lock (_lockR800L1DialsObject2)
                    {
                        //Common.DebugP("Just read R-800L1 freq dial 2 position: " + _r800l1CockpitFreq2DialPos + "  " + +Environment.TickCount);
                        var tmp = _r800l1CockpitFreq2DialPos;
                        _r800l1CockpitFreq2DialPos = _r800l1DcsbiosOutputFreqDial2.GetUIntValue(data);
                        if (tmp != _r800l1CockpitFreq2DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            //Common.DebugP("R-800L1 freq dial 2 Before : " + tmp + "  now: " + _r800l1CockpitFreq2DialPos);
                            Interlocked.Exchange(ref _r800l1Dial2WaitingForFeedback, 0);
                        }
                    }
                }

                //VHF2 Dial 3 R-800L1
                if (address == _r800l1DcsbiosOutputFreqDial3.Address)
                {
                    //Common.DebugP("R-800L1 freq dial 3 position arrived, waiting for lock." + Environment.TickCount);
                    lock (_lockR800L1DialsObject3)
                    {
                        //Common.DebugP("Just read R-800L1 freq dial 3 position: " + _r800l1CockpitFreq3DialPos + "  " + +Environment.TickCount);
                        var tmp = _r800l1CockpitFreq3DialPos;
                        _r800l1CockpitFreq3DialPos = _r800l1DcsbiosOutputFreqDial3.GetUIntValue(data);
                        if (tmp != _r800l1CockpitFreq3DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            //Common.DebugP("R-800L1 freq dial 3 Before : " + tmp + "  now: " + _r800l1CockpitFreq3DialPos);
                            Interlocked.Exchange(ref _r800l1Dial3WaitingForFeedback, 0);
                        }
                    }
                }

                //VHF2 Dial 4 R-800L1
                if (address == _r800l1DcsbiosOutputFreqDial4.Address)
                {
                    //Common.DebugP("R-800L1 freq dial 4 position arrived, waiting for lock." + Environment.TickCount);
                    lock (_lockR800L1DialsObject4)
                    {
                        //Common.DebugP("Just read R-800L1 freq dial 4 position: " + _r800l1CockpitFreq4DialPos + "  " + +Environment.TickCount);
                        var tmp = _r800l1CockpitFreq4DialPos;
                        _r800l1CockpitFreq4DialPos = _r800l1DcsbiosOutputFreqDial4.GetUIntValue(data);
                        if (tmp != _r800l1CockpitFreq4DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            //Common.DebugP("R-800L1 freq dial 4 Before : " + tmp + "  now: " + _r800l1CockpitFreq4DialPos);
                            Interlocked.Exchange(ref _r800l1Dial4WaitingForFeedback, 0);
                        }
                    }
                }

                //ADF Preset Dial
                if (address == _adfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockADFDialObject1)
                    {
                        //Common.DebugP("SET _adfCockpitPresetDialPos = " + _adfCockpitPresetDialPos);
                        var tmp = _adfCockpitPresetDialPos;
                        _adfCockpitPresetDialPos = _adfDcsbiosOutputPresetDial.GetUIntValue(data);
                        if (tmp != _adfCockpitPresetDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }



                //ADF Mode
                if (address == _adfModeDcsbiosOutput.Address)
                {
                    lock (_lockADFModeDialObject)
                    {
                        var tmp = _adfModeCockpitPos;
                        _adfModeCockpitPos = _adfModeDcsbiosOutput.GetUIntValue(data);
                        if (tmp != _adfModeCockpitPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
                //Common.DebugP("PZ69 Ka50 READ EXITING");
            }
            catch (Exception ex)
            {
                Common.LogError(77001, ex);
            }
        }


        private void SendFrequencyToDCSBIOS(bool isOn, RadioPanelPZ69KnobsKa50 knob)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SendFrequencyToDCSBIOS()");
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }
                switch (knob)
                {
                    case RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentKa50RadioMode.VHF1_R828:
                                    {
                                        break;
                                    }
                                case CurrentKa50RadioMode.VHF2_R800L1:
                                    {
                                        if (isOn)
                                        {
                                            SendR800L1ToDCSBIOS();
                                        }
                                        break;
                                    }
                                case CurrentKa50RadioMode.ABRIS:
                                    {
                                        if (isOn)
                                        {
                                            DCSBIOS.Send(ARBISRightDialPushToggleOnCommand);
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(ARBISRightDialPushToggleOffCommand);
                                        }
                                        break;
                                    }
                                case CurrentKa50RadioMode.ADF_ARK22:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentKa50RadioMode.VHF1_R828:
                                    {
                                        break;
                                    }
                                case CurrentKa50RadioMode.VHF2_R800L1:
                                    {
                                        if (isOn)
                                        {
                                            SendR800L1ToDCSBIOS();
                                        }
                                        break;
                                    }
                                case CurrentKa50RadioMode.ABRIS:
                                    {
                                        if (isOn)
                                        {
                                            DCSBIOS.Send(ARBISRightDialPushToggleOnCommand);
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(ARBISRightDialPushToggleOffCommand);
                                        }
                                        break;
                                    }
                                case CurrentKa50RadioMode.ADF_ARK22:
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
                Common.LogError(77002, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SendFrequencyToDCSBIOS()");
        }


        private void SendR800L1ToDCSBIOS()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SendR800L1ToDCSBIOS()");
                if (R800L1NowSyncing())
                {
                    return;
                }
                SaveCockpitFrequencyR800L1();


                if (_r800l1SyncThread != null)
                {
                    _r800l1SyncThread.Abort();
                }
                _r800l1SyncThread = new Thread(() => R800L1SynchThreadMethod());
                _r800l1SyncThread.Start();

            }
            catch (Exception ex)
            {
                Common.LogError(77003, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SendR800L1ToDCSBIOS()");
        }

        private void R800L1SynchThreadMethod()
        {
            try
            {
                try
                {
                    try
                    {   /*
                     * Ka-50 R-800L1 VHF 2
                     */
                        Common.DebugP("Entering Ka-50 Radio R800L1SynchThreadMethod()");
                        string str;
                        Interlocked.Exchange(ref _r800l1ThreadNowSynching, 1);
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

                        var frequencyAsString = _r800l1BigFrequencyStandby.ToString() + "." + _r800l1SmallFrequencyStandby.ToString().PadLeft(2, '0');
                        frequencyAsString = frequencyAsString.PadRight(6, '0');
                        //Frequency selector 1      R800_FREQ1
                        //      "10" "11" "12" "13" "14" "22" "23" "24" "25" "26" "27" "28" "29" "30" "31" "32" "33" "34" "35" "36" "37" "38" "39"
                        //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15   16   17   18   19   20   21   22

                        //Frequency selector 2      R800_FREQ2
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 3      R800_FREQ3
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 4      R800_FREQ4
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
                        desiredPositionDial1X = Array.IndexOf(_r800l1Freq1DialValues, int.Parse(frequencyAsString.Substring(0, 2)));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(4, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(5, 1));

                        do
                        {
                            if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "R-800L1 dial1Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800l1Dial1WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 1");
                            }
                            if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "R-800L1 dial2Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800l1Dial2WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 2");
                            }
                            if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "R-800L1 dial3Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800l1Dial3WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 3");
                            }
                            if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "R-800L1 dial4Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800l1Dial4WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 4");
                            }
                            if (Interlocked.Read(ref _r800l1Dial1WaitingForFeedback) == 0)
                            {
                                lock (_lockR800L1DialsObject1)
                                {

                                    Common.DebugP("_r800l1CockpitFreq1DialPos is " + _r800l1CockpitFreq1DialPos + " and should be " + desiredPositionDial1X);
                                    if (_r800l1CockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        str = R800L1Freq1DialCommand + GetCommandDirectionForR800L1Dial1(desiredPositionDial1X, _r800l1CockpitFreq1DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial1SendCount++;
                                        Interlocked.Exchange(ref _r800l1Dial1WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial1Timeout);
                                }
                            }
                            else
                            {
                                dial1OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _r800l1Dial2WaitingForFeedback) == 0)
                            {
                                lock (_lockR800L1DialsObject2)
                                {
                                    Common.DebugP("_r800l1CockpitFreq2DialPos is " + _r800l1CockpitFreq2DialPos + " and should be " + desiredPositionDial2X);
                                    if (_r800l1CockpitFreq2DialPos != desiredPositionDial2X)
                                    {
                                        dial2OkTime = DateTime.Now.Ticks;
                                        str = R800L1Freq2DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial2X, _r800l1CockpitFreq2DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial2SendCount++;
                                        Interlocked.Exchange(ref _r800l1Dial2WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial2Timeout);
                                }
                            }
                            else
                            {
                                dial2OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _r800l1Dial3WaitingForFeedback) == 0)
                            {
                                lock (_lockR800L1DialsObject3)
                                {
                                    Common.DebugP("_r800l1CockpitFreq3DialPos is " + _r800l1CockpitFreq3DialPos + " and should be " + desiredPositionDial3X);
                                    if (_r800l1CockpitFreq3DialPos != desiredPositionDial3X)
                                    {
                                        dial3OkTime = DateTime.Now.Ticks;
                                        str = R800L1Freq3DialCommand + GetCommandDirectionFor0To9Dials(desiredPositionDial3X, _r800l1CockpitFreq3DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial3SendCount++;
                                        Interlocked.Exchange(ref _r800l1Dial3WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial3Timeout);
                                }
                            }
                            else
                            {
                                dial3OkTime = DateTime.Now.Ticks;
                            }
                            var desiredPositionDial4 = 0;
                            if (Interlocked.Read(ref _r800l1Dial4WaitingForFeedback) == 0)
                            {
                                if (desiredPositionDial4X == 0)
                                {
                                    desiredPositionDial4 = 0;
                                }
                                else if (desiredPositionDial4X == 2)
                                {
                                    desiredPositionDial4 = 0;
                                }
                                else if (desiredPositionDial4X == 5)
                                {
                                    desiredPositionDial4 = 2;
                                }
                                else if (desiredPositionDial4X == 7)
                                {
                                    desiredPositionDial4 = 2;
                                }
                                //      "00" "25" "50" "75", only "00" and "50" used.
                                //Pos     0    1    2    3

                                lock (_lockR800L1DialsObject4)
                                {
                                    Common.DebugP("_r800l1CockpitFreq4DialPos is " + _r800l1CockpitFreq4DialPos + " and should be " + desiredPositionDial4);
                                    if (_r800l1CockpitFreq4DialPos < desiredPositionDial4)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R800L1Freq4DialCommand + "INC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r800l1Dial4WaitingForFeedback, 1);
                                    }
                                    else if (_r800l1CockpitFreq4DialPos > desiredPositionDial4)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R800L1Freq4DialCommand + "DEC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r800l1Dial4WaitingForFeedback, 1);
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
                        SwapCockpitStandbyFrequencyR800L1();
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
                    Interlocked.Exchange(ref _r800l1ThreadNowSynching, 0);
                }

            }
            catch (Exception ex)
            {
                Common.LogError(77004, ex);
            }
            //Refresh panel once this debacle is finished
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            Common.DebugP("Leaving Ka-50 Radio R800L1SynchThreadMethod()");
        }

        private void SwapCockpitStandbyFrequencyR800L1()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SwapCockpitStandbyFrequencyR800L1()");
                _r800l1BigFrequencyStandby = _r800l1SavedCockpitBigFrequency;
                _r800l1SmallFrequencyStandby = _r800l1SavedCockpitSmallFrequency;
            }
            catch (Exception ex)
            {
                Common.LogError(77005, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SwapCockpitStandbyFrequencyR800L1()");
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (_lockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobKa50)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsKa50.UPPER_VHF1_R828:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentKa50RadioMode.VHF1_R828);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_VHF2_R800L1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentKa50RadioMode.VHF2_R800L1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_ADF_ARK22:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentKa50RadioMode.ADF_ARK22);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_VHF1_R828:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentKa50RadioMode.VHF1_R828);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_VHF2_R800L1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentKa50RadioMode.VHF2_R800L1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_ADF_ARK22:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentKa50RadioMode.ADF_ARK22);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_ABRIS:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentKa50RadioMode.ABRIS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_NOUSE2:
                            case RadioPanelPZ69KnobsKa50.UPPER_NOUSE3:
                            case RadioPanelPZ69KnobsKa50.UPPER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentKa50RadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_ABRIS:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentKa50RadioMode.ABRIS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_NOUSE2:
                            case RadioPanelPZ69KnobsKa50.LOWER_NOUSE3:
                            case RadioPanelPZ69KnobsKa50.LOWER_NOUSE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentKa50RadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentKa50RadioMode.VHF1_R828)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(Vhf1TunerButtonPress);
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(Vhf1TunerButtonRelease);
                                        }
                                    }
                                    else if (_currentUpperRadioMode == CurrentKa50RadioMode.ADF_ARK22 && radioPanelKnob.IsOn)
                                    {
                                        lock (_lockADFModeDialObject)
                                        {
                                            if (_adfModeSwitchDirectionUp && _adfModeCockpitPos == 2)
                                            {
                                                _adfModeSwitchDirectionUp = false;
                                                DCSBIOS.Send(ADFModeDec);
                                            }
                                            else if (!_adfModeSwitchDirectionUp && _adfModeCockpitPos == 0)
                                            {
                                                _adfModeSwitchDirectionUp = true;
                                                DCSBIOS.Send(ADFModeInc);
                                            }
                                            else if (_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADFModeInc);
                                            }
                                            else if (!_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADFModeDec);
                                            }
                                        }
                                        /*if (_adfModeSwitchLastSent.Equals(ADFModeSwitchAntenna))
                                        {
                                            DCSBIOS.Send(ADFModeSwitchCompass);
                                            _adfModeSwitchLastSent = ADFModeSwitchCompass;
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(ADFModeSwitchAntenna);
                                            _adfModeSwitchLastSent = ADFModeSwitchAntenna;
                                        }*/
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentKa50RadioMode.VHF1_R828)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(Vhf1TunerButtonPress);
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(Vhf1TunerButtonRelease);
                                        }
                                    }
                                    else if (_currentLowerRadioMode == CurrentKa50RadioMode.ADF_ARK22 && radioPanelKnob.IsOn)
                                    {
                                        lock (_lockADFModeDialObject)
                                        {
                                            if (_adfModeSwitchDirectionUp && _adfModeCockpitPos == 2)
                                            {
                                                _adfModeSwitchDirectionUp = false;
                                                DCSBIOS.Send(ADFModeDec);
                                            }
                                            else if (!_adfModeSwitchDirectionUp && _adfModeCockpitPos == 0)
                                            {
                                                _adfModeSwitchDirectionUp = true;
                                                DCSBIOS.Send(ADFModeInc);
                                            }
                                            else if (_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADFModeInc);
                                            }
                                            else if (!_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADFModeDec);
                                            }
                                        }
                                        /*
                                        if (_adfModeSwitchLastSent.Equals(ADFModeSwitchAntenna))
                                        {
                                            DCSBIOS.Send(ADFModeSwitchCompass);
                                            _adfModeSwitchLastSent = ADFModeSwitchCompass;
                                        }
                                        else
                                        {
                                            DCSBIOS.Send(ADFModeSwitchAntenna);
                                            _adfModeSwitchLastSent = ADFModeSwitchAntenna;
                                        }*/
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH);
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
                Common.LogError(77006, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobKa50 = (RadioPanelKnobKa50)o;
                    if (radioPanelKnobKa50.IsOn)
                    {
                        switch (radioPanelKnobKa50.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    DCSBIOS.Send(Vhf1PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                //100-149  220-399
                                                if (_r800l1BigFrequencyStandby.Equals(399))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                if (_r800l1BigFrequencyStandby.Equals(149))
                                                {
                                                    _r800l1BigFrequencyStandby = 220;
                                                }
                                                else
                                                {
                                                    _r800l1BigFrequencyStandby++;
                                                }

                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisLeftDialIncreaseChangeMonitor.Click();
                                                if (_arbisLeftDialIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandIncMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADFPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    DCSBIOS.Send(Vhf1PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                //100-149  220-399
                                                if (_r800l1BigFrequencyStandby.Equals(100))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                if (_r800l1BigFrequencyStandby.Equals(220))
                                                {
                                                    _r800l1BigFrequencyStandby = 149;
                                                }
                                                else
                                                {
                                                    _r800l1BigFrequencyStandby--;
                                                }

                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisLeftDialDecreaseChangeMonitor.Click();
                                                if (_arbisLeftDialDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandDecMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADFPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800l1SmallFrequencyStandby >= 95)
                                                {
                                                    //At max value
                                                    _r800l1SmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r800l1SmallFrequencyStandby = _r800l1SmallFrequencyStandby + 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisRightDialIncreaseChangeMonitor.Click();
                                                if (_arbisRightDialIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandIncMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800l1SmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _r800l1SmallFrequencyStandby = 95;
                                                    break;
                                                }
                                                _r800l1SmallFrequencyStandby = _r800l1SmallFrequencyStandby - 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisRightDialDecreaseChangeMonitor.Click();
                                                if (_arbisRightDialDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandDecMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    DCSBIOS.Send(Vhf1PresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                var changeFaster = false;
                                                _r800l1BigFreqIncreaseChangeMonitor.Click();
                                                if (_r800l1BigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (changeFaster)
                                                {
                                                    _r800l1BigFrequencyStandby = _r800l1BigFrequencyStandby + ChangeValue;
                                                }
                                                else
                                                {
                                                    _r800l1BigFrequencyStandby++;
                                                }
                                                if (_r800l1BigFrequencyStandby > 399)
                                                {
                                                    //@ max value
                                                    _r800l1BigFrequencyStandby = 399;
                                                    break;
                                                }
                                                if (_r800l1BigFrequencyStandby > 149 && _r800l1BigFrequencyStandby < 220)
                                                {
                                                    _r800l1BigFrequencyStandby = _r800l1BigFrequencyStandby - 149 + 220;
                                                }
                                                Common.DebugP("_r800l1BigFrequencyStandby is now " + _r800l1BigFrequencyStandby);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisLeftDialIncreaseChangeMonitor.Click();
                                                if (_arbisLeftDialIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandIncMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADFPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    DCSBIOS.Send(Vhf1PresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                var changeFaster = false;
                                                _r800l1BigFreqDecreaseChangeMonitor.Click();
                                                if (_r800l1BigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (changeFaster)
                                                {
                                                    _r800l1BigFrequencyStandby = _r800l1BigFrequencyStandby - ChangeValue;
                                                }
                                                else
                                                {
                                                    _r800l1BigFrequencyStandby--;
                                                }
                                                if (_r800l1BigFrequencyStandby <= 100)
                                                {
                                                    //@ max value
                                                    _r800l1BigFrequencyStandby = 100;
                                                    break;
                                                }
                                                if (_r800l1BigFrequencyStandby > 149 && _r800l1BigFrequencyStandby < 220)
                                                {
                                                    _r800l1BigFrequencyStandby = 149 - (220 - _r800l1BigFrequencyStandby);
                                                }
                                                Common.DebugP("_r800l1BigFrequencyStandby is now " + _r800l1BigFrequencyStandby);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisLeftDialDecreaseChangeMonitor.Click();
                                                if (_arbisLeftDialDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandDecMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISLeftDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADFPresetCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800l1SmallFrequencyStandby >= 95)
                                                {
                                                    //At max value
                                                    _r800l1SmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r800l1SmallFrequencyStandby = _r800l1SmallFrequencyStandby + 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisRightDialIncreaseChangeMonitor.Click();
                                                if (_arbisRightDialIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandIncMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandInc);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentKa50RadioMode.VHF1_R828:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800l1SmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _r800l1SmallFrequencyStandby = 95;
                                                    break;
                                                }
                                                _r800l1SmallFrequencyStandby = _r800l1SmallFrequencyStandby - 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _arbisRightDialDecreaseChangeMonitor.Click();
                                                if (_arbisRightDialDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandDecMore);
                                                }
                                                else
                                                {
                                                    DCSBIOS.Send(ARBISRightDialCommandDec);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADFVolumeKnobCommandDec);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.NOUSE:
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
                Common.LogError(77007, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio AdjustFrequency()");
        }


        private void CheckFrequenciesForValidity()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio CheckFrequenciesForValidity()");
                //Crude fix if any freqs are outside the valid boundaries

                //R-800L VHF 2
                //100.00 - 149.00
                //220.00 - 399.00
                if (_r800l1BigFrequencyStandby < 100)
                {
                    _r800l1BigFrequencyStandby = 100;
                }
                if (_r800l1BigFrequencyStandby > 399)
                {
                    _r800l1BigFrequencyStandby = 399;
                }
                if (_r800l1BigFrequencyStandby == 399 && _r800l1SmallFrequencyStandby > 0)
                {
                    _r800l1SmallFrequencyStandby = 0;
                }
                if (_r800l1BigFrequencyStandby == 149 && _r800l1SmallFrequencyStandby > 0)
                {
                    _r800l1SmallFrequencyStandby = 0;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(77008, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio CheckFrequenciesForValidity()");
        }

        private bool SkipVhf1PresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SkipVhf1PresetDialChange()");
                if (_currentUpperRadioMode == CurrentKa50RadioMode.VHF1_R828 || _currentLowerRadioMode == CurrentKa50RadioMode.VHF1_R828)
                {
                    if (_vhf1PresetDialSkipper > 2)
                    {
                        _vhf1PresetDialSkipper = 0;
                        Common.DebugP("Leaving Ka-50 Radio SkipVhf1PresetDialChange()");
                        return false;
                    }
                    _vhf1PresetDialSkipper++;
                    Common.DebugP("Leaving Ka-50 Radio SkipVhf1PresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Ka-50 Radio SkipVhf1PresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(77009, ex);
            }
            return false;
        }

        private bool SkipADFPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SkipADFPresetDialChange()");
                if (_currentUpperRadioMode == CurrentKa50RadioMode.ADF_ARK22 || _currentLowerRadioMode == CurrentKa50RadioMode.ADF_ARK22)
                {
                    if (_adfPresetDialSkipper > 2)
                    {
                        _adfPresetDialSkipper = 0;
                        Common.DebugP("Leaving Ka-50 Radio SkipADFPresetDialChange()");
                        return false;
                    }
                    _adfPresetDialSkipper++;
                    Common.DebugP("Leaving Ka-50 Radio SkipADFPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving Ka-50 Radio SkipADFPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(77010, ex);
            }
            return false;
        }

        private void ShowFrequenciesOnPanel()
        {
            try
            {
                lock (_lockShowFrequenciesOnPanelObject)
                {
                    if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                    {
                        //Common.DebugP("Leaving Ka-50 Radio ShowFrequenciesOnPanel() NO KNOBS/FREQS changed");
                        return;
                    }
                    //Common.DebugP("ShowFrequenciesOnPanel " + id);
                    if (!FirstReportHasBeenRead)
                    {
                        //Common.DebugP("Leaving Ka-50 Radio ShowFrequenciesOnPanel()");
                        return;
                    }

                    Common.DebugP("Entering Ka-50 Radio ShowFrequenciesOnPanel()");
                    CheckFrequenciesForValidity();
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentKa50RadioMode.VHF1_R828:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                var channelAsString = "";
                                lock (_lockVhf1DialObject1)
                                {
                                    channelAsString = (_vhf1CockpitPresetDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.VHF2_R800L1:
                            {
                                var frequencyAsString = "";
                                lock (_lockR800L1DialsObject1)
                                {
                                    frequencyAsString = _r800l1Freq1DialValues[_r800l1CockpitFreq1DialPos].ToString();
                                }
                                lock (_lockR800L1DialsObject2)
                                {

                                    frequencyAsString = frequencyAsString + _r800l1CockpitFreq2DialPos;
                                }
                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR800L1DialsObject3)
                                {

                                    frequencyAsString = frequencyAsString + _r800l1CockpitFreq3DialPos;
                                }
                                lock (_lockR800L1DialsObject4)
                                {

                                    frequencyAsString = frequencyAsString + GetR800L1DialFrequencyForPosition(_r800l1CockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r800l1BigFrequencyStandby + "." + _r800l1SmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                                break;
                            }
                        case CurrentKa50RadioMode.ABRIS:
                            {
                                var channelAsString = "88888";
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.ADF_ARK22:
                            {
                                //Preset Channel Selector
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                var channelAsString = "";
                                lock (_lockADFDialObject1)
                                {
                                    switch (_adfCockpitPresetDialPos)
                                    {
                                        case 0:
                                            {
                                                channelAsString = "9".PadLeft(2, ' ');
                                                break;
                                            }
                                        case 1:
                                            {
                                                channelAsString = "10".PadLeft(2, ' ');
                                                break;
                                            }
                                        default:
                                            {
                                                channelAsString = (_adfCockpitPresetDialPos - 1).ToString().PadLeft(2, ' ');
                                                break;
                                            }
                                    }
                                }
                                uint adfMode = 0;
                                lock (_lockADFModeDialObject)
                                {
                                    adfMode = _adfModeCockpitPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, adfMode, PZ69LCDPosition.UPPER_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentKa50RadioMode.VHF1_R828:
                            {
                                //Preset Channel Selector
                                //      " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                var channelAsString = "";
                                lock (_lockVhf1DialObject1)
                                {
                                    channelAsString = (_vhf1CockpitPresetDialPos + 1).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.VHF2_R800L1:
                            {
                                var frequencyAsString = "";
                                lock (_lockR800L1DialsObject1)
                                {
                                    frequencyAsString = _r800l1Freq1DialValues[_r800l1CockpitFreq1DialPos].ToString();
                                }
                                lock (_lockR800L1DialsObject2)
                                {

                                    frequencyAsString = frequencyAsString + _r800l1CockpitFreq2DialPos;
                                }
                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR800L1DialsObject3)
                                {

                                    frequencyAsString = frequencyAsString + _r800l1CockpitFreq3DialPos;
                                }
                                lock (_lockR800L1DialsObject4)
                                {

                                    frequencyAsString = frequencyAsString + GetR800L1DialFrequencyForPosition(_r800l1CockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r800l1BigFrequencyStandby + "." + _r800l1SmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_RIGHT);
                                break;
                            }
                        case CurrentKa50RadioMode.ABRIS:
                            {
                                var channelAsString = "88888";
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.ADF_ARK22:
                            {
                                //Preset Channel Selector
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12
                                var channelAsString = "";
                                lock (_lockADFDialObject1)
                                {
                                    switch (_adfCockpitPresetDialPos)
                                    {
                                        case 0:
                                            {
                                                channelAsString = "9".PadLeft(2, ' ');
                                                break;
                                            }
                                        case 1:
                                            {
                                                channelAsString = "10".PadLeft(2, ' ');
                                                break;
                                            }
                                        default:
                                            {
                                                channelAsString = (_adfCockpitPresetDialPos - 1).ToString().PadLeft(2, ' ');
                                                break;
                                            }
                                    }
                                }
                                uint adfMode = 0;
                                lock (_lockADFModeDialObject)
                                {
                                    adfMode = _adfModeCockpitPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, adfMode, PZ69LCDPosition.LOWER_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.NOUSE:
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
                Common.LogError(77011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving Ka-50 Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering Ka-50 Radio OnReport()");
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
                                    var knob = (RadioPanelKnobKa50)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobKa50)radioPanelKnob));
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
                Common.LogError(77012, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering Ka-50 Radio GetHashSetOfChangedKnobs()");
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
                Common.LogError(77013, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("Ka-50");

                //VHF1
                _vhf1DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R828_CHANNEL");

                //VHF2
                _r800l1DcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ1");
                _r800l1DcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ2");
                _r800l1DcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ3");
                _r800l1DcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ4");

                //ADF
                _adfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_CHANNEL");
                _adfModeDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_NDB_MODE");

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69Ka50.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving Ka-50 Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobKa50.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobKa50 radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentKa50RadioMode currentKa50RadioMode)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentKa50RadioMode);
                _currentUpperRadioMode = currentKa50RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(77014, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentKa50RadioMode currentKa50RadioMode)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentKa50RadioMode);
                _currentLowerRadioMode = currentKa50RadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(77015, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SetLowerRadioMode()");
        }

        private bool R800L1NowSyncing()
        {
            return Interlocked.Read(ref _r800l1ThreadNowSynching) > 0;
        }

        private void SaveCockpitFrequencyR800L1()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SaveCockpitFrequencyR800L1()");
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

                lock (_lockR800L1DialsObject1)
                {
                    lock (_lockR800L1DialsObject2)
                    {
                        lock (_lockR800L1DialsObject3)
                        {
                            lock (_lockR800L1DialsObject4)
                            {
                                uint dial4 = 0;
                                switch (_r800l1CockpitFreq4DialPos)
                                {
                                    case 0:
                                    case 1:
                                        {
                                            //00 & 25
                                            dial4 = 0;
                                            break;
                                        }
                                    case 2:
                                    case 3:
                                        {
                                            //50 & 75
                                            dial4 = 5;
                                            break;
                                        }
                                }
                                _r800l1SavedCockpitBigFrequency = uint.Parse(_r800l1Freq1DialValues[_r800l1CockpitFreq1DialPos].ToString() + _r800l1CockpitFreq2DialPos.ToString());
                                _r800l1SavedCockpitSmallFrequency = uint.Parse(_r800l1CockpitFreq3DialPos.ToString() + dial4.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(77016, ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SaveCockpitFrequencyR800L1()");
        }


        private string GetCommandDirectionForR800L1Dial1(int desiredDialPosition, uint actualDialPosition)
        {
            var inc = "INC\n";
            var dec = "DEC\n";
            try
            {
                Common.DebugP("Entering Ka-50 Radio GetCommandDirectionForR800L1Dial1()");

                var tmpPos = actualDialPosition;
                var countUp = 0;
                var countDown = 0;
                while (true)
                {
                    //0 1 2
                    //len 3
                    Common.DebugP("GetCommandDirectionForR800L1Dial1 #1 : tmpPos = " + tmpPos + " desiredDialPosition = " + desiredDialPosition);
                    if (tmpPos == desiredDialPosition)
                    {
                        break;
                    }
                    if (tmpPos <= _r800l1Freq1DialValues.Length - 1)
                    {
                        tmpPos++;
                        countUp++;
                    }
                    else
                    {
                        tmpPos = 0;
                        countUp++;
                    }
                }
                tmpPos = actualDialPosition;
                while (true)
                {
                    //0 1 2
                    //len 3
                    Common.DebugP("GetCommandDirectionForR800L1Dial1 #2 : tmpPos = " + tmpPos + " desiredDialPosition = " + desiredDialPosition);
                    if (tmpPos == desiredDialPosition)
                    {
                        break;
                    }
                    if (tmpPos == 0)
                    {
                        tmpPos = unchecked((uint)_r800l1Freq1DialValues.Length - 1);
                        countDown++;
                    }
                    else
                    {
                        tmpPos--;
                        countDown++;
                    }
                }

                Common.DebugP("GetCommandDirectionForR800L1Dial1 : countDown = " + countDown + " countUp = " + countUp);
                if (countDown < countUp)
                {
                    Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionForR800L1Dial1()");
                    return dec;
                }
                Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionForR800L1Dial1()");
            }
            catch (Exception ex)
            {
                Common.LogError(77017, ex);
            }
            return inc;
        }

        private string GetCommandDirectionFor0To9Dials(int desiredDialPosition, uint actualDialPosition)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //-4 DEC
                                        return dec;
                                    }
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 1:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 2:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 3:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 4:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 5:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 6:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 7:
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 7:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 8:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        //Do nothing
                                        return null;
                                    }
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return dec;
                                    }
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                                        return inc;
                                    }
                                case 9:
                                    {
                                        Common.DebugP("Leaving Ka-50 Radio GetCommandDirectionFor0To9Dials()");
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
                Common.LogError(77018, ex);
            }
            throw new Exception("Should reach this code. private String GetCommandDirectionFor0To9Dials(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        private string GetR800L1DialFrequencyForPosition(uint position)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio GetR800L1DialFrequencyForPosition()");
                //        "00"  "25" "50" "75"
                //          0    1    2    3  
                switch (position)
                {
                    case 0:
                        {
                            Common.DebugP("Leaving Ka-50 Radio GetR800L1DialFrequencyForPosition()");
                            return "0";
                        }
                    case 1:
                        {
                            Common.DebugP("Leaving Ka-50 Radio GetR800L1DialFrequencyForPosition()");
                            return "5";
                        }
                    case 2:
                        {
                            Common.DebugP("Leaving Ka-50 Radio GetR800L1DialFrequencyForPosition()");
                            return "5";
                        }
                    case 3:
                        {
                            Common.DebugP("Leaving Ka-50 Radio GetR800L1DialFrequencyForPosition()");
                            return "0";
                        }
                }
                Common.DebugP("ERROR!!! Leaving Ka-50 Radio GetR800L1DialFrequencyForPosition()");
            }
            catch (Exception ex)
            {
                Common.LogError(77019, ex);
            }
            return "";
        }

        public override String SettingsVersion()
        {
            return "0X";
        }
    }
}
