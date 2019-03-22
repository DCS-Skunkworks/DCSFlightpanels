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

        /*TACAN*/
        //Tens dial 0-12 [step of 1]
        //Ones dial 0-9 [step of 1]
        //Last : X/Y [0,1]
        private int _tacanTensFrequencyStandby = 6;
        private int _tacanOnesFrequencyStandby = 5;
        private int _tacanXYStandby;
        private int _tacanSavedCockpitTensFrequency = 6;
        private int _tacanSavedCockpitOnesFrequency = 5;
        private int _tacanSavedCockpitXY;
        private readonly object _lockTacanTensDialObject = new object();
        private readonly object _lockTacanOnesObject = new object();
        private readonly object _lockTacanXYDialObject = new object();
        private DCSBIOSOutput _tacanDcsbiosOutputTensDial;
        private DCSBIOSOutput _tacanDcsbiosOutputOnesDial;
        private DCSBIOSOutput _tacanDcsbiosOutputXYDial;
        private volatile uint _tacanCockpitTensDialPos = 1;
        private volatile uint _tacanCockpitOnesDialPos = 1;
        private volatile uint _tacanCockpitXYDialPos = 1;
        private const string TACAN_TENS_DIAL_COMMAND = "PLT_TACAN_DIAL_TENS ";
        private const string TACAN_ONES_DIAL_COMMAND = "PLT_TACAN_DIAL_ONES ";
        private const string TACAN_XY_DIAL_COMMAND = "PLT_TACAN_CHANNEL "; //X = 0 | Y = 1
        private Thread _tacanSyncThread;
        private long _tacanThreadNowSynching;
        private long _tacanTensWaitingForFeedback;
        private long _tacanOnesWaitingForFeedback;
        private long _tacanXYWaitingForFeedback;

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

            //TACAN
            if (e.Address == _tacanDcsbiosOutputTensDial.Address)
            {
                var tmp = _tacanCockpitTensDialPos;
                _tacanCockpitTensDialPos = _tacanDcsbiosOutputTensDial.GetUIntValue(e.Data);
                if (tmp != _tacanCockpitTensDialPos)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _tacanDcsbiosOutputOnesDial.Address)
            {
                var tmp = _tacanCockpitOnesDialPos;
                _tacanCockpitOnesDialPos = _tacanDcsbiosOutputOnesDial.GetUIntValue(e.Data);
                if (tmp != _tacanCockpitOnesDialPos)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 5);
                }
            }
            if (e.Address == _tacanDcsbiosOutputXYDial.Address)
            {
                var tmp = _tacanCockpitXYDialPos;
                _tacanCockpitXYDialPos = _tacanDcsbiosOutputXYDial.GetUIntValue(e.Data);
                if (tmp != _tacanCockpitXYDialPos)
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
                                        DCSBIOS.Send("SET_VUHF_FREQ " + freq + "\n");
                                        SwapCockpitStandbyFrequencyVuhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
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
                                        DCSBIOS.Send("SET_VUHF_FREQ " + freq + "\n");
                                        SwapCockpitStandbyFrequencyVuhf();
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                        ShowFrequenciesOnPanel();
                                    }
                                    break;
                                }
                            case CurrentF14RadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
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
            _tacanSyncThread = new Thread(() => TacanSynchThreadMethod(_tacanTensFrequencyStandby, _tacanOnesFrequencyStandby, _tacanXYStandby));
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
                            Interlocked.Exchange(ref _tacanTensWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 1");
                        }

                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "TACAN dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanOnesWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 2");
                        }

                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "TACAN dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanXYWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 3");
                        }

                        if (Interlocked.Read(ref _tacanTensWaitingForFeedback) == 0)
                        {

                            lock (_lockTacanTensDialObject)
                            {
                                if (_tacanCockpitTensDialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_tacanCockpitFreq1DialPos is " + _tacanCockpitTensDialPos + " and should be " + desiredPositionDial1);
                                    var str = TACAN_TENS_DIAL_COMMAND + (_tacanCockpitTensDialPos < desiredPositionDial1 ? inc : dec);
                                    Common.DebugP("Sending " + str);
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

                                    var str = TACAN_ONES_DIAL_COMMAND + (_tacanCockpitOnesDialPos < desiredPositionDial2 ? inc : dec);
                                    Common.DebugP("Sending " + str);
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

                        if (Interlocked.Read(ref _tacanXYWaitingForFeedback) == 0)
                        {

                            lock (_lockTacanXYDialObject)
                            {
                                if (_tacanCockpitXYDialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;

                                    var str = TACAN_XY_DIAL_COMMAND + (_tacanCockpitXYDialPos < desiredPositionDial3 ? inc : dec);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _tacanXYWaitingForFeedback, 1);
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
                            lock (_lockTacanTensDialObject)
                            {
                                lock (_lockTacanOnesObject)
                                {
                                    frequencyAsString = _tacanCockpitTensDialPos.ToString() + _tacanCockpitOnesDialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockTacanXYDialObject)
                            {
                                frequencyAsString = frequencyAsString + _tacanCockpitXYDialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanTensFrequencyStandby.ToString() + _tacanOnesFrequencyStandby.ToString() + "." + _tacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_STBY_RIGHT);
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
                    case CurrentF14RadioMode.TACAN:
                        {
                            var frequencyAsString = "";
                            lock (_lockTacanTensDialObject)
                            {
                                lock (_lockTacanOnesObject)
                                {
                                    frequencyAsString = _tacanCockpitTensDialPos.ToString() + _tacanCockpitOnesDialPos.ToString();
                                }
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockTacanXYDialObject)
                            {
                                frequencyAsString = frequencyAsString + _tacanCockpitXYDialPos.ToString();
                            }

                            SetPZ69DisplayBytes(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanTensFrequencyStandby.ToString() + _tacanOnesFrequencyStandby.ToString() + "." + _tacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_STBY_RIGHT);
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
                        frequencyAsString = _vuhfCockpitBigFrequency.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
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
                                                if (_vuhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VuhfPresetSelected() && _vuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_PRESET_INCREASE);
                                                }
                                                else
                                                {
                                                    AdjustVUHFBigFrequency(true);
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

                                            if (_tacanTensFrequencyStandby >= 12)
                                            {
                                                _tacanTensFrequencyStandby = 12;
                                                break;
                                            }
                                            _tacanTensFrequencyStandby++;
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

                                            if (_tacanTensFrequencyStandby <= 0)
                                            {
                                                _tacanTensFrequencyStandby = 0;
                                                break;
                                            }
                                            _tacanTensFrequencyStandby--;
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

                                            if (_tacanOnesFrequencyStandby >= 9)
                                            {
                                                _tacanOnesFrequencyStandby = 9;
                                                _tacanXYStandby = 1;
                                                break;
                                            }
                                            _tacanOnesFrequencyStandby++;
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

                                            if (_tacanOnesFrequencyStandby <= 0)
                                            {
                                                _tacanOnesFrequencyStandby = 0;
                                                _tacanXYStandby = 0;
                                                break;
                                            }
                                            _tacanOnesFrequencyStandby--;
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
                                                if (_vuhfModeClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_MODE_INCREASE);
                                                }
                                            }
                                            else
                                            {
                                                if (VuhfPresetSelected() && _vuhfChannelClickSpeedDetector.ClickAndCheck())
                                                {
                                                    DCSBIOS.Send(VUHF_PRESET_INCREASE);
                                                }
                                                else
                                                {
                                                    AdjustVUHFBigFrequency(true);
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

                                            if (_tacanTensFrequencyStandby >= 12)
                                            {
                                                _tacanTensFrequencyStandby = 12;
                                                break;
                                            }
                                            _tacanTensFrequencyStandby++;
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

                                            if (_tacanTensFrequencyStandby <= 0)
                                            {
                                                _tacanTensFrequencyStandby = 0;
                                                break;
                                            }
                                            _tacanTensFrequencyStandby--;
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

                                            if (_tacanOnesFrequencyStandby >= 9)
                                            {
                                                _tacanOnesFrequencyStandby = 9;
                                                _tacanXYStandby = 1;
                                                break;
                                            }
                                            _tacanOnesFrequencyStandby++;
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

                                            if (_tacanOnesFrequencyStandby <= 0)
                                            {
                                                _tacanOnesFrequencyStandby = 0;
                                                _tacanXYStandby = 0;
                                                break;
                                            }
                                            _tacanOnesFrequencyStandby--;
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

        private void UHFSmallFrequencyStandbyAdjust(bool increase)
        {
            _skipUhfSmallFreqChange++;
            if (_skipUhfSmallFreqChange < 2)
            {
                return;
            }
            _skipUhfSmallFreqChange = 0;
            var tmp = _uhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
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
            var tmp = _vuhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadRight(3, '0');
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
                        case RadioPanelPZ69KnobsF14B.UPPER_NOUSE4:
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
                        case RadioPanelPZ69KnobsF14B.LOWER_NOUSE4:
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

                //TACAN
                _tacanDcsbiosOutputTensDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_TACAN_DIAL_TENS");
                _tacanDcsbiosOutputOnesDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_TACAN_DIAL_ONES");
                _tacanDcsbiosOutputXYDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_TACAN_CHANNEL");

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


        private void SaveCockpitFrequencyTacan()
        {
            /*TACAN*/
            //Large dial 0-12 [step of 1]
            //Small dial 0-9 [step of 1]
            //Last : X/Y [0,1]
            lock (_lockTacanTensDialObject)
            {
                lock (_lockTacanOnesObject)
                {
                    lock (_lockTacanXYDialObject)
                    {
                        _tacanSavedCockpitTensFrequency = Convert.ToInt32(_tacanCockpitTensDialPos);
                        _tacanSavedCockpitOnesFrequency = Convert.ToInt32(_tacanCockpitOnesDialPos);
                        _tacanSavedCockpitXY = Convert.ToInt32(_tacanCockpitXYDialPos);
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyTacan()
        {
            _tacanTensFrequencyStandby = _tacanSavedCockpitTensFrequency;
            _tacanOnesFrequencyStandby = _tacanSavedCockpitOnesFrequency;
            _tacanXYStandby = _tacanSavedCockpitXY;
        }

        private bool UhfPresetSelected()
        {
            return _uhfCockpitFreqMode == 0;
        }

        private bool VuhfPresetSelected()
        {
            return _vuhfCockpitFreqMode == 3;
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

