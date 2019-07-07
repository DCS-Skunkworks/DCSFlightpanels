using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;


namespace NonVisuals.Radios
{
    public class RadioPanelPZ69F14B : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
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

        /* UHF AN/ARC-182 VHF UHF
           Large dial
            30-87[.975] 
            108-173[.975] 
            225-399[.975]

           Small dial 
            0.00-0.975 [step of x.x[0 2 5 7]

            This is a RIO radio.
        */
        private uint _vuhfBigFrequencyStandby = 225;
        private uint _vuhfSmallFrequencyStandby;
        private uint _vuhfCockpitBigFrequency;
        private uint _vuhfCockpitDial3Frequency;
        private uint _vuhfCockpitDial4Frequency;
        private uint _vuhfSavedCockpitBigFrequency;
        private uint _vuhfSavedCockpitDial3Frequency;
        private uint _vuhfSavedCockpitDial4Frequency;
        private readonly object _lockVuhfBigFreqObject1 = new object();
        private readonly object _lockVuhfDial3FreqObject2 = new object();
        private readonly object _lockVuhfDial4FreqObject2 = new object();
        private DCSBIOSOutput _vuhfDcsbiosOutputBigFrequencyNumber;
        private DCSBIOSOutput _vuhfDcsbiosOutputDial3FrequencyNumber;
        private DCSBIOSOutput _vuhfDcsbiosOutputDial4FrequencyNumber;
        private DCSBIOSOutput _vuhfDcsbiosOutputChannelFreqMode;  // 0 = PRESET
        private DCSBIOSOutput _vuhfDcsbiosOutputSelectedChannel;
        private volatile uint _vuhfCockpitFreqMode = 0;
        private volatile uint _vuhfCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _vuhfChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _vuhfFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string VUHF_PRESET_INCREASE = "RIO_VUHF_PRESETS INC\n";
        private const string VUHF_PRESET_DECREASE = "RIO_VUHF_PRESETS DEC\n";
        private const string VUHF_FREQ_MODE_INCREASE = "RIO_VUHF_FREQ_MODE INC\n";
        private const string VUHF_FREQ_MODE_DECREASE = "RIO_VUHF_FREQ_MODE DEC\n";

        private const string VUHF_MODE_INCREASE = "RIO_VUHF_MODE INC\n";
        private const string VUHF_MODE_DECREASE = "RIO_VUHF_MODE DEC\n";
        private DCSBIOSOutput _vuhfDcsbiosOutputMode;
        private volatile uint _vuhfCockpitMode = 0; // OFF = 0
        private readonly ClickSpeedDetector _vuhfModeClickSpeedDetector = new ClickSpeedDetector(8);
        private byte _skipVuhfSmallFreqChange = 0;

        /*PILOT TACAN NAV1*/
        //Tens dial 0-12 [step of 1]
        //Ones dial 0-9 [step of 1]
        //Last : X/Y [0,1]
        private int _pilotTacanTensFrequencyStandby = 6;
        private int _pilotTacanOnesFrequencyStandby = 5;
        private int _pilotTacanXYStandby;
        private int _pilotTacanSavedCockpitTensFrequency = 6;
        private int _pilotTacanSavedCockpitOnesFrequency = 5;
        private int _pilotTacanSavedCockpitXY;
        private readonly object _lockPilotTacanTensDialObject = new object();
        private readonly object _lockPilotTacanOnesObject = new object();
        private readonly object _lockPilotTacanXYDialObject = new object();
        private DCSBIOSOutput _pilotTacanDcsbiosOutputTensDial;
        private DCSBIOSOutput _pilotTacanDcsbiosOutputOnesDial;
        private DCSBIOSOutput _pilotTacanDcsbiosOutputXYDial;
        private volatile uint _pilotTacanCockpitTensDialPos = 1;
        private volatile uint _pilotTacanCockpitOnesDialPos = 1;
        private volatile uint _pilotTacanCockpitXYDialPos = 1;
        private const string PILOT_TACAN_TENS_DIAL_COMMAND = "PLT_TACAN_DIAL_TENS ";
        private const string PILOT_TACAN_ONES_DIAL_COMMAND = "PLT_TACAN_DIAL_ONES ";
        private const string PILOT_TACAN_XY_DIAL_COMMAND = "PLT_TACAN_CHANNEL "; //X = 0 | Y = 1
        private Thread _pilotTacanSyncThread;
        private long _pilotTacanThreadNowSynching;
        private long _pilotTacanTensWaitingForFeedback;
        private long _pilotTacanOnesWaitingForFeedback;
        private long _pilotTacanXYWaitingForFeedback;

        /*RIO TACAN NAV2*/
        //Tens dial 0-12 [step of 1]
        //Ones dial 0-9 [step of 1]
        //Last : X/Y [0,1]
        private int _rioTacanTensFrequencyStandby = 6;
        private int _rioTacanOnesFrequencyStandby = 5;
        private int _rioTacanXYStandby;
        private int _rioTacanSavedCockpitTensFrequency = 6;
        private int _rioTacanSavedCockpitOnesFrequency = 5;
        private int _rioTacanSavedCockpitXY;
        private readonly object _lockRioTacanTensDialObject = new object();
        private readonly object _lockRioTacanOnesObject = new object();
        private readonly object _lockRioTacanXYDialObject = new object();
        private DCSBIOSOutput _rioTacanDcsbiosOutputTensDial;
        private DCSBIOSOutput _rioTacanDcsbiosOutputOnesDial;
        private DCSBIOSOutput _rioTacanDcsbiosOutputXYDial;
        private volatile uint _rioTacanCockpitTensDialPos = 1;
        private volatile uint _rioTacanCockpitOnesDialPos = 1;
        private volatile uint _rioTacanCockpitXYDialPos = 1;
        private const string RIO_TACAN_TENS_DIAL_COMMAND = "RIO_TACAN_DIAL_TENS ";
        private const string RIO_TACAN_ONES_DIAL_COMMAND = "RIO_TACAN_DIAL_ONES ";
        private const string RIO_TACAN_XY_DIAL_COMMAND = "RIO_TACAN_CHANNEL "; //X = 0 | Y = 1
        private Thread _rioTacanSyncThread;
        private long _rioTacanThreadNowSynching;
        private long _rioTacanTensWaitingForFeedback;
        private long _rioTacanOnesWaitingForFeedback;
        private long _rioTacanXYWaitingForFeedback;

        /*RIO Link 4*/
        //Large Dial Hundreds 0 - 9  [step of 1]
        //Small Dial Tens     0 - 99 [step of 1]
        //Button Pressed + Large => ON/OFF/AUX
        private int _rioLink4HundredsFrequencyStandby = 0;
        private int _rioLink4TensAndOnesFrequencyStandby = 0;
        private int _rioLink4SavedCockpitHundredsFrequency = 0;
        private int _rioLink4SavedCockpitTensAndOnesFrequency = 0;
        private readonly object _lockRioLink4HundredsDial = new object();
        private readonly object _lockRioLink4TensDial = new object();
        private readonly object _lockRioLink4OnesDial = new object();
        private DCSBIOSOutput _rioLink4DcsbiosOutputHundredsDial;
        private DCSBIOSOutput _rioLink4DcsbiosOutputTensDial;
        private DCSBIOSOutput _rioLink4DcsbiosOutputOnesDial;
        private DCSBIOSOutput _rioLink4DcsbiosOutputPowerSwitch;//RIO_DATALINK_PW
        private volatile uint _rioLink4HundredsCockpitFrequency = 0;
        private volatile uint _rioLink4TensCockpitFrequency = 0;
        private volatile uint _rioLink4OnesCockpitFrequency = 0;
        private volatile uint _rioLink4CockpitPowerSwitch = 0;
        private const string RIO_LINK4_HUNDREDS_DIAL_COMMAND = "RIO_DATALINK_FREQ_10 ";
        private const string RIO_LINK4_TENS_DIAL_COMMAND = "RIO_DATALINK_FREQ_1 ";
        private const string RIO_LINK4_ONES_DIAL_COMMAND = "RIO_DATALINK_FREQ_100 ";
        private const string RIO_LINK4_POWER_COMMAND_INC = "RIO_DATALINK_PW INC\n";
        private const string RIO_LINK4_POWER_COMMAND_DEC = "RIO_DATALINK_PW DEC\n";
        private Thread _rioLink4SyncThread;
        private long _rioLink4ThreadNowSynching;
        private long _rioLinkHundredsWaitingForFeedback;
        private long _rioLinkTensWaitingForFeedback;
        private long _rioLinkOnesWaitingForFeedback;
        private readonly ClickSpeedDetector _rioLink4TensAndOnesClickSpeedDetector = new ClickSpeedDetector(20);
        private byte _skipRioLink4TensAndOnesFreqChange = 0;

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
            _pilotTacanSyncThread?.Abort();
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
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
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputChannelFreqMode.Address)
            {
                var tmp = _uhfCockpitFreqMode;
                _uhfCockpitFreqMode = _uhfDcsbiosOutputChannelFreqMode.GetUIntValue(e.Data);
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
            if (e.Address == _uhfDcsbiosOutputMode.Address)
            {
                var tmp = _uhfCockpitMode;
                _uhfCockpitMode = _uhfDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            //VHF UHF
            if (e.Address == _vuhfDcsbiosOutputBigFrequencyNumber.Address)
            {
                lock (_lockUhfBigFreqObject1)
                {
                    var tmp = _vuhfCockpitBigFrequency;
                    _vuhfCockpitBigFrequency = _vuhfDcsbiosOutputBigFrequencyNumber.GetUIntValue(e.Data);
                    if (tmp != _vuhfCockpitBigFrequency)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vuhfDcsbiosOutputDial3FrequencyNumber.Address)
            {
                lock (_lockUhfDial3FreqObject2)
                {
                    var tmp = _vuhfCockpitDial3Frequency;
                    _vuhfCockpitDial3Frequency = _vuhfDcsbiosOutputDial3FrequencyNumber.GetUIntValue(e.Data);
                    if (tmp != _vuhfCockpitDial3Frequency)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vuhfDcsbiosOutputDial4FrequencyNumber.Address)
            {
                lock (_lockUhfDial4FreqObject2)
                {
                    var tmp = _vuhfCockpitDial4Frequency;
                    _vuhfCockpitDial4Frequency = _vuhfDcsbiosOutputDial4FrequencyNumber.GetUIntValue(e.Data);
                    if (tmp != _vuhfCockpitDial4Frequency)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 5);
                    }
                }
            }
            if (e.Address == _vuhfDcsbiosOutputChannelFreqMode.Address)
            {
                var tmp = _vuhfCockpitFreqMode;
                _vuhfCockpitFreqMode = _vuhfDcsbiosOutputChannelFreqMode.GetUIntValue(e.Data);
                if (tmp != _vuhfCockpitFreqMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _vuhfDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _vuhfCockpitPresetChannel;
                _vuhfCockpitPresetChannel = _vuhfDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _vuhfCockpitPresetChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _vuhfDcsbiosOutputMode.Address)
            {
                var tmp = _vuhfCockpitMode;
                _vuhfCockpitMode = _vuhfDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _vuhfCockpitMode)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            //Pilot TACAN
            if (e.Address == _pilotTacanDcsbiosOutputTensDial.Address)
            {
                var tmp = _pilotTacanCockpitTensDialPos;
                _pilotTacanCockpitTensDialPos = _pilotTacanDcsbiosOutputTensDial.GetUIntValue(e.Data);
                if (tmp != _pilotTacanCockpitTensDialPos)
                {
                    Interlocked.Exchange(ref _pilotTacanTensWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _pilotTacanDcsbiosOutputOnesDial.Address)
            {
                var tmp = _pilotTacanCockpitOnesDialPos;
                _pilotTacanCockpitOnesDialPos = _pilotTacanDcsbiosOutputOnesDial.GetUIntValue(e.Data);
                if (tmp != _pilotTacanCockpitOnesDialPos)
                {
                    Interlocked.Exchange(ref _pilotTacanOnesWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _pilotTacanDcsbiosOutputXYDial.Address)
            {
                var tmp = _pilotTacanCockpitXYDialPos;
                _pilotTacanCockpitXYDialPos = _pilotTacanDcsbiosOutputXYDial.GetUIntValue(e.Data);
                if (tmp != _pilotTacanCockpitXYDialPos)
                {
                    Interlocked.Exchange(ref _pilotTacanXYWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            //RIO TACAN
            if (e.Address == _rioTacanDcsbiosOutputTensDial.Address)
            {
                var tmp = _rioTacanCockpitTensDialPos;
                _rioTacanCockpitTensDialPos = _rioTacanDcsbiosOutputTensDial.GetUIntValue(e.Data);
                if (tmp != _rioTacanCockpitTensDialPos)
                {
                    Interlocked.Exchange(ref _rioTacanTensWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _rioTacanDcsbiosOutputOnesDial.Address)
            {
                var tmp = _rioTacanCockpitOnesDialPos;
                _rioTacanCockpitOnesDialPos = _rioTacanDcsbiosOutputOnesDial.GetUIntValue(e.Data);
                if (tmp != _rioTacanCockpitOnesDialPos)
                {
                    Interlocked.Exchange(ref _rioTacanOnesWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _rioTacanDcsbiosOutputXYDial.Address)
            {
                var tmp = _rioTacanCockpitXYDialPos;
                _rioTacanCockpitXYDialPos = _rioTacanDcsbiosOutputXYDial.GetUIntValue(e.Data);
                if (tmp != _rioTacanCockpitXYDialPos)
                {
                    Interlocked.Exchange(ref _rioTacanXYWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            //RIO Link 4
            if (e.Address == _rioLink4DcsbiosOutputHundredsDial.Address)
            {
                var tmp = _rioLink4HundredsCockpitFrequency;
                _rioLink4HundredsCockpitFrequency = _rioLink4DcsbiosOutputHundredsDial.GetUIntValue(e.Data);
                if (tmp != _rioLink4HundredsCockpitFrequency)
                {
                    Interlocked.Exchange(ref _rioLinkHundredsWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _rioLink4DcsbiosOutputTensDial.Address)
            {
                var tmp = _rioLink4TensCockpitFrequency;
                _rioLink4TensCockpitFrequency = _rioLink4DcsbiosOutputTensDial.GetUIntValue(e.Data);
                if (tmp != _rioLink4TensCockpitFrequency)
                {
                    Interlocked.Exchange(ref _rioLinkTensWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _rioLink4DcsbiosOutputOnesDial.Address)
            {
                var tmp = _rioLink4OnesCockpitFrequency;
                _rioLink4OnesCockpitFrequency = _rioLink4DcsbiosOutputOnesDial.GetUIntValue(e.Data);
                if (tmp != _rioLink4OnesCockpitFrequency)
                {
                    Interlocked.Exchange(ref _rioLinkOnesWaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _rioLink4DcsbiosOutputPowerSwitch.Address)
            {
                var tmp = _rioLink4CockpitPowerSwitch;
                _rioLink4CockpitPowerSwitch = _rioLink4DcsbiosOutputPowerSwitch.GetUIntValue(e.Data);
                if (tmp != _rioLink4CockpitPowerSwitch)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.LogError(349998, ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF14B knob)
        {

            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH))
            {
                //Don't do anything on the very first button press as the panel sends ALL
                //switches when it is manipulated the first time
                //This would cause unintended sync.
                return;
            }
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
                                    if (_vuhfCockpitMode != 0 && !VuhfPresetSelected())
                                    {
                                        SaveCockpitFrequencyVuhf();
                                        var freq = _vuhfBigFrequencyStandby * 1000 + _vuhfSmallFrequencyStandby;
//Debug.WriteLine("Setting freg = > " + "SET_VUHF_FREQ " + freq);
                                        DCSBIOS.Send("SET_VUHF_FREQ " + freq + "\n");
                                        SwapCockpitStandbyFrequencyVuhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.PLT_TACAN:
                                {
                                    SendPilotTacanToDCSBIOS();
                                    break;
                                }
                            case CurrentF14RadioMode.RIO_TACAN:
                                {
                                    SendRioTacanToDCSBIOS();
                                    break;
                                }
                            case CurrentF14RadioMode.LINK4:
                                {
                                    SendLink4ToDCSBIOS();
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
                                    if (_vuhfCockpitMode != 0 && !VuhfPresetSelected())
                                    {
                                        SaveCockpitFrequencyVuhf();
                                        var freq = _vuhfBigFrequencyStandby * 1000 + _vuhfSmallFrequencyStandby;
//Debug.WriteLine("Setting freg = > " + "SET_VUHF_FREQ " + freq);
                                        DCSBIOS.Send("SET_VUHF_FREQ " + freq + "\n");
                                        SwapCockpitStandbyFrequencyVuhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.PLT_TACAN:
                                {
                                    SendPilotTacanToDCSBIOS();
                                    break;
                                }
                            case CurrentF14RadioMode.RIO_TACAN:
                                {
                                    SendRioTacanToDCSBIOS();
                                    break;
                                }
                            case CurrentF14RadioMode.LINK4:
                                {
                                    SendLink4ToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendLink4ToDCSBIOS()
        {
            if (Link4NowSyncing())
            {
                return;
            }
            SaveLink4Frequency();
            var dial2 = int.Parse(_rioLink4TensAndOnesFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0').Substring(0, 1));
            var dial3 = int.Parse(_rioLink4TensAndOnesFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0').Substring(1, 1));
            _rioLink4SyncThread?.Abort();
            _rioLink4SyncThread = new Thread(() => RioDatalink4SynchThreadMethod(_rioLink4HundredsFrequencyStandby, dial2, dial3));
            _rioLink4SyncThread.Start();
        }


        private void RioDatalink4SynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _rioLink4ThreadNowSynching, 1);

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
                            Interlocked.Exchange(ref _rioLinkHundredsWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 1");
                        }

                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "TACAN dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _rioLinkTensWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 2");
                        }

                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "TACAN dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _rioLinkOnesWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 3");
                        }

                        if (Interlocked.Read(ref _rioLinkHundredsWaitingForFeedback) == 0)
                        {
                            lock (_lockRioLink4HundredsDial)
                            {
                                if (_rioLink4HundredsCockpitFrequency != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_rioLink4HundredsCockpitFrequency is " + _rioLink4HundredsCockpitFrequency + " and should be " + desiredPositionDial1);
                                    var str = RIO_LINK4_HUNDREDS_DIAL_COMMAND + (_rioLink4HundredsCockpitFrequency < desiredPositionDial1 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _rioLinkHundredsWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _rioLinkTensWaitingForFeedback) == 0)
                        {
                            lock (_lockRioLink4TensDial)
                            {
                                if (_rioLink4TensCockpitFrequency != desiredPositionDial2)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_rioLink4TensCockpitFrequency is " + _rioLink4TensCockpitFrequency + " and should be " + desiredPositionDial2);
                                    var str = RIO_LINK4_TENS_DIAL_COMMAND + (_rioLink4TensCockpitFrequency < desiredPositionDial2 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _rioLinkTensWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _rioLinkOnesWaitingForFeedback) == 0)
                        {
                            lock (_lockRioLink4OnesDial)
                            {
                                if (_rioLink4OnesCockpitFrequency != desiredPositionDial3)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_rioLink4OnesCockpitFrequency is " + _rioLink4OnesCockpitFrequency + " and should be " + desiredPositionDial3);
                                    var str = RIO_LINK4_ONES_DIAL_COMMAND + (_rioLink4OnesCockpitFrequency < desiredPositionDial3 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _rioLinkOnesWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
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
                    SwapCockpitStandbyFrequencyDataLink4();
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
                Interlocked.Exchange(ref _rioLink4ThreadNowSynching, 0);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
        }



        private void SendPilotTacanToDCSBIOS()
        {
            if (PilotTacanNowSyncing())
            {
                return;
            }
            SavePilotCockpitFrequencyTacan();
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

            _pilotTacanSyncThread?.Abort();
            _pilotTacanSyncThread = new Thread(() => PilotTacanSynchThreadMethod(_pilotTacanTensFrequencyStandby, _pilotTacanOnesFrequencyStandby, _pilotTacanXYStandby));
            _pilotTacanSyncThread.Start();
        }

        private void PilotTacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _pilotTacanThreadNowSynching, 1);

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
                            Interlocked.Exchange(ref _pilotTacanTensWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 1");
                        }

                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "TACAN dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _pilotTacanOnesWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 2");
                        }

                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "TACAN dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _pilotTacanXYWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 3");
                        }

                        if (Interlocked.Read(ref _pilotTacanTensWaitingForFeedback) == 0)
                        {

                            lock (_lockPilotTacanTensDialObject)
                            {
                                if (_pilotTacanCockpitTensDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_pilotTacanCockpitFreq1DialPos is " + _pilotTacanCockpitTensDialPos + " and should be " + desiredPositionDial1);
                                    var str = PILOT_TACAN_TENS_DIAL_COMMAND + (_pilotTacanCockpitTensDialPos < desiredPositionDial1 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _pilotTacanTensWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _pilotTacanOnesWaitingForFeedback) == 0)
                        {
                            // Common.DebugP("b");
                            lock (_lockPilotTacanOnesObject)
                            {
                                if (_pilotTacanCockpitOnesDialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;

                                    var str = PILOT_TACAN_ONES_DIAL_COMMAND + (_pilotTacanCockpitOnesDialPos < desiredPositionDial2 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _pilotTacanOnesWaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _pilotTacanXYWaitingForFeedback) == 0)
                        {

                            lock (_lockPilotTacanXYDialObject)
                            {
                                if (_pilotTacanCockpitXYDialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;

                                    var str = PILOT_TACAN_XY_DIAL_COMMAND + (_pilotTacanCockpitXYDialPos < desiredPositionDial3 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _pilotTacanXYWaitingForFeedback, 1);
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
                    SwapPilotCockpitStandbyFrequencyTacan();
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
                Interlocked.Exchange(ref _pilotTacanThreadNowSynching, 0);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
        }


        private void SendRioTacanToDCSBIOS()
        {
            if (RioTacanNowSyncing())
            {
                return;
            }
            SaveRioCockpitFrequencyTacan();
            _rioTacanSyncThread?.Abort();
            _rioTacanSyncThread = new Thread(() => RioTacanSynchThreadMethod(_rioTacanTensFrequencyStandby, _rioTacanOnesFrequencyStandby, _rioTacanXYStandby));
            _rioTacanSyncThread.Start();
        }

        private void RioTacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _rioTacanThreadNowSynching, 1);

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
                            Interlocked.Exchange(ref _rioTacanTensWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 1");
                        }

                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "TACAN dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _rioTacanOnesWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 2");
                        }

                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "TACAN dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _rioTacanXYWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 3");
                        }

                        if (Interlocked.Read(ref _rioTacanTensWaitingForFeedback) == 0)
                        {

                            lock (_lockRioTacanTensDialObject)
                            {
                                if (_rioTacanCockpitTensDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_rioTacanCockpitFreq1DialPos is " + _rioTacanCockpitTensDialPos + " and should be " + desiredPositionDial1);
                                    var str = RIO_TACAN_TENS_DIAL_COMMAND + (_rioTacanCockpitTensDialPos < desiredPositionDial1 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _rioTacanTensWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _rioTacanOnesWaitingForFeedback) == 0)
                        {
                            // Common.DebugP("b");
                            lock (_lockRioTacanOnesObject)
                            {
                                if (_rioTacanCockpitOnesDialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;

                                    var str = RIO_TACAN_ONES_DIAL_COMMAND + (_rioTacanCockpitOnesDialPos < desiredPositionDial2 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _rioTacanOnesWaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _rioTacanXYWaitingForFeedback) == 0)
                        {

                            lock (_lockRioTacanXYDialObject)
                            {
                                if (_rioTacanCockpitXYDialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;

                                    var str = RIO_TACAN_XY_DIAL_COMMAND + (_rioTacanCockpitXYDialPos < desiredPositionDial3 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _rioTacanXYWaitingForFeedback, 1);
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
                    SwapRioCockpitStandbyFrequencyTacan();
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
                Interlocked.Exchange(ref _rioTacanThreadNowSynching, 0);
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
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vuhfCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vuhfCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_vuhfCockpitMode != 0 && VuhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vuhfCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_vuhfCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    var frequencyAsString = GetVUHFCockpitFrequencyAsString();
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, (double)_vuhfBigFrequencyStandby + (((double)_vuhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF14RadioMode.PLT_TACAN:
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
                            lock (_lockPilotTacanTensDialObject)
                            {
                                lock (_lockPilotTacanOnesObject)
                                {
                                    frequencyAsString = _pilotTacanCockpitTensDialPos.ToString() + _pilotTacanCockpitOnesDialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockPilotTacanXYDialObject)
                            {
                                frequencyAsString = frequencyAsString + _pilotTacanCockpitXYDialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_pilotTacanTensFrequencyStandby.ToString() + _pilotTacanOnesFrequencyStandby.ToString() + "." + _pilotTacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                    case CurrentF14RadioMode.RIO_TACAN:
                        {
                            var frequencyAsString = "";
                            lock (_lockRioTacanTensDialObject)
                            {
                                lock (_lockRioTacanOnesObject)
                                {
                                    frequencyAsString = _rioTacanCockpitTensDialPos.ToString() + _rioTacanCockpitOnesDialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockRioTacanXYDialObject)
                            {
                                frequencyAsString = frequencyAsString + _rioTacanCockpitXYDialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_rioTacanTensFrequencyStandby.ToString() + _rioTacanOnesFrequencyStandby.ToString() + "." + _rioTacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                    case CurrentF14RadioMode.LINK4:
                        {
                            if (_rioLink4CockpitPowerSwitch == 0) // OFF
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_rioLink4CockpitPowerSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                var frequencyAsString = "";
                                lock (_lockRioLink4HundredsDial)
                                {
                                    lock (_lockRioLink4TensDial)
                                    {
                                        lock (_lockRioLink4OnesDial)
                                        {
                                            frequencyAsString =
                                                _rioLink4HundredsCockpitFrequency.ToString() +
                                                _rioLink4TensCockpitFrequency.ToString() +
                                                _rioLink4OnesCockpitFrequency;
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesString(ref bytes, frequencyAsString.ToString(), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesString(ref bytes, _rioLink4HundredsFrequencyStandby.ToString() + _rioLink4TensAndOnesFrequencyStandby.ToString().PadLeft(2, '0'), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            break;
                        }
                    case CurrentF14RadioMode.NOUSE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
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
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vuhfCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vuhfCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_vuhfCockpitMode != 0 && VuhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_vuhfCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_vuhfCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
                                    var frequencyAsString = GetVUHFCockpitFrequencyAsString();
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, (double)_vuhfBigFrequencyStandby + (((double)_vuhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF14RadioMode.PLT_TACAN:
                        {
                            var frequencyAsString = "";
                            lock (_lockPilotTacanTensDialObject)
                            {
                                lock (_lockPilotTacanOnesObject)
                                {
                                    frequencyAsString = _pilotTacanCockpitTensDialPos.ToString() + _pilotTacanCockpitOnesDialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockPilotTacanXYDialObject)
                            {
                                frequencyAsString = frequencyAsString + _pilotTacanCockpitXYDialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_pilotTacanTensFrequencyStandby.ToString() + _pilotTacanOnesFrequencyStandby.ToString() + "." + _pilotTacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                    case CurrentF14RadioMode.RIO_TACAN:
                        {
                            var frequencyAsString = "";
                            lock (_lockRioTacanTensDialObject)
                            {
                                lock (_lockRioTacanOnesObject)
                                {
                                    frequencyAsString = _rioTacanCockpitTensDialPos.ToString() + _rioTacanCockpitOnesDialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockRioTacanXYDialObject)
                            {
                                frequencyAsString = frequencyAsString + _rioTacanCockpitXYDialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_rioTacanTensFrequencyStandby.ToString() + _rioTacanOnesFrequencyStandby.ToString() + "." + _rioTacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                    case CurrentF14RadioMode.LINK4:
                        {
                            if (_rioLink4CockpitPowerSwitch == 0) // OFF
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_rioLink4CockpitPowerSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                var frequencyAsString = "";
                                lock (_lockRioLink4HundredsDial)
                                {
                                    lock (_lockRioLink4TensDial)
                                    {
                                        lock (_lockRioLink4OnesDial)
                                        {
                                            frequencyAsString =
                                                _rioLink4HundredsCockpitFrequency.ToString() +
                                                _rioLink4TensCockpitFrequency.ToString() +
                                                _rioLink4OnesCockpitFrequency;
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesString(ref bytes, frequencyAsString.ToString(), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesString(ref bytes, _rioLink4HundredsFrequencyStandby.ToString() + _rioLink4TensAndOnesFrequencyStandby.ToString().PadLeft(2, '0'), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            break;
                        }
                    case CurrentF14RadioMode.NOUSE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
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
                        frequencyAsString = _uhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
                        frequencyAsString = frequencyAsString + ".";
                        frequencyAsString = frequencyAsString + _uhfCockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString = frequencyAsString + _uhfCockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');
                        //225.000 7 characters
                    }
                }
            }
            return frequencyAsString;
        }

        private string GetVUHFCockpitFrequencyAsString()
        {
            var frequencyAsString = "";
            lock (_lockVuhfBigFreqObject1)
            {
                lock (_lockVuhfDial3FreqObject2)
                {
                    lock (_lockVuhfDial4FreqObject2)
                    {
                        frequencyAsString = _vuhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString = frequencyAsString + ".";
                        frequencyAsString = frequencyAsString + _vuhfCockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString = frequencyAsString + _vuhfCockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');
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
                                                    if (_uhfCockpitPresetChannel < 20)
                                                    {
                                                        DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                    }
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
                                                if (_vuhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VuhfPresetSelected() && _vuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    if (_vuhfCockpitPresetChannel < 30)
                                                    {
                                                        DCSBIOS.Send(VUHF_PRESET_INCREASE);
                                                    }
                                                }
                                                else
                                                {
                                                    AdjustVUHFBigFrequency(true);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanTensFrequencyStandby >= 12)
                                            {
                                                _pilotTacanTensFrequencyStandby = 12;
                                                break;
                                            }
                                            _pilotTacanTensFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanTensFrequencyStandby >= 12)
                                            {
                                                _rioTacanTensFrequencyStandby = 12;
                                                break;
                                            }
                                            _rioTacanTensFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                DCSBIOS.Send(RIO_LINK4_POWER_COMMAND_INC);
                                            }
                                            else
                                            {
                                                if (_rioLink4HundredsFrequencyStandby >= 9)
                                                {
                                                    _rioLink4HundredsFrequencyStandby = 0;
                                                    break;
                                                }

                                                _rioLink4HundredsFrequencyStandby++;
                                            }
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
                                                    DCSBIOS.Send(UHF_PRESET_DECREASE);
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
                                                if (_vuhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VuhfPresetSelected() && _vuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_PRESET_DECREASE);
                                                }
                                                else
                                                {
                                                    AdjustVUHFBigFrequency(false);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanTensFrequencyStandby <= 0)
                                            {
                                                _pilotTacanTensFrequencyStandby = 0;
                                                break;
                                            }
                                            _pilotTacanTensFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanTensFrequencyStandby <= 0)
                                            {
                                                _rioTacanTensFrequencyStandby = 0;
                                                break;
                                            }
                                            _rioTacanTensFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            if (_upperButtonPressed)
                                            {
                                                _upperButtonPressedAndDialRotated = true;
                                                DCSBIOS.Send(RIO_LINK4_POWER_COMMAND_DEC);
                                            }
                                            else
                                            {
                                                if (_rioLink4HundredsFrequencyStandby <= 0)
                                                {
                                                    _rioLink4HundredsFrequencyStandby = 9;
                                                    break;
                                                }

                                                _rioLink4HundredsFrequencyStandby--;
                                            }
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
                                                if (_vuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                VUHFSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanOnesFrequencyStandby >= 9)
                                            {
                                                _pilotTacanOnesFrequencyStandby = 9;
                                                _pilotTacanXYStandby = 1;
                                                break;
                                            }
                                            _pilotTacanOnesFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanOnesFrequencyStandby >= 9)
                                            {
                                                _rioTacanOnesFrequencyStandby = 9;
                                                _rioTacanXYStandby = 1;
                                                break;
                                            }
                                            _rioTacanOnesFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            Link4TensAndOnesFrequencyStandbyAdjust(true);
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
                                                if (_vuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                VUHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanOnesFrequencyStandby <= 0)
                                            {
                                                _pilotTacanOnesFrequencyStandby = 0;
                                                _pilotTacanXYStandby = 0;
                                                break;
                                            }
                                            _pilotTacanOnesFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanOnesFrequencyStandby <= 0)
                                            {
                                                _rioTacanOnesFrequencyStandby = 0;
                                                _rioTacanXYStandby = 0;
                                                break;
                                            }
                                            _rioTacanOnesFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            Link4TensAndOnesFrequencyStandbyAdjust(false);
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
                                                    if (_uhfCockpitPresetChannel < 20)
                                                    {
                                                        DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                    }
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
                                                if (_vuhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VuhfPresetSelected() && _vuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    if (_vuhfCockpitPresetChannel < 30)
                                                    {
                                                        DCSBIOS.Send(VUHF_PRESET_INCREASE);
                                                    }
                                                }
                                                else
                                                {
                                                    AdjustVUHFBigFrequency(true);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanTensFrequencyStandby >= 12)
                                            {
                                                _pilotTacanTensFrequencyStandby = 12;
                                                break;
                                            }
                                            _pilotTacanTensFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanTensFrequencyStandby >= 12)
                                            {
                                                _rioTacanTensFrequencyStandby = 12;
                                                break;
                                            }
                                            _rioTacanTensFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                DCSBIOS.Send(RIO_LINK4_POWER_COMMAND_INC);
                                            }
                                            else
                                            {
                                                if (_rioLink4HundredsFrequencyStandby >= 9)
                                                {
                                                    _rioLink4HundredsFrequencyStandby = 0;
                                                    break;
                                                }
                                                _rioLink4HundredsFrequencyStandby++;
                                            }
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
                                                if (_vuhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VuhfPresetSelected() && _vuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_PRESET_DECREASE);
                                                }
                                                else
                                                {
                                                    AdjustVUHFBigFrequency(false);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanTensFrequencyStandby <= 0)
                                            {
                                                _pilotTacanTensFrequencyStandby = 0;
                                                break;
                                            }
                                            _pilotTacanTensFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanTensFrequencyStandby <= 0)
                                            {
                                                _rioTacanTensFrequencyStandby = 0;
                                                break;
                                            }
                                            _rioTacanTensFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            if (_lowerButtonPressed)
                                            {
                                                _lowerButtonPressedAndDialRotated = true;
                                                DCSBIOS.Send(RIO_LINK4_POWER_COMMAND_DEC);
                                            }
                                            else
                                            {
                                                if (_rioLink4HundredsFrequencyStandby <= 0)
                                                {
                                                    _rioLink4HundredsFrequencyStandby = 9;
                                                    break;
                                                }

                                                _rioLink4HundredsFrequencyStandby--;
                                            }
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
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_FREQ_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                VUHFSmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanOnesFrequencyStandby >= 9)
                                            {
                                                _pilotTacanOnesFrequencyStandby = 9;
                                                _pilotTacanXYStandby = 1;
                                                break;
                                            }
                                            _pilotTacanOnesFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanOnesFrequencyStandby >= 9)
                                            {
                                                _rioTacanOnesFrequencyStandby = 9;
                                                _rioTacanXYStandby = 1;
                                                break;
                                            }
                                            _rioTacanOnesFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            Link4TensAndOnesFrequencyStandbyAdjust(true);
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
                                                _upperButtonPressedAndDialRotated = true;
                                                if (_vuhfFreqModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_FREQ_MODE_DECREASE);
                                                }
                                            }
                                            else
                                            {
                                                VUHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF14RadioMode.PLT_TACAN:
                                        {
                                            //TACAN  00X/Y --> 129X/Y
                                            //
                                            //Frequency selector 1      LEFT
                                            //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            //Frequency selector 2      MIDDLE
                                            //0 1 2 3 4 5 6 7 8 9

                                            //Frequency selector 3      RIGHT
                                            //X=0 / Y=1

                                            if (_pilotTacanOnesFrequencyStandby <= 0)
                                            {
                                                _pilotTacanOnesFrequencyStandby = 0;
                                                _pilotTacanXYStandby = 0;
                                                break;
                                            }
                                            _pilotTacanOnesFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.RIO_TACAN:
                                        {
                                            if (_rioTacanOnesFrequencyStandby <= 0)
                                            {
                                                _rioTacanOnesFrequencyStandby = 0;
                                                _rioTacanXYStandby = 0;
                                                break;
                                            }
                                            _rioTacanOnesFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentF14RadioMode.LINK4:
                                        {
                                            Link4TensAndOnesFrequencyStandbyAdjust(false);
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


        private void Link4TensAndOnesFrequencyStandbyAdjust(bool increase)
        {
            var changeValue = 1;
            _skipRioLink4TensAndOnesFreqChange++;
            if (_skipRioLink4TensAndOnesFreqChange < 2)
            {
                _rioLink4TensAndOnesClickSpeedDetector.Click();
                return;
            }
            _skipRioLink4TensAndOnesFreqChange = 0;
            if (_rioLink4TensAndOnesClickSpeedDetector.ClickAndCheck())
            {
                changeValue = 5;
            }
            if (increase)
            {
                _rioLink4TensAndOnesFrequencyStandby += changeValue;
            }
            else
            {
                _rioLink4TensAndOnesFrequencyStandby -= changeValue;
            }

            if (_rioLink4TensAndOnesFrequencyStandby > 99)
            {
                _rioLink4TensAndOnesFrequencyStandby = 0;
            }
            else if (_rioLink4TensAndOnesFrequencyStandby < 0)
            {
                _rioLink4TensAndOnesFrequencyStandby = 99;
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

        private void VUHFSmallFrequencyStandbyAdjust(bool increase)
        {
            _skipVuhfSmallFreqChange++;
            if (_skipVuhfSmallFreqChange < 2)
            {
                return;
            }
            _skipVuhfSmallFreqChange = 0;

            if (increase)
            {
                _vuhfSmallFrequencyStandby += 25;
            }
            else
            {
                if (_vuhfSmallFrequencyStandby == 0)
                {
                    _vuhfSmallFrequencyStandby = 975;
                }
                else
                {
                    _vuhfSmallFrequencyStandby -= 25;
                }
            }

            if (_vuhfSmallFrequencyStandby > 975)
            {
                _vuhfSmallFrequencyStandby = 0;
            }
        }

        private void AdjustVUHFBigFrequency(bool increase)
        {
            if (increase)
            {
                if (_vuhfBigFrequencyStandby == 87)
                {
                    _vuhfBigFrequencyStandby = 108;
                }
                else if (_vuhfBigFrequencyStandby == 173)
                {
                    _vuhfBigFrequencyStandby = 225;
                }
                else if (_vuhfBigFrequencyStandby == 399)
                {
                    _vuhfBigFrequencyStandby = 30;
                }
                else
                {
                    _vuhfBigFrequencyStandby++;
                }
            }
            else
            {
                if (_vuhfBigFrequencyStandby == 30)
                {
                    _vuhfBigFrequencyStandby = 399;
                }
                else if (_vuhfBigFrequencyStandby == 225)
                {
                    _vuhfBigFrequencyStandby = 173;
                }
                else if (_vuhfBigFrequencyStandby == 108)
                {
                    _vuhfBigFrequencyStandby = 87;
                }
                else
                {
                    _vuhfBigFrequencyStandby--;
                }
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


            //TACAN
            //00X/Y - 129X/Y
            if (_pilotTacanTensFrequencyStandby < 0)
            {
                _pilotTacanTensFrequencyStandby = 0;
            }
            if (_pilotTacanTensFrequencyStandby > 12)
            {
                _pilotTacanTensFrequencyStandby = 12;
            }
            if (_pilotTacanOnesFrequencyStandby < 0)
            {
                _pilotTacanOnesFrequencyStandby = 0;
            }
            if (_pilotTacanOnesFrequencyStandby > 9)
            {
                _pilotTacanOnesFrequencyStandby = 9;
            }
            if (_pilotTacanXYStandby < 0)
            {
                _pilotTacanXYStandby = 0;
            }
            if (_pilotTacanXYStandby > 1)
            {
                _pilotTacanXYStandby = 1;
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
                        case RadioPanelPZ69KnobsF14B.UPPER_PLT_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.PLT_TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_RIO_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.RIO_TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_LINK4:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF14RadioMode.LINK4;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.UPPER_ADF:
                        case RadioPanelPZ69KnobsF14B.UPPER_XPDR:
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
                        case RadioPanelPZ69KnobsF14B.LOWER_PLT_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.PLT_TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_RIO_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.RIO_TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_LINK4:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.LINK4;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF14B.LOWER_ADF:
                        case RadioPanelPZ69KnobsF14B.LOWER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF14RadioMode.NOUSE;
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
                _uhfDcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF_DIAL3_FREQ");
                _uhfDcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF_DIAL4_FREQ");
                _uhfDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_UHF1_FUNCTION");

                //VUHF
                _vuhfDcsbiosOutputChannelFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_VUHF_FREQ_MODE");
                _vuhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_VUHF_PRESETS");
                _vuhfDcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_VUHF_HIGH_FREQ");
                _vuhfDcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_VUHF_DIAL3_FREQ");
                _vuhfDcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_VUHF_DIAL4_FREQ");
                _vuhfDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_VUHF_MODE");

                //Pilot & RIO TACAN
                _pilotTacanDcsbiosOutputTensDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_TACAN_DIAL_TENS");
                _pilotTacanDcsbiosOutputOnesDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_TACAN_DIAL_ONES");
                _pilotTacanDcsbiosOutputXYDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_TACAN_CHANNEL");
                _rioTacanDcsbiosOutputTensDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_TACAN_DIAL_TENS");
                _rioTacanDcsbiosOutputOnesDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_TACAN_DIAL_ONES");
                _rioTacanDcsbiosOutputXYDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_TACAN_CHANNEL");

                //RIO Link 4
                _rioLink4DcsbiosOutputHundredsDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_DATALINK_FREQ_10");
                _rioLink4DcsbiosOutputTensDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_DATALINK_FREQ_1");
                _rioLink4DcsbiosOutputOnesDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_DATALINK_FREQ_100");
                _rioLink4DcsbiosOutputPowerSwitch = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_DATALINK_PW");

                StartListeningForPanelChanges();
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

        protected override void SaitekPanelKnobChanged(IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(hashSet);
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobF14B.GetRadioPanelKnobs();
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
                        _uhfBigFrequencyStandby = _uhfSavedCockpitBigFrequency;
                        _uhfSmallFrequencyStandby = _uhfSavedCockpitDial3Frequency * 100 + _uhfSavedCockpitDial4Frequency;
                    }
                }
            }
        }


        private void SaveCockpitFrequencyVuhf()
        {
            lock (_lockVuhfBigFreqObject1)
            {
                lock (_lockVuhfDial3FreqObject2)
                {
                    lock (_lockVuhfDial4FreqObject2)
                    {
                        _vuhfSavedCockpitBigFrequency = _vuhfCockpitBigFrequency;
                        _vuhfSavedCockpitDial3Frequency = _vuhfCockpitDial3Frequency;
                        _vuhfSavedCockpitDial4Frequency = _vuhfCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVuhf()
        {
            lock (_lockVuhfBigFreqObject1)
            {
                lock (_lockVuhfDial3FreqObject2)
                {
                    lock (_lockVuhfDial4FreqObject2)
                    {
                        _vuhfBigFrequencyStandby = _vuhfSavedCockpitBigFrequency;
                        _vuhfSmallFrequencyStandby = _vuhfSavedCockpitDial3Frequency * 100 + _vuhfSavedCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyDataLink4()
        {
            lock (_lockRioLink4HundredsDial)
            {
                lock (_lockRioLink4TensDial)
                {
                    lock (_lockRioLink4OnesDial)
                    {
                        _rioLink4HundredsFrequencyStandby = _rioLink4SavedCockpitHundredsFrequency;
                        _rioLink4TensAndOnesFrequencyStandby = _rioLink4SavedCockpitTensAndOnesFrequency;
                    }
                }
            }
        }

        private void SaveLink4Frequency()
        {
            lock (_lockRioLink4HundredsDial)
            {
                lock (_lockRioLink4TensDial)
                {
                    lock (_lockRioLink4OnesDial)
                    {
                        _rioLink4SavedCockpitHundredsFrequency = (int)_rioLink4HundredsCockpitFrequency;
                        _rioLink4SavedCockpitTensAndOnesFrequency = int.Parse(_rioLink4TensCockpitFrequency.ToString(CultureInfo.InvariantCulture) + _rioLink4OnesCockpitFrequency.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private void SavePilotCockpitFrequencyTacan()
        {
            /*TACAN*/
            //Large dial 0-12 [step of 1]
            //Small dial 0-9 [step of 1]
            //Last : X/Y [0,1]
            lock (_lockPilotTacanTensDialObject)
            {
                lock (_lockPilotTacanOnesObject)
                {
                    lock (_lockPilotTacanXYDialObject)
                    {
                        _pilotTacanSavedCockpitTensFrequency = Convert.ToInt32(_pilotTacanCockpitTensDialPos);
                        _pilotTacanSavedCockpitOnesFrequency = Convert.ToInt32(_pilotTacanCockpitOnesDialPos);
                        _pilotTacanSavedCockpitXY = Convert.ToInt32(_pilotTacanCockpitXYDialPos);
                    }
                }
            }
        }

        private void SwapPilotCockpitStandbyFrequencyTacan()
        {
            _pilotTacanTensFrequencyStandby = _pilotTacanSavedCockpitTensFrequency;
            _pilotTacanOnesFrequencyStandby = _pilotTacanSavedCockpitOnesFrequency;
            _pilotTacanXYStandby = _pilotTacanSavedCockpitXY;
        }

        private void SaveRioCockpitFrequencyTacan()
        {
            /*TACAN*/
            //Large dial 0-12 [step of 1]
            //Small dial 0-9 [step of 1]
            //Last : X/Y [0,1]
            lock (_lockRioTacanTensDialObject)
            {
                lock (_lockRioTacanOnesObject)
                {
                    lock (_lockRioTacanXYDialObject)
                    {
                        _rioTacanSavedCockpitTensFrequency = Convert.ToInt32(_rioTacanCockpitTensDialPos);
                        _rioTacanSavedCockpitOnesFrequency = Convert.ToInt32(_rioTacanCockpitOnesDialPos);
                        _rioTacanSavedCockpitXY = Convert.ToInt32(_rioTacanCockpitXYDialPos);
                    }
                }
            }
        }

        private void SwapRioCockpitStandbyFrequencyTacan()
        {
            _rioTacanTensFrequencyStandby = _rioTacanSavedCockpitTensFrequency;
            _rioTacanOnesFrequencyStandby = _rioTacanSavedCockpitOnesFrequency;
            _rioTacanXYStandby = _rioTacanSavedCockpitXY;
        }

        private bool UhfPresetSelected()
        {
            return _uhfCockpitFreqMode == 0;
        }

        private bool VuhfPresetSelected()
        {
            return _vuhfCockpitFreqMode == 3;
        }

        private bool PilotTacanNowSyncing()
        {
            return Interlocked.Read(ref _pilotTacanThreadNowSynching) > 0;
        }

        private bool Link4NowSyncing()
        {
            return Interlocked.Read(ref _rioLink4ThreadNowSynching) > 0;
        }

        private bool RioTacanNowSyncing()
        {
            return Interlocked.Read(ref _rioTacanThreadNowSynching) > 0;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }
    }

}

