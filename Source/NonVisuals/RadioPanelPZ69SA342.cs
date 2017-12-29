using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69SA342 : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private HashSet<RadioPanelKnobSA342> _radioPanelKnobs = new HashSet<RadioPanelKnobSA342>();
        private CurrentSA345RadioMode _currentUpperRadioMode = CurrentSA345RadioMode.VHFAM;
        private CurrentSA345RadioMode _currentLowerRadioMode = CurrentSA345RadioMode.VHFAM;

        //118.175
        private enum VhfAmDigit
        {
            First,
            Second,
            Third,
            Fourth,
            LastTwoSpecial
        }

        public enum CurrentSA345RadioMode
        {
            VHFFM = 2,
            VHFAM = 4
        }
        /*COM1 SA345 VHF AM Radio*/
        //Large dial 
        //Small dial 
        private int[] _dialPositionsWholeNumbers = new int[] { 0, 6553, 13107, 19660, 26214, 32767, 39321, 45874, 52428, 58981 };
        private int[] _dialPositionsDecial100s = new int[] { 0, 16383, 32767, 49151 };
        private double _vhfAmBigFrequencyStandby = 118;
        private double _vhfAmSmallFrequencyStandby;
        private double _vhfAmSavedCockpitBigFrequency;
        private double _vhfAmSavedCockpitSmallFrequency;
        private DCSBIOSOutput _vhfAmDcsbiosOutputReading10s;           //1[1]8.375
        private DCSBIOSOutput _vhfAmDcsbiosOutputReading1s;            //11[8].375
        private DCSBIOSOutput _vhfAmDcsbiosOutputReadingDecimal10s;    //118.[3]75
        private DCSBIOSOutput _vhfAmDcsbiosOutputReadingDecimal100s;   //118.3[75]
        /*
         AM_RADIO_FREQ_10s
         AM_RADIO_FREQ_1s
         AM_RADIO_FREQ_TENTHS
         AM_RADIO_FREQ_HUNDREDTHS
        */
        private const string VhfAmLeftDialDialCommandInc = "AM_RADIO_FREQUENCY_DIAL_LEFT +3200\n";
        private const string VhfAmLeftDialDialCommandDec = "AM_RADIO_FREQUENCY_DIAL_LEFT -3200\n";
        private const string VhfAmRightDialDialCommandInc = "AM_RADIO_FREQUENCY_DIAL_RIGHT +3200\n";
        private const string VhfAmRightDialDialCommandDec = "AM_RADIO_FREQUENCY_DIAL_RIGHT -3200\n";
        private readonly object _lockVhfAm10sObject = new object();
        private readonly object _lockVhfAm1sObject = new object();
        private readonly object _lockVhfAmDecimal10sObject = new object();
        private readonly object _lockVhfAmDecimal100sObject = new object();
        private volatile uint _vhfAmCockpit10sFrequencyValue = 6553;
        private volatile uint _vhfAmCockpit1sFrequencyValue = 6553;
        private volatile uint _vhfAmCockpitDecimal10sFrequencyValue = 6553;
        private volatile uint _vhfAmCockpitDecimal100sFrequencyValue = 6553;
        private Thread _vhfAmSyncThread;
        private long _vhfAmThreadNowSynching;
        private long _vhfAmValue1WaitingForFeedback; //10s
        private long _vhfAmValue2WaitingForFeedback; //1s
        private long _vhfAmValue3WaitingForFeedback; //Decimal 10s
        private long _vhfAmValue4WaitingForFeedback; //Decimal 100s
        private int _vhfAmLeftDialSkipper;
        private int _vhfAmRightDialSkipper;

        /*COM2 SA345 FM PR4G Radio*/
        //Large dial 0-7 Presets 1, 2, 3, 4, 5, 6, 0, RG
        //Small dial 
        private DCSBIOSOutput _fmRadioPresetDcsbiosOutput;
        private volatile uint _fmRadioPresetCockpitDialPos = 1;
        private const string FmRadioPresetCommandInc = "FM_RADIO_CHANNEL INC\n";
        private const string FmRadioPresetCommandDec = "FM_RADIO_CHANNEL DEC\n";
        private object _lockFmRadioPresetObject = new object();

        private readonly object _lockShowFrequenciesOnPanelObject = new object();

        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SA342(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69SA342()
        {
            if (_vhfAmSyncThread != null)
            {
                _vhfAmSyncThread.Abort();
            }

        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            UpdateCounter(address, data);


            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */

            //VHF AM
            if (address == _vhfAmDcsbiosOutputReading10s.Address)
            {
                lock (_lockVhfAm10sObject)
                {
                    //When dialing this radio a lot of intermediate (incorrect) raw values are sent. Only trap
                    //know raw values as in the member array _dialPositions
                    if (CorrectPositionWholeNumbers(_vhfAmDcsbiosOutputReading10s.GetUIntValue(data)))
                    {
                        var tmp = _vhfAmCockpit10sFrequencyValue;
                        _vhfAmCockpit10sFrequencyValue = _vhfAmDcsbiosOutputReading10s.GetUIntValue(data);
                        if (tmp != _vhfAmCockpit10sFrequencyValue)
                        {
                            //Debug.Print("RECEIVE _vhfAmCockpit10sFrequencyValue " + _vhfAmCockpit10sFrequencyValue);
                            Interlocked.Exchange(ref _vhfAmValue1WaitingForFeedback, 0);
                        }
                    }
                }
            }


            if (address == _vhfAmDcsbiosOutputReading1s.Address)
            {
                lock (_lockVhfAm1sObject)
                {
                    if (CorrectPositionWholeNumbers(_vhfAmDcsbiosOutputReading1s.GetUIntValue(data)))
                    {
                        var tmp = _vhfAmCockpit1sFrequencyValue;
                        _vhfAmCockpit1sFrequencyValue = _vhfAmDcsbiosOutputReading1s.GetUIntValue(data);
                        if (tmp != _vhfAmCockpit1sFrequencyValue)
                        {
                            //Debug.Print("RECEIVE _vhfAmCockpit1sFrequencyValue " + _vhfAmCockpit1sFrequencyValue);
                            Interlocked.Exchange(ref _vhfAmValue2WaitingForFeedback, 0);
                        }
                    }
                }
            }


            if (address == _vhfAmDcsbiosOutputReadingDecimal10s.Address)
            {
                lock (_lockVhfAmDecimal10sObject)
                {
                    //Debug.Print("RECEIVE _vhfAmCockpitDecimal10sFrequencyValue " + _vhfAmCockpitDecimal10sFrequencyValue);
                    if (CorrectPositionWholeNumbers(_vhfAmDcsbiosOutputReadingDecimal10s.GetUIntValue(data)))
                    {
                        var tmp = _vhfAmCockpitDecimal10sFrequencyValue;
                        _vhfAmCockpitDecimal10sFrequencyValue = _vhfAmDcsbiosOutputReadingDecimal10s.GetUIntValue(data);
                        if (tmp != _vhfAmCockpitDecimal10sFrequencyValue)
                        {
                            //Debug.Print("RECEIVE _vhfAmCockpitDecimal10sFrequencyValue " + _vhfAmCockpitDecimal10sFrequencyValue);
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _vhfAmValue3WaitingForFeedback, 0);
                        }
                    }
                }
            }


            if (address == _vhfAmDcsbiosOutputReadingDecimal100s.Address)
            {
                //Debug.Print("RECEIVE _vhfAmCockpitDecimal100sFrequencyValue (" + _vhfAmDcsbiosOutputReadingDecimal100s.Address + ") " + _vhfAmCockpitDecimal100sFrequencyValue);
                lock (_lockVhfAmDecimal100sObject)
                {
                    if (CorrectPositionDecimal100s(_vhfAmDcsbiosOutputReadingDecimal100s.GetUIntValue(data)))
                    {
                        var tmp = _vhfAmCockpitDecimal100sFrequencyValue;
                        _vhfAmCockpitDecimal100sFrequencyValue = _vhfAmDcsbiosOutputReadingDecimal100s.GetUIntValue(data);
                        if (tmp != _vhfAmCockpitDecimal100sFrequencyValue)
                        {
                            //Debug.Print("RECEIVE _vhfAmCockpitDecimal100sFrequencyValue " + _vhfAmCockpitDecimal100sFrequencyValue);
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            Interlocked.Exchange(ref _vhfAmValue4WaitingForFeedback, 0);
                        }
                    }
                }
            }

            //VHF FM PR4G
            if (address == _fmRadioPresetDcsbiosOutput.Address)
            {
                lock (_lockFmRadioPresetObject)
                {
                    var tmp = _fmRadioPresetCockpitDialPos;
                    _fmRadioPresetCockpitDialPos = _fmRadioPresetDcsbiosOutput.GetUIntValue(data);
                    if (tmp != _fmRadioPresetCockpitDialPos)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {


            }
            catch (Exception e)
            {
                Common.LogError(349998, e, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsSA342 knob)
        {
            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }
            switch (knob)
            {
                case RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentSA345RadioMode.VHFAM:
                                {
                                    SendVhfAmToDCSBIOS();
                                    break;
                                }
                            case CurrentSA345RadioMode.VHFFM:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentSA345RadioMode.VHFAM:
                                {
                                    SendVhfAmToDCSBIOS();
                                    break;
                                }
                            case CurrentSA345RadioMode.VHFFM:
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
            var frequency = _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby;
            var frequencyAsString = frequency.ToString("0.000", NumberFormatInfoFullDisplay);

            var desiredPositionDialWholeNumbers = 0;
            var desiredPositionDecimals = 0;

            //118.950
            //First digit is always 1, no need to do anything about it.
            desiredPositionDialWholeNumbers = int.Parse(frequencyAsString.Substring(1, 2));
            if (frequencyAsString.Length < 7)
            {
                desiredPositionDecimals = int.Parse(frequencyAsString.Substring(4, 2) + "0");
            }
            else
            {
                desiredPositionDecimals = int.Parse(frequencyAsString.Substring(4, 3));
            }

            //#1
            if (_vhfAmSyncThread != null)
            {
                _vhfAmSyncThread.Abort();
            }
            _vhfAmSyncThread = new Thread(() => VhfAmSynchThreadMethod(desiredPositionDialWholeNumbers, desiredPositionDecimals));
            _vhfAmSyncThread.Start();
        }

        private void VhfAmSynchThreadMethod(int desiredPositionDialWholeNumbers, int desiredPositionDialDecimals)
        {
            try
            {
                try
                {   /*
                     * COM1 VHF AM
                     * 
                     */

                    string str;
                    Interlocked.Exchange(ref _vhfAmThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial1Time = 0;
                    long dial2Time = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF AM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfAmValue1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF AM 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF AM dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfAmValue2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF AM 2");
                        }
                        if (Interlocked.Read(ref _vhfAmValue1WaitingForFeedback) == 0 || Interlocked.Read(ref _vhfAmValue2WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAm10sObject)
                            {
                                lock (_lockVhfAm1sObject)
                                {
                                    var frequencyWholeNumbers = GetVhfAmDialFrequencyFromRawValue(0, _vhfAmCockpit10sFrequencyValue) + "" + GetVhfAmDialFrequencyFromRawValue(0, _vhfAmCockpit1sFrequencyValue);
                                    if (int.Parse(frequencyWholeNumbers) != desiredPositionDialWholeNumbers)
                                    {
                                        Debug.Print("cockpit frequencyWholeNumbers = " + int.Parse(frequencyWholeNumbers));
                                        Debug.Print("desiredPositionDialWholeNumbers = " + desiredPositionDialWholeNumbers);
                                        //Debug.Print("_vhfAmCockpit10sFrequencyValue RAW = " + _vhfAmCockpit10sFrequencyValue);
                                        //Debug.Print("_vhfAmCockpit1sFrequencyValue RAW = " + _vhfAmCockpit1sFrequencyValue);
                                        var command = "";
                                        if (int.Parse(frequencyWholeNumbers) < desiredPositionDialWholeNumbers)
                                        {
                                            Debug.Print("frequencyWholeNumbers sending INC");
                                            command = VhfAmLeftDialDialCommandInc;
                                        }
                                        if (int.Parse(frequencyWholeNumbers) > desiredPositionDialWholeNumbers)
                                        {
                                            Debug.Print("frequencyWholeNumbers sending DEC");
                                            command = VhfAmLeftDialDialCommandDec;
                                        }
                                        DCSBIOS.Send(command);
                                        dial1Time = DateTime.Now.Ticks;
                                        dial1SendCount++;
                                        Interlocked.Exchange(ref _vhfAmValue1WaitingForFeedback, 1);
                                        Interlocked.Exchange(ref _vhfAmValue2WaitingForFeedback, 1);
                                        Reset(ref dial1Timeout);
                                    }
                                }
                            }
                        }
                        else
                        {
                            dial1Time = DateTime.Now.Ticks;
                        }
                        if (Interlocked.Read(ref _vhfAmValue3WaitingForFeedback) == 0 || Interlocked.Read(ref _vhfAmValue4WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAmDecimal10sObject)
                            {
                                lock (_lockVhfAmDecimal100sObject)
                                {
                                    var cockpitFrequencyDecimals = GetVhfAmDialFrequencyFromRawValue(0, _vhfAmCockpitDecimal10sFrequencyValue) + "" + GetVhfAmDialFrequencyFromRawValue(1, _vhfAmCockpitDecimal100sFrequencyValue);
                                    if (int.Parse(cockpitFrequencyDecimals) != desiredPositionDialDecimals)
                                    {
                                        /*Debug.Print("cockpit frequencyDecimals = " + int.Parse(frequencyDecimals));
                                        Debug.Print("desiredPositionDialDecimals = " + desiredPositionDialDecimals);
                                        Debug.Print("cockpit _vhfAmCockpitDecimal10sFrequencyValue RAW = " + _vhfAmCockpitDecimal10sFrequencyValue);
                                        Debug.Print("cockpit _vhfAmCockpitDecimal100sFrequencyValue RAW = " + _vhfAmCockpitDecimal100sFrequencyValue);*/
                                        if (SwitchVhfAmDecimalDirectionUp(int.Parse(cockpitFrequencyDecimals), desiredPositionDialDecimals))
                                        {
                                            Debug.Print("Sending INC");
                                            DCSBIOS.Send(VhfAmRightDialDialCommandInc);
                                        }
                                        else
                                        {
                                            Debug.Print("Sending DEC");
                                            DCSBIOS.Send(VhfAmRightDialDialCommandDec);
                                        }
                                        dial2Time = DateTime.Now.Ticks;
                                        dial2SendCount++;
                                        Interlocked.Exchange(ref _vhfAmValue3WaitingForFeedback, 1);
                                        Interlocked.Exchange(ref _vhfAmValue4WaitingForFeedback, 1);
                                        Reset(ref dial2Time);
                                    }
                                }
                            }
                        }
                        else
                        {
                            dial2Time = DateTime.Now.Ticks;
                        }
                        if (dial1SendCount > 30 || dial2SendCount > 40)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS

                    } while (IsTooShort(dial1Time) || IsTooShort(dial2Time));
                    SwapCockpitStandbyFrequencyVhfAm();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.LogError(56443, ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _vhfAmThreadNowSynching, 0);
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
                    case CurrentSA345RadioMode.VHFAM:
                        {
                            if (VhfAmNowSyncing())
                            {
                                return;
                            }
                            //Frequency selector 1      VHFAM_FREQ1
                            //1

                            //Frequency selector 2      VHFAM_FREQ2
                            //1-4

                            //Frequency selector 3      VHFAM_FREQ3
                            //0-9

                            //Frequency selector 4      VHFAM_FREQ4
                            //      "00" "25" "50" "75", only "00" and "50" used.
                            //Pos     0    1    2    3
                            var frequencyAsString = "";
                            lock (_lockVhfAm10sObject)
                            {
                                frequencyAsString = "1" + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Second, _vhfAmCockpit10sFrequencyValue);
                            }
                            lock (_lockVhfAm1sObject)
                            {
                                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Third, _vhfAmCockpit1sFrequencyValue);
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockVhfAmDecimal10sObject)
                            {
                                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Fourth, _vhfAmCockpitDecimal10sFrequencyValue);
                            }
                            lock (_lockVhfAmDecimal100sObject)
                            {
                                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(VhfAmDigit.LastTwoSpecial, _vhfAmCockpitDecimal100sFrequencyValue);
                            }
                            SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby, PZ69LCDPosition.UPPER_RIGHT);
                            break;
                        }
                    case CurrentSA345RadioMode.VHFFM:
                        {
                            //Presets
                            //0 - 8

                            uint preset = 0;
                            lock (_lockFmRadioPresetObject)
                            {
                                preset = _fmRadioPresetCockpitDialPos + 1;
                            }
                            SetPZ69DisplayBytesInteger(ref bytes, (int)preset, PZ69LCDPosition.UPPER_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_RIGHT);
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentSA345RadioMode.VHFAM:
                        {
                            if (VhfAmNowSyncing())
                            {
                                return;
                            }
                            //Frequency selector 1      VHFAM_FREQ1
                            //1

                            //Frequency selector 2      VHFAM_FREQ2
                            //1-4

                            //Frequency selector 3      VHFAM_FREQ3
                            //0-9

                            //Frequency selector 4      VHFAM_FREQ4
                            //      "00" "25" "50" "75", only "00" and "50" used.
                            //Pos     0    1    2    3
                            var frequencyAsString = "";
                            lock (_lockVhfAm10sObject)
                            {
                                frequencyAsString = "1" + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Second, _vhfAmCockpit10sFrequencyValue);
                            }
                            lock (_lockVhfAm1sObject)
                            {
                                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Third, _vhfAmCockpit1sFrequencyValue);
                            }
                            frequencyAsString = frequencyAsString + ".";
                            lock (_lockVhfAmDecimal10sObject)
                            {
                                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Fourth, _vhfAmCockpitDecimal10sFrequencyValue);
                            }
                            lock (_lockVhfAmDecimal100sObject)
                            {
                                frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(VhfAmDigit.LastTwoSpecial, _vhfAmCockpitDecimal100sFrequencyValue);
                            }
                            SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby, PZ69LCDPosition.LOWER_RIGHT);
                            break;
                        }
                    case CurrentSA345RadioMode.VHFFM:
                        {
                            //Presets
                            //0 - 8

                            uint preset = 0;
                            lock (_lockFmRadioPresetObject)
                            {
                                preset = _fmRadioPresetCockpitDialPos + 1;
                            }
                            SetPZ69DisplayBytesInteger(ref bytes, (int)preset, PZ69LCDPosition.LOWER_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_RIGHT);
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
                var radioPanelKnobSA345 = (RadioPanelKnobSA342)o;
                if (radioPanelKnobSA345.IsOn)
                {
                    switch (radioPanelKnobSA345.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmLeftDialChange())
                                            {
                                                if (_vhfAmBigFrequencyStandby.Equals(143.00))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                _vhfAmBigFrequencyStandby++;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {
                                            /*if (_vhfFmBigFrequencyStandby.Equals(76))
                                            {
                                                //@ max value
                                                break;
                                            }*/
                                            DCSBIOS.Send(FmRadioPresetCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmLeftDialChange())
                                            {
                                                if (_vhfAmBigFrequencyStandby.Equals(118.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                _vhfAmBigFrequencyStandby--;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {
                                            /*if (_vhfFmBigFrequencyStandby.Equals(30))
                                            {
                                                //@ min value
                                                break;
                                            }*/
                                            DCSBIOS.Send(FmRadioPresetCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmRightDialChange())
                                            {
                                                if (_vhfAmSmallFrequencyStandby >= 0.95)
                                                {
                                                    //At max value
                                                    _vhfAmSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby + 0.05;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmRightDialChange())
                                            {
                                                if (_vhfAmSmallFrequencyStandby <= 0.00)
                                                {
                                                    //At min value
                                                    _vhfAmSmallFrequencyStandby = 0.975;
                                                    break;
                                                }
                                                _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby - 0.025;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmLeftDialChange())
                                            {
                                                if (_vhfAmBigFrequencyStandby.Equals(143.00))
                                                {
                                                    //@ max value
                                                    break;
                                                }
                                                _vhfAmBigFrequencyStandby = _vhfAmBigFrequencyStandby + 1;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {
                                            /*if (_vhfFmBigFrequencyStandby.Equals(76))
                                            {
                                                //@ max value
                                                break;
                                            }*/
                                            DCSBIOS.Send(FmRadioPresetCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmLeftDialChange())
                                            {
                                                if (_vhfAmBigFrequencyStandby.Equals(118.00))
                                                {
                                                    //@ min value
                                                    break;
                                                }
                                                _vhfAmBigFrequencyStandby = _vhfAmBigFrequencyStandby - 1;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {
                                            /*if (_vhfFmBigFrequencyStandby.Equals(30))
                                            {
                                                //@ min value
                                                break;
                                            }*/
                                            DCSBIOS.Send(FmRadioPresetCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmRightDialChange())
                                            {
                                                if (_vhfAmSmallFrequencyStandby >= 0.975)
                                                {
                                                    //At max value
                                                    _vhfAmSmallFrequencyStandby = 0;
                                                    break;
                                                }
                                                _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby + 0.025;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
                                        {

                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA345RadioMode.VHFAM:
                                        {
                                            if (!SkipVhfAmRightDialChange())
                                            {
                                                if (_vhfAmSmallFrequencyStandby <= 0.00)
                                                {
                                                    //At min value
                                                    _vhfAmSmallFrequencyStandby = 0.975;
                                                    break;
                                                }
                                                _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby - 0.025;
                                            }
                                            break;
                                        }
                                    case CurrentSA345RadioMode.VHFFM:
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
            //VHF AM
            if (_vhfAmBigFrequencyStandby < 118)
            {
                _vhfAmBigFrequencyStandby = 118;
            }
            if (_vhfAmBigFrequencyStandby > 148)
            {
                _vhfAmBigFrequencyStandby = 148;
            }
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (_lockLCDUpdateObject)
            {
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobSA342)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {

                        case RadioPanelPZ69KnobsSA342.UPPER_VHFAM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA345RadioMode.VHFAM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA345RadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_XPDR:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_VHFAM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA345RadioMode.VHFAM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA345RadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_XPDR:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH);
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH);
                                }
                                break;
                            }
                    }


                }
                AdjustFrequency(hashSet);
            }
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("A-10C");

                //VHF AM
                _vhfAmDcsbiosOutputReading10s = DCSBIOSControlLocator.GetDCSBIOSOutput("AM_RADIO_FREQ_10s");
                _vhfAmDcsbiosOutputReading1s = DCSBIOSControlLocator.GetDCSBIOSOutput("AM_RADIO_FREQ_1s");
                _vhfAmDcsbiosOutputReadingDecimal10s = DCSBIOSControlLocator.GetDCSBIOSOutput("AM_RADIO_FREQ_TENTHS");
                _vhfAmDcsbiosOutputReadingDecimal100s = DCSBIOSControlLocator.GetDCSBIOSOutput("AM_RADIO_FREQ_HUNDREDTHS");

                //FM PR4G
                _fmRadioPresetDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("FM_RADIO_CHANNEL");

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69S342.StartUp() : " + ex.Message);
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
            _radioPanelKnobs = RadioPanelKnobSA342.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobSA342 radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private string GetVhfAmDialFrequencyFromRawValue(int dial, uint position)
        {
            switch (dial)
            {
                case 0:
                    {
                        for (int i = 0; i < _dialPositionsWholeNumbers.Length; i++)
                        {
                            if (_dialPositionsWholeNumbers[i] == position)
                            {
                                return i.ToString();
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        switch (position)
                        {
                            case 0:
                                {
                                    return "00";
                                }
                            case 16383:
                                {
                                    return "25";
                                }
                            case 32767:
                                {
                                    return "50";
                                }
                            case 49151:
                                {
                                    return "75";
                                }
                        }
                    }
                    break;
            }
            return "00";
        }

        private string GetVhfAmDialFrequencyForPosition(VhfAmDigit vhfAmDigit, uint position)
        {
            switch (vhfAmDigit)
            {
                case VhfAmDigit.First:
                case VhfAmDigit.Second:
                case VhfAmDigit.Third:
                case VhfAmDigit.Fourth:
                    {
                        for (int i = 0; i < _dialPositionsWholeNumbers.Length; i++)
                        {
                            if (_dialPositionsWholeNumbers[i] == position)
                            {
                                return i.ToString();
                            }
                        }
                        break;
                    }
                case VhfAmDigit.LastTwoSpecial:
                    {
                        switch (position)
                        {
                            case 0:
                                {
                                    return "00";
                                }
                            case 16383:
                                {
                                    return "25";
                                }
                            case 32767:
                                {
                                    return "50";
                                }
                            case 49151:
                                {
                                    return "75";
                                }
                        }
                    }
                    break;
            }
            return "00";
        }

        private void SaveCockpitFrequencyVhfAm()
        {
            /*
             * Dial 1
             * 1
             * 
             * Dial 2
             * 0 - 4
             * 
             * Dial 3
             * 0 - 9
             * 
             * "."
             * 
             * Dial 4
             * 00/25/75/50
             */
            lock (_lockVhfAm10sObject)
            {
                lock (_lockVhfAm10sObject)
                {
                    lock (_lockVhfAm1sObject)
                    {
                        lock (_lockVhfAmDecimal10sObject)
                        {
                            var dial10s = GetVhfAmDialFrequencyForPosition(VhfAmDigit.Second, _vhfAmCockpit10sFrequencyValue);
                            var dial1s = GetVhfAmDialFrequencyForPosition(VhfAmDigit.Third, _vhfAmCockpit1sFrequencyValue);
                            var diald10s = GetVhfAmDialFrequencyForPosition(VhfAmDigit.Fourth, _vhfAmCockpitDecimal10sFrequencyValue);
                            var diald100s = GetVhfAmDialFrequencyForPosition(VhfAmDigit.LastTwoSpecial, _vhfAmCockpitDecimal100sFrequencyValue);

                            Debug.Print("SaveCockpitFrequencyVhfAm : 0. + diald10s + diald100s -> " + "0." + diald10s + diald100s);
                            _vhfAmSavedCockpitBigFrequency = Double.Parse("1" + dial10s + dial1s, NumberFormatInfoFullDisplay);
                            _vhfAmSavedCockpitSmallFrequency = Double.Parse("0." + diald10s + diald100s, NumberFormatInfoFullDisplay);
                        }
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVhfAm()
        {
            Debug.Print("Before swap : ");
            Debug.Print("_vhfAmBigFrequencyStandby : " + _vhfAmBigFrequencyStandby);
            Debug.Print("_vhfAmSmallFrequencyStandby : " + _vhfAmSmallFrequencyStandby);
            Debug.Print("_vhfAmSavedCockpitBigFrequency : " + _vhfAmSavedCockpitBigFrequency);
            Debug.Print("_vhfAmSavedCockpitSmallFrequency : " + _vhfAmSavedCockpitSmallFrequency);
            _vhfAmBigFrequencyStandby = _vhfAmSavedCockpitBigFrequency;
            _vhfAmSmallFrequencyStandby = _vhfAmSavedCockpitSmallFrequency;
        }


        private bool CorrectPositionWholeNumbers(uint value)
        {
            for (int i = 0; i < _dialPositionsWholeNumbers.Length; i++)
            {
                if (_dialPositionsWholeNumbers[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CorrectPositionDecimal100s(uint value)
        {
            for (int i = 0; i < _dialPositionsDecial100s.Length; i++)
            {
                if (_dialPositionsDecial100s[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VhfAmNowSyncing()
        {
            return Interlocked.Read(ref _vhfAmThreadNowSynching) > 0;
        }

        public override String SettingsVersion()
        {
            return "0X";
        }

        private bool SwitchVhfAmDecimalDirectionUp(int cockpitValue, int desiredValue)
        {
            var upCount = 0;
            var downCount = 0;
            var tmpCockpitValue = cockpitValue;
            while(tmpCockpitValue != desiredValue)
            {
                upCount++;
                tmpCockpitValue = tmpCockpitValue + 25;
                if (tmpCockpitValue > 975)
                {
                    tmpCockpitValue = 0;
                }
            }
            tmpCockpitValue = cockpitValue;
            while (tmpCockpitValue != desiredValue)
            {
                downCount++;
                tmpCockpitValue = tmpCockpitValue - 25;
                if (tmpCockpitValue < 0)
                {
                    tmpCockpitValue = 975;
                }
            }
            Debug.Print("SwitchVhfAmDecimalDirectionUp cockpitValue " + cockpitValue);
            Debug.Print("SwitchVhfAmDecimalDirectionUp desiredValue " + desiredValue);
            Debug.Print("SwitchVhfAmDecimalDirectionUp upCount " + upCount);
            Debug.Print("SwitchVhfAmDecimalDirectionUp downCount " + downCount);
            if (upCount < downCount)
            {
                return true;
            }
                return false;
        }

        private bool SkipVhfAmLeftDialChange()
        {
            try
            {
                Common.DebugP("Entering SA342 Radio SkipVhfAmLeftDialChange()");
                if (_currentUpperRadioMode == CurrentSA345RadioMode.VHFAM || _currentLowerRadioMode == CurrentSA345RadioMode.VHFAM)
                {
                    if (_vhfAmLeftDialSkipper > 2)
                    {
                        _vhfAmLeftDialSkipper = 0;
                        Common.DebugP("Leaving SA342 Radio SkipVhfAmLeftDialChange()");
                        return false;
                    }
                    _vhfAmLeftDialSkipper++;
                    Common.DebugP("Leaving SA342 Radio SkipVhfAmLeftDialChange()");
                    return true;
                }
                Common.DebugP("Leaving SA342 Radio SkipVhfAmLeftDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(77009, ex);
            }
            return false;
        }

        private bool SkipVhfAmRightDialChange()
        {
            try
            {
                Common.DebugP("Entering SA342 Radio SkipVhfAmRightDialChange()");
                if (_currentUpperRadioMode == CurrentSA345RadioMode.VHFAM || _currentLowerRadioMode == CurrentSA345RadioMode.VHFAM)
                {
                    if (_vhfAmRightDialSkipper > 4)
                    {
                        _vhfAmRightDialSkipper = 0;
                        Common.DebugP("Leaving SA342 Radio SkipVhfAmRightDialChange()");
                        return false;
                    }
                    _vhfAmRightDialSkipper++;
                    Common.DebugP("Leaving SA342 Radio SkipVhfAmRightDialChange()");
                    return true;
                }
                Common.DebugP("Leaving SA342 Radio SkipVhfAmRightDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(77009, ex);
            }
            return false;
        }
    }

}
