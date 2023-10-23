using NonVisuals.BindingClasses.BIP;
using NonVisuals.Helpers;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
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


    /*
     * Pre-programmed radio panel for the AH-64D.
     */
    public class RadioPanelPZ69AH64D : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentAH64DRadioMode
        {
            VHF,
            UHF,
            FM1,
            FM2,
            HF,
            NO_USE
        }

        private CurrentAH64DRadioMode _currentUpperRadioMode = CurrentAH64DRadioMode.UHF;
        private CurrentAH64DRadioMode _currentLowerRadioMode = CurrentAH64DRadioMode.UHF;

        /*VHF AM*/
        /*108.000 - 151.975 MHz*/
        private readonly object _lockVHFObject = new();
        private uint _vhfBigFrequencyStandby = 108;
        private uint _vhfSmallFrequencyStandby;
        private string _vhfCockpitFrequency = "108.000";
        private DCSBIOSOutput _vhfRadioControl;
        private const string VHF_RADIO_COMMAND = "VHF_AM_RADIO ";

        /*UHF*/
        /* 225.000 - 399.975 MHz */
        private readonly object _lockUHFObject = new();
        private uint _uhfBigFrequencyStandby = 225;
        private uint _uhfSmallFrequencyStandby;
        private string _uhfCockpitFrequency = "225.000";
        private DCSBIOSOutput _uhfRadioControl;
        private const string UHF_RADIO_COMMAND = "UHF_RADIO ";

        /*FM1*/
        /* 30.000 - 87.975 MHz */
        private readonly object _lockFM1Object = new();
        private uint _fm1BigFrequencyStandby = 30;
        private uint _fm1SmallFrequencyStandby;
        private string _fm1CockpitFrequency = "30.000";
        private DCSBIOSOutput _fm1RadioControl;
        private const string FM1_RADIO_COMMAND = "FM1_RADIO ";

        /*FM2*/
        /* 30.000 - 87.975 MHz */
        private readonly object _lockFM2Object = new();
        private uint _fm2BigFrequencyStandby = 30;
        private uint _fm2SmallFrequencyStandby;
        private string _fm2CockpitFrequency = "30.000";
        private DCSBIOSOutput _fm2RadioControl;
        private const string FM2_RADIO_COMMAND = "FM2_RADIO ";

        /*HF*/
        /* 2.0000 - 29.9999 MHz */
        private readonly ClickSpeedDetector _hfSmallFrequencyStandbyMonitor = new(25);
        private const uint HF_ELEVATED_CHANGE_RATE = 50;
        private readonly object _lockHFObject = new();
        private uint _hfBigFrequencyStandby = 2;
        private uint _hfSmallFrequencyStandby;
        private string _hfCockpitFrequency = "2.0000";
        private DCSBIOSOutput _hfRadioControl;
        private const string HF_RADIO_COMMAND = "HF_RADIO ";

        private long _doUpdatePanelLCD;
        private readonly object _lockShowFrequenciesOnPanelObject = new();

        public RadioPanelPZ69AH64D(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (_vhfRadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockVHFObject)
                    {
                        _vhfCockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_uhfRadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockUHFObject)
                    {
                        _uhfCockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_fm1RadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockFM1Object)
                    {
                        _fm1CockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_fm2RadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockFM2Object)
                    {
                        _fm2CockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                if (_hfRadioControl.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockHFObject)
                    {
                        _hfCockpitFrequency = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
            ShowFrequenciesOnPanel();
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            //ShowFrequenciesOnPanel();
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAH64D knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsAH64D.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsAH64D.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsAH64D.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentAH64DRadioMode.VHF:
                                {
                                    SendVHFToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.UHF:
                                {
                                    SendUHFToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.FM1:
                                {
                                    SendFM1ToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.FM2:
                                {
                                    SendFM2ToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.HF:
                                {
                                    SendHFToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }

                case RadioPanelPZ69KnobsAH64D.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {

                            case CurrentAH64DRadioMode.VHF:
                                {
                                    SendVHFToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.UHF:
                                {
                                    SendUHFToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.FM1:
                                {
                                    SendFM1ToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.FM2:
                                {
                                    SendFM2ToDCSBIOS();
                                    break;
                                }
                            case CurrentAH64DRadioMode.HF:
                                {
                                    SendHFToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void SendVHFToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _vhfCockpitFrequency;
                DCSBIOS.Send($"{VHF_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentAH64DRadioMode.VHF)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _vhfBigFrequencyStandby = uint.Parse(array[0]);
                _vhfSmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendUHFToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _uhfCockpitFrequency;
                DCSBIOS.Send($"{UHF_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentAH64DRadioMode.UHF)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _uhfBigFrequencyStandby = uint.Parse(array[0]);
                _uhfSmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendFM1ToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _fm1CockpitFrequency;
                DCSBIOS.Send($"{FM1_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentAH64DRadioMode.FM1)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _fm1BigFrequencyStandby = uint.Parse(array[0]);
                _fm1SmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendFM2ToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _fm2CockpitFrequency;
                DCSBIOS.Send($"{FM2_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentAH64DRadioMode.FM2)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _fm2BigFrequencyStandby = uint.Parse(array[0]);
                _fm2SmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendHFToDCSBIOS()
        {
            try
            {
                var newStandbyFrequency = _hfCockpitFrequency;
                DCSBIOS.Send($"{HF_RADIO_COMMAND} {GetStandbyFrequencyString(CurrentAH64DRadioMode.HF)}\n");
                var array = newStandbyFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
                _hfBigFrequencyStandby = uint.Parse(array[0]);
                _hfSmallFrequencyStandby = uint.Parse(array[1]);
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private string GetStandbyFrequencyString(CurrentAH64DRadioMode radio)
        {
            return radio switch
            {
                CurrentAH64DRadioMode.VHF => _vhfBigFrequencyStandby + "." + _vhfSmallFrequencyStandby.ToString().PadLeft(3, '0'),
                CurrentAH64DRadioMode.UHF => _uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(3, '0'),
                CurrentAH64DRadioMode.FM1 => _fm1BigFrequencyStandby + "." + _fm1SmallFrequencyStandby.ToString().PadLeft(3, '0'),
                CurrentAH64DRadioMode.FM2 => _fm2BigFrequencyStandby + "." + _fm2SmallFrequencyStandby.ToString().PadLeft(3, '0'),
                CurrentAH64DRadioMode.HF => _hfBigFrequencyStandby + "." + _hfSmallFrequencyStandby.ToString().PadLeft(4, '0'),
                _ => throw new ArgumentOutOfRangeException(nameof(radio), radio, "AH-64D.GetFrequencyString()")
            };
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
                    case CurrentAH64DRadioMode.NO_USE:
                        {
                            lock (_lockFM1Object)
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.VHF:
                        {
                            lock (_lockVHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.VHF), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _vhfCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.UHF:
                        {
                            lock (_lockUHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.UHF), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _uhfCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.FM1:
                        {
                            lock (_lockFM1Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.FM1), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _fm1CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.FM2:
                        {
                            lock (_lockFM2Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.FM2), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _fm2CockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.HF:
                        {
                            lock (_lockHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.HF), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _hfCockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            }
                            break;
                        }
                }

                switch (_currentLowerRadioMode)
                {
                    case CurrentAH64DRadioMode.NO_USE:
                        {
                            lock (_lockFM1Object)
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.VHF:
                        {
                            lock (_lockVHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.VHF), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _vhfCockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.UHF:
                        {
                            lock (_lockUHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.UHF), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _uhfCockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.FM1:
                        {
                            lock (_lockFM1Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.FM1), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _fm1CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }

                    case CurrentAH64DRadioMode.FM2:
                        {
                            lock (_lockFM2Object)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.FM2), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _fm2CockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                    case CurrentAH64DRadioMode.HF:
                        {
                            lock (_lockHFObject)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, GetStandbyFrequencyString(CurrentAH64DRadioMode.HF), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, _hfCockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            }
                            break;
                        }
                }

                SendLCDData(bytes);
            }
            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            if (SkipCurrentFrequencyChange())
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var radioPanelKnobAH64D = (RadioPanelKnobAH64D)o;
                if (radioPanelKnobAH64D.IsOn)
                {
                    switch (radioPanelKnobAH64D.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby > 151)
                                            {
                                                // @ max value
                                                _vhfBigFrequencyStandby = 151;
                                                break;
                                            }
                                            _vhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby > 399)
                                            {
                                                _uhfBigFrequencyStandby = 399;
                                                // @ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1BigFrequencyStandby > 87)
                                            {
                                                _fm1BigFrequencyStandby = 87;
                                                // @ max value
                                                break;
                                            }
                                            _fm1BigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2BigFrequencyStandby > 87)
                                            {
                                                _fm2BigFrequencyStandby = 87;
                                                // @ max value
                                                break;
                                            }
                                            _fm2BigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfBigFrequencyStandby > 29)
                                            {
                                                _hfBigFrequencyStandby = 29;
                                                // @ max value
                                                break;
                                            }
                                            _hfBigFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby <= 108)
                                            {
                                                _vhfBigFrequencyStandby = 108;
                                                // @ min value
                                                break;
                                            }
                                            _vhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby <= 225)
                                            {
                                                _uhfBigFrequencyStandby = 225;
                                                // @ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1BigFrequencyStandby <= 30)
                                            {
                                                _fm1BigFrequencyStandby = 30;
                                                // @ min value
                                                break;
                                            }
                                            _fm1BigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2BigFrequencyStandby <= 30)
                                            {
                                                _fm2BigFrequencyStandby = 30;
                                                // @ min value
                                                break;
                                            }
                                            _fm2BigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfBigFrequencyStandby <= 2)
                                            {
                                                _hfBigFrequencyStandby = 2;
                                                // @ min value
                                                break;
                                            }
                                            _hfBigFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby > 975)
                                            {
                                                _vhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }

                                            _vhfSmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby > 975)
                                            {
                                                _uhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1SmallFrequencyStandby > 975)
                                            {
                                                _fm1SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _fm1SmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2SmallFrequencyStandby > 975)
                                            {
                                                _fm2SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _fm2SmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            uint changeValue = 1;
                                            if (_hfSmallFrequencyStandbyMonitor.ClickAndCheck())
                                            {
                                                changeValue = HF_ELEVATED_CHANGE_RATE;
                                            }
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfSmallFrequencyStandby > 9999)
                                            {
                                                _hfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _hfSmallFrequencyStandby += changeValue;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _vhfSmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1SmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _fm1SmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2SmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _fm2SmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            uint changeValue = 1;
                                            if (_hfSmallFrequencyStandbyMonitor.ClickAndCheck())
                                            {
                                                changeValue = HF_ELEVATED_CHANGE_RATE;
                                            }
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfSmallFrequencyStandby < changeValue)
                                            {
                                                _hfSmallFrequencyStandby = 9999;
                                                // @ min value
                                                break;
                                            }
                                            _hfSmallFrequencyStandby -= changeValue;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby > 151)
                                            {
                                                // @ max value
                                                _vhfBigFrequencyStandby = 151;
                                                break;
                                            }
                                            _vhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby > 399)
                                            {
                                                _uhfBigFrequencyStandby = 399;
                                                // @ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1BigFrequencyStandby > 87)
                                            {
                                                _fm1BigFrequencyStandby = 87;
                                                // @ max value
                                                break;
                                            }
                                            _fm1BigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2BigFrequencyStandby > 87)
                                            {
                                                _fm2BigFrequencyStandby = 87;
                                                // @ max value
                                                break;
                                            }
                                            _fm2BigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfBigFrequencyStandby > 29)
                                            {
                                                _hfBigFrequencyStandby = 29;
                                                // @ max value
                                                break;
                                            }
                                            _hfBigFrequencyStandby++;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfBigFrequencyStandby <= 108)
                                            {
                                                _vhfBigFrequencyStandby = 108;
                                                // @ min value
                                                break;
                                            }
                                            _vhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfBigFrequencyStandby <= 225)
                                            {
                                                _uhfBigFrequencyStandby = 225;
                                                // @ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1BigFrequencyStandby <= 30)
                                            {
                                                _fm1BigFrequencyStandby = 30;
                                                // @ min value
                                                break;
                                            }
                                            _fm1BigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2BigFrequencyStandby <= 30)
                                            {
                                                _fm2BigFrequencyStandby = 30;
                                                // @ min value
                                                break;
                                            }
                                            _fm2BigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfBigFrequencyStandby <= 2)
                                            {
                                                _hfBigFrequencyStandby = 2;
                                                // @ min value
                                                break;
                                            }
                                            _hfBigFrequencyStandby--;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby > 975)
                                            {
                                                _vhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }

                                            _vhfSmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby > 975)
                                            {
                                                _uhfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1SmallFrequencyStandby > 975)
                                            {
                                                _fm1SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _fm1SmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2SmallFrequencyStandby > 975)
                                            {
                                                _fm2SmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _fm2SmallFrequencyStandby += 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            uint changeValue = 1;
                                            if (_hfSmallFrequencyStandbyMonitor.ClickAndCheck())
                                            {
                                                changeValue = HF_ELEVATED_CHANGE_RATE;
                                            }
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfSmallFrequencyStandby > 9999)
                                            {
                                                _hfSmallFrequencyStandby = 0;
                                                // @ max value
                                                break;
                                            }
                                            _hfSmallFrequencyStandby += changeValue;
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentAH64DRadioMode.VHF:
                                        {
                                            /* 108.000 - 151.975 MHz */
                                            if (_vhfSmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _vhfSmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.UHF:
                                        {
                                            /* 225.000 - 399.975 MHz */
                                            if (_uhfSmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM1:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm1SmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _fm1SmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.FM2:
                                        {
                                            /* 30.000 - 87.975 MHz */
                                            if (_fm2SmallFrequencyStandby.Equals(0))
                                            {
                                                // @ min value
                                                break;
                                            }
                                            _fm2SmallFrequencyStandby -= 25;
                                            break;
                                        }
                                    case CurrentAH64DRadioMode.HF:
                                        {
                                            uint changeValue = 1;
                                            if (_hfSmallFrequencyStandbyMonitor.ClickAndCheck())
                                            {
                                                changeValue = HF_ELEVATED_CHANGE_RATE;
                                            }
                                            /* 2.0000 - 29.9999 MHz */
                                            if (_hfSmallFrequencyStandby < changeValue)
                                            {
                                                _hfSmallFrequencyStandby = 9999;
                                                // @ min value
                                                break;
                                            }
                                            _hfSmallFrequencyStandby -= changeValue;
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

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            Interlocked.Increment(ref _doUpdatePanelLCD);
            lock (LockLCDUpdateObject)
            {
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobAH64D)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsAH64D.UPPER_VHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAH64DRadioMode.VHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAH64DRadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.UPPER_FM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAH64DRadioMode.FM1;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.UPPER_FM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAH64DRadioMode.FM2;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.UPPER_HF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAH64DRadioMode.HF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_VHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAH64DRadioMode.VHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAH64DRadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_FM1:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAH64DRadioMode.FM1;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_FM2:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAH64DRadioMode.FM2;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_HF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAH64DRadioMode.HF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.UPPER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAH64D.UPPER_FREQ_SWITCH);
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_FREQ_SWITCH:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsAH64D.LOWER_FREQ_SWITCH);
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.UPPER_NO_USE3:
                        case RadioPanelPZ69KnobsAH64D.UPPER_NO_USE4:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentAH64DRadioMode.NO_USE;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsAH64D.LOWER_NO_USE3:
                        case RadioPanelPZ69KnobsAH64D.LOWER_NO_USE4:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentAH64DRadioMode.NO_USE;
                                }
                                break;
                            }
                    }

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(
                            DCSAircraft.SelectedAircraft.Description,
                            HIDInstance,
                            PluginGamingPanelEnum.PZ69RadioPanel_PreProg_AH64D,
                            (int)radioPanelKnob.RadioPanelPZ69Knob,
                            radioPanelKnob.IsOn,
                            null);
                    }
                }
                AdjustFrequency(hashSet);
            }
        }

        public sealed override void Startup()
        {
            try
            {
                // VHF
                _vhfRadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("VHF_AM_RADIO");

                // UHF
                _uhfRadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UHF_RADIO");

                // FM1 FM2
                _fm1RadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("FM1_RADIO");
                _fm2RadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("FM2_RADIO");

                // HF
                _hfRadioControl = DCSBIOSControlLocator.GetStringDCSBIOSOutput("HF_RADIO");

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobAH64D.GetRadioPanelKnobs();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}

