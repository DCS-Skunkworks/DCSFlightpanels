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
    using DCS_BIOS.Serialized;
    using DCS_BIOS.ControlLocator;


    /// <summary>
    /// Pre-programmed radio panel for the T-45C. 
    /// </summary>
    public class RadioPanelPZ69T45C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentT45RadioMode
        {
            VUHF1,
            VUHF2,
            TACAN,
            VOR,
            NO_USE
        }

        private CurrentT45RadioMode _currentUpperRadioMode = CurrentT45RadioMode.VUHF1;
        private CurrentT45RadioMode _currentLowerRadioMode = CurrentT45RadioMode.VUHF1;

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;
        private bool _ignoreUpperButtonOnce = true;
        private bool _ignoreLowerButtonOnce = true;

        /* COMM 1 AN/ARC-182 VHF UHF (sort of.....)
            Large dial
             30-87[.975] 
             108-173[.975] 
             225-399[.975]

            Small dial 
             0.00-0.975 [step of x.x[0 2 5 7]

         */
        private uint _vuhf1CockpitBigFrequency;
        private uint _vuhf1CockpitDial1Frequency;
        private uint _vuhf1CockpitDial2Frequency;
        private uint _vuhf1CockpitDial3Frequency;
        private uint _vuhf1CockpitDial4Frequency;
        private uint _vuhf1SavedCockpitDial1Frequency;
        private uint _vuhf1SavedCockpitDial2Frequency;
        private uint _vuhf1SavedCockpitDial3Frequency;
        private uint _vuhf1SavedCockpitDial4Frequency;
        private uint _vuhf1BigFrequencyStandby = 249;
        private uint _vuhf1SmallFrequencyStandby;
        private readonly object _lockVuhf1BigFreqObject = new();
        private readonly object _lockVuhf1Dial3FreqObject = new();
        private readonly object _lockVuhf1Dial4FreqObject = new();
        private DCSBIOSOutput _vuhf1DcsbiosOutputBigFrequencyNumber;
        private DCSBIOSOutput _vuhf1DcsbiosOutputDial3FrequencyNumber;
        private DCSBIOSOutput _vuhf1DcsbiosOutputDial4FrequencyNumber;

        private const string VUHF1_1ST_DIAL_INCREASE = "COMM_1_FREQ_10 INC\n";
        private const string VUHF1_1ST_DIAL_DECREASE = "COMM_1_FREQ_10 DEC\n";
        private const string VUHF1_1ST_DIAL_NEUTRAL = "COMM_1_FREQ_10 1\n";

        private const string VUHF1_2ND_DIAL_INCREASE = "COMM_1_FREQ_1 INC\n";
        private const string VUHF1_2ND_DIAL_DECREASE = "COMM_1_FREQ_1 DEC\n";
        private const string VUHF1_2ND_DIAL_NEUTRAL = "COMM_1_FREQ_1 1\n";

        private const string VUHF1_3RD_DIAL_INCREASE = "COMM_1_FREQ_010 INC\n";
        private const string VUHF1_3RD_DIAL_DECREASE = "COMM_1_FREQ_010 DEC\n";
        private const string VUHF1_3RD_DIAL_NEUTRAL = "COMM_1_FREQ_010 1\n";

        private const string VUHF1_4TH_DIAL_INCREASE = "COMM_1_FREQ_100 INC\n";
        private const string VUHF1_4TH_DIAL_DECREASE = "COMM_1_FREQ_100 DEC\n";
        private const string VUHF1_4TH_DIAL_NEUTRAL = "COMM_1_FREQ_100 1\n";

        /*private DCSBIOSOutput _vuhf1DcsbiosOutputMode;
        private volatile uint _vuhf1CockpitMode; // OFF = 0*/
        //private readonly ClickSpeedDetector _vuhf1ModeClickSpeedDetector = new(8);
        private byte _skipVuhf1SmallFreqChange;
        private long _vuhf1ThreadNowSynching;
        private Thread _vuhf1SyncThread;
        private long _vuhf1Dial1WaitingForFeedback;
        private long _vuhf1Dial2WaitingForFeedback;
        private long _vuhf1Dial3WaitingForFeedback;
        private long _vuhf1Dial4WaitingForFeedback;

        /* COMM 2 AN/ARC-182 VHF UHF  (sort of.....)
            Large dial
             30-87[.975] 
             108-173[.975] 
             225-399[.975]

            Small dial 
             0.00-0.975 [step of x.x[0 2 5 7]

         */
        private uint _vuhf2CockpitBigFrequency;
        private uint _vuhf2CockpitDial1Frequency;
        private uint _vuhf2CockpitDial2Frequency;
        private uint _vuhf2CockpitDial3Frequency;
        private uint _vuhf2CockpitDial4Frequency;
        private uint _vuhf2SavedCockpitDial1Frequency;
        private uint _vuhf2SavedCockpitDial2Frequency;
        private uint _vuhf2SavedCockpitDial3Frequency;
        private uint _vuhf2SavedCockpitDial4Frequency;
        private uint _vuhf2BigFrequencyStandby = 249;
        private uint _vuhf2SmallFrequencyStandby;
        private readonly object _lockVuhf2BigFreqObject = new();
        private readonly object _lockVuhf2Dial3FreqObject = new();
        private readonly object _lockVuhf2Dial4FreqObject = new();
        private DCSBIOSOutput _vuhf2DcsbiosOutputBigFrequencyNumber;
        private DCSBIOSOutput _vuhf2DcsbiosOutputDial3FrequencyNumber;
        private DCSBIOSOutput _vuhf2DcsbiosOutputDial4FrequencyNumber;

        private const string VUHF2_1ST_DIAL_INCREASE = "COMM_2_FREQ_10 INC\n";
        private const string VUHF2_1ST_DIAL_DECREASE = "COMM_2_FREQ_10 DEC\n";
        private const string VUHF2_1ST_DIAL_NEUTRAL = "COMM_2_FREQ_10 1\n";

        private const string VUHF2_2ND_DIAL_INCREASE = "COMM_2_FREQ_1 INC\n";
        private const string VUHF2_2ND_DIAL_DECREASE = "COMM_2_FREQ_1 DEC\n";
        private const string VUHF2_2ND_DIAL_NEUTRAL = "COMM_2_FREQ_1 1\n";

        private const string VUHF2_3RD_DIAL_INCREASE = "COMM_2_FREQ_010 INC\n";
        private const string VUHF2_3RD_DIAL_DECREASE = "COMM_2_FREQ_010 DEC\n";
        private const string VUHF2_3RD_DIAL_NEUTRAL = "COMM_2_FREQ_010 1\n";

        private const string VUHF2_4TH_DIAL_INCREASE = "COMM_2_FREQ_100 INC\n";
        private const string VUHF2_4TH_DIAL_DECREASE = "COMM_2_FREQ_100 DEC\n";
        private const string VUHF2_4TH_DIAL_NEUTRAL = "COMM_2_FREQ_100 1\n";

        /*private DCSBIOSOutput _vuhf2DcsbiosOutputMode;
        private volatile uint _vuhf2CockpitMode; // OFF = 0
        private readonly ClickSpeedDetector _vuhf2ModeClickSpeedDetector = new(8);*/
        private byte _skipVuhf2SmallFreqChange;
        private long _vuhf2ThreadNowSynching;
        private Thread _vuhf2SyncThread;
        private long _vuhf2Dial1WaitingForFeedback;
        private long _vuhf2Dial2WaitingForFeedback;
        private long _vuhf2Dial3WaitingForFeedback;
        private long _vuhf2Dial4WaitingForFeedback;

        /* NAV 1 TACAN */
        // Tens dial 0-12 [step of 1]
        // Ones dial 0-9 [step of 1]
        private int _tacanTensFrequencyStandby = 0;
        private int _tacanOnesFrequencyStandby = 1;
        private int _tacanSavedCockpitTensFrequency = 0;
        private int _tacanSavedCockpitOnesFrequency = 1;
        private readonly object _lockTacanTensDialObject = new();
        private readonly object _lockTacanOnesObject = new();
        private DCSBIOSOutput _tacanDcsbiosOutputTensDial;
        private DCSBIOSOutput _tacanDcsbiosOutputOnesDial;
        private volatile uint _tacanCockpitTensDialPos = 1;
        private volatile uint _tacanCockpitOnesDialPos = 1;
        private const string TACAN_TENS_DIAL_COMMAND = "TACAN_CHAN_10 ";
        private const string TACAN_ONES_DIAL_COMMAND = "TACAN_CHAN_1 ";
        private Thread _tacanSyncThread;
        private long _tacanThreadNowSynching;
        private long _tacanTensWaitingForFeedback;
        private long _tacanOnesWaitingForFeedback;

        /* NAV 2 AN/ARN-144 VOR */
        // Mhz dial 0-9 [step of 1]
        // Khz dial 0-19 [step of 1]
        private int _vorMhzFrequencyStandby = 108;
        private int _vorKhzFrequencyStandby = 10;
        private int _vorSavedCockpitMhzFrequency = 108;
        private int _vorSavedCockpitKhzFrequency = 10;
        private readonly object _lockVorMhzDialObject = new();
        private readonly object _lockVorKhzObject = new();
        private DCSBIOSOutput _vorDcsbiosOutputMhzDial;
        private DCSBIOSOutput _vorDcsbiosOutputKhzDial;
        private volatile uint _vorCockpitMhzDialPos = 0;
        private volatile uint _vorCockpitKhzDialPos = 2;
        private const string VOR_MHZ_DIAL_COMMAND = "VOR_ILS_FREQ_1 ";
        private const string VOR_KHZ_DIAL_COMMAND = "VOR_ILS_FREQ_50 ";
        private Thread _vorSyncThread;
        private long _vorThreadNowSynching;
        private long _vorMhzWaitingForFeedback;
        private long _vorKhzWaitingForFeedback;

        private readonly object _lockShowFrequenciesOnPanelObject = new();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69T45C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                    _shutdownVUHF1Thread = true;
                    _shutdownTACANThread = true;
                    _shutdownVORThread = true;
                    _shutdownVUHF2Thread = true;
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

            // COMM 1 VUHF1
            _vuhf1DcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM_1_HIGH_FREQ");
            _vuhf1DcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM_1_DIAL3_FREQ");
            _vuhf1DcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM_1_DIAL4_FREQ");

            // COMM 2 VUHF2
            _vuhf2DcsbiosOutputBigFrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM_2_HIGH_FREQ");
            _vuhf2DcsbiosOutputDial3FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM_2_DIAL3_FREQ");
            _vuhf2DcsbiosOutputDial4FrequencyNumber = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM_2_DIAL4_FREQ");

            // NAV 1 TACAN
            _tacanDcsbiosOutputTensDial = DCSBIOSControlLocator.GetStringDCSBIOSOutput("TACAN_CHAN_10");
            _tacanDcsbiosOutputOnesDial = DCSBIOSControlLocator.GetStringDCSBIOSOutput("TACAN_CHAN_1");

            // NAV 2 VOR
            _vorDcsbiosOutputMhzDial = DCSBIOSControlLocator.GetStringDCSBIOSOutput("VOR_ILS_FREQ_1");
            _vorDcsbiosOutputKhzDial = DCSBIOSControlLocator.GetStringDCSBIOSOutput("VOR_ILS_FREQ_50");

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
            // COMM 1 VHF UHF
            if (_vuhf1DcsbiosOutputBigFrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhf1BigFreqObject)
                {
                    _vuhf1CockpitBigFrequency = _vuhf1DcsbiosOutputBigFrequencyNumber.LastUIntValue;
                    var asString = _vuhf1CockpitBigFrequency.ToString().PadLeft(3, '0');
                    _vuhf1CockpitDial1Frequency = uint.Parse(asString.Substring(0, 2));
                    _vuhf1CockpitDial2Frequency = uint.Parse(asString.Substring(2, 1));
                    Interlocked.Exchange(ref _vuhf1Dial1WaitingForFeedback, 0);
                    Interlocked.Exchange(ref _vuhf1Dial2WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhf1DcsbiosOutputDial3FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhf1Dial3FreqObject)
                {
                    _vuhf1CockpitDial3Frequency = _vuhf1DcsbiosOutputDial3FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _vuhf1Dial3WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhf1DcsbiosOutputDial4FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhf1Dial4FreqObject)
                {
                    _vuhf1CockpitDial4Frequency = _vuhf1DcsbiosOutputDial4FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _vuhf1Dial4WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            // COMM 2 VHF UHF
            if (_vuhf2DcsbiosOutputBigFrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhf2BigFreqObject)
                {
                    _vuhf2CockpitBigFrequency = _vuhf2DcsbiosOutputBigFrequencyNumber.LastUIntValue;
                    var asString = _vuhf2CockpitBigFrequency.ToString().PadLeft(3, '0');
                    _vuhf2CockpitDial1Frequency = uint.Parse(asString.Substring(0, 2));
                    _vuhf2CockpitDial2Frequency = uint.Parse(asString.Substring(2, 1));
                    Interlocked.Exchange(ref _vuhf2Dial1WaitingForFeedback, 0);
                    Interlocked.Exchange(ref _vuhf2Dial2WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhf2DcsbiosOutputDial3FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhf2Dial3FreqObject)
                {
                    _vuhf2CockpitDial3Frequency = _vuhf2DcsbiosOutputDial3FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _vuhf2Dial3WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            if (_vuhf2DcsbiosOutputDial4FrequencyNumber.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVuhf2Dial4FreqObject)
                {
                    _vuhf2CockpitDial4Frequency = _vuhf2DcsbiosOutputDial4FrequencyNumber.LastUIntValue;
                    Interlocked.Exchange(ref _vuhf2Dial4WaitingForFeedback, 0);
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }

            // NAV 1 TACAN
            if (_tacanDcsbiosOutputTensDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _tacanCockpitTensDialPos = _tacanDcsbiosOutputTensDial.LastUIntValue;
                Interlocked.Exchange(ref _tacanTensWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_tacanDcsbiosOutputOnesDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _tacanCockpitOnesDialPos = _tacanDcsbiosOutputOnesDial.LastUIntValue;
                Interlocked.Exchange(ref _tacanOnesWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            // NAV 2 VOR
            if (_vorDcsbiosOutputMhzDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _vorCockpitMhzDialPos = _vorDcsbiosOutputMhzDial.LastUIntValue;
                Interlocked.Exchange(ref _vorMhzWaitingForFeedback, 0);
                Interlocked.Add(ref _doUpdatePanelLCD, 5);
            }

            if (_vorDcsbiosOutputKhzDial.UIntValueHasChanged(e.Address, e.Data))
            {
                _vorCockpitKhzDialPos = _vorDcsbiosOutputKhzDial.LastUIntValue;
                Interlocked.Exchange(ref _vorKhzWaitingForFeedback, 0);
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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsT45C knob)
        {

            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsT45C.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsT45C.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsT45C.UPPER_FREQ_SWITCH:
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
                            case CurrentT45RadioMode.VUHF1:
                                {
                                    SendVUHF1ToDCSBIOS();
                                    ShowFrequenciesOnPanel();
                                    break;
                                }

                            case CurrentT45RadioMode.VUHF2:
                                {
                                    SendVUHF2ToDCSBIOS();
                                    ShowFrequenciesOnPanel();
                                    break;
                                }

                            case CurrentT45RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }

                            case CurrentT45RadioMode.VOR:
                                {
                                    SendVorToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelPZ69KnobsT45C.LOWER_FREQ_SWITCH:
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
                            case CurrentT45RadioMode.VUHF1:
                                {
                                    SendVUHF1ToDCSBIOS();
                                    ShowFrequenciesOnPanel();
                                    break;
                                }

                            case CurrentT45RadioMode.VUHF2:
                                {
                                    SendVUHF2ToDCSBIOS();
                                    ShowFrequenciesOnPanel();
                                    break;
                                }

                            case CurrentT45RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }

                            case CurrentT45RadioMode.VOR:
                                {
                                    SendVorToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendVUHF1ToDCSBIOS()
        {
            if (VUHF1NowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyVuhf1();
            var frequencyAsString = _vuhf1BigFrequencyStandby + "." + _vuhf1SmallFrequencyStandby.ToString().PadLeft(3, '0');

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

            _shutdownVUHF1Thread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVUHF1Thread = false;
            _vuhf1SyncThread = new Thread(() => VUHF1SynchThreadMethod(desiredDial1Value, desiredDial2Value, desiredDial3Value, desiredDial4Value));
            _vuhf1SyncThread.Start();
        }

        private volatile bool _shutdownVUHF1Thread;
        private void VUHF1SynchThreadMethod(int desiredValueDial1, int desiredValueDial2, int desiredValueDial3, int desiredValueDial4)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _vuhf1ThreadNowSynching, 1);
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
                            ResetWaitingForFeedBack(ref _vuhf1Dial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhf1Dial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhf1Dial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial4Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhf1Dial4WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _vuhf1Dial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf1BigFreqObject)
                            {
                                if (_vuhf1CockpitDial1Frequency != desiredValueDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf1CockpitDial1Frequency < desiredValueDial1 ? VUHF1_1ST_DIAL_INCREASE : VUHF1_1ST_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF1_1ST_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhf1Dial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhf1Dial2WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf1BigFreqObject)
                            {
                                if (_vuhf1CockpitDial2Frequency != desiredValueDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf1CockpitDial2Frequency < desiredValueDial2 ? VUHF1_2ND_DIAL_INCREASE : VUHF1_2ND_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF1_2ND_DIAL_NEUTRAL);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vuhf1Dial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhf1Dial3WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf1Dial3FreqObject)
                            {
                                if (_vuhf1CockpitDial3Frequency != desiredValueDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf1CockpitDial3Frequency < desiredValueDial3 ? VUHF1_3RD_DIAL_INCREASE : VUHF1_3RD_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF1_3RD_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhf1Dial3WaitingForFeedback, 1);
                                }
                            }
                            Reset(ref dial3Timeout);
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhf1Dial4WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf1Dial4FreqObject)
                            {
                                if (_vuhf1CockpitDial4Frequency != desiredValueDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf1CockpitDial4Frequency < desiredValueDial4 ? VUHF1_4TH_DIAL_INCREASE : VUHF1_4TH_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF1_4TH_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhf1Dial4WaitingForFeedback, 1);
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
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownVUHF1Thread);
                    SwapCockpitStandbyFrequencyVuhf1();
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
                Interlocked.Exchange(ref _vuhf1ThreadNowSynching, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendVUHF2ToDCSBIOS()
        {
            if (VUHF2NowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyVuhf2();
            var frequencyAsString = _vuhf2BigFrequencyStandby + "." + _vuhf2SmallFrequencyStandby.ToString().PadLeft(3, '0');

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

            _shutdownVUHF2Thread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVUHF2Thread = false;
            _vuhf2SyncThread = new Thread(() => VUHF2SynchThreadMethod(desiredDial1Value, desiredDial2Value, desiredDial3Value, desiredDial4Value));
            _vuhf2SyncThread.Start();
        }

        private volatile bool _shutdownVUHF2Thread;
        private void VUHF2SynchThreadMethod(int desiredValueDial1, int desiredValueDial2, int desiredValueDial3, int desiredValueDial4)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _vuhf2ThreadNowSynching, 1);
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
                            ResetWaitingForFeedBack(ref _vuhf2Dial1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhf2Dial2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial3Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhf2Dial3WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial4Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vuhf2Dial4WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _vuhf2Dial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf2BigFreqObject)
                            {
                                if (_vuhf2CockpitDial1Frequency != desiredValueDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf2CockpitDial1Frequency < desiredValueDial1 ? VUHF2_1ST_DIAL_INCREASE : VUHF2_1ST_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF2_1ST_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhf2Dial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhf2Dial2WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf2BigFreqObject)
                            {
                                if (_vuhf2CockpitDial2Frequency != desiredValueDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf2CockpitDial2Frequency < desiredValueDial2 ? VUHF2_2ND_DIAL_INCREASE : VUHF2_2ND_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF2_2ND_DIAL_NEUTRAL);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vuhf2Dial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhf2Dial3WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf2Dial3FreqObject)
                            {
                                if (_vuhf2CockpitDial3Frequency != desiredValueDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf2CockpitDial3Frequency < desiredValueDial3 ? VUHF2_3RD_DIAL_INCREASE : VUHF2_3RD_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF2_3RD_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhf2Dial3WaitingForFeedback, 1);
                                }
                            }
                            Reset(ref dial3Timeout);
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vuhf2Dial4WaitingForFeedback) == 0)
                        {
                            lock (_lockVuhf2Dial4FreqObject)
                            {
                                if (_vuhf2CockpitDial4Frequency != desiredValueDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    DCSBIOS.Send(_vuhf2CockpitDial4Frequency < desiredValueDial4 ? VUHF2_4TH_DIAL_INCREASE : VUHF2_4TH_DIAL_DECREASE);
                                    DCSBIOS.Send(VUHF2_4TH_DIAL_NEUTRAL);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vuhf2Dial4WaitingForFeedback, 1);
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
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownVUHF2Thread);
                    SwapCockpitStandbyFrequencyVuhf2();
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
                Interlocked.Exchange(ref _vuhf2ThreadNowSynching, 0);
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
            // Frequency selector 1      BOTTOM
            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

            // Frequency selector 2      TOP
            // 0 1 2 3 4 5 6 7 8 9

            // 120
            // #1 = 12  (position = value)
            // #2 = 0   (position = value)
            _shutdownTACANThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownTACANThread = false;
            _tacanSyncThread = new Thread(() => TacanSynchThreadMethod(_tacanTensFrequencyStandby, _tacanOnesFrequencyStandby));
            _tacanSyncThread.Start();
        }

        private volatile bool _shutdownTACANThread;
        private void TacanSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _tacanThreadNowSynching, 1);

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
                            ResetWaitingForFeedBack(ref _tacanTensWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _tacanOnesWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _tacanTensWaitingForFeedback) == 0)
                        {
                            lock (_lockTacanTensDialObject)
                            {
                                if (_tacanCockpitTensDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = TACAN_TENS_DIAL_COMMAND + (_tacanCockpitTensDialPos < desiredPositionDial1 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _tacanTensWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _tacanOnesWaitingForFeedback) == 0)
                        {
                            // Common.DebugP("b");
                            lock (_lockTacanOnesObject)
                            {
                                if (_tacanCockpitOnesDialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;

                                    var str = TACAN_ONES_DIAL_COMMAND + (_tacanCockpitOnesDialPos < desiredPositionDial2 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _tacanOnesWaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }
                        if (dial1SendCount > 12 || dial2SendCount > 10 /*|| dial3SendCount > 2*/)
                        {
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime)) && !_shutdownTACANThread);
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

        private void SendVorToDCSBIOS()
        {
            if (VorNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyVor();
            _shutdownVORThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVORThread = false;
            _vorSyncThread = new Thread(() => VorSynchThreadMethod(_vorMhzFrequencyStandby, _vorKhzFrequencyStandby));
            _vorSyncThread.Start();
        }

        private volatile bool _shutdownVORThread;
        private void VorSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _vorThreadNowSynching, 1);

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
                            ResetWaitingForFeedBack(ref _vorMhzWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vorKhzWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _vorMhzWaitingForFeedback) == 0)
                        {

                            lock (_lockVorMhzDialObject)
                            {
                                if (_vorCockpitMhzDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = VOR_MHZ_DIAL_COMMAND + (_vorCockpitMhzDialPos < desiredPositionDial1 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vorMhzWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vorKhzWaitingForFeedback) == 0)
                        {
                            // Common.DebugP("b");
                            lock (_lockVorKhzObject)
                            {
                                if (_vorCockpitKhzDialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;

                                    var str = VOR_KHZ_DIAL_COMMAND + (_vorCockpitKhzDialPos < desiredPositionDial2 ? DCSBIOS_INCREASE_COMMAND : DCSBIOS_DECREASE_COMMAND);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vorKhzWaitingForFeedback, 1);
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
                    while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime)) && !_shutdownVORThread);
                    SwapStandbyFrequencyVor();
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
                Interlocked.Exchange(ref _vorThreadNowSynching, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        /// <summary>
        /// 
        /// </summary>
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
                    case CurrentT45RadioMode.VUHF1:
                        {
                            var frequencyAsString = GetVUHF1CockpitFrequencyAsString();
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vuhf1BigFrequencyStandby + (((double)_vuhf1SmallFrequencyStandby) / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.VUHF2:
                        {
                            var frequencyAsString = GetVUHF2CockpitFrequencyAsString();
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vuhf2BigFrequencyStandby + (((double)_vuhf2SmallFrequencyStandby) / 1000), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.TACAN:
                        {
                            // TACAN  00 --> 129
                            // Frequency selector 1      TOP
                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                            // Frequency selector 2      BOTTOM
                            // 0 1 2 3 4 5 6 7 8 9

                            string frequencyAsString;
                            lock (_lockTacanTensDialObject)
                            {
                                lock (_lockTacanOnesObject)
                                {
                                    frequencyAsString = _tacanCockpitTensDialPos.ToString() + _tacanCockpitOnesDialPos.ToString();
                                }
                            }
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, uint.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, uint.Parse(_tacanTensFrequencyStandby + _tacanOnesFrequencyStandby.ToString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.VOR:
                        {
                            string frequencyAsString;
                            lock (_lockVorMhzDialObject)
                            {
                                lock (_lockVorKhzObject)
                                {
                                    frequencyAsString = (108 + _vorCockpitMhzDialPos).ToString() + "." + (5 * _vorCockpitKhzDialPos).ToString("D2");
                                }
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse((108 + _vorMhzFrequencyStandby).ToString() + "." + (5 * _vorKhzFrequencyStandby).ToString("D2"), NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentT45RadioMode.VUHF1:
                        {
                            var frequencyAsString = GetVUHF1CockpitFrequencyAsString();
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vuhf1BigFrequencyStandby + (((double)_vuhf1SmallFrequencyStandby) / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.VUHF2:
                        {
                            var frequencyAsString = GetVUHF2CockpitFrequencyAsString();
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vuhf2BigFrequencyStandby + (((double)_vuhf2SmallFrequencyStandby) / 1000), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.TACAN:
                        {
                            string frequencyAsString;
                            lock (_lockTacanTensDialObject)
                            {
                                lock (_lockTacanOnesObject)
                                {
                                    frequencyAsString = _tacanCockpitTensDialPos.ToString() + _tacanCockpitOnesDialPos.ToString();
                                }
                            }
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, uint.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, uint.Parse(_tacanTensFrequencyStandby + _tacanOnesFrequencyStandby.ToString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.VOR:
                        {
                            string frequencyAsString;
                            lock (_lockVorMhzDialObject)
                            {
                                lock (_lockVorKhzObject)
                                {
                                    frequencyAsString = (108 + _vorCockpitMhzDialPos).ToString() + "." + (5 * _vorCockpitKhzDialPos).ToString("D2");
                                }
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse((108 + _vorMhzFrequencyStandby).ToString() + "." + (5 * _vorKhzFrequencyStandby).ToString("D2"), NumberFormatInfoFullDisplay), 2, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentT45RadioMode.NO_USE:
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

        private string GetVUHF1CockpitFrequencyAsString()
        {
            string frequencyAsString;
            lock (_lockVuhf1BigFreqObject)
            {
                lock (_lockVuhf1Dial3FreqObject)
                {
                    lock (_lockVuhf1Dial4FreqObject)
                    {
                        frequencyAsString = _vuhf1CockpitBigFrequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += ".";
                        frequencyAsString += _vuhf1CockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += _vuhf1CockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');

                        // 225.000 7 characters
                    }
                }
            }
            return frequencyAsString;
        }

        private string GetVUHF2CockpitFrequencyAsString()
        {
            string frequencyAsString;
            lock (_lockVuhf2BigFreqObject)
            {
                lock (_lockVuhf2Dial3FreqObject)
                {
                    lock (_lockVuhf2Dial4FreqObject)
                    {
                        frequencyAsString = _vuhf2CockpitBigFrequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += ".";
                        frequencyAsString += _vuhf2CockpitDial3Frequency.ToString(CultureInfo.InvariantCulture);
                        frequencyAsString += _vuhf2CockpitDial4Frequency.ToString(CultureInfo.InvariantCulture).PadRight(2, '0');

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
                var radioPanelKnobT45 = (RadioPanelKnobT45C)o;
                if (radioPanelKnobT45.IsOn)
                {
                    switch (radioPanelKnobT45.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsT45C.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                AdjustVUHF1BigFrequency(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                AdjustVUHF2BigFrequency(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanTensFrequencyStandby >= 12)
                                            {
                                                _tacanTensFrequencyStandby = 12;
                                                break;
                                            }

                                            _tacanTensFrequencyStandby++;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorMhzFrequencyStandby >= 9)
                                            {
                                                _vorMhzFrequencyStandby = 9;
                                                break;
                                            }

                                            _vorMhzFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                AdjustVUHF1BigFrequency(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                AdjustVUHF2BigFrequency(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanTensFrequencyStandby <= 0)
                                            {
                                                _tacanTensFrequencyStandby = 0;
                                                break;
                                            }

                                            _tacanTensFrequencyStandby--;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorMhzFrequencyStandby <= 0)
                                            {
                                                _vorMhzFrequencyStandby = 0;
                                                break;
                                            }

                                            _vorMhzFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                VUHF1SmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                VUHF2SmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanOnesFrequencyStandby >= 9)
                                            {
                                                _tacanOnesFrequencyStandby = 9;
                                                break;
                                            }

                                            _tacanOnesFrequencyStandby++;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorKhzFrequencyStandby >= 19)
                                            {
                                                _vorKhzFrequencyStandby = 19;
                                                break;
                                            }

                                            _vorKhzFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                VUHF1SmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }


                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_upperButtonPressed)
                                            {
                                                VUHF2SmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanOnesFrequencyStandby <= 0)
                                            {
                                                _tacanOnesFrequencyStandby = 0;
                                                break;
                                            }

                                            _tacanOnesFrequencyStandby--;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorKhzFrequencyStandby <= 0)
                                            {
                                                _vorKhzFrequencyStandby = 0;
                                                break;
                                            }

                                            _vorKhzFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_lowerButtonPressed)
                                            {
                                                AdjustVUHF1BigFrequency(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_lowerButtonPressed)
                                            {
                                                AdjustVUHF2BigFrequency(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanTensFrequencyStandby >= 12)
                                            {
                                                _tacanTensFrequencyStandby = 12;
                                                break;
                                            }

                                            _tacanTensFrequencyStandby++;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorMhzFrequencyStandby >= 9)
                                            {
                                                _vorMhzFrequencyStandby = 9;
                                                break;
                                            }

                                            _vorMhzFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {

                                            if (!_lowerButtonPressed)
                                            {
                                                AdjustVUHF1BigFrequency(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {

                                            if (!_lowerButtonPressed)
                                            {
                                                AdjustVUHF2BigFrequency(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanTensFrequencyStandby <= 0)
                                            {
                                                _tacanTensFrequencyStandby = 0;
                                                break;
                                            }

                                            _tacanTensFrequencyStandby--;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorMhzFrequencyStandby <= 0)
                                            {
                                                _vorMhzFrequencyStandby = 0;
                                                break;
                                            }

                                            _vorMhzFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_lowerButtonPressed)
                                            {
                                                VUHF1SmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_lowerButtonPressed)
                                            {
                                                VUHF2SmallFrequencyStandbyAdjust(true);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanOnesFrequencyStandby >= 9)
                                            {
                                                _tacanOnesFrequencyStandby = 9;
                                                break;
                                            }

                                            _tacanOnesFrequencyStandby++;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorKhzFrequencyStandby >= 19)
                                            {
                                                _vorKhzFrequencyStandby = 19;
                                                break;
                                            }

                                            _vorKhzFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentT45RadioMode.VUHF1:
                                        {
                                            if (!_lowerButtonPressed)
                                            {
                                                VUHF1SmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.VUHF2:
                                        {
                                            if (!_lowerButtonPressed)
                                            {
                                                VUHF2SmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }

                                    case CurrentT45RadioMode.TACAN:
                                        {
                                            // TACAN  00 --> 129
                                            // Frequency selector 1      TOP
                                            // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12

                                            // Frequency selector 2      BOTTOM
                                            // 0 1 2 3 4 5 6 7 8 9
                                            if (_tacanOnesFrequencyStandby <= 0)
                                            {
                                                _tacanOnesFrequencyStandby = 0;
                                                break;
                                            }

                                            _tacanOnesFrequencyStandby--;
                                            break;
                                        }

                                    case CurrentT45RadioMode.VOR:
                                        {
                                            if (_vorKhzFrequencyStandby <= 0)
                                            {
                                                _vorKhzFrequencyStandby = 0;
                                                break;
                                            }

                                            _vorKhzFrequencyStandby--;
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

        private void VUHF1SmallFrequencyStandbyAdjust(bool increase)
        {
            _skipVuhf1SmallFreqChange++;
            if (_skipVuhf1SmallFreqChange < 2)
            {
                return;
            }

            _skipVuhf1SmallFreqChange = 0;

            if (increase)
            {
                _vuhf1SmallFrequencyStandby += 25;
            }
            else
            {
                if (_vuhf1SmallFrequencyStandby == 0)
                {
                    _vuhf1SmallFrequencyStandby = 975;
                }
                else
                {
                    _vuhf1SmallFrequencyStandby -= 25;
                }
            }

            if (_vuhf1SmallFrequencyStandby > 975)
            {
                _vuhf1SmallFrequencyStandby = 0;
            }
        }

        private void AdjustVUHF1BigFrequency(bool increase)
        {
            if (increase)
            {
                if (_vuhf1BigFrequencyStandby == 87)
                {
                    _vuhf1BigFrequencyStandby = 108;
                }
                else if (_vuhf1BigFrequencyStandby == 173)
                {
                    _vuhf1BigFrequencyStandby = 225;
                }
                else if (_vuhf1BigFrequencyStandby == 399)
                {
                    _vuhf1BigFrequencyStandby = 30;
                }
                else
                {
                    _vuhf1BigFrequencyStandby++;
                }
            }
            else
            {
                if (_vuhf1BigFrequencyStandby == 30)
                {
                    _vuhf1BigFrequencyStandby = 399;
                }
                else if (_vuhf1BigFrequencyStandby == 225)
                {
                    _vuhf1BigFrequencyStandby = 173;
                }
                else if (_vuhf1BigFrequencyStandby == 108)
                {
                    _vuhf1BigFrequencyStandby = 87;
                }
                else
                {
                    _vuhf1BigFrequencyStandby--;
                }
            }
        }

        private void VUHF2SmallFrequencyStandbyAdjust(bool increase)
        {
            _skipVuhf2SmallFreqChange++;
            if (_skipVuhf2SmallFreqChange < 2)
            {
                return;
            }

            _skipVuhf2SmallFreqChange = 0;

            if (increase)
            {
                _vuhf2SmallFrequencyStandby += 25;
            }
            else
            {
                if (_vuhf2SmallFrequencyStandby == 0)
                {
                    _vuhf2SmallFrequencyStandby = 975;
                }
                else
                {
                    _vuhf2SmallFrequencyStandby -= 25;
                }
            }

            if (_vuhf2SmallFrequencyStandby > 975)
            {
                _vuhf2SmallFrequencyStandby = 0;
            }
        }

        private void AdjustVUHF2BigFrequency(bool increase)
        {
            if (increase)
            {
                if (_vuhf2BigFrequencyStandby == 87)
                {
                    _vuhf2BigFrequencyStandby = 108;
                }
                else if (_vuhf2BigFrequencyStandby == 173)
                {
                    _vuhf2BigFrequencyStandby = 225;
                }
                else if (_vuhf2BigFrequencyStandby == 399)
                {
                    _vuhf2BigFrequencyStandby = 30;
                }
                else
                {
                    _vuhf2BigFrequencyStandby++;
                }
            }
            else
            {
                if (_vuhf2BigFrequencyStandby == 30)
                {
                    _vuhf2BigFrequencyStandby = 399;
                }
                else if (_vuhf2BigFrequencyStandby == 225)
                {
                    _vuhf2BigFrequencyStandby = 173;
                }
                else if (_vuhf2BigFrequencyStandby == 108)
                {
                    _vuhf2BigFrequencyStandby = 87;
                }
                else
                {
                    _vuhf2BigFrequencyStandby--;
                }
            }
        }

        private void CheckFrequenciesForValidity()
        {
            // Crude fix if any freqs are outside the valid boundaries


            // UHF
            // Required until the module has a true ARC-182 implementation
            // 225.00 - 399.975
            if (_vuhf1BigFrequencyStandby < 225)
            {
                _vuhf1BigFrequencyStandby = 225;
            }

            if (_vuhf1BigFrequencyStandby > 399)
            {
                _vuhf1BigFrequencyStandby = 399;
            }

            if (_vuhf2BigFrequencyStandby < 225)
            {
                _vuhf2BigFrequencyStandby = 225;
            }

            if (_vuhf2BigFrequencyStandby > 399)
            {
                _vuhf2BigFrequencyStandby = 399;
            }



            // TACAN
            // 00 - 129
            if (_tacanTensFrequencyStandby < 0)
            {
                _tacanTensFrequencyStandby = 0;
            }

            if (_tacanTensFrequencyStandby > 12)
            {
                _tacanTensFrequencyStandby = 12;
            }

            if (_tacanOnesFrequencyStandby < 0)
            {
                _tacanOnesFrequencyStandby = 0;
            }

            if (_tacanOnesFrequencyStandby > 9)
            {
                _tacanOnesFrequencyStandby = 9;
            }

            // VOR
            // 108.00 - 117.95
            if (_vorMhzFrequencyStandby < 0)
            {
                _vorMhzFrequencyStandby = 0;
            }

            if (_vorMhzFrequencyStandby > 9)
            {
                _vorMhzFrequencyStandby = 9;
            }

            if (_vorKhzFrequencyStandby < 0)
            {
                _vorKhzFrequencyStandby = 0;
            }

            if (_vorKhzFrequencyStandby > 19)
            {
                _vorKhzFrequencyStandby = 19;
            }


        }

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobT45C)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsT45C.UPPER_VUHF1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentT45RadioMode.VUHF1;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_VUHF2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentT45RadioMode.VUHF2;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentT45RadioMode.TACAN;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_VOR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentT45RadioMode.VOR;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_NO_USE1:
                        case RadioPanelPZ69KnobsT45C.UPPER_NO_USE2:
                        case RadioPanelPZ69KnobsT45C.UPPER_NO_USE3:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentT45RadioMode.NO_USE;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_VUHF1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentT45RadioMode.VUHF1;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_VUHF2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentT45RadioMode.VUHF2;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentT45RadioMode.TACAN;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_VOR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentT45RadioMode.VOR;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_NO_USE1:
                        case RadioPanelPZ69KnobsT45C.LOWER_NO_USE2:
                        case RadioPanelPZ69KnobsT45C.LOWER_NO_USE3:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentT45RadioMode.NO_USE;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.UPPER_FREQ_SWITCH:
                            {
                                _upperButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsT45C.UPPER_FREQ_SWITCH);
                                    }

                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsT45C.LOWER_FREQ_SWITCH:
                            {
                                _lowerButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsT45C.LOWER_FREQ_SWITCH);
                                    }

                                    _lowerButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                    }

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_T45C, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
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
            SaitekPanelKnobs = RadioPanelKnobT45C.GetRadioPanelKnobs();
        }

        private void SaveCockpitFrequencyVuhf1()
        {
            lock (_lockVuhf1BigFreqObject)
            {
                lock (_lockVuhf1Dial3FreqObject)
                {
                    lock (_lockVuhf1Dial4FreqObject)
                    {
                        _vuhf1SavedCockpitDial1Frequency = _vuhf1CockpitDial1Frequency;
                        _vuhf1SavedCockpitDial2Frequency = _vuhf1CockpitDial2Frequency;
                        _vuhf1SavedCockpitDial3Frequency = _vuhf1CockpitDial3Frequency;
                        _vuhf1SavedCockpitDial4Frequency = _vuhf1CockpitDial4Frequency;
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVuhf1()
        {
            lock (_lockVuhf1BigFreqObject)
            {
                lock (_lockVuhf1Dial3FreqObject)
                {
                    lock (_lockVuhf1Dial4FreqObject)
                    {
                        _vuhf1BigFrequencyStandby = _vuhf1SavedCockpitDial1Frequency * 10 + _vuhf1SavedCockpitDial2Frequency;
                        _vuhf1SmallFrequencyStandby = _vuhf1SavedCockpitDial3Frequency * 100 + _vuhf1SavedCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SaveCockpitFrequencyVuhf2()
        {
            lock (_lockVuhf2BigFreqObject)
            {
                lock (_lockVuhf2Dial3FreqObject)
                {
                    lock (_lockVuhf2Dial4FreqObject)
                    {
                        _vuhf2SavedCockpitDial1Frequency = _vuhf2CockpitDial1Frequency;
                        _vuhf2SavedCockpitDial2Frequency = _vuhf2CockpitDial2Frequency;
                        _vuhf2SavedCockpitDial3Frequency = _vuhf2CockpitDial3Frequency;
                        _vuhf2SavedCockpitDial4Frequency = _vuhf2CockpitDial4Frequency;
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVuhf2()
        {
            lock (_lockVuhf2BigFreqObject)
            {
                lock (_lockVuhf2Dial3FreqObject)
                {
                    lock (_lockVuhf2Dial4FreqObject)
                    {
                        _vuhf2BigFrequencyStandby = _vuhf2SavedCockpitDial1Frequency * 10 + _vuhf2SavedCockpitDial2Frequency;
                        _vuhf2SmallFrequencyStandby = _vuhf2SavedCockpitDial3Frequency * 100 + _vuhf2SavedCockpitDial4Frequency;
                    }
                }
            }
        }

        private void SaveCockpitFrequencyTacan()
        {
            /*TACAN*/
            // Large dial 0-12 [step of 1]
            // Small dial 0-9 [step of 1]
            lock (_lockTacanTensDialObject)
            {
                lock (_lockTacanOnesObject)
                {
                    _tacanSavedCockpitTensFrequency = Convert.ToInt32(_tacanCockpitTensDialPos);
                    _tacanSavedCockpitOnesFrequency = Convert.ToInt32(_tacanCockpitOnesDialPos);
                }
            }
        }

        private void SwapCockpitStandbyFrequencyTacan()
        {
            _tacanTensFrequencyStandby = _tacanSavedCockpitTensFrequency;
            _tacanOnesFrequencyStandby = _tacanSavedCockpitOnesFrequency;
        }

        private void SaveCockpitFrequencyVor()
        {
            lock (_lockVorMhzDialObject)
            {
                lock (_lockVorKhzObject)
                {
                    _vorSavedCockpitMhzFrequency = Convert.ToInt32(_vorCockpitMhzDialPos);
                    _vorSavedCockpitKhzFrequency = Convert.ToInt32(_vorCockpitKhzDialPos);
                }
            }
        }

        private void SwapStandbyFrequencyVor()
        {
            _vorMhzFrequencyStandby = _vorSavedCockpitMhzFrequency;
            _vorKhzFrequencyStandby = _vorSavedCockpitKhzFrequency;
        }

        private bool TacanNowSyncing()
        {
            return Interlocked.Read(ref _tacanThreadNowSynching) > 0;
        }

        private bool VorNowSyncing()
        {
            return Interlocked.Read(ref _vorThreadNowSynching) > 0;
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

        private bool VUHF1NowSyncing()
        {
            return Interlocked.Read(ref _vuhf1ThreadNowSynching) > 0;
        }

        private bool VUHF2NowSyncing()
        {
            return Interlocked.Read(ref _vuhf2ThreadNowSynching) > 0;
        }

        /*private static string GetCommandDirection10Dial(int desiredDialPosition, uint actualDialPosition)
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
        }*/
    }
}

