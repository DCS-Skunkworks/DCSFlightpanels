//
//  added by Capt Zeen
//

using NonVisuals.BindingClasses.BIP;

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
    using DCS_BIOS.Serialized;
    using DCS_BIOS.ControlLocator;


    /// <summary>
    /// Pre-programmed radio panel for the F/A-18C. 
    /// </summary>
    public class RadioPanelPZ69FA18C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentFA18CRadioMode
        {
            COMM2,
            VHFFM,
            COMM1,
            TACAN,
            ILS
        }

        private CurrentFA18CRadioMode _currentUpperRadioMode = CurrentFA18CRadioMode.COMM1;
        private CurrentFA18CRadioMode _currentLowerRadioMode = CurrentFA18CRadioMode.COMM2;
        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        /*FA-18C COMM1 radio*/
        private const string COMM1_CHANNEL_INC = "UFC_COMM1_CHANNEL_SELECT INC\n";
        private const string COMM1_CHANNEL_DEC = "UFC_COMM1_CHANNEL_SELECT DEC\n";
        private const string COMM1_VOL_INC = "UFC_COMM1_VOL +4000\n";
        private const string COMM1_VOL_DEC = "UFC_COMM1_VOL -4000\n";
        private const string COMM1_PULL_PRESS = "UFC_COMM1_PULL INC\n";
        private const string COMM1_PULL_RELEASE = "UFC_COMM1_PULL DEC\n";
        private readonly object _lockCOMM1DialsObject = new();
        private DCSBIOSOutput _comm1DcsbiosOutputFreq; // comm1 frequency from DCSbios
        private DCSBIOSOutput _comm1DcsbiosOutputChannel; // comm1 channel 1 to 24 from CDSbios
        private volatile uint _comm1CockpitFreq = 12400;
        private volatile uint _comm1CockpitChannel = 1; // channel number 1 to 24
        private long _comm1DialWaitingForFeedback;

        /*FA-18C COMM2 radio*/
        private const string COMM2_CHANNEL_INC = "UFC_COMM2_CHANNEL_SELECT INC\n";
        private const string COMM2_CHANNEL_DEC = "UFC_COMM2_CHANNEL_SELECT DEC\n";
        private const string COMM2_VOL_INC = "UFC_COMM2_VOL +4000\n";
        private const string COMM2_VOL_DEC = "UFC_COMM2_VOL -4000\n";
        private const string COMM2_PULL_PRESS = "UFC_COMM2_PULL INC\n";
        private const string COMM2_PULL_RELEASE = "UFC_COMM2_PULL DEC\n";
        private double _comm2BigFrequencyStandby = 255;
        private readonly object _lockComm2DialObject = new();
        private DCSBIOSOutput _comm2DcsbiosOutputFreq; // comm2 frequency from DCSbios
        private DCSBIOSOutput _comm2DcsbiosOutputChannel; // comm2 channel 1 to 24

        // private DCSBIOSOutput _comm2DcsbiosOutputPull;  // comm2 pull button
        // private DCSBIOSOutput _comm2DcsbiosOutputVol;   // comm2 volume
        private volatile uint _comm2CockpitFreq = 12400;
        private volatile uint _comm2CockpitChannel = 1; // channel number 1 to 24
        private long _comm2DialWaitingForFeedback;

        /*FA-18C ILS*/
        private uint _ilsChannelStandby = 10;
        private uint _ilsSavedCockpitChannel = 1;
        private readonly object _lockIlsDialsObject = new();
        private DCSBIOSOutput _ilsDcsbiosOutputChannel;
        private volatile uint _ilsCockpitChannel = 1;
        private const string ILS_CHANNEL_COMMAND = "COM_ILS_CHANNEL_SW ";
        private Thread _ilsSyncThread;
        private long _ilsThreadNowSyncing;
        private long _ilsDialWaitingForFeedback;
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69FA18C(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {}

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            TurnOffAllDisplays();
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownILSThread = true;
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void InitPanel()
        {
            CreateRadioKnobs();

            // COMM 1
            _comm1DcsbiosOutputFreq = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM1_FREQ");
            _comm1DcsbiosOutputChannel = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM1_CHANNEL_NUMERIC");

            // COMM 2
            _comm2DcsbiosOutputFreq = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM2_FREQ");
            _comm2DcsbiosOutputChannel = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COMM2_CHANNEL_NUMERIC");

            // ILS
            _ilsDcsbiosOutputChannel = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("COM_ILS_CHANNEL_SW");

            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
        }
        
        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            // -------------------------   Get the data from DCSbios
            UpdateCounter(e.Address, e.Data);

            /*
             * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
             * Once a dial has been deemed to be "off" position and needs to be changed
             * a change command is sent to DCS-BIOS.
             * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
             * reset. Reading the dial's position with no change in value will not reset.
             */

            // COMM 1
            if (_comm1DcsbiosOutputFreq.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockCOMM1DialsObject)
                {
                    _comm1CockpitFreq = _comm1DcsbiosOutputFreq.LastUIntValue;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                    Interlocked.Exchange(ref _comm1DialWaitingForFeedback, 0);
                }
            }

            if (_comm1DcsbiosOutputChannel.UIntValueHasChanged(e.Address, e.Data))
            {
                _comm1CockpitChannel = _comm1DcsbiosOutputChannel.LastUIntValue;
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }

            // COMM2
            if (_comm2DcsbiosOutputFreq.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockComm2DialObject)
                {
                    _comm2CockpitFreq = _comm2DcsbiosOutputFreq.LastUIntValue;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                    Interlocked.Exchange(ref _comm2DialWaitingForFeedback, 0);
                }
            }

            if (_comm2DcsbiosOutputChannel.UIntValueHasChanged(e.Address, e.Data))
            {
                _comm2CockpitChannel = _comm2DcsbiosOutputChannel.LastUIntValue;
                Interlocked.Increment(ref _doUpdatePanelLCD);
            }

            // VHF FM

            // ILS
            if (_ilsDcsbiosOutputChannel.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockIlsDialsObject)
                {
                    _ilsCockpitChannel = _ilsDcsbiosOutputChannel.LastUIntValue + 1;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                    Interlocked.Exchange(ref _ilsDialWaitingForFeedback, 0);
                }
            }

            // TACAN is set via String listener

            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                // Common.DebugP("RadioPanelPZ69FA18C Received DCSBIOS stringData : ->" + e.StringData + "<-");
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    // Common.DebugP("Received DCSBIOS stringData : " + e.StringData);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsFA18C knob)
        {
            // Send changes to DCSbios when press the UPPER_FREQ_SWITCH or LOWER_FREQ_SWITCH
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH))
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

        private void SendILSToDCSBIOS()
        {
            if (IlsNowSyncing())
            {
                return;
            }

            SaveCockpitFrequencyIls();

            _shutdownILSThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownILSThread = false;
            _ilsSyncThread = new Thread(() => ILSSynchThreadMethod(_ilsChannelStandby));
            _ilsSyncThread.Start();
        }

        private volatile bool _shutdownILSThread;
        private void ILSSynchThreadMethod(uint standbyPosition)
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _ilsThreadNowSyncing, 1);

                    long dialTimeout = DateTime.Now.Ticks;
                    long dialOkTime = 0;
                    int dialSendCount = 0;

                    do
                    {
                        if (IsTimedOut(ref dialTimeout))
                        {
                            ResetWaitingForFeedBack(ref _ilsDialWaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _ilsDialWaitingForFeedback) == 0)
                        {
                            lock (_lockIlsDialsObject)
                            {
                                if (_ilsCockpitChannel < standbyPosition)
                                {
                                    dialOkTime = DateTime.Now.Ticks;
                                    const string str = ILS_CHANNEL_COMMAND + DCSBIOS_INCREASE_COMMAND;
                                    DCSBIOS.Send(str);
                                    dialSendCount++;
                                    Interlocked.Exchange(ref _ilsDialWaitingForFeedback, 1);
                                }
                                else if (_ilsCockpitChannel > standbyPosition)
                                {
                                    dialOkTime = DateTime.Now.Ticks;
                                    const string str = ILS_CHANNEL_COMMAND + DCSBIOS_DECREASE_COMMAND;
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
                            // "Race" condition detected?
                            dialSendCount = 0;

                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while (IsTooShort(dialOkTime) && !_shutdownILSThread);

                    SwapCockpitStandbyFrequencyIls();
                    ShowFrequenciesOnPanel();
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _ilsThreadNowSyncing, 0);
            }

            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private string GetCOM1FrequencyAsString()
        {
            uint integerCOMM1 = _comm1CockpitFreq / 100;
            uint decimalCOMM1 = _comm1CockpitFreq - (integerCOMM1 * 100);
            return $"{integerCOMM1}.{decimalCOMM1}";
        }

        private string GetCOM2FrequencyAsString()
        {
            uint integerCOMM2 = _comm2CockpitFreq / 100;
            uint decimalCOMM2 = _comm2CockpitFreq - (integerCOMM2 * 100);
            return $"{integerCOMM2}.{decimalCOMM2}";
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

                // UPPER PANEL

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                switch (_currentUpperRadioMode)
                {
                    case CurrentFA18CRadioMode.COMM1:
                        {
                            // show comm1 frequencies in upper panel
                            string frequencyAsString = GetCOM1FrequencyAsString();

                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _comm1CockpitChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }

                            break;
                        }

                    case CurrentFA18CRadioMode.COMM2:
                        {
                            // show comm2 frequencies in upper panel
                            string frequencyAsString = GetCOM2FrequencyAsString();

                            if (_upperButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _comm2CockpitChannel, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            }

                            break;
                        }

                    case CurrentFA18CRadioMode.VHFFM:
                        {
                            // clear displays
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
                            uint ilsChannel;
                            lock (_lockIlsDialsObject)
                            {
                                ilsChannel = _ilsCockpitChannel;
                            }

                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, ilsChannel, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, _ilsChannelStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentFA18CRadioMode.TACAN:
                        {
                            // clear displays
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
                    // LOWER PANEL

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    case CurrentFA18CRadioMode.COMM1:
                        {
                            // show comm1 frequencies in lower panel
                            string frequencyAsString = GetCOM1FrequencyAsString();

                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _comm1CockpitChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }

                            break;
                        }

                    case CurrentFA18CRadioMode.COMM2:
                        {
                            // show comm2 frequencies in lower panel
                            string frequencyAsString = GetCOM2FrequencyAsString();

                            if (_lowerButtonPressed)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }
                            else
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _comm2CockpitChannel, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            }

                            break;
                        }

                    case CurrentFA18CRadioMode.VHFFM:
                        {
                            // clear displays
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);

                            break;
                        }

                    case CurrentFA18CRadioMode.ILS:
                        {
                            uint ilsChannel;
                            lock (_lockIlsDialsObject)
                            {
                                ilsChannel = _ilsCockpitChannel;
                            }

                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, ilsChannel, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, _ilsChannelStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentFA18CRadioMode.TACAN:
                        {
                            // clear displays
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);

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
                                            DCSBIOS.Send(COMM1_CHANNEL_INC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_CHANNEL_INC);
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
                                            DCSBIOS.Send(COMM1_CHANNEL_DEC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_CHANNEL_DEC);
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
                                            DCSBIOS.Send(COMM1_VOL_INC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_INC);
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
                                            DCSBIOS.Send(COMM1_VOL_DEC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_DEC);
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
                                            DCSBIOS.Send(COMM1_CHANNEL_INC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_CHANNEL_INC);
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

                        case RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentFA18CRadioMode.COMM1:
                                        {
                                            DCSBIOS.Send(COMM1_CHANNEL_DEC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_CHANNEL_DEC);
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
                                            DCSBIOS.Send(COMM1_VOL_INC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_INC);
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
                                            DCSBIOS.Send(COMM1_VOL_DEC);
                                            break;
                                        }

                                    case CurrentFA18CRadioMode.COMM2:
                                        {
                                            DCSBIOS.Send(COMM2_VOL_DEC);
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
            // Crude fix if any freqs are outside the valid boundaries

            // VHF FM
            // 30.000 - 76.000Mhz

            // COMM2
            // 225.000 - 399.975 MHz
            if (_comm2BigFrequencyStandby < 225)
            {
                _comm2BigFrequencyStandby = 225;
            }

            if (_comm2BigFrequencyStandby > 399)
            {
                _comm2BigFrequencyStandby = 399;
            }

            // ILS
            // ils channels 1 to 20
            if (_ilsChannelStandby < 1)
            {
                _ilsChannelStandby = 1;
            }

            if (_ilsChannelStandby > 20)
            {
                _ilsChannelStandby = 20;
            }

            // TACAN
            // 00X/Y - 129X/Y
        }

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
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
                                    DCSBIOS.Send(_upperButtonPressed ? COMM1_PULL_PRESS : COMM1_PULL_RELEASE);
                                }

                                if (_currentUpperRadioMode == CurrentFA18CRadioMode.COMM2)
                                {
                                    DCSBIOS.Send(_upperButtonPressed ? COMM2_PULL_PRESS : COMM2_PULL_RELEASE);
                                }

                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_upperButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Do when user releases button
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
                                    DCSBIOS.Send(_lowerButtonPressed ? COMM1_PULL_PRESS : COMM1_PULL_RELEASE);
                                }

                                if (_currentLowerRadioMode == CurrentFA18CRadioMode.COMM2)
                                {
                                    DCSBIOS.Send(_lowerButtonPressed ? COMM2_PULL_PRESS : COMM2_PULL_RELEASE);
                                }

                                if (!radioPanelKnob.IsOn)
                                {
                                    if (!_lowerButtonPressedAndDialRotated)
                                    {
                                        // Do not synch if user has pressed the button to configure the radio
                                        // Do when user releases button
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH);
                                    }

                                    _lowerButtonPressedAndDialRotated = false;
                                }
                                break;
                            }
                    }

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(
                            DCSAircraft.SelectedAircraft.Description,
                            HIDInstance,
                            PluginGamingPanelEnum.PZ69RadioPanel_PreProg_FA18C,
                            (int)radioPanelKnob.RadioPanelPZ69Knob,
                            radioPanelKnob.IsOn,
                            null);
                    }
                }
                AdjustFrequency(hashSet);
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
            SaitekPanelKnobs = RadioPanelKnobFA18C.GetRadioPanelKnobs();
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

        private bool IlsNowSyncing()
        {
            return Interlocked.Read(ref _ilsThreadNowSyncing) > 0;
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }
    }
}
