using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using MEF;
    using Plugin;
    using Knobs;
    using Panels.Saitek;
    using HID;
    using Helpers;
    using DCS_BIOS.Serialized;
    using DCS_BIOS.ControlLocator;


    /// <summary>
    /// Pre-programmed radio panel for the F-14B. 
    /// </summary>
    public class RadioPanelPZ69F14B : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentF14RadioMode
        {
            UHF,
            VUHF,
            PLT_TACAN,
            RIO_TACAN,
            LINK4,
            NO_USE
        }

        private CurrentF14RadioMode _currentUpperRadioMode = CurrentF14RadioMode.UHF;
        private CurrentF14RadioMode _currentLowerRadioMode = CurrentF14RadioMode.UHF;

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;
        private bool _ignoreUpperButtonOnce = true;
        private bool _ignoreLowerButtonOnce = true;

        /* UHF AN/ARC-159 */
        // Large dial 225-399 [step of 1]
        // Small dial 0.00-0.97 [step of x.x[0 2 5 7]
        private uint _uhfBigFrequencyStandby = 225;
        private uint _uhfSmallFrequencyStandby;
        private uint _uhfCockpitBigFrequency;
        private uint _uhfCockpitDial1Frequency;
        private uint _uhfCockpitDial2Frequency;
        private uint _uhfCockpitDial3Frequency;
        private uint _uhfCockpitDial4Frequency;
        private uint _uhfSavedCockpitDial1Frequency;
        private uint _uhfSavedCockpitDial2Frequency;
        private uint _uhfSavedCockpitDial3Frequency;
        private uint _uhfSavedCockpitDial4Frequency;
        private readonly object _lockUhfDialBigFreqObject = new();
        private readonly object _lockUhfDial3FreqObject = new();
        private readonly object _lockUhfDial4FreqObject = new();
        private readonly object _lockUhfPresetObject = new();
        private DCSBIOSOutput _uhfDcsbiosOutputBigFrequencyNumber;
        private DCSBIOSOutput _uhfDcsbiosOutputDial3FrequencyNumber;
        private DCSBIOSOutput _uhfDcsbiosOutputDial4FrequencyNumber;
        private DCSBIOSOutput _uhfDcsbiosOutputChannelFreqMode;  // 0 = PRESET
        private DCSBIOSOutput _uhfDcsbiosOutputSelectedChannel;
        private volatile uint _uhfCockpitFreqMode;
        private volatile uint _uhfCockpitPresetChannel;
        private readonly ClickSpeedDetector _uhfChannelClickSpeedDetector = new(8);
        private readonly ClickSpeedDetector _uhfFreqModeClickSpeedDetector = new(6);

        private const string UHF_1ST_DIAL_INCREASE = "PLT_UHF1_110_DIAL INC\n";
        private const string UHF_1ST_DIAL_DECREASE = "PLT_UHF1_110_DIAL DEC\n";
        private const string UHF_1ST_DIAL_NEUTRAL = "PLT_UHF1_110_DIAL 1\n";

        private const string UHF_2ND_DIAL_COMMAND = "PLT_UHF1_1_DIAL ";
        private const string UHF_2ND_DIAL_NEUTRAL = "PLT_UHF1_1_DIAL 1\n";

        private const string UHF_3RD_DIAL_COMMAND = "PLT_UHF1_01_DIAL ";
        private const string UHF_3RD_DIAL_NEUTRAL = "PLT_UHF1_01_DIAL 1\n";

        private const string UHF_4TH_DIAL_INCREASE = "PLT_UHF1_025_DIAL INC\n";
        private const string UHF_4TH_DIAL_DECREASE = "PLT_UHF1_025_DIAL DEC\n";
        private const string UHF_4TH_DIAL_NEUTRAL = "PLT_UHF1_025_DIAL 1\n";

        private const string UHF_PRESET_INCREASE = "PLT_UHF1_PRESETS INC\n";
        private const string UHF_PRESET_DECREASE = "PLT_UHF1_PRESETS DEC\n";
        private const string UHF_FREQ_MODE_INCREASE = "PLT_UHF1_FREQ_MODE INC\n";
        private const string UHF_FREQ_MODE_DECREASE = "PLT_UHF1_FREQ_MODE DEC\n";

        private const string UHF_MODE_INCREASE = "PLT_UHF1_FUNCTION INC\n";
        private const string UHF_MODE_DECREASE = "PLT_UHF1_FUNCTION DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputMode;
        private volatile uint _uhfCockpitMode; // OFF = 0
        private readonly ClickSpeedDetector _uhfModeClickSpeedDetector = new(8);
        private byte _skipUhfSmallFreqChange;
        private long _uhfThreadNowSyncing;
        private Thread _uhfSyncThread;
        private long _uhfDial1WaitingForFeedback;
        private long _uhfDial2WaitingForFeedback;
        private long _uhfDial3WaitingForFeedback;
        private long _uhfDial4WaitingForFeedback;

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
        private uint _vuhfCockpitDial1Frequency;
        private uint _vuhfCockpitDial2Frequency;
        private uint _vuhfCockpitDial3Frequency;
        private uint _vuhfCockpitDial4Frequency;
        private uint _vuhfSavedCockpitDial1Frequency;
        private uint _vuhfSavedCockpitDial2Frequency;
        private uint _vuhfSavedCockpitDial3Frequency;
        private uint _vuhfSavedCockpitDial4Frequency;
        private readonly object _lockVuhfBigFreqObject = new();
        private readonly object _lockVuhfDial3FreqObject = new();
        private readonly object _lockVuhfDial4FreqObject = new();
        private readonly object _lockVuhfPresetObject = new();
        private DCSBIOSOutput _vuhfDcsbiosOutputBigFrequencyNumber;
        private DCSBIOSOutput _vuhfDcsbiosOutputDial3FrequencyNumber;
        private DCSBIOSOutput _vuhfDcsbiosOutputDial4FrequencyNumber;
        private DCSBIOSOutput _vuhfDcsbiosOutputChannelFreqMode;  // 0 = PRESET
        private DCSBIOSOutput _vuhfDcsbiosOutputSelectedChannel;
        private volatile uint _vuhfCockpitFreqMode;
        private volatile uint _vuhfCockpitPresetChannel;
        private readonly ClickSpeedDetector _vuhfChannelClickSpeedDetector = new(8);
        private readonly ClickSpeedDetector _vuhfFreqModeClickSpeedDetector = new(6);

        private const string VUHF_1ST_DIAL_INCREASE = "RIO_VUHF_110_DIAL INC\n";
        private const string VUHF_1ST_DIAL_DECREASE = "RIO_VUHF_110_DIAL DEC\n";
        private const string VUHF_1ST_DIAL_NEUTRAL = "RIO_VUHF_110_DIAL 1\n";

        private const string VUHF_2ND_DIAL_COMMAND = "RIO_VUHF_1_DIAL ";
        private const string VUHF_2ND_DIAL_NEUTRAL = "RIO_VUHF_1_DIAL 1\n";

        private const string VUHF_3RD_DIAL_COMMAND = "RIO_VUHF_01_DIAL ";
        private const string VUHF_3RD_DIAL_NEUTRAL = "RIO_VUHF_01_DIAL 1\n";

        private const string VUHF_4TH_DIAL_INCREASE = "RIO_VUHF_025_DIAL INC\n";
        private const string VUHF_4TH_DIAL_DECREASE = "RIO_VUHF_025_DIAL DEC\n";
        private const string VUHF_4TH_DIAL_NEUTRAL = "RIO_VUHF_025_DIAL 1\n";

        private const string VUHF_PRESET_INCREASE = "RIO_VUHF_PRESETS INC\n";
        private const string VUHF_PRESET_DECREASE = "RIO_VUHF_PRESETS DEC\n";
        private const string VUHF_FREQ_MODE_INCREASE = "RIO_VUHF_FREQ_MODE INC\n";
        private const string VUHF_FREQ_MODE_DECREASE = "RIO_VUHF_FREQ_MODE DEC\n";

        private const string VUHF_MODE_INCREASE = "RIO_VUHF_MODE INC\n";
        private const string VUHF_MODE_DECREASE = "RIO_VUHF_MODE DEC\n";
        private DCSBIOSOutput _vuhfDcsbiosOutputMode;
        private volatile uint _vuhfCockpitMode; // OFF = 0
        private readonly ClickSpeedDetector _vuhfModeClickSpeedDetector = new(8);
        private byte _skipVuhfSmallFreqChange;
        private long _vuhfThreadNowSyncing;
        private Thread _vuhfSyncThread;
        private long _vuhfDial1WaitingForFeedback;
        private long _vuhfDial2WaitingForFeedback;
        private long _vuhfDial3WaitingForFeedback;
        private long _vuhfDial4WaitingForFeedback;

        /*PILOT TACAN NAV1*/
        // Tens dial 0-12 [step of 1]
        // Ones dial 0-9 [step of 1]
        // Last : X/Y [0,1]
        private int _pilotTacanTensFrequencyStandby = 6;
        private int _pilotTacanOnesFrequencyStandby = 5;
        private int _pilotTacanXYStandby;
        private int _pilotTacanSavedCockpitTensFrequency = 6;
        private int _pilotTacanSavedCockpitOnesFrequency = 5;
        private int _pilotTacanSavedCockpitXY;
        private readonly object _lockPilotTacanTensDialObject = new();
        private readonly object _lockPilotTacanOnesObject = new();
        private readonly object _lockPilotTacanXYDialObject = new();
        private DCSBIOSOutput _pilotTacanDcsbiosOutputTensDial;
        private DCSBIOSOutput _pilotTacanDcsbiosOutputOnesDial;
        private DCSBIOSOutput _pilotTacanDcsbiosOutputXYDial;
        private volatile uint _pilotTacanCockpitTensDialPos = 1;
        private volatile uint _pilotTacanCockpitOnesDialPos = 1;
        private volatile uint _pilotTacanCockpitXYDialPos = 1;
        private const string PILOT_TACAN_TENS_DIAL_COMMAND = "PLT_TACAN_DIAL_TENS ";
        private const string PILOT_TACAN_ONES_DIAL_COMMAND = "PLT_TACAN_DIAL_ONES ";
        private const string PILOT_TACAN_XY_DIAL_COMMAND = "PLT_TACAN_CHANNEL "; // X = 0 | Y = 1
        private Thread _pilotTacanSyncThread;
        private long _pilotTacanThreadNowSyncing;
        private long _pilotTacanTensWaitingForFeedback;
        private long _pilotTacanOnesWaitingForFeedback;
        private long _pilotTacanXYWaitingForFeedback;

        /*RIO TACAN NAV2*/
        // Tens dial 0-12 [step of 1]
        // Ones dial 0-9 [step of 1]
        // Last : X/Y [0,1]
        private int _rioTacanTensFrequencyStandby = 6;
        private int _rioTacanOnesFrequencyStandby = 5;
        private int _rioTacanXYStandby;
        private int _rioTacanSavedCockpitTensFrequency = 6;
        private int _rioTacanSavedCockpitOnesFrequency = 5;
        private int _rioTacanSavedCockpitXY;
        private readonly object _lockRioTacanTensDialObject = new();
        private readonly object _lockRioTacanOnesObject = new();
        private readonly object _lockRioTacanXYDialObject = new();
        private DCSBIOSOutput _rioTacanDcsbiosOutputTensDial;
        private DCSBIOSOutput _rioTacanDcsbiosOutputOnesDial;
        private DCSBIOSOutput _rioTacanDcsbiosOutputXYDial;
        private volatile uint _rioTacanCockpitTensDialPos = 1;
        private volatile uint _rioTacanCockpitOnesDialPos = 1;
        private volatile uint _rioTacanCockpitXYDialPos = 1;
        private const string RIO_TACAN_TENS_DIAL_COMMAND = "RIO_TACAN_DIAL_TENS ";
        private const string RIO_TACAN_ONES_DIAL_COMMAND = "RIO_TACAN_DIAL_ONES ";
        private const string RIO_TACAN_XY_DIAL_COMMAND = "RIO_TACAN_CHANNEL "; // X = 0 | Y = 1
        private Thread _rioTacanSyncThread;
        private long _rioTacanThreadNowSyncing;
        private long _rioTacanTensWaitingForFeedback;
        private long _rioTacanOnesWaitingForFeedback;
        private long _rioTacanXYWaitingForFeedback;

        /*RIO Link 4*/
        // Large Dial Hundreds 0 - 9  [step of 1]
        // Small Dial Tens     0 - 99 [step of 1]
        // Button Pressed + Large => ON/OFF/AUX
        private int _rioLink4HundredsFrequencyStandby;
        private int _rioLink4TensAndOnesFrequencyStandby;
        private int _rioLink4SavedCockpitHundredsFrequency;
        private int _rioLink4SavedCockpitTensAndOnesFrequency;
        private readonly object _lockRioLink4HundredsDial = new();
        private readonly object _lockRioLink4TensDial = new();
        private readonly object _lockRioLink4OnesDial = new();
        private DCSBIOSOutput _rioLink4DcsbiosOutputHundredsDial;
        private DCSBIOSOutput _rioLink4DcsbiosOutputTensDial;
        private DCSBIOSOutput _rioLink4DcsbiosOutputOnesDial;
        private DCSBIOSOutput _rioLink4DcsbiosOutputPowerSwitch;// RIO_DATALINK_PW
        private volatile uint _rioLink4HundredsCockpitFrequency;
        private volatile uint _rioLink4TensCockpitFrequency;
        private volatile uint _rioLink4OnesCockpitFrequency;
        private volatile uint _rioLink4CockpitPowerSwitch;
        private const string RIO_LINK4_HUNDREDS_DIAL_COMMAND = "RIO_DATALINK_FREQ_10 ";
        private const string RIO_LINK4_TENS_DIAL_COMMAND = "RIO_DATALINK_FREQ_1 ";
        private const string RIO_LINK4_ONES_DIAL_COMMAND = "RIO_DATALINK_FREQ_100 ";
        private const string RIO_LINK4_POWER_COMMAND_INC = "RIO_DATALINK_PW INC\n";
        private const string RIO_LINK4_POWER_COMMAND_DEC = "RIO_DATALINK_PW DEC\n";
        private Thread _rioLink4SyncThread;
        private long _rioLink4ThreadNowSyncing;
        private long _rioLinkHundredsWaitingForFeedback;
        private long _rioLinkTensWaitingForFeedback;
        private long _rioLinkOnesWaitingForFeedback;
        private readonly ClickSpeedDetector _rioLink4TensAndOnesClickSpeedDetector = new(20);
        private byte _skipRioLink4TensAndOnesFreqChange;

        private readonly object _lockShowFrequenciesOnPanelObject = new();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69F14B(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {}

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            TurnOffAllDisplays();
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownUHFThread = true;
                    _shutdownPilotTACANThread = true;
                    _shutdownRIOLink4Thread = true;
                    _shutdownRIOTACANThread = true;
                    _shutdownVUHFThread = true;
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void InitPanel()
        {
            CreateRadioKnobs();

            // UHF
            _uhfDcsbiosOutputChannelFreqMode = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_UHF1_FREQ_MODE");
            _uhfDcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_UHF_HIGH_FREQ");
            _uhfDcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_UHF_DIAL3_FREQ");
            _uhfDcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_UHF_DIAL4_FREQ");
            _uhfDcsbiosOutputMode = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_UHF1_FUNCTION");
            _uhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_UHF_REMOTE_DISP");

            // VUHF
            _vuhfDcsbiosOutputChannelFreqMode = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_FREQ_MODE");
            _vuhfDcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_HIGH_FREQ");
            _vuhfDcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_DIAL3_FREQ");
            _vuhfDcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_DIAL4_FREQ");
            _vuhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_VUHF_REMOTE_DISP");

            _vuhfDcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_HIGH_FREQ");
            _vuhfDcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_DIAL3_FREQ");
            _vuhfDcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_VUHF_DIAL4_FREQ");
            _vuhfDcsbiosOutputMode = DCSBIOSControlLocator.GetStringDCSBIOSOutput("RIO_VUHF_MODE");

            // Pilot & RIO TACAN
            _pilotTacanDcsbiosOutputTensDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_TACAN_DIAL_TENS");
            _pilotTacanDcsbiosOutputOnesDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_TACAN_DIAL_ONES");
            _pilotTacanDcsbiosOutputXYDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("PLT_TACAN_CHANNEL");
            _rioTacanDcsbiosOutputTensDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_TACAN_DIAL_TENS");
            _rioTacanDcsbiosOutputOnesDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_TACAN_DIAL_ONES");
            _rioTacanDcsbiosOutputXYDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_TACAN_CHANNEL");

            // RIO Link 4
            _rioLink4DcsbiosOutputHundredsDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_DATALINK_FREQ_10");
            _rioLink4DcsbiosOutputTensDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_DATALINK_FREQ_1");
            _rioLink4DcsbiosOutputOnesDial = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_DATALINK_FREQ_100");
            _rioLink4DcsbiosOutputPowerSwitch = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_DATALINK_PW");
            
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this); 
            StartListeningForHidPanelChanges();
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
            // UHF
            if (_uhfDcsbiosOutputBigFrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockUhfDialBigFreqObject)
                {
                    _uhfCockpitBigFrequency = _uhfDcsbiosOutputBigFrequencyNumber.LastUIntValue;
                    var asString = _uhfCockpitBigFrequency.ToString().PadLeft(3, '0');
                    _uhfCockpitDial1Frequency = uint.Parse(asString.Substring(0, 2));
                    _uhfCockpitDial2Frequency = uint.Parse(asString.Substring(2, 1));
                    Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
                    Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_uhfDcsbiosOutputDial3FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockUhfDial3FreqObject)
                {
                    _uhfCockpitDial3Frequency = _uhfDcsbiosOutputDial3FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_uhfDcsbiosOutputDial4FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockUhfDial4FreqObject)
                {
                    _uhfCockpitDial4Frequency = _uhfDcsbiosOutputDial4FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_uhfDcsbiosOutputChannelFreqMode.UIntValueHasChanged(e.Address, e.Data))
            {
                _uhfCockpitFreqMode = _uhfDcsbiosOutputChannelFreqMode.LastUIntValue;
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_uhfDcsbiosOutputSelectedChannel.UIntValueHasChanged(e.Address, e.Data))
            {
                _uhfCockpitPresetChannel = _uhfDcsbiosOutputSelectedChannel.LastUIntValue + 1;
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_uhfDcsbiosOutputMode.UIntValueHasChanged(e.Address, e.Data))
            {
                _uhfCockpitMode = _uhfDcsbiosOutputMode.LastUIntValue;
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            // VHF UHF
            if (_vuhfDcsbiosOutputBigFrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhfBigFreqObject)
                {
                    _vuhfCockpitBigFrequency = _vuhfDcsbiosOutputBigFrequencyNumber.LastUIntValue;
                    var asString = _vuhfCockpitBigFrequency.ToString().PadLeft(3, '0');
                    _vuhfCockpitDial1Frequency = uint.Parse(asString.Substring(0, 2));
                    _vuhfCockpitDial2Frequency = uint.Parse(asString.Substring(2, 1));
                    Interlocked.Exchange(ref _vuhfDial1WaitingForFeedback, 0);
                    Interlocked.Exchange(ref _vuhfDial2WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhfDcsbiosOutputDial3FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhfDial3FreqObject)
                {
                    _vuhfCockpitDial3Frequency = _vuhfDcsbiosOutputDial3FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _vuhfDial3WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhfDcsbiosOutputDial4FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhfDial4FreqObject)
                {
                    _vuhfCockpitDial4Frequency = _vuhfDcsbiosOutputDial4FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _vuhfDial4WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhfDcsbiosOutputChannelFreqMode.UIntValueHasChanged(e.Address, e.Data))
            {
                _vuhfCockpitFreqMode = _vuhfDcsbiosOutputChannelFreqMode.LastUIntValue;
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_vuhfDcsbiosOutputMode.UIntValueHasChanged(e.Address, e.Data))
            {
                _vuhfCockpitMode = _vuhfDcsbiosOutputMode.LastUIntValue;
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            // Pilot TACAN
            if (_pilotTacanDcsbiosOutputTensDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _pilotTacanCockpitTensDialPos = _pilotTacanDcsbiosOutputTensDial.LastUIntValue;
                Interlocked.Exchange(ref _pilotTacanTensWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_pilotTacanDcsbiosOutputOnesDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _pilotTacanCockpitOnesDialPos = _pilotTacanDcsbiosOutputOnesDial.LastUIntValue;
                Interlocked.Exchange(ref _pilotTacanOnesWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_pilotTacanDcsbiosOutputXYDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _pilotTacanCockpitXYDialPos = _pilotTacanDcsbiosOutputXYDial.LastUIntValue;
                Interlocked.Exchange(ref _pilotTacanXYWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            // RIO TACAN
            if (_rioTacanDcsbiosOutputTensDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioTacanCockpitTensDialPos = _rioTacanDcsbiosOutputTensDial.LastUIntValue;
                Interlocked.Exchange(ref _rioTacanTensWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_rioTacanDcsbiosOutputOnesDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioTacanCockpitOnesDialPos = _rioTacanDcsbiosOutputOnesDial.LastUIntValue;
                Interlocked.Exchange(ref _rioTacanOnesWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_rioTacanDcsbiosOutputXYDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioTacanCockpitXYDialPos = _rioTacanDcsbiosOutputXYDial.LastUIntValue;
                Interlocked.Exchange(ref _rioTacanXYWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            // RIO Link 4
            if (_rioLink4DcsbiosOutputHundredsDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioLink4HundredsCockpitFrequency = _rioLink4DcsbiosOutputHundredsDial.LastUIntValue;
                Interlocked.Exchange(ref _rioLinkHundredsWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_rioLink4DcsbiosOutputTensDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioLink4TensCockpitFrequency = _rioLink4DcsbiosOutputTensDial.LastUIntValue;
                Interlocked.Exchange(ref _rioLinkTensWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_rioLink4DcsbiosOutputOnesDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioLink4OnesCockpitFrequency = _rioLink4DcsbiosOutputOnesDial.LastUIntValue;
                Interlocked.Exchange(ref _rioLinkOnesWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_rioLink4DcsbiosOutputPowerSwitch.UIntValueHasChanged(e.Address, e.Data))
            {
                _rioLink4CockpitPowerSwitch = _rioLink4DcsbiosOutputPowerSwitch.LastUIntValue;
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (_vuhfDcsbiosOutputSelectedChannel.StringValueHasChanged(e.Address, e.StringData))
                {
                    try
                    {
                        lock (_lockVuhfPresetObject)
                        {
                            if (!uint.TryParse(e.StringData.Substring(0, 7), out var tmpUint))
                            {
                                return;
                            }

                            if (tmpUint != _vuhfCockpitPresetChannel)
                            {
                                _vuhfCockpitPresetChannel = tmpUint;
                                Interlocked.Add(ref _doUpdatePanelLCD, 5);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                else if (_uhfDcsbiosOutputSelectedChannel.StringValueHasChanged(e.Address, e.StringData))
                {
                    try
                    {
                        lock (_lockUhfPresetObject)
                        {
                            if (!uint.TryParse(e.StringData.Substring(0, 7), out var tmpUint))
                            {
                                return;
                            }

                            if (tmpUint != _uhfCockpitPresetChannel)
                            {
                                _uhfCockpitPresetChannel = tmpUint;
                                Interlocked.Add(ref _doUpdatePanelLCD, 5);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Strange values from DCS-BIOS
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF14B knob)
        {

            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH))
            {
                // Don't do anything on the very first button press as the panel sends ALL
                // switches when it is manipulated the first time
                // This would cause unintended sync.
                return;
            }

            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                // Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }

            switch (knob)
            {
                case RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH:
                    {
                        if (_ignoreUpperButtonOnce)
                        {
                            // Don't do anything on the very first button press as the panel sends ALL
                            // switches when it is manipulated the first time
                            // This would cause unintended sync.
                            _ignoreUpperButtonOnce = false;
                            return;
                        }

                        switch (_currentUpperRadioMode)
                        {
                            case CurrentF14RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SendUHFToDCSBIOS();
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }

                            case CurrentF14RadioMode.VUHF:
                                {
                                    if (_vuhfCockpitMode != 0 && !VuhfPresetSelected())
                                    {
                                        SendVUHFToDCSBIOS();
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
                            // Don't do anything on the very first button press as the panel sends ALL
                            // switches when it is manipulated the first time
                            // This would cause unintended sync.
                            _ignoreLowerButtonOnce = false;
                            return;
                        }

                        switch (_currentLowerRadioMode)
                        {
                            case CurrentF14RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SendUHFToDCSBIOS();
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }

                            case CurrentF14RadioMode.VUHF:
                                {
                                    if (_vuhfCockpitMode != 0 && !VuhfPresetSelected())
                                    {
                                        SendVUHFToDCSBIOS();
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

        private void SendUHFToDCSBIOS()
        {
            if (UHFNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyUhf();
            var frequencyAsString = _uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(3, '0');

            int desiredDial1Value;
            int desiredDial2Value;
            int desiredDial3Value;
            int desiredDial4Value;

            if (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture) == 2)
            {
                // 30.025
                desiredDial1Value = int.Parse(frequencyAsString.Substring(0, 1));
                desiredDial2Value = int.Parse(frequencyAsString.Substring(1, 1));
                desiredDial3Value = int.Parse(frequencyAsString.Substring(3, 1));
                desiredDial4Value = int.Parse(frequencyAsString.Substring(4, 2));
            }
            else
            {
                // 151.950
                // This is a quick and dirty fix. We should not be here when dealing with VHF FM because the range is 30.000 to 76.000 MHz.
                // Set freq to 45.000 MHz (sort of an reset)
                desiredDial1Value = int.Parse(frequencyAsString.Substring(0, 2));
                desiredDial2Value = int.Parse(frequencyAsString.Substring(2, 1));
                desiredDial3Value = int.Parse(frequencyAsString.Substring(4, 1));
                desiredDial4Value = int.Parse(frequencyAsString.Substring(5, 2));
            }

            _shutdownUHFThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownUHFThread = false;
            _uhfSyncThread = new Thread(() => UHFSynchThreadMethod(desiredDial1Value, desiredDial2Value, desiredDial3Value, desiredDial4Value));
            _uhfSyncThread.Start();
        }

        private volatile bool _shutdownUHFThread;
        private void UHFSynchThreadMethod(int desiredValueDial1, int desiredValueDial2, int desiredValueDial3, int desiredValueDial4)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _uhfThreadNowSyncing, 1);
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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _uhfDial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _uhfDial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _uhfDial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial4Timeout))
                        {
                            ResetWaitingForFeedBack(ref _uhfDial4WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _uhfDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialBigFreqObject)
                            {
                                if (_uhfCockpitDial1Frequency != desiredValueDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_uhfCockpitDial1Frequency < desiredValueDial1 ? UHF_1ST_DIAL_INCREASE : UHF_1ST_DIAL_DECREASE);
                                    DCSBIOS.Send(UHF_1ST_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _uhfDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialBigFreqObject)
                            {
                                if (_uhfCockpitDial2Frequency != desiredValueDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(UHF_2ND_DIAL_COMMAND + GetCommandDirection10Dial(desiredValueDial2, _uhfCockpitDial2Frequency));
                                    DCSBIOS.Send(UHF_2ND_DIAL_NEUTRAL);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _uhfDial3WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDial3FreqObject)
                            {
                                if (_uhfCockpitDial3Frequency != desiredValueDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(UHF_3RD_DIAL_COMMAND + GetCommandDirection10Dial(desiredValueDial3, _uhfCockpitDial3Frequency));
                                    DCSBIOS.Send(UHF_3RD_DIAL_NEUTRAL);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 1);
                                }
                            }
                            Reset(ref dial3Timeout);
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _uhfDial4WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDial4FreqObject)
                            {
                                if (_uhfCockpitDial4Frequency != desiredValueDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_uhfCockpitDial4Frequency < desiredValueDial4 ? UHF_4TH_DIAL_INCREASE : UHF_4TH_DIAL_DECREASE);
                                    DCSBIOS.Send(UHF_4TH_DIAL_NEUTRAL);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 1);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            dial4SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS

                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownUHFThread);
                    SwapCockpitStandbyFrequencyUhf();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _uhfThreadNowSyncing, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendVUHFToDCSBIOS()
        {
            if (VUHFNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyVuhf();
            var frequencyAsString = _vuhfBigFrequencyStandby + "." + _vuhfSmallFrequencyStandby.ToString().PadLeft(3, '0');

            int desiredDial1Value;
            int desiredDial2Value;
            int desiredDial3Value;
            int desiredDial4Value;

            if (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture) == 2)
            {
                // 30.025
                desiredDial1Value = int.Parse(frequencyAsString.Substring(0, 1));
                desiredDial2Value = int.Parse(frequencyAsString.Substring(1, 1));
                desiredDial3Value = int.Parse(frequencyAsString.Substring(3, 1));
                desiredDial4Value = int.Parse(frequencyAsString.Substring(4, 2));
            }
            else
            {
                // 151.950
                // This is a quick and dirty fix. We should not be here when dealing with VHF FM because the range is 30.000 to 76.000 MHz.
                // Set freq to 45.000 MHz (sort of an reset)
                desiredDial1Value = int.Parse(frequencyAsString.Substring(0, 2));
                desiredDial2Value = int.Parse(frequencyAsString.Substring(2, 1));
                desiredDial3Value = int.Parse(frequencyAsString.Substring(4, 1));
                desiredDial4Value = int.Parse(frequencyAsString.Substring(5, 2));
            }

            _shutdownVUHFThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVUHFThread = false;
            _vuhfSyncThread = new Thread(() => VUHFSynchThreadMethod(desiredDial1Value, desiredDial2Value, desiredDial3Value, desiredDial4Value));
            _vuhfSyncThread.Start();
        }

        private volatile bool _shutdownVUHFThread;
        private void VUHFSynchThreadMethod(int desiredValueDial1, int desiredValueDial2, int desiredValueDial3, int desiredValueDial4)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _vuhfThreadNowSyncing, 1);
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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhfDial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhfDial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhfDial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial4Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhfDial4WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _vuhfDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhfBigFreqObject)
                            {
                                if (_vuhfCockpitDial1Frequency != desiredValueDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhfCockpitDial1Frequency < desiredValueDial1 ? VUHF_1ST_DIAL_INCREASE : VUHF_1ST_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF_1ST_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhfDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhfDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhfBigFreqObject)
                            {
                                if (_vuhfCockpitDial2Frequency != desiredValueDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(VUHF_2ND_DIAL_COMMAND + GetCommandDirection10Dial(desiredValueDial2, _vuhfCockpitDial2Frequency));
                                    DCSBIOS.Send(VUHF_2ND_DIAL_NEUTRAL);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vuhfDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhfDial3WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhfDial3FreqObject)
                            {
                                if (_vuhfCockpitDial3Frequency != desiredValueDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(VUHF_3RD_DIAL_COMMAND + GetCommandDirection10Dial(desiredValueDial3, _vuhfCockpitDial3Frequency));
                                    DCSBIOS.Send(VUHF_3RD_DIAL_NEUTRAL);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _vuhfDial3WaitingForFeedback, 1);
                                }
                            }
                            Reset(ref dial3Timeout);
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhfDial4WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhfDial4FreqObject)
                            {
                                if (_vuhfCockpitDial4Frequency != desiredValueDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhfCockpitDial4Frequency < desiredValueDial4 ? VUHF_4TH_DIAL_INCREASE : VUHF_4TH_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF_4TH_DIAL_NEUTRAL);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vuhfDial4WaitingForFeedback, 1);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            dial4SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownVUHFThread);
                    SwapCockpitStandbyFrequencyVuhf();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _vuhfThreadNowSyncing, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
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
            _shutdownRIOLink4Thread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownRIOLink4Thread = false;
            _rioLink4SyncThread = new Thread(() => RioDatalink4SynchThreadMethod(_rioLink4HundredsFrequencyStandby, dial2, dial3));
            _rioLink4SyncThread.Start();
        }

        private volatile bool _shutdownRIOLink4Thread;
        private void RioDatalink4SynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _rioLink4ThreadNowSyncing, 1);

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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _rioLinkHundredsWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _rioLinkTensWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _rioLinkOnesWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _rioLinkHundredsWaitingForFeedback) == 0)
                        {
                            lock (_lockRioLink4HundredsDial)
                            {
                                if (_rioLink4HundredsCockpitFrequency != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = RIO_LINK4_HUNDREDS_DIAL_COMMAND + (_rioLink4HundredsCockpitFrequency < desiredPositionDial1 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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
                                    var str = RIO_LINK4_TENS_DIAL_COMMAND + (_rioLink4TensCockpitFrequency < desiredPositionDial2 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
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
                                    var str = RIO_LINK4_ONES_DIAL_COMMAND + (_rioLink4OnesCockpitFrequency < desiredPositionDial3 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime)) && !_shutdownRIOLink4Thread);
                    SwapCockpitStandbyFrequencyDataLink4();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _rioLink4ThreadNowSyncing, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendPilotTacanToDCSBIOS()
        {
            if (PilotTacanNowSyncing())
            {
                return;
            }

            SavePilotCockpitFrequencyTacan();

            // TACAN  00X/Y --> 129X/Y
            // Frequency selector 1      LEFT
            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            // Frequency selector 2      MIDDLE
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3      RIGHT
            // X=0 / Y=1

            // 120X
            // #1 = 12  (position = value)
            // #2 = 0   (position = value)
            // #3 = 1   (position = value)
            _shutdownPilotTACANThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownPilotTACANThread = false;
            _pilotTacanSyncThread = new Thread(() => PilotTacanSynchThreadMethod(_pilotTacanTensFrequencyStandby, _pilotTacanOnesFrequencyStandby, _pilotTacanXYStandby));
            _pilotTacanSyncThread.Start();
        }

        private volatile bool _shutdownPilotTACANThread;
        private void PilotTacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _pilotTacanThreadNowSyncing, 1);

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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _pilotTacanTensWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _pilotTacanOnesWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _pilotTacanXYWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _pilotTacanTensWaitingForFeedback) == 0)
                        {
                            lock (_lockPilotTacanTensDialObject)
                            {
                                if (_pilotTacanCockpitTensDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = PILOT_TACAN_TENS_DIAL_COMMAND + (_pilotTacanCockpitTensDialPos < desiredPositionDial1 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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

                                    var str = PILOT_TACAN_ONES_DIAL_COMMAND + (_pilotTacanCockpitOnesDialPos < desiredPositionDial2 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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

                                    var str = PILOT_TACAN_XY_DIAL_COMMAND + (_pilotTacanCockpitXYDialPos < desiredPositionDial3 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime)) && !_shutdownPilotTACANThread);
                    SwapPilotCockpitStandbyFrequencyTacan();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _pilotTacanThreadNowSyncing, 0);
            }

            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendRioTacanToDCSBIOS()
        {
            if (RioTacanNowSyncing())
            {
                return;
            }

            SaveRioCockpitFrequencyTacan();
            _shutdownRIOTACANThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownRIOTACANThread = false;
            _rioTacanSyncThread = new Thread(() => RioTacanSynchThreadMethod(_rioTacanTensFrequencyStandby, _rioTacanOnesFrequencyStandby, _rioTacanXYStandby));
            _rioTacanSyncThread.Start();
        }

        private volatile bool _shutdownRIOTACANThread;
        private void RioTacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _rioTacanThreadNowSyncing, 1);

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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _rioTacanTensWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _rioTacanOnesWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _rioTacanXYWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _rioTacanTensWaitingForFeedback) == 0)
                        {

                            lock (_lockRioTacanTensDialObject)
                            {
                                if (_rioTacanCockpitTensDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = RIO_TACAN_TENS_DIAL_COMMAND + (_rioTacanCockpitTensDialPos < desiredPositionDial1 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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

                                    var str = RIO_TACAN_ONES_DIAL_COMMAND + (_rioTacanCockpitOnesDialPos < desiredPositionDial2 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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

                                    var str = RIO_TACAN_XY_DIAL_COMMAND + (_rioTacanCockpitXYDialPos < desiredPositionDial3 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime)) && !_shutdownRIOTACANThread);
                    SwapRioCockpitStandbyFrequencyTacan();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _rioTacanThreadNowSyncing, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _uhfCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _uhfCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_uhfCockpitMode != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _uhfCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + (((double)_uhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }

                    case CurrentF14RadioMode.VUHF:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vuhfCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vuhfCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_vuhfCockpitMode != 0 && VuhfPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vuhfCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                                    SetPZ69DisplayBytesDefault(ref bytes, _vuhfBigFrequencyStandby + (((double)_vuhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }

                    case CurrentF14RadioMode.PLT_TACAN:
                        {
                            // TACAN  00X/Y --> 129X/Y
                            // Frequency selector 1      LEFT
                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                            // Frequency selector 2      MIDDLE
                            // 0 1 2 3 4 5 6 7 8 9

                            // Frequency selector 3      RIGHT
                            // X=0 / Y=1
                            string frequencyAsString;
                            lock (_lockPilotTacanTensDialObject)
                            {
                                lock (_lockPilotTacanOnesObject)
                                {
                                    frequencyAsString = _pilotTacanCockpitTensDialPos + _pilotTacanCockpitOnesDialPos.ToString();
                                }
                            }

                            frequencyAsString += ".";
                            lock (_lockPilotTacanXYDialObject)
                            {
                                frequencyAsString += _pilotTacanCockpitXYDialPos;
                            }
                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_pilotTacanTensFrequencyStandby + _pilotTacanOnesFrequencyStandby.ToString() + "." + _pilotTacanXYStandby, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentF14RadioMode.RIO_TACAN:
                        {
                            string frequencyAsString;
                            lock (_lockRioTacanTensDialObject)
                            {
                                lock (_lockRioTacanOnesObject)
                                {
                                    frequencyAsString = _rioTacanCockpitTensDialPos + _rioTacanCockpitOnesDialPos.ToString();
                                }
                            }

                            frequencyAsString += ".";
                            lock (_lockRioTacanXYDialObject)
                            {
                                frequencyAsString += _rioTacanCockpitXYDialPos;
                            }
                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_rioTacanTensFrequencyStandby + _rioTacanOnesFrequencyStandby.ToString() + "." + _rioTacanXYStandby, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentF14RadioMode.LINK4:
                        {
                            if (_rioLink4CockpitPowerSwitch == 0)
                            {
                                // OFF
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _rioLink4CockpitPowerSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                string frequencyAsString;
                                lock (_lockRioLink4HundredsDial)
                                {
                                    lock (_lockRioLink4TensDial)
                                    {
                                        lock (_lockRioLink4OnesDial)
                                        {
                                            frequencyAsString =
                                                _rioLink4HundredsCockpitFrequency +
                                                _rioLink4TensCockpitFrequency.ToString() +
                                                _rioLink4OnesCockpitFrequency;
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, frequencyAsString.PadLeft(5, ' '), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, _rioLink4HundredsFrequencyStandby + _rioLink4TensAndOnesFrequencyStandby.ToString().PadLeft(2, '0'), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            break;
                        }

                    case CurrentF14RadioMode.NO_USE:
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
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _uhfCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _uhfCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_uhfCockpitMode != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _uhfCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + (((double)_uhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }

                    case CurrentF14RadioMode.VUHF:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vuhfCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vuhfCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_vuhfCockpitMode != 0 && VuhfPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vuhfCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
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
                                    SetPZ69DisplayBytesDefault(ref bytes, _vuhfBigFrequencyStandby + (((double)_vuhfSmallFrequencyStandby) / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }

                    case CurrentF14RadioMode.PLT_TACAN:
                        {
                            string frequencyAsString;
                            lock (_lockPilotTacanTensDialObject)
                            {
                                lock (_lockPilotTacanOnesObject)
                                {
                                    frequencyAsString = _pilotTacanCockpitTensDialPos + _pilotTacanCockpitOnesDialPos.ToString();
                                }
                            }

                            frequencyAsString += ".";
                            lock (_lockPilotTacanXYDialObject)
                            {
                                frequencyAsString += _pilotTacanCockpitXYDialPos;
                            }
                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_pilotTacanTensFrequencyStandby + _pilotTacanOnesFrequencyStandby.ToString() + "." + _pilotTacanXYStandby, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentF14RadioMode.RIO_TACAN:
                        {
                            string frequencyAsString;
                            lock (_lockRioTacanTensDialObject)
                            {
                                lock (_lockRioTacanOnesObject)
                                {
                                    frequencyAsString = _rioTacanCockpitTensDialPos + _rioTacanCockpitOnesDialPos.ToString();
                                }
                            }

                            frequencyAsString += ".";
                            lock (_lockRioTacanXYDialObject)
                            {
                                frequencyAsString += _rioTacanCockpitXYDialPos;
                            }
                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_rioTacanTensFrequencyStandby + _rioTacanOnesFrequencyStandby.ToString() + "." + _rioTacanXYStandby, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentF14RadioMode.LINK4:
                        {
                            if (_rioLink4CockpitPowerSwitch == 0)
                            {
                                // OFF
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _rioLink4CockpitPowerSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                string frequencyAsString;
                                lock (_lockRioLink4HundredsDial)
                                {
                                    lock (_lockRioLink4TensDial)
                                    {
                                        lock (_lockRioLink4OnesDial)
                                        {
                                            frequencyAsString =
                                                _rioLink4HundredsCockpitFrequency +
                                                _rioLink4TensCockpitFrequency.ToString() +
                                                _rioLink4OnesCockpitFrequency;
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, frequencyAsString.PadLeft(5, ' '), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, _rioLink4HundredsFrequencyStandby + _rioLink4TensAndOnesFrequencyStandby.ToString().PadLeft(2, '0'), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            break;
                        }

                    case CurrentF14RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                }
                SendLCDData(bytes);
            }
            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        private string GetUHFCockpitFrequencyAsString()
        {
            string frequencyAsString;
            lock (_lockUhfDialBigFreqObject)
            {
                lock (_lockUhfDial3FreqObject)
                {
                    lock (_lockUhfDial4FreqObject)
                    {
                        frequencyAsString = _uhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
                        frequencyAsString += ".";
                        frequencyAsString += _uhfCockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += _uhfCockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');

                        // 225.000 7 characters
                    }
                }
            }
            return frequencyAsString;
        }

        private string GetVUHFCockpitFrequencyAsString()
        {
            string frequencyAsString;
            lock (_lockVuhfBigFreqObject)
            {
                lock (_lockVuhfDial3FreqObject)
                {
                    lock (_lockVuhfDial4FreqObject)
                    {
                        frequencyAsString = _vuhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += ".";
                        frequencyAsString += _vuhfCockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += _vuhfCockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');

                        // 225.000 7 characters
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
                var radioPanelKnobF14 = (RadioPanelKnobF14B)o;
                if (radioPanelKnobF14.IsOn)
                {
                    switch (radioPanelKnobF14.RadioPanelPZ69Knob)
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
            Interlocked.Increment(ref _doUpdatePanelLCD);
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
            // Crude fix if any freqs are outside the valid boundaries

            // UHF
            // 225.00 - 399.975
            if (_uhfBigFrequencyStandby < 225)
            {
                _uhfBigFrequencyStandby = 399;
            }

            if (_uhfBigFrequencyStandby > 399)
            {
                _uhfBigFrequencyStandby = 225;
            }

            // TACAN
            // 00X/Y - 129X/Y
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

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
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
                                    _currentUpperRadioMode = CurrentF14RadioMode.NO_USE;
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
                                    _currentLowerRadioMode = CurrentF14RadioMode.NO_USE;
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
                                        // Do not sync if user has pressed the button to configure the radio
                                        // Do when user releases button
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
                                        // Do not sync if user has pressed the button to configure the radio
                                        // Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH);
                                    }

                                    _lowerButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                    }

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F14B, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                    }
                }
                AdjustFrequency(hashSet);
            }
        }

        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }
        

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobF14B.GetRadioPanelKnobs();
        }

        private void SaveCockpitFrequencyUhf()
        {
            lock (_lockUhfDialBigFreqObject)
            {
                lock (_lockUhfDial3FreqObject)
                {
                    lock (_lockUhfDial4FreqObject)
                    {
                        _uhfSavedCockpitDial1Frequency = _uhfCockpitDial1Frequency;
                        _uhfSavedCockpitDial2Frequency = _uhfCockpitDial2Frequency;
                        _uhfSavedCockpitDial3Frequency = _uhfCockpitDial3Frequency;
                        _uhfSavedCockpitDial4Frequency = _uhfCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyUhf()
        {
            lock (_lockUhfDialBigFreqObject)
            {
                lock (_lockUhfDial3FreqObject)
                {
                    lock (_lockUhfDial4FreqObject)
                    {
                        _uhfBigFrequencyStandby = _uhfSavedCockpitDial1Frequency * 10 + _uhfSavedCockpitDial2Frequency;
                        _uhfSmallFrequencyStandby = _uhfSavedCockpitDial3Frequency * 100 + _uhfSavedCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SaveCockpitFrequencyVuhf()
        {
            lock (_lockVuhfBigFreqObject)
            {
                lock (_lockVuhfDial3FreqObject)
                {
                    lock (_lockVuhfDial4FreqObject)
                    {
                        _vuhfSavedCockpitDial1Frequency = _vuhfCockpitDial1Frequency;
                        _vuhfSavedCockpitDial2Frequency = _vuhfCockpitDial2Frequency;
                        _vuhfSavedCockpitDial3Frequency = _vuhfCockpitDial3Frequency;
                        _vuhfSavedCockpitDial4Frequency = _vuhfCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVuhf()
        {
            lock (_lockVuhfBigFreqObject)
            {
                lock (_lockVuhfDial3FreqObject)
                {
                    lock (_lockVuhfDial4FreqObject)
                    {
                        _vuhfBigFrequencyStandby = _vuhfSavedCockpitDial1Frequency * 10 + _vuhfSavedCockpitDial2Frequency;
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
            // Large dial 0-12 [step of 1]
            // Small dial 0-9 [step of 1]
            // Last : X/Y [0,1]
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
            // Large dial 0-12 [step of 1]
            // Small dial 0-9 [step of 1]
            // Last : X/Y [0,1]
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
            return Interlocked.Read(ref _pilotTacanThreadNowSyncing) > 0;
        }

        private bool Link4NowSyncing()
        {
            return Interlocked.Read(ref _rioLink4ThreadNowSyncing) > 0;
        }

        private bool RioTacanNowSyncing()
        {
            return Interlocked.Read(ref _rioTacanThreadNowSyncing) > 0;
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }

        private bool UHFNowSyncing()
        {
            return Interlocked.Read(ref _uhfThreadNowSyncing) > 0;
        }

        private bool VUHFNowSyncing()
        {
            return Interlocked.Read(ref _vuhfThreadNowSyncing) > 0;
        }

        private static string GetCommandDirection10Dial(int desiredDialPosition, uint actualDialPosition)
        {
            var counterUp = 0;
            var counterDown = 0;

            var tmpActual = (int)actualDialPosition;
            while (true)
            {
                counterUp++;
                tmpActual++;
                if (tmpActual > 9)
                {
                    tmpActual = 0;
                }

                if (tmpActual == desiredDialPosition)
                {
                    break;
                }
            }

            tmpActual = (int)actualDialPosition;
            while (true)
            {
                counterDown++;
                tmpActual--;
                if (tmpActual < 0)
                {
                    tmpActual = 9;
                }

                if (tmpActual == desiredDialPosition)
                {
                    break;
                }
            }
            return counterUp > counterDown ? DCSBIOS_DECREASE_COMMAND : DCSBIOS_INCREASE_COMMAND;
        }
    }
}

