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
    public class RadioPanelPZ69F5E : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private CurrentF5ERadioMode _currentUpperRadioMode = CurrentF5ERadioMode.UHF;
        private CurrentF5ERadioMode _currentLowerRadioMode = CurrentF5ERadioMode.UHF;

        private bool _upperButtonPressed = false;
        private bool _lowerButtonPressed = false;
        private bool _upperButtonPressedAndDialRotated = false;
        private bool _lowerButtonPressedAndDialRotated = false;

        /*F-5E UHF Radio COM1*/
        //Large dial 225-399 [step of 1]
        //Small dial 0.00-0.97 [step of 0 2 5 7]

        /*
         * Note, because of lack of information the A & T of the UHF
         * will be treated as 100 and 400 MHz.
         */
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
        private const string UHF_FREQ_1DIAL_COMMAND = "UHF_100MHZ_SEL ";		//"2" "3" "A"
        private const string UHF_FREQ_2DIAL_COMMAND = "UHF_10MHZ_SEL ";		//0 1 2 3 4 5 6 7 8 9
        private const string UHF_FREQ_3DIAL_COMMAND = "UHF_1MHZ_SEL ";			//0 1 2 3 4 5 6 7 8 9
        private const string UHF_FREQ_4DIAL_COMMAND = "UHF_01MHZ_SEL ";    //0 1 2 3 4 5 6 7 8 9
        private const string UHF_FREQ_5DIAL_COMMAND = "UHF_0025MHZ_SEL ";		//"00" "25" "50" "75"
        private Thread _uhfSyncThread;
        private long _uhfThreadNowSynching;
        private long _uhfDial1WaitingForFeedback;
        private long _uhfDial2WaitingForFeedback;
        private long _uhfDial3WaitingForFeedback;
        private long _uhfDial4WaitingForFeedback;
        private long _uhfDial5WaitingForFeedback;
        private const string UHF_PRESET_INCREASE = "UHF_PRESET_SEL INC\n";
        private const string UHF_PRESET_DECREASE = "UHF_PRESET_SEL DEC\n";
        private const string UHF_FREQ_MODE_INCREASE = "UHF_FREQ INC\n";
        private const string UHF_FREQ_MODE_DECREASE = "UHF_FREQ DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputFreqMode;  // 1 = PRESET
        private DCSBIOSOutput _uhfDcsbiosOutputSelectedChannel;
        private volatile uint _uhfCockpitFreqMode = 0;
        private volatile uint _uhfCockpitPresetChannel = 0;
        private readonly ClickSpeedDetector _uhfBigFreqIncreaseClickSpeedDetector = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _uhfBigFreqDecreaseClickSpeedDetector = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _uhfChannelClickSpeedDetector = new ClickSpeedDetector(8);
        private readonly ClickSpeedDetector _uhfFreqModeClickSpeedDetector = new ClickSpeedDetector(6);

        private const string UHF_FUNCTION_INCREASE = "UHF_FUNC INC\n";
        private const string UHF_FUNCTION_DECREASE = "UHF_FUNC DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputFunction;  // UHF_FUNC
        private volatile uint _uhfCockpitFunction = 0;
        private readonly ClickSpeedDetector _uhfFunctionClickSpeedDetector = new ClickSpeedDetector(8);

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
        private const string TACAN_FREQ_1DIAL_COMMAND = "TACAN_10 ";
        private const string TACAN_FREQ_2DIAL_COMMAND = "TACAN_1 ";
        private const string TACAN_FREQ_3DIAL_COMMAND = "TACAN_XY ";
        private Thread _tacanSyncThread;
        private long _tacanThreadNowSynching;
        private long _tacanDial1WaitingForFeedback;
        private long _tacanDial2WaitingForFeedback;
        private long _tacanDial3WaitingForFeedback;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69F5E(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69F5E()
        {
            _uhfSyncThread?.Abort();
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
            if (e.Address == _uhfDcsbiosOutputFreqDial1.Address)
            {
                lock (_lockUhfDialsObject1)
                {
                    var tmp = _uhfCockpitFreq1DialPos;
                    _uhfCockpitFreq1DialPos = 4 - _uhfDcsbiosOutputFreqDial1.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq1DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Common.DebugP("_uhfCockpitFreq1DialPos Before : " + tmp + "  now: " + _uhfCockpitFreq1DialPos);
                        Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial2.Address)
            {
                lock (_lockUhfDialsObject2)
                {
                    var tmp = _uhfCockpitFreq2DialPos;
                    _uhfCockpitFreq2DialPos = 10 - _uhfDcsbiosOutputFreqDial2.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq2DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial3.Address)
            {
                lock (_lockUhfDialsObject3)
                {
                    var tmp = _uhfCockpitFreq3DialPos;
                    _uhfCockpitFreq3DialPos = 10 - _uhfDcsbiosOutputFreqDial3.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq3DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial4.Address)
            {
                lock (_lockUhfDialsObject4)
                {
                    var tmp = _uhfCockpitFreq4DialPos;
                    _uhfCockpitFreq4DialPos = 10 - _uhfDcsbiosOutputFreqDial4.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq4DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 0);
                    }
                }
            }
            if (e.Address == _uhfDcsbiosOutputFreqDial5.Address)
            {
                lock (_lockUhfDialsObject5)
                {
                    var tmp = _uhfCockpitFreq5DialPos;
                    _uhfCockpitFreq5DialPos = 4 - _uhfDcsbiosOutputFreqDial5.GetUIntValue(e.Data);
                    if (tmp != _uhfCockpitFreq5DialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
            if (e.Address == _uhfDcsbiosOutputFunction.Address)
            {
                var tmp = _uhfCockpitFunction;
                _uhfCockpitFunction = _uhfDcsbiosOutputFunction.GetUIntValue(e.Data);
                if (tmp != _uhfCockpitFunction)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                //Common.DebugP("RadioPanelPZ69F5E Received DCSBIOS stringData : ->" + e.StringData + "<-");
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
                }
            }
            catch (Exception ex)
            {
                Common.LogError(349998, ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF5E knob)
        {

            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentF5ERadioMode.UHF:
                                {
                                    if (_uhfCockpitFunction != 0 && !UhfPresetSelected())
                                    {
                                        SendUhfToDCSBIOS();
                                    }

                                    break;
                                }
                            case CurrentF5ERadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                        }

                        break;
                    }
                case RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentF5ERadioMode.UHF:
                                {
                                    if (_uhfCockpitFunction != 0 && !UhfPresetSelected())
                                    {
                                        SendUhfToDCSBIOS();
                                    }

                                    break;
                                }
                            case CurrentF5ERadioMode.TACAN:
                                {
                                    SendTacanToDCSBIOS();
                                    break;
                                }
                        }

                        break;
                    }
            }
        }

        private void SendUhfToDCSBIOS()
        {
            if (UhfNowSyncing())
            {
                return;
            }
            SaveCockpitFrequencyUhf();
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
            var frequency = _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby;
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);


            var freqDial1 = 0;
            var freqDial2 = 0;
            var freqDial3 = 0;
            var freqDial4 = 0;
            var freqDial5 = 0;
            freqDial1 = int.Parse(frequencyAsString.Substring(0, 1));
            freqDial2 = int.Parse(frequencyAsString.Substring(1, 1));
            freqDial3 = int.Parse(frequencyAsString.Substring(2, 1));
            freqDial4 = int.Parse(frequencyAsString.Substring(4, 1));

            var tmp = int.Parse(frequencyAsString.Substring(5, 1));
            switch (tmp)
            {
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
                default:
                    {
                        //Safeguard in case it is in a invalid position
                        freqDial5 = 0;
                        break;
                    }
            }
            Common.DebugP("freqDial1 =" + freqDial1);
            Common.DebugP("freqDial2 =" + freqDial2);
            Common.DebugP("freqDial3 =" + freqDial3);
            Common.DebugP("freqDial4 =" + freqDial4);
            Common.DebugP("freqDial5 =" + freqDial5);

            _uhfSyncThread?.Abort();

            _uhfSyncThread = new Thread(() => UhfSynchThreadMethod(freqDial1, freqDial2, freqDial3, freqDial4, freqDial5));
            _uhfSyncThread.Start();
        }

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
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "UHF dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "UHF dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 2");
                        }
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "UHF dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 3");
                        }
                        if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "UHF dial4Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 4");
                        }
                        if (IsTimedOut(ref dial5Timeout, ResetSyncTimeout, "UHF dial5Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 5");
                        }
                        //Frequency selector 1     
                        //       "2"  "3"  "A"/"-1"
                        //Pos     0    1    2
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
                                    const string str = UHF_FREQ_1DIAL_COMMAND + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq1DialPos > desiredPosition1)
                                {
                                    const string str = UHF_FREQ_1DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
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
                                    const string str = UHF_FREQ_2DIAL_COMMAND + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq2DialPos > desiredPosition2)
                                {
                                    const string str = UHF_FREQ_2DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
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
                                    const string str = UHF_FREQ_3DIAL_COMMAND + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq3DialPos > desiredPosition3)
                                {
                                    const string str = UHF_FREQ_3DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
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
                                    const string str = UHF_FREQ_4DIAL_COMMAND + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq4DialPos > desiredPosition4)
                                {
                                    const string str = UHF_FREQ_4DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
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
                                    const string str = UHF_FREQ_5DIAL_COMMAND + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial5SendCount++;
                                    Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 1);
                                }
                                else if (_uhfCockpitFreq5DialPos > desiredPosition5)
                                {
                                    const string str = UHF_FREQ_5DIAL_COMMAND + "INC\n";
                                    Common.DebugP("Sending " + str);
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
                    SwapCockpitStandbyFrequencyUhf();
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
                Interlocked.Exchange(ref _uhfThreadNowSynching, 0);
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
                                    var str = TACAN_FREQ_1DIAL_COMMAND + (_tacanCockpitFreq1DialPos < desiredPositionDial1 ? inc : dec);
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

                                    var str = TACAN_FREQ_2DIAL_COMMAND + (_tacanCockpitFreq2DialPos < desiredPositionDial2 ? inc : dec);
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

                                    var str = TACAN_FREQ_3DIAL_COMMAND + (_tacanCockpitFreq3DialPos < desiredPositionDial3 ? inc : dec);
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
                    case CurrentF5ERadioMode.UHF:
                        {
                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitFunction, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitFreqMode, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else if (_uhfCockpitFunction != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitPresetChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                //Frequency selector 1     
                                //     // "T" "2"  "3"  "A"
                                //Pos      0   1    2    3

                                //Frequency selector 2      
                                //0 1 2 3 4 5 6 7 8 9

                                //Frequency selector 3
                                //0 1 2 3 4 5 6 7 8 9


                                //Frequency selector 4
                                //0 1 2 3 4 5 6 7 8 9

                                //Frequency selector 5
                                //      "00" "25" "50" "75", only 0 2 5 7 used.
                                //Pos     0    1    2    3

                                //251.75
                                if (_uhfCockpitFunction == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                else
                                {
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

                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF5ERadioMode.TACAN:
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
                    default:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentF5ERadioMode.UHF:
                        {
                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitFunction, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitFreqMode, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else if (_uhfCockpitFunction != 0 && UhfPresetSelected())
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_uhfCockpitPresetChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                if (_uhfCockpitFunction == 0)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                else
                                {
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

                                    SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                            }
                            break;
                        }
                    case CurrentF5ERadioMode.TACAN:
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
                    default:
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

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            if (SkipCurrentFrequencyChange())
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var radioPanelKnobF5E = (RadioPanelKnobF5E)o;
                if (radioPanelKnobF5E.IsOn)
                {
                    switch (radioPanelKnobF5E.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                                    UHFBigFrequencyStandbyAdjust(true);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                                else if (_uhfBigFrequencyStandby.Equals(100.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    UHFBigFrequencyStandbyAdjust(false);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                                    UHFBigFrequencyStandbyAdjust(true);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                                else if (_uhfBigFrequencyStandby.Equals(100.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                else
                                                {
                                                    UHFBigFrequencyStandbyAdjust(false);
                                                }
                                            }
                                            break;
                                        }
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                    case CurrentF5ERadioMode.TACAN:
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
                        case RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentF5ERadioMode.UHF:
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
                                                if (_uhfSmallFrequencyStandby <= 0.00)
                                                {
                                                    //At min value
                                                    _uhfSmallFrequencyStandby = 0.97;
                                                    break;
                                                }
                                                UHFSmallFrequencyStandbyAdjust(false);
                                            }
                                            break;
                                        }
                                    case CurrentF5ERadioMode.TACAN:
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


        private void UHFSmallFrequencyStandbyAdjust(bool increase)
        {
            var tmp = _uhfSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture);
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
                        _uhfSmallFrequencyStandby += 0.02;
                    }
                    else if (tmp.EndsWith("2"))
                    {
                        _uhfSmallFrequencyStandby += 0.03;
                    }
                    else if (tmp.EndsWith("5"))
                    {
                        _uhfSmallFrequencyStandby += 0.02;
                    }
                    else if (tmp.EndsWith("7"))
                    {
                        _uhfSmallFrequencyStandby += 0.03;
                    }
                }
                else
                {
                    /*
                     * Zero assumed
                     * e.g. 0.10
                     *         ^
                     */
                    _uhfSmallFrequencyStandby += 0.02;
                }
            }
            else
            {
                if (tmp.Length == 4)
                {
                    if (tmp.EndsWith("0"))
                    {
                        _uhfSmallFrequencyStandby -= 0.03;
                    }
                    else if (tmp.EndsWith("2"))
                    {
                        _uhfSmallFrequencyStandby -= 0.02;
                    }
                    else if (tmp.EndsWith("5"))
                    {
                        _uhfSmallFrequencyStandby -= 0.03;
                    }
                    else if (tmp.EndsWith("7"))
                    {
                        _uhfSmallFrequencyStandby -= 0.02;
                    }
                }
                else
                {
                    /*
                     * Zero assumed
                     * e.g. 0.10
                     */
                    _uhfSmallFrequencyStandby -= 0.03;
                }
            }

            if (_uhfSmallFrequencyStandby < 0)
            {
                _uhfSmallFrequencyStandby = 0.97;
            }
            else if (_uhfSmallFrequencyStandby > 0.97)
            {
                _uhfSmallFrequencyStandby = 0.0;
            }
        }

        private void UHFBigFrequencyStandbyAdjust(bool increase)
        {
            if (increase)
            {
                if (_uhfBigFreqIncreaseClickSpeedDetector.ClickAndCheck())
                {
                    _uhfBigFrequencyStandby += 10;
                }
                else
                {
                    _uhfBigFrequencyStandby++;
                }
            }
            else
            {
                if (_uhfBigFreqDecreaseClickSpeedDetector.ClickAndCheck())
                {
                    _uhfBigFrequencyStandby -= 10;
                }
                else
                {
                    _uhfBigFrequencyStandby--;
                }
            }
        }

        private void CheckFrequenciesForValidity()
        {
            //Crude fix if any freqs are outside the valid boundaries


            //UHF
            //225.000 - 399.975 MHz
            if (_uhfBigFrequencyStandby < 100)
            {
                _uhfBigFrequencyStandby = 100;
            }
            if (_uhfBigFrequencyStandby > 399)
            {
                _uhfBigFrequencyStandby = 399;
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
                    var radioPanelKnob = (RadioPanelKnobF5E)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsF5E.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF5ERadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF5ERadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_NOUSE1:
                        case RadioPanelPZ69KnobsF5E.UPPER_NOUSE2:
                        case RadioPanelPZ69KnobsF5E.UPPER_NOUSE3:
                        case RadioPanelPZ69KnobsF5E.UPPER_NOUSE4:
                        case RadioPanelPZ69KnobsF5E.UPPER_NOUSE5:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentF5ERadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF5ERadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF5ERadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_NOUSE1:
                        case RadioPanelPZ69KnobsF5E.LOWER_NOUSE2:
                        case RadioPanelPZ69KnobsF5E.LOWER_NOUSE3:
                        case RadioPanelPZ69KnobsF5E.LOWER_NOUSE4:
                        case RadioPanelPZ69KnobsF5E.LOWER_NOUSE5:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentF5ERadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH:
                            {
                                if (ReportCounter == 1)
                                {
                                    //Don't do synch from the initial panel touch
                                    //where are switches are included
                                    break;
                                }
                                _upperButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH);
                                    }
                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH:
                            {
                                if (ReportCounter == 1)
                                {
                                    //Don't do synch from the initial panel touch
                                    //where are switches are included
                                    break;
                                }
                                _lowerButtonPressed = radioPanelKnob.IsOn;
                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH);
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
                StartupBase("F-5E");

                //UHF
                _uhfDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_100MHZ_SEL");
                _uhfDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_10MHZ_SEL");
                _uhfDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_01MHZ_SEL");
                _uhfDcsbiosOutputFreqDial5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_0025MHZ_SEL");
                _uhfDcsbiosOutputFreqMode = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FREQ");
                _uhfDcsbiosOutputSelectedChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_SEL");
                _uhfDcsbiosOutputFunction = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FUNC");

                //TACAN
                _tacanDcsbiosOutputFreqChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("TACAN_CHANNEL");
                DCSBIOSStringListenerHandler.AddAddress(_tacanDcsbiosOutputFreqChannel.Address, 4, this); //_tacanDcsbiosOutputFreqChannel.MaxLength does not work. Bad JSON format.

                StartListeningForPanelChanges();
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69F5E.StartUp() : " + ex.Message);
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

        protected override void GamingPanelKnobChanged(IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(hashSet);
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobF5E.GetRadioPanelKnobs();
        }

        private string GetUhfDialFrequencyForPosition(int dial, uint position)
        {
            //Frequency selector 1     
            //     // "T"  "2"  "3"  "A"
            //Pos      0    1    2    4

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
                            case 1:
                                {
                                    return "1";
                                }
                            case 2:
                                {
                                    return "2";
                                }
                            case 3:
                                {
                                    return "3";
                                }
                            case 4:
                                {
                                    return "4";
                                    //throw new NotImplementedException("check how A should be treated.");
                                    //return "0";//should be "A"
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
                var smallFrequencyAsString = "0.";
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
            Common.DebugP("Old _uhfBigFrequencyStandby = " + _uhfBigFrequencyStandby);
            Common.DebugP("Old _uhfSmallFrequencyStandby = " + _uhfSmallFrequencyStandby);
            _uhfBigFrequencyStandby = _uhfSavedCockpitBigFrequency;
            _uhfSmallFrequencyStandby = _uhfSavedCockpitSmallFrequency;
            Common.DebugP("New _uhfBigFrequencyStandby = " + _uhfBigFrequencyStandby);
            Common.DebugP("New _uhfSmallFrequencyStandby = " + _uhfSmallFrequencyStandby);
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
            return _uhfCockpitFreqMode == 1;
        }

        private bool UhfNowSyncing()
        {
            return Interlocked.Read(ref _uhfThreadNowSynching) > 0;
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
