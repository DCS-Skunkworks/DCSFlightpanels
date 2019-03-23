using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69A10C : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private HashSet<RadioPanelKnobA10C> _radioPanelKnobs = new HashSet<RadioPanelKnobA10C>();
        private CurrentA10RadioMode _currentUpperRadioMode = CurrentA10RadioMode.UHF;
        private CurrentA10RadioMode _currentLowerRadioMode = CurrentA10RadioMode.UHF;

        private bool _upperButtonPressed = false;
        private bool _lowerButtonPressed = false;
        private bool _upperButtonPressedAndDialRotated = false;
        private bool _lowerButtonPressedAndDialRotated = false;
        private bool _ignoreUpperButtonOnce = true;
        private bool _ignoreLowerButtonOnce = true;

        /*A-10C AN/ARC-186(V) VHF AM Radio 1*/
        //Large dial 116-151 [step of 1]
        //Small dial 0.00-0.97 [step of x.x[0 2 5 7]
        private double _vhfAmBigFrequencyStandby = 116;
        private double _vhfAmSmallFrequencyStandby;
        private double _vhfAmSavedCockpitBigFrequency;
        private double _vhfAmSavedCockpitSmallFrequency;
        private readonly object _lockVhfAmDialsObject1 = new object();
        private readonly object _lockVhfAmDialsObject2 = new object();
        private readonly object _lockVhfAmDialsObject3 = new object();
        private readonly object _lockVhfAmDialsObject4 = new object();
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial4;
        private volatile uint _vhfAmCockpitFreq1DialPos = 1;
        private volatile uint _vhfAmCockpitFreq2DialPos = 1;
        private volatile uint _vhfAmCockpitFreq3DialPos = 1;
        private volatile uint _vhfAmCockpitFreq4DialPos = 1;
        private const string VHF_AM_PRESET_INCREASE = "VHFAM_PRESET INC\n";
        private const string VHF_AM_PRESET_DECREASE = "VHFAM_PRESET DEC\n";
        private const string VHF_AM_FREQ_MODE_INCREASE = "VHFAM_FREQEMER INC\n";
        private const string VHF_AM_FREQ_MODE_DECREASE = "VHFAM_FREQEMER DEC\n";
        private DCSBIOSOutput _vhfAmDcsbiosOutputChannelFreqMode;  // 3 = PRESET
        private DCSBIOSOutput _vhfAmDcsbiosOutputSelectedChannel;
        private volatile uint _vhfAmCockpitFreqMode = 0;
        private volatile uint _vhfAmCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _vhfAmChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _vhfAmFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string VHF_AM_MODE_INCREASE = "VHFAM_MODE INC\n";
        private const string VHF_AM_MODE_DECREASE = "VHFAM_MODE DEC\n";
        private DCSBIOSOutput _vhfAmDcsbiosOutputMode;  // VHFAM_MODE
        private volatile uint _vhfAmCockpitMode = 0; // OFF = 0
        private readonly ClickSpeedDetector _vhfAmModeClickSpeedDetector = new ClickSpeedDetector(8);
        private const string VHF_AM_SET_FREQ = "SET_VHF_AM ";

        /*A-10C AN/ARC-164 UHF Radio 2*/
        //Large dial 225-399 [step of 1]
        //Small dial 0.00-0.97 [step of 0 2 5 7]
        private double _uhfBigFrequencyStandby = 299;
        private double _uhfSmallFrequencyStandby;
        private double _uhfSavedCockpitBigFrequency;
        private double _uhfSavedCockpitSmallFrequency;
        private readonly object _lockUhfDialsObject1 = new object();
        private readonly object _lockUhfDialsObject2 = new object();
        private readonly object _lockUhfDialsObject3 = new object();
        private readonly object _lockUhfDialsObject4 = new object();
        private readonly object _lockUhfDialsObject5 = new object();
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial4;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial5;
        private volatile uint _uhfCockpitFreq1DialPos = 1;
        private volatile uint _uhfCockpitFreq2DialPos = 1;
        private volatile uint _uhfCockpitFreq3DialPos = 1;
        private volatile uint _uhfCockpitFreq4DialPos = 1;
        private volatile uint _uhfCockpitFreq5DialPos = 1;
        private const string UHF_PRESET_INCREASE = "UHF_PRESET_SEL INC\n";
        private const string UHF_PRESET_DECREASE = "UHF_PRESET_SEL DEC\n";
        private const string UHF_FREQ_MODE_INCREASE = "UHF_MODE INC\n";
        private const string UHF_FREQ_MODE_DECREASE = "UHF_MODE DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputFreqMode;  // 1 = PRESET
        private DCSBIOSOutput _uhfDcsbiosOutputSelectedChannel;
        private volatile uint _uhfCockpitFreqMode = 0;
        private volatile uint _uhfCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _uhfChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _uhfFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string UHF_FUNCTION_INCREASE = "UHF_FUNCTION INC\n";
        private const string UHF_FUNCTION_DECREASE = "UHF_FUNCTION DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputFunction;  // UHF_FUNCTION
        private volatile uint _uhfCockpitMode = 0;
        private readonly ClickSpeedDetector _uhfFunctionClickSpeedDetector = new ClickSpeedDetector(8);
        private const string UHF_SET_FREQ = "SET_UHF ";

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
        private const string VHF_FM_PRESET_INCREASE = "VHFFM_PRESET INC\n";
        private const string VHF_FM_PRESET_DECREASE = "VHFFM_PRESET DEC\n";
        private const string VHF_FM_FREQ_MODE_INCREASE = "VHFFM_FREQEMER INC\n";
        private const string VHF_FM_FREQ_MODE_DECREASE = "VHFFM_FREQEMER DEC\n";
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqMode;// 3 = PRESET
        private DCSBIOSOutput _vhfFmDcsbiosOutputSelectedChannel;
        private volatile uint _vhfFmCockpitFreqMode = 0;
        private volatile uint _vhfFmCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _vhfFmChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _vhfFmFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string VHF_FM_MODE_INCREASE = "VHFFM_MODE INC\n";
        private const string VHF_FM_MODE_DECREASE = "VHFFM_MODE DEC\n";
        private DCSBIOSOutput _vhfFmDcsbiosOutputMode;// VHFFM_MODE
        private volatile uint _vhfFmCockpitMode = 0;
        private readonly ClickSpeedDetector _vhfFmModeClickSpeedDetector = new ClickSpeedDetector(6);
        private const string VHF_FM_SET_FREQ = "SET_VHF_FM ";

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
        private const string ILS_FREQ1_DIAL_COMMAND = "ILS_MHZ ";
        private const string ILS_FREQ2_DIAL_COMMAND = "ILS_KHZ ";
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
        private const string TACAN_FREQ1_DIAL_COMMAND = "TACAN_10 ";
        private const string TACAN_FREQ2_DIAL_COMMAND = "TACAN_1 ";
        private const string TACAN_FREQ3_DIAL_COMMAND = "TACAN_XY ";
        private Thread _tacanSyncThread;
        private long _tacanThreadNowSynching;
        private long _tacanDial1WaitingForFeedback;
        private long _tacanDial2WaitingForFeedback;
        private long _tacanDial3WaitingForFeedback;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69A10C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69A10C()
        {
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
            //VHF AM
            if (e.Address == _vhfAmDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockVhfAmDialsObject1)
                {
                    var tmp = _vhfAmCockpitFreq1DialPos;
                    _vhfAmCockpitFreq1DialPos = _vhfAmDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _vhfAmCockpitFreq1DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vhfAmDcsbiosOutputFreqDial2.Address)
            {
                lock (_lockVhfAmDialsObject2)
                {
                    var tmp = _vhfAmCockpitFreq2DialPos;
                    _vhfAmCockpitFreq2DialPos = _vhfAmDcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                    if (tmp != _vhfAmCockpitFreq2DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vhfAmDcsbiosOutputFreqDial3.Address)
            {
                lock (_lockVhfAmDialsObject3)
                {
                    var tmp = _vhfAmCockpitFreq3DialPos;
                    _vhfAmCockpitFreq3DialPos = _vhfAmDcsbiosOutputFreqDial3.GetUIntValue(e.Data);
                    if (tmp != _vhfAmCockpitFreq3DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vhfAmDcsbiosOutputFreqDial4.Address)
            {
                lock (_lockVhfAmDialsObject4)
                {
                    var tmp = _vhfAmCockpitFreq4DialPos;
                    _vhfAmCockpitFreq4DialPos = _vhfAmDcsbiosOutputFreqDial4.GetUIntValue(e.Data);
                    if (tmp != _vhfAmCockpitFreq4DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vhfAmDcsbiosOutputChannelFreqMode.Address)
            {
                var tmp = _vhfAmCockpitFreqMode;
                _vhfAmCockpitFreqMode = _vhfAmDcsbiosOutputChannelFreqMode.GetUIntValue(e.Data);
                if (tmp != _vhfAmCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _vhfAmDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _vhfAmCockpitPresetChannel;
                _vhfAmCockpitPresetChannel = _vhfAmDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _vhfAmCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _vhfAmDcsbiosOutputMode.Address)
            {
                var tmp = _vhfAmCockpitMode;
                _vhfAmCockpitMode = _vhfAmDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _vhfAmCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            //UHF
            if (e.Address == _uhfDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockUhfDialsObject1)
                {
                    var tmp = _uhfCockpitFreq1DialPos;
                    _uhfCockpitFreq1DialPos = _uhfDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq1DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        Common.DebugP("_uhfCockpitFreq1DialPos Before : " + tmp + "  now: " + _uhfCockpitFreq1DialPos);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial2.Address)
            {
                lock (_lockUhfDialsObject2)
                {
                    var tmp = _uhfCockpitFreq2DialPos;
                    _uhfCockpitFreq2DialPos = _uhfDcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq2DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial3.Address)
            {
                lock (_lockUhfDialsObject3)
                {
                    var tmp = _uhfCockpitFreq3DialPos;
                    _uhfCockpitFreq3DialPos = _uhfDcsbiosOutputFreqDial3.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq3DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial4.Address)
            {
                lock (_lockUhfDialsObject4)
                {
                    var tmp = _uhfCockpitFreq4DialPos;
                    _uhfCockpitFreq4DialPos = _uhfDcsbiosOutputFreqDial4.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq4DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial5.Address)
            {
                lock (_lockUhfDialsObject5)
                {
                    var tmp = _uhfCockpitFreq5DialPos;
                    _uhfCockpitFreq5DialPos = _uhfDcsbiosOutputFreqDial5.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq5DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqMode.Address)
            {
                var tmp = _uhfCockpitFreqMode;
                _uhfCockpitFreqMode = _uhfDcsbiosOutputFreqMode.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _uhfDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _uhfCockpitPresetChannel;
                _uhfCockpitPresetChannel = _uhfDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _uhfCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _uhfDcsbiosOutputFunction.Address)
            {
                var tmp = _uhfCockpitMode;
                _uhfCockpitMode = _uhfDcsbiosOutputFunction.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputFreqMode.Address)
            {
                var tmp = _vhfFmCockpitFreqMode;
                _vhfFmCockpitFreqMode = _vhfFmDcsbiosOutputFreqMode.GetUIntValue(e.Data);
                if (tmp != _vhfFmCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _vhfFmCockpitPresetChannel;
                _vhfFmCockpitPresetChannel = _vhfFmDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _vhfFmCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _vhfFmDcsbiosOutputMode.Address)
            {
                var tmp = _vhfFmCockpitMode;
                _vhfFmCockpitMode = _vhfFmDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _vhfFmCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 0);
                    }
                }
            }

            //TACAN is set via String listener

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                //Common.DebugP("RadioPanelPZ69A10C Received DCSBIOS stringData : ->" + e.StringData + "<-");
                if (string.IsNullOrWhiteSpace(e.StringData))
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
                            Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        }

                        if ((changeCount & 4) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 0);
                            Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        }

                        if ((changeCount & 8) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 0);
                            Interlocked.Add(ref _doUpdatePanelLCD, 5);
                        }

                    }
                    catch (Exception)
                    {
                        //Common.LogError(123, "DCSBIOSStringReceived TACAN: >" + e.StringData + "< " + exception.Message + " \n" + exception.StackTrace);
                        //TODO Strange values from DCS-BIOS
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(349998, ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C knob)
        {
            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }
            switch (knob)
            {
                case RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH:
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
                            case CurrentA10RadioMode.VHFAM:
                                {
                                    if (_vhfAmCockpitMode != 0 && !VhfAmPresetSelected())
                                    {
                                        SaveCockpitFrequencyVhfAm();
                                        DCSBIOS.Send(VHF_AM_SET_FREQ + _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\n");
                                        SwapCockpitStandbyFrequencyVhfAm();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentA10RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SaveCockpitFrequencyUhf();
                                        DCSBIOS.Send(UHF_SET_FREQ + _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\n");
                                        SwapCockpitStandbyFrequencyUhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentA10RadioMode.VHFFM:
                                {
                                    if (_vhfFmCockpitMode != 0 && !VhfFmPresetSelected())
                                    {
                                        SaveCockpitFrequencyVhfFm();
                                        DCSBIOS.Send(VHF_FM_SET_FREQ + _vhfFmBigFrequencyStandby + _vhfFmSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\n");
                                        SwapCockpitStandbyFrequencyVhfFm();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentA10RadioMode.ILS:
                                {
                                    SendILSToDCSBIOS();
                                    break;
                                }
                            case CurrentA10RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
                case RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH:
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
                            case CurrentA10RadioMode.VHFAM:
                                {
                                    if (_vhfAmCockpitMode != 0 && !VhfAmPresetSelected())
                                    {
                                        SaveCockpitFrequencyVhfAm();
                                        DCSBIOS.Send(VHF_AM_SET_FREQ + _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\n");
                                        SwapCockpitStandbyFrequencyVhfAm();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentA10RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SaveCockpitFrequencyUhf();
                                        DCSBIOS.Send(UHF_SET_FREQ + _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\n");
                                        SwapCockpitStandbyFrequencyUhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentA10RadioMode.VHFFM:
                                {
                                    if (_vhfFmCockpitMode != 0 && !VhfFmPresetSelected())
                                    {
                                        SaveCockpitFrequencyVhfFm();
                                        DCSBIOS.Send(VHF_FM_SET_FREQ + _vhfFmBigFrequencyStandby + _vhfFmSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\n");
                                        SwapCockpitStandbyFrequencyVhfFm();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentA10RadioMode.ILS:
                                {
                                    SendILSToDCSBIOS();
                                    break;
                                }
                            case CurrentA10RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
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
                                    const string str = ILS_FREQ1_DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitFreq1DialPos > position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    const string str = ILS_FREQ1_DIAL_COMMAND + "DEC\n";
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
                                    const string str = ILS_FREQ2_DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitFreq2DialPos > position2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    const string str = ILS_FREQ2_DIAL_COMMAND + "DEC\n";
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
                                    var str = TACAN_FREQ1_DIAL_COMMAND + (_tacanCockpitFreq1DialPos < desiredPositionDial1 ? inc : dec);
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

                                    var str = TACAN_FREQ2_DIAL_COMMAND + (_tacanCockpitFreq2DialPos < desiredPositionDial2 ? inc : dec);
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

                                    var str = TACAN_FREQ3_DIAL_COMMAND + (_tacanCockpitFreq3DialPos < desiredPositionDial3 ? inc : dec);
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
                    case CurrentA10RadioMode.VHFAM:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfAmCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfAmCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_vhfAmCockpitMode != 0 && VhfAmPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfAmCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_vhfAmCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(GetVhfAmFrequencyAsString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby / 1000, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }

                            break;
                        }
                    case CurrentA10RadioMode.UHF:
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
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(GetUhfFrequencyAsString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby / 1000, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentA10RadioMode.VHFFM:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfFmCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfFmCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_vhfFmCockpitMode != 0 && VhfFmPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfFmCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                //Frequency selector 1      VHFFM_FREQ1
                                //      " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
                                //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                //Frequency selector 2      VHFFM_FREQ2
                                //0 1 2 3 4 5 6 7 8 9

                                //Frequency selector 3      VHFFM_FREQ3
                                //0 1 2 3 4 5 6 7 8 9

                                //Frequency selector 4      VHFFM_FREQ4
                                //      "00" "25" "50" "75", only "00" and "50" used.
                                //Pos     0    1    2    3
                                if (_vhfFmCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    var dial1 = "";
                                    var dial2 = "";
                                    var dial3 = "";
                                    var dial4 = "";
                                    lock (_lockVhfFmDialsObject1)
                                    {
                                        dial1 = GetVhfFmDialFrequencyForPosition(1, _vhfFmCockpitFreq1DialPos);
                                    }

                                    lock (_lockVhfFmDialsObject2)
                                    {
                                        dial2 = GetVhfFmDialFrequencyForPosition(2, _vhfFmCockpitFreq2DialPos);
                                    }

                                    lock (_lockVhfFmDialsObject3)
                                    {
                                        dial3 = GetVhfFmDialFrequencyForPosition(3, _vhfFmCockpitFreq3DialPos);
                                    }

                                    lock (_lockVhfFmDialsObject4)
                                    {
                                        dial4 = GetVhfFmDialFrequencyForPosition(4, _vhfFmCockpitFreq4DialPos);
                                    }

                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(dial1 + dial2 + "." + dial3 + dial4, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_vhfFmBigFrequencyStandby + "." + _vhfFmSmallFrequencyStandby.ToString().PadLeft(3, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentA10RadioMode.ILS:
                        {
                            //Mhz   "108" "109" "110" "111"
                            //Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                            var frequencyAsString = "";
                            lock (_lockIlsDialsObject1)
                            {
                                frequencyAsString = GetILSDialFrequencyForPosition(1, _ilsCockpitFreq1DialPos);
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockIlsDialsObject2)
                            {
                                frequencyAsString = frequencyAsString + GetILSDialFrequencyForPosition(2, _ilsCockpitFreq2DialPos);
                            }
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                    case CurrentA10RadioMode.TACAN:
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
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentA10RadioMode.VHFAM:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfAmCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfAmCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_vhfAmCockpitMode != 0 && VhfAmPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfAmCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_vhfAmCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(GetVhfAmFrequencyAsString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby / 1000, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentA10RadioMode.UHF:
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
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(GetUhfFrequencyAsString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby / 1000, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentA10RadioMode.VHFFM:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfFmCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfFmCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            if (_vhfFmCockpitMode != 0 && VhfFmPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vhfFmCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {


                                if (_vhfFmCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    var dial1 = "";
                                    var dial2 = "";
                                    var dial3 = "";
                                    var dial4 = "";
                                    lock (_lockVhfFmDialsObject1)
                                    {
                                        dial1 = GetVhfFmDialFrequencyForPosition(1, _vhfFmCockpitFreq1DialPos);
                                    }

                                    lock (_lockVhfFmDialsObject2)
                                    {
                                        dial2 = GetVhfFmDialFrequencyForPosition(2, _vhfFmCockpitFreq2DialPos);
                                    }

                                    lock (_lockVhfFmDialsObject3)
                                    {
                                        dial3 = GetVhfFmDialFrequencyForPosition(3, _vhfFmCockpitFreq3DialPos);
                                    }

                                    lock (_lockVhfFmDialsObject4)
                                    {
                                        dial4 = GetVhfFmDialFrequencyForPosition(4, _vhfFmCockpitFreq4DialPos);
                                    }

                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(dial1 + dial2 + "." + dial3 + dial4, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_vhfFmBigFrequencyStandby + "." + _vhfFmSmallFrequencyStandby.ToString().PadLeft(3, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentA10RadioMode.ILS:
                        {
                            var frequencyAsString = "";
                            lock (_lockIlsDialsObject1)
                            {
                                frequencyAsString = GetILSDialFrequencyForPosition(1, _ilsCockpitFreq1DialPos);
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockIlsDialsObject2)
                            {
                                frequencyAsString = frequencyAsString + GetILSDialFrequencyForPosition(2, _ilsCockpitFreq2DialPos);
                            }
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                    case CurrentA10RadioMode.TACAN:
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
                }
                SendLCDData(bytes);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
        }

        private string GetVhfAmFrequencyAsString()
        {
            //Frequency selector 1      VHFAM_FREQ1
            //      " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            //Frequency selector 2      VHFAM_FREQ2
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 3      VHFAM_FREQ3
            //0 1 2 3 4 5 6 7 8 9

            //Frequency selector 4      VHFAM_FREQ4
            //      "00" "25" "50" "75", only "00" and "50" used.
            //Pos     0    1    2    3
            var frequencyAsString = "";
            lock (_lockVhfAmDialsObject1)
            {
                frequencyAsString = GetVhfAmDialFrequencyForPosition(1, _vhfAmCockpitFreq1DialPos);
            }

            lock (_lockVhfAmDialsObject2)
            {
                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(2, _vhfAmCockpitFreq2DialPos);
            }

            frequencyAsString = frequencyAsString + ".";
            lock (_lockVhfAmDialsObject3)
            {
                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(3, _vhfAmCockpitFreq3DialPos);
            }

            lock (_lockVhfAmDialsObject4)
            {
                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(4, _vhfAmCockpitFreq4DialPos);
            }

            return frequencyAsString;
        }

        private string GetUhfFrequencyAsString()
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
            var frequencyAsString = "";
            lock (_lockUhfDialsObject1)
            {
                frequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfCockpitFreq1DialPos);
            }

            lock (_lockUhfDialsObject2)
            {
                frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(2, _uhfCockpitFreq2DialPos);
            }

            lock (_lockUhfDialsObject3)
            {
                frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(3, _uhfCockpitFreq3DialPos);
            }

            frequencyAsString = frequencyAsString + ".";
            lock (_lockUhfDialsObject4)
            {
                frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(4, _uhfCockpitFreq4DialPos);
            }

            lock (_lockUhfDialsObject5)
            {
                frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(5, _uhfCockpitFreq5DialPos);
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
                var radioPanelKnobA10C = (RadioPanelKnobA10C)o;
                if (radioPanelKnobA10C.IsOn)
                {
                    switch (radioPanelKnobA10C.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfAmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfAmPresetSelected() && _vhfAmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_PRESET_INCREASE);
                                                }
                                                else if (_vhfAmBigFrequencyStandby.Equals(151.00))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfAmBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_uhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FUNCTION_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(399.00))
                                                {
                                                    //225-399
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfFmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfFmPresetSelected() && _vhfFmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_PRESET_INCREASE);
                                                }
                                                else if (_vhfFmBigFrequencyStandby.Equals(76))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfFmBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //Mhz "108" "109" "110" "111"
                                            if (_ilsBigFrequencyStandby >= 111)
                                            {
                                                _ilsBigFrequencyStandby = 111;
                                                break;
                                            }
                                            _ilsBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfAmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfAmPresetSelected() && _vhfAmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_PRESET_DECREASE);
                                                }
                                                else if (_vhfAmBigFrequencyStandby.Equals(116.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfAmBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_uhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FUNCTION_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_DECREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(225.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfFmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfFmPresetSelected() && _vhfFmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_PRESET_DECREASE);
                                                }
                                                else if (_vhfFmBigFrequencyStandby.Equals(30))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfFmBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //"108" "109" "110" "111"
                                            if (_ilsBigFrequencyStandby <= 108)
                                            {
                                                _ilsBigFrequencyStandby = 108;
                                                break;
                                            }
                                            _ilsBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfAmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFAmSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
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
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfFmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFFMSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            switch (_ilsSmallFrequencyStandby)
                                            {
                                                case 10:
                                                    {
                                                        _ilsSmallFrequencyStandby = 15;
                                                        break;
                                                    }
                                                case 15:
                                                    {
                                                        _ilsSmallFrequencyStandby = 30;
                                                        break;
                                                    }
                                                case 30:
                                                    {
                                                        _ilsSmallFrequencyStandby = 35;
                                                        break;
                                                    }
                                                case 35:
                                                    {
                                                        _ilsSmallFrequencyStandby = 50;
                                                        break;
                                                    }
                                                case 50:
                                                    {
                                                        _ilsSmallFrequencyStandby = 55;
                                                        break;
                                                    }
                                                case 55:
                                                    {
                                                        _ilsSmallFrequencyStandby = 70;
                                                        break;
                                                    }
                                                case 70:
                                                    {
                                                        _ilsSmallFrequencyStandby = 75;
                                                        break;
                                                    }
                                                case 75:
                                                    {
                                                        _ilsSmallFrequencyStandby = 90;
                                                        break;
                                                    }
                                                case 90:
                                                    {
                                                        _ilsSmallFrequencyStandby = 95;
                                                        break;
                                                    }
                                                case 95:
                                                case 100:
                                                case 105:
                                                    {
                                                        //Just safe guard in case it pops above the limit. Happened to VHF AM for some !?!?!? reason.
                                                        _ilsSmallFrequencyStandby = 10;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfAmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFAmSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
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
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vhfFmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFFMSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            switch (_ilsSmallFrequencyStandby)
                                            {
                                                case 0:
                                                case 5:
                                                case 10:
                                                    {
                                                        _ilsSmallFrequencyStandby = 95;
                                                        break;
                                                    }
                                                case 15:
                                                    {
                                                        _ilsSmallFrequencyStandby = 10;
                                                        break;
                                                    }
                                                case 30:
                                                    {
                                                        _ilsSmallFrequencyStandby = 15;
                                                        break;
                                                    }
                                                case 35:
                                                    {
                                                        _ilsSmallFrequencyStandby = 30;
                                                        break;
                                                    }
                                                case 50:
                                                    {
                                                        _ilsSmallFrequencyStandby = 35;
                                                        break;
                                                    }
                                                case 55:
                                                    {
                                                        _ilsSmallFrequencyStandby = 50;
                                                        break;
                                                    }
                                                case 70:
                                                    {
                                                        _ilsSmallFrequencyStandby = 55;
                                                        break;
                                                    }
                                                case 75:
                                                    {
                                                        _ilsSmallFrequencyStandby = 70;
                                                        break;
                                                    }
                                                case 90:
                                                    {
                                                        _ilsSmallFrequencyStandby = 75;
                                                        break;
                                                    }
                                                case 95:
                                                    {
                                                        _ilsSmallFrequencyStandby = 90;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfAmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfAmPresetSelected() && _vhfAmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_PRESET_INCREASE);
                                                }
                                                else if (!_lowerButtonPressed && _vhfAmBigFrequencyStandby.Equals(151.00))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfAmBigFrequencyStandby = _vhfAmBigFrequencyStandby + 1;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_uhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FUNCTION_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    //225-399
                                                    DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(399.00))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby++;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfFmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfFmPresetSelected() && _vhfFmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_PRESET_INCREASE);
                                                }
                                                else if (_vhfFmBigFrequencyStandby.Equals(76))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfFmBigFrequencyStandby = _vhfFmBigFrequencyStandby + 1;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //Mhz "108" "109" "110" "111"
                                            if (_ilsBigFrequencyStandby >= 111)
                                            {
                                                _ilsBigFrequencyStandby = 111;
                                                break;
                                            }
                                            _ilsBigFrequencyStandby++;
                                            break; ;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfAmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfAmPresetSelected() && _vhfAmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_PRESET_DECREASE);
                                                }
                                                else if (_vhfAmBigFrequencyStandby.Equals(116.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfAmBigFrequencyStandby = _vhfAmBigFrequencyStandby - 1;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_uhfFunctionClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_FUNCTION_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (UhfPresetSelected() && _uhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(UHF_PRESET_DECREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(225.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _uhfBigFrequencyStandby--;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfFmModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VhfFmPresetSelected() && _vhfFmChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_PRESET_DECREASE);
                                                }
                                                else if (_vhfFmBigFrequencyStandby.Equals(30))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    _vhfFmBigFrequencyStandby = _vhfFmBigFrequencyStandby - 1;
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //"108" "109" "110" "111"
                                            if (_ilsBigFrequencyStandby <= 108)
                                            {
                                                _ilsBigFrequencyStandby = 108;
                                                break;
                                            }
                                            _ilsBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfAmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFAmSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
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
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfFmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFFMSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            switch (_ilsSmallFrequencyStandby)
                                            {
                                                case 10:
                                                    {
                                                        _ilsSmallFrequencyStandby = 15;
                                                        break;
                                                    }
                                                case 15:
                                                    {
                                                        _ilsSmallFrequencyStandby = 30;
                                                        break;
                                                    }
                                                case 30:
                                                    {
                                                        _ilsSmallFrequencyStandby = 35;
                                                        break;
                                                    }
                                                case 35:
                                                    {
                                                        _ilsSmallFrequencyStandby = 50;
                                                        break;
                                                    }
                                                case 50:
                                                    {
                                                        _ilsSmallFrequencyStandby = 55;
                                                        break;
                                                    }
                                                case 55:
                                                    {
                                                        _ilsSmallFrequencyStandby = 70;
                                                        break;
                                                    }
                                                case 70:
                                                    {
                                                        _ilsSmallFrequencyStandby = 75;
                                                        break;
                                                    }
                                                case 75:
                                                    {
                                                        _ilsSmallFrequencyStandby = 90;
                                                        break;
                                                    }
                                                case 90:
                                                    {
                                                        _ilsSmallFrequencyStandby = 95;
                                                        break;
                                                    }
                                                case 95:
                                                case 100:
                                                case 105:
                                                    {
                                                        //Just safe guard in case it pops above the limit. Happened to VHF AM for some !?!?!? reason.
                                                        _ilsSmallFrequencyStandby = 10;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentA10RadioMode.VHFAM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfAmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_AM_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFAmSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
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
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                if (_vhfFmFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VHF_FM_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                VHFFMSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.ILS:
                                        {
                                            //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            switch (_ilsSmallFrequencyStandby)
                                            {
                                                case 0:
                                                case 5:
                                                case 10:
                                                    {
                                                        _ilsSmallFrequencyStandby = 95;
                                                        break;
                                                    }
                                                case 15:
                                                    {
                                                        _ilsSmallFrequencyStandby = 10;
                                                        break;
                                                    }
                                                case 30:
                                                    {
                                                        _ilsSmallFrequencyStandby = 15;
                                                        break;
                                                    }
                                                case 35:
                                                    {
                                                        _ilsSmallFrequencyStandby = 30;
                                                        break;
                                                    }
                                                case 50:
                                                    {
                                                        _ilsSmallFrequencyStandby = 35;
                                                        break;
                                                    }
                                                case 55:
                                                    {
                                                        _ilsSmallFrequencyStandby = 50;
                                                        break;
                                                    }
                                                case 70:
                                                    {
                                                        _ilsSmallFrequencyStandby = 55;
                                                        break;
                                                    }
                                                case 75:
                                                    {
                                                        _ilsSmallFrequencyStandby = 70;
                                                        break;
                                                    }
                                                case 90:
                                                    {
                                                        _ilsSmallFrequencyStandby = 75;
                                                        break;
                                                    }
                                                case 95:
                                                    {
                                                        _ilsSmallFrequencyStandby = 90;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case CurrentA10RadioMode.TACAN:
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
                                }
                                break;
                            }
                    }
                }
            }
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
                if (_vhfFmSmallFrequencyStandby == 0)
                {
                    _vhfFmSmallFrequencyStandby = 975;
                }
                else
                {
                    _vhfFmSmallFrequencyStandby -= 25;
                }
            }


            if (_vhfFmSmallFrequencyStandby > 975)
            {
                _vhfFmSmallFrequencyStandby = 0;
            }
        }

        private void VHFAmSmallFrequencyStandbyAdjust(bool increase)
        {
            if (increase)
            {
                _vhfAmSmallFrequencyStandby += 25;
            }
            else
            {
                _vhfAmSmallFrequencyStandby -= 25;
            }

            if (_vhfAmSmallFrequencyStandby < 0)
            {
                _vhfAmSmallFrequencyStandby = 975;
            }
            else if (_vhfAmSmallFrequencyStandby > 975)
            {
                _vhfAmSmallFrequencyStandby = 0;
            }
        }

        private void UHFSmallFrequencyStandbyAdjust(bool increase)
        {
            if (increase)
            {
                _uhfSmallFrequencyStandby += 25;
            }
            else
            {
                _uhfSmallFrequencyStandby -= 25;
            }

            if (_uhfSmallFrequencyStandby < 0)
            {
                _uhfSmallFrequencyStandby = 975;
            }
            else if (_uhfSmallFrequencyStandby > 975)
            {
                _uhfSmallFrequencyStandby = 0;
            }
        }

        private void CheckFrequenciesForValidity()
        {
            //Crude fix if any freqs are outside the valid boundaries

            //VHF AM
            //116.00 - 151.975
            if (_vhfAmBigFrequencyStandby < 116)
            {
                _vhfAmBigFrequencyStandby = 116;
            }
            if (_vhfAmBigFrequencyStandby > 151)
            {
                _vhfAmBigFrequencyStandby = 151;
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
            if (_uhfBigFrequencyStandby < 225)
            {
                _uhfBigFrequencyStandby = 225;
            }
            if (_uhfBigFrequencyStandby > 399)
            {
                _uhfBigFrequencyStandby = 399;
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
                    var radioPanelKnob = (RadioPanelKnobA10C)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsA10C.UPPER_VHFAM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentA10RadioMode.VHFAM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentA10RadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentA10RadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_ILS:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentA10RadioMode.ILS;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentA10RadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_XPDR:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_VHFAM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentA10RadioMode.VHFAM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentA10RadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentA10RadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_ILS:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentA10RadioMode.ILS;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentA10RadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_XPDR:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH:
                            {
                                _upperButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH);
                                    }
                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH:
                            {
                                _lowerButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH);
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
                StartupBase("A-10C");

                //VHF AM
                _vhfAmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ1");
                //_vhfAmDcsbiosOutputFreqDial1.Debug = true;
                _vhfAmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ2");
                //_vhfAmDcsbiosOutputFreqDial2.Debug = true;
                _vhfAmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ3");
                //_vhfAmDcsbiosOutputFreqDial3.Debug = true;
                _vhfAmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ4");
                //_vhfAmDcsbiosOutputFreqDial4.Debug = true;
                _vhfAmDcsbiosOutputChannelFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQEMER");
                _vhfAmDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_PRESET");
                _vhfAmDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_MODE");
                //UHF
                _uhfDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_100MHZ_SEL");
                _uhfDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_10MHZ_SEL");
                _uhfDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT25_SEL");
                _uhfDcsbiosOutputFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_MODE");
                _uhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_SEL");
                _uhfDcsbiosOutputFunction = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FUNCTION");

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


                //ILS
                _ilsDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ILS_MHZ");
                _ilsDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ILS_KHZ");

                //TACAN
                _tacanDcsbiosOutputFreqChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("TACAN_CHANNEL");
                DCSBIOSStringListenerHandler.AddAddress(_tacanDcsbiosOutputFreqChannel.Address, 4, this); //_tacanDcsbiosOutputFreqChannel.MaxLength does not work. Bad JSON format.

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69A10C.StartUp() : " + ex.Message);
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
                            var knob = (RadioPanelKnobA10C)radioPanelKnob;

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
            _radioPanelKnobs = RadioPanelKnobA10C.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobA10C radioPanelKnob)
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
                                    return "00";
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

        private void SaveCockpitFrequencyVhfAm()
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
             * 00 25 50 75
             */
            lock (_lockVhfAmDialsObject1)
            {
                lock (_lockVhfAmDialsObject2)
                {
                    lock (_lockVhfAmDialsObject3)
                    {
                        lock (_lockVhfAmDialsObject4)
                        {
                            uint dial4 = 0;

                            switch (_vhfAmCockpitFreq4DialPos)
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
                                        //25
                                        dial4 = 50;
                                        break;
                                    }
                                case 3:
                                    {
                                        dial4 = 75;
                                        break;
                                    }
                            }
                            _vhfAmSavedCockpitBigFrequency = double.Parse((_vhfAmCockpitFreq1DialPos + 3).ToString() + _vhfAmCockpitFreq2DialPos.ToString(), NumberFormatInfoFullDisplay);
                            _vhfAmSavedCockpitSmallFrequency = double.Parse(_vhfAmCockpitFreq3DialPos.ToString() + dial4, NumberFormatInfoFullDisplay);
                        }
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVhfAm()
        {
            _vhfAmBigFrequencyStandby = _vhfAmSavedCockpitBigFrequency;
            _vhfAmSmallFrequencyStandby = _vhfAmSavedCockpitSmallFrequency;
        }

        private void SaveCockpitFrequencyUhf()
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
                var smallFrequencyAsString = "";
                lock (_lockUhfDialsObject1)
                {
                    bigFrequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfCockpitFreq1DialPos);

                }
                lock (_lockUhfDialsObject2)
                {
                    bigFrequencyAsString = bigFrequencyAsString + GetUhfDialFrequencyForPosition(2, _uhfCockpitFreq2DialPos);

                }
                lock (_lockUhfDialsObject3)
                {
                    bigFrequencyAsString = bigFrequencyAsString + GetUhfDialFrequencyForPosition(3, _uhfCockpitFreq3DialPos);

                }
                lock (_lockUhfDialsObject4)
                {
                    smallFrequencyAsString = smallFrequencyAsString + GetUhfDialFrequencyForPosition(4, _uhfCockpitFreq4DialPos);
                }
                lock (_lockUhfDialsObject5)
                {
                    smallFrequencyAsString = smallFrequencyAsString + GetUhfDialFrequencyForPosition(5, _uhfCockpitFreq5DialPos);
                }


                _uhfSavedCockpitBigFrequency = double.Parse(bigFrequencyAsString, NumberFormatInfoFullDisplay);
                _uhfSavedCockpitSmallFrequency = double.Parse(smallFrequencyAsString, NumberFormatInfoFullDisplay);



            }
            catch (Exception ex)
            {
                Common.LogError(83244, ex, "SaveCockpitFrequencyUhf()");
                throw;
            }
        }

        private void SwapCockpitStandbyFrequencyUhf()
        {
            _uhfBigFrequencyStandby = _uhfSavedCockpitBigFrequency;
            _uhfSmallFrequencyStandby = _uhfSavedCockpitSmallFrequency;
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

        private bool VhfAmPresetSelected()
        {
            return _vhfAmCockpitFreqMode == 3;
        }

        private bool VhfFmPresetSelected()
        {
            return _vhfFmCockpitFreqMode == 3;
        }

        private bool UhfPresetSelected()
        {
            return _uhfCockpitFreqMode == 1;
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
