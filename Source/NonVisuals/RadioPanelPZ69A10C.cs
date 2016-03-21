using System;
using System.Collections.Generic;
using System.Threading;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69A10C : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private HashSet<RadioPanelKnobA10C> _radioPanelKnobs = new HashSet<RadioPanelKnobA10C>();
        private CurrentA10RadioMode _currentUpperRadioMode = CurrentA10RadioMode.UHF;
        private CurrentA10RadioMode _currentLowerRadioMode = CurrentA10RadioMode.UHF;

        /*A-10C AN/ARC-186(V) VHF AM Radio 1*/
        //Large dial 116-151 [step of 1]
        //Small dial 0.00-0.95 [step of 0.05]
        private double _vhfAmBigFrequencyStandby = 116;
        private double _vhfAmSmallFrequencyStandby;
        private double _vhfAmSavedActiveBigFrequency;
        private double _vhfAmSavedActiveSmallFrequency;
        private object _lockVhfAmDialsObject1 = new object();
        private object _lockVhfAmDialsObject2 = new object();
        private object _lockVhfAmDialsObject3 = new object();
        private object _lockVhfAmDialsObject4 = new object();
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfAmDcsbiosOutputFreqDial4;
        private volatile uint _vhfAmActiveFreq1DialPos = 1;
        private volatile uint _vhfAmActiveFreq2DialPos = 1;
        private volatile uint _vhfAmActiveFreq3DialPos = 1;
        private volatile uint _vhfAmActiveFreq4DialPos = 1;
        private const string VhfAmFreq1DialCommand = "VHFAM_FREQ1 ";
        private const string VhfAmFreq2DialCommand = "VHFAM_FREQ2 ";
        private const string VhfAmFreq3DialCommand = "VHFAM_FREQ3 ";
        private const string VhfAmFreq4DialCommand = "VHFAM_FREQ4 ";
        private Thread _vhfAmSyncThread;
        private long _vhfAmThreadNowSynching;
        private long _vhfAmDial1WaitingForFeedback;
        private long _vhfAmDial2WaitingForFeedback;
        private long _vhfAmDial3WaitingForFeedback;
        private long _vhfAmDial4WaitingForFeedback;

        /*A-10C AN/ARC-164 UHF Radio 2*/
        //Large dial 225-399 [step of 1]
        //Small dial 0.00-0.95 [step of 0.05]
        private double _uhfBigFrequencyStandby = 299;
        private double _uhfSmallFrequencyStandby;
        private double _uhfSavedActiveBigFrequency;
        private double _uhfSavedActiveSmallFrequency;
        private object _lockUhfDialsObject1 = new object();
        private object _lockUhfDialsObject2 = new object();
        private object _lockUhfDialsObject3 = new object();
        private object _lockUhfDialsObject4 = new object();
        private object _lockUhfDialsObject5 = new object();
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial4;
        private DCSBIOSOutput _uhfDcsbiosOutputFreqDial5;
        private volatile uint _uhfActiveFreq1DialPos = 1;
        private volatile uint _uhfActiveFreq2DialPos = 1;
        private volatile uint _uhfActiveFreq3DialPos = 1;
        private volatile uint _uhfActiveFreq4DialPos = 1;
        private volatile uint _uhfActiveFreq5DialPos = 1;
        private const string UhfFreq1DialCommand = "UHF_100MHZ_SEL ";		//"2" "3" "A"
        private const string UhfFreq2DialCommand = "UHF_10MHZ_SEL ";		//0 1 2 3 4 5 6 7 8 9
        private const string UhfFreq3DialCommand = "UHF_1MHZ_SEL ";			//0 1 2 3 4 5 6 7 8 9
        private const string UhfFreq4DialCommand = "UHF_POINT1MHZ_SEL ";    //0 1 2 3 4 5 6 7 8 9
        private const string UhfFreq5DialCommand = "UHF_POINT25_SEL ";		//"00" "25" "50" "75"
        private Thread _uhfSyncThread;
        private long _uhfThreadNowSynching;
        private long _uhfDial1WaitingForFeedback;
        private long _uhfDial2WaitingForFeedback;
        private long _uhfDial3WaitingForFeedback;
        private long _uhfDial4WaitingForFeedback;
        private long _uhfDial5WaitingForFeedback;

        /*A-10C AN/ARC-186(V) VHF FM Radio 3*/
        //Large dial 30-76 [step of 1]
        //Small dial 0.00-0.95 [step of 0.05]
        private uint _vhfFmBigFrequencyStandby = 45;
        private uint _vhfFmSmallFrequencyStandby;
        private uint _vhfFmSavedActiveBigFrequency;
        private uint _vhfFmSavedActiveSmallFrequency;
        private object _lockVhfFmDialsObject1 = new object();
        private object _lockVhfFmDialsObject2 = new object();
        private object _lockVhfFmDialsObject3 = new object();
        private object _lockVhfFmDialsObject4 = new object();
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial4;
        private volatile uint _vhfFmActiveFreq1DialPos = 1;
        private volatile uint _vhfFmActiveFreq2DialPos = 1;
        private volatile uint _vhfFmActiveFreq3DialPos = 1;
        private volatile uint _vhfFmActiveFreq4DialPos = 1;
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

        /*A-10C ILS*/
        //Large dial 108-111 [step of 1]
        //Small dial 10-95 [step of 5]
        private uint _ilsBigFrequencyStandby = 108; //"108" "109" "110" "111"
        private uint _ilsSmallFrequencyStandby = 10; //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
        private uint _ilsSavedActiveBigFrequency = 108; //"108" "109" "110" "111"
        private uint _ilsSavedActiveSmallFrequency = 10; //"10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
        private object _lockIlsDialsObject1 = new object();
        private object _lockIlsDialsObject2 = new object();
        private DCSBIOSOutput _ilsDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _ilsDcsbiosOutputFreqDial2;
        private volatile uint _ilsActiveFreq1DialPos = 1;
        private volatile uint _ilsActiveFreq2DialPos = 1;
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
        private int _tacanSavedActiveBigFrequency = 6;
        private int _tacanSavedActiveSmallFrequency = 5;
        private int _tacanSavedActiveXY;
        private object _lockTacanDialsObject1 = new object();
        private object _lockTacanDialsObject2 = new object();
        private object _lockTacanDialsObject3 = new object();
        private DCSBIOSOutput _tacanDcsbiosOutputFreqChannel;
        private volatile uint _tacanActiveFreq1DialPos = 1;
        private volatile uint _tacanActiveFreq2DialPos = 1;
        private volatile uint _tacanActiveFreq3DialPos = 1;
        private const string TacanFreq1DialCommand = "TACAN_10 ";
        private const string TacanFreq2DialCommand = "TACAN_1 ";
        private const string TacanFreq3DialCommand = "TACAN_XY ";
        private Thread _tacanSyncThread;
        private long _tacanThreadNowSynching;
        private long _tacanDial1WaitingForFeedback;
        private long _tacanDial2WaitingForFeedback;
        private long _tacanDial3WaitingForFeedback;

        public RadioPanelPZ69A10C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69A10C()
        {
            if (_vhfAmSyncThread != null)
            {
                _vhfAmSyncThread.Abort();
            }
            if (_vhfFmSyncThread != null)
            {
                _vhfFmSyncThread.Abort();
            }
            if (_uhfSyncThread != null)
            {
                _uhfSyncThread.Abort();
            }
            if (_ilsSyncThread != null)
            {
                _ilsSyncThread.Abort();
            }
            if (_tacanSyncThread != null)
            {
                _tacanSyncThread.Abort();
            }
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //Common.DebugP("PZ69 A10 READ ENTERING");
            UpdateCounter(address, data);
            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */

            //VHF AM
            if (address == _vhfAmDcsbiosOutputFreqDial1.Address)
            {
                //Common.DebugP("VHFAM_FREQ1 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfAmDialsObject1)
                {
                    //Common.DebugP("Just read VHF AM Dial 1 Position: " + _vhfAmActiveFreq1DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfAmActiveFreq1DialPos;
                    _vhfAmActiveFreq1DialPos = _vhfAmDcsbiosOutputFreqDial1.GetUIntValue(data);
                    if (tmp != _vhfAmActiveFreq1DialPos)
                    {
                        //Common.DebugP("VHFAM_FREQ1 Before : " + tmp + "  now: " + _vhfAmActiveFreq1DialPos);
                        Interlocked.Exchange(ref _vhfAmDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfAmDcsbiosOutputFreqDial2.Address)
            {
                //Common.DebugP("VHFAM_FREQ2 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfAmDialsObject2)
                {
                    //Common.DebugP("Just read VHF AM Dial 2 Position: " + _vhfAmActiveFreq2DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfAmActiveFreq2DialPos;
                    _vhfAmActiveFreq2DialPos = _vhfAmDcsbiosOutputFreqDial2.GetUIntValue(data);
                    if (tmp != _vhfAmActiveFreq2DialPos)
                    {
                        //Common.DebugP("VHFAM_FREQ2 Before : " + tmp + "  now: " + _vhfAmActiveFreq2DialPos);
                        Interlocked.Exchange(ref _vhfAmDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfAmDcsbiosOutputFreqDial3.Address)
            {
                //Common.DebugP("VHFAM_FREQ3 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfAmDialsObject3)
                {
                    //Common.DebugP("Just read VHF AM Dial 3 Position: " + _vhfAmActiveFreq3DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfAmActiveFreq3DialPos;
                    _vhfAmActiveFreq3DialPos = _vhfAmDcsbiosOutputFreqDial3.GetUIntValue(data);
                    if (tmp != _vhfAmActiveFreq3DialPos)
                    {
                        //Common.DebugP("VHFAM_FREQ3 Before : " + tmp + "  now: " + _vhfAmActiveFreq3DialPos);
                        Interlocked.Exchange(ref _vhfAmDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfAmDcsbiosOutputFreqDial4.Address)
            {
                //Common.DebugP("VHFAM_FREQ4 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfAmDialsObject4)
                {
                    //Common.DebugP("Just read VHF AM Dial 4 Position: " + _vhfAmActiveFreq4DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfAmActiveFreq4DialPos;
                    _vhfAmActiveFreq4DialPos = _vhfAmDcsbiosOutputFreqDial4.GetUIntValue(data);
                    if (tmp != _vhfAmActiveFreq4DialPos)
                    {
                        //Common.DebugP("VHFAM_FREQ4 Before : " + tmp + "  now: " + _vhfAmActiveFreq4DialPos);
                        Interlocked.Exchange(ref _vhfAmDial4WaitingForFeedback, 0);
                    }
                }
            }

            //UHF
            if (address == _uhfDcsbiosOutputFreqDial1.Address)
            {
                //Common.DebugP("UHF_100MHZ_SEL Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockUhfDialsObject1)
                {
                    //Common.DebugP("Just read UHF Dial 1 Position: " + _uhfActiveFreq1DialPos + "  " + +Environment.TickCount);
                    var tmp = _uhfActiveFreq1DialPos;
                    _uhfActiveFreq1DialPos = _uhfDcsbiosOutputFreqDial1.GetUIntValue(data);
                    if (tmp != _uhfActiveFreq1DialPos)
                    {
                        Common.DebugP("_uhfActiveFreq1DialPos Before : " + tmp + "  now: " + _uhfActiveFreq1DialPos);
                        Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _uhfDcsbiosOutputFreqDial2.Address)
            {
                //Common.DebugP("UHF_10MHZ_SEL Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockUhfDialsObject2)
                {
                    //Common.DebugP("Just read UHF Dial 2 Position: " + _uhfActiveFreq2DialPos + "  " + +Environment.TickCount);
                    var tmp = _uhfActiveFreq2DialPos;
                    _uhfActiveFreq2DialPos = _uhfDcsbiosOutputFreqDial2.GetUIntValue(data);
                    if (tmp != _uhfActiveFreq2DialPos)
                    {
                        //Common.DebugP("UHF_10MHZ_SEL Before : " + tmp + "  now: " + _uhfActiveFreq2DialPos);
                        Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _uhfDcsbiosOutputFreqDial3.Address)
            {
                //Common.DebugP("VHFAM_FREQ1 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockUhfDialsObject3)
                {
                    //Common.DebugP("Just read UHF Dial 3 Position: " + _uhfActiveFreq3DialPos + "  " + +Environment.TickCount);
                    var tmp = _uhfActiveFreq3DialPos;
                    _uhfActiveFreq3DialPos = _uhfDcsbiosOutputFreqDial3.GetUIntValue(data);
                    if (tmp != _uhfActiveFreq3DialPos)
                    {
                        //Common.DebugP("UHF_1MHZ_SEL Before : " + tmp + "  now: " + _uhfActiveFreq3DialPos);
                        Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _uhfDcsbiosOutputFreqDial4.Address)
            {
                //Common.DebugP("UHF_POINT1MHZ_SEL Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockUhfDialsObject4)
                {
                    //Common.DebugP("Just read UHF Dial 4 Position: " + _uhfActiveFreq4DialPos + "  " + +Environment.TickCount);
                    var tmp = _uhfActiveFreq4DialPos;
                    _uhfActiveFreq4DialPos = _uhfDcsbiosOutputFreqDial4.GetUIntValue(data);
                    if (tmp != _uhfActiveFreq4DialPos)
                    {
                        //Common.DebugP("UHF_POINT1MHZ_SEL Before : " + tmp + "  now: " + _uhfActiveFreq4DialPos);
                        Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _uhfDcsbiosOutputFreqDial5.Address)
            {
                //Common.DebugP("UHF_POINT25_SEL Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockUhfDialsObject5)
                {
                    //Common.DebugP("Just read UHF Dial 5 Position: " + _uhfActiveFreq5DialPos + "  " + +Environment.TickCount);
                    var tmp = _uhfActiveFreq5DialPos;
                    _uhfActiveFreq5DialPos = _uhfDcsbiosOutputFreqDial5.GetUIntValue(data);
                    if (tmp != _uhfActiveFreq5DialPos)
                    {
                        //Common.DebugP("UHF_POINT25_SEL Before : " + tmp + "  now: " + _uhfActiveFreq5DialPos);
                        Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 0);
                    }
                }
            }


            //VHF FM
            if (address == _vhfFmDcsbiosOutputFreqDial1.Address)
            {
                //Common.DebugP("VHFFM_FREQ1 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject1)
                {
                    //Common.DebugP("Just read VHF FM Dial 1 Position: " + _vhfFmActiveFreq1DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfFmActiveFreq1DialPos;
                    _vhfFmActiveFreq1DialPos = _vhfFmDcsbiosOutputFreqDial1.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq1DialPos)
                    {
                        //Common.DebugP("VHFFM_FREQ1 Before : " + tmp + "  now: " + _vhfFmActiveFreq1DialPos);
                        Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfFmDcsbiosOutputFreqDial2.Address)
            {
                //Common.DebugP("VHFAM_FREQ1 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject2)
                {
                    //Common.DebugP("Just read VHF FM Dial 2 Position: " + _vhfFmActiveFreq2DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfFmActiveFreq2DialPos;
                    _vhfFmActiveFreq2DialPos = _vhfFmDcsbiosOutputFreqDial2.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq2DialPos)
                    {
                        //Common.DebugP("VHFFM_FREQ2 Before : " + tmp + "  now: " + _vhfFmActiveFreq2DialPos);
                        Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfFmDcsbiosOutputFreqDial3.Address)
            {
                //Common.DebugP("VHFFM_FREQ3 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject3)
                {
                    //Common.DebugP("Just read VHF FM Dial 3 Position: " + _vhfFmActiveFreq3DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfFmActiveFreq3DialPos;
                    _vhfFmActiveFreq3DialPos = _vhfFmDcsbiosOutputFreqDial3.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq3DialPos)
                    {
                        //Common.DebugP("VHFFM_FREQ3 Before : " + tmp + "  now: " + _vhfFmActiveFreq3DialPos);
                        Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfFmDcsbiosOutputFreqDial4.Address)
            {
                //Common.DebugP("VHFFM_FREQ4 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject4)
                {
                    //Common.DebugP("Just read VHF FM Dial 4 Position: " + _vhfFmActiveFreq4DialPos + "  " + +Environment.TickCount);
                    var tmp = _vhfFmActiveFreq4DialPos;
                    _vhfFmActiveFreq4DialPos = _vhfFmDcsbiosOutputFreqDial4.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq4DialPos)
                    {
                        //Common.DebugP("VHFFM_FREQ4 Before : " + tmp + "  now: " + _vhfFmActiveFreq4DialPos);
                        Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 0);
                    }
                }
            }

            //ILS
            if (address == _ilsDcsbiosOutputFreqDial1.Address)
            {
                //Common.DebugP("ILS_MHZ Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockIlsDialsObject1)
                {
                    //Common.DebugP("Just read ILS Mhz Dial 1 Position: " + _ilsMhzActiveFreq1DialPos + "  " + +Environment.TickCount);
                    var tmp = _ilsActiveFreq1DialPos;
                    _ilsActiveFreq1DialPos = _ilsDcsbiosOutputFreqDial1.GetUIntValue(data);
                    if (tmp != _ilsActiveFreq1DialPos)
                    {
                        //Common.DebugP("ILS_MHZ Before : " + tmp + "  now: " + _ilsMhzActiveFreq1DialPos);
                        Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _ilsDcsbiosOutputFreqDial2.Address)
            {
                //Common.DebugP("ILS_KHZ Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockIlsDialsObject2)
                {
                    //Common.DebugP("Just read ILS Khz Dial 2 Position: " + _ilsKhzActiveFreq2DialPos + "  " + +Environment.TickCount);
                    var tmp = _ilsActiveFreq2DialPos;
                    _ilsActiveFreq2DialPos = _ilsDcsbiosOutputFreqDial2.GetUIntValue(data);
                    if (tmp != _ilsActiveFreq2DialPos)
                    {
                        //Common.DebugP("ILS_KHZ Before : " + tmp + "  now: " + _ilsKhzActiveFreq2DialPos);
                        Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 0);
                    }
                }
            }

            //TACAN is set via String listener

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();

            //Common.DebugP("PZ69 A10 READ EXITING");
        }

        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + stringData);
                    return;
                }
                if (address.Equals(_tacanDcsbiosOutputFreqChannel.Address))
                {
                    //" 00X" --> "129X"
                    lock (_lockTacanDialsObject1)
                    {
                        var tmp = _tacanActiveFreq1DialPos;
                        _tacanActiveFreq1DialPos = uint.Parse(stringData.Substring(0, 2));
                        if (tmp != _tacanActiveFreq1DialPos)
                        {
                            //Common.DebugP("TACAN DIAL 1 Before : " + tmp + "  now: " + _tacanActiveFreq1DialPos);
                            Interlocked.Exchange(ref _tacanDial1WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockTacanDialsObject2)
                    {
                        var tmp = _tacanActiveFreq2DialPos;
                        _tacanActiveFreq2DialPos = uint.Parse(stringData.Substring(2, 1));
                        if (tmp != _tacanActiveFreq2DialPos)
                        {
                            //Common.DebugP("TACAN DIAL 2 Before : " + tmp + "  now: " + _tacanActiveFreq2DialPos);
                            Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockTacanDialsObject3)
                    {
                        var tmp = _tacanActiveFreq3DialPos;
                        var tmpXY = stringData.Substring(3, 1);
                        _tacanActiveFreq3DialPos = tmpXY.Equals("X") ? (uint)0 : (uint)1;
                        if (tmp != _tacanActiveFreq3DialPos)
                        {
                            //Common.DebugP("TACAN DIAL 3 Before : " + tmp + "  now: " + _tacanActiveFreq3DialPos);
                            Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.LogError(349998, e, "DCSBIOSStringReceived()");
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
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentA10RadioMode.VHFAM:
                                {
                                    SendVhfAmToDCSBIOS();
                                    break;
                                }
                            case CurrentA10RadioMode.UHF:
                                {
                                    SendUhfToDCSBIOS();
                                    break;
                                }
                            case CurrentA10RadioMode.VHFFM:
                                {
                                    SendVhfFmToDCSBIOS();
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
                                    SendVhfAmToDCSBIOS();
                                    break;
                                }
                            case CurrentA10RadioMode.UHF:
                                {
                                    SendUhfToDCSBIOS();
                                    break;
                                }
                            case CurrentA10RadioMode.VHFFM:
                                {
                                    SendVhfFmToDCSBIOS();
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
            SaveActiveFrequencyVhfAm();
            var frequency = _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby;
            var frequencyAsString = frequency.ToString("0.00", NumberFormatInfoFullDisplay);
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


            var desiredPositionDial1 = 0;
            var desiredPositionDial2 = 0;
            var desiredPositionDial3 = 0;
            var desiredPositionDial4 = 0;

            if (frequencyAsString.IndexOf(".", StringComparison.InvariantCulture) == 2)
            {
                //30.00
                //#1 = 3  (position = value - 3)
                //#2 = 0   (position = value)
                //#3 = 0   (position = value)
                //#4 = 00
                desiredPositionDial1 = int.Parse(frequencyAsString.Substring(0, 1)) - 3;
                desiredPositionDial2 = int.Parse(frequencyAsString.Substring(1, 1));
                desiredPositionDial3 = int.Parse(frequencyAsString.Substring(3, 1));
                desiredPositionDial4 = int.Parse(frequencyAsString.Substring(4, 1));
            }
            else
            {
                //151.95
                //#1 = 15  (position = value - 3)
                //#2 = 1   (position = value)
                //#3 = 9   (position = value)
                //#4 = 5
                desiredPositionDial1 = int.Parse(frequencyAsString.Substring(0, 2)) - 3;
                desiredPositionDial2 = int.Parse(frequencyAsString.Substring(2, 1));
                desiredPositionDial3 = int.Parse(frequencyAsString.Substring(4, 1));
                desiredPositionDial4 = int.Parse(frequencyAsString.Substring(5, 1));
            }
            //#1
            if (_vhfAmSyncThread != null)
            {
                _vhfAmSyncThread.Abort();
            }
            _vhfAmSyncThread = new Thread(() => VhfAmSynchThreadMethod(desiredPositionDial1, desiredPositionDial2, desiredPositionDial3, desiredPositionDial4));
            _vhfAmSyncThread.Start();

        }

        private void VhfAmSynchThreadMethod(int desiredPositionDial1, int desiredPositionDial2, int desiredPositionDial3, int frequencyDial4)
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

                    string str;
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
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF AM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfAmDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF AM 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF AM dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfAmDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF AM 2");
                        }
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "VHF AM dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfAmDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF AM 3");
                        }
                        if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "VHF AM dial4Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfAmDial4WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF AM 4");
                        }
                        if (Interlocked.Read(ref _vhfAmDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAmDialsObject1)
                            {

                                //Common.DebugP("_vhfAmActiveFreq1DialPos is " + _vhfAmActiveFreq1DialPos + " and should be " + positionDial1);
                                if (_vhfAmActiveFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    str = VhfAmFreq1DialCommand + GetCommandDirectionForVhfDial1(desiredPositionDial1, _vhfAmActiveFreq1DialPos);
                                    Common.DebugP("Sending " + str);
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
                                //Common.DebugP("_vhfAmActiveFreq2DialPos is " + _vhfAmActiveFreq2DialPos + " and should be " + positionDial2);
                                if (_vhfAmActiveFreq2DialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    str = VhfAmFreq2DialCommand + GetCommandDirectionForVhfDial23(desiredPositionDial2, _vhfAmActiveFreq2DialPos);
                                    Common.DebugP("Sending " + str);
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
                                //Common.DebugP("_vhfAmActiveFreq3DialPos is " + _vhfAmActiveFreq3DialPos + " and should be " + positionDial3);
                                if (_vhfAmActiveFreq3DialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    str = VhfAmFreq3DialCommand + GetCommandDirectionForVhfDial23(desiredPositionDial3, _vhfAmActiveFreq3DialPos);
                                    Common.DebugP("Sending " + str);
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
                        var desiredPositionDial4 = 0;
                        if (Interlocked.Read(ref _vhfAmDial4WaitingForFeedback) == 0)
                        {
                            if (frequencyDial4 == 0)
                            {
                                desiredPositionDial4 = 0;
                            }
                            else if (frequencyDial4 == 2)
                            {
                                desiredPositionDial4 = 0;
                            }
                            else if (frequencyDial4 == 5)
                            {
                                desiredPositionDial4 = 2;
                            }
                            else if (frequencyDial4 == 7)
                            {
                                desiredPositionDial4 = 2;
                            }
                            //      "00" "25" "50" "75", only "00" and "50" used.
                            //Pos     0    1    2    3

                            lock (_lockVhfAmDialsObject4)
                            {
                                //Common.DebugP("_vhfAmActiveFreq4DialPos is " + _vhfAmActiveFreq4DialPos + " and should be " + positionDial4);
                                if (_vhfAmActiveFreq4DialPos < desiredPositionDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    str = VhfAmFreq4DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfAmDial4WaitingForFeedback, 1);
                                }
                                else if (_vhfAmActiveFreq4DialPos > desiredPositionDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    str = VhfAmFreq4DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
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
                }
                finally
                {
                    Interlocked.Exchange(ref _vhfAmThreadNowSynching, 0);
                }
                SwapActiveStandbyFrequencyVhfAm();
                ShowFrequenciesOnPanel();
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(56443, ex);
            }
        }

        private void SendUhfToDCSBIOS()
        {
            if (UhfNowSyncing())
            {
                return;
            }
            SaveActiveFrequencyUhf();
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
                //      "00" "25" "50" "75", only "00" and "50" used.
                //Pos     0    1    2    3
                case 0:
                    {
                        break;
                    }
                case 2:
                    {
                        freqDial5 = 0;
                        break;
                    }
                case 5:
                    {
                        freqDial5 = 2;
                        break;
                    }
                case 7:
                    {
                        freqDial5 = 2;
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
            if (_uhfSyncThread != null)
            {
                _uhfSyncThread.Abort();
            }
            if (freqDial1 >= 2 && freqDial1 <= 3)
            {
                _uhfSyncThread = new Thread(() => UhfSynchThreadMethod(freqDial1 - 2, freqDial2, freqDial3, freqDial4, freqDial5));
            }
            else
            {
                //The first dial is set to "A", pos 2   (freqDial1 == -1)
                _uhfSyncThread = new Thread(() => UhfSynchThreadMethod(2, freqDial2, freqDial3, freqDial4, freqDial5));
            }
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
                                if (_uhfActiveFreq1DialPos != desiredPosition1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_uhfActiveFreq1DialPos is " + _uhfActiveFreq1DialPos + " and should be " + position1);
                                if (_uhfActiveFreq1DialPos < desiredPosition1)
                                {
                                    var str = UhfFreq1DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 1);
                                }
                                else if (_uhfActiveFreq1DialPos > desiredPosition1)
                                {
                                    var str = UhfFreq1DialCommand + "DEC\n";
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
                                if (_uhfActiveFreq2DialPos != desiredPosition2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_uhfActiveFreq2DialPos is " + _uhfActiveFreq2DialPos + " and should be " + position2);
                                if (_uhfActiveFreq2DialPos < desiredPosition2)
                                {
                                    var str = UhfFreq2DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 1);
                                }
                                else if (_uhfActiveFreq2DialPos > desiredPosition2)
                                {
                                    var str = UhfFreq2DialCommand + "DEC\n";
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
                                if (_uhfActiveFreq3DialPos != desiredPosition3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_uhfActiveFreq3DialPos is " + _uhfActiveFreq3DialPos + " and should be " + position3);
                                if (_uhfActiveFreq3DialPos < desiredPosition3)
                                {
                                    var str = UhfFreq3DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 1);
                                }
                                else if (_uhfActiveFreq3DialPos > desiredPosition3)
                                {
                                    var str = UhfFreq3DialCommand + "DEC\n";
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
                                if (_uhfActiveFreq4DialPos != desiredPosition4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_uhfActiveFreq4DialPos is " + _uhfActiveFreq4DialPos + " and should be " + position4);
                                if (_uhfActiveFreq4DialPos < desiredPosition4)
                                {
                                    var str = UhfFreq4DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _uhfDial4WaitingForFeedback, 1);
                                }
                                else if (_uhfActiveFreq4DialPos > desiredPosition4)
                                {
                                    var str = UhfFreq4DialCommand + "DEC\n";
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
                                if (_uhfActiveFreq5DialPos != desiredPosition5)
                                {
                                    dial5OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_uhfActiveFreq5DialPos is " + _uhfActiveFreq5DialPos + " and should be " + position5);
                                if (_uhfActiveFreq5DialPos < desiredPosition5)
                                {
                                    var str = UhfFreq5DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial5SendCount++;
                                    Interlocked.Exchange(ref _uhfDial5WaitingForFeedback, 1);
                                }
                                else if (_uhfActiveFreq5DialPos > desiredPosition5)
                                {
                                    var str = UhfFreq5DialCommand + "DEC\n";
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
                }
                finally
                {
                    Interlocked.Exchange(ref _uhfThreadNowSynching, 0);
                }
                SwapActiveStandbyFrequencyUhf();
                ShowFrequenciesOnPanel();
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(56453, ex);
            }
        }

        private void SendVhfFmToDCSBIOS()
        {
            if (VhfFmNowSyncing())
            {
                return;
            }
            SaveActiveFrequencyVhfFm();
            var frequencyAsString = (_vhfFmBigFrequencyStandby + "." + _vhfFmSmallFrequencyStandby.ToString().PadLeft(3,'0'));
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

            if (_vhfFmSyncThread != null)
            {
                _vhfFmSyncThread.Abort();
            }
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
                        //Common.DebugP("dial1Timeout is " + (Environment.TickCount - dial1Timeout));
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF FM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 1");
                        }
                        //Common.DebugP("dial2Timeout is " + (Environment.TickCount - dial2Timeout));
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF FM dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 2");
                        }
                        //Common.DebugP("dial3Timeout is " + (Environment.TickCount - dial3Timeout));
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "VHF FM dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 3");
                        }
                        //Common.DebugP("dial4Timeout is " + (Environment.TickCount - dial4Timeout));
                        if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "VHF FM dial4Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 4");
                        }
                        if (Interlocked.Read(ref _vhfFmDial1WaitingForFeedback) == 0)
                        {
                            //Common.DebugP("a");
                            lock (_lockVhfFmDialsObject1)
                            {
                                if (_vhfFmActiveFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_vhfFmActiveFreq1DialPos is " + _vhfFmActiveFreq1DialPos + " and should be " + desiredPositionDial1);
                                    var str = VhfFmFreq1DialCommand + GetCommandDirectionForVhfDial1(desiredPositionDial1, _vhfFmActiveFreq1DialPos);
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
                                if (_vhfFmActiveFreq2DialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    //Common.DebugP("_vhfFmActiveFreq2DialPos is " + _vhfFmActiveFreq2DialPos + " and should be " + desiredPositionDial2);
                                    var str = VhfFmFreq2DialCommand + GetCommandDirectionForVhfDial23(desiredPositionDial2, _vhfFmActiveFreq2DialPos);
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
                            //Common.DebugP("c");
                            lock (_lockVhfFmDialsObject3)
                            {
                                if (_vhfFmActiveFreq3DialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    //Common.DebugP("_vhfFmActiveFreq3DialPos is " + _vhfFmActiveFreq3DialPos + " and should be " + desiredPositionDial3);
                                    var str = VhfFmFreq3DialCommand + GetCommandDirectionForVhfDial23(desiredPositionDial3, _vhfFmActiveFreq3DialPos);
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
                            //Common.DebugP("d");
                            lock (_lockVhfFmDialsObject4)
                            {
                                //      "00" "25" "50" "75", only "00" and "50" used.
                                //Pos     0    1    2    3
                                if (_vhfFmActiveFreq4DialPos < frequencyDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    //Common.DebugP("_vhfFmActiveFreq4DialPos is " + _vhfFmActiveFreq4DialPos + " and should be " + position);
                                    var str = VhfFmFreq4DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 1);
                                }
                                else if (_vhfFmActiveFreq4DialPos > frequencyDial4)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                    //Common.DebugP("_vhfFmActiveFreq4DialPos is " + _vhfFmActiveFreq4DialPos + " and should be " + position);
                                    var str = VhfFmFreq4DialCommand + "DEC\n";
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

                        //Common.DebugP(dial1Ok + " " + dial2Ok + " " + dial3Ok + " " + dial4Ok);
                    }
                    while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime));
                }
                finally
                {
                    Interlocked.Exchange(ref _vhfFmThreadNowSynching, 0);
                }
                SwapActiveStandbyFrequencyVhfFm();
                ShowFrequenciesOnPanel();
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(56463, ex);
            }
        }

        private void SendILSToDCSBIOS()
        {
            if (IlsNowSyncing())
            {
                return;
            }
            SaveActiveFrequencyIls();
            var frequency = Double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay);
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

            if (_ilsSyncThread != null)
            {
                _ilsSyncThread.Abort();
            }
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
                                //Common.DebugP("_ilsMhzActiveFreq1DialPos is " + _ilsMhzActiveFreq1DialPos + " and should be " + position1);
                                if (_ilsActiveFreq1DialPos < position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = ILSFreq1DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _ilsDial1WaitingForFeedback, 1);
                                }
                                else if (_ilsActiveFreq1DialPos > position1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    var str = ILSFreq1DialCommand + "DEC\n";
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
                                //Common.DebugP("_ilsKhzActiveFreq2DialPos is " + _ilsKhzActiveFreq2DialPos + " and should be " + position2);
                                if (_ilsActiveFreq2DialPos < position2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    var str = ILSFreq2DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _ilsDial2WaitingForFeedback, 1);
                                }
                                else if (_ilsActiveFreq2DialPos > position2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    var str = ILSFreq2DialCommand + "DEC\n";
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
                }
                finally
                {
                    Interlocked.Exchange(ref _ilsThreadNowSynching, 0);
                }
                SwapActiveStandbyFrequencyIls();
                ShowFrequenciesOnPanel();
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(56473, ex);
            }
        }

        private void SendTacanToDCSBIOS()
        {
            if (TacanNowSyncing())
            {
                return;
            }
            SaveActiveFrequencyTacan();
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

            if (_tacanSyncThread != null)
            {
                _tacanSyncThread.Abort();
            }
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

                    var inc = "INC\n";
                    var dec = "DEC\n";
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
                        //Common.DebugP("dial1Timeout is " + (Environment.TickCount - dial1Timeout));
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "TACAN dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 1");
                        }
                        //Common.DebugP("dial2Timeout is " + (Environment.TickCount - dial2Timeout));
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "TACAN dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 2");
                        }
                        //Common.DebugP("dial3Timeout is " + (Environment.TickCount - dial3Timeout));
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "TACAN dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _tacanDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for TACAN 3");
                        }

                        if (Interlocked.Read(ref _tacanDial1WaitingForFeedback) == 0)
                        {
                            //Common.DebugP("a");
                            lock (_lockTacanDialsObject1)
                            {
                                if (_tacanActiveFreq1DialPos != desiredPositionDial1)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    Common.DebugP("_tacanActiveFreq1DialPos is " + _tacanActiveFreq1DialPos + " and should be " + desiredPositionDial1);
                                    var str = TacanFreq1DialCommand + (_tacanActiveFreq1DialPos < desiredPositionDial1 ? inc : dec);
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
                                if (_tacanActiveFreq2DialPos != desiredPositionDial2)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    //Common.DebugP("_tacanActiveFreq2DialPos is " + _tacanActiveFreq2DialPos + " and should be " + desiredPositionDial2);
                                    var str = TacanFreq2DialCommand + (_tacanActiveFreq2DialPos < desiredPositionDial2 ? inc : dec);
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
                            //Common.DebugP("c");
                            lock (_lockTacanDialsObject3)
                            {
                                if (_tacanActiveFreq3DialPos != desiredPositionDial3)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    //Common.DebugP("_tacanActiveFreq3DialPos is " + _tacanActiveFreq3DialPos + " and should be " + desiredPositionDial3);
                                    var str = TacanFreq3DialCommand + (_tacanActiveFreq3DialPos < desiredPositionDial3 ? inc : dec);
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

                        //Common.DebugP(dial1Ok + " " + dial2Ok + " " + dial3Ok + " " + dial4Ok);
                    }
                    while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime));
                }
                finally
                {
                    Interlocked.Exchange(ref _tacanThreadNowSynching, 0);
                }
                SwapActiveStandbyFrequencyTacan();
                ShowFrequenciesOnPanel();
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(56873, ex);
            }
        }

        private void ShowFrequenciesOnPanel()
        {
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
                            frequencyAsString = GetVhfAmDialFrequencyForPosition(1, _vhfAmActiveFreq1DialPos);
                        }
                        lock (_lockVhfAmDialsObject2)
                        {
                            frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(2, _vhfAmActiveFreq2DialPos);
                        }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockVhfAmDialsObject3)
                        {
                            frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(3, _vhfAmActiveFreq3DialPos);
                        }
                        lock (_lockVhfAmDialsObject4)
                        {
                            frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(4, _vhfAmActiveFreq4DialPos);
                        }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby, PZ69LCDPosition.UPPER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.UHF:
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
                            frequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfActiveFreq1DialPos);
                        }
                        lock (_lockUhfDialsObject2)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(2, _uhfActiveFreq2DialPos);
                        } lock (_lockUhfDialsObject3)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(3, _uhfActiveFreq3DialPos);
                        }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockUhfDialsObject4)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(4, _uhfActiveFreq4DialPos);
                        } lock (_lockUhfDialsObject5)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(5, _uhfActiveFreq5DialPos);
                        }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby, PZ69LCDPosition.UPPER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.VHFFM:
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
                        var dial1 = "";
                        var dial2 = "";
                        var dial3 = "";
                        var dial4 = "";
                        lock (_lockVhfFmDialsObject1)
                        {
                            dial1 = GetVhfFmDialFrequencyForPosition(1, _vhfFmActiveFreq1DialPos);
                        }
                        lock (_lockVhfFmDialsObject2)
                        {
                            dial2 = GetVhfFmDialFrequencyForPosition(2, _vhfFmActiveFreq2DialPos);
                        }
                        lock (_lockVhfFmDialsObject3)
                        {
                            dial3 = GetVhfFmDialFrequencyForPosition(3, _vhfFmActiveFreq3DialPos);
                        }
                        lock (_lockVhfFmDialsObject4)
                        {
                            dial4 = GetVhfFmDialFrequencyForPosition(4, _vhfFmActiveFreq4DialPos);
                        }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(dial1 + dial2 + "." + dial3 + dial4, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(_vhfFmBigFrequencyStandby + "." + _vhfFmSmallFrequencyStandby.ToString().PadLeft(3,'0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.ILS:
                    {
                        //Mhz   "108" "109" "110" "111"
                        //Khz   "10" "15" "30" "35" "50" "55" "70" "75" "90" "95"
                        var frequencyAsString = "";
                        lock (_lockIlsDialsObject1) { frequencyAsString = GetILSDialFrequencyForPosition(1, _ilsActiveFreq1DialPos); }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockIlsDialsObject2) { frequencyAsString = frequencyAsString + GetILSDialFrequencyForPosition(2, _ilsActiveFreq2DialPos); }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
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
                                frequencyAsString = _tacanActiveFreq1DialPos.ToString() + _tacanActiveFreq2DialPos.ToString();
                            }
                        }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockTacanDialsObject3)
                        {
                            frequencyAsString = frequencyAsString + _tacanActiveFreq3DialPos.ToString();
                        }

                        SetPZ69DisplayBytes(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_LEFT);
                        SetPZ69DisplayBytes(ref bytes, Double.Parse(_tacanBigFrequencyStandby.ToString() + _tacanSmallFrequencyStandby.ToString() + "." + _tacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.UPPER_RIGHT);
                        break;
                    }
            }
            switch (_currentLowerRadioMode)
            {
                case CurrentA10RadioMode.VHFAM:
                    {
                        var frequencyAsString = "";
                        lock (_lockVhfAmDialsObject1)
                        {
                            frequencyAsString = GetVhfAmDialFrequencyForPosition(1, _vhfAmActiveFreq1DialPos);
                        }
                        lock (_lockVhfAmDialsObject2)
                        {
                            frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(2, _vhfAmActiveFreq2DialPos);
                        }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockVhfAmDialsObject3)
                        {
                            frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(3, _vhfAmActiveFreq3DialPos);
                        }
                        lock (_lockVhfAmDialsObject4)
                        {
                            frequencyAsString = frequencyAsString + GetVhfAmDialFrequencyForPosition(4, _vhfAmActiveFreq4DialPos);
                        }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby, PZ69LCDPosition.LOWER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.UHF:
                    {
                        var frequencyAsString = "";
                        lock (_lockUhfDialsObject1)
                        {
                            frequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfActiveFreq1DialPos);
                        }
                        lock (_lockUhfDialsObject2)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(2, _uhfActiveFreq2DialPos);
                        } lock (_lockUhfDialsObject3)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(3, _uhfActiveFreq3DialPos);
                        }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockUhfDialsObject4)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(4, _uhfActiveFreq4DialPos);
                        } lock (_lockUhfDialsObject5)
                        {

                            frequencyAsString = frequencyAsString + GetUhfDialFrequencyForPosition(5, _uhfActiveFreq5DialPos);
                        }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, _uhfBigFrequencyStandby + _uhfSmallFrequencyStandby, PZ69LCDPosition.LOWER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.VHFFM:
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
                        var dial1 = "";
                        var dial2 = "";
                        var dial3 = "";
                        var dial4 = "";
                        lock (_lockVhfFmDialsObject1)
                        {
                            dial1 = GetVhfFmDialFrequencyForPosition(1, _vhfFmActiveFreq1DialPos);
                        }
                        lock (_lockVhfFmDialsObject2)
                        {
                            dial2 = GetVhfFmDialFrequencyForPosition(2, _vhfFmActiveFreq2DialPos);
                        }
                        lock (_lockVhfFmDialsObject3)
                        {
                            dial3 = GetVhfFmDialFrequencyForPosition(3, _vhfFmActiveFreq3DialPos);
                        }
                        lock (_lockVhfFmDialsObject4)
                        {
                            dial4 = GetVhfFmDialFrequencyForPosition(4, _vhfFmActiveFreq4DialPos);
                        }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(dial1 + dial2 + "." + dial3 + dial4, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(_vhfFmBigFrequencyStandby + "." + _vhfFmSmallFrequencyStandby.ToString().PadLeft(3, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.ILS:
                    {
                        var frequencyAsString = "";
                        lock (_lockIlsDialsObject1) { frequencyAsString = GetILSDialFrequencyForPosition(1, _ilsActiveFreq1DialPos); }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockIlsDialsObject2) { frequencyAsString = frequencyAsString + GetILSDialFrequencyForPosition(2, _ilsActiveFreq2DialPos); }
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_LEFT);
                        SetPZ69DisplayBytesDefault(ref bytes, Double.Parse(_ilsBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay) + "." + _ilsSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_RIGHT);
                        break;
                    }
                case CurrentA10RadioMode.TACAN:
                    {
                        var frequencyAsString = "";
                        lock (_lockTacanDialsObject1)
                        {
                            lock (_lockTacanDialsObject2)
                            {
                                frequencyAsString = _tacanActiveFreq1DialPos.ToString() + _tacanActiveFreq2DialPos.ToString();
                            }
                        }
                        frequencyAsString = frequencyAsString + ".";
                        lock (_lockTacanDialsObject3)
                        {
                            frequencyAsString = frequencyAsString + _tacanActiveFreq3DialPos.ToString();
                        }

                        SetPZ69DisplayBytes(ref bytes, Double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_LEFT);
                        SetPZ69DisplayBytes(ref bytes, Double.Parse(_tacanBigFrequencyStandby.ToString() + _tacanSmallFrequencyStandby.ToString() + "." + _tacanXYStandby.ToString(), NumberFormatInfoFullDisplay), 1, PZ69LCDPosition.LOWER_RIGHT);
                        break;
                    }
            }
            SendLCDData(bytes);
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
                                            if (_vhfAmBigFrequencyStandby.Equals(151.00))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfAmBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            //225-399
                                            if (_uhfBigFrequencyStandby.Equals(399.00))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmBigFrequencyStandby.Equals(76))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby++;
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
                                            if (_vhfAmBigFrequencyStandby.Equals(116.00))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfAmBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_uhfBigFrequencyStandby.Equals(225.00))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmBigFrequencyStandby.Equals(30))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby--;
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
                                            if (_vhfAmSmallFrequencyStandby >= 0.95)
                                            {
                                                //At max value
                                                _vhfAmSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby + 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            //Small dial 0.000 0.025 0.050 0.075 [only 0.00 and 0.05 are used]
                                            if (_uhfSmallFrequencyStandby >= 0.95)
                                            {
                                                //At max value
                                                _uhfSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby + 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby >= 975)
                                            {
                                                //At max value
                                                _vhfFmSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby = _vhfFmSmallFrequencyStandby + 25;
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
                                            if (_vhfAmSmallFrequencyStandby <= 0.00)
                                            {
                                                //At min value
                                                _vhfAmSmallFrequencyStandby = 0.95;
                                                break;
                                            }
                                            _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby - 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_uhfSmallFrequencyStandby <= 0.00)
                                            {
                                                //At min value
                                                _uhfSmallFrequencyStandby = 0.95;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby - 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfFmSmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby = _vhfFmSmallFrequencyStandby - 25;
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
                                            if (_vhfAmBigFrequencyStandby.Equals(151.00))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfAmBigFrequencyStandby = _vhfAmBigFrequencyStandby + 1;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            //225-399
                                            if (_uhfBigFrequencyStandby.Equals(399.00))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmBigFrequencyStandby.Equals(76))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby = _vhfFmBigFrequencyStandby + 1;
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
                                            if (_vhfAmBigFrequencyStandby.Equals(116.00))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfAmBigFrequencyStandby = _vhfAmBigFrequencyStandby - 1;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_uhfBigFrequencyStandby.Equals(225.00))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmBigFrequencyStandby.Equals(30))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby = _vhfFmBigFrequencyStandby - 1;
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
                                            if (_vhfAmSmallFrequencyStandby >= 0.95)
                                            {
                                                //At max value
                                                _vhfAmSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby + 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            //Small dial 0.000 0.025 0.050 0.075 [only 0.00 and 0.05 are used]
                                            if (_uhfSmallFrequencyStandby >= 0.95)
                                            {
                                                //At max value
                                                _uhfSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby + 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby >= 975)
                                            {
                                                //At max value
                                                _vhfFmSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby = _vhfFmSmallFrequencyStandby + 25;
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
                                            if (_vhfAmSmallFrequencyStandby <= 0.00)
                                            {
                                                //At min value
                                                _vhfAmSmallFrequencyStandby = 0.95;
                                                break;
                                            }
                                            _vhfAmSmallFrequencyStandby = _vhfAmSmallFrequencyStandby - 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.UHF:
                                        {
                                            if (_uhfSmallFrequencyStandby <= 0.00)
                                            {
                                                //At min value
                                                _uhfSmallFrequencyStandby = 0.95;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby - 0.05;
                                            break;
                                        }
                                    case CurrentA10RadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfFmSmallFrequencyStandby = 975;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby = _vhfFmSmallFrequencyStandby - 25;
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
            lock (_lockLCDUpdateObject)
            {
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
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH);
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH);
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
                _vhfAmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ1");
                //_vhfAmDcsbiosOutputFreqDial1.Debug = true;
                _vhfAmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ2");
                //_vhfAmDcsbiosOutputFreqDial2.Debug = true;
                _vhfAmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ3");
                //_vhfAmDcsbiosOutputFreqDial3.Debug = true;
                _vhfAmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFAM_FREQ4");
                //_vhfAmDcsbiosOutputFreqDial4.Debug = true;

                //UHF
                _uhfDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_100MHZ_SEL");
                _uhfDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_10MHZ_SEL");
                _uhfDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT1MHZ_SEL");
                _uhfDcsbiosOutputFreqDial5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_POINT25_SEL");

                //VHF FM
                _vhfFmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ1");
                //_vhfFmDcsbiosOutputFreqDial1.Debug = true;
                _vhfFmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ2");
                //_vhfFmDcsbiosOutputFreqDial2.Debug = true;
                _vhfFmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ3");
                //_vhfFmDcsbiosOutputFreqDial3.Debug = true;
                _vhfFmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ4");
                //_vhfFmDcsbiosOutputFreqDial4.Debug = true;

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
                            //Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(RadioPanelPZ69SO._newRadioPanelValue, (RadioPanelKnobA10C)radioPanelKnob));
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
            //Common.DebugP("Old: " + Convert.ToString(oldValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[2], 2).PadLeft(8, '0'));
            //Common.DebugP("New: " + Convert.ToString(newValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[2], 2).PadLeft(8, '0'));
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
                        //Common.DebugP("Following knob has changed : " + radioPanelKnob.RadioPanelPZ69Knob + " isOn? : " + radioPanelKnob.IsOn);
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
                            //      "00" "25" "50" "75", only "00" and "50" used.
                            //Pos     0    1    2    3
                            case 0:
                                {
                                    return "0";
                                }
                            case 1:
                                {
                                    return "0";
                                }
                            case 2:
                                {
                                    return "5";
                                }
                            case 3:
                                {
                                    return "5";
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
                                    return "0";
                                }
                            case 2:
                                {
                                    return "5";
                                }
                            case 3:
                                {
                                    return "5";
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

        private string GetCommandDirectionForVhfDial1(int desiredDialPosition, uint actualDialPosition)
        {
            var inc = "INC\n";
            var dec = "DEC\n";
            switch (desiredDialPosition)
            {
                case 0:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    //-6
                                    return dec;
                                }
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    //5
                                    return inc;
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
                                    return inc;
                                }
                            case 1:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return dec;
                                }
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 2:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return dec;
                                }
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 3:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return dec;
                                }
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 4:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                {
                                    return dec;
                                }
                            case 11:
                            case 12:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 5:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                                {
                                    return dec;
                                }
                            case 12:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 6:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    return inc;
                                }
                            case 7:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return inc;
                                }
                            case 8:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return inc;
                                }
                            case 9:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 10:
                            case 11:
                            case 12:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return inc;
                                }
                            case 10:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 11:
                            case 12:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                {
                                    return inc;
                                }
                            case 11:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 12:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                                {
                                    return inc;
                                }
                            case 12:
                                {
                                    //Do nothing
                                    return null;
                                }
                        }
                        break;
                    }
            }
            throw new Exception("Should reach this code. private String GetCommandDirectionForVhfDial1(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        private string GetCommandDirectionForVhfDial23(int desiredDialPosition, uint actualDialPosition)
        {
            var inc = "INC\n";
            var dec = "DEC\n";
            switch (desiredDialPosition)
            {
                case 0:
                    {
                        switch (actualDialPosition)
                        {
                            case 0:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                {
                                    //-4 DEC
                                    return dec;
                                }
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    //5 INC
                                    return inc;
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
                                    return inc;
                                }
                            case 1:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                {
                                    return dec;
                                }
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 2:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    return dec;
                                }
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 3:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return dec;
                                }
                            case 8:
                            case 9:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 4:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return dec;
                                }
                            case 9:
                                {
                                    return inc;
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
                                    return inc;
                                }
                            case 5:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                {
                                    return inc;
                                }
                            case 6:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 7:
                            case 8:
                            case 9:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                {
                                    return inc;
                                }
                            case 7:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 8:
                            case 9:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                {
                                    return inc;
                                }
                            case 8:
                                {
                                    //Do nothing
                                    return null;
                                }
                            case 9:
                                {
                                    return dec;
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
                                    return dec;
                                }
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                {
                                    return inc;
                                }
                            case 9:
                                {
                                    //Do nothing
                                    return null;
                                }
                        }
                        break;
                    }
            }
            throw new Exception("Should reach this code. private String GetCommandDirectionForVhfDial23(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        private void SaveActiveFrequencyVhfAm()
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
            lock (_lockVhfAmDialsObject1)
            {
                lock (_lockVhfAmDialsObject2)
                {
                    lock (_lockVhfAmDialsObject3)
                    {
                        lock (_lockVhfAmDialsObject4)
                        {
                            uint dial4 = 0;
                            //Common.DebugP("******A _vhfAmActiveFreq4DialPos : " + _vhfAmActiveFreq4DialPos);
                            switch (_vhfAmActiveFreq4DialPos)
                            {
                                case 0:
                                case 1:
                                    {
                                        //00 & 25
                                        dial4 = 0;
                                        break;
                                    }
                                case 2:
                                case 3:
                                    {
                                        //50 & 75
                                        dial4 = 50;
                                        break;
                                    }
                            }
                            _vhfAmSavedActiveBigFrequency = Double.Parse((_vhfAmActiveFreq1DialPos + 3).ToString() + _vhfAmActiveFreq2DialPos.ToString(), NumberFormatInfoFullDisplay);
                            _vhfAmSavedActiveSmallFrequency = Double.Parse("0." + _vhfAmActiveFreq3DialPos.ToString() + dial4, NumberFormatInfoFullDisplay);
                        }
                    }
                }
            }
        }

        private void SwapActiveStandbyFrequencyVhfAm()
        {
            _vhfAmBigFrequencyStandby = _vhfAmSavedActiveBigFrequency;
            _vhfAmSmallFrequencyStandby = _vhfAmSavedActiveSmallFrequency;
        }

        private void SaveActiveFrequencyUhf()
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
                    bigFrequencyAsString = GetUhfDialFrequencyForPosition(1, _uhfActiveFreq1DialPos);
                    //Common.DebugP("******A bigFrequencyAsString : " + bigFrequencyAsString + " _uhfActiveFreq1DialPos = " + _uhfActiveFreq1DialPos);
                }
                lock (_lockUhfDialsObject2)
                {
                    bigFrequencyAsString = bigFrequencyAsString + GetUhfDialFrequencyForPosition(2, _uhfActiveFreq2DialPos);
                    //Common.DebugP("******A bigFrequencyAsString : " + bigFrequencyAsString + " _uhfActiveFreq2DialPos = " + _uhfActiveFreq2DialPos);
                }
                lock (_lockUhfDialsObject3)
                {
                    bigFrequencyAsString = bigFrequencyAsString + GetUhfDialFrequencyForPosition(3, _uhfActiveFreq3DialPos);
                    //Common.DebugP("******A bigFrequencyAsString : " + bigFrequencyAsString + " _uhfActiveFreq3DialPos = " + _uhfActiveFreq3DialPos);
                }
                lock (_lockUhfDialsObject4)
                {
                    smallFrequencyAsString = smallFrequencyAsString + GetUhfDialFrequencyForPosition(4, _uhfActiveFreq4DialPos);
                }
                lock (_lockUhfDialsObject5)
                {
                    smallFrequencyAsString = smallFrequencyAsString + GetUhfDialFrequencyForPosition(5, _uhfActiveFreq5DialPos);
                }


                _uhfSavedActiveBigFrequency = Double.Parse(bigFrequencyAsString, NumberFormatInfoFullDisplay);
                _uhfSavedActiveSmallFrequency = Double.Parse(smallFrequencyAsString, NumberFormatInfoFullDisplay);

                //Common.DebugP("bigFrequencyAsString : " + bigFrequencyAsString);
                //Common.DebugP("_uhfSavedActiveBigFrequency : " + _uhfSavedActiveBigFrequency.ToString(NumberFormatInfoFullDisplay));
                //Common.DebugP("_uhfSavedActiveSmallFrequency : " + _uhfSavedActiveSmallFrequency.ToString(NumberFormatInfoFullDisplay));
            }
            catch (Exception ex)
            {
                Common.LogError(83244, ex, "SaveActiveFrequencyUhf()");
                throw;
            }
        }

        private void SwapActiveStandbyFrequencyUhf()
        {
            _uhfBigFrequencyStandby = _uhfSavedActiveBigFrequency;
            _uhfSmallFrequencyStandby = _uhfSavedActiveSmallFrequency;
            //Common.DebugP("_uhfBigFrequencyStandby : " + _uhfBigFrequencyStandby.ToString(NumberFormatInfoFullDisplay));
            //Common.DebugP("_uhfSmallFrequencyStandby : " + _uhfSmallFrequencyStandby.ToString(NumberFormatInfoFullDisplay));
        }

        private void SaveActiveFrequencyVhfFm()
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
                            //Common.DebugP("******A _vhfAmActiveFreq4DialPos : " + _vhfAmActiveFreq4DialPos);
                            switch (_vhfFmActiveFreq4DialPos)
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
                            _vhfFmSavedActiveBigFrequency = uint.Parse((_vhfFmActiveFreq1DialPos + 3).ToString() + _vhfFmActiveFreq2DialPos.ToString(), NumberFormatInfoFullDisplay);
                            _vhfFmSavedActiveSmallFrequency = uint.Parse((_vhfFmActiveFreq3DialPos.ToString() + dial4).PadLeft(3,'0'), NumberFormatInfoFullDisplay);
                        }
                    }
                }
            }
        }

        private void SwapActiveStandbyFrequencyVhfFm()
        {
            _vhfFmBigFrequencyStandby = _vhfFmSavedActiveBigFrequency;
            _vhfFmSmallFrequencyStandby = _vhfFmSavedActiveSmallFrequency;
        }

        private void SaveActiveFrequencyIls()
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
                    _ilsSavedActiveBigFrequency = uint.Parse(GetILSDialFrequencyForPosition(1, _ilsActiveFreq1DialPos).ToString());
                    _ilsSavedActiveSmallFrequency = uint.Parse(GetILSDialFrequencyForPosition(2, _ilsActiveFreq2DialPos).ToString());
                }
            }
        }

        private void SwapActiveStandbyFrequencyIls()
        {
            _ilsBigFrequencyStandby = _ilsSavedActiveBigFrequency;
            _ilsSmallFrequencyStandby = _ilsSavedActiveSmallFrequency;
        }

        private void SaveActiveFrequencyTacan()
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
                        _tacanSavedActiveBigFrequency = Convert.ToInt32(_tacanActiveFreq1DialPos);
                        _tacanSavedActiveSmallFrequency = Convert.ToInt32(_tacanActiveFreq2DialPos);
                        _tacanSavedActiveXY = Convert.ToInt32(_tacanActiveFreq3DialPos);
                        //Common.DebugP("_tacanSavedActiveBigFrequency : " + _tacanSavedActiveBigFrequency);
                        //Common.DebugP("_tacanSavedActiveSmallFrequency : " + _tacanSavedActiveSmallFrequency);
                        //Common.DebugP("_tacanSavedActiveXY : " + _tacanSavedActiveXY);
                    }
                }
            }
        }

        private void SwapActiveStandbyFrequencyTacan()
        {
            _tacanBigFrequencyStandby = _tacanSavedActiveBigFrequency;
            _tacanSmallFrequencyStandby = _tacanSavedActiveSmallFrequency;
            _tacanXYStandby = _tacanSavedActiveXY;
            //Common.DebugP("_tacanBigFrequencyStandby : " + _tacanBigFrequencyStandby);
            //Common.DebugP("_tacanSmallFrequencyStandby : " + _tacanSmallFrequencyStandby);
            //Common.DebugP("_tacanXYStandby : " + _tacanXYStandby);
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
    }

}
