using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69F14B : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private HashSet<RadioPanelKnobF14B> _radioPanelKnobs = new HashSet<RadioPanelKnobF14B>();
        private CurrentF14RadioMode _currentUpperRadioMode = CurrentF14RadioMode.UHF;
        private CurrentF14RadioMode _currentLowerRadioMode = CurrentF14RadioMode.UHF;

        private bool _upperButtonPressed = false;
        private bool _lowerButtonPressed = false;
        private bool _upperButtonPressedAndDialRotated = false;
        private bool _lowerButtonPressedAndDialRotated = false;
        private bool _ignoreUpperButtonOnce = true;
        private bool _ignoreLowerButtonOnce = true;

        /* UHF AN/ARC-159 */
        //Large dial 225-399 [step of 1]
        //Small dial 0.00-0.97 [step of x.x[0 2 5 7]
        private uint _uhfBigFrequencyStandby = 225;
        private uint _uhfSmallFrequencyStandby;
        private uint _uhfCockpitBigFrequency;
        private uint _uhfCockpitDial3Frequency;
        private uint _uhfCockpitDial4Frequency;
        private uint _uhfSavedCockpitBigFrequency;
        private uint _uhfSavedCockpitDial3Frequency;
        private uint _uhfSavedCockpitDial4Frequency;
        private readonly object _lockUhfBigFreqObject1 = new object();
        private readonly object _lockUhfDial3FreqObject2 = new object();
        private readonly object _lockUhfDial4FreqObject2 = new object();
        private DCSBIOSOutput _uhfDcsbiosOutputBigFrequencyNumber;
        private DCSBIOSOutput _uhfDcsbiosOutputDial3FrequencyNumber;
        private DCSBIOSOutput _uhfDcsbiosOutputDial4FrequencyNumber;
        private volatile uint _uhfCockpitFreq1VirtualDialPos = 1;
        private volatile uint _uhfCockpitFreq2VirtualDialPos = 1;
        private volatile uint _uhfCockpitFreq3VirtualDialPos = 1;
        private volatile uint _uhfCockpitFreq4VirtualDialPos = 1;
        private DCSBIOSOutput _uhfDcsbiosOutputChannelFreqMode;  // 0 = PRESET
        private DCSBIOSOutput _uhfDcsbiosOutputSelectedChannel;
        private volatile uint _uhfCockpitFreqMode = 0;
        private volatile uint _uhfCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _uhfChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _uhfFreqModeClickSpeedDetector = new ClickSpeedDetector(6);
        
        private const string UHF_PRESET_INCREASE = "PLT_UHF1_PRESETS INC\n";
        private const string UHF_PRESET_DECREASE = "PLT_UHF1_PRESETS DEC\n";
        private const string UHF_FREQ_MODE_INCREASE = "PLT_UHF1_FREQ_MODE INC\n";
        private const string UHF_FREQ_MODE_DECREASE = "PLT_UHF1_FREQ_MODEDEC\n";

        private const string UHF_MODE_INCREASE = "PLT_UHF1_FUNCTION INC\n";
        private const string UHF_MODE_DECREASE = "PLT_UHF1_FUNCTION DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputMode;
        private volatile uint _uhfCockpitMode = 0; // OFF = 0
        private readonly ClickSpeedDetector _uhfModeClickSpeedDetector = new ClickSpeedDetector(8);
        private byte _skipUhfSmallFreqChange = 0;

        /*A-10C AN/ARC-164 UHF Radio 2*/
        //Large dial 225-399 [step of 1]
        //Small dial 0.00-0.97 [step of 0 2 5 7]
        private double _xuhfBigFrequencyStandby = 299;
        private double _xuhfSmallFrequencyStandby;
        private double _xuhfSavedCockpitBigFrequency;
        private double _xuhfSavedCockpitSmallFrequency;
        private readonly object _xlockUhfDialsObject1 = new object();
        private readonly object _xlockUhfDialsObject2 = new object();
        private readonly object _xlockUhfDialsObject3 = new object();
        private readonly object _xlockUhfDialsObject4 = new object();
        private readonly object _xlockUhfDialsObject5 = new object();
        private DCSBIOSOutput _xuhfDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _xuhfDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _xuhfDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _xuhfDcsbiosOutputFreqDial4;
        private DCSBIOSOutput _xuhfDcsbiosOutputFreqDial5;
        private volatile uint _xuhfCockpitFreq1DialPos = 1;
        private volatile uint _xuhfCockpitFreq2DialPos = 1;
        private volatile uint _xuhfCockpitFreq3DialPos = 1;
        private volatile uint _xuhfCockpitFreq4DialPos = 1;
        private volatile uint _xuhfCockpitFreq5DialPos = 1;
        private const string xUhfFreq1DialCommand = "UHF_110MHZ_SEL ";		//"2" "3" "A"
        private const string xUhfFreq2DialCommand = "UHF_10MHZ_SEL ";		//0 1 2 3 4 5 6 7 8 9
        private const string xUhfFreq3DialCommand = "UHF_1MHZ_SEL ";			//0 1 2 3 4 5 6 7 8 9
        private const string xUhfFreq4DialCommand = "UHF_POINT1MHZ_SEL ";    //0 1 2 3 4 5 6 7 8 9
        private const string xUhfFreq5DialCommand = "UHF_POINT25_SEL ";		//"00" "25" "50" "75"
        private Thread _xuhfSyncThread;
        private long _xuhfThreadNowSynching;
        private long _xuhfDial1WaitingForFeedback;
        private long _xuhfDial2WaitingForFeedback;
        private long _xuhfDial3WaitingForFeedback;
        private long _xuhfDial4WaitingForFeedback;
        private long _xuhfDial5WaitingForFeedback;
        private const string xUhfPresetIncrease = "UHF_PRESET_SEL INC\n";
        private const string xUhfPresetDecrease = "UHF_PRESET_SEL DEC\n";
        private const string xUhfFreqModeIncrease = "UHF_MODE INC\n";
        private const string xUhfFreqModeDecrease = "UHF_MODE DEC\n";
        private DCSBIOSOutput _xuhfDcsbiosOutputFreqMode;  // 1 = PRESET
        private DCSBIOSOutput _xuhfDcsbiosOutputSelectedChannel;
        private volatile uint _xuhfCockpitFreqMode = 0;
        private volatile uint _xuhfCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _xuhfChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _xuhfFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string xUhfFunctionIncrease = "UHF_FUNCTION INC\n";
        private const string xUhfFunctionDecrease = "UHF_FUNCTION DEC\n";
        private DCSBIOSOutput _xuhfDcsbiosOutputFunction;  // UHF_FUNCTION
        private volatile uint _xuhfCockpitFunction = 0;
        private readonly ClickSpeedDetector _xuhfFunctionClickSpeedDetector = new ClickSpeedDetector(8);

        /*A-10C AN/ARC-186(V) VHF FM Radio 3*/
        //Large dial 30-76 [step of 1]
        //Small dial 000 - 975 [0 2 5 7]
        private uint _vhfFmBigFrequencyStandby = 45;
        private uint _vhfFmSmallFrequencyStandby;
        private uint _vhfFmSavedCockpitBigFrequency;
        private uint _vhfFmSavedCockpitSmallFrequency;
        private readonly object _lockVhfFmDialsObject1 = new object();
        private readonly object _lockVhfFmDialsObject2 = new object();
        private readonly object _lockVhfFmDialsObject3 = new object();
        private readonly object _lockVhfFmDialsObject4 = new object();
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial4;
        private volatile uint _vhfFmCockpitFreq1DialPos = 1;
        private volatile uint _vhfFmCockpitFreq2DialPos = 1;
        private volatile uint _vhfFmCockpitFreq3DialPos = 1;
        private volatile uint _vhfFmCockpitFreq4DialPos = 1;
        private const string VhfFmFreq1DialCommand = "VHFFM_FREQ1 ";
        private const string VhfFmFreq2DialCommand = "VHFFM_FREQ2 ";
        private const string VhfFmFreq3DialCommand = "VHFFM_FREQ3 ";
        private const string VhfFmFreq4DialCommand = "VHFFM_FREQ4 ";
        private Thread _vhfFmSyncThread;
        private long _vhfFmThreadNowSynching;
        private long _vhfFmDial1WaitingForFeedback;
        private long _vhfFmDial2WaitingForFeedback;
        private long _vhfFmDial3WaitingForFeedback;
        private long _vhfFmDial4WaitingForFeedback;
        private const string VhfFmPresetIncrease = "VHFFM_PRESET INC\n";
        private const string VhfFmPresetDecrease = "VHFFM_PRESET DEC\n";
        private const string VhfFmFreqModeIncrease = "VHFFM_FREQEMER INC\n";
        private const string VhfFmFreqModeDecrease = "VHFFM_FREQEMER DEC\n";
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqMode;// 3 = PRESET
        private DCSBIOSOutput _vhfFmDcsbiosOutputSelectedChannel;
        private volatile uint _vhfFmCockpitFreqMode = 0;
        private volatile uint _vhfFmCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _vhfFmChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _vhfFmFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string VhfFmModeIncrease = "VHFFM_MODE INC\n";
        private const string VhfFmModeDecrease = "VHFFM_MODE DEC\n";
        private DCSBIOSOutput _vhfFmDcsbiosOutputMode;// VHFFM_MODE
        private volatile uint _vhfFmCockpitMode = 0;
        private readonly ClickSpeedDetector _vhfFmModeClickSpeedDetector = new ClickSpeedDetector(6);

        /*A-10C ILS*/
        //Large dial 108-111 [step of 1]
        //Small dial 10-95 [step of 5]
        private uint _ilsBigFrequencyStandby = 108; //"108" "109" "110" "111"
        private uint _ilsSmallFrequencyStandby = 10; //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
        private uint _ilsSavedCockpitBigFrequency = 108; //"108" "109" "110" "111"
        private uint _ilsSavedCockpitSmallFrequency = 10; //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
        private readonly object _lockIlsDialsObject1 = new object();
        private readonly object _lockIlsDialsObject2 = new object();
        private DCSBIOSOutput _ilsDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _ilsDcsbiosOutputFreqDial2;
        private volatile uint _ilsCockpitFreq1DialPos = 1;
        private volatile uint _ilsCockpitFreq2DialPos = 1;
        private const string ILSFreq1DialCommand = "ILS_MHZ ";
        private const string ILSFreq2DialCommand = "ILS_KHZ ";
        private Thread _ilsSyncThread;
        private long _ilsThreadNowSynching;
        private long _ilsDial1WaitingForFeedback;
        private long _ilsDial2WaitingForFeedback;


        /*TACAN*/
        //Large dial 0-12 [step of 1]
        //Small dial 0-9 [step of 1]
        //Last : X/Y [0,1]
        private int _tacanBigFrequencyStandby = 6;
        private int _tacanSmallFrequencyStandby = 5;
        private int _tacanXYStandby;
        private int _tacanSavedCockpitBigFrequency = 6;
        private int _tacanSavedCockpitSmallFrequency = 5;
        private int _tacanSavedCockpitXY;
        private readonly object _lockTacanDialsObject1 = new object();
        private readonly object _lockTacanDialsObject2 = new object();
        private readonly object _lockTacanDialsObject3 = new object();
        private DCSBIOSOutput _tacanDcsbiosOutputFreqChannel;
        private volatile uint _tacanCockpitFreq1DialPos = 1;
        private volatile uint _tacanCockpitFreq2DialPos = 1;
        private volatile uint _tacanCockpitFreq3DialPos = 1;
        private const string TacanFreq1DialCommand = "TACAN_10 ";
        private const string TacanFreq2DialCommand = "TACAN_1 ";
        private const string TacanFreq3DialCommand = "TACAN_XY ";
        private Thread _tacanSyncThread;
        private long _tacanThreadNowSynching;
        private long _tacanDial1WaitingForFeedback;
        private long _tacanDial2WaitingForFeedback;
        private long _tacanDial3WaitingForFeedback;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69F14B(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69F14B()
        {
            _vhfFmSyncThread?.Abort();
            _xuhfSyncThread?.Abort();
            _ilsSyncThread?.Abort();
            _tacanSyncThread?.Abort();
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateCounter(e.Address, e.Data);


            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */
            //UHF
            if (e.Address == _uhfDcsbiosOutputBigFrequencyNumber.Address)
            {
                lock (_lockUhfBigFreqObject1)
                {
                    var tmp = _uhfCockpitBigFrequency;
                    _uhfCockpitBigFrequency = _uhfDcsbiosOutputBigFrequencyNumber.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitBigFrequency)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputDial3FrequencyNumber.Address)
            {
                lock (_lockUhfDial3FreqObject2)
                {
                    var tmp = _uhfCockpitDial3Frequency;
                    _uhfCockpitDial3Frequency = _uhfDcsbiosOutputDial3FrequencyNumber.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitDial3Frequency)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputDial4FrequencyNumber.Address)
            {
                lock (_lockUhfDial4FreqObject2)
                {
                    var tmp = _uhfCockpitDial4Frequency;
                    _uhfCockpitDial4Frequency = _uhfDcsbiosOutputDial4FrequencyNumber.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitDial4Frequency)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputChannelFreqMode.Address)
            {
                var tmp = _uhfCockpitFreqMode;
                _uhfCockpitFreqMode = _uhfDcsbiosOutputChannelFreqMode.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            if (e.Address == _uhfDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _uhfCockpitPresetChannel;
                _uhfCockpitPresetChannel = _uhfDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _uhfCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            if (e.Address == _uhfDcsbiosOutputMode.Address)
            {
                var tmp = _uhfCockpitMode;
                _uhfCockpitMode = _uhfDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            /*
            //UHF
            if (e.Address == _xuhfDcsbiosOutputFreqDial1.Address)
            {
                lock (_xlockUhfDialsObject1)
                {
                    var tmp = _xuhfCockpitFreq1DialPos;
                    _xuhfCockpitFreq1DialPos = _xuhfDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _xuhfCockpitFreq1DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Common.DebugP("_xuhfCockpitFreq1DialPos Before : " + tmp + "  now: " + _xuhfCockpitFreq1DialPos);
                        Interlocked.Exchange(ref _xuhfDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _xuhfDcsbiosOutputFreqDial2.Address)
            {
                lock (_xlockUhfDialsObject2)
                {
                    var tmp = _xuhfCockpitFreq2DialPos;
                    _xuhfCockpitFreq2DialPos = _xuhfDcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                    if (tmp != _xuhfCockpitFreq2DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _xuhfDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _xuhfDcsbiosOutputFreqDial3.Address)
            {
                lock (_xlockUhfDialsObject3)
                {
                    var tmp = _xuhfCockpitFreq3DialPos;
                    _xuhfCockpitFreq3DialPos = _xuhfDcsbiosOutputFreqDial3.GetUIntValue(e.Data);
                    if (tmp != _xuhfCockpitFreq3DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _xuhfDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _xuhfDcsbiosOutputFreqDial4.Address)
            {
                lock (_xlockUhfDialsObject4)
                {
                    var tmp = _xuhfCockpitFreq4DialPos;
                    _xuhfCockpitFreq4DialPos = _xuhfDcsbiosOutputFreqDial4.GetUIntValue(e.Data);
                    if (tmp != _xuhfCockpitFreq4DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _xuhfDial4WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _xuhfDcsbiosOutputFreqDial5.Address)
            {
                lock (_xlockUhfDialsObject5)
                {
                    var tmp = _xuhfCockpitFreq5DialPos;
                    _xuhfCockpitFreq5DialPos = _xuhfDcsbiosOutputFreqDial5.GetUIntValue(e.Data);
                    if (tmp != _xuhfCockpitFreq5DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _xuhfDial5WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _xuhfDcsbiosOutputFreqMode.Address)
            {
                var tmp = _xuhfCockpitFreqMode;
                _xuhfCockpitFreqMode = _xuhfDcsbiosOutputFreqMode.GetUIntValue(e.Data);
                if (tmp != _xuhfCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            if (e.Address == _xuhfDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _xuhfCockpitPresetChannel;
                _xuhfCockpitPresetChannel = _xuhfDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _xuhfCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            if (e.Address == _xuhfDcsbiosOutputFunction.Address)
            {
                var tmp = _xuhfCockpitFunction;
                _xuhfCockpitFunction = _xuhfDcsbiosOutputFunction.GetUIntValue(e.Data);
                if (tmp != _xuhfCockpitFunction)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }

            //VHF FM
            if (e.Address == _vhfFmDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockVhfFmDialsObject1)
                {
                    var tmp = _vhfFmCockpitFreq1DialPos;
                    _vhfFmCockpitFreq1DialPos = _vhfFmDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _vhfFmCockpitFreq1DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputFreqDial2.Address)
            {
                lock (_lockVhfFmDialsObject2)
                {
                    var tmp = _vhfFmCockpitFreq2DialPos;
                    _vhfFmCockpitFreq2DialPos = _vhfFmDcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                    if (tmp != _vhfFmCockpitFreq2DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputFreqDial3.Address)
            {
                lock (_lockVhfFmDialsObject3)
                {
                    var tmp = _vhfFmCockpitFreq3DialPos;
                    _vhfFmCockpitFreq3DialPos = _vhfFmDcsbiosOutputFreqDial3.GetUIntValue(e.Data);
                    if (tmp != _vhfFmCockpitFreq3DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputFreqDial4.Address)
            {
                lock (_lockVhfFmDialsObject4)
                {
                    var tmp = _vhfFmCockpitFreq4DialPos;
                    _vhfFmCockpitFreq4DialPos = _vhfFmDcsbiosOutputFreqDial4.GetUIntValue(e.Data);
                    if (tmp != _vhfFmCockpitFreq4DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputFreqMode.Address)
            {
                var tmp = _vhfFmCockpitFreqMode;
                _vhfFmCockpitFreqMode = _vhfFmDcsbiosOutputFreqMode.GetUIntValue(e.Data);
                if (tmp != _vhfFmCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _vhfFmCockpitPresetChannel;
                _vhfFmCockpitPresetChannel = _vhfFmDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _vhfFmCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputMode.Address)
            {
                var tmp = _vhfFmCockpitMode;
                _vhfFmCockpitMode = _vhfFmDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _vhfFmCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }

            //ILS
            if (e.Address == _ilsDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockIlsDialsObject1)
                {
                    var tmp = _ilsCockpitFreq1DialPos;
                    _ilsCockpitFreq1DialPos = _ilsDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _ilsCockpitFreq1DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _ilsDcsbiosOutputFreqDial2.Address)
            {
                lock (_lockIlsDialsObject2)
                {
                    var tmp = _ilsCockpitFreq2DialPos;
                    _ilsCockpitFreq2DialPos = _ilsDcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                    if (tmp != _ilsCockpitFreq2DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 0);
                    }
                }
            }
            */
            //TACAN is set via String listener

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                //Common.DebugP("RadioPanelPZ69F14B Received DCSBIOS stringData : ->" + e.StringData + "<-");
                /*if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    //Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }
                if (e.Address.Equals(_tacanDcsbiosOutputFreqChannel.Address))
                {
                    try
                    {
                        int changeCount = 0;
                        //" 00X" --> "129X"
                        lock (_lockTacanDialsObject1)
                        {
                            if (!uint.TryParse(e.StringData.Substring(0, 2), out var tmpUint))
                            {
                                return;
                            }
                            if (tmpUint != _tacanCockpitFreq1DialPos)
                            {
                                changeCount = changeCount | 2;
                                _tacanCockpitFreq1DialPos = tmpUint;
                            }
                        }
                        lock (_lockTacanDialsObject2)
                        {
                            if (!uint.TryParse(e.StringData.Substring(2, 1), out var tmpUint))
                            {
                                return;
                            }
                            if (tmpUint != _tacanCockpitFreq2DialPos)
                            {
                                changeCount = changeCount | 4;
                                _tacanCockpitFreq2DialPos = tmpUint;
                            }
                        }
                        lock (_lockTacanDialsObject3)
                        {
                            var tmp = _tacanCockpitFreq3DialPos;
                            var tmpXY = e.StringData.Substring(3, 1);
                            _tacanCockpitFreq3DialPos = tmpXY.Equals("X") ? (uint)0 : (uint)1;
                            if (tmp != _tacanCockpitFreq3DialPos)
                            {
                                changeCount = changeCount | 8;
                            }
                        }

                        if ((changeCount & 2) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial1WaitingForFeedback, 0);
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }

                        if ((changeCount & 4) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 0);
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }

                        if ((changeCount & 8) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 0);
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }

                    }
                    catch (Exception)
                    {
                        //Common.LogError(123, "DCSBIOSStringReceived TACAN: >" + e.StringData + "< " + exception.Message + " \n" + exception.StackTrace);
                        //TODO Strange values from DCS-BIOS
                    }
                }*/
            }
            catch (Exception ex)
            {
                Common.LogError(349998, ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF14B knob)
        {
            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }
            switch (knob)
            {
                case RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH:
                    {
                        if (_ignoreUpperButtonOnce)
                        {
                            //Don't do anything on the very first button press as the panel sends ALL
                            //switches when it is manipulated the first time
                            //This would cause unintended sync.
                            _ignoreUpperButtonOnce = false;
                            return;
                        }
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentF14RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SaveCockpitFrequencyUhf();
                                        var freq = _uhfBigFrequencyStandby * 1000 + _uhfSmallFrequencyStandby;
                                        DCSBIOS.Send("SET_UHF_FREQ " + freq + "\n");
                                        SwapCockpitStandbyFrequencyUhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.VUHF:
                                {
                                    if (_xuhfCockpitFunction != 0 && !UhfPresetSelected())
                                    {
                                        //SendUhfToDCSBIOS();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                            case CurrentF14RadioMode.KY28:
                                {
                                    //SendTacanToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
                case RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH:
                    {
                        if (_ignoreLowerButtonOnce)
                        {
                            //Don't do anything on the very first button press as the panel sends ALL
                            //switches when it is manipulated the first time
                            //This would cause unintended sync.
                            _ignoreLowerButtonOnce = false;
                            return;
                        }
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentF14RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SaveCockpitFrequencyUhf();
                                        var freq = _uhfBigFrequencyStandby * 1000 + _uhfSmallFrequencyStandby;
                                        DCSBIOS.Send("SET_UHF_FREQ " + freq + "\n");
                                        SwapCockpitStandbyFrequencyUhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.VUHF:
                                {
                                    if (_xuhfCockpitFunction != 0 && !UhfPresetSelected())
                                    {
                                        //SendUhfToDCSBIOS();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                            case CurrentF14RadioMode.KY28:
                                {
                                    //SendTacanToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendxUhfToDCSBIOS()
        {
            /*if (UhfNowSyncing())
            {
                return;
            }*/
            SaveCockpitFrequencyxUhf();
            //Frequency selector 1     
            //       "2"  "3"  "A"
            //Pos     0    1    2

            //Frequency selector 2      
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3
            //0 1 2 3 4 5 6 7 8 9


            //Frequency selector 4
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 5
            //      "00" "25" "50" "75", only "00" and "50" used.
            //Pos     0    1    2    3

            //Large dial 225-399 [step of 1]
            //Small dial 0.00-0.95 [step of 0.05]
            var frequency = _xuhfBigFrequencyStandby + _xuhfSmallFrequencyStandby;
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);


            var freqDial1 = 0;
            var freqDial2 = 0;
            var freqDial3 = 0;
            var freqDial4 = 0;
            var freqDial5 = 0;

            //Special case! If Dial 1 = "A" then all digits can be disregarded once they are set to zero
            switch (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture))
            {
                //0.075Mhz
                case 1:
                    {
                        freqDial1 = -1; // ("A")
                        freqDial2 = 0;
                        freqDial3 = int.Parse(frequencyAsString.Substring(0, 1));
                        freqDial4 = int.Parse(frequencyAsString.Substring(2, 1));
                        freqDial5 = int.Parse(frequencyAsString.Substring(3, 1));
                        break;
                    }
                //10.075Mhz
                case 2:
                    {
                        freqDial1 = -1; // ("A")
                        freqDial2 = int.Parse(frequencyAsString.Substring(0, 1));
                        freqDial3 = int.Parse(frequencyAsString.Substring(1, 1));
                        freqDial4 = int.Parse(frequencyAsString.Substring(3, 1));
                        freqDial5 = int.Parse(frequencyAsString.Substring(4, 1));
                        break;
                    }
                //100.075Mhz
                case 3:
                    {
                        freqDial1 = int.Parse(frequencyAsString.Substring(0, 1));
                        freqDial2 = int.Parse(frequencyAsString.Substring(1, 1));
                        freqDial3 = int.Parse(frequencyAsString.Substring(2, 1));
                        freqDial4 = int.Parse(frequencyAsString.Substring(4, 1));
                        freqDial5 = int.Parse(frequencyAsString.Substring(5, 1));
                        break;
                    }
            }
            switch (freqDial5)
            {
                //Frequency selector 5
                //      "00" "25" "50" "75", only 0 2 5 7 used.
                //Pos     0    1    2    3
                case 0:
                    {
                        freqDial5 = 0;
                        break;
                    }
                case 2:
                    {
                        freqDial5 = 1;
                        break;
                    }
                case 5:
                    {
                        freqDial5 = 2;
                        break;
                    }
                case 7:
                    {
                        freqDial5 = 3;
                        break;
                    }
            }
            //Frequency selector 1     
            //       "2"  "3"  "A"/"-1"
            //Pos     0    1    2

            //Frequency selector 2      
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3
            //0 1 2 3 4 5 6 7 8 9


            //Frequency selector 4
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 5
            //      "00" "25" "50" "75", only "00" and "50" used.
            //Pos     0    1    2    3

            //Large dial 225-399 [step of 1]
            //Small dial 0.00-0.95 [step of 0.05]

            //#1
            _xuhfSyncThread?.Abort();
            if (freqDial1 >= 2 && freqDial1 <= 3)
            {
                _xuhfSyncThread = new Thread(() => xUhfSynchThreadMethod(freqDial1 - 2, freqDial2, freqDial3, freqDial4, freqDial5));
            }
            else
            {
                //The first dial is set to "A", pos 2   (freqDial1 == -1)
                _xuhfSyncThread = new Thread(() => xUhfSynchThreadMethod(2, freqDial2, freqDial3, freqDial4, freqDial5));
            }
            _xuhfSyncThread.Start();
        }

        private void xUhfSynchThreadMethod(int desiredPosition1, int desiredPosition2, int desiredPosition3, int desiredPosition4, int desiredPosition5)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _xuhfThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial3Timeout = DateTime.Now.Ticks;
                    long dial4Timeout = DateTime.Now.Ticks;
                    long dial5Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    long dial3OkTime = 0;
                    long dial4OkTime = 0;
                    long dial5OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    var dial3SendCount = 0;
                    var dial4SendCount = 0;
                    var dial5SendCount = 0;
                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "UHF dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _xuhfDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "UHF dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _xuhfDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 2");
                        }
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "UHF dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _xuhfDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 3");
                        }
                        if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "UHF dial4Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _xuhfDial4WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 4");
                        }
                        if (IsTimedOut(ref dial5Timeout, ResetSyncTimeout, "UHF dial5Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _xuhfDial5WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 5");
                        }
                        //Frequency selector 1     
                        //       "2"  "3"  "A"/"-1"
                        //Pos     0    1    2
                        if (Interlocked.Read(ref _xuhfDial1WaitingForFeedback) == 0)
                        {
                            lock (_xlockUhfDialsObject1)
                            {
                                if (_xuhfCockpitFreq1DialPos != desiredPosition1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                }
                                if (_xuhfCockpitFreq1DialPos < desiredPosition1)
                                {
                                    const string str = xUhfFreq1DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial1WaitingForFeedback, 1);
                                }
                                else if (_xuhfCockpitFreq1DialPos > desiredPosition1)
                                {
                                    const string str = xUhfFreq1DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _xuhfDial2WaitingForFeedback) == 0)
                        {
                            lock (_xlockUhfDialsObject2)
                            {
                                if (_xuhfCockpitFreq2DialPos != desiredPosition2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                }
                                if (_xuhfCockpitFreq2DialPos < desiredPosition2)
                                {
                                    const string str = xUhfFreq2DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial2WaitingForFeedback, 1);
                                }
                                else if (_xuhfCockpitFreq2DialPos > desiredPosition2)
                                {
                                    const string str = xUhfFreq2DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _xuhfDial3WaitingForFeedback) == 0)
                        {
                            lock (_xlockUhfDialsObject3)
                            {
                                if (_xuhfCockpitFreq3DialPos != desiredPosition3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                }
                                if (_xuhfCockpitFreq3DialPos < desiredPosition3)
                                {
                                    const string str = xUhfFreq3DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial3WaitingForFeedback, 1);
                                }
                                else if (_xuhfCockpitFreq3DialPos > desiredPosition3)
                                {
                                    const string str = xUhfFreq3DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial3WaitingForFeedback, 1);
                                }
                                Reset(ref dial3Timeout);
                            }
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _xuhfDial4WaitingForFeedback) == 0)
                        {
                            lock (_xlockUhfDialsObject4)
                            {
                                if (_xuhfCockpitFreq4DialPos != desiredPosition4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                }
                                if (_xuhfCockpitFreq4DialPos < desiredPosition4)
                                {
                                    const string str = xUhfFreq4DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial4WaitingForFeedback, 1);
                                }
                                else if (_xuhfCockpitFreq4DialPos > desiredPosition4)
                                {
                                    const string str = xUhfFreq4DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial4WaitingForFeedback, 1);
                                }
                                Reset(ref dial4Timeout);
                            }
                        }
                        else
                        {
                            dial4OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _xuhfDial5WaitingForFeedback) == 0)
                        {
                            lock (_xlockUhfDialsObject5)
                            {
                                if (_xuhfCockpitFreq5DialPos != desiredPosition5)
                                {
                                    dial5OkTime = DateTime.Now.Ticks;
                                }
                                if (_xuhfCockpitFreq5DialPos < desiredPosition5)
                                {
                                    const string str = xUhfFreq5DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial5SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial5WaitingForFeedback, 1);
                                }
                                else if (_xuhfCockpitFreq5DialPos > desiredPosition5)
                                {
                                    const string str = xUhfFreq5DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial5SendCount++;
                                    Interlocked.Exchange(ref _xuhfDial5WaitingForFeedback, 1);
                                }
                                Reset(ref dial5Timeout);
                            }
                        }
                        else
                        {
                            dial5OkTime = DateTime.Now.Ticks;
                        }
                        if (dial1SendCount > 3 || dial2SendCount > 10 || dial3SendCount > 10 || dial4SendCount > 10 || dial5SendCount > 5)
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
                    while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime) || IsTooShort(dial5OkTime));
                    SwapCockpitStandbyFrequencyxUhf();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.LogError(56453, ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _xuhfThreadNowSynching, 0);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
        }

        private void SendVhfFmToDCSBIOS()
        {
            if (VhfFmNowSyncing())
            {
                return;
            }
            SaveCockpitFrequencyVhfFm();
            var frequencyAsString = (_vhfFmBigFrequencyStandby + "." + _vhfFmSmallFrequencyStandby.ToString().PadLeft(3, '0'));
            //Frequency selector 1      VHFFM_FREQ1
            //      " 3" " 4" " 5" " 6" " 7" THESE ARE NOT USED IN FM RANGE ----> " 8" " 9" "10" "11" "12" "13" "14" "15"
            //Pos     0    1    2    3    4                                         5    6    7    8    9   10   11   12

            //Frequency selector 2      VHFFM_FREQ2
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3      VHFFM_FREQ3
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 4      VHFFM_FREQ4
            //      "00" "25" "50" "75"
            //Pos     0    1    2    3


            var desiredPositionDial1 = 0;
            var desiredPositionDial2 = 0;
            var desiredPositionDial3 = 0;
            var desiredPositionDial4 = 0;

            if (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture) == 2)
            {
                //30.025
                //#1 = 3  (position = value - 3)
                //#2 = 0   (position = value)
                //#3 = 0   (position = value)
                //#4 = 25
                desiredPositionDial1 = int.Parse(frequencyAsString.Substring(0, 1)) - 3;
                desiredPositionDial2 = int.Parse(frequencyAsString.Substring(1, 1));
                desiredPositionDial3 = int.Parse(frequencyAsString.Substring(3, 1));
                var tmpPosition = int.Parse(frequencyAsString.Substring(4, 2));
                switch (tmpPosition)
                {
                    case 0:
                        {
                            desiredPositionDial4 = 0;
                            break;
                        }
                    case 25:
                        {
                            desiredPositionDial4 = 1;
                            break;
                        }
                    case 50:
                        {
                            desiredPositionDial4 = 2;
                            break;
                        }
                    case 75:
                        {
                            desiredPositionDial4 = 3;
                            break;
                        }
                }
            }
            else
            {
                //151.95
                //This is a quick and dirty fix. We should not be here when dealing with VHF FM because the range is 30.000 to 76.000 MHz.
                //Set freq to 45.000 MHz (sort of an reset)

                desiredPositionDial1 = 1;//(4)
                desiredPositionDial2 = 5;
                desiredPositionDial3 = 0;
                desiredPositionDial4 = 0;
            }

            _vhfFmSyncThread?.Abort();
            _vhfFmSyncThread = new Thread(() => VhfFmSynchThreadMethod(desiredPositionDial1, desiredPositionDial2, desiredPositionDial3, desiredPositionDial4));
            _vhfFmSyncThread.Start();
        }

        private void VhfFmSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3, int frequencyDial4)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _vhfFmThreadNowSynching, 1);
                    var dial1Timeout = DateTime.Now.Ticks;
                    var dial2Timeout = DateTime.Now.Ticks;
                    var dial3Timeout = DateTime.Now.Ticks;
                    var dial4Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    long dial3OkTime = 0;
                    long dial4OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    var dial3SendCount = 0;
                    var dial4SendCount = 0;


                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF FM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF FM dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 2");
                        }
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "VHF FM dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 3");
                        }
                        if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "VHF FM dial4Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 4");
                        }
                        if (Interlocked.Read(ref _vhfFmDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject1)
                            {
                                if (_vhfFmCockpitFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_vhfFmCockpitFreq1DialPos is " + _vhfFmCockpitFreq1DialPos + " and should be " + desiredPositionDial1);
                                    var str = "";//VhfFmFreq1DialCommand + GetCommandDirectionForVhfDial1(desiredPositionDial1, _vhfFmCockpitFreq1DialPos);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfFmDial2WaitingForFeedback) == 0)
                        {
                            // Common.DebugP("b");
                            lock (_lockVhfFmDialsObject2)
                            {
                                if (_vhfFmCockpitFreq2DialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    var str = "";//VhfFmFreq2DialCommand + GetCommandDirectionForVhfDial23(desiredPositionDial2, _vhfFmCockpitFreq2DialPos);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfFmDial3WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject3)
                            {
                                if (_vhfFmCockpitFreq3DialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    var str = "";//VhfFmFreq3DialCommand + GetCommandDirectionForVhfDial23(desiredPositionDial3, _vhfFmCockpitFreq3DialPos);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 1);
                                }
                            }
                            Reset(ref dial3Timeout);
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfFmDial4WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject4)
                            {
                                //      "00" "25" "50" "75", only "00" and "50" used.
                                //Pos     0    1    2    3
                                if (_vhfFmCockpitFreq4DialPos < frequencyDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    const string str = VhfFmFreq4DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 1);
                                }
                                else if (_vhfFmCockpitFreq4DialPos > frequencyDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    const string str = VhfFmFreq4DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 1);
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
                    SwapCockpitStandbyFrequencyVhfFm();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.LogError(56463, ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _vhfFmThreadNowSynching, 0);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
        }

        private void SendILSToDCSBIOS()
        {
            if (IlsNowSyncing())
            {
                return;
            }
            SaveCockpitFrequencyIls();
            var frequency = double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay);
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);

            //Frequency selector 1   
            //      "108" "109" "110" "111"
            //         0     1     2     3

            //Frequency selector 2   
            //        "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            //          0    1    2    3    4    5    6    7    8    9

            var freqDial1 = 0;
            var freqDial2 = 0;

            //108.95
            //#1 = 0
            //#2 = 9
            freqDial1 = GetILSDialPosForFrequency(1, int.Parse(frequencyAsString.Substring(0, 3)));
            freqDial2 = GetILSDialPosForFrequency(2, int.Parse(frequencyAsString.Substring(4, 2)));
            //#1

            _ilsSyncThread?.Abort();
            _ilsSyncThread = new Thread(() => ILSSynchThreadMethod(freqDial1, freqDial2));
            _ilsSyncThread.Start();


        }

        private void ILSSynchThreadMethod(int position1, int position2)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _ilsThreadNowSynching, 1);

                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;

                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "ILS dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for ILS 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "ILS dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for ILS 2");
                        }
                        if (Interlocked.Read(ref _ilsDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockIlsDialsObject1)
                            {
                                if (_ilsCockpitFreq1DialPos < position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    const string str = ILSFreq1DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitFreq1DialPos > position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    const string str = ILSFreq1DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _ilsDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockIlsDialsObject2)
                            {

                                if (_ilsCockpitFreq2DialPos < position2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    const string str = ILSFreq2DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitFreq2DialPos > position2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    const string str = ILSFreq2DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }
                        if (dial1SendCount > 12 || dial2SendCount > 10)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    } while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime));
                    SwapCockpitStandbyFrequencyIls();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.LogError(56473, ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _ilsThreadNowSynching, 0);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
        }

        private void SendTacanToDCSBIOS()
        {
            if (TacanNowSyncing())
            {
                return;
            }
            SaveCockpitFrequencyTacan();
            //TACAN  00X/Y --> 129X/Y
            //
            //Frequency selector 1      LEFT
            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            //Frequency selector 2      MIDDLE
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3      RIGHT
            //X=0 / Y=1

            //120X
            //#1 = 12  (position = value)
            //#2 = 0   (position = value)
            //#3 = 1   (position = value)

            _tacanSyncThread?.Abort();
            _tacanSyncThread = new Thread(() => TacanSynchThreadMethod(_tacanBigFrequencyStandby, _tacanSmallFrequencyStandby, _tacanXYStandby));
            _tacanSyncThread.Start();
        }

        private void TacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _tacanThreadNowSynching, 1);

                    const string inc = "INC\n";
                    const string dec = "DEC\n";
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial3Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    long dial3OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    var dial3SendCount = 0;


                    do
                    {

                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "TACAN dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 1");
                        }

                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "TACAN dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 2");
                        }

                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "TACAN dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 3");
                        }

                        if (Interlocked.Read(ref _tacanDial1WaitingForFeedback) == 0)
                        {

                            lock (_lockTacanDialsObject1)
                            {
                                if (_tacanCockpitFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_tacanCockpitFreq1DialPos is " + _tacanCockpitFreq1DialPos + " and should be " + desiredPositionDial1);
                                    var str = TacanFreq1DialCommand + (_tacanCockpitFreq1DialPos < desiredPositionDial1 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _tacanDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _tacanDial2WaitingForFeedback) == 0)
                        {
                            // Common.DebugP("b");
                            lock (_lockTacanDialsObject2)
                            {
                                if (_tacanCockpitFreq2DialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;

                                    var str = TacanFreq2DialCommand + (_tacanCockpitFreq2DialPos < desiredPositionDial2 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _tacanDial3WaitingForFeedback) == 0)
                        {

                            lock (_lockTacanDialsObject3)
                            {
                                if (_tacanCockpitFreq3DialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;

                                    var str = TacanFreq3DialCommand + (_tacanCockpitFreq3DialPos < desiredPositionDial3 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 1);
                                }
                            }
                            Reset(ref dial3Timeout);
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (dial1SendCount > 12 || dial2SendCount > 10 || dial3SendCount > 2)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS


                    }
                    while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime));
                    SwapCockpitStandbyFrequencyTacan();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.LogError(56873, ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _tacanThreadNowSynching, 0);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
        }

        private void ShowFrequenciesOnPanel()
        {
            lock (_lockShowFrequenciesOnPanelObject)
            {
                if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                {
                    return;
                }
                CheckFrequenciesForValidity();
                if (!FirstReportHasBeenRead)
                {
                    return;
                }
                var bytes = new byte[21];
                bytes[0] = 0x0;

                switch (_currentUpperRadioMode)
                {
                    case CurrentF14RadioMode.UHF:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_uhfCockpitMode != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_uhfCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    var frequencyAsString = GetUHFCockpitFrequencyAsString();
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, (double)_uhfBigFrequencyStandby + (((double)_uhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF14RadioMode.VUHF:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_xuhfCockpitFunction, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_xuhfCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_xuhfCockpitFunction != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_xuhfCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                //Frequency selector 1     
                                //     //"2"  "3"  "A"
                                //Pos     0    1    2

                                //Frequency selector 2      
                                //0 1 2 3 4 5 6 7 8 9

                                //Frequency selector 3
                                //0 1 2 3 4 5 6 7 8 9


                                //Frequency selector 4
                                //0 1 2 3 4 5 6 7 8 9

                                //Frequency selector 5
                                //      "00" "25" "50" "75", only "00" and "50" used.
                                //Pos     0    1    2    3

                                //251.75
                                if (_xuhfCockpitFunction == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    var frequencyAsString = "";
                                    lock (_xlockUhfDialsObject1)
                                    {
                                        frequencyAsString = GetUhfDialFrequencyForPosition(1, _xuhfCockpitFreq1DialPos);
                                    }

                                    lock (_xlockUhfDialsObject2)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(2, _xuhfCockpitFreq2DialPos);
                                    }

                                    lock (_xlockUhfDialsObject3)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(3, _xuhfCockpitFreq3DialPos);
                                    }

                                    frequencyAsString = frequencyAsString + ".";
                                    lock (_xlockUhfDialsObject4)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(4, _xuhfCockpitFreq4DialPos);
                                    }

                                    lock (_xlockUhfDialsObject5)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(5, _xuhfCockpitFreq5DialPos);
                                    }

                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _xuhfBigFrequencyStandby + _xuhfSmallFrequencyStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF14RadioMode.TACAN:
                        {
                            //TACAN  00X/Y --> 129X/Y
                            //
                            //Frequency selector 1      LEFT
                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                            //Frequency selector 2      MIDDLE
                            //0 1 2 3 4 5 6 7 8 9

                            //Frequency selector 3      RIGHT
                            //X=0 / Y=1
                            var frequencyAsString = "";
                            lock (_lockTacanDialsObject1)
                            {
                                lock (_lockTacanDialsObject2)
                                {
                                    frequencyAsString = _tacanCockpitFreq1DialPos.ToString() + _tacanCockpitFreq2DialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockTacanDialsObject3)
                            {
                                frequencyAsString = frequencyAsString + _tacanCockpitFreq3DialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanBigFrequencyStandby.ToString() + _tacanSmallFrequencyStandby.ToString() + "." + _tacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                    case CurrentF14RadioMode.KY28:
                        {
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentF14RadioMode.UHF:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_uhfCockpitMode != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_uhfCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    var frequencyAsString = GetUHFCockpitFrequencyAsString();
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, (double)_uhfBigFrequencyStandby + (((double)_uhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF14RadioMode.VUHF:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_xuhfCockpitFunction, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_xuhfCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_xuhfCockpitFunction != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_xuhfCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_xuhfCockpitFunction == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    var frequencyAsString = "";
                                    lock (_xlockUhfDialsObject1)
                                    {
                                        frequencyAsString = GetUhfDialFrequencyForPosition(1, _xuhfCockpitFreq1DialPos);
                                    }

                                    lock (_xlockUhfDialsObject2)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(2, _xuhfCockpitFreq2DialPos);
                                    }

                                    lock (_xlockUhfDialsObject3)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(3, _xuhfCockpitFreq3DialPos);
                                    }

                                    frequencyAsString = frequencyAsString + ".";
                                    lock (_xlockUhfDialsObject4)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(4, _xuhfCockpitFreq4DialPos);
                                    }

                                    lock (_xlockUhfDialsObject5)
                                    {

                                        frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(5, _xuhfCockpitFreq5DialPos);
                                    }

                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _xuhfBigFrequencyStandby + _xuhfSmallFrequencyStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF14RadioMode.TACAN:
                        {
                            var frequencyAsString = "";
                            lock (_lockTacanDialsObject1)
                            {
                                lock (_lockTacanDialsObject2)
                                {
                                    frequencyAsString = _tacanCockpitFreq1DialPos.ToString() + _tacanCockpitFreq2DialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockTacanDialsObject3)
                            {
                                frequencyAsString = frequencyAsString + _tacanCockpitFreq3DialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanBigFrequencyStandby.ToString() + _tacanSmallFrequencyStandby.ToString() + "." + _tacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                    case CurrentF14RadioMode.KY28:
                        {
                            break;
                        }
                }
                SendLCDData(bytes);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
        }

        private string GetUHFCockpitFrequencyAsString()
        {
            var frequencyAsString = "";
            lock (_lockUhfBigFreqObject1)
            {
                lock (_lockUhfDial3FreqObject2)
                {
                    lock (_lockUhfDial4FreqObject2)
                    {
                        Console.WriteLine("GetUHFCockpitFrequencyAsString() BIG : " + _uhfCockpitBigFrequency);
                        Console.WriteLine("GetUHFCockpitFrequencyAsString() DIAL 3 : " + _uhfCockpitDial3Frequency);
                        Console.WriteLine("GetUHFCockpitFrequencyAsString() DIAL 4 : " + _uhfCockpitDial4Frequency);
                        frequencyAsString = _uhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
                        frequencyAsString = frequencyAsString + ".";
                        frequencyAsString = frequencyAsString + _uhfCockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString = frequencyAsString + _uhfCockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2,'0');
                        //225.000 7 characters
                    }
                }
            }
            return frequencyAsString;
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            if (SkipCurrentFrequencyChange())
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var RadioPanelKnobF14 = (RadioPanelKnobF14B)o;
                if (RadioPanelKnobF14.IsOn)
                {
                    switch (RadioPanelKnobF14.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_uhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(399))
                                                {
                                                    _uhfBigFrequencyStandby = 225;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_xuhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFunctionIncrease);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _xuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfPresetIncrease);
                                                }
                                                else if (_xuhfBigFrequencyStandby.Equals(399.00))
                                                {
                                                    //225-399
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _xuhfBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanBigFrequencyStandby >= 12)
                                            {
                                                _tacanBigFrequencyStandby = 12;
                                                break;
                                            }
                                            _tacanBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_uhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    //DCSBIOS.Send(UhfPresetDecrease);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(225))
                                                {
                                                    _uhfBigFrequencyStandby = 399;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_xuhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFunctionDecrease);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _xuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfPresetDecrease);
                                                }
                                                else if (_xuhfBigFrequencyStandby.Equals(225.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _xuhfBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanBigFrequencyStandby <= 0)
                                            {
                                                _tacanBigFrequencyStandby = 0;
                                                break;
                                            }
                                            _tacanBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_uhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                UHFSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_xuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFreqModeIncrease);
                                                }
                                            }
                                            else
                                            {
                                                xUHFSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanSmallFrequencyStandby >= 9)
                                            {
                                                _tacanSmallFrequencyStandby = 9;
                                                _tacanXYStandby = 1;
                                                break;
                                            }
                                            _tacanSmallFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_uhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                UHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_xuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFreqModeDecrease);
                                                }
                                            }
                                            else
                                            {
                                                xUHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanSmallFrequencyStandby <= 0)
                                            {
                                                _tacanSmallFrequencyStandby = 0;
                                                _tacanXYStandby = 0;
                                                break;
                                            }
                                            _tacanSmallFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_uhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                }
                                                else if (!_lowerButtonPressed && _uhfBigFrequencyStandby.Equals(399))
                                                {
                                                    _uhfBigFrequencyStandby = 225;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby += 1;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_xuhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFunctionIncrease);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _xuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    //225-399
                                                    DCSBIOS.Send(xUhfPresetIncrease);
                                                }
                                                else if (_xuhfBigFrequencyStandby.Equals(399.00))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _xuhfBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanBigFrequencyStandby >= 12)
                                            {
                                                _tacanBigFrequencyStandby = 12;
                                                break;
                                            }
                                            _tacanBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_uhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_DECREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(225))
                                                {
                                                    _uhfBigFrequencyStandby = 399;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby -= 1;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_xuhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFunctionDecrease);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _xuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfPresetDecrease);
                                                }
                                                else if (_xuhfBigFrequencyStandby.Equals(225.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _xuhfBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanBigFrequencyStandby <= 0)
                                            {
                                                _tacanBigFrequencyStandby = 0;
                                                break;
                                            }
                                            _tacanBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_uhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                UHFSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_xuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFreqModeIncrease);
                                                }
                                            }
                                            else
                                            {
                                                xUHFSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanSmallFrequencyStandby >= 9)
                                            {
                                                _tacanSmallFrequencyStandby = 9;
                                                _tacanXYStandby = 1;
                                                break;
                                            }
                                            _tacanSmallFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF14RadioMode.UHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_uhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                UHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.VUHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_xuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(xUhfFreqModeDecrease);
                                                }
                                            }
                                            else
                                            {
                                                xUHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_tacanSmallFrequencyStandby <= 0)
                                            {
                                                _tacanSmallFrequencyStandby = 0;
                                                _tacanXYStandby = 0;
                                                break;
                                            }
                                            _tacanSmallFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.KY28:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            ShowFrequenciesOnPanel();
        }

        private void VHFFMSmallFrequencyStandbyAdjust(bool increase)
        {
            if (increase)
            {
                _vhfFmSmallFrequencyStandby += 25;
            }
            else
            {
                if ((int)(_vhfFmSmallFrequencyStandby - 25) < 0)
                {
                    _vhfFmSmallFrequencyStandby = 0;
                }
                else
                {
                    _vhfFmSmallFrequencyStandby -= 25;
                }
            }
        }

        private void UHFSmallFrequencyStandbyAdjust(bool increase)
        {
            _skipUhfSmallFreqChange++;
            if (_skipUhfSmallFreqChange < 2)
            {
                return;
            }
            _skipUhfSmallFreqChange = 0;
            var tmp = _uhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3,'0');
            if (increase)
            {
                _uhfSmallFrequencyStandby += 25;
            }
            else
            {
                if (_uhfSmallFrequencyStandby == 0)
                {
                    _uhfSmallFrequencyStandby = 975;
                }
                else
                {
                    _uhfSmallFrequencyStandby -= 25;
                }
            }

            if (_uhfSmallFrequencyStandby > 975)
            {
                _uhfSmallFrequencyStandby = 0;
            }
        }

        private void xUHFSmallFrequencyStandbyAdjust(bool increase)
        {
            var tmp = _xuhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture);
            if (increase)
            {
                /*
                 * "0.1"
                 * "0.12"
                 * "0.15"
                 * "0.17"
                 * "0.97"
                 */
                if (tmp.Length == 4)
                {
                    if (tmp.EndsWith("0"))
                    {
                        _xuhfSmallFrequencyStandby += 0.02;
                    }
                    else if (tmp.EndsWith("2"))
                    {
                        _xuhfSmallFrequencyStandby += 0.03;
                    }
                    else if (tmp.EndsWith("5"))
                    {
                        _xuhfSmallFrequencyStandby += 0.02;
                    }
                    else if (tmp.EndsWith("7"))
                    {
                        _xuhfSmallFrequencyStandby += 0.03;
                    }
                }
                else
                {
                    /*
                     * Zero assumed
                     * e.g. 0.10
                     *         ^
                     */
                    _xuhfSmallFrequencyStandby += 0.02;
                }
            }
            else
            {
                if (tmp.Length == 4)
                {
                    if (tmp.EndsWith("0"))
                    {
                        _xuhfSmallFrequencyStandby -= 0.03;
                    }
                    else if (tmp.EndsWith("2"))
                    {
                        _xuhfSmallFrequencyStandby -= 0.02;
                    }
                    else if (tmp.EndsWith("5"))
                    {
                        _xuhfSmallFrequencyStandby -= 0.03;
                    }
                    else if (tmp.EndsWith("7"))
                    {
                        _xuhfSmallFrequencyStandby -= 0.02;
                    }
                }
                else
                {
                    /*
                     * Zero assumed
                     * e.g. 0.10
                     */
                    _xuhfSmallFrequencyStandby -= 0.03;
                }
            }

            if (_xuhfSmallFrequencyStandby < 0)
            {
                _xuhfSmallFrequencyStandby = 0.97;
            }
            else if (_xuhfSmallFrequencyStandby > 0.97)
            {
                _xuhfSmallFrequencyStandby = 0.0;
            }
        }

        private void CheckFrequenciesForValidity()
        {
            //Crude fix if any freqs are outside the valid boundaries

            //UHF
            //225.00 - 399.975
            if (_uhfBigFrequencyStandby < 225)
            {
                _uhfBigFrequencyStandby = 399;
            }
            if (_uhfBigFrequencyStandby > 399)
            {
                _uhfBigFrequencyStandby = 225;
            }

            //VHF FM
            //30.000 - 76.000Mhz
            if (_vhfFmBigFrequencyStandby < 30)
            {
                _vhfFmBigFrequencyStandby = 30;
            }
            if (_vhfFmBigFrequencyStandby > 76)
            {
                _vhfFmBigFrequencyStandby = 76;
            }
            if (_vhfFmBigFrequencyStandby >= 76 && _vhfFmSmallFrequencyStandby > 0)
            {
                _vhfFmBigFrequencyStandby = 76;
                _vhfFmSmallFrequencyStandby = 0;
            }

            //UHF
            //225.000 - 399.975 MHz
            if (_xuhfBigFrequencyStandby < 225)
            {
                _xuhfBigFrequencyStandby = 225;
            }
            if (_xuhfBigFrequencyStandby > 399)
            {
                _xuhfBigFrequencyStandby = 399;
            }

            //ILS
            //108.10 - 111.95
            if (_ilsBigFrequencyStandby < 108)
            {
                _ilsBigFrequencyStandby = 108;
            }
            if (_ilsBigFrequencyStandby > 111)
            {
                _ilsBigFrequencyStandby = 111;
            }

            //TACAN
            //00X/Y - 129X/Y
            if (_tacanBigFrequencyStandby < 0)
            {
                _tacanBigFrequencyStandby = 0;
            }
            if (_tacanBigFrequencyStandby > 12)
            {
                _tacanBigFrequencyStandby = 12;
            }
            if (_tacanSmallFrequencyStandby < 0)
            {
                _tacanSmallFrequencyStandby = 0;
            }
            if (_tacanSmallFrequencyStandby > 9)
            {
                _tacanSmallFrequencyStandby = 9;
            }
            if (_tacanXYStandby < 0)
            {
                _tacanXYStandby = 0;
            }
            if (_tacanXYStandby > 1)
            {
                _tacanXYStandby = 1;
            }
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobF14B)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsF14B.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_VUHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.VUHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_KY28:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.KY28;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_NOUSE:
                        case RadioPanelPZ69KnobsF14B.UPPER_NOUSE2:
                        case RadioPanelPZ69KnobsF14B.UPPER_NOUSE3:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.NOUSE;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_VUHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.VUHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_KY28:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.KY28;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_NOUSE:
                        case RadioPanelPZ69KnobsF14B.LOWER_NOUSE2:
                        case RadioPanelPZ69KnobsF14B.LOWER_NOUSE3:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.NOUSE;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH:
                            {
                                _upperButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH);
                                    }
                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH:
                            {
                                _lowerButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH);
                                    }
                                    _lowerButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                    }


                }
                AdjustFrequency(hashSet);
            }
        }

        public sealed override void Startup()
        {
            try
            {
                StartupBase("F-14B");

                //UHF
                _uhfDcsbiosOutputChannelFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF1_FREQ_MODE");
                _uhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF1_PRESETS");
                _uhfDcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF_HIGH_FREQ");
                _uhfDcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF_LOW1_FREQ");
                _uhfDcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF_LOW2_FREQ");
                _uhfDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF1_FUNCTION");

                //UHF
                /*_xuhfDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_110MHZ_SEL");
                _xuhfDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_10MHZ_SEL");
                _xuhfDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_1MHZ_SEL");
                _xuhfDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT1MHZ_SEL");
                _xuhfDcsbiosOutputFreqDial5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT25_SEL");
                _xuhfDcsbiosOutputFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_MODE");
                _xuhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_SEL");
                _xuhfDcsbiosOutputFunction = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FUNCTION");*/
                /*
                                //VHF FM
                                _vhfFmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ1");
                                //_vhfFmDcsbiosOutputFreqDial1.Debug = true;
                                _vhfFmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ2");
                                //_vhfFmDcsbiosOutputFreqDial2.Debug = true;
                                _vhfFmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ3");
                                //_vhfFmDcsbiosOutputFreqDial3.Debug = true;
                                _vhfFmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ4");
                                //_vhfFmDcsbiosOutputFreqDial4.Debug = true;
                                _vhfFmDcsbiosOutputFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQEMER");
                                _vhfFmDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_PRESET");
                                _vhfFmDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_MODE");
                                */

                //ILS
                //_ilsDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ILS_MHZ");
                //_ilsDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ILS_KHZ");

                //TACAN
                //_tacanDcsbiosOutputFreqChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("TACAN_CHANNEL");
                //DCSBIOSStringListenerHandler.AddAddress(_tacanDcsbiosOutputFreqChannel.Address, 4, this); //_tacanDcsbiosOutputFreqChannel.MaxLength does not work. Bad JSON format.

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69F14B.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
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

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 3)
            {
                Array.Copy(NewRadioPanelValue, OldRadioPanelValue, 3);
                Array.Copy(report.Data, NewRadioPanelValue, 3);
                var hashSet = GetHashSetOfChangedKnobs(OldRadioPanelValue, NewRadioPanelValue);
                PZ69KnobChanged(hashSet);
                OnSwitchesChanged(hashSet);
                FirstReportHasBeenRead = true;
                /*if (Common.Debug && 1 == 2)
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
                            var knob = (RadioPanelKnobF14)radioPanelKnob;

                        }
                    }
                }
                Common.DebugP("\r\nDone!\r\n");*/
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
            }
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();


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
            return result;
        }

        private void CreateRadioKnobs()
        {
            _radioPanelKnobs = RadioPanelKnobF14B.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobF14B radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private string GetVhfAmDialFrequencyForPosition(int dial, uint position)
        {

            //Frequency selector 1      VHFAM_FREQ1
            //      " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            //Frequency selector 2      VHFAM_FREQ2
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3      VHFAM_FREQ3
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 4      VHFAM_FREQ4
            //      "00" "25" "50" "75", only 0 2 5 7 used.
            //Pos     0    1    2    3
            switch (dial)
            {
                case 1:
                    {
                        switch (position)
                        {
                            case 0:
                                {
                                    return "3";
                                }
                            case 1:
                                {
                                    return "4";
                                }
                            case 2:
                                {
                                    return "5";
                                }
                            case 3:
                                {
                                    return "6";
                                }
                            case 4:
                                {
                                    return "7";
                                }
                            case 5:
                                {
                                    return "8";
                                }
                            case 6:
                                {
                                    return "9";
                                }
                            case 7:
                                {
                                    return "10";
                                }
                            case 8:
                                {
                                    return "11";
                                }
                            case 9:
                                {
                                    return "12";
                                }
                            case 10:
                                {
                                    return "13";
                                }
                            case 11:
                                {
                                    return "14";
                                }
                            case 12:
                                {
                                    return "15";
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        return position.ToString();
                    }
                case 3:
                    {
                        return position.ToString();
                    }
                case 4:
                    {
                        switch (position)
                        {
                            //      "00" "25" "50" "75", 0 2 5 7 used.
                            //Pos     0    1    2    3
                            case 0:
                                {
                                    return "0";
                                }
                            case 1:
                                {
                                    return "2";
                                }
                            case 2:
                                {
                                    return "5";
                                }
                            case 3:
                                {
                                    return "7";
                                }
                        }
                    }
                    break;
            }
            return "";
        }

        private string GetUhfDialFrequencyForPosition(int dial, uint position)
        {
            //Frequency selector 1     
            //     //"2"  "3"  "A"
            //Pos     0    1    2

            //Frequency selector 2      
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3
            //0 1 2 3 4 5 6 7 8 9


            //Frequency selector 4
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 5
            //      "00" "25" "50" "75", only "00" and "50" used.
            //Pos     0    1    2    3
            switch (dial)
            {
                case 1:
                    {
                        switch (position)
                        {
                            case 0:
                                {
                                    return "2";
                                }
                            case 1:
                                {
                                    return "3";
                                }
                            case 2:
                                {
                                    //throw new NotImplementedException("check how A should be treated.");
                                    return "0";//should be "A"
                                }
                        }
                        break;
                    }
                case 2:
                case 3:
                case 4:
                    {
                        return position.ToString();
                    }
                case 5:
                    {
                        switch (position)
                        {
                            //      "00" "25" "50" "75", only "00" and "50" used.
                            //Pos     0    1    2    3
                            case 0:
                                {
                                    return "0";
                                }
                            case 1:
                                {
                                    return "2";
                                }
                            case 2:
                                {
                                    return "5";
                                }
                            case 3:
                                {
                                    return "7";
                                }
                        }
                    }
                    break;
            }
            return "";
        }

        private string GetVhfFmDialFrequencyForPosition(int dial, uint position)
        {

            //Frequency selector 1      VHFFM_FREQ1
            //      " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            //Frequency selector 2      VHFFM_FREQ2
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3      VHFFM_FREQ3
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 4      VHFFM_FREQ4
            //      "00" "25" "50" "75", 0 2 5 7 used.
            //Pos     0    1    2    3
            switch (dial)
            {
                case 1:
                    {
                        switch (position)
                        {
                            case 0:
                                {
                                    return "3";
                                }
                            case 1:
                                {
                                    return "4";
                                }
                            case 2:
                                {
                                    return "5";
                                }
                            case 3:
                                {
                                    return "6";
                                }
                            case 4:
                                {
                                    return "7";
                                }
                            case 5:
                                {
                                    return "8";
                                }
                            case 6:
                                {
                                    return "9";
                                }
                            case 7:
                                {
                                    return "10";
                                }
                            case 8:
                                {
                                    return "11";
                                }
                            case 9:
                                {
                                    return "12";
                                }
                            case 10:
                                {
                                    return "13";
                                }
                            case 11:
                                {
                                    return "14";
                                }
                            case 12:
                                {
                                    return "15";
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        return position.ToString();
                    }
                case 3:
                    {
                        return position.ToString();
                    }
                case 4:
                    {
                        switch (position)
                        {
                            //      "00" "25" "50" "75"
                            //Pos     0    1    2    3
                            case 0:
                                {
                                    return "0";
                                }
                            case 1:
                                {
                                    return "25";
                                }
                            case 2:
                                {
                                    return "50";
                                }
                            case 3:
                                {
                                    return "75";
                                }
                        }
                    }
                    break;
            }
            return "";
        }

        private string GetILSDialFrequencyForPosition(int dial, uint position)
        {
            //1 Mhz   "108" "109" "110" "111"
            //           0     1     2     3
            //2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            //          0    1    2    3    4    5    6    7    8    9
            switch (dial)
            {
                case 1:
                    {
                        switch (position)
                        {
                            case 0:
                                {
                                    return "108";
                                }
                            case 1:
                                {
                                    return "109";
                                }
                            case 2:
                                {
                                    return "110";
                                }
                            case 3:
                                {
                                    return "111";
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        //2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                        //          0    1    2    3    4    5    6    7    8    9
                        switch (position)
                        {
                            case 0:
                                {
                                    return "10";
                                }
                            case 1:
                                {
                                    return "15";
                                }
                            case 2:
                                {
                                    return "30";
                                }
                            case 3:
                                {
                                    return "35";
                                }
                            case 4:
                                {
                                    return "50";
                                }
                            case 5:
                                {
                                    return "55";
                                }
                            case 6:
                                {
                                    return "70";
                                }
                            case 7:
                                {
                                    return "75";
                                }
                            case 8:
                                {
                                    return "90";
                                }
                            case 9:
                                {
                                    return "95";
                                }
                        }
                    }
                    break;
            }
            return "";
        }

        private int GetILSDialPosForFrequency(int dial, int freq)
        {
            //1 Mhz   "108" "109" "110" "111"
            //           0     1     2     3
            //2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            //          0    1    2    3    4    5    6    7    8    9
            switch (dial)
            {
                case 1:
                    {
                        switch (freq)
                        {
                            case 108:
                                {
                                    return 0;
                                }
                            case 109:
                                {
                                    return 1;
                                }
                            case 110:
                                {
                                    return 2;
                                }
                            case 111:
                                {
                                    return 3;
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        //2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                        //          0    1    2    3    4    5    6    7    8    9
                        switch (freq)
                        {
                            case 10:
                                {
                                    return 0;
                                }
                            case 15:
                                {
                                    return 1;
                                }
                            case 30:
                                {
                                    return 2;
                                }
                            case 35:
                                {
                                    return 3;
                                }
                            case 50:
                                {
                                    return 4;
                                }
                            case 55:
                                {
                                    return 5;
                                }
                            case 70:
                                {
                                    return 6;
                                }
                            case 75:
                                {
                                    return 7;
                                }
                            case 90:
                                {
                                    return 8;
                                }
                            case 95:
                                {
                                    return 9;
                                }
                        }
                    }
                    break;
            }
            return 0;
        }


        private void SaveCockpitFrequencyUhf()
        {
            lock (_lockUhfBigFreqObject1)
            {
                lock (_lockUhfDial3FreqObject2)
                {
                    lock (_lockUhfDial4FreqObject2)
                    {
                        _uhfSavedCockpitBigFrequency = _uhfCockpitBigFrequency;
                        _uhfSavedCockpitDial3Frequency = _uhfCockpitDial3Frequency;
                        _uhfSavedCockpitDial4Frequency = _uhfCockpitDial4Frequency;
                        Console.WriteLine("SAVED _uhfSavedCockpitBigFrequency = " + _uhfSavedCockpitBigFrequency);
                        Console.WriteLine("SAVED _uhfSavedCockpitDial3Frequency = " + _uhfSavedCockpitDial3Frequency);
                        Console.WriteLine("SAVED _uhfSavedCockpitDial4Frequency = " + _uhfSavedCockpitDial4Frequency);
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyUhf()
        {
            lock (_lockUhfBigFreqObject1)
            {
                lock (_lockUhfDial3FreqObject2)
                {
                    lock (_lockUhfDial4FreqObject2)
                    {
                        Console.WriteLine("SWAPPING _uhfSavedCockpitBigFrequency = " + _uhfSavedCockpitBigFrequency);
                        Console.WriteLine("SWAPPING _uhfSavedCockpitDial3Frequency = " + _uhfSavedCockpitDial3Frequency);
                        Console.WriteLine("SWAPPING _uhfSavedCockpitDial4Frequency = " + _uhfSavedCockpitDial4Frequency);
                        _uhfBigFrequencyStandby = _uhfSavedCockpitBigFrequency;
                        _uhfSmallFrequencyStandby = _uhfSavedCockpitDial3Frequency * 100 + _uhfSavedCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SaveCockpitFrequencyxUhf()
        {
            /*
             * Dial 1
             *      2   3   A
             * Pos  0   1   2
             * 
             * Dial 2
             * 0 - 9
             * 
             * Dial 3
             * 0 - 9
             * 
             * "."
             * 
             * Dial 4
             * 0 - 9
             * 
             * Dial 5
             * 00/50
             */
            try
            {
                var bigFrequencyAsString = "";
                var smallFrequencyAsString = "0.";
                lock (_xlockUhfDialsObject1)
                {
                    bigFrequencyAsString = GetUhfDialFrequencyForPosition(1, _xuhfCockpitFreq1DialPos);

                }
                lock (_xlockUhfDialsObject2)
                {
                    bigFrequencyAsString = bigFrequencyAsString + GetUhfDialFrequencyForPosition(2, _xuhfCockpitFreq2DialPos);

                }
                lock (_xlockUhfDialsObject3)
                {
                    bigFrequencyAsString = bigFrequencyAsString + GetUhfDialFrequencyForPosition(3, _xuhfCockpitFreq3DialPos);

                }
                lock (_xlockUhfDialsObject4)
                {
                    smallFrequencyAsString = smallFrequencyAsString + GetUhfDialFrequencyForPosition(4, _xuhfCockpitFreq4DialPos);
                }
                lock (_xlockUhfDialsObject5)
                {
                    smallFrequencyAsString = smallFrequencyAsString + GetUhfDialFrequencyForPosition(5, _xuhfCockpitFreq5DialPos);
                }


                _xuhfSavedCockpitBigFrequency = double.Parse(bigFrequencyAsString, NumberFormatInfoFullDisplay);
                _xuhfSavedCockpitSmallFrequency = double.Parse(smallFrequencyAsString, NumberFormatInfoFullDisplay);




            }
            catch (Exception ex)
            {
                Common.LogError(83244, ex, "SaveCockpitFrequencyUhf()");
                throw;
            }
        }

        private void SwapCockpitStandbyFrequencyxUhf()
        {
            _xuhfBigFrequencyStandby = _xuhfSavedCockpitBigFrequency;
            _xuhfSmallFrequencyStandby = _xuhfSavedCockpitSmallFrequency;
        }

        private void SaveCockpitFrequencyVhfFm()
        {
            /*
             * Dial 1
             *      3   4   5   6   7   8   9   10  11  12  13  14  15
             * Pos  0   1   2   3   4   5   6   7   8   9   10  11  12
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
            lock (_lockVhfFmDialsObject1)
            {
                lock (_lockVhfFmDialsObject2)
                {
                    lock (_lockVhfFmDialsObject3)
                    {
                        lock (_lockVhfFmDialsObject4)
                        {
                            uint dial4 = 0;
                            switch (_vhfFmCockpitFreq4DialPos)
                            {
                                case 0:
                                    {
                                        dial4 = 0;
                                        break;
                                    }
                                case 1:
                                    {
                                        dial4 = 25;
                                        break;
                                    }
                                case 2:
                                    {
                                        dial4 = 50;
                                        break;
                                    }
                                case 3:
                                    {
                                        dial4 = 75;
                                        break;
                                    }
                            }
                            _vhfFmSavedCockpitBigFrequency = uint.Parse((_vhfFmCockpitFreq1DialPos + 3).ToString() + _vhfFmCockpitFreq2DialPos.ToString(), NumberFormatInfoFullDisplay);
                            _vhfFmSavedCockpitSmallFrequency = uint.Parse((_vhfFmCockpitFreq3DialPos.ToString() + dial4).PadLeft(3, '0'), NumberFormatInfoFullDisplay);
                        }
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVhfFm()
        {
            _vhfFmBigFrequencyStandby = _vhfFmSavedCockpitBigFrequency;
            _vhfFmSmallFrequencyStandby = _vhfFmSavedCockpitSmallFrequency;
        }

        private void SaveCockpitFrequencyIls()
        {
            //Large dial 108-111 [step of 1]
            //Small dial 10-95 [step of 5]
            //"108" "109" "110" "111"
            //  0     1      2    3 
            //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            //  0    1    2    3    4    5    6    7    8   9
            lock (_lockIlsDialsObject1)
            {
                lock (_lockIlsDialsObject2)
                {
                    _ilsSavedCockpitBigFrequency = uint.Parse(GetILSDialFrequencyForPosition(1, _ilsCockpitFreq1DialPos).ToString());
                    _ilsSavedCockpitSmallFrequency = uint.Parse(GetILSDialFrequencyForPosition(2, _ilsCockpitFreq2DialPos).ToString());
                }
            }
        }

        private void SwapCockpitStandbyFrequencyIls()
        {
            _ilsBigFrequencyStandby = _ilsSavedCockpitBigFrequency;
            _ilsSmallFrequencyStandby = _ilsSavedCockpitSmallFrequency;
        }

        private void SaveCockpitFrequencyTacan()
        {
            /*TACAN*/
            //Large dial 0-12 [step of 1]
            //Small dial 0-9 [step of 1]
            //Last : X/Y [0,1]
            lock (_lockTacanDialsObject1)
            {
                lock (_lockTacanDialsObject2)
                {
                    lock (_lockTacanDialsObject3)
                    {
                        _tacanSavedCockpitBigFrequency = Convert.ToInt32(_tacanCockpitFreq1DialPos);
                        _tacanSavedCockpitSmallFrequency = Convert.ToInt32(_tacanCockpitFreq2DialPos);
                        _tacanSavedCockpitXY = Convert.ToInt32(_tacanCockpitFreq3DialPos);
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyTacan()
        {
            _tacanBigFrequencyStandby = _tacanSavedCockpitBigFrequency;
            _tacanSmallFrequencyStandby = _tacanSavedCockpitSmallFrequency;
            _tacanXYStandby = _tacanSavedCockpitXY;
        }

        private bool UhfPresetSelected()
        {
            return _uhfCockpitFreqMode == 0;
        }

        private bool VhfFmPresetSelected()
        {
            return _vhfFmCockpitFreqMode == 3;
        }

        private bool xUhfPresetSelected()
        {
            return _xuhfCockpitFreqMode == 1;
        }

        private bool VhfFmNowSyncing()
        {
            return Interlocked.Read(ref _vhfFmThreadNowSynching) > 0;
        }

        private bool IlsNowSyncing()
        {
            return Interlocked.Read(ref _ilsThreadNowSynching) > 0;
        }

        private bool TacanNowSyncing()
        {
            return Interlocked.Read(ref _tacanThreadNowSynching) > 0;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }
    }

}

