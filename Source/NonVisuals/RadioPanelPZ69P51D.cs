﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69P51D : RadioPanelPZ69Base, IRadioPanel
    {
        private HashSet<RadioPanelKnobP51D> _radioPanelKnobs = new HashSet<RadioPanelKnobP51D>();
        private CurrentP51DRadioMode _currentUpperRadioMode = CurrentP51DRadioMode.VHF;
        private CurrentP51DRadioMode _currentLowerRadioMode = CurrentP51DRadioMode.VHF;

        /*P-51D VHF Presets 1-4*/
        //Large dial 1-4 [step of 1]
        //Small dial volume control
        private readonly object _lockVhf1DialObject1 = new object();
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton0;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton1;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton2;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton3;
        private DCSBIOSOutput _vhf1DcsbiosOutputPresetButton4;
        private volatile uint _vhf1CockpitPresetActiveButton = 0;
        private int _vhf1PresetDialSkipper;
        private const string Vhf1VolumeKnobCommandInc = "RADIO_VOLUME +2000\n";
        private const string Vhf1VolumeKnobCommandDec = "RADIO_VOLUME -2000\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69P51D(HIDSkeleton hidSkeleton, bool enableDCSBIOS = true) : base(hidSkeleton, enableDCSBIOS)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
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

                //VHF On Off
                if (e.Address == _vhf1DcsbiosOutputPresetButton0.Address)
                {
                    lock (_lockVhf1DialObject1)
                    {

                        var tmp = _vhf1CockpitPresetActiveButton;
                        if (_vhf1DcsbiosOutputPresetButton0.GetUIntValue(e.Data) == 1)
                        {
                            //Radio is off
                            _vhf1CockpitPresetActiveButton = 0;
                        }
                        if (tmp != _vhf1CockpitPresetActiveButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //VHF A
                if (e.Address == _vhf1DcsbiosOutputPresetButton1.Address)
                {
                    lock (_lockVhf1DialObject1)
                    {
                        var tmp = _vhf1CockpitPresetActiveButton;
                        if (_vhf1DcsbiosOutputPresetButton1.GetUIntValue(e.Data) == 1)
                        {
                            //Radio is on A
                            _vhf1CockpitPresetActiveButton = 1;
                        }
                        if (tmp != _vhf1CockpitPresetActiveButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //VHF B
                if (e.Address == _vhf1DcsbiosOutputPresetButton2.Address)
                {
                    lock (_lockVhf1DialObject1)
                    {
                        var tmp = _vhf1CockpitPresetActiveButton;
                        if (_vhf1DcsbiosOutputPresetButton2.GetUIntValue(e.Data) == 1)
                        {
                            //Radio is on A
                            _vhf1CockpitPresetActiveButton = 2;
                        }
                        if (tmp != _vhf1CockpitPresetActiveButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //VHF C
                if (e.Address == _vhf1DcsbiosOutputPresetButton3.Address)
                {
                    lock (_lockVhf1DialObject1)
                    {
                        var tmp = _vhf1CockpitPresetActiveButton;
                        if (_vhf1DcsbiosOutputPresetButton3.GetUIntValue(e.Data) == 1)
                        {
                            //Radio is on A
                            _vhf1CockpitPresetActiveButton = 3;
                        }
                        if (tmp != _vhf1CockpitPresetActiveButton)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }

                //VHF D
                if (e.Address == _vhf1DcsbiosOutputPresetButton4.Address)
                {
                    lock (_lockVhf1DialObject1)
                    {
                        var tmp = _vhf1CockpitPresetActiveButton;
                        if (_vhf1DcsbiosOutputPresetButton4.GetUIntValue(e.Data) == 1)
                        {
                            //Radio is on A
                            _vhf1CockpitPresetActiveButton = 4;
                        }
                        if (tmp != _vhf1CockpitPresetActiveButton)
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
                Common.LogError(84001, ex);
            }
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering P-51D Radio PZ69KnobChanged()");
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (_lockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobP51D)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsP51D.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP51DRadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_NOUSE0:
                            case RadioPanelPZ69KnobsP51D.UPPER_NOUSE1:
                            case RadioPanelPZ69KnobsP51D.UPPER_NOUSE2:
                            case RadioPanelPZ69KnobsP51D.UPPER_NOUSE3:
                            case RadioPanelPZ69KnobsP51D.UPPER_NOUSE4:
                            case RadioPanelPZ69KnobsP51D.UPPER_NOUSE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentP51DRadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP51DRadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_NOUSE0:
                            case RadioPanelPZ69KnobsP51D.LOWER_NOUSE1:
                            case RadioPanelPZ69KnobsP51D.LOWER_NOUSE2:
                            case RadioPanelPZ69KnobsP51D.LOWER_NOUSE3:
                            case RadioPanelPZ69KnobsP51D.LOWER_NOUSE4:
                            case RadioPanelPZ69KnobsP51D.LOWER_NOUSE5:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentP51DRadioMode.NOUSE);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentP51DRadioMode.VHF)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {

                                        }
                                        else
                                        {

                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentP51DRadioMode.VHF)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            //
                                        }
                                        else
                                        {
                                            //
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(84006, ex);
            }
            Common.DebugP("Leaving P-51D Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering P-51D Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobP51D = (RadioPanelKnobP51D)o;
                    if (radioPanelKnobP51D.IsOn)
                    {
                        switch (radioPanelKnobP51D.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    SendIncVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    SendDecVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandInc);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandDec);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    SendIncVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                if (!SkipVhf1PresetDialChange())
                                                {
                                                    SendDecVHFPresetCommand();
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandInc);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentP51DRadioMode.VHF:
                                            {
                                                DCSBIOS.Send(Vhf1VolumeKnobCommandDec);
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
                Common.LogError(84007, ex);
            }
            Common.DebugP("Leaving P-51D Radio AdjustFrequency()");
        }


        private bool SkipVhf1PresetDialChange()
        {
            try
            {
                Common.DebugP("Entering P-51D Radio SkipVhf1PresetDialChange()");
                if (_currentUpperRadioMode == CurrentP51DRadioMode.VHF || _currentLowerRadioMode == CurrentP51DRadioMode.VHF)
                {
                    if (_vhf1PresetDialSkipper > 2)
                    {
                        _vhf1PresetDialSkipper = 0;
                        Common.DebugP("Leaving P-51D Radio SkipVhf1PresetDialChange()");
                        return false;
                    }
                    _vhf1PresetDialSkipper++;
                    Common.DebugP("Leaving P-51D Radio SkipVhf1PresetDialChange()");
                    return true;
                }
                Common.DebugP("Leaving P-51D Radio SkipVhf1PresetDialChange()");
            }
            catch (Exception ex)
            {
                Common.LogError(84009, ex);
            }
            return false;
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

                    Common.DebugP("Entering P-51D Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentP51DRadioMode.VHF:
                            {
                                //Pos     0    1    2    3    4

                                var channelAsString = "";
                                lock (_lockVhf1DialObject1)
                                {
                                    channelAsString = _vhf1CockpitPresetActiveButton.ToString();
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentP51DRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentP51DRadioMode.VHF:
                            {
                                //Pos     0    1    2    3    4

                                var channelAsString = "";
                                lock (_lockVhf1DialObject1)
                                {
                                    channelAsString = _vhf1CockpitPresetActiveButton.ToString();
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentP51DRadioMode.NOUSE:
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
                Common.LogError(84011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving P-51D Radio ShowFrequenciesOnPanel()");
        }

        private void SendIncVHFPresetCommand()
        {
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            lock (_lockVhf1DialObject1)
            {
                switch (_vhf1CockpitPresetActiveButton)
                {
                    case 0:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_A 1\n");
                            break;
                        }
                    case 1:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_B 1\n");
                            break;
                        }
                    case 2:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_C 1\n");
                            break;
                        }
                    case 3:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_D 1\n");
                            break;
                        }
                    case 4:
                        {
                            break;
                        }
                }
            }
        }

        private void SendDecVHFPresetCommand()
        {
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
            lock (_lockVhf1DialObject1)
            {
                switch (_vhf1CockpitPresetActiveButton)
                {
                    case 0:
                        {
                            break;
                        }
                    case 1:
                        {
                            DCSBIOS.Send("VHF_RADIO_ON_OFF 1\n");
                            break;
                        }
                    case 2:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_A 1\n");
                            break;
                        }
                    case 3:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_B 1\n");
                            break;
                        }
                    case 4:
                        {
                            DCSBIOS.Send("VHF_RADIO_CHAN_C 1\n");
                            break;
                        }
                }
            }
        }

        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering P-51D Radio OnReport()");
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
                                    var knob = (RadioPanelKnobP51D)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobP51D)radioPanelKnob));
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
                Common.LogError(84012, ex);
            }
            Common.DebugP("Leaving P-51D Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering P-51D Radio GetHashSetOfChangedKnobs()");


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
                Common.LogError(84013, ex);
            }
            Common.DebugP("Leaving P-51D Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("P-51D");

                //VHF
                _vhf1DcsbiosOutputPresetButton0 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_RADIO_ON_OFF");
                _vhf1DcsbiosOutputPresetButton1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_RADIO_CHAN_A");
                _vhf1DcsbiosOutputPresetButton2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_RADIO_CHAN_B");
                _vhf1DcsbiosOutputPresetButton3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_RADIO_CHAN_C");
                _vhf1DcsbiosOutputPresetButton4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHF_RADIO_CHAN_D");


                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69P51D.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Common.DebugP("Entering P-51D Radio Shutdown()");
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving P-51D Radio Shutdown()");
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
            _radioPanelKnobs = RadioPanelKnobP51D.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobP51D radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentP51DRadioMode currentP51DRadioMode)
        {
            try
            {
                Common.DebugP("Entering P-51D Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentP51DRadioMode);
                _currentUpperRadioMode = currentP51DRadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(84014, ex);
            }
            Common.DebugP("Leaving P-51D Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentP51DRadioMode currentP51DRadioMode)
        {
            try
            {
                Common.DebugP("Entering P-51D Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentP51DRadioMode);
                _currentLowerRadioMode = currentP51DRadioMode;
                //If NOUSE then send next round of e.Data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(84015, ex);
            }
            Common.DebugP("Leaving P-51D Radio SetLowerRadioMode()");
        }

        public override string SettingsVersion()
        {
            return "0X";
        }


    }
}
