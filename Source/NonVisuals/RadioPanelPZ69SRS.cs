﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;
using Newtonsoft;
using Newtonsoft.Json;

namespace NonVisuals
{
    public class RadioPanelPZ69SRS : RadioPanelPZ69Base, IRadioPanel, ISRSDataListener
    {
        private HashSet<RadioPanelKnobSRS> _radioPanelKnobs = new HashSet<RadioPanelKnobSRS>();
        private CurrentSRSRadioMode _currentUpperRadioMode = CurrentSRSRadioMode.COM1;
        private CurrentSRSRadioMode _currentLowerRadioMode = CurrentSRSRadioMode.COM1;
        
        //private List<double> _listMainFrequencies = new List<double>(7) { 0, 0, 0, 0, 0, 0, 0 };
        //private List<double> _listGuardFrequencies = new List<double>(7) { 0, 0, 0, 0, 0, 0, 0 };
        private readonly object _freqListLockObject = new object();

        /*Radio1 COM1*/
        /*Radio2 COM2*/
        /*Radio3 NAV1*/
        /*Radio4 NAV2*/
        /*Radio5 ADF*/
        /*Radio6 DME*/
        /*Radio7 XPDR*/
        //Large dial
        //Small dial

        private double _upperMainFreq = 0;
        private double _lowerMainFreq = 0;
        private double _upperGuardFreq = 0;
        private double _lowerGuardFreq = 0;
        private long _upperFreqSwitchPressed = 0;
        private long _lowerFreqSwitchPressed = 0;
        private int _largeDialSkipper;
        private int _smallDialSkipper;
        private ClickSpeedDetector _largeDialIncreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _largeDialDecreaseChangeMonitor = new ClickSpeedDetector(20);
        private ClickSpeedDetector _firstSmallDialIncreaseChangeMonitor = new ClickSpeedDetector(30);
        private ClickSpeedDetector _firstSmallDialDecreaseChangeMonitor = new ClickSpeedDetector(30);
        private ClickSpeedDetector _secondSmallDialIncreaseChangeMonitor = new ClickSpeedDetector(36);
        private ClickSpeedDetector _secondSmallDialDecreaseChangeMonitor = new ClickSpeedDetector(36);

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SRS(int portFrom, string ipAddressTo, int portTo, HIDSkeleton hidSkeleton) : base(hidSkeleton, false)
        {
            SRSListenerFactory.SetParams(portFrom, ipAddressTo, portTo);
            SRSListenerFactory.GetSRSListener().Attach(this);
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        public sealed override void Startup()
        {
            try
            {
                StartupBase("SRS");
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69SRS.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }


        public override void Shutdown()
        {
            try
            {
                SRSListenerFactory.GetSRSListener().Detach(this);
                Common.DebugP("Entering SRS Radio Shutdown()");
                ShutdownBase();
                SRSListenerFactory.Shutdown();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
            Common.DebugP("Leaving SRS Radio Shutdown()");
        }

        public void SRSDataReceived(object sender)
        {
            try
            {
                _upperMainFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentUpperRadioMode);
                _upperGuardFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentUpperRadioMode, true);
                _lowerMainFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentLowerRadioMode);
                _lowerGuardFreq = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_currentLowerRadioMode, true);
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                ShowFrequenciesOnPanel();
            }
            catch (Exception e)
            {
                Common.LogError(8159006, e);
            }
        }

        private void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering SRS Radio PZ69KnobChanged()");
                lock (_lockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobSRS)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSRS.UPPER_COM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.COM1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_COM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.COM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NAV1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NAV1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_NAV2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.NAV2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_DME:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.DME);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_XPDR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentSRSRadioMode.XPDR);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_COM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.COM1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_COM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.COM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NAV1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NAV1);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_NAV2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.NAV2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_DME:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.DME);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_XPDR:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentSRSRadioMode.XPDR);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        _upperFreqSwitchPressed = DateTime.Now.Ticks;
                                    }

                                    if (!radioPanelKnob.IsOn)
                                    {
                                        var timeSpan = new TimeSpan(DateTime.Now.Ticks - _upperFreqSwitchPressed);
                                        if (timeSpan.Seconds <= 2)
                                        {
                                            var message = GetToggleGuardFreqCommand(_currentUpperRadioMode);
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(message);
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        _lowerFreqSwitchPressed = DateTime.Now.Ticks;
                                    }

                                    if (!radioPanelKnob.IsOn)
                                    {
                                        var timeSpan = new TimeSpan(DateTime.Now.Ticks - _lowerFreqSwitchPressed);
                                        if (timeSpan.Seconds <= 2)
                                        {
                                            var message = GetToggleGuardFreqCommand(_currentLowerRadioMode);
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(message);
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
                Common.LogError(82006, ex);
            }
            Common.DebugP("Leaving SRS Radio PZ69KnobChanged()");
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                Common.DebugP("Entering SRS Radio AdjustFrequency()");

                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobSRS = (RadioPanelKnobSRS)o;
                    if (radioPanelKnobSRS.IsOn)
                    {
                        switch (radioPanelKnobSRS.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    _largeDialIncreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(GetChannelCommand(_currentUpperRadioMode, true));
                                        }
                                        else
                                        {
                                            var changeValue = 1.0;
                                            if (_largeDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = 5.0;
                                            }

                                            var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _largeDialDecreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(GetChannelCommand(_currentUpperRadioMode, false));
                                        }
                                        else
                                        {
                                            var changeValue = -1.0;
                                            if (_largeDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = -5.0;
                                            }

                                            var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    _firstSmallDialIncreaseChangeMonitor.Click();
                                    _secondSmallDialIncreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = 0.001;
                                        if (_firstSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.01;
                                        }
                                        if (_secondSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                        SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    _firstSmallDialDecreaseChangeMonitor.Click();
                                    _secondSmallDialDecreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = -0.001;
                                        if (_firstSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.01;
                                        }
                                        if (_secondSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentUpperRadioMode, changeValue);
                                        SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    _largeDialIncreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(GetChannelCommand(_currentLowerRadioMode, true));
                                        }
                                        else
                                        {
                                            var changeValue = 1.0;
                                            if (_largeDialIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = 10.0;
                                            }

                                            var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    _largeDialDecreaseChangeMonitor.Click();
                                    if (!SkipLargeDialDialChange())
                                    {
                                        if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                                        {
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(GetChannelCommand(_currentLowerRadioMode, false));
                                        }
                                        else
                                        {
                                            var changeValue = -1.0;
                                            if (_largeDialDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                changeValue = -10.0;
                                            }

                                            var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                            SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                        }
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    _firstSmallDialIncreaseChangeMonitor.Click();
                                    _secondSmallDialIncreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = 0.001;
                                        if (_firstSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.01;
                                        }
                                        if (_secondSmallDialIncreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = 0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                        SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    _firstSmallDialDecreaseChangeMonitor.Click();
                                    _secondSmallDialDecreaseChangeMonitor.Click();
                                    if (!SkipSmallDialDialChange())
                                    {
                                        var changeValue = -0.001;
                                        if (_firstSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.01;
                                        }
                                        if (_secondSmallDialDecreaseChangeMonitor.ClickThresholdReached())
                                        {
                                            changeValue = -0.1;
                                        }
                                        var command = GetFreqChangeSendCommand(_currentLowerRadioMode, changeValue);
                                        SRSListenerFactory.GetSRSListener().SendDataFunction(command);
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82007, ex);
            }
            Common.DebugP("Leaving SRS Radio AdjustFrequency()");
        }

        private string GetToggleGuardFreqCommand(CurrentSRSRadioMode currentSRSRadioMode)
        {
            var radioId = 0;
            switch (currentSRSRadioMode)
            {
                case CurrentSRSRadioMode.COM1:
                    {
                        radioId = 1;
                        break;
                    }
                case CurrentSRSRadioMode.COM2:
                    {
                        radioId = 2;
                        break;
                    }
                case CurrentSRSRadioMode.NAV1:
                    {
                        radioId = 3;
                        break;
                    }
                case CurrentSRSRadioMode.NAV2:
                    {
                        radioId = 4;
                        break;
                    }
                case CurrentSRSRadioMode.ADF:
                    {
                        radioId = 5;
                        break;
                    }
                case CurrentSRSRadioMode.DME:
                    {
                        radioId = 6;
                        break;
                    }
                case CurrentSRSRadioMode.XPDR:
                    {
                        radioId = 7;
                        break;
                    }
            }
            var result = "{\"Command\": 2,\"RadioId\":" + radioId + "}\n";
            //{ "Command": 2,"RadioId":2} 
            Common.DebugP(result);
            return result;
        }

        private string GetChannelCommand(CurrentSRSRadioMode currentSRSRadioMode, bool increase)
        {
            var radioId = 0;
            switch (currentSRSRadioMode)
            {
                case CurrentSRSRadioMode.COM1:
                {
                    radioId = 1;
                    break;
                }
                case CurrentSRSRadioMode.COM2:
                {
                    radioId = 2;
                    break;
                }
                case CurrentSRSRadioMode.NAV1:
                {
                    radioId = 3;
                    break;
                }
                case CurrentSRSRadioMode.NAV2:
                {
                    radioId = 4;
                    break;
                }
                case CurrentSRSRadioMode.ADF:
                {
                    radioId = 5;
                    break;
                }
                case CurrentSRSRadioMode.DME:
                {
                    radioId = 6;
                    break;
                }
                case CurrentSRSRadioMode.XPDR:
                {
                    radioId = 7;
                    break;
                }
            }
            /*{ "Command": 3,"RadioId":1}
            --channel up(if channels have been configured)

            { "Command": 4,"RadioId":1}
            --channel down(if channels have been configured)*/
            var result = "";
            if (increase)
            {
                result = "{\"Command\": 3,\"RadioId\":" + radioId + "}\n";
            }
            else
            {
                result = "{\"Command\": 4,\"RadioId\":" + radioId + "}\n";
            }
            Common.DebugP(result);
            return result;
        }

        private string GetFreqChangeSendCommand(CurrentSRSRadioMode currentSRSRadioMode, double value)
        {
            var radioId = 0;
            switch (currentSRSRadioMode)
            {
                case CurrentSRSRadioMode.COM1:
                    {
                        radioId = 1;
                        break;
                    }
                case CurrentSRSRadioMode.COM2:
                    {
                        radioId = 2;
                        break;
                    }
                case CurrentSRSRadioMode.NAV1:
                    {
                        radioId = 3;
                        break;
                    }
                case CurrentSRSRadioMode.NAV2:
                    {
                        radioId = 4;
                        break;
                    }
                case CurrentSRSRadioMode.ADF:
                    {
                        radioId = 5;
                        break;
                    }
                case CurrentSRSRadioMode.DME:
                    {
                        radioId = 6;
                        break;
                    }
                case CurrentSRSRadioMode.XPDR:
                    {
                        radioId = 7;
                        break;
                    }
            }
            var result = "{ \"Command\": 0,\"RadioId\":" + radioId + ",\"Frequency\": " + value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " }\n";
            Common.DebugP(result);
            return result;
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

                    Common.DebugP("Entering SRS Radio ShowFrequenciesOnPanel()");
                    var bytes = new byte[21];
                    bytes[0] = 0x0;
                    lock (_freqListLockObject)
                    {
                        if (_upperMainFreq > 0)
                        {
                            if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentUpperRadioMode) == SRSRadioMode.Channel)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)Math.Floor(_upperMainFreq), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, (_upperMainFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                        }
                        if (_upperGuardFreq > 0)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, (_upperGuardFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                        }
                        if (_lowerMainFreq > 0)
                        {
                            if (SRSListenerFactory.GetSRSListener().GetRadioMode(_currentLowerRadioMode) == SRSRadioMode.Channel)
                            {
                                SetPZ69DisplayBytesInteger(ref bytes, (int)Math.Floor(_lowerMainFreq), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, (_lowerMainFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        if (_lowerGuardFreq > 0)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, (_lowerGuardFreq / 1000000).ToString("0.000", CultureInfo.InvariantCulture), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        SendLCDData(bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(82011, ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
            Common.DebugP("Leaving SRS Radio ShowFrequenciesOnPanel()");
        }


        private void OnReport(HidReport report)
        {
            try
            {
                try
                {
                    Common.DebugP("Entering SRS Radio OnReport()");
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
                                    var knob = (RadioPanelKnobSRS)radioPanelKnob;
                                    Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(NewRadioPanelValue, (RadioPanelKnobSRS)radioPanelKnob));
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
                Common.LogError(82012, ex);
            }
            Common.DebugP("Leaving SRS Radio OnReport()");
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            try
            {
                Common.DebugP("Entering SRS Radio GetHashSetOfChangedKnobs()");


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
                Common.LogError(82013, ex);
            }
            Common.DebugP("Leaving SRS Radio GetHashSetOfChangedKnobs()");
            return result;
        }

        public override void ClearSettings()
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            _radioPanelKnobs = RadioPanelKnobSRS.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobSRS radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private void SetUpperRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering SRS Radio SetUpperRadioMode()");
                Common.DebugP("Setting upper radio mode to " + currentBf109RadioMode);
                _currentUpperRadioMode = currentBf109RadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError(82014, ex);
            }
            Common.DebugP("Leaving SRS Radio SetUpperRadioMode()");
        }

        private void SetLowerRadioMode(CurrentSRSRadioMode currentBf109RadioMode)
        {
            try
            {
                Common.DebugP("Entering SRS Radio SetLowerRadioMode()");
                Common.DebugP("Setting lower radio mode to " + currentBf109RadioMode);
                _currentLowerRadioMode = currentBf109RadioMode;
                //If NOUSE then send next round of data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError(82015, ex);
            }
            Common.DebugP("Leaving SRS Radio SetLowerRadioMode()");
        }

        private bool SkipSmallDialDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipSmallDialDialChange()");
                if (_smallDialSkipper > 2)
                {
                    _smallDialSkipper = 0;
                    Common.DebugP("Leaving SRS Radio SkipSmallDialDialChange()");
                    return false;
                }
                _smallDialSkipper++;
                Common.DebugP("Leaving SRS Radio SkipSmallDialDialChange()");
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(82009, ex);
            }
            return false;
        }

        private bool SkipLargeDialDialChange()
        {
            try
            {
                Common.DebugP("Entering SRS Radio SkipLargeDialDialChange()");
                if (_largeDialSkipper > 2)
                {
                    _largeDialSkipper = 0;
                    Common.DebugP("Leaving SRS Radio SkipLargeDialDialChange()");
                    return false;
                }
                _largeDialSkipper++;
                Common.DebugP("Leaving SRS Radio SkipLargeDialDialChange()");
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(82009, ex);
            }
            return false;
        }


        public override String SettingsVersion()
        {
            return "0X";
        }


        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.LogError(78030, ex, "DCSBIOSStringReceived()");
            }
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.LogError(82001, ex);
            }
        }
    }
}
