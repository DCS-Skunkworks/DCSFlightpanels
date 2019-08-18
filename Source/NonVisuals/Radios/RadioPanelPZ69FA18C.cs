//
//  added by Capt Zeen
//

using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;


namespace NonVisuals.Radios
{
    public class RadioPanelPZ69FA18C : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private CurrentFA18CRadioMode _currentUpperRadioMode = CurrentFA18CRadioMode.COMM1;
        private CurrentFA18CRadioMode _currentLowerRadioMode = CurrentFA18CRadioMode.COMM2;
        private bool _upperButtonPressed = false;
        private bool _lowerButtonPressed = false;
        private bool _upperButtonPressedAndDialRotated = false;
        private bool _lowerButtonPressedAndDialRotated = false;

        /*FA-18C COMM1 radio*/
        //
        //
        private const string COMM1ChannelInc = "UFC_COMM1_CHANNEL_SELECT +3200\n";
        private const string COMM1ChannelDec = "UFC_COMM1_CHANNEL_SELECT -3200\n";
        private const string COMM1VolInc = "UFC_COMM1_VOL +4000\n";
        private const string COMM1VolDec = "UFC_COMM1_VOL -4000\n";
        private const string COMM1PullPress = "UFC_COMM1_PULL INC\n";
        private const string COMM1PullRelease = "UFC_COMM1_PULL DEC\n";
        private double _comm1BigFrequencyStandby = 225;
        private double _comm1SmallFrequencyStandby = 0;
        private double _comm1SavedCockpitBigFrequency;
        private readonly object _lockCOMM1DialsObject = new object();
        private DCSBIOSOutput _comm1DcsbiosOutputFreq;  // comm1 frequency from DCSbios
        private DCSBIOSOutput _comm1DcsbiosOutputChannel; // comm1 channel 1 to 24 from CDSbios
        private volatile uint _comm1CockpitFreq = 12400;
        private volatile uint _comm1CockpitChannel = 1; // channel number 1 to 24
        private long _comm1ThreadNowSynching;
        private long _comm1DialWaitingForFeedback;
        private readonly ClickSpeedDetector _comm1ChannelClickSpeedDetector = new ClickSpeedDetector(8);


        /*FA-18C COMM2 radio*/
        //
        //
        private const string COMM2ChannelInc = "UFC_COMM2_CHANNEL_SELECT +3200\n";
        private const string COMM2ChannelDec = "UFC_COMM2_CHANNEL_SELECT -3200\n";
        private const string COMM2VolInc = "UFC_COMM2_VOL +4000\n";
        private const string COMM2VolDec = "UFC_COMM2_VOL -4000\n";
        private const string COMM2PullPress = "UFC_COMM2_PULL INC\n";
        private const string COMM2PullRelease = "UFC_COMM2_PULL DEC\n";
        private double _comm2BigFrequencyStandby = 255;
        private double _comm2SmallFrequencyStandby = 0;
        private double _comm2SavedCockpitBigFrequency;
        private readonly object _lockComm2DialObject = new object();
        private DCSBIOSOutput _comm2DcsbiosOutputFreq; // comm2 frequency from DCSbios
        private DCSBIOSOutput _comm2DcsbiosOutputChannel; // comm2 channel 1 to 24
        //private DCSBIOSOutput _comm2DcsbiosOutputPull;  // comm2 pull button
        //private DCSBIOSOutput _comm2DcsbiosOutputVol;   // comm2 volume
        private volatile uint _comm2CockpitFreq = 12400;
        private volatile uint _comm2CockpitChannel = 1; // channel number 1 to 24
        private long _comm2ThreadNowSynching;
        private long _comm2DialWaitingForFeedback;
        private readonly ClickSpeedDetector _uhfChannelClickSpeedDetector = new ClickSpeedDetector(8);


        /*FA-18C ILS*/
        //
        //
        private uint _ilsChannelStandby = 10;
        private uint _ilsSavedCockpitChannel = 1;
        private readonly object _lockIlsDialsObject = new object();
        private DCSBIOSOutput _ilsDcsbiosOutputChannel;
        private volatile uint _ilsCockpitChannel = 1;
        private const string ILSChannelInc = "COM_ILS_CHANNEL_SW INC\n";
        private const string ILSChannelDec = "COM_ILS_CHANNEL_SW DEC\n";
        private const string ILSChannelCommand = "COM_ILS_CHANNEL_SW ";
        private Thread _ilsSyncThread;
        private long _ilsThreadNowSynching;
        private long _ilsDialWaitingForFeedback;



        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;



        public RadioPanelPZ69FA18C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69FA18C()
        {
            _ilsSyncThread?.Abort();

        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)   // -------------------------   Get the data from DCSbios
        {
            UpdateCounter(e.Address, e.Data);


            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */

            //COMM 1
            if (e.Address == _comm1DcsbiosOutputFreq.Address)
            {
                lock (_lockCOMM1DialsObject)
                {
                    var tmp = _comm1CockpitFreq;
                    _comm1CockpitFreq = _comm1DcsbiosOutputFreq.GetUIntValue(e.Data);
                    if (tmp != _comm1CockpitFreq)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _comm1DialWaitingForFeedback, 0);
                    }
                }
            }


            if (e.Address == _comm1DcsbiosOutputChannel.Address)
            {
                var tmp = _comm1CockpitChannel;
                _comm1CockpitChannel = _comm1DcsbiosOutputChannel.GetUIntValue(e.Data);
                if (tmp != _comm1CockpitChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }



            //COMM2
            if (e.Address == _comm2DcsbiosOutputFreq.Address)
            {
                lock (_lockComm2DialObject)
                {
                    var tmp = _comm2CockpitFreq;
                    _comm2CockpitFreq = _comm2DcsbiosOutputFreq.GetUIntValue(e.Data);
                    if (tmp != _comm2CockpitFreq)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Common.DebugP("_uhfCockpitFreq1DialPos Before : " + tmp + "  now: " + _comm2CockpitFreq);
                        Interlocked.Exchange(ref _comm2DialWaitingForFeedback, 0);
                    }
                }
            }

            if (e.Address == _comm2DcsbiosOutputChannel.Address)
            {
                var tmp = _comm2CockpitChannel;
                _comm2CockpitChannel = _comm2DcsbiosOutputChannel.GetUIntValue(e.Data);
                if (tmp != _comm2CockpitChannel)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                }
            }




            //VHF FM



            //ILS
            if (e.Address == _ilsDcsbiosOutputChannel.Address)
            {
                lock (_lockIlsDialsObject)
                {
                    var tmp = _ilsCockpitChannel;
                    _ilsCockpitChannel = _ilsDcsbiosOutputChannel.GetUIntValue(e.Data) + 1;
                    if (tmp != _ilsCockpitChannel)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        Interlocked.Exchange(ref _ilsDialWaitingForFeedback, 0);
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
                //Common.DebugP("RadioPanelPZ69FA18C Received DCSBIOS stringData : ->" + e.StringData + "<-");
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    //Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }

            }
            catch (Exception ex)
            {
                Common.LogError(349998, ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsFA18C knob)   // Send changes to DCSbios when press the UPPER_FREQ_SWITCH or LOWER_FREQ_SWITCH
        {

            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentFA18CRadioMode.COMM1:
                                {

                                    break;
                                }
                            case CurrentFA18CRadioMode.COMM2:
                                {

                                    break;
                                }
                            case CurrentFA18CRadioMode.VHFFM:
                                {

                                    break;
                                }
                            case CurrentFA18CRadioMode.ILS:
                                {
                                    SendILSToDCSBIOS();
                                    break;
                                }
                            case CurrentFA18CRadioMode.TACAN:
                                {

                                    break;
                                }
                        }
                        break;
                    }
                case RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentFA18CRadioMode.COMM1:
                                {

                                    break;
                                }
                            case CurrentFA18CRadioMode.COMM2:
                                {

                                    break;
                                }
                            case CurrentFA18CRadioMode.VHFFM:
                                {

                                    break;
                                }
                            case CurrentFA18CRadioMode.ILS:
                                {
                                    SendILSToDCSBIOS();
                                    break;
                                }
                            case CurrentFA18CRadioMode.TACAN:
                                {

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
            var frequency = _comm1BigFrequencyStandby + _comm1SmallFrequencyStandby;
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);

            // send frequency to dcsbios here 

            SwapCockpitStandbyFrequencyVhfAm();
            ShowFrequenciesOnPanel();


        }



        private void SendUhfToDCSBIOS()
        {
            if (UhfNowSyncing())
            {
                return;
            }
            SaveCockpitFrequencyUhf();

            var frequency = _comm2BigFrequencyStandby + _comm2SmallFrequencyStandby;
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);

            // send frequency to dcsbios here 


            SwapCockpitStandbyFrequencyUhf();
            ShowFrequenciesOnPanel();

        }




        private void SendILSToDCSBIOS()
        {
            if (IlsNowSyncing())
            {
                return;
            }
            SaveCockpitFrequencyIls();

            _ilsSyncThread?.Abort();
            _ilsSyncThread = new Thread(() => ILSSynchThreadMethod(_ilsChannelStandby));
            _ilsSyncThread.Start();


        }

        private void ILSSynchThreadMethod(uint standbyPosition)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _ilsThreadNowSynching, 1);

                    long dialTimeout = DateTime.Now.Ticks;
                    long dialOkTime = 0;

                    var dialSendCount = 0;


                    do
                    {
                        if (IsTimedOut(ref dialTimeout, ResetSyncTimeout, "ILS dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _ilsDialWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for ILS 1");
                        }

                        if (Interlocked.Read(ref _ilsDialWaitingForFeedback) == 0)
                        {
                            lock (_lockIlsDialsObject)
                            {
                                if (_ilsCockpitChannel < standbyPosition)
                                {
                                    dialOkTime = DateTime.Now.Ticks;
                                    const string str = ILSChannelCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dialSendCount++;
                                    Interlocked.Exchange(ref _ilsDialWaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitChannel > standbyPosition)
                                {
                                    dialOkTime = DateTime.Now.Ticks;
                                    const string str = ILSChannelCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dialSendCount++;
                                    Interlocked.Exchange(ref _ilsDialWaitingForFeedback, 1);
                                }
                                Reset(ref dialTimeout);
                            }
                        }
                        else
                        {
                            dialOkTime = DateTime.Now.Ticks;
                        }


                        if (dialSendCount > 12)
                        {
                            //"Race" condition detected?
                            dialSendCount = 0;

                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    } while (IsTooShort(dialOkTime));
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


                //                                                          UPPER PANEL

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



                switch (_currentUpperRadioMode)
                {
                    case CurrentFA18CRadioMode.COMM1:    // show comm1 frequencies in upper panel
                        {
                            var frequencyAsString = "";

                            uint integer_comm1 = _comm1CockpitFreq / 100;
                            uint decimal_comm1 = _comm1CockpitFreq - (integer_comm1 * 100);
                            frequencyAsString = "" + integer_comm1;
                            frequencyAsString = frequencyAsString + ".";
                            frequencyAsString = frequencyAsString + decimal_comm1;

                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }

                            else
                            {

                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                //SetPZ69DisplayBytesDefault(ref bytes, _COMM1BigFrequencyStandby + _COMM1SmallFrequencyStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                //SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_comm1CockpitChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);

                            }
                            break;
                        }






                    case CurrentFA18CRadioMode.COMM2:     //  show comm2 frequencies in upper panel
                        {
                            var frequencyAsString = "";
                            uint integer_comm2 = _comm2CockpitFreq / 100;
                            uint decimal_comm2 = _comm2CockpitFreq - (integer_comm2 * 100);
                            frequencyAsString = "" + integer_comm2;
                            frequencyAsString = frequencyAsString + ".";
                            frequencyAsString = frequencyAsString + decimal_comm2;

                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }

                            else
                            {

                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                //SetPZ69DisplayBytesDefault(ref bytes, _COMM1BigFrequencyStandby + _COMM1SmallFrequencyStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                //SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_comm2CockpitChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);

                            }
                            break;

                        }




                    case CurrentFA18CRadioMode.VHFFM:  // clear displays
                        {

                            if (_upperButtonPressed)
                            {

                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }

                            else
                            {

                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);

                            }
                            break;
                        }


                    case CurrentFA18CRadioMode.ILS:
                        {

                            uint ILSChannel = 1;
                            lock (_lockIlsDialsObject)
                            {
                                ILSChannel = _ilsCockpitChannel;
                            }
                            SetPZ69DisplayBytesInteger(ref bytes, (int)ILSChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesInteger(ref bytes, (int)_ilsChannelStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                    case CurrentFA18CRadioMode.TACAN:  // clear displays
                        {
                            if (_upperButtonPressed)
                            {

                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }

                            else
                            {

                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);

                            }
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {





                    //                                                          LOWER PANEL

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




                    case CurrentFA18CRadioMode.COMM1:     //  show comm1 frequencies in lower panel
                        {
                            var frequencyAsString = "";

                            uint integer_comm1 = _comm1CockpitFreq / 100;
                            uint decimal_comm1 = _comm1CockpitFreq - (integer_comm1 * 100);
                            frequencyAsString = "" + integer_comm1;
                            frequencyAsString = frequencyAsString + ".";
                            frequencyAsString = frequencyAsString + decimal_comm1;

                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }

                            else
                            {

                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                //SetPZ69DisplayBytesDefault(ref bytes, _COMM1BigFrequencyStandby + _COMM1SmallFrequencyStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                //SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_comm1CockpitChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);

                            }
                            break;
                        }




                    case CurrentFA18CRadioMode.COMM2:     //  show comm2 frequencies in lower panel
                        {
                            var frequencyAsString = "";
                            uint integer_comm2 = _comm2CockpitFreq / 100;
                            uint decimal_comm2 = _comm2CockpitFreq - (integer_comm2 * 100);
                            frequencyAsString = "" + integer_comm2;
                            frequencyAsString = frequencyAsString + ".";
                            frequencyAsString = frequencyAsString + decimal_comm2;

                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }

                            else
                            {

                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                //SetPZ69DisplayBytesDefault(ref bytes, _COMM1BigFrequencyStandby + _COMM1SmallFrequencyStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                //SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesInteger(ref bytes, (int)_comm2CockpitChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);

                            }
                            break;

                        }




                    case CurrentFA18CRadioMode.VHFFM:  // clear displays
                        {


                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);


                            break;
                        }


                    case CurrentFA18CRadioMode.ILS:
                        {

                            uint ILSChannel = 1;
                            lock (_lockIlsDialsObject)
                            {
                                ILSChannel = _ilsCockpitChannel;
                            }
                            SetPZ69DisplayBytesInteger(ref bytes, (int)ILSChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesInteger(ref bytes, (int)_ilsChannelStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                    case CurrentFA18CRadioMode.TACAN:  // clear displays
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
                var radioPanelKnobFA18C = (RadioPanelKnobFA18C)o;
                if (radioPanelKnobFA18C.IsOn)
                {
                    switch (radioPanelKnobFA18C.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1ChannelInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2ChannelInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {
                                            // ils channels 1 to 20
                                            if (_ilsChannelStandby >= 20)
                                            {
                                                _ilsChannelStandby = 20;
                                                break;
                                            }
                                            _ilsChannelStandby++;
                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1ChannelDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2ChannelDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {
                                            // ils channels 1 to 20
                                            if (_ilsChannelStandby <= 1)
                                            {
                                                _ilsChannelStandby = 1;
                                                break;
                                            }
                                            _ilsChannelStandby--;
                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1VolInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2VolInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1VolDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2VolDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1ChannelInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2ChannelInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {
                                            // ils channels 1 to 20
                                            if (_ilsChannelStandby >= 20)
                                            {
                                                _ilsChannelStandby = 20;
                                                break;
                                            }
                                            _ilsChannelStandby++;
                                            break; ;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1ChannelDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2ChannelDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {
                                            // ils channels 1 to 20
                                            if (_ilsChannelStandby <= 1)
                                            {
                                                _ilsChannelStandby = 1;
                                                break;
                                            }
                                            _ilsChannelStandby--;
                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1VolInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2VolInc);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {

                                            DCSBIOS.Send(COMM1VolDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.COMM2:
                                        {

                                            DCSBIOS.Send(COMM2VolDec);
                                            break;

                                        }
                                    case CurrentFA18CRadioMode.VHFFM:
                                        {
                                            break;
                                        }
                                    case CurrentFA18CRadioMode.ILS:
                                        {

                                            break;
                                        }
                                    case CurrentFA18CRadioMode.TACAN:
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

        private void CheckFrequenciesForValidity()
        {
            //Crude fix if any freqs are outside the valid boundaries

            //COMM1
            //116.00 - 151.975
            if (_comm1BigFrequencyStandby < 116)
            {
                _comm1BigFrequencyStandby = 116;
            }
            if (_comm1BigFrequencyStandby > 151)
            {
                _comm1BigFrequencyStandby = 151;
            }

            //VHF FM
            //30.000 - 76.000Mhz

            //COMM2
            //225.000 - 399.975 MHz
            if (_comm2BigFrequencyStandby < 225)
            {
                _comm2BigFrequencyStandby = 225;
            }
            if (_comm2BigFrequencyStandby > 399)
            {
                _comm2BigFrequencyStandby = 399;
            }

            //ILS
            // ils channels 1 to 20
            if (_ilsChannelStandby < 1)
            {
                _ilsChannelStandby = 1;
            }
            if (_ilsChannelStandby > 20)
            {
                _ilsChannelStandby = 20;
            }

            //TACAN
            //00X/Y - 129X/Y

        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobFA18C)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsFA18C.UPPER_COMM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentFA18CRadioMode.COMM1;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_COMM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentFA18CRadioMode.COMM2;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentFA18CRadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_ILS:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentFA18CRadioMode.ILS;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentFA18CRadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_XPDR:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_COMM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentFA18CRadioMode.COMM1;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_COMM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentFA18CRadioMode.COMM2;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_VHFFM:
                            {

                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentFA18CRadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_ILS:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentFA18CRadioMode.ILS;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_TACAN:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentFA18CRadioMode.TACAN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_XPDR:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH:
                            {
                                _upperButtonPressed = radioPanelKnob.IsOn;

                                if (_currentUpperRadioMode == CurrentFA18CRadioMode.COMM1)
                                {
                                    DCSBIOS.Send(_upperButtonPressed ? COMM1PullPress : COMM1PullRelease);
                                }
                                if (_currentUpperRadioMode == CurrentFA18CRadioMode.COMM2)
                                {
                                    DCSBIOS.Send(_upperButtonPressed ? COMM2PullPress : COMM2PullRelease);
                                }

                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH);
                                    }
                                    _upperButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH:
                            {
                                _lowerButtonPressed = radioPanelKnob.IsOn;

                                if (_currentLowerRadioMode == CurrentFA18CRadioMode.COMM1)
                                {
                                    DCSBIOS.Send(_lowerButtonPressed ? COMM1PullPress : COMM1PullRelease);
                                }
                                if (_currentLowerRadioMode == CurrentFA18CRadioMode.COMM2)
                                {
                                    DCSBIOS.Send(_lowerButtonPressed ? COMM2PullPress : COMM2PullRelease);
                                }


                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        //Do not synch if user has pressed the button to configure the radio
                                        //Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH);
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
                StartupBase("FA-18C_hornet");

                //COMM 1

                _comm1DcsbiosOutputFreq = DCSBIOSControlLocator.GetDCSBIOSOutput("COMM1_FREQ");
                _comm1DcsbiosOutputChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("COMM1_CHANNEL_NUMERIC");
                //_comm1DcsbiosOutputPull = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_COMM1_PULL");
                //_comm1DcsbiosOutputVol = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_COMM1_VOL");



                //COMM 2
                _comm2DcsbiosOutputFreq = DCSBIOSControlLocator.GetDCSBIOSOutput("COMM2_FREQ");
                _comm2DcsbiosOutputChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("COMM2_CHANNEL_NUMERIC");
                //_comm1DcsbiosOutputPull = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_COMM1_PULL");
                //_comm1DcsbiosOutputVol = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_COMM1_VOL");


                //VHF FM


                //ILS
                _ilsDcsbiosOutputChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("COM_ILS_CHANNEL_SW");


                //TACAN



                StartListeningForPanelChanges();
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69FA18C.StartUp() : " + ex.Message);
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
            SaitekPanelKnobs = RadioPanelKnobFA18C.GetRadioPanelKnobs();
        }

        private void SaveCockpitFrequencyVhfAm()
        {

            lock (_lockCOMM1DialsObject)
            {

                _comm1SavedCockpitBigFrequency = double.Parse((_comm1CockpitFreq + 3).ToString(), NumberFormatInfoFullDisplay);


            }
        }

        private void SwapCockpitStandbyFrequencyVhfAm()
        {
            _comm1BigFrequencyStandby = _comm1SavedCockpitBigFrequency;

        }

        private void SaveCockpitFrequencyUhf()
        {

            try
            {
                var bigFrequencyAsString = "";



                _comm2SavedCockpitBigFrequency = double.Parse(bigFrequencyAsString, NumberFormatInfoFullDisplay);


            }
            catch (Exception ex)
            {
                Common.LogError(83244, ex, "SaveCockpitFrequencyUhf()");
                throw;
            }
        }

        private void SwapCockpitStandbyFrequencyUhf()
        {
            _comm2BigFrequencyStandby = _comm2SavedCockpitBigFrequency;

        }


        private void SaveCockpitFrequencyIls()
        {

            lock (_lockIlsDialsObject)
            {

                _ilsSavedCockpitChannel = _ilsCockpitChannel;


            }
        }

        private void SwapCockpitStandbyFrequencyIls()
        {
            _ilsChannelStandby = _ilsSavedCockpitChannel;

        }



        private bool VhfAmNowSyncing()
        {
            return Interlocked.Read(ref _comm1ThreadNowSynching) > 0;
        }

        private bool UhfNowSyncing()
        {
            return Interlocked.Read(ref _comm2ThreadNowSynching) > 0;
        }



        private bool IlsNowSyncing()
        {
            return Interlocked.Read(ref _ilsThreadNowSynching) > 0;
        }



        public override string SettingsVersion()
        {
            return "0X";
        }
    }

}
