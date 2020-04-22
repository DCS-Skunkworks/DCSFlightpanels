using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;


namespace NonVisuals.Radios
{
    public class RadioPanelPZ69Ka50 : RadioPanelPZ69Base, IRadioPanel
    {
        private CurrentKa50RadioMode _currentUpperRadioMode = CurrentKa50RadioMode.VHF1_R828;
        private CurrentKa50RadioMode _currentLowerRadioMode = CurrentKa50RadioMode.VHF1_R828;

        /*COM1 Ka-50 VHF 1 R-828*/
        //Large dial 1-10 [step of 1]
        //Small dial volume control
        private readonly object _lockVhf1DialObject1 = new object();
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetDial;
        private volatile uint _vhf1CockpitPresetDialPos = 1;
        private const string VHF1_PRESET_COMMAND_INC = "R828_CHANNEL INC\n";
        private const string VHF1_PRESET_COMMAND_DEC = "R828_CHANNEL DEC\n";
        private int _vhf1PresetDialSkipper;
        //private DCSBIOSOutput _vhf1DcsbiosOutputVolumeDial;
        private const string VHF1_VOLUME_KNOB_COMMAND_INC = "R828_VOLUME +2500\n";
        private const string VHF1_VOLUME_KNOB_COMMAND_DEC = "R828_VOLUME -2500\n";
        private const string VHF1_TUNER_BUTTON_PRESS = "R828_TUNER INC\n";
        private const string VHF1_TUNER_BUTTON_RELEASE = "R828_TUNER DEC\n";

        /*COM2 Ka-50 VHF 2 R-800L1*/
        //Large dial 100-149  -> 220 - 399 [step of 1]
        //Small dial 0 - 95
        /*private int[] _r800l1UsedBigFrequencyValues =
        {   //These are currently used by DCS, to get more efficient usage by disregarding freqs not used in game (51 positions)
            108, 109, 110, 111, 113, 114, 115, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130,131, 132, 133, 134, 135, 136, 138, 139, 140, 141, 250, 251, 252, 253, 254, 255, 256,257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 344, 365, 385
            //
        };*/
        private readonly ClickSpeedDetector _r800L1BigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private readonly ClickSpeedDetector _r800L1BigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);
        const int CHANGE_VALUE = 10;
        //private long _changesWithinLastticksSinceLastChangeLargeDial;
        private readonly int[] _r800L1Freq1DialValues = { 10, 11, 12, 13, 14, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 };
        private volatile uint _r800L1BigFrequencyStandby = 108;
        private volatile uint _r800L1SmallFrequencyStandby;
        private volatile uint _r800L1SavedCockpitBigFrequency;
        private volatile uint _r800L1SavedCockpitSmallFrequency;
        private readonly object _lockR800L1DialsObject1 = new object();
        private readonly object _lockR800L1DialsObject2 = new object();
        private readonly object _lockR800L1DialsObject3 = new object();
        private readonly object _lockR800L1DialsObject4 = new object();
        private DCSBIOSOutput _r800L1DcsbiosOutputFreqDial1;
        private DCSBIOSOutput _r800L1DcsbiosOutputFreqDial2;
        private DCSBIOSOutput _r800L1DcsbiosOutputFreqDial3;
        private DCSBIOSOutput _r800L1DcsbiosOutputFreqDial4;
        private volatile uint _r800L1CockpitFreq1DialPos = 1;
        private volatile uint _r800L1CockpitFreq2DialPos = 1;
        private volatile uint _r800L1CockpitFreq3DialPos = 1;
        private volatile uint _r800L1CockpitFreq4DialPos = 1;
        private const string R800_L1_FREQ_1DIAL_COMMAND = "R800_FREQ1 ";
        private const string R800_L1_FREQ_2DIAL_COMMAND = "R800_FREQ2 ";
        private const string R800_L1_FREQ_3DIAL_COMMAND = "R800_FREQ3 ";
        private const string R800_L1_FREQ_4DIAL_COMMAND = "R800_FREQ4 ";
        private Thread _r800L1SyncThread;
        private long _r800L1ThreadNowSynching;
        private long _r800L1Dial1WaitingForFeedback;
        private long _r800L1Dial2WaitingForFeedback;
        private long _r800L1Dial3WaitingForFeedback;
        private long _r800L1Dial4WaitingForFeedback;


        /*ADF Ka-50 ARK-22 ADF*/
        //Large dial 0-9 [step of 1]
        //Small dial volume control
        //ACT/STBY Switch between ADF Modes Inner Auto Outer
        private readonly object _lockADFDialObject1 = new object();
        private DCSBIOSOutput _adfDcsbiosOutputPresetDial;
        private volatile uint _adfCockpitPresetDialPos = 1;
        private const string ADF_PRESET_COMMAND_INC = "ADF_CHANNEL INC\n";
        private const string ADF_PRESET_COMMAND_DEC = "ADF_CHANNEL DEC\n";
        private int _adfPresetDialSkipper;
        private const string ADF_VOLUME_KNOB_COMMAND_INC = "ADF_VOLUME +2500\n";
        private const string ADF_VOLUME_KNOB_COMMAND_DEC = "ADF_VOLUME -2500\n";
        /*private const string ADFModeSwitchAntenna = "ADF_CMPS_ANT INC\n";
        private const string ADFModeSwitchCompass = "ADF_CMPS_ANT DEC\n";
        private string _adfModeSwitchLastSent = "";*/
        private readonly object _lockADFModeDialObject = new object();
        private DCSBIOSOutput _adfModeDcsbiosOutput;
        private volatile uint _adfModeCockpitPos = 1;
        private const string ADF_MODE_INC = "ADF_NDB_MODE INC\n";
        private const string ADF_MODE_DEC = "ADF_NDB_MODE DEC\n";
        private bool _adfModeSwitchDirectionUp = false;



        /*NAV1 Ka-50 ABRIS NAV1 (Not radio but programmed as there are so few radio systems on the KA-50*/
        //Large ABRIS Left Dial
        //Small ABRIS Right Dial
        //ACT/STBY Push Right ABRIS Dial IN/OUT
        private readonly ClickSpeedDetector _abrisLeftDialIncreaseChangeMonitor = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _abrisLeftDialDecreaseChangeMonitor = new ClickSpeedDetector(10);
        private const string ABRIS_LEFT_DIAL_COMMAND_INC_MORE = "ABRIS_BRIGHTNESS +2500\n";
        private const string ABRIS_LEFT_DIAL_COMMAND_DEC_MORE = "ABRIS_BRIGHTNESS -2500\n";
        private const string ABRIS_LEFT_DIAL_COMMAND_INC = "ABRIS_BRIGHTNESS +1000\n";
        private const string ABRIS_LEFT_DIAL_COMMAND_DEC = "ABRIS_BRIGHTNESS -1000\n";
        private readonly ClickSpeedDetector _abrisRightDialIncreaseChangeMonitor = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _abrisRightDialDecreaseChangeMonitor = new ClickSpeedDetector(10);
        private const string ABRIS_RIGHT_DIAL_COMMAND_INC_MORE = "ABRIS_CURSOR_ROT +2500\n";
        private const string ABRIS_RIGHT_DIAL_COMMAND_DEC_MORE = "ABRIS_CURSOR_ROT -2500\n";
        private const string ABRIS_RIGHT_DIAL_COMMAND_INC = "ABRIS_CURSOR_ROT +5000\n";
        private const string ABRIS_RIGHT_DIAL_COMMAND_DEC = "ABRIS_CURSOR_ROT -5000\n";
        private const string ABRIS_RIGHT_DIAL_PUSH_TOGGLE_ON_COMMAND = "ABRIS_CURSOR_BTN 1\n";
        private const string ABRIS_RIGHT_DIAL_PUSH_TOGGLE_OFF_COMMAND = "ABRIS_CURSOR_BTN 0\n";

        /*NAV2 Ka-50 Datalink Operation*/
        //Large dial Datalink Master Mode 0-3
        //Small dial Datalink Self ID
        //ACT/STBY Datalink ON/OFF
        private readonly object _lockDatalinkMasterModeObject = new object();
        private DCSBIOSOutput _datalinkMasterModeDcsbiosOutput;
        private volatile uint _datalinkMasterModeCockpitPos = 1;
        private const string DATALINK_MASTER_MODE_COMMAND_INC = "DLNK_MASTER_MODE INC\n";
        private const string DATALINK_MASTER_MODE_COMMAND_DEC = "DLNK_MASTER_MODE DEC\n";
        private int _datalinkMasterModeDialSkipper;
        private readonly object _lockDatalinkSelfIdObject = new object();
        private DCSBIOSOutput _datalinkSelfIdDcsbiosOutput;
        private volatile uint _datalinkSelfIdCockpitPos = 1;
        private const string DATALINK_SELF_ID_COMMAND_INC = "DLNK_SELF_ID INC\n";
        private const string DATALINK_SELF_ID_COMMAND_DEC = "DLNK_SELF_ID DEC\n";
        private int _datalinkSelfIdDialSkipper;
        private readonly object _lockDatalinkPowerOnOffObject = new object();
        private DCSBIOSOutput _datalinkPowerOnOffDcsbiosOutput;
        private volatile uint _datalinkPowerOnOffCockpitPos = 1;
        private const string DATALINK_POWER_ON_OFF_COMMAND_TOGGLE = "PVI_POWER TOGGLE\n";





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
            _r800L1SyncThread?.Abort();
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

                //VHF1 Preset Channel Dial
                if (e.Address == _vhf1DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockVhf1DialObject1)
                    {
                        var tmp = _vhf1CockpitPresetDialPos;
                        _vhf1CockpitPresetDialPos = _vhf1DcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _vhf1CockpitPresetDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //VHF2 Dial 1 R-800L1
                if (e.Address == _r800L1DcsbiosOutputFreqDial1.Address)
                {
                    lock (_lockR800L1DialsObject1)
                    {
                        var tmp = _r800L1CockpitFreq1DialPos;
                        _r800L1CockpitFreq1DialPos = _r800L1DcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                        if (tmp != _r800L1CockpitFreq1DialPos)
                        {
                            //Debug.WriteLine("_r800l1CockpitFreq1DialPos was " + tmp + ", is now " + _r800l1CockpitFreq1DialPos);
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r800L1Dial1WaitingForFeedback, 0);
                        }
                    }
                }

                //VHF2 Dial 2 R-800L1
                if (e.Address == _r800L1DcsbiosOutputFreqDial2.Address)
                {
                    lock (_lockR800L1DialsObject2)
                    {
                        var tmp = _r800L1CockpitFreq2DialPos;
                        _r800L1CockpitFreq2DialPos = _r800L1DcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                        if (tmp != _r800L1CockpitFreq2DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r800L1Dial2WaitingForFeedback, 0);
                        }
                    }
                }

                //VHF2 Dial 3 R-800L1
                if (e.Address == _r800L1DcsbiosOutputFreqDial3.Address)
                {
                    lock (_lockR800L1DialsObject3)
                    {
                        var tmp = _r800L1CockpitFreq3DialPos;
                        _r800L1CockpitFreq3DialPos = _r800L1DcsbiosOutputFreqDial3.GetUIntValue(e.Data);
                        if (tmp != _r800L1CockpitFreq3DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r800L1Dial3WaitingForFeedback, 0);
                        }
                    }
                }

                //VHF2 Dial 4 R-800L1
                if (e.Address == _r800L1DcsbiosOutputFreqDial4.Address)
                {
                    lock (_lockR800L1DialsObject4)
                    {
                        var tmp = _r800L1CockpitFreq4DialPos;
                        _r800L1CockpitFreq4DialPos = _r800L1DcsbiosOutputFreqDial4.GetUIntValue(e.Data);
                        if (tmp != _r800L1CockpitFreq4DialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _r800L1Dial4WaitingForFeedback, 0);
                        }
                    }
                }

                //NAV2 Datalink Master Mode
                if (e.Address == _datalinkMasterModeDcsbiosOutput.Address)
                {
                    lock (_lockDatalinkMasterModeObject)
                    {
                        var tmp = _datalinkMasterModeCockpitPos;
                        _datalinkMasterModeCockpitPos = _datalinkMasterModeDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _datalinkMasterModeCockpitPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //NAV2 Datalink Self ID
                if (e.Address == _datalinkSelfIdDcsbiosOutput.Address)
                {
                    lock (_lockDatalinkSelfIdObject)
                    {
                        var tmp = _datalinkSelfIdCockpitPos;
                        _datalinkSelfIdCockpitPos = _datalinkSelfIdDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _datalinkSelfIdCockpitPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //NAV2 Datalink Power ON/OFF
                if (e.Address == _datalinkPowerOnOffDcsbiosOutput.Address)
                {
                    lock (_lockDatalinkPowerOnOffObject)
                    {
                        var tmp = _datalinkPowerOnOffCockpitPos;
                        _datalinkPowerOnOffCockpitPos = _datalinkPowerOnOffDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _datalinkPowerOnOffCockpitPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //ADF Preset Dial
                if (e.Address == _adfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockADFDialObject1)
                    {
                        var tmp = _adfCockpitPresetDialPos;
                        _adfCockpitPresetDialPos = _adfDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _adfCockpitPresetDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }



                //ADF Mode
                if (e.Address == _adfModeDcsbiosOutput.Address)
                {
                    lock (_lockADFModeDialObject)
                    {
                        var tmp = _adfModeCockpitPos;
                        _adfModeCockpitPos = _adfModeDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _adfModeCockpitPos)
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
                Common.LogError( ex);
            }
        }


        private void SendFrequencyToDCSBIOS(bool isOn, RadioPanelPZ69KnobsKa50 knob)
        {
            try
            {

                if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH))
                {
                    //Don't do anything on the very first button press as the panel sends ALL
                    //switches when it is manipulated the first time
                    //This would cause unintended sync.
                    return;
                }
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
                                        DCSBIOS.Send(isOn ? ABRIS_RIGHT_DIAL_PUSH_TOGGLE_ON_COMMAND : ABRIS_RIGHT_DIAL_PUSH_TOGGLE_OFF_COMMAND);
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
                                        DCSBIOS.Send(isOn ? ABRIS_RIGHT_DIAL_PUSH_TOGGLE_ON_COMMAND : ABRIS_RIGHT_DIAL_PUSH_TOGGLE_OFF_COMMAND);
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
                Common.LogError( ex);
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


                _r800L1SyncThread?.Abort();
                _r800L1SyncThread = new Thread(() => R800L1SynchThreadMethod());
                _r800L1SyncThread.Start();

            }
            catch (Exception ex)
            {
                Common.LogError( ex);
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
                        Interlocked.Exchange(ref _r800L1ThreadNowSynching, 1);
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

                        var frequencyAsString = _r800L1BigFrequencyStandby.ToString() + "." + _r800L1SmallFrequencyStandby.ToString().PadLeft(2, '0');
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
                        desiredPositionDial1X = Array.IndexOf(_r800L1Freq1DialValues, int.Parse(frequencyAsString.Substring(0, 2)));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(4, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(5, 1));

                        do
                        {
                            if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "R-800L1 dial1Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800L1Dial1WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 1");
                            }
                            if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "R-800L1 dial2Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800L1Dial2WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 2");
                            }
                            if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "R-800L1 dial3Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800L1Dial3WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 3");
                            }
                            if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "R-800L1 dial4Timeout"))
                            {
                                //Lets do an ugly reset
                                Interlocked.Exchange(ref _r800L1Dial4WaitingForFeedback, 0);
                                Common.DebugP("Resetting SYNC for R-800L1 4");
                            }

                            string str;
                            if (Interlocked.Read(ref _r800L1Dial1WaitingForFeedback) == 0)
                            {
                                lock (_lockR800L1DialsObject1)
                                {

                                    Common.DebugP("_r800l1CockpitFreq1DialPos is " + _r800L1CockpitFreq1DialPos + " and should be " + desiredPositionDial1X);
                                    if (_r800L1CockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        str = R800_L1_FREQ_1DIAL_COMMAND + GetCommandDirectionForR800L1Dial1(desiredPositionDial1X, _r800L1CockpitFreq1DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial1SendCount++;
                                        Interlocked.Exchange(ref _r800L1Dial1WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial1Timeout);
                                }
                            }
                            else
                            {
                                dial1OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _r800L1Dial2WaitingForFeedback) == 0)
                            {
                                lock (_lockR800L1DialsObject2)
                                {
                                    Common.DebugP("_r800l1CockpitFreq2DialPos is " + _r800L1CockpitFreq2DialPos + " and should be " + desiredPositionDial2X);
                                    if (_r800L1CockpitFreq2DialPos != desiredPositionDial2X)
                                    {
                                        dial2OkTime = DateTime.Now.Ticks;
                                        str = R800_L1_FREQ_2DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial2X, _r800L1CockpitFreq2DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial2SendCount++;
                                        Interlocked.Exchange(ref _r800L1Dial2WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial2Timeout);
                                }
                            }
                            else
                            {
                                dial2OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _r800L1Dial3WaitingForFeedback) == 0)
                            {
                                lock (_lockR800L1DialsObject3)
                                {
                                    Common.DebugP("_r800l1CockpitFreq3DialPos is " + _r800L1CockpitFreq3DialPos + " and should be " + desiredPositionDial3X);
                                    if (_r800L1CockpitFreq3DialPos != desiredPositionDial3X)
                                    {
                                        dial3OkTime = DateTime.Now.Ticks;
                                        str = R800_L1_FREQ_3DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial3X, _r800L1CockpitFreq3DialPos);
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial3SendCount++;
                                        Interlocked.Exchange(ref _r800L1Dial3WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial3Timeout);
                                }
                            }
                            else
                            {
                                dial3OkTime = DateTime.Now.Ticks;
                            }
                            var desiredPositionDial4 = 0;
                            if (Interlocked.Read(ref _r800L1Dial4WaitingForFeedback) == 0)
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
                                    Common.DebugP("_r800l1CockpitFreq4DialPos is " + _r800L1CockpitFreq4DialPos + " and should be " + desiredPositionDial4);
                                    if (_r800L1CockpitFreq4DialPos < desiredPositionDial4)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R800_L1_FREQ_4DIAL_COMMAND + "INC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r800L1Dial4WaitingForFeedback, 1);
                                    }
                                    else if (_r800L1CockpitFreq4DialPos > desiredPositionDial4)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R800_L1_FREQ_4DIAL_COMMAND + "DEC\n";
                                        Common.DebugP("Sending " + str);
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r800L1Dial4WaitingForFeedback, 1);
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
                        Common.ShowErrorMessageBox( ex);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _r800L1ThreadNowSynching, 0);
                }

            }
            catch (Exception ex)
            {
                Common.LogError( ex);
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
                _r800L1BigFrequencyStandby = _r800L1SavedCockpitBigFrequency;
                _r800L1SmallFrequencyStandby = _r800L1SavedCockpitSmallFrequency;
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SwapCockpitStandbyFrequencyR800L1()");
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (LockLCDUpdateObject)
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
                            case RadioPanelPZ69KnobsKa50.UPPER_DATALINK:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentKa50RadioMode.DATALINK);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsKa50.UPPER_NO_USE4:
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
                            case RadioPanelPZ69KnobsKa50.LOWER_DATALINK:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentKa50RadioMode.DATALINK);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsKa50.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsKa50.LOWER_NO_USE4:
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
                                        DCSBIOS.Send(radioPanelKnob.IsOn ? VHF1_TUNER_BUTTON_PRESS : VHF1_TUNER_BUTTON_RELEASE);
                                    }
                                    else if (_currentUpperRadioMode == CurrentKa50RadioMode.ADF_ARK22 && radioPanelKnob.IsOn)
                                    {
                                        lock (_lockADFModeDialObject)
                                        {
                                            if (_adfModeSwitchDirectionUp && _adfModeCockpitPos == 2)
                                            {
                                                _adfModeSwitchDirectionUp = false;
                                                DCSBIOS.Send(ADF_MODE_DEC);
                                            }
                                            else if (!_adfModeSwitchDirectionUp && _adfModeCockpitPos == 0)
                                            {
                                                _adfModeSwitchDirectionUp = true;
                                                DCSBIOS.Send(ADF_MODE_INC);
                                            }
                                            else if (_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADF_MODE_INC);
                                            }
                                            else if (!_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADF_MODE_DEC);
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
                                    else if (_currentUpperRadioMode == CurrentKa50RadioMode.DATALINK && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(DATALINK_POWER_ON_OFF_COMMAND_TOGGLE);
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
                                        DCSBIOS.Send(radioPanelKnob.IsOn ? VHF1_TUNER_BUTTON_PRESS : VHF1_TUNER_BUTTON_RELEASE);
                                    }
                                    else if (_currentLowerRadioMode == CurrentKa50RadioMode.ADF_ARK22 && radioPanelKnob.IsOn)
                                    {
                                        lock (_lockADFModeDialObject)
                                        {
                                            if (_adfModeSwitchDirectionUp && _adfModeCockpitPos == 2)
                                            {
                                                _adfModeSwitchDirectionUp = false;
                                                DCSBIOS.Send(ADF_MODE_DEC);
                                            }
                                            else if (!_adfModeSwitchDirectionUp && _adfModeCockpitPos == 0)
                                            {
                                                _adfModeSwitchDirectionUp = true;
                                                DCSBIOS.Send(ADF_MODE_INC);
                                            }
                                            else if (_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADF_MODE_INC);
                                            }
                                            else if (!_adfModeSwitchDirectionUp)
                                            {
                                                DCSBIOS.Send(ADF_MODE_DEC);
                                            }
                                        }
                                    }
                                    else if (_currentLowerRadioMode == CurrentKa50RadioMode.DATALINK && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(DATALINK_POWER_ON_OFF_COMMAND_TOGGLE);
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH);
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
                Common.LogError( ex);
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
                                                    DCSBIOS.Send(VHF1_PRESET_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                //100-149  220-399
                                                if (_r800L1BigFrequencyStandby.Equals(399))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                if (_r800L1BigFrequencyStandby.Equals(149))
                                                {
                                                    _r800L1BigFrequencyStandby = 220;
                                                }
                                                else
                                                {
                                                    _r800L1BigFrequencyStandby++;
                                                }

                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisLeftDialIncreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisLeftDialIncreaseChangeMonitor.ClickThresholdReached() ? ABRIS_LEFT_DIAL_COMMAND_INC_MORE : ABRIS_LEFT_DIAL_COMMAND_INC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkMasterModeChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_MASTER_MODE_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADF_PRESET_COMMAND_INC);
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
                                                    DCSBIOS.Send(VHF1_PRESET_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                //100-149  220-399
                                                if (_r800L1BigFrequencyStandby.Equals(100))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                if (_r800L1BigFrequencyStandby.Equals(220))
                                                {
                                                    _r800L1BigFrequencyStandby = 149;
                                                }
                                                else
                                                {
                                                    _r800L1BigFrequencyStandby--;
                                                }

                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisLeftDialDecreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisLeftDialDecreaseChangeMonitor.ClickThresholdReached() ? ABRIS_LEFT_DIAL_COMMAND_DEC_MORE : ABRIS_LEFT_DIAL_COMMAND_DEC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkMasterModeChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_MASTER_MODE_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADF_PRESET_COMMAND_DEC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800L1SmallFrequencyStandby >= 95)
                                                {
                                                    //At max value
                                                    _r800L1SmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r800L1SmallFrequencyStandby = _r800L1SmallFrequencyStandby + 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisRightDialIncreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisRightDialIncreaseChangeMonitor.ClickThresholdReached() ? ABRIS_RIGHT_DIAL_COMMAND_INC_MORE : ABRIS_RIGHT_DIAL_COMMAND_INC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkSelfIdChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_SELF_ID_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADF_VOLUME_KNOB_COMMAND_INC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800L1SmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _r800L1SmallFrequencyStandby = 95;
                                                    break;
                                                }
                                                _r800L1SmallFrequencyStandby = _r800L1SmallFrequencyStandby - 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisRightDialDecreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisRightDialDecreaseChangeMonitor.ClickThresholdReached() ? ABRIS_RIGHT_DIAL_COMMAND_DEC_MORE : ABRIS_RIGHT_DIAL_COMMAND_DEC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkSelfIdChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_SELF_ID_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADF_VOLUME_KNOB_COMMAND_DEC);
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
                                                    DCSBIOS.Send(VHF1_PRESET_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                var changeFaster = false;
                                                _r800L1BigFreqIncreaseChangeMonitor.Click();
                                                if (_r800L1BigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (changeFaster)
                                                {
                                                    _r800L1BigFrequencyStandby = _r800L1BigFrequencyStandby + CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _r800L1BigFrequencyStandby++;
                                                }
                                                if (_r800L1BigFrequencyStandby > 399)
                                                {
                                                    //@ max value
                                                    _r800L1BigFrequencyStandby = 399;
                                                    break;
                                                }
                                                if (_r800L1BigFrequencyStandby > 149 && _r800L1BigFrequencyStandby < 220)
                                                {
                                                    _r800L1BigFrequencyStandby = _r800L1BigFrequencyStandby - 149 + 220;
                                                }
                                                Common.DebugP("_r800l1BigFrequencyStandby is now " + _r800L1BigFrequencyStandby);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisLeftDialIncreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisLeftDialIncreaseChangeMonitor.ClickThresholdReached() ? ABRIS_LEFT_DIAL_COMMAND_INC_MORE : ABRIS_LEFT_DIAL_COMMAND_INC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkMasterModeChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_MASTER_MODE_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADF_PRESET_COMMAND_INC);
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
                                                    DCSBIOS.Send(VHF1_PRESET_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                var changeFaster = false;
                                                _r800L1BigFreqDecreaseChangeMonitor.Click();
                                                if (_r800L1BigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    //Change faster
                                                    changeFaster = true;
                                                }
                                                //100-149  220-399
                                                if (changeFaster)
                                                {
                                                    _r800L1BigFrequencyStandby = _r800L1BigFrequencyStandby - CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _r800L1BigFrequencyStandby--;
                                                }
                                                if (_r800L1BigFrequencyStandby <= 100)
                                                {
                                                    //@ max value
                                                    _r800L1BigFrequencyStandby = 100;
                                                    break;
                                                }
                                                if (_r800L1BigFrequencyStandby > 149 && _r800L1BigFrequencyStandby < 220)
                                                {
                                                    _r800L1BigFrequencyStandby = 149 - (220 - _r800L1BigFrequencyStandby);
                                                }
                                                Common.DebugP("_r800l1BigFrequencyStandby is now " + _r800L1BigFrequencyStandby);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisLeftDialDecreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisLeftDialDecreaseChangeMonitor.ClickThresholdReached() ? ABRIS_LEFT_DIAL_COMMAND_DEC_MORE : ABRIS_LEFT_DIAL_COMMAND_DEC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkMasterModeChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_MASTER_MODE_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                if (!SkipADFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ADF_PRESET_COMMAND_DEC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800L1SmallFrequencyStandby >= 95)
                                                {
                                                    //At max value
                                                    _r800L1SmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _r800L1SmallFrequencyStandby = _r800L1SmallFrequencyStandby + 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisRightDialIncreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisRightDialIncreaseChangeMonitor.ClickThresholdReached() ? ABRIS_RIGHT_DIAL_COMMAND_INC_MORE : ABRIS_RIGHT_DIAL_COMMAND_INC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkSelfIdChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_SELF_ID_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADF_VOLUME_KNOB_COMMAND_INC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.VHF2_R800L1:
                                            {
                                                if (_r800L1SmallFrequencyStandby <= 0)
                                                {
                                                    //At min value
                                                    _r800L1SmallFrequencyStandby = 95;
                                                    break;
                                                }
                                                _r800L1SmallFrequencyStandby = _r800L1SmallFrequencyStandby - 5;
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ABRIS:
                                            {
                                                _abrisRightDialDecreaseChangeMonitor.Click();
                                                DCSBIOS.Send(_abrisRightDialDecreaseChangeMonitor.ClickThresholdReached() ? ABRIS_RIGHT_DIAL_COMMAND_DEC_MORE : ABRIS_RIGHT_DIAL_COMMAND_DEC);
                                                break;
                                            }
                                        case CurrentKa50RadioMode.DATALINK:
                                            {
                                                if (!SkipDataLinkSelfIdChange())
                                                {
                                                    DCSBIOS.Send(DATALINK_SELF_ID_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentKa50RadioMode.ADF_ARK22:
                                            {
                                                DCSBIOS.Send(ADF_VOLUME_KNOB_COMMAND_DEC);
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
                Common.LogError( ex);
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
                if (_r800L1BigFrequencyStandby < 100)
                {
                    _r800L1BigFrequencyStandby = 100;
                }
                if (_r800L1BigFrequencyStandby > 399)
                {
                    _r800L1BigFrequencyStandby = 399;
                }
                if (_r800L1BigFrequencyStandby == 399 && _r800L1SmallFrequencyStandby > 0)
                {
                    _r800L1SmallFrequencyStandby = 0;
                }
                if (_r800L1BigFrequencyStandby == 149 && _r800L1SmallFrequencyStandby > 0)
                {
                    _r800L1SmallFrequencyStandby = 0;
                }
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
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
                Common.LogError( ex);
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
                Common.LogError( ex);
            }
            return false;
        }

        private bool SkipDataLinkMasterModeChange()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SkipDataLinkMasterModeChange()");
                if (_currentUpperRadioMode == CurrentKa50RadioMode.DATALINK || _currentLowerRadioMode == CurrentKa50RadioMode.DATALINK)
                {
                    if (_datalinkMasterModeDialSkipper > 2)
                    {
                        _datalinkMasterModeDialSkipper = 0;
                        Common.DebugP("Leaving Ka-50 Radio SkipDataLinkMasterModeChange()");
                        return false;
                    }
                    _datalinkMasterModeDialSkipper++;
                    Common.DebugP("Leaving Ka-50 Radio SkipDataLinkMasterModeChange()");
                    return true;
                }
                Common.DebugP("Leaving Ka-50 Radio SkipDataLinkMasterModeChange()");
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
            }
            return false;
        }

        private bool SkipDataLinkSelfIdChange()
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio SkipDataLinkSelfIdChange()");
                if (_currentUpperRadioMode == CurrentKa50RadioMode.DATALINK || _currentLowerRadioMode == CurrentKa50RadioMode.DATALINK)
                {
                    if (_datalinkSelfIdDialSkipper > 2)
                    {
                        _datalinkSelfIdDialSkipper = 0;
                        Common.DebugP("Leaving Ka-50 Radio SkipDataLinkSelfIdChange()");
                        return false;
                    }
                    _datalinkSelfIdDialSkipper++;
                    Common.DebugP("Leaving Ka-50 Radio SkipDataLinkSelfIdChange()");
                    return true;
                }
                Common.DebugP("Leaving Ka-50 Radio SkipDataLinkSelfIdChange()");
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
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
                        return;
                    }
                    if (!FirstReportHasBeenRead)
                    {
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.VHF2_R800L1:
                            {
                                var frequencyAsString = "";
                                lock (_lockR800L1DialsObject1)
                                {
                                    try
                                    {
                                        frequencyAsString = _r800L1Freq1DialValues[_r800L1CockpitFreq1DialPos].ToString();
                                    }
                                    catch (Exception e)
                                    {
                                        throw new Exception("Failed to get dial position from array _r800l1Freq1DialValues based on index " + _r800L1CockpitFreq1DialPos + ". Max array pos is " + _r800L1Freq1DialValues.Length + "\n" + e.Message + "\n" + e.StackTrace);
                                    }
                                }
                                lock (_lockR800L1DialsObject2)
                                {

                                    frequencyAsString = frequencyAsString + _r800L1CockpitFreq2DialPos;
                                }
                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR800L1DialsObject3)
                                {

                                    frequencyAsString = frequencyAsString + _r800L1CockpitFreq3DialPos;
                                }
                                lock (_lockR800L1DialsObject4)
                                {

                                    frequencyAsString = frequencyAsString + GetR800L1DialFrequencyForPosition(_r800L1CockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r800L1BigFrequencyStandby + "." + _r800L1SmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentKa50RadioMode.ABRIS:
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32("88888"), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.DATALINK:
                            {
                                uint masterMode = 0;
                                uint selfId = 0;
                                uint power = 0;
                                lock (_lockDatalinkMasterModeObject)
                                {
                                    masterMode = _datalinkMasterModeCockpitPos;
                                }
                                lock (_lockDatalinkSelfIdObject)
                                {
                                    selfId = _datalinkSelfIdCockpitPos + 1;
                                }
                                lock (_lockDatalinkPowerOnOffObject)
                                {
                                    power = _datalinkPowerOnOffCockpitPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, masterMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, (power + "   " + selfId), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, adfMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.VHF2_R800L1:
                            {
                                var frequencyAsString = "";
                                lock (_lockR800L1DialsObject1)
                                {
                                    frequencyAsString = _r800L1Freq1DialValues[_r800L1CockpitFreq1DialPos].ToString();
                                }
                                lock (_lockR800L1DialsObject2)
                                {

                                    frequencyAsString = frequencyAsString + _r800L1CockpitFreq2DialPos;
                                }
                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR800L1DialsObject3)
                                {

                                    frequencyAsString = frequencyAsString + _r800L1CockpitFreq3DialPos;
                                }
                                lock (_lockR800L1DialsObject4)
                                {

                                    frequencyAsString = frequencyAsString + GetR800L1DialFrequencyForPosition(_r800L1CockpitFreq4DialPos);
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_r800L1BigFrequencyStandby + "." + _r800L1SmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentKa50RadioMode.ABRIS:
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32("88888"), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.DATALINK:
                            {
                                uint masterMode = 0;
                                uint selfId = 0;
                                uint power = 0;
                                lock (_lockDatalinkMasterModeObject)
                                {
                                    masterMode = _datalinkMasterModeCockpitPos;
                                }
                                lock (_lockDatalinkSelfIdObject)
                                {
                                    selfId = _datalinkSelfIdCockpitPos + 1;
                                }
                                lock (_lockDatalinkPowerOnOffObject)
                                {
                                    power = _datalinkPowerOnOffCockpitPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, masterMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, (power + "   " + selfId), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, adfMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentKa50RadioMode.NOUSE:
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
                Common.LogError( ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving Ka-50 Radio ShowFrequenciesOnPanel()");
        }


        protected override void GamingPanelKnobChanged(IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(hashSet);
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("Ka-50");

                //VHF1
                _vhf1DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R828_CHANNEL");

                //VHF2
                _r800L1DcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ1");
                _r800L1DcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ2");
                _r800L1DcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ3");
                _r800L1DcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("R800_FREQ4");

                //ADF
                _adfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_CHANNEL");
                _adfModeDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_NDB_MODE");

                //NAV2 Datalink
                _datalinkMasterModeDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("DLNK_MASTER_MODE");
                _datalinkSelfIdDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("DLNK_SELF_ID");
                _datalinkPowerOnOffDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("PVI_POWER");



                StartListeningForPanelChanges();
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69Ka50.StartUp() : " + ex.Message);
                Common.LogError( ex);
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

        public override void ClearSettings() { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobKa50.GetRadioPanelKnobs();
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
                Common.LogError( ex);
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
                Common.LogError( ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SetLowerRadioMode()");
        }

        private bool R800L1NowSyncing()
        {
            return Interlocked.Read(ref _r800L1ThreadNowSynching) > 0;
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
                                switch (_r800L1CockpitFreq4DialPos)
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
                                _r800L1SavedCockpitBigFrequency = uint.Parse(_r800L1Freq1DialValues[_r800L1CockpitFreq1DialPos].ToString() + _r800L1CockpitFreq2DialPos.ToString());
                                _r800L1SavedCockpitSmallFrequency = uint.Parse(_r800L1CockpitFreq3DialPos.ToString() + dial4.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
            }
            Common.DebugP("Leaving Ka-50 Radio SaveCockpitFrequencyR800L1()");
        }


        private string GetCommandDirectionForR800L1Dial1(int desiredDialPosition, uint actualDialPosition)
        {
            const string inc = "INC\n";
            const string dec = "DEC\n";
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
                    if (tmpPos <= _r800L1Freq1DialValues.Length - 1)
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
                        tmpPos = unchecked((uint)_r800L1Freq1DialValues.Length - 1);
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
                Common.LogError( ex);
            }
            return inc;
        }

        private string GetCommandDirectionFor0To9Dials(int desiredDialPosition, uint actualDialPosition)
        {
            try
            {
                Common.DebugP("Entering Ka-50 Radio GetCommandDirectionFor0To9Dials()");
                const string inc = "INC\n";
                const string dec = "DEC\n";
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
                Common.LogError( ex);
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
                Common.LogError( ex);
            }
            return "";
        }

        public override string SettingsVersion()
        {
            return "0X";
        }
    }
}
