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

    /// <summary>
    /// Pre-programmed radio panel for the A-10C.
    /// </summary>
    public class RadioPanelPZ69A10C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentA10RadioMode
        {
            UHF,
            VHFFM,
            VHFAM,
            TACAN,
            ILS
        }

        private CurrentA10RadioMode _currentUpperRadioMode = CurrentA10RadioMode.UHF;
        private CurrentA10RadioMode _currentLowerRadioMode = CurrentA10RadioMode.UHF;

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        /*A-10C AN/ARC-186(V) VHF AM Radio 1*/
        // Large dial 116-151 [step of 1]
        // Small dial 0.00-0.97 [step of x.x[0 2 5 7]
        private double _vhfAmBigFrequencyStandby = 116;
        private double _vhfAmSmallFrequencyStandby;
        private double _vhfAmSavedCockpitBigFrequency;
        private double _vhfAmSavedCockpitSmallFrequency;
        private readonly object _lockVhfAmDialsObject1 = new();
        private readonly object _lockVhfAmDialsObject2 = new();
        private readonly object _lockVhfAmDialsObject3 = new();
        private readonly object _lockVhfAmDialsObject4 = new();
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial4;
        private volatile uint _vhfAmCockpitFreq1DialPos = 1;
        private volatile uint _vhfAmCockpitFreq2DialPos = 1;
        private volatile uint _vhfAmCockpitFreq3DialPos = 1;
        private volatile uint _vhfAmCockpitFreq4DialPos = 1;
        private const string VHF_AM_FREQ_1DIAL_COMMAND = "VHFAM_FREQ1 ";
        private const string VHF_AM_FREQ_2DIAL_COMMAND = "VHFAM_FREQ2 ";
        private const string VHF_AM_FREQ_3DIAL_COMMAND = "VHFAM_FREQ3 ";
        private const string VHF_AM_FREQ_4DIAL_COMMAND = "VHFAM_FREQ4 ";
        private Thread _vhfAmSyncThread;
        private long _vhfAmThreadNowSynching;
        private long _vhfAmDial1WaitingForFeedback;
        private long _vhfAmDial2WaitingForFeedback;
        private long _vhfAmDial3WaitingForFeedback;
        private long _vhfAmDial4WaitingForFeedback;
        private const string VHF_AM_PRESET_INCREASE = "VHFAM_PRESET INC\n";
        private const string VHF_AM_PRESET_DECREASE = "VHFAM_PRESET DEC\n";
        private const string VHF_AM_FREQ_MODE_INCREASE = "VHFAM_FREQEMER INC\n";
        private const string VHF_AM_FREQ_MODE_DECREASE = "VHFAM_FREQEMER DEC\n";
        private DCSBIOSOutput _vhfAmDcsbiosOutputChannelFreqMode;  // 3 = PRESET
        private DCSBIOSOutput _vhfAmDcsbiosOutputSelectedChannel;
        private volatile uint _vhfAmCockpitFreqMode;
        private volatile uint _vhfAmCockpitPresetChannel;
        private readonly ClickSpeedDetector _vhfAmChannelClickSpeedDetector = new(8);
        private readonly ClickSpeedDetector _vhfAmFreqModeClickSpeedDetector = new(6);

        private const string VHF_AM_MODE_INCREASE = "VHFAM_MODE INC\n";
        private const string VHF_AM_MODE_DECREASE = "VHFAM_MODE DEC\n";
        private DCSBIOSOutput _vhfAmDcsbiosOutputMode;  // VHFAM_MODE
        private volatile uint _vhfAmCockpitMode; // OFF = 0
        private readonly ClickSpeedDetector _vhfAmModeClickSpeedDetector = new(8);

        /*A-10C AN/ARC-164 UHF Radio 2*/
        // Large dial 225-399 [step of 1]
        // Small dial 0.00-0.97 [step of 0 2 5 7]
        private double _uhfBigFrequencyStandby = 299;
        private double _uhfSmallFrequencyStandby;
        private double _uhfSavedCockpitBigFrequency;
        private double _uhfSavedCockpitSmallFrequency;
        private readonly object _lockUhfDialsObject1 = new();
        private readonly object _lockUhfDialsObject2 = new();
        private readonly object _lockUhfDialsObject3 = new();
        private readonly object _lockUhfDialsObject4 = new();
        private readonly object _lockUhfDialsObject5 = new();
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
        private const string UHF_FREQ_1DIAL_COMMAND = "UHF_100MHZ_SEL ";		// "2" "3" "A"
        private const string UHF_FREQ_2DIAL_COMMAND = "UHF_10MHZ_SEL ";		// 0 1 2 3 4 5 6 7 8 9
        private const string UHF_FREQ_3DIAL_COMMAND = "UHF_1MHZ_SEL ";			// 0 1 2 3 4 5 6 7 8 9
        private const string UHF_FREQ_4DIAL_COMMAND = "UHF_POINT1MHZ_SEL ";    // 0 1 2 3 4 5 6 7 8 9
        private const string UHF_FREQ_5DIAL_COMMAND = "UHF_POINT25_SEL ";		// "00" "25" "50" "75"
        private Thread _uhfSyncThread;
        private long _uhfThreadNowSynching;
        private long _uhfDial1WaitingForFeedback;
        private long _uhfDial2WaitingForFeedback;
        private long _uhfDial3WaitingForFeedback;
        private long _uhfDial4WaitingForFeedback;
        private long _uhfDial5WaitingForFeedback;
        private const string UHF_PRESET_INCREASE = "UHF_PRESET_SEL INC\n";
        private const string UHF_PRESET_DECREASE = "UHF_PRESET_SEL DEC\n";
        private const string UHF_FREQ_MODE_INCREASE = "UHF_MODE INC\n";
        private const string UHF_FREQ_MODE_DECREASE = "UHF_MODE DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputFreqMode;  // 1 = PRESET
        private DCSBIOSOutput _uhfDcsbiosOutputSelectedChannel;
        private volatile uint _uhfCockpitFreqMode;
        private volatile uint _uhfCockpitPresetChannel;
        private readonly ClickSpeedDetector _uhfChannelClickSpeedDetector = new(8);
        private readonly ClickSpeedDetector _uhfFreqModeClickSpeedDetector = new(6);

        private const string UHF_FUNCTION_INCREASE = "UHF_FUNCTION INC\n";
        private const string UHF_FUNCTION_DECREASE = "UHF_FUNCTION DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputFunction;  // UHF_FUNCTION
        private volatile uint _uhfCockpitMode;
        private readonly ClickSpeedDetector _uhfFunctionClickSpeedDetector = new(8);

        /*A-10C AN/ARC-186(V) VHF FM Radio 3*/
        // Large dial 30-76 [step of 1]
        // Small dial 000 - 975 [0 2 5 7]
        private uint _vhfFmBigFrequencyStandby = 45;
        private uint _vhfFmSmallFrequencyStandby;
        private uint _vhfFmSavedCockpitBigFrequency;
        private uint _vhfFmSavedCockpitSmallFrequency;
        private readonly object _lockVhfFmDialsObject1 = new();
        private readonly object _lockVhfFmDialsObject2 = new();
        private readonly object _lockVhfFmDialsObject3 = new();
        private readonly object _lockVhfFmDialsObject4 = new();
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial4;
        private volatile uint _vhfFmCockpitFreq1DialPos = 1;
        private volatile uint _vhfFmCockpitFreq2DialPos = 1;
        private volatile uint _vhfFmCockpitFreq3DialPos = 1;
        private volatile uint _vhfFmCockpitFreq4DialPos = 1;
        private const string VHF_FM_FREQ_1DIAL_COMMAND = "VHFFM_FREQ1 ";
        private const string VHF_FM_FREQ_2DIAL_COMMAND = "VHFFM_FREQ2 ";
        private const string VHF_FM_FREQ_3DIAL_COMMAND = "VHFFM_FREQ3 ";
        private const string VHF_FM_FREQ_4DIAL_COMMAND = "VHFFM_FREQ4 ";
        private Thread _vhfFmSyncThread;
        private long _vhfFmThreadNowSynching;
        private long _vhfFmDial1WaitingForFeedback;
        private long _vhfFmDial2WaitingForFeedback;
        private long _vhfFmDial3WaitingForFeedback;
        private long _vhfFmDial4WaitingForFeedback;
        private const string VHF_FM_PRESET_INCREASE = "VHFFM_PRESET INC\n";
        private const string VHF_FM_PRESET_DECREASE = "VHFFM_PRESET DEC\n";
        private const string VHF_FM_FREQ_MODE_INCREASE = "VHFFM_FREQEMER INC\n";
        private const string VHF_FM_FREQ_MODE_DECREASE = "VHFFM_FREQEMER DEC\n";
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqMode;// 3 = PRESET
        private DCSBIOSOutput _vhfFmDcsbiosOutputSelectedChannel;
        private volatile uint _vhfFmCockpitFreqMode;
        private volatile uint _vhfFmCockpitPresetChannel;
        private readonly ClickSpeedDetector _vhfFmChannelClickSpeedDetector = new(8);
        private readonly ClickSpeedDetector _vhfFmFreqModeClickSpeedDetector = new(6);

        private const string VHF_FM_MODE_INCREASE = "VHFFM_MODE INC\n";
        private const string VHF_FM_MODE_DECREASE = "VHFFM_MODE DEC\n";
        private DCSBIOSOutput _vhfFmDcsbiosOutputMode;// VHFFM_MODE
        private volatile uint _vhfFmCockpitMode;
        private readonly ClickSpeedDetector _vhfFmModeClickSpeedDetector = new(6);

        /*A-10C ILS*/
        // Large dial 108-111 [step of 1]
        // Small dial 10-95 [step of 5]
        private uint _ilsBigFrequencyStandby = 108; // "108" "109" "110" "111"
        private uint _ilsSmallFrequencyStandby = 10; // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
        private uint _ilsSavedCockpitBigFrequency = 108; // "108" "109" "110" "111"
        private uint _ilsSavedCockpitSmallFrequency = 10; // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
        private readonly object _lockIlsDialsObject1 = new();
        private readonly object _lockIlsDialsObject2 = new();
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
        // Large dial 0-12 [step of 1]
        // Small dial 0-9 [step of 1]
        // Last : X/Y [0,1]
        private int _tacanBigFrequencyStandby = 6;
        private int _tacanSmallFrequencyStandby = 5;
        private int _tacanXYStandby;
        private int _tacanSavedCockpitBigFrequency = 6;
        private int _tacanSavedCockpitSmallFrequency = 5;
        private int _tacanSavedCockpitXY;
        private readonly object _lockTacanDialsObject1 = new();
        private readonly object _lockTacanDialsObject2 = new();
        private readonly object _lockTacanDialsObject3 = new();
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

        private readonly object _lockShowFrequenciesOnPanelObject = new();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69A10C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownUHFThread = true;
                    _shutdownVHFAMThread = true;
                    _shutdownILSThread = true;
                    _shutdownTACANThread = true;
                    _shutdownVHFFMThread = true;
                    BIOSEventHandler.DetachDataListener(this);
                    BIOSEventHandler.DetachStringListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
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
            // VHF AM
            if (e.Address == _vhfAmDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockVhfAmDialsObject1)
                {
                    var tmp = _vhfAmCockpitFreq1DialPos;
                    _vhfAmCockpitFreq1DialPos = _vhfAmDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _vhfAmCockpitFreq1DialPos)
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _vhfAmDial1WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _vhfAmDial2WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _vhfAmDial3WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _vhfAmDial4WaitingForFeedback, 0);
                    }
                }
            }

            if (e.Address == _vhfAmDcsbiosOutputChannelFreqMode.Address)
            {
                var tmp = _vhfAmCockpitFreqMode;
                _vhfAmCockpitFreqMode = _vhfAmDcsbiosOutputChannelFreqMode.GetUIntValue(e.Data);
                if (tmp != _vhfAmCockpitFreqMode)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            if (e.Address == _vhfAmDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _vhfAmCockpitPresetChannel;
                _vhfAmCockpitPresetChannel = _vhfAmDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _vhfAmCockpitPresetChannel)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            if (e.Address == _vhfAmDcsbiosOutputMode.Address)
            {
                var tmp = _vhfAmCockpitMode;
                _vhfAmCockpitMode = _vhfAmDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _vhfAmCockpitMode)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // UHF
            if (e.Address == _uhfDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockUhfDialsObject1)
                {
                    var tmp = _uhfCockpitFreq1DialPos;
                    _uhfCockpitFreq1DialPos = _uhfDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq1DialPos)
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 0);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 0);
                    }
                }
            }

            if (e.Address == _uhfDcsbiosOutputFreqMode.Address)
            {
                var tmp = _uhfCockpitFreqMode;
                _uhfCockpitFreqMode = _uhfDcsbiosOutputFreqMode.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitFreqMode)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            if (e.Address == _uhfDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _uhfCockpitPresetChannel;
                _uhfCockpitPresetChannel = _uhfDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _uhfCockpitPresetChannel)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            if (e.Address == _uhfDcsbiosOutputFunction.Address)
            {
                var tmp = _uhfCockpitMode;
                _uhfCockpitMode = _uhfDcsbiosOutputFunction.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitMode)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // VHF FM
            if (e.Address == _vhfFmDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockVhfFmDialsObject1)
                {
                    var tmp = _vhfFmCockpitFreq1DialPos;
                    _vhfFmCockpitFreq1DialPos = _vhfFmDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _vhfFmCockpitFreq1DialPos)
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
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
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            if (e.Address == _vhfFmDcsbiosOutputSelectedChannel.Address)
            {
                var tmp = _vhfFmCockpitPresetChannel;
                _vhfFmCockpitPresetChannel = _vhfFmDcsbiosOutputSelectedChannel.GetUIntValue(e.Data) + 1;
                if (tmp != _vhfFmCockpitPresetChannel)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            if (e.Address == _vhfFmDcsbiosOutputMode.Address)
            {
                var tmp = _vhfFmCockpitMode;
                _vhfFmCockpitMode = _vhfFmDcsbiosOutputMode.GetUIntValue(e.Data);
                if (tmp != _vhfFmCockpitMode)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // ILS
            if (e.Address == _ilsDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockIlsDialsObject1)
                {
                    var tmp = _ilsCockpitFreq1DialPos;
                    _ilsCockpitFreq1DialPos = _ilsDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _ilsCockpitFreq1DialPos)
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
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
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 0);
                    }
                }
            }

            // TACAN is set via String listener

            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                // Common.DebugP("RadioPanelPZ69A10C Received DCSBIOS stringData : ->" + e.StringData + "<-");
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    // Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }

                if (e.Address.Equals(_tacanDcsbiosOutputFreqChannel.Address))
                {
                    try
                    {
                        int changeCount = 0;

                        // " 00X" --> "129X"
                        lock (_lockTacanDialsObject1)
                        {
                            if (!uint.TryParse(e.StringData.Substring(0, 2), out var tmpUint))
                            {
                                return;
                            }

                            if (tmpUint != _tacanCockpitFreq1DialPos)
                            {
                                changeCount |= 2;
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
                                changeCount |= 4;
                                _tacanCockpitFreq2DialPos = tmpUint;
                            }
                        }

                        lock (_lockTacanDialsObject3)
                        {
                            var tmp = _tacanCockpitFreq3DialPos;
                            var tmpXY = e.StringData.Substring(3, 1);
                            _tacanCockpitFreq3DialPos = tmpXY.Equals("X") ? 0 : (uint)1;
                            if (tmp != _tacanCockpitFreq3DialPos)
                            {
                                changeCount |= 8;
                            }
                        }

                        if ((changeCount & 2) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial1WaitingForFeedback, 0);
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }

                        if ((changeCount & 4) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 0);
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }

                        if ((changeCount & 8) > 0)
                        {
                            Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 0);
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                        // Common.LogError(123, "DCSBIOSStringReceived TACAN: >" + e.StringData + "< " + exception.Message + " \n" + exception.StackTrace);
                        // TODO Strange values from DCS-BIOS
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentA10RadioMode.VHFAM:
                                {
                                    if (_vhfAmCockpitMode != 0 && !VhfAmPresetSelected())
                                    {
                                        SendVhfAmToDCSBIOS();
                                    }
                                    break;
                                }

                            case CurrentA10RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SendUhfToDCSBIOS();
                                    }
                                    break;
                                }

                            case CurrentA10RadioMode.VHFFM:
                                {
                                    if (_vhfFmCockpitMode != 0 && !VhfFmPresetSelected())
                                    {
                                        SendVhfFmToDCSBIOS();
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
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentA10RadioMode.VHFAM:
                                {
                                    if (_vhfAmCockpitMode != 0 && !VhfAmPresetSelected())
                                    {
                                        SendVhfAmToDCSBIOS();
                                    }
                                    break;
                                }

                            case CurrentA10RadioMode.UHF:
                                {
                                    if (_uhfCockpitMode != 0 && !UhfPresetSelected())
                                    {
                                        SendUhfToDCSBIOS();
                                    }
                                    break;
                                }

                            case CurrentA10RadioMode.VHFFM:
                                {
                                    if (_vhfFmCockpitMode != 0 && !VhfFmPresetSelected())
                                    {
                                        SendVhfFmToDCSBIOS();
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

        private void SendVhfAmToDCSBIOS()
        {
            if (VhfAmNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyVhfAm();
            var frequencyAsString = _vhfAmBigFrequencyStandby + "." + _vhfAmSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0');

            // Frequency selector 1      VHFAM_FREQ1
            // " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            // Frequency selector 2      VHFAM_FREQ2
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3      VHFAM_FREQ3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4      VHFAM_FREQ4
            // "00" "25" "50" "75", only "00" and "50" used.
            // Pos     0    1    2    3
            int desiredPositionDial1;
            int desiredPositionDial2;
            int desiredPositionDial3;
            int tmp;

            if (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture) == 2)
            {
                // 30.00
                // #1 = 3  (position = value - 3)
                // #2 = 0   (position = value)
                // #3 = 0   (position = value)
                // #4 = 00
                desiredPositionDial1 = int.Parse(frequencyAsString.Substring(0, 1)) - 3;
                desiredPositionDial2 = int.Parse(frequencyAsString.Substring(1, 1));
                desiredPositionDial3 = int.Parse(frequencyAsString.Substring(3, 1));
                tmp = int.Parse(frequencyAsString.Substring(4, 1));
            }
            else
            {
                // 151.95
                // #1 = 15  (position = value - 3)
                // #2 = 1   (position = value)
                // #3 = 9   (position = value)
                // #4 = 5
                desiredPositionDial1 = int.Parse(frequencyAsString.Substring(0, 2)) - 3;
                desiredPositionDial2 = int.Parse(frequencyAsString.Substring(2, 1));
                desiredPositionDial3 = int.Parse(frequencyAsString.Substring(4, 1));
                tmp = int.Parse(frequencyAsString.Substring(5, 1));
            }

            var desiredPositionDial4 = tmp switch
            {
                0 => 0,
                2 => 1,
                5 => 2,
                7 => 3,
                _ => 0
            };

            // #1
            _shutdownVHFAMThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVHFAMThread = false;
            _vhfAmSyncThread = new Thread(() => VhfAmSynchThreadMethod(desiredPositionDial1, desiredPositionDial2, desiredPositionDial3, desiredPositionDial4));
            _vhfAmSyncThread.Start();
        }

        private volatile bool _shutdownVHFAMThread;
        private void VhfAmSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3, int desiredPositionDial4)
        {
            try
            {
                try
                {   /*
                     * A-10C AN/ARC-186(V) VHF AM Radio 1
                     * 
                     * Large dial 116-151 [step of 1]
                     * Small dial 0.00-0.95 [step of 0.05]
                     */

                    Interlocked.Exchange(ref _vhfAmThreadNowSynching, 1);
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
                    do
                    {
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfAmDial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfAmDial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfAmDial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial4Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfAmDial4WaitingForFeedback); // Lets do an ugly reset
                        }

                        string str;
                        if (Interlocked.Read(ref _vhfAmDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAmDialsObject1)
                            {

                                if (_vhfAmCockpitFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    str = VHF_AM_FREQ_1DIAL_COMMAND + GetCommandDirectionForVhfDial1(desiredPositionDial1, _vhfAmCockpitFreq1DialPos);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vhfAmDial1WaitingForFeedback, 1);
                                }

                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfAmDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAmDialsObject2)
                            {
                                if (_vhfAmCockpitFreq2DialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    str = VHF_AM_FREQ_2DIAL_COMMAND + GetCommandDirectionForVhfDial23(desiredPositionDial2, _vhfAmCockpitFreq2DialPos);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vhfAmDial2WaitingForFeedback, 1);
                                }

                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfAmDial3WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAmDialsObject3)
                            {
                                if (_vhfAmCockpitFreq3DialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    str = VHF_AM_FREQ_3DIAL_COMMAND + GetCommandDirectionForVhfDial23(desiredPositionDial3, _vhfAmCockpitFreq3DialPos);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _vhfAmDial3WaitingForFeedback, 1);
                                }

                                Reset(ref dial3Timeout);
                            }
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfAmDial4WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAmDialsObject4)
                            {
                                if (_vhfAmCockpitFreq4DialPos < desiredPositionDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    str = VHF_AM_FREQ_4DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfAmDial4WaitingForFeedback, 1);
                                }
                                else if (_vhfAmCockpitFreq4DialPos > desiredPositionDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    str = VHF_AM_FREQ_4DIAL_COMMAND + Decrease;
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfAmDial4WaitingForFeedback, 1);
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
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownVHFAMThread);
                    SwapCockpitStandbyFrequencyVhfAm();
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
                Interlocked.Exchange(ref _vhfAmThreadNowSynching, 0);
            }

            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendUhfToDCSBIOS()
        {
            if (UhfNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyUhf();

            // Frequency selector 1     
            // "2"  "3"  "A"
            // Pos     0    1    2

            // Frequency selector 2      
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 5
            // "00" "25" "50" "75", only "00" and "50" used.
            // Pos     0    1    2    3

            // Large dial 225-399 [step of 1]
            // Small dial 0.00-0.95 [step of 0.05]
            var frequencyAsString = _uhfBigFrequencyStandby.ToString(CultureInfo.InvariantCulture) + "." + _uhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0');

            int freqDial1;
            int freqDial2;
            int freqDial3;
            int freqDial4;
            int freqDial5;

            // Special case! If Dial 1 = "A" then all digits can be disregarded once they are set to zero
            var index = frequencyAsString.IndexOf(".", StringComparison.InvariantCulture);
            switch (index)
            {
                // 0.075Mhz
                case 1:
                    {
                        freqDial1 = 2; // ("A")
                        freqDial2 = 0;
                        freqDial3 = int.Parse(frequencyAsString.Substring(0, 1));
                        freqDial4 = int.Parse(frequencyAsString.Substring(2, 1));
                        freqDial5 = int.Parse(frequencyAsString.Substring(3, 1));
                        break;
                    }

                // 10.075Mhz
                case 2:
                    {
                        freqDial1 = 2; // ("A")
                        freqDial2 = int.Parse(frequencyAsString.Substring(0, 1));
                        freqDial3 = int.Parse(frequencyAsString.Substring(1, 1));
                        freqDial4 = int.Parse(frequencyAsString.Substring(3, 1));
                        freqDial5 = int.Parse(frequencyAsString.Substring(4, 1));
                        break;
                    }

                // 100.075Mhz
                case 3:
                    {
                        freqDial1 = int.Parse(frequencyAsString.Substring(0, 1));
                        switch (freqDial1)
                        {
                            case 2:
                                {
                                    freqDial1 = 0;
                                    break;
                                }

                            case 3:
                                {
                                    freqDial1 = 1;
                                    break;
                                }
                        }

                        freqDial2 = int.Parse(frequencyAsString.Substring(1, 1));
                        freqDial3 = int.Parse(frequencyAsString.Substring(2, 1));
                        freqDial4 = int.Parse(frequencyAsString.Substring(4, 1));
                        freqDial5 = int.Parse(frequencyAsString.Substring(5, 1));
                        break;
                    }

                default:
                    {
                        throw new Exception($"Failed to find separator in frequency string {frequencyAsString}");
                    }
            }

            switch (freqDial5)
            {
                // Frequency selector 5
                // "00" "25" "50" "75", only 0 2 5 7 used.
                // Pos     0    1    2    3
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

            // Frequency selector 1     
            // "2"  "3"  "A"/"-1"
            // Pos     0    1    2

            // Frequency selector 2      
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 5
            // "00" "25" "50" "75", only "00" and "50" used.
            // Pos     0    1    2    3

            // Large dial 225-399 [step of 1]
            // Small dial 0.00-0.95 [step of 0.05]

            // #1
            _shutdownUHFThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownUHFThread = false;
            _uhfSyncThread = new Thread(() => UhfSynchThreadMethod(freqDial1, freqDial2, freqDial3, freqDial4, freqDial5));
            _uhfSyncThread.Start();
        }

        private volatile bool _shutdownUHFThread;
        private void UhfSynchThreadMethod(int desiredPosition1, int desiredPosition2, int desiredPosition3, int desiredPosition4, int desiredPosition5)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _uhfThreadNowSynching, 1);
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

                        if (IsTimedOut(ref dial5Timeout))
                        {
                            ResetWaitingForFeedBack(ref _uhfDial5WaitingForFeedback); // Lets do an ugly reset
                        }

                        // Frequency selector 1     
                        // "2"  "3"  "A"/"-1"
                        // Pos     0    1    2
                        if (Interlocked.Read(ref _uhfDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialsObject1)
                            {
                                if (_uhfCockpitFreq1DialPos != desiredPosition1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                }

                                if (_uhfCockpitFreq1DialPos < desiredPosition1)
                                {
                                    const string str = UHF_FREQ_1DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq1DialPos > desiredPosition1)
                                {
                                    const string str = UHF_FREQ_1DIAL_COMMAND + Decrease;
                                    DCSBIOS.Send(str);
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
                            lock (_lockUhfDialsObject2)
                            {
                                if (_uhfCockpitFreq2DialPos != desiredPosition2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                }

                                if (_uhfCockpitFreq2DialPos < desiredPosition2)
                                {
                                    const string str = UHF_FREQ_2DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq2DialPos > desiredPosition2)
                                {
                                    const string str = UHF_FREQ_2DIAL_COMMAND + Decrease;
                                    DCSBIOS.Send(str);
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
                            lock (_lockUhfDialsObject3)
                            {
                                if (_uhfCockpitFreq3DialPos != desiredPosition3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                }

                                if (_uhfCockpitFreq3DialPos < desiredPosition3)
                                {
                                    const string str = UHF_FREQ_3DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq3DialPos > desiredPosition3)
                                {
                                    const string str = UHF_FREQ_3DIAL_COMMAND + Decrease;
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 1);
                                }

                                Reset(ref dial3Timeout);
                            }
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _uhfDial4WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialsObject4)
                            {
                                if (_uhfCockpitFreq4DialPos != desiredPosition4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                }

                                if (_uhfCockpitFreq4DialPos < desiredPosition4)
                                {
                                    const string str = UHF_FREQ_4DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq4DialPos > desiredPosition4)
                                {
                                    const string str = UHF_FREQ_4DIAL_COMMAND + Decrease;
                                    DCSBIOS.Send(str);
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

                        if (Interlocked.Read(ref _uhfDial5WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialsObject5)
                            {
                                if (_uhfCockpitFreq5DialPos != desiredPosition5)
                                {
                                    dial5OkTime = DateTime.Now.Ticks;
                                }

                                if (_uhfCockpitFreq5DialPos < desiredPosition5)
                                {
                                    const string str = UHF_FREQ_5DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial5SendCount++;
                                    Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq5DialPos > desiredPosition5)
                                {
                                    const string str = UHF_FREQ_5DIAL_COMMAND + Decrease;
                                    DCSBIOS.Send(str);
                                    dial5SendCount++;
                                    Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 1);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            dial4SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime) || IsTooShort(dial5OkTime)) && !_shutdownUHFThread);
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
                Interlocked.Exchange(ref _uhfThreadNowSynching, 0);
            }

            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendVhfFmToDCSBIOS()
        {
            if (VhfFmNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyVhfFm();
            var frequencyAsString = _vhfFmBigFrequencyStandby + "." + (_vhfFmSmallFrequencyStandby.ToString().PadLeft(3, '0'));

            // Frequency selector 1      VHFFM_FREQ1
            // " 3" " 4" " 5" " 6" " 7" THESE ARE NOT USED IN FM RANGE ----> " 8" " 9" "10" "11" "12" "13" "14" "15"
            // Pos     0    1    2    3    4                                         5    6    7    8    9   10   11   12

            // Frequency selector 2      VHFFM_FREQ2
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3      VHFFM_FREQ3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4      VHFFM_FREQ4
            // "00" "25" "50" "75"
            // Pos     0    1    2    3
            int desiredPositionDial1;
            int desiredPositionDial2;
            int desiredPositionDial3;
            int desiredPositionDial4;

            if (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture) == 2)
            {
                // 30.025
                // #1 = 3  (position = value - 3)
                // #2 = 0   (position = value)
                // #3 = 0   (position = value)
                // #4 = 25
                desiredPositionDial1 = int.Parse(frequencyAsString.Substring(0, 1)) - 3;
                desiredPositionDial2 = int.Parse(frequencyAsString.Substring(1, 1));
                desiredPositionDial3 = int.Parse(frequencyAsString.Substring(3, 1));
                var tmpPosition = int.Parse(frequencyAsString.Substring(4, 2));

                desiredPositionDial4 = tmpPosition switch
                {
                    0 => 0,
                    25 => 1,
                    50 => 2,
                    75 => 3,
                    _ => throw new Exception($"Unexpected tmpPosition position [{tmpPosition}] in SendVhfFmToDCSBIOS")
                };
            }
            else
            {
                // 151.95
                // This is a quick and dirty fix. We should not be here when dealing with VHF FM because the range is 30.000 to 76.000 MHz.
                // Set freq to 45.000 MHz (sort of an reset)
                desiredPositionDial1 = 1; // (4)
                desiredPositionDial2 = 5;
                desiredPositionDial3 = 0;
                desiredPositionDial4 = 0;
            }

            _shutdownVHFFMThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVHFFMThread = false;
            _vhfFmSyncThread = new Thread(() => VhfFmSynchThreadMethod(desiredPositionDial1, desiredPositionDial2, desiredPositionDial3, desiredPositionDial4));
            _vhfFmSyncThread.Start();
        }

        private volatile bool _shutdownVHFFMThread;
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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfFmDial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfFmDial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfFmDial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial4Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfFmDial4WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _vhfFmDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject1)
                            {
                                if (_vhfFmCockpitFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = VHF_FM_FREQ_1DIAL_COMMAND + GetCommandDirectionForVhfDial1(desiredPositionDial1, _vhfFmCockpitFreq1DialPos);
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
                                    var str = VHF_FM_FREQ_2DIAL_COMMAND + GetCommandDirectionForVhfDial23(desiredPositionDial2, _vhfFmCockpitFreq2DialPos);
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
                                    var str = VHF_FM_FREQ_3DIAL_COMMAND + GetCommandDirectionForVhfDial23(desiredPositionDial3, _vhfFmCockpitFreq3DialPos);
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
                                // "00" "25" "50" "75", only "00" and "50" used.
                                // Pos     0    1    2    3
                                if (_vhfFmCockpitFreq4DialPos < frequencyDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    const string str = VHF_FM_FREQ_4DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 1);
                                }
                                else if (_vhfFmCockpitFreq4DialPos > frequencyDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    const string str = VHF_FM_FREQ_4DIAL_COMMAND + Decrease;
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            dial4SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS

                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownVHFFMThread);
                    SwapCockpitStandbyFrequencyVhfFm();
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
                Interlocked.Exchange(ref _vhfFmThreadNowSynching, 0);
            }

            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendILSToDCSBIOS()
        {
            if (IlsNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyIls();
            var frequency = double.Parse(
                _ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay),
                NumberFormatInfoFullDisplay);
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);

            // Frequency selector 1   
            // "108" "109" "110" "111"
            // 0     1     2     3

            // Frequency selector 2   
            // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            // 0    1    2    3    4    5    6    7    8    9

            // 108.95
            // #1 = 0
            // #2 = 9
            var freqDial1 = GetILSDialPosForFrequency(1, int.Parse(frequencyAsString.Substring(0, 3)));
            var freqDial2 = GetILSDialPosForFrequency(2, int.Parse(frequencyAsString.Substring(4, 2)));

            // #1
            _shutdownILSThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownILSThread = false;
            _ilsSyncThread = new Thread(() => ILSSynchThreadMethod(freqDial1, freqDial2));
            _ilsSyncThread.Start();
        }

        private volatile bool _shutdownILSThread;
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
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _ilsDial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _ilsDial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _ilsDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockIlsDialsObject1)
                            {
                                if (_ilsCockpitFreq1DialPos < position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    const string str = ILS_FREQ1_DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitFreq1DialPos > position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    const string str = ILS_FREQ1_DIAL_COMMAND + Decrease;
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
                                    const string str = ILS_FREQ2_DIAL_COMMAND + Increase;
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitFreq2DialPos > position2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    const string str = ILS_FREQ2_DIAL_COMMAND + Decrease;
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime)) && !_shutdownILSThread);
                    SwapCockpitStandbyFrequencyIls();
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
                Interlocked.Exchange(ref _ilsThreadNowSynching, 0);
            }

            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendTacanToDCSBIOS()
        {
            if (TacanNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyTacan();

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
            _shutdownTACANThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownTACANThread = false;
            _tacanSyncThread = new Thread(() => TacanSynchThreadMethod(_tacanBigFrequencyStandby, _tacanSmallFrequencyStandby, _tacanXYStandby));
            _tacanSyncThread.Start();
        }

        private volatile bool _shutdownTACANThread;
        private void TacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _tacanThreadNowSynching, 1);

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
                            ResetWaitingForFeedBack(ref _tacanDial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _tacanDial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _tacanDial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _tacanDial1WaitingForFeedback) == 0)
                        {

                            lock (_lockTacanDialsObject1)
                            {
                                if (_tacanCockpitFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = TACAN_FREQ1_DIAL_COMMAND + (_tacanCockpitFreq1DialPos < desiredPositionDial1 ? Increase : Decrease);
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

                                    var str = TACAN_FREQ2_DIAL_COMMAND + (_tacanCockpitFreq2DialPos < desiredPositionDial2 ? Increase : Decrease);
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

                                    var str = TACAN_FREQ3_DIAL_COMMAND + (_tacanCockpitFreq3DialPos < desiredPositionDial3 ? Increase : Decrease);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS


                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime)) && !_shutdownTACANThread);
                    SwapCockpitStandbyFrequencyTacan();
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
                Interlocked.Exchange(ref _tacanThreadNowSynching, 0);
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
                    case CurrentA10RadioMode.VHFAM:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfAmCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfAmCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_vhfAmCockpitMode != 0 && VhfAmPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfAmCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
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
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(GetUhfFrequencyAsString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + (_uhfSmallFrequencyStandby / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }

                            break;
                        }

                    case CurrentA10RadioMode.VHFFM:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfFmCockpitMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfFmCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_vhfFmCockpitMode != 0 && VhfFmPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfFmCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                // Frequency selector 1      VHFFM_FREQ1
                                // " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
                                // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                // Frequency selector 2      VHFFM_FREQ2
                                // 0 1 2 3 4 5 6 7 8 9

                                // Frequency selector 3      VHFFM_FREQ3
                                // 0 1 2 3 4 5 6 7 8 9

                                // Frequency selector 4      VHFFM_FREQ4
                                // "00" "25" "50" "75", only "00" and "50" used.
                                // Pos     0    1    2    3
                                if (_vhfFmCockpitMode == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
                                    string dial1;
                                    string dial2;
                                    string dial3;
                                    string dial4;
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
                            // Mhz   "108" "109" "110" "111"
                            // Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                            string frequencyAsString;
                            lock (_lockIlsDialsObject1)
                            {
                                frequencyAsString = GetILSDialFrequencyForPosition(1, _ilsCockpitFreq1DialPos);
                            }

                            frequencyAsString += ".";
                            lock (_lockIlsDialsObject2)
                            {
                                frequencyAsString += GetILSDialFrequencyForPosition(2, _ilsCockpitFreq2DialPos);
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentA10RadioMode.TACAN:
                        {
                            // TACAN  00X/Y --> 129X/Y
                            // Frequency selector 1      LEFT
                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                            // Frequency selector 2      MIDDLE
                            // 0 1 2 3 4 5 6 7 8 9

                            // Frequency selector 3      RIGHT
                            // X=0 / Y=1
                            string frequencyAsString;
                            lock (_lockTacanDialsObject1)
                            {
                                lock (_lockTacanDialsObject2)
                                {
                                    frequencyAsString = _tacanCockpitFreq1DialPos + _tacanCockpitFreq2DialPos.ToString();
                                }
                            }

                            frequencyAsString += ".";
                            lock (_lockTacanDialsObject3)
                            {
                                frequencyAsString += _tacanCockpitFreq3DialPos;
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanBigFrequencyStandby + _tacanSmallFrequencyStandby.ToString() + "." + _tacanXYStandby, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentA10RadioMode.VHFAM:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfAmCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfAmCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_vhfAmCockpitMode != 0 && VhfAmPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfAmCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                                    SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + (_vhfAmSmallFrequencyStandby / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }

                            break;
                        }

                    case CurrentA10RadioMode.UHF:
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
                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(GetUhfFrequencyAsString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + (_uhfSmallFrequencyStandby / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }

                            break;
                        }

                    case CurrentA10RadioMode.VHFFM:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfFmCockpitMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfFmCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            if (_vhfFmCockpitMode != 0 && VhfFmPresetSelected())
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfFmCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
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
                                    string dial1;
                                    string dial2;
                                    string dial3;
                                    string dial4;
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
                            string frequencyAsString;
                            lock (_lockIlsDialsObject1)
                            {
                                frequencyAsString = GetILSDialFrequencyForPosition(1, _ilsCockpitFreq1DialPos);
                            }

                            frequencyAsString += ".";
                            lock (_lockIlsDialsObject2)
                            {
                                frequencyAsString += GetILSDialFrequencyForPosition(2, _ilsCockpitFreq2DialPos);
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentA10RadioMode.TACAN:
                        {
                            string frequencyAsString;
                            lock (_lockTacanDialsObject1)
                            {
                                lock (_lockTacanDialsObject2)
                                {
                                    frequencyAsString = _tacanCockpitFreq1DialPos + _tacanCockpitFreq2DialPos.ToString();
                                }
                            }

                            frequencyAsString += ".";
                            lock (_lockTacanDialsObject3)
                            {
                                frequencyAsString += _tacanCockpitFreq3DialPos;
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanBigFrequencyStandby + _tacanSmallFrequencyStandby.ToString() + "." + _tacanXYStandby, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                }
                SendLCDData(bytes);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        private string GetVhfAmFrequencyAsString()
        {
            // Frequency selector 1      VHFAM_FREQ1
            // " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            // Frequency selector 2      VHFAM_FREQ2
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3      VHFAM_FREQ3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4      VHFAM_FREQ4
            // "00" "25" "50" "75", only "00" and "50" used.
            // Pos     0    1    2    3
            string frequencyAsString;

            lock (_lockVhfAmDialsObject1)
            {
                frequencyAsString = GetVhfAmDialFrequencyForPosition(1, _vhfAmCockpitFreq1DialPos);
            }

            lock (_lockVhfAmDialsObject2)
            {
                frequencyAsString += GetVhfAmDialFrequencyForPosition(2, _vhfAmCockpitFreq2DialPos);
            }

            frequencyAsString += ".";
            lock (_lockVhfAmDialsObject3)
            {
                frequencyAsString += GetVhfAmDialFrequencyForPosition(3, _vhfAmCockpitFreq3DialPos);
            }

            lock (_lockVhfAmDialsObject4)
            {
                frequencyAsString += GetVhfAmDialFrequencyForPosition(4, _vhfAmCockpitFreq4DialPos);
            }

            return frequencyAsString;
        }

        private string GetUhfFrequencyAsString()
        {
            // Frequency selector 1     
            // //"2"  "3"  "A"
            // Pos     0    1    2

            // Frequency selector 2      
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3
            // 0 1 2 3 4 5 6 7 8 9


            // Frequency selector 4
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 5
            // "00" "25" "50" "75", only "00" and "50" used.
            // Pos     0    1    2    3

            // 251.75
            string frequencyAsString;
            lock (_lockUhfDialsObject1)
            {
                frequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfCockpitFreq1DialPos);
            }

            lock (_lockUhfDialsObject2)
            {
                frequencyAsString += GetUhfDialFrequencyForPosition(2, _uhfCockpitFreq2DialPos);
            }

            lock (_lockUhfDialsObject3)
            {
                frequencyAsString += GetUhfDialFrequencyForPosition(3, _uhfCockpitFreq3DialPos);
            }

            frequencyAsString += ".";
            lock (_lockUhfDialsObject4)
            {
                frequencyAsString += GetUhfDialFrequencyForPosition(4, _uhfCockpitFreq4DialPos);
            }

            lock (_lockUhfDialsObject5)
            {
                frequencyAsString += GetUhfDialFrequencyForPosition(5, _uhfCockpitFreq5DialPos);
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
                                                    // @ max value
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
                                                    // 225-399
                                                    // @ max value
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
                                                    // @ max value
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
                                            // Mhz "108" "109" "110" "111"
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                    // @ min value
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
                                                    // @ min value
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
                                                    // @ min value
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
                                            // "108" "109" "110" "111"
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                if (_vhfFmSmallFrequencyStandby >= 975)
                                                {
                                                    // At max value
                                                    _vhfFmSmallFrequencyStandby = 0;
                                                    break;
                                                }

                                                VHFFMSmallFrequencyStandbyAdjust(true);
                                            }

                                            break;
                                        }

                                    case CurrentA10RadioMode.ILS:
                                        {
                                            // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            _ilsSmallFrequencyStandby = _ilsSmallFrequencyStandby switch
                                            {
                                                10 => 15,
                                                15 => 30,
                                                30 => 35,
                                                35 => 50,
                                                50 => 55,
                                                55 => 70,
                                                70 => 75,
                                                75 => 90,
                                                90 => 95,
                                                95 or 100 or 105 => 10, // Just safe guard in case it pops above the limit. Happened to VHF AM for some !?!?!? reason.
                                                _ => throw new Exception($"Unexpected _ilsSmallFrequencyStandby position [{_ilsSmallFrequencyStandby}] in AdjustFrequency - Upper Increase ILS")
                                            };
                                            break;
                                        }

                                    case CurrentA10RadioMode.TACAN:
                                        {
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                if (_vhfFmSmallFrequencyStandby <= 0)
                                                {
                                                    // At min value
                                                    _vhfFmSmallFrequencyStandby = 975;
                                                    break;
                                                }

                                                VHFFMSmallFrequencyStandbyAdjust(false);
                                            }

                                            break;
                                        }

                                    case CurrentA10RadioMode.ILS:
                                        {
                                            // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            _ilsSmallFrequencyStandby = _ilsSmallFrequencyStandby switch
                                            {
                                                0 or 5 or 10 => 95,
                                                15 => 10,
                                                30 => 15,
                                                35 => 30,
                                                50 => 35,
                                                55 => 50,
                                                70 => 55,
                                                75 => 70,
                                                90 => 75,
                                                95 => 90,
                                                _ => throw new Exception($"Unexpected _ilsSmallFrequencyStandby position [{_ilsSmallFrequencyStandby}] in AdjustFrequency - Upper Decrease ILS")
                                            };
                                            break;
                                        }

                                    case CurrentA10RadioMode.TACAN:
                                        {
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                    // @ max value
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
                                                    // 225-399
                                                    DCSBIOS.Send(UHF_PRESET_INCREASE);
                                                }
                                                else if (_uhfBigFrequencyStandby.Equals(399.00))
                                                {
                                                    // @ max value
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
                                                    // @ max value
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
                                            // Mhz "108" "109" "110" "111"
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                    // @ min value
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
                                                    // @ min value
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
                                                    // @ min value
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
                                            // "108" "109" "110" "111"
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
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                if (_vhfFmSmallFrequencyStandby >= 975)
                                                {
                                                    // At max value
                                                    _vhfFmSmallFrequencyStandby = 0;
                                                    break;
                                                }

                                                VHFFMSmallFrequencyStandbyAdjust(true);
                                            }

                                            break;
                                        }

                                    case CurrentA10RadioMode.ILS:
                                        {
                                            // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            _ilsSmallFrequencyStandby = _ilsSmallFrequencyStandby switch
                                            {
                                                10 => 15,
                                                15 => 30,
                                                30 => 35,
                                                35 => 50,
                                                50 => 55,
                                                55 => 70,
                                                70 => 75,
                                                75 => 90,
                                                90 => 95,
                                                95 or 100 or 105 => 10, // Just safe guard in case it pops above the limit. Happened to VHF AM for some !?!?!? reason.
                                                _ => throw new Exception($"Unexpected _ilsSmallFrequencyStandby position [{_ilsSmallFrequencyStandby}] in AdjustFrequency - Lower Increase ILS")
                                            };
                                            break;
                                        }

                                    case CurrentA10RadioMode.TACAN:
                                        {
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
                                                if (_vhfFmSmallFrequencyStandby <= 0)
                                                {
                                                    // At min value
                                                    _vhfFmSmallFrequencyStandby = 975;
                                                    break;
                                                }

                                                VHFFMSmallFrequencyStandbyAdjust(false);
                                            }

                                            break;
                                        }

                                    case CurrentA10RadioMode.ILS:
                                        {
                                            // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                                            _ilsSmallFrequencyStandby = _ilsSmallFrequencyStandby switch
                                            {
                                                0 or 5 or 10 => 95,
                                                15 => 10,
                                                30 => 15,
                                                35 => 30,
                                                50 => 35,
                                                55 => 50,
                                                70 => 55,
                                                75 => 70,
                                                90 => 75,
                                                95 => 90,
                                                _ => throw new Exception($"Unexpected _ilsSmallFrequencyStandby position [{_ilsSmallFrequencyStandby}] in AdjustFrequency - Lower Decrease ILS")
                                            };
                                            break;
                                        }

                                    case CurrentA10RadioMode.TACAN:
                                        {
                                            // TACAN  00X/Y --> 129X/Y
                                            // Frequency selector 1      LEFT
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      MIDDLE
                                            // 0 1 2 3 4 5 6 7 8 9

                                            // Frequency selector 3      RIGHT
                                            // X=0 / Y=1
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
            // Crude fix if any freqs are outside the valid boundaries

            // VHF AM
            // 116.00 - 151.975
            if (_vhfAmBigFrequencyStandby < 116)
            {
                _vhfAmBigFrequencyStandby = 116;
            }

            if (_vhfAmBigFrequencyStandby > 151)
            {
                _vhfAmBigFrequencyStandby = 151;
            }

            // VHF FM
            // 30.000 - 76.000Mhz
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

            // UHF
            // 225.000 - 399.975 MHz
            if (_uhfBigFrequencyStandby < 225)
            {
                _uhfBigFrequencyStandby = 225;
            }

            if (_uhfBigFrequencyStandby > 399)
            {
                _uhfBigFrequencyStandby = 399;
            }

            // ILS
            // 108.10 - 111.95
            if (_ilsBigFrequencyStandby < 108)
            {
                _ilsBigFrequencyStandby = 108;
            }

            if (_ilsBigFrequencyStandby > 111)
            {
                _ilsBigFrequencyStandby = 111;
            }

            // TACAN
            // 00X/Y - 129X/Y
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

        public void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (isFirstReport)
            {
                return;
            }

            lock (LockLCDUpdateObject)
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
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
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Do when user releases button
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
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH);
                                    }

                                    _lowerButtonPressedAndDialRotated = false;
                                }

                                break;
                            }
                    }

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(DCSFPProfile.SelectedProfile.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_A10C, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                    }
                }

                AdjustFrequency(hashSet);
            }
        }

        public sealed override void Startup()
        {
            try
            {
                // VHF AM
                _vhfAmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ1");

                // _vhfAmDcsbiosOutputFreqDial1.Debug = true;
                _vhfAmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ2");

                // _vhfAmDcsbiosOutputFreqDial2.Debug = true;
                _vhfAmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ3");

                // _vhfAmDcsbiosOutputFreqDial3.Debug = true;
                _vhfAmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ4");

                // _vhfAmDcsbiosOutputFreqDial4.Debug = true;
                _vhfAmDcsbiosOutputChannelFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQEMER");
                _vhfAmDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_PRESET");
                _vhfAmDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_MODE");

                // UHF
                _uhfDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_100MHZ_SEL");
                _uhfDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_10MHZ_SEL");
                _uhfDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT25_SEL");
                _uhfDcsbiosOutputFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_MODE");
                _uhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_SEL");
                _uhfDcsbiosOutputFunction = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FUNCTION");

                // VHF FM
                _vhfFmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ1");

                // _vhfFmDcsbiosOutputFreqDial1.Debug = true;
                _vhfFmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ2");

                // _vhfFmDcsbiosOutputFreqDial2.Debug = true;
                _vhfFmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ3");

                // _vhfFmDcsbiosOutputFreqDial3.Debug = true;
                _vhfFmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ4");

                // _vhfFmDcsbiosOutputFreqDial4.Debug = true;
                _vhfFmDcsbiosOutputFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQEMER");
                _vhfFmDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_PRESET");
                _vhfFmDcsbiosOutputMode = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_MODE");

                // ILS
                _ilsDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ILS_MHZ");
                _ilsDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ILS_KHZ");

                // TACAN
                _tacanDcsbiosOutputFreqChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("TACAN_CHANNEL");
                DCSBIOSStringManager.AddListeningAddress(_tacanDcsbiosOutputFreqChannel); // _tacanDcsbiosOutputFreqChannel.MaxLength does not work. Bad JSON format.

                StartListeningForHidPanelChanges();

                // IsAttached = true;
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            // ignore
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobA10C.GetRadioPanelKnobs();
        }

        public static string GetVhfAmDialFrequencyForPosition(int dial, uint position)
        {

            // Frequency selector 1      VHFAM_FREQ1
            // " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            // Frequency selector 2      VHFAM_FREQ2
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3      VHFAM_FREQ3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4      VHFAM_FREQ4
            // "00" "25" "50" "75", only 0 2 5 7 used.
            // Pos     0    1    2    3

            return dial switch
            {

                1 => position switch
                {
                    0 => "3",
                    1 => "4",
                    2 => "5",
                    3 => "6",
                    4 => "7",
                    5 => "8",
                    6 => "9",
                    7 => "10",
                    8 => "11",
                    9 => "12",
                    10 => "13",
                    11 => "14",
                    12 => "15",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"VhfAm Unexpected position switch value {position} for dial value of 1"),
                },

                2 or 3 => position.ToString(),

                4 => position switch
                {
                    0 => "0",
                    1 => "2",
                    2 => "5",
                    3 => "7",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"VhfAm Unexpected position switch value {position} for dial value of 4"),
                },

                _ => string.Empty
            };
        }

        public static string GetUhfDialFrequencyForPosition(int dial, uint position)
        {
            // Frequency selector 1     
            // //"2"  "3"  "A"
            // Pos     0    1    2

            // Frequency selector 2      
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3
            // 0 1 2 3 4 5 6 7 8 9


            // Frequency selector 4
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 5
            // "00" "25" "50" "75", only "00" and "50" used.
            // Pos     0    1    2    3
            return dial switch
            {

                1 => position switch
                {
                    0 => "2",
                    1 => "3",
                    2 => "0", // should be "A" // throw new NotImplementedException("check how A should be treated.");
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"Uhf Unexpected position switch value {position} for dial value of 1"),
                },

                2 or 3 or 4 => position.ToString(),

                // "00" "25" "50" "75", only "00" and "50" used.
                // Pos     0    1    2    3
                5 => position switch
                {
                    0 => "00",
                    1 => "25",
                    2 => "50",
                    3 => "75",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"Uhf Unexpected position switch value {position} for dial value of 5"),
                },
                _ => string.Empty
            };
        }

        public static string GetVhfFmDialFrequencyForPosition(int dial, uint position)
        {
            // Frequency selector 1      VHFFM_FREQ1
            // " 3" " 4" " 5" " 6" " 7" " 8" " 9" "10" "11" "12" "13" "14" "15"
            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            // Frequency selector 2      VHFFM_FREQ2
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 3      VHFFM_FREQ3
            // 0 1 2 3 4 5 6 7 8 9

            // Frequency selector 4      VHFFM_FREQ4
            // "00" "25" "50" "75", 0 2 5 7 used.
            // Pos     0    1    2    3
            return dial switch
            {

                1 => position switch
                {
                    0 => "3",
                    1 => "4",
                    2 => "5",
                    3 => "6",
                    4 => "7",
                    5 => "8",
                    6 => "9",
                    7 => "10",
                    8 => "11",
                    9 => "12",
                    10 => "13",
                    11 => "14",
                    12 => "15",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"VhfFm Unexpected position switch value {position} for dial value of 1"),
                },

                2 or 3 => position.ToString(),

                // "00" "25" "50" "75"
                // Pos     0    1    2    3
                4 => position switch
                {
                    0 => "00",
                    1 => "25",
                    2 => "50",
                    3 => "75",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"VhfFm Unexpected position switch value {position} for dial value of 4"),
                },

                _ => string.Empty
            };
        }

        public static string GetILSDialFrequencyForPosition(int dial, uint position)
        {
            // 1 Mhz   "108" "109" "110" "111"
            // 0     1     2     3
            // 2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            // 0    1    2    3    4    5    6    7    8    9
            return dial switch
            {

                1 => position switch
                {
                    0 => "108",
                    1 => "109",
                    2 => "110",
                    3 => "111",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"ILSFreqPos Unexpected position switch value {position} for dial value of 1"),
                },

                2 => position switch
                {
                    0 => "10",
                    1 => "15",
                    2 => "30",
                    3 => "35",
                    4 => "50",
                    5 => "55",
                    6 => "70",
                    7 => "75",
                    8 => "90",
                    9 => "95",
                    _ => throw new ArgumentOutOfRangeException(nameof(position), $"ILSFreqPos Unexpected position switch value {position} for dial value of 2"),
                },

                _ => string.Empty
            };
        }

        public static int GetILSDialPosForFrequency(int dial, int freq)
        {
            // 1 Mhz   "108" "109" "110" "111"
            // 0     1     2     3
            // 2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            // 0    1    2    3    4    5    6    7    8    9
            return dial switch
            {
                1 => freq switch
                {
                    108 => 0,
                    109 => 1,
                    110 => 2,
                    111 => 3,
                    _ => throw new ArgumentOutOfRangeException(nameof(freq), $"ILSPosFreq Unexpected position switch value {freq} for dial value of 1")
                },

                // 2 Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                // 0    1    2    3    4    5    6    7    8    9
                2 => freq switch
                {
                    10 => 0,
                    15 => 1,
                    30 => 2,
                    35 => 3,
                    50 => 4,
                    55 => 5,
                    70 => 6,
                    75 => 7,
                    90 => 8,
                    95 => 9,
                    _ => throw new ArgumentOutOfRangeException(nameof(freq), $"ILSPosFreq Unexpected position switch value {freq} for dial value of 2")
                },

                _ => 0
            };
        }

        public static string GetCommandDirectionForVhfDial1(int desiredDialPosition, uint actualDialPosition)
        {
            switch (desiredDialPosition)
            {
                case 0:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    // -6
                                    return Decrease;
                                }

                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    // 5
                                    return Increase;
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
                                    return Increase;
                                }

                            case 1:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return Decrease;
                                }

                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 2:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return Decrease;
                                }

                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 3:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return Decrease;
                                }

                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 4:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                {
                                    return Decrease;
                                }

                            case 11:
                            case 12:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 5:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                                {
                                    return Decrease;
                                }

                            case 12:
                                {
                                    return Increase;
                                }
                        }
                        break;
                    }

                case 6:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                {
                                    return Increase;
                                }

                            case 6:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Decrease;
                                }
                        }
                        break;
                    }

                case 7:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                                {
                                    return Decrease;
                                }

                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    return Increase;
                                }

                            case 7:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Decrease;
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
                                {
                                    return Decrease;
                                }

                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return Increase;
                                }

                            case 8:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Decrease;
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
                                {
                                    return Decrease;
                                }

                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return Increase;
                                }

                            case 9:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 10:
                            case 11:
                            case 12:
                                {
                                    return Decrease;
                                }
                        }
                        break;
                    }

                case 10:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                                {
                                    return Decrease;
                                }

                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return Increase;
                                }

                            case 10:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 11:
                            case 12:
                                {
                                    return Decrease;
                                }
                        }
                        break;
                    }

                case 11:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                {
                                    return Decrease;
                                }

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                {
                                    return Increase;
                                }

                            case 11:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 12:
                                {
                                    return Decrease;
                                }
                        }
                        break;
                    }

                case 12:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                {
                                    return Decrease;
                                }

                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                                {
                                    return Increase;
                                }

                            case 12:
                                {
                                    // Do nothing
                                    return null;
                                }
                        }
                        break;
                    }
            }
            throw new Exception("Should not reach this code. private String GetCommandDirectionForVhfDial1(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        public static string GetCommandDirectionForVhfDial23(int desiredDialPosition, uint actualDialPosition)
        {
            switch (desiredDialPosition)
            {
                case 0:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                {
                                    // -4 DEC
                                    return Decrease;
                                }

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    // 5 INC
                                    return Increase;
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
                                    return Increase;
                                }

                            case 1:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                {
                                    return Decrease;
                                }

                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 2:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    return Decrease;
                                }

                            case 7:
                            case 8:
                            case 9:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 3:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return Decrease;
                                }

                            case 8:
                            case 9:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 4:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return Decrease;
                                }

                            case 9:
                                {
                                    return Increase;
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
                                    return Increase;
                                }

                            case 5:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return Decrease;
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
                                    return Decrease;
                                }

                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                {
                                    return Increase;
                                }

                            case 6:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 7:
                            case 8:
                            case 9:
                                {
                                    return Decrease;
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
                                    return Decrease;
                                }

                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    return Increase;
                                }

                            case 7:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 8:
                            case 9:
                                {
                                    return Decrease;
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
                                    return Decrease;
                                }

                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return Increase;
                                }

                            case 8:
                                {
                                    // Do nothing
                                    return null;
                                }

                            case 9:
                                {
                                    return Decrease;
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
                                    return Decrease;
                                }

                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return Increase;
                                }

                            case 9:
                                {
                                    // Do nothing
                                    return null;
                                }
                        }
                        break;
                    }
            }
            throw new Exception("Should not reach this code. private String GetCommandDirectionForVhfDial23(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
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
                            uint dial4 = _vhfAmCockpitFreq4DialPos switch
                            {
                                0 => 0,
                                1 => 25,
                                2 => 50,
                                3 => 75,
                                _ => throw new Exception($"Unexpected _vhfAmCockpitFreq4DialPos position [{_vhfAmCockpitFreq4DialPos}] in SaveCockpitFrequencyVhfAm")
                            };
                            _vhfAmSavedCockpitBigFrequency = double.Parse((_vhfAmCockpitFreq1DialPos + 3) + _vhfAmCockpitFreq2DialPos.ToString(), NumberFormatInfoFullDisplay);
                            _vhfAmSavedCockpitSmallFrequency = double.Parse(_vhfAmCockpitFreq3DialPos + dial4.ToString(NumberFormatInfoFullDisplay).PadLeft(2, '0'), NumberFormatInfoFullDisplay);
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
                string bigFrequencyAsString;
                var smallFrequencyAsString = string.Empty;
                lock (_lockUhfDialsObject1)
                {
                    bigFrequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfCockpitFreq1DialPos);

                }

                lock (_lockUhfDialsObject2)
                {
                    bigFrequencyAsString += GetUhfDialFrequencyForPosition(2, _uhfCockpitFreq2DialPos);

                }

                lock (_lockUhfDialsObject3)
                {
                    bigFrequencyAsString += GetUhfDialFrequencyForPosition(3, _uhfCockpitFreq3DialPos);

                }

                lock (_lockUhfDialsObject4)
                {
                    smallFrequencyAsString += GetUhfDialFrequencyForPosition(4, _uhfCockpitFreq4DialPos);
                }

                lock (_lockUhfDialsObject5)
                {
                    smallFrequencyAsString += GetUhfDialFrequencyForPosition(5, _uhfCockpitFreq5DialPos);
                }


                _uhfSavedCockpitBigFrequency = double.Parse(bigFrequencyAsString, NumberFormatInfoFullDisplay);
                _uhfSavedCockpitSmallFrequency = double.Parse(smallFrequencyAsString, NumberFormatInfoFullDisplay);




            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "SaveCockpitFrequencyUhf()");
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
                            uint dial4 = _vhfFmCockpitFreq4DialPos switch
                            {
                                0 => 0,
                                1 => 25,
                                2 => 50,
                                3 => 75,
                                _ => throw new Exception($"Unexpected _vhfFmCockpitFreq4DialPos position [{_vhfFmCockpitFreq4DialPos}] in SaveCockpitFrequencyVhfFm")
                            };
                            _vhfFmSavedCockpitBigFrequency = uint.Parse((_vhfFmCockpitFreq1DialPos + 3) + _vhfFmCockpitFreq2DialPos.ToString(), NumberFormatInfoFullDisplay);
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
            // Large dial 108-111 [step of 1]
            // Small dial 10-95 [step of 5]
            // "108" "109" "110" "111"
            // 0     1      2    3 
            // "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
            // 0    1    2    3    4    5    6    7    8   9
            lock (_lockIlsDialsObject1)
            {
                lock (_lockIlsDialsObject2)
                {
                    _ilsSavedCockpitBigFrequency = uint.Parse(GetILSDialFrequencyForPosition(1, _ilsCockpitFreq1DialPos));
                    _ilsSavedCockpitSmallFrequency = uint.Parse(GetILSDialFrequencyForPosition(2, _ilsCockpitFreq2DialPos));
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
            // Large dial 0-12 [step of 1]
            // Small dial 0-9 [step of 1]
            // Last : X/Y [0,1]
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

        private bool VhfAmNowSyncing()
        {
            return Interlocked.Read(ref _vhfAmThreadNowSynching) > 0;
        }

        private bool UhfNowSyncing()
        {
            return Interlocked.Read(ref _uhfThreadNowSynching) > 0;
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
    }
}
