using System;
using System.Collections.Generic;
using DCS_BIOS;
using HidLibrary;
using System.Threading;

namespace NonVisuals
{
    public class RadioPanelPZ69MiG21Bis : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private HashSet<RadioPanelKnobMiG21Bis> _radioPanelKnobs = new HashSet<RadioPanelKnobMiG21Bis>();
        private CurrentMiG21BisRadioMode _currentUpperRadioMode = CurrentMiG21BisRadioMode.Radio;
        private CurrentMiG21BisRadioMode _currentLowerRadioMode = CurrentMiG21BisRadioMode.Radio;

        /*MiG-21bis Radio*/
        //Large dial Freq selector 0-19  RAD_CHAN
        //Small dial Radio volume RAD_VOL +/- 
        //STBY/ACT, radio on/off RAD_PWR TOGGLE
        private volatile uint _radioFreqSelectorPositionCockpit;
        private readonly object _lockRadioFreqSelectorPositionObject = new object();
        private DCSBIOSOutput _radioDcsbiosOutputFreqSelectorPosition;
        private const string RadioFreqSelectorPositionCommandInc = "RAD_CHAN INC\n";
        private const string RadioFreqSelectorPositionCommandDec = "RAD_CHAN DEC\n";
        private const string RadioVolumeCommandInc = "RAD_VOL +3200\n";
        private const string RadioVolumeCommandDec = "RAD_VOL -3200\n";
        private const string RadioOnOffToggleCommand = "RAD_PWR TOGGLE\n";

        /*MiG-21bis RSBN*/
        //Large dial RSBN Nav RSBN_CHAN
        //Small dial RSBN ILS PRMG_CHAN
        //STBY/ACT, RSBN/ARC switch  RSBN_ARC_SEL
        private volatile uint _rsbnNavChannelCockpit = 1;
        private readonly object _lockRSBNNavChannelObject = new object();
        private DCSBIOSOutput _rsbnNavChannelCockpitOutput;
        private volatile uint _rsbnILSChannelCockpit = 1;
        private readonly object _lockRSBNILSChannelObject = new object();
        private DCSBIOSOutput _rsbnILSChannelCockpitOutput;
        private const string RSBNNavChannelCommandInc = "RSBN_CHAN INC\n";
        private const string RSBNNavChannelCommandDec = "RSBN_CHAN DEC\n";
        private const string RSBNIlsChannelCommandInc = "PRMG_CHAN INC\n";
        private const string RSBNIlsChannelCommandDec = "PRMG_CHAN DEC\n";
        private const string SelectRSBNCommand = "RSBN_ARC_SEL INC\n";

        /*MiG-21bis ARC*/
        //Large dial ARC Sector ARC_ZONE
        //Small dial ARC Preset ARC_CHAN
        //STBY/ACT, RSBN/ARC switch  RSBN_ARC_SEL 1
        private volatile uint _arcSectorCockpit = 1;
        private readonly object _lockARCSectorObject = new object();
        private DCSBIOSOutput _arcSectorCockpitOutput;
        private volatile uint _arcPresetChannelCockpit = 1;
        private readonly object _lockARCPresetChannelObject = new object();
        private DCSBIOSOutput _arcPresetChannelCockpitOutput;
        private const string ARCSectorCommandInc = "ARC_ZONE INC\n";
        private const string ARCSectorCommandDec = "ARC_ZONE DEC\n";
        private const string ARCPresetChannelCommandInc = "ARC_CHAN INC\n";
        private const string ARCPresetChannelCommandDec = "ARC_CHAN DEC\n";
        private const string SelectARCCommand = "RSBN_ARC_SEL DEC\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69MiG21Bis(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        ~RadioPanelPZ69MiG21Bis()
        {
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //Common.DebugP("PZ69 MiG21 READ ENTERING");
            UpdateCounter(address, data);
            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */

            //Radio
            if (address == _radioDcsbiosOutputFreqSelectorPosition.Address)
            {
                //Common.DebugP("Radio freq pos arrived, waiting for lock." + Environment.TickCount);
                lock (_lockRadioFreqSelectorPositionObject)
                {
                    var tmp = _radioFreqSelectorPositionCockpit;
                    if (tmp != _radioFreqSelectorPositionCockpit)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        _radioFreqSelectorPositionCockpit = _radioDcsbiosOutputFreqSelectorPosition.GetUIntValue(data);
                        //Common.DebugP("Just read Radio freq. sel. pos.: " + _radioFreqSelectorPositionCockpit + "  " + Environment.TickCount);
                    }
                }
            }

            //RSBN Nav
            if (address == _rsbnNavChannelCockpitOutput.Address)
            {
                //Common.DebugP("RSBN Nav channel arrived, waiting for lock." + Environment.TickCount);
                lock (_lockRSBNNavChannelObject)
                {
                    var tmp = _rsbnNavChannelCockpit;
                    //Common.DebugP("Just read RSBN Nav channel : " + _rsbnNavChannelCockpit + "  " + Environment.TickCount);
                    _rsbnNavChannelCockpit = _rsbnNavChannelCockpitOutput.GetUIntValue(data);
                    if (tmp != _rsbnNavChannelCockpit)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }

            //RSBN ILS
            if (address == _rsbnILSChannelCockpitOutput.Address)
            {
                //Common.DebugP("RSBN ILS channel arrived, waiting for lock." + Environment.TickCount);
                lock (_lockRSBNILSChannelObject)
                {
                    var tmp = _rsbnILSChannelCockpit;
                    //Common.DebugP("Just read RSBN Nav channel : " + _rsbnILSChannelCockpit + "  " + Environment.TickCount);
                    _rsbnILSChannelCockpit = _rsbnILSChannelCockpitOutput.GetUIntValue(data);
                    if (tmp != _rsbnILSChannelCockpit)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }

            //ARC Sector
            if (address == _arcSectorCockpitOutput.Address)
            {
                //Common.DebugP("ARC Sector arrived, waiting for lock." + Environment.TickCount);
                lock (_lockARCSectorObject)
                {
                    var tmp = _arcSectorCockpit;
                    //Common.DebugP("Just read ARC Sector : " + _arcSectorCockpit + "  " + Environment.TickCount);
                    _arcSectorCockpit = _arcSectorCockpitOutput.GetUIntValue(data);
                    if (tmp != _arcSectorCockpit)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }

            //ARC Preset
            if (address == _arcPresetChannelCockpitOutput.Address)
            {
                //Common.DebugP("ARC Preset Channel, waiting for lock." + Environment.TickCount);
                lock (_lockARCPresetChannelObject)
                {
                    var tmp = _arcPresetChannelCockpit;
                    //Common.DebugP("Just read ARC Preset Channel : " + _arcPresetChannelCockpit + "  " + Environment.TickCount);
                    _arcPresetChannelCockpit = _arcPresetChannelCockpitOutput.GetUIntValue(data) + 1;
                    if (tmp != _arcPresetChannelCockpit)
                    {
                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    }
                }
            }

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
            //Common.DebugP("PZ69 MiG21 READ EXITING");
        }

        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {
                //nada
            }
            catch (Exception e)
            {
                Common.LogError(349998, e, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsMiG21Bis knob)
        {
            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }
            switch (knob)
            {
                case RadioPanelPZ69KnobsMiG21Bis.UpperFreqSwitch:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentMiG21BisRadioMode.Radio:
                                {
                                    DCSBIOS.Send(RadioOnOffToggleCommand);
                                    break;
                                }
                            case CurrentMiG21BisRadioMode.RSBN:
                                {
                                    DCSBIOS.Send(SelectRSBNCommand);
                                    break;
                                }
                            case CurrentMiG21BisRadioMode.ARC:
                                {
                                    DCSBIOS.Send(SelectARCCommand);
                                    break;
                                }
                        }
                        break;
                    }
                case RadioPanelPZ69KnobsMiG21Bis.LowerFreqSwitch:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentMiG21BisRadioMode.Radio:
                                {
                                    DCSBIOS.Send(RadioOnOffToggleCommand);
                                    break;
                                }
                            case CurrentMiG21BisRadioMode.RSBN:
                                {
                                    DCSBIOS.Send(SelectRSBNCommand);
                                    break;
                                }
                            case CurrentMiG21BisRadioMode.ARC:
                                {
                                    DCSBIOS.Send(SelectARCCommand);
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void ShowFrequenciesOnPanel()
        {
            lock (_lockShowFrequenciesOnPanelObject)
            {
                if (!FirstReportHasBeenRead)
                {
                    return;
                }
                if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                {
                    return;
                }
                var bytes = new byte[21];
                bytes[0] = 0x0;

                switch (_currentUpperRadioMode)
                {
                    case CurrentMiG21BisRadioMode.Radio:
                        {
                            lock (_lockRadioFreqSelectorPositionObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _radioFreqSelectorPositionCockpit, PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_RIGHT);
                            }
                            break;
                        }
                    case CurrentMiG21BisRadioMode.RSBN:
                        {
                            lock (_lockRSBNNavChannelObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _rsbnNavChannelCockpit, PZ69LCDPosition.UPPER_LEFT);
                            }
                            lock (_lockRSBNILSChannelObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _rsbnILSChannelCockpit, PZ69LCDPosition.UPPER_RIGHT);
                            }
                            break;
                        }
                    case CurrentMiG21BisRadioMode.ARC:
                        {
                            lock (_lockARCSectorObject)
                            {
                                SetPZ69DisplayBytesCustom1(ref bytes, GetARCSectorBytesForDisplay(), PZ69LCDPosition.UPPER_LEFT);
                            }
                            lock (_lockARCPresetChannelObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _arcPresetChannelCockpit, PZ69LCDPosition.UPPER_RIGHT);
                            }
                            break;
                        }
                }
                switch (_currentLowerRadioMode)
                {
                    case CurrentMiG21BisRadioMode.Radio:
                        {
                            lock (_lockRadioFreqSelectorPositionObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _radioFreqSelectorPositionCockpit, PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_RIGHT);
                            }
                            break;
                        }
                    case CurrentMiG21BisRadioMode.RSBN:
                        {
                            lock (_lockRSBNNavChannelObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _rsbnNavChannelCockpit, PZ69LCDPosition.LOWER_LEFT);
                            }
                            lock (_lockRSBNILSChannelObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _rsbnILSChannelCockpit, PZ69LCDPosition.LOWER_RIGHT);
                            }
                            break;
                        }
                    case CurrentMiG21BisRadioMode.ARC:
                        {
                            lock (_lockARCSectorObject)
                            {
                                SetPZ69DisplayBytesCustom1(ref bytes, GetARCSectorBytesForDisplay(), PZ69LCDPosition.LOWER_LEFT);
                            }
                            lock (_lockARCPresetChannelObject)
                            {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _arcPresetChannelCockpit, PZ69LCDPosition.LOWER_RIGHT);
                            }
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
                var radioPanelKnobMiG21Bis = (RadioPanelKnobMiG21Bis)o;
                if (radioPanelKnobMiG21Bis.IsOn)
                {
                    switch (radioPanelKnobMiG21Bis.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelInc:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioFreqSelectorPositionCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNNavChannelCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCSectorCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelDec:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioFreqSelectorPositionCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNNavChannelCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCSectorCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelInc:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioVolumeCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNIlsChannelCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCPresetChannelCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelDec:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioVolumeCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNIlsChannelCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCPresetChannelCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelInc:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioFreqSelectorPositionCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNNavChannelCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCSectorCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelDec:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioFreqSelectorPositionCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {

                                            DCSBIOS.Send(RSBNNavChannelCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCSectorCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelInc:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioVolumeCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNIlsChannelCommandInc);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCPresetChannelCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelDec:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMiG21BisRadioMode.Radio:
                                        {
                                            DCSBIOS.Send(RadioVolumeCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.RSBN:
                                        {
                                            DCSBIOS.Send(RSBNIlsChannelCommandDec);
                                            break;
                                        }
                                    case CurrentMiG21BisRadioMode.ARC:
                                        {
                                            DCSBIOS.Send(ARCPresetChannelCommandDec);
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

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (_lockLCDUpdateObject)
            {
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobMiG21Bis)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsMiG21Bis.UpperRadio:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentMiG21BisRadioMode.Radio;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperRsbn:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentMiG21BisRadioMode.RSBN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperArc:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentMiG21BisRadioMode.ARC;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperCom2:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperNav2:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperDme:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperXpdr:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerRadio:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentMiG21BisRadioMode.Radio;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerRsbn:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentMiG21BisRadioMode.RSBN;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerArc:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentMiG21BisRadioMode.ARC;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerCom2:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerNav2:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerDme:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerXpdr:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelInc:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelDec:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelInc:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelDec:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelInc:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelDec:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelInc:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelDec:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.UpperFreqSwitch:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsMiG21Bis.UpperFreqSwitch);
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMiG21Bis.LowerFreqSwitch:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsMiG21Bis.LowerFreqSwitch);
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
                StartupBase("MiG21-Bis");


                //Radio
                _radioDcsbiosOutputFreqSelectorPosition = DCSBIOSControlLocator.GetDCSBIOSOutput("RAD_CHAN");

                //RSBN
                _rsbnNavChannelCockpitOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("RSBN_CHAN");
                _rsbnILSChannelCockpitOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("PRMG_CHAN");

                //ARC
                _arcSectorCockpitOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_ZONE");
                _arcPresetChannelCockpitOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_CHAN");

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69MiG21bis.StartUp() : " + ex.Message);
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
                            var knob = (RadioPanelKnobMiG21bis)radioPanelKnob;
                            //Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(RadioPanelPZ69SO._newRadioPanelValue, (RadioPanelKnobMiG21bis)radioPanelKnob));
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
            _radioPanelKnobs = RadioPanelKnobMiG21Bis.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobMiG21Bis radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private byte[] GetARCSectorBytesForDisplay()
        {
            var result = new byte[5];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = 0xff;
            }

            lock (_lockARCSectorObject)
            {
                switch (_arcSectorCockpit)
                {
                    case 0://1  1 
                        {
                            result[0] = 1;
                            result[4] = 1;
                            break;
                        }
                    case 1://1  2
                        {
                            result[0] = 1;
                            result[4] = 2;
                            break;
                        }
                    case 2://2  1
                        {
                            result[0] = 2;
                            result[4] = 1;
                            break;
                        }
                    case 3://2  2
                        {
                            result[0] = 2;
                            result[4] = 2;
                            break;
                        }
                    case 4://3  1
                        {
                            result[0] = 3;
                            result[4] = 1;
                            break;
                        }
                    case 5://3  2
                        {
                            result[0] = 3;
                            result[4] = 2;
                            break;
                        }
                    case 6://4  1
                        {
                            result[0] = 4;
                            result[4] = 1;
                            break;
                        }
                    case 7://4  2
                        {
                            result[0] = 4;
                            result[4] = 2;
                            break;
                        }
                }
            }

            return result;
        }
    }

}
