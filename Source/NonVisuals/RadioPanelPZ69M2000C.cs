using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69M2000C : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private HashSet<RadioPanelKnobM2000C> _radioPanelKnobs = new HashSet<RadioPanelKnobM2000C>();
        private CurrentM2000CRadioMode _currentUpperRadioMode = CurrentM2000CRadioMode.VUHF;
        private CurrentM2000CRadioMode _currentLowerRadioMode = CurrentM2000CRadioMode.VUHF;

        /*M-2000C V/UHF PRESETS COM1*/
        //Large dial 1-18 [step of 1]
        //Small dial Power/Mode control
        private readonly object _lockVUHFPresetDialObject = new object();
        private DCSBIOSOutput _vuhfDcsbiosOutputPresetDial;
        private volatile uint _vuhfPresetCockpitDialPos = 1;
        private const string VUHFPresetCommandInc = "UVHF_PRESET_KNOB INC\n";
        private const string VUHFPresetCommandDec = "UVHF_PRESET_KNOB DEC\n";
        private int _vuhfPresetDialSkipper;

        /*M2000C UHF PRESETS COM2*/
        //Small dial Volume Control
        private readonly object _lockUHFPresetDialObject = new object();
        private DCSBIOSOutput _uhfDcsbiosOutputPresetDial;
        private volatile uint _uhfPresetCockpitDialPos = 1;
        private const string UHFPresetCommandInc = "UHF_PRESET_KNOB INC\n";
        private const string UHFPresetCommandDec = "UHF_PRESET_KNOB DEC\n";
        private int _uhfPresetDialSkipper;

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69M2000C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                /*
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                    return;
                }*/
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
            }
            //ShowFrequenciesOnPanel();
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {

                UpdateCounter(e.Address, e.Data);
                /*
                 * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
                 * Once a dial has been deemed to be "off" position and needs to be changed
                 * a change command is sent to DCS-BIOS.
                 * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
                 * reset. Reading the dial's position with no change in value will not reset.
                 */


                // V/UHF Preset Channel Dial
                if (e.Address == _vuhfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockVUHFPresetDialObject)
                    {
                        var tmp = _vuhfPresetCockpitDialPos;
                        _vuhfPresetCockpitDialPos = _vuhfDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        _vuhfPresetCockpitDialPos++;
                        if (tmp != _vuhfPresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                // UHF Preset Channel Dial
                if (e.Address == _uhfDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockUHFPresetDialObject)
                    {
                        var tmp = _uhfPresetCockpitDialPos;
                        _uhfPresetCockpitDialPos = _uhfDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        _uhfPresetCockpitDialPos++;
                        if (tmp != _uhfPresetCockpitDialPos)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.LogError(78001, ex);
            }
        }


        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsM2000C knob)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SendFrequencyToDCSBIOS()");
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }
                switch (knob)
                {
                    case RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentM2000CRadioMode.VUHF:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.UHF:
                                    {
                                        break;
                                    }
                                case CurrentM2000CRadioMode.NOUSE:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentM2000CRadioMode.VUHF:
                                {
                                    break;
                                }
                                case CurrentM2000CRadioMode.UHF:
                                {
                                    break;
                                }
                                case CurrentM2000CRadioMode.NOUSE:
                                {
                                    break;
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78002, ex);
            }
            Common.DebugP("Leaving M2000C Radio SendFrequencyToDCSBIOS()");
        }


        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobM2000C)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsM2000C.UPPER_VUHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.VUHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.UHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsM2000C.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentM2000CRadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_VUHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.VUHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.UHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsM2000C.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentM2000CRadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH:
                                {
                                    /*if (_currentUpperRadioMode == CurrentM2000CRadioMode.)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH);
                                        }
                                    }*/
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH:
                                {
                                    /*if (_currentLowerRadioMode == CurrentM2000CRadioMode.)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH);
                                        }
                                    }*/
                                    break;
                                }
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78006, ex);
            }
            Common.DebugP("Leaving M2000C Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobM2000C = (RadioPanelKnobM2000C)o;
                    if (radioPanelKnobM2000C.IsOn)
                    {
                        switch (radioPanelKnobM2000C.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                            {
                                                if (!SkipVUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(VUHFPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentM2000CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHFPresetCommandInc);
                                                }
                                                break;
                                            }
                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                        {
                                            if (!SkipVUHFPresetDialChange())
                                            {
                                                DCSBIOS.Send(VUHFPresetCommandDec);
                                            }
                                            break;
                                        }
                                        case CurrentM2000CRadioMode.UHF:
                                        {
                                            if (!SkipUHFPresetDialChange())
                                            {
                                                DCSBIOS.Send(UHFPresetCommandDec);
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                        {
                                            if (!SkipVUHFPresetDialChange())
                                            {
                                                DCSBIOS.Send(VUHFPresetCommandInc);
                                            }
                                            break;
                                        }
                                        case CurrentM2000CRadioMode.UHF:
                                        {
                                            if (!SkipUHFPresetDialChange())
                                            {
                                                DCSBIOS.Send(UHFPresetCommandInc);
                                            }
                                            break;
                                        }
                                        case CurrentM2000CRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.VUHF:
                                        {
                                            if (!SkipVUHFPresetDialChange())
                                            {
                                                DCSBIOS.Send(VUHFPresetCommandDec);
                                            }
                                            break;
                                        }
                                        case CurrentM2000CRadioMode.UHF:
                                        {
                                            if (!SkipUHFPresetDialChange())
                                            {
                                                DCSBIOS.Send(UHFPresetCommandDec);
                                            }
                                            break;
                                        }
                                        case CurrentM2000CRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentM2000CRadioMode.NOUSE:
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
            catch (Exception ex)
            {
                Common.LogError(78007, ex);
            }
            Common.DebugP("Leaving M2000C Radio AdjustFrequency()");
        }

        private void ShowFrequenciesOnPanel()
        {
            try
            {
                lock (_lockShowFrequenciesOnPanelObject)
                {
                    if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                    {

                        return;
                    }

                    if (!FirstReportHasBeenRead)
                    {

                        return;
                    }

                    Common.DebugP("Entering M2000C Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentM2000CRadioMode.VUHF:
                            {
                                var channelAsString = "";
                                lock (_lockVUHFPresetDialObject)
                                {
                                    channelAsString = (_vuhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentM2000CRadioMode.UHF:
                            {
                                var channelAsString = "";
                                lock (_lockUHFPresetDialObject)
                                {
                                    channelAsString = (_uhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }

                        case CurrentM2000CRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentM2000CRadioMode.VUHF:
                        {
                            var channelAsString = "";
                            lock (_lockVUHFPresetDialObject)
                            {
                                channelAsString = (_vuhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                            }
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                        case CurrentM2000CRadioMode.UHF:
                        {
                            var channelAsString = "";
                            lock (_lockUHFPresetDialObject)
                            {
                                channelAsString = (_uhfPresetCockpitDialPos).ToString().PadLeft(2, ' ');
                            }
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                        case CurrentM2000CRadioMode.NOUSE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }
                    }
                    SendLCDData(bytes);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving M2000C Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering M2000C Radio OnReport()");
                    //if (IsAttached == false) { return; }

                    if (report.Data.Length == 3)
                    {
                        Array.Copy(NewRadioPanelValue, OldRadioPanelValue, 3);
                        Array.Copy(report.Data, NewRadioPanelValue, 3);
                        var hashSet = GetHashSetOfChangedKnobs(OldRadioPanelValue, NewRadioPanelValue);
                        PZ69KnobChanged(hashSet);
                        OnSwitchesChanged(hashSet);
                        FirstReportHasBeenRead = true;
                        if (1 == 2 && Common.DebugOn)
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
                                    var knob = (RadioPanelKnobM2000C)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobM2000C)radioPanelKnob));
                                }
                            }
                        }
                        Common.DebugP("\r\nDone!\r\n");
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message + "\n" + ex.StackTrace);
                    SetLastException(ex);
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
                    SetLastException(ex);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78012, ex);
            }
            Common.DebugP("Leaving M2000C Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering M2000C Radio GetHashSetOfChangedKnobs()");


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
            }
            catch (Exception ex)
            {
                Common.LogError(78013, ex);
            }
            Common.DebugP("Leaving M2000C Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("M2000C");

                //COM1
                _vuhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("UVHF_PRESET_KNOB");

                //COM2
                _uhfDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET_KNOB");
                
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69M2000C.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering M2000C Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving M2000C Radio Shutdown()");
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

        private void CreateRadioKnobs()
        {
            _radioPanelKnobs = RadioPanelKnobM2000C.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobM2000C radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentM2000CRadioMode);
                _currentUpperRadioMode = currentM2000CRadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(78014, ex);
            }
            Common.DebugP("Leaving M2000C Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentM2000CRadioMode currentM2000CRadioMode)
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentM2000CRadioMode);
                _currentLowerRadioMode = currentM2000CRadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(78015, ex);
            }
            Common.DebugP("Leaving M2000C Radio SetLowerRadioMode()");
        }


        private bool SkipVUHFPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SkipVUHFPresetDialChange()");
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.VUHF || _currentLowerRadioMode == CurrentM2000CRadioMode.VUHF)
                {
                    if (_vuhfPresetDialSkipper > 2)
                    {
                        _vuhfPresetDialSkipper = 0;
                        Common.DebugP("Leaving M2000C Radio SkipVUHFPresetDialChange()");
                        return false;
                    }
                    _vuhfPresetDialSkipper++;
                    Common.DebugP("Leaving M2000C Radio SkipVUHFPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving M2000C Radio SkipVUHFPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        private bool SkipUHFPresetDialChange()
        {
            try
            {
                Common.DebugP("Entering M2000C Radio SkipUHFPresetDialChange()");
                if (_currentUpperRadioMode == CurrentM2000CRadioMode.UHF || _currentLowerRadioMode == CurrentM2000CRadioMode.UHF)
                {
                    if (_uhfPresetDialSkipper > 2)
                    {
                        _uhfPresetDialSkipper = 0;
                        Common.DebugP("Leaving M2000C Radio SkipUHFPresetDialChange()");
                        return false;
                    }
                    _uhfPresetDialSkipper++;
                    Common.DebugP("Leaving M2000C Radio SkipUHFPresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving M2000C Radio SkipUHFPresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(78009, ex);
            }
            return false;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }

    }
}
