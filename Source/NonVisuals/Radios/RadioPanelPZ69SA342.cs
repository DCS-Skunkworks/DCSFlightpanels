using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using NonVisuals.Helpers;




    /// <summary>
    /// Pre-programmed radio panel for the SA342. 
    /// </summary>
    public class RadioPanelPZ69SA342 : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentSA342RadioMode
        {
            VHFFM,
            VHFAM,
            UHF,
            ADF,
            NADIR,
            NO_USE
        }

        private CurrentSA342RadioMode _currentUpperRadioMode = CurrentSA342RadioMode.VHFAM;
        private CurrentSA342RadioMode _currentLowerRadioMode = CurrentSA342RadioMode.VHFAM;

        // 118.175
        private enum VhfAmDigit
        {
            First,
            Second,
            Third,
            Fourth,
            LastTwoSpecial
        }

        private enum AdfDigit
        {
            Digit100S,
            Digit10S,
            Digits1S
        }

        /*COM1 SA342 VHF AM Radio*/
        // Large dial 118-143
        // Small dial 0-975
        private readonly int[] _dialPositionsWholeNumbers = { 0, 6553, 13107, 19660, 26214, 32767, 39321, 45874, 52428, 58981 };
        private readonly int[] _dialPositionsDecial100S = { 0, 16383, 32767, 49151 };
        private double _vhfAmBigFrequencyStandby = 118;
        private double _vhfAmSmallFrequencyStandby;
        private double _vhfAmSavedCockpitBigFrequency;
        private double _vhfAmSavedCockpitSmallFrequency;
        private DCSBIOSOutput _vhfAmDcsbiosOutputReading10S; // 1[1]8.375
        private DCSBIOSOutput _vhfAmDcsbiosOutputReading1S; // 11[8].375
        private DCSBIOSOutput _vhfAmDcsbiosOutputReadingDecimal10S; // 118.[3]75
        private DCSBIOSOutput _vhfAmDcsbiosOutputReadingDecimal100S; // 118.3[75]
        private const string VHF_AM_LEFT_DIAL_DIAL_COMMAND_INC = "AM_RADIO_FREQUENCY_DIAL_LEFT +3200\n";
        private const string VHF_AM_LEFT_DIAL_DIAL_COMMAND_DEC = "AM_RADIO_FREQUENCY_DIAL_LEFT -3200\n";
        private const string VHF_AM_RIGHT_DIAL_DIAL_COMMAND_INC = "AM_RADIO_FREQUENCY_DIAL_RIGHT +3200\n";
        private const string VHF_AM_RIGHT_DIAL_DIAL_COMMAND_DEC = "AM_RADIO_FREQUENCY_DIAL_RIGHT -3200\n";
        private readonly object _lockVhfAm10SObject = new();
        private readonly object _lockVhfAm1SObject = new();
        private readonly object _lockVhfAmDecimal10SObject = new();
        private readonly object _lockVhfAmDecimal100SObject = new();
        private volatile uint _vhfAmCockpit10SFrequencyValue = 6553;
        private volatile uint _vhfAmCockpit1SFrequencyValue = 6553;
        private volatile uint _vhfAmCockpitDecimal10SFrequencyValue = 6553;
        private volatile uint _vhfAmCockpitDecimal100SFrequencyValue = 6553;

        private Thread _vhfAmSyncThread;
        private long _vhfAmThreadNowSynching;
        private long _vhfAmValue1WaitingForFeedback; // 10s
        private long _vhfAmValue2WaitingForFeedback; // 1s
        private long _vhfAmValue3WaitingForFeedback; // Decimal 10s
        private long _vhfAmValue4WaitingForFeedback; // Decimal 100s
        private readonly ClickSkipper _vhfAmLeftDialSkipper = new(2);
        private readonly ClickSkipper _vhfAmRightDialSkipper = new(2);
        private volatile bool _shutdownVHFAMThread;

        /*COM2 SA342 FM PR4G Radio*/
        // Large dial 0-7 Presets 1, 2, 3, 4, 5, 6, 0, RG
        // Small dial 
        private DCSBIOSOutput _fmRadioPresetDcsbiosOutput;
        private volatile uint _fmRadioPresetCockpitDialPos = 1;
        private const string FM_RADIO_PRESET_COMMAND_INC = "FM_RADIO_CHANNEL INC\n";
        private const string FM_RADIO_PRESET_COMMAND_DEC = "FM_RADIO_CHANNEL DEC\n";
        private readonly object _lockFmRadioPresetObject = new();

        /*NAV1 SA342 UHF Radio*/
        // Large dial 225-399
        // Small dial 000-975 where only 2 digits can be used
        private readonly ClickSpeedDetector _uhfBigFreqIncreaseChangeMonitor = new(20);
        private readonly ClickSpeedDetector _uhfBigFreqDecreaseChangeMonitor = new(20);
        private readonly ClickSpeedDetector _uhfSmallFreqIncreaseChangeMonitor = new(20);
        private readonly ClickSpeedDetector _uhfSmallFreqDecreaseChangeMonitor = new(20);
        private double _uhfBigFrequencyStandby = 225;
        private double _uhfSmallFrequencyStandby;
        private const string UHF_BUTTON0_COMMAND_ON = "UHF_RADIO_BUTTON_0 1\n";
        private const string UHF_BUTTON0_COMMAND_OFF = "UHF_RADIO_BUTTON_0 0\n";
        private const string UHF_BUTTON1_COMMAND_ON = "UHF_RADIO_BUTTON_1 1\n";
        private const string UHF_BUTTON1_COMMAND_OFF = "UHF_RADIO_BUTTON_1 0\n";
        private const string UHF_BUTTON2_COMMAND_ON = "UHF_RADIO_BUTTON_2 1\n";
        private const string UHF_BUTTON2_COMMAND_OFF = "UHF_RADIO_BUTTON_2 0\n";
        private const string UHF_BUTTON3_COMMAND_ON = "UHF_RADIO_BUTTON_3 1\n";
        private const string UHF_BUTTON3_COMMAND_OFF = "UHF_RADIO_BUTTON_3 0\n";
        private const string UHF_BUTTON4_COMMAND_ON = "UHF_RADIO_BUTTON_4 1\n";
        private const string UHF_BUTTON4_COMMAND_OFF = "UHF_RADIO_BUTTON_4 0\n";
        private const string UHF_BUTTON5_COMMAND_ON = "UHF_RADIO_BUTTON_5 1\n";
        private const string UHF_BUTTON5_COMMAND_OFF = "UHF_RADIO_BUTTON_5 0\n";
        private const string UHF_BUTTON6_COMMAND_ON = "UHF_RADIO_BUTTON_6 1\n";
        private const string UHF_BUTTON6_COMMAND_OFF = "UHF_RADIO_BUTTON_6 0\n";
        private const string UHF_BUTTON7_COMMAND_ON = "UHF_RADIO_BUTTON_7 1\n";
        private const string UHF_BUTTON7_COMMAND_OFF = "UHF_RADIO_BUTTON_7 0\n";
        private const string UHF_BUTTON8_COMMAND_ON = "UHF_RADIO_BUTTON_8 1\n";
        private const string UHF_BUTTON8_COMMAND_OFF = "UHF_RADIO_BUTTON_8 0\n";
        private const string UHF_BUTTON9_COMMAND_ON = "UHF_RADIO_BUTTON_9 1\n";
        private const string UHF_BUTTON9_COMMAND_OFF = "UHF_RADIO_BUTTON_9 0\n";
        private const string UHF_BUTTON_VALIDATE_COMMAND_ON = "UHF_RADIO_BUTTON_VLD 1\n";
        private const string UHF_BUTTON_VALIDATE_COMMAND_OFF = "UHF_RADIO_BUTTON_VLD 0\n";
        private readonly ClickSkipper _uhfBigFrequencySkipper = new(2);
        private readonly ClickSkipper _uhfSmallFrequencySkipper = new(2);

        /*ADF SA342*/
        /*Large dial Counter Clockwise 100s increase*/
        /*Large dial Clockwise 10s increase*/
        /*Small dial 1s and decimals*/
        private const string ADF1_UNIT100_S_INCREASE = "ADF_NAV1_100 +3200\n";
        private const string ADF1_UNIT10_S_INCREASE = "ADF_NAV1_10 +3200\n";
        private const string ADF1_UNIT1_S_DECIMALS_INCREASE = "ADF_NAV1_1 +3200\n";
        private const string ADF1_UNIT1_S_DECIMALS_DECREASE = "ADF_NAV1_1 -3200\n";
        private const string ADF2_UNIT100_S_INCREASE = "ADF_NAV2_100 +3200\n";
        private const string ADF2_UNIT10_S_INCREASE = "ADF_NAV2_10 +3200\n";
        private const string ADF2_UNIT1_S_DECIMALS_INCREASE = "ADF_NAV2_1 +3200\n";
        private const string ADF2_UNIT1_S_DECIMALS_DECREASE = "ADF_NAV2_1 -3200\n";
        private const string ADF_SWITCH_UNIT_COMMAND = "ADF1_ADF2_SELECT TOGGLE\n";
        private readonly object _lockAdfUnitObject = new();
        private volatile uint _adfCockpitSelectedUnitValue = 1;
        private DCSBIOSOutput _adfSwitchUnitDcsbiosOutput;
        private readonly ClickSkipper _adf100SDialSkipper = new(2);
        private readonly ClickSkipper _adf10SDialSkipper = new(2);
        private readonly ClickSkipper _adf1SDialSkipper = new(2);

        // DME NADIR
        // Large dial Mode selector (VENT, C.M DEC, V.S DER, TPS CAP,P.P, BUT)
        // Small dial Doppler modes ARRET, VEILLE, TERRE, MER, ANEMO,TEST SOL.
        // Large
        private const string NADIR_MODE_COMMAND_INC = "NADIR_PARAMETER INC\n";
        private const string NADIR_MODE_COMMAND_DEC = "NADIR_PARAMETER DEC\n";

        // Small
        private const string NADIR_DOPPLER_COMMAND_INC = "NADIR_DOPPLER_MODE INC\n";
        private const string NADIR_DOPPLER_COMMAND_DEC = "NADIR_DOPPLER_MODE DEC\n";
        private volatile uint _nadirModeCockpitValue;
        private volatile uint _nadirDopplerModeCockpitValue;
        private DCSBIOSOutput _nadirModeDcsbiosOutput;
        private DCSBIOSOutput _nadirDopplerModeDcsbiosOutput;
        private readonly object _lockNADIRUnitObject = new();
        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69SA342(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {}

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownVHFAMThread = true;
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

            // VHF AM
            _vhfAmDcsbiosOutputReading10S = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("AM_RADIO_FREQ_10s");
            _vhfAmDcsbiosOutputReading1S = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("AM_RADIO_FREQ_1s");
            _vhfAmDcsbiosOutputReadingDecimal10S = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("AM_RADIO_FREQ_TENTHS");
            _vhfAmDcsbiosOutputReadingDecimal100S = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("AM_RADIO_FREQ_HUNDREDTHS");

            // FM PR4G
            _fmRadioPresetDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("FM_RADIO_CHANNEL");

            // ADF
            _adfSwitchUnitDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("ADF1_ADF2_SELECT");

            // DME
            _nadirModeDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("NADIR_PARAMETER");
            _nadirDopplerModeDcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("NADIR_DOPPLER_MODE");

            BIOSEventHandler.AttachDataListener(this);
            StartListeningForHidPanelChanges();
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

            // VHF AM
            if (_vhfAmDcsbiosOutputReading10S.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVhfAm10SObject)
                {
                    // When dialing this radio a lot of intermediate (incorrect) raw values are sent. Only trap
                    // know raw values as in the member array _dialPositions
                    if (CorrectPositionWholeNumbers(_vhfAmDcsbiosOutputReading10S.LastUIntValue))
                    {
                        _vhfAmCockpit10SFrequencyValue = _vhfAmDcsbiosOutputReading10S.LastUIntValue;
                        Interlocked.Exchange(ref _vhfAmValue1WaitingForFeedback, 0);
                    }
                }
            }

            if (_vhfAmDcsbiosOutputReading1S.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVhfAm1SObject)
                {
                    if (CorrectPositionWholeNumbers(_vhfAmDcsbiosOutputReading1S.LastUIntValue))
                    {
                        _vhfAmCockpit1SFrequencyValue = _vhfAmDcsbiosOutputReading1S.LastUIntValue;
                        Interlocked.Exchange(ref _vhfAmValue2WaitingForFeedback, 0);
                    }
                }
            }

            if (_vhfAmDcsbiosOutputReadingDecimal10S.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVhfAmDecimal10SObject)
                {
                    if (CorrectPositionWholeNumbers(_vhfAmDcsbiosOutputReadingDecimal10S.LastUIntValue))
                    {
                        _vhfAmCockpitDecimal10SFrequencyValue = _vhfAmDcsbiosOutputReadingDecimal10S.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _vhfAmValue3WaitingForFeedback, 0);
                    }
                }
            }

            if (_vhfAmDcsbiosOutputReadingDecimal100S.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockVhfAmDecimal100SObject)
                {
                    if (CorrectPositionDecimal100S(_vhfAmDcsbiosOutputReadingDecimal100S.LastUIntValue))
                    {
                        _vhfAmCockpitDecimal100SFrequencyValue = _vhfAmDcsbiosOutputReadingDecimal100S.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                        Interlocked.Exchange(ref _vhfAmValue4WaitingForFeedback, 0);
                    }
                }
            }

            // VHF FM PR4G
            if (_fmRadioPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockFmRadioPresetObject)
                {
                    _fmRadioPresetCockpitDialPos = _fmRadioPresetDcsbiosOutput.LastUIntValue;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // ADF
            if (_adfSwitchUnitDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockAdfUnitObject)
                {
                    _adfCockpitSelectedUnitValue = _adfSwitchUnitDcsbiosOutput.LastUIntValue;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // NADIR Mode
            if (_nadirModeDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockNADIRUnitObject)
                {
                    _nadirModeCockpitValue = _nadirModeDcsbiosOutput.LastUIntValue;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // NADIR Doppler Mode
            if (_nadirDopplerModeDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
            {
                lock (_lockNADIRUnitObject)
                {
                    _nadirDopplerModeCockpitValue = _nadirDopplerModeDcsbiosOutput.LastUIntValue;
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                }
            }

            // Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
        }

        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsSA342 knob)
        {
            if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH))
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
                case RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentSA342RadioMode.VHFAM:
                                {
                                    SendVhfAmToDCSBIOS();
                                    break;
                                }

                            case CurrentSA342RadioMode.VHFFM:
                                {
                                    break;
                                }

                            case CurrentSA342RadioMode.UHF:
                                {
                                    SendUhfToDCSBIOS();
                                    break;
                                }

                            case CurrentSA342RadioMode.ADF:
                                {
                                    DCSBIOS.Send(ADF_SWITCH_UNIT_COMMAND);
                                    break;
                                }
                        }

                        break;
                    }

                case RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentSA342RadioMode.VHFAM:
                                {
                                    SendVhfAmToDCSBIOS();
                                    break;
                                }

                            case CurrentSA342RadioMode.VHFFM:
                                {
                                    break;
                                }

                            case CurrentSA342RadioMode.UHF:
                                {
                                    SendUhfToDCSBIOS();
                                    break;
                                }

                            case CurrentSA342RadioMode.ADF:
                                {
                                    DCSBIOS.Send(ADF_SWITCH_UNIT_COMMAND);
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

            // 118.950
            // First digit is always 1, no need to do anything about it.
            var desiredPositionDialWholeNumbers = int.Parse(frequencyAsString.Substring(1, 2));
            var desiredPositionDecimals = frequencyAsString.Length < 7 ? int.Parse(frequencyAsString.Substring(4, 2) + "0") : int.Parse(frequencyAsString.Substring(4, 3));

            // #1
            _shutdownVHFAMThread = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            _shutdownVHFAMThread = false;
            _vhfAmSyncThread = new Thread(() => VhfAmSynchThreadMethod(desiredPositionDialWholeNumbers, desiredPositionDecimals));
            _vhfAmSyncThread.Start();
        }

        private void VhfAmSynchThreadMethod(int desiredPositionDialWholeNumbers, int desiredPositionDialDecimals)
        {
            try
            {
                try
                {
                    /*
                    * COM1 VHF AM
                    * 
                    */
                    Interlocked.Exchange(ref _vhfAmThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial1Time = 0;
                    long dial2Time = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    do
                    {
                        if (IsTimedOut(ref dial1Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfAmValue1WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (IsTimedOut(ref dial2Timeout))
                        {
                            ResetWaitingForFeedBack(ref _vhfAmValue2WaitingForFeedback); // Lets do an ugly reset
                        }

                        if (Interlocked.Read(ref _vhfAmValue1WaitingForFeedback) == 0 || Interlocked.Read(ref _vhfAmValue2WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfAm10SObject)
                            {
                                lock (_lockVhfAm1SObject)
                                {
                                    var frequencyWholeNumbers = GetVhfAmDialFrequencyFromRawValue(0, _vhfAmCockpit10SFrequencyValue) + string.Empty
                                                                + GetVhfAmDialFrequencyFromRawValue(0, _vhfAmCockpit1SFrequencyValue);
                                    if (int.Parse(frequencyWholeNumbers) != desiredPositionDialWholeNumbers)
                                    {
                                        var command = string.Empty;
                                        if (int.Parse(frequencyWholeNumbers) < desiredPositionDialWholeNumbers)
                                        {
                                            command = VHF_AM_LEFT_DIAL_DIAL_COMMAND_INC;
                                        }

                                        if (int.Parse(frequencyWholeNumbers) > desiredPositionDialWholeNumbers)
                                        {
                                            command = VHF_AM_LEFT_DIAL_DIAL_COMMAND_DEC;
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
                            lock (_lockVhfAmDecimal10SObject)
                            {
                                lock (_lockVhfAmDecimal100SObject)
                                {
                                    var cockpitFrequencyDecimals = GetVhfAmDialFrequencyFromRawValue(0, _vhfAmCockpitDecimal10SFrequencyValue) + string.Empty
                                                                   + GetVhfAmDialFrequencyFromRawValue(1, _vhfAmCockpitDecimal100SFrequencyValue);
                                    if (int.Parse(cockpitFrequencyDecimals) != desiredPositionDialDecimals)
                                    {
                                        /*Debug.Print("cockpit frequencyDecimals = " + int.Parse(frequencyDecimals));
                                        Debug.Print("desiredPositionDialDecimals = " + desiredPositionDialDecimals);
                                        Debug.Print("cockpit _vhfAmCockpitDecimal10sFrequencyValue RAW = " + _vhfAmCockpitDecimal10sFrequencyValue);
                                        Debug.Print("cockpit _vhfAmCockpitDecimal100sFrequencyValue RAW = " + _vhfAmCockpitDecimal100sFrequencyValue);*/
                                        DCSBIOS.Send(
                                            SwitchVhfAmDecimalDirectionUp(int.Parse(cockpitFrequencyDecimals), desiredPositionDialDecimals)
                                                ? VHF_AM_RIGHT_DIAL_DIAL_COMMAND_INC
                                                : VHF_AM_RIGHT_DIAL_DIAL_COMMAND_DEC);
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
                            // "Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                    }
                    while ((IsTooShort(dial1Time) || IsTooShort(dial2Time)) && !_shutdownVHFAMThread);

                    SwapCockpitStandbyFrequencyVhfAm();
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
                Interlocked.Exchange(ref _vhfAmThreadNowSynching, 0);
            }
            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SendUhfToDCSBIOS()
        {
            // "399.950" [7]
            // "399.95" [6]
            var frequencyAsString = (_uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(2, '0')).PadRight(6, '0');
            const int sleepLength = 100;
            foreach (char c in frequencyAsString)
            {
                Debug.Print("CHAR IS " + c);
                switch (c)
                {
                    case '0':
                        {
                            // Debug.Print("Sending 0 ");
                            DCSBIOS.Send(UHF_BUTTON0_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON0_COMMAND_OFF);
                            break;
                        }

                    case '1':
                        {
                            // Debug.Print("Sending 1 ");
                            DCSBIOS.Send(UHF_BUTTON1_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON1_COMMAND_OFF);
                            break;
                        }

                    case '2':
                        {
                            // Debug.Print("Sending 2 ");
                            DCSBIOS.Send(UHF_BUTTON2_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON2_COMMAND_OFF);
                            break;
                        }

                    case '3':
                        {
                            // Debug.Print("Sending 3 ");
                            DCSBIOS.Send(UHF_BUTTON3_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON3_COMMAND_OFF);
                            break;
                        }

                    case '4':
                        {
                            // Debug.Print("Sending 4 ");
                            DCSBIOS.Send(UHF_BUTTON4_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON4_COMMAND_OFF);
                            break;
                        }

                    case '5':
                        {
                            // Debug.Print("Sending 5 ");
                            DCSBIOS.Send(UHF_BUTTON5_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON5_COMMAND_OFF);
                            break;
                        }

                    case '6':
                        {
                            // Debug.Print("Sending 6 ");
                            DCSBIOS.Send(UHF_BUTTON6_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON6_COMMAND_OFF);
                            break;
                        }

                    case '7':
                        {
                            // Debug.Print("Sending 7 ");
                            DCSBIOS.Send(UHF_BUTTON7_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON7_COMMAND_OFF);
                            break;
                        }

                    case '8':
                        {
                            // Debug.Print("Sending 8 ");
                            DCSBIOS.Send(UHF_BUTTON8_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON8_COMMAND_OFF);
                            break;
                        }

                    case '9':
                        {
                            // Debug.Print("Sending 9 ");
                            DCSBIOS.Send(UHF_BUTTON9_COMMAND_ON);
                            Thread.Sleep(sleepLength);
                            DCSBIOS.Send(UHF_BUTTON9_COMMAND_OFF);
                            break;
                        }
                }
            }

            if (frequencyAsString.Length == 6)
            {
                // Debug.Print("Sending 0 ");
                DCSBIOS.Send(UHF_BUTTON0_COMMAND_ON);
                Thread.Sleep(sleepLength);
                DCSBIOS.Send(UHF_BUTTON0_COMMAND_OFF);
            }

            // Debug.Print("Sending VALIDATE ");
            DCSBIOS.Send(UHF_BUTTON_VALIDATE_COMMAND_ON);
            Thread.Sleep(sleepLength);
            DCSBIOS.Send(UHF_BUTTON_VALIDATE_COMMAND_OFF);
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
                    case CurrentSA342RadioMode.VHFAM:
                        {
                            if (VhfAmNowSyncing())
                            {
                                return;
                            }

                            // Frequency selector 1      VHFAM_FREQ1
                            // 1

                            // Frequency selector 2      VHFAM_FREQ2
                            // 1-4

                            // Frequency selector 3      VHFAM_FREQ3
                            // 0-9

                            // Frequency selector 4      VHFAM_FREQ4
                            // "00" "25" "50" "75", only "00" and "50" used.
                            // Pos     0    1    2    3
                            string frequencyAsString;
                            lock (_lockVhfAm10SObject)
                            {
                                frequencyAsString = "1" + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Second, _vhfAmCockpit10SFrequencyValue);
                            }

                            lock (_lockVhfAm1SObject)
                            {
                                frequencyAsString += GetVhfAmDialFrequencyForPosition(VhfAmDigit.Third, _vhfAmCockpit1SFrequencyValue);
                            }

                            frequencyAsString += ".";
                            lock (_lockVhfAmDecimal10SObject)
                            {
                                frequencyAsString += GetVhfAmDialFrequencyForPosition(VhfAmDigit.Fourth, _vhfAmCockpitDecimal10SFrequencyValue);
                            }

                            lock (_lockVhfAmDecimal100SObject)
                            {
                                frequencyAsString += GetVhfAmDialFrequencyForPosition(VhfAmDigit.LastTwoSpecial, _vhfAmCockpitDecimal100SFrequencyValue);
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);

                            // Debug.Print("_vhfAmBigFrequencyStandby " + _vhfAmBigFrequencyStandby);
                            // Debug.Print("_vhfAmSmallFrequencyStandby " + _vhfAmSmallFrequencyStandby);
                            SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.VHFFM:
                        {
                            // Presets
                            // 0 - 8
                            uint preset;
                            lock (_lockFmRadioPresetObject)
                            {
                                preset = _fmRadioPresetCockpitDialPos + 1;
                            }

                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, preset, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.UHF:
                        {
                            /*NAV1 SA342 UHF Radio*/
                            // Large dial 225-399
                            // Small dial 000-975 where only 2 digits can be used
                            var frequencyAsString = (_uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(2, '0')).PadRight(6, '0');
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.ADF:
                        {
                            uint tmpValue;
                            lock (_adfSwitchUnitDcsbiosOutput)
                            {
                                tmpValue = _adfCockpitSelectedUnitValue + 1;
                            }

                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, tmpValue, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.NADIR:
                        {
                            uint tmpValueMode;
                            uint tmpValueDopper;
                            lock (_lockNADIRUnitObject)
                            {
                                tmpValueMode = _nadirModeCockpitValue + 1;
                                tmpValueDopper = _nadirDopplerModeCockpitValue + 1;
                            }

                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, tmpValueMode, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, tmpValueDopper, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.NO_USE:
                        {
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            break;
                        }
                }

                switch (_currentLowerRadioMode)
                {
                    case CurrentSA342RadioMode.VHFAM:
                        {
                            if (VhfAmNowSyncing())
                            {
                                return;
                            }

                            // Frequency selector 1      VHFAM_FREQ1
                            // 1

                            // Frequency selector 2      VHFAM_FREQ2
                            // 1-4

                            // Frequency selector 3      VHFAM_FREQ3
                            // 0-9

                            // Frequency selector 4      VHFAM_FREQ4
                            // "00" "25" "50" "75", only "00" and "50" used.
                            // Pos     0    1    2    3
                            string frequencyAsString;
                            lock (_lockVhfAm10SObject)
                            {
                                frequencyAsString = "1" + GetVhfAmDialFrequencyForPosition(VhfAmDigit.Second, _vhfAmCockpit10SFrequencyValue);
                            }

                            lock (_lockVhfAm1SObject)
                            {
                                frequencyAsString += GetVhfAmDialFrequencyForPosition(VhfAmDigit.Third, _vhfAmCockpit1SFrequencyValue);
                            }

                            frequencyAsString += ".";
                            lock (_lockVhfAmDecimal10SObject)
                            {
                                frequencyAsString += GetVhfAmDialFrequencyForPosition(VhfAmDigit.Fourth, _vhfAmCockpitDecimal10SFrequencyValue);
                            }

                            lock (_lockVhfAmDecimal100SObject)
                            {
                                frequencyAsString += GetVhfAmDialFrequencyForPosition(VhfAmDigit.LastTwoSpecial, _vhfAmCockpitDecimal100SFrequencyValue);
                            }

                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, _vhfAmBigFrequencyStandby + _vhfAmSmallFrequencyStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.VHFFM:
                        {
                            // Presets
                            // 0 - 8
                            uint preset;
                            lock (_lockFmRadioPresetObject)
                            {
                                preset = _fmRadioPresetCockpitDialPos + 1;
                            }

                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, preset, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.UHF:
                        {
                            /*NAV1 SA342 UHF Radio*/
                            // Large dial 225-399
                            // Small dial 000-975 where only 2 digits can be used
                            var frequencyAsString = (_uhfBigFrequencyStandby + "." + _uhfSmallFrequencyStandby.ToString().PadLeft(2, '0')).PadRight(6, '0');
                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.ADF:
                        {
                            uint tmpValue;
                            lock (_adfSwitchUnitDcsbiosOutput)
                            {
                                tmpValue = _adfCockpitSelectedUnitValue + 1;
                            }

                            SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, tmpValue, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.NADIR:
                        {
                            uint tmpValueMode;
                            uint tmpValueDopper;
                            lock (_lockNADIRUnitObject)
                            {
                                tmpValueMode = _nadirModeCockpitValue + 1;
                                tmpValueDopper = _nadirDopplerModeCockpitValue + 1;
                            }

                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, tmpValueMode, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, tmpValueDopper, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            break;
                        }

                    case CurrentSA342RadioMode.NO_USE:
                        {
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
                var radioPanelKnobSA342 = (RadioPanelKnobSA342)o;
                if (radioPanelKnobSA342.IsOn)
                {
                    switch (radioPanelKnobSA342.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmLeftDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmBigFrequencyStandby++;
                                                if (_vhfAmBigFrequencyStandby > 143.00)
                                                {
                                                    _vhfAmBigFrequencyStandby = 118;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            DCSBIOS.Send(FM_RADIO_PRESET_COMMAND_INC);
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfBigFreqIncreaseChangeMonitor.Click();
                                            if (_uhfBigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfBigFrequencyStandby += 5;
                                            }
                                            else
                                            {
                                                if (!_uhfBigFrequencySkipper.ShouldSkip())
                                                {
                                                    _uhfBigFrequencyStandby++;
                                                }
                                            }

                                            if (_uhfBigFrequencyStandby > 399)
                                            {
                                                _uhfBigFrequencyStandby = 225;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf10SDialSkipper.Click(GetAdfCommand(AdfDigit.Digit10S, true));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_MODE_COMMAND_INC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmLeftDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmBigFrequencyStandby--;
                                                if (_vhfAmBigFrequencyStandby < 118.00)
                                                {
                                                    _vhfAmBigFrequencyStandby = 143;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            DCSBIOS.Send(FM_RADIO_PRESET_COMMAND_DEC);
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfBigFreqDecreaseChangeMonitor.Click();
                                            if (_uhfBigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfBigFrequencyStandby -= 5;
                                            }
                                            else
                                            {
                                                if (!_uhfBigFrequencySkipper.ShouldSkip())
                                                {
                                                    _uhfBigFrequencyStandby--;
                                                }
                                            }

                                            if (_uhfBigFrequencyStandby < 225)
                                            {
                                                _uhfBigFrequencyStandby = 399;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf100SDialSkipper.Click(GetAdfCommand(AdfDigit.Digit100S, true));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_MODE_COMMAND_DEC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmRightDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmSmallFrequencyStandby += 0.025;
                                                if (_vhfAmSmallFrequencyStandby > 0.975)
                                                {
                                                    // At max value
                                                    _vhfAmSmallFrequencyStandby = 0;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfSmallFreqIncreaseChangeMonitor.Click();
                                            if (_uhfSmallFreqIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfSmallFrequencyStandby += 5;
                                            }
                                            else if (!_uhfSmallFrequencySkipper.ShouldSkip())
                                            {
                                                _uhfSmallFrequencyStandby += 1;
                                            }

                                            if (_uhfSmallFrequencyStandby >= 99)
                                            {
                                                // At max value
                                                _uhfSmallFrequencyStandby = 0;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf1SDialSkipper.Click(GetAdfCommand(AdfDigit.Digits1S, true));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_DOPPLER_COMMAND_INC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmRightDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmSmallFrequencyStandby -= 0.025;
                                                if (_vhfAmSmallFrequencyStandby < 0.00)
                                                {
                                                    // At min value
                                                    _vhfAmSmallFrequencyStandby = 0.975;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfSmallFreqDecreaseChangeMonitor.Click();
                                            if (_uhfSmallFreqDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfSmallFrequencyStandby -= 5;
                                            }
                                            else if (!_uhfSmallFrequencySkipper.ShouldSkip())
                                            {
                                                _uhfSmallFrequencyStandby--;
                                            }

                                            if (_uhfSmallFrequencyStandby < 0)
                                            {
                                                // At max value
                                                _uhfSmallFrequencyStandby = 99;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf1SDialSkipper.Click(GetAdfCommand(AdfDigit.Digits1S, false));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_DOPPLER_COMMAND_DEC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmLeftDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmBigFrequencyStandby++;
                                                if (_vhfAmBigFrequencyStandby > 143.00)
                                                {
                                                    _vhfAmBigFrequencyStandby = 118;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            /*if (_vhfFmBigFrequencyStandby.Equals(76))
                                            {
                                                //@ max value
                                                break;
                                            }*/
                                            DCSBIOS.Send(FM_RADIO_PRESET_COMMAND_INC);
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfBigFreqIncreaseChangeMonitor.Click();
                                            if (_uhfBigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfBigFrequencyStandby += 5;
                                            }
                                            else
                                            {
                                                if (!_uhfBigFrequencySkipper.ShouldSkip())
                                                {
                                                    _uhfBigFrequencyStandby++;
                                                }
                                            }

                                            if (_uhfBigFrequencyStandby > 399)
                                            {
                                                _uhfBigFrequencyStandby = 225;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf10SDialSkipper.Click(GetAdfCommand(AdfDigit.Digit10S, true));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_MODE_COMMAND_INC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmLeftDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmBigFrequencyStandby--;
                                                if (_vhfAmBigFrequencyStandby < 118.00)
                                                {
                                                    _vhfAmBigFrequencyStandby = 143;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            DCSBIOS.Send(FM_RADIO_PRESET_COMMAND_DEC);
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfBigFreqDecreaseChangeMonitor.Click();
                                            if (_uhfBigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfBigFrequencyStandby -= 5;
                                            }
                                            else
                                            {
                                                if (!_uhfBigFrequencySkipper.ShouldSkip())
                                                {
                                                    _uhfBigFrequencyStandby--;
                                                }
                                            }

                                            if (_uhfBigFrequencyStandby < 225)
                                            {
                                                _uhfBigFrequencyStandby = 399;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf100SDialSkipper.Click(GetAdfCommand(AdfDigit.Digit100S, true));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_MODE_COMMAND_DEC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmRightDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmSmallFrequencyStandby += 0.025;
                                                if (_vhfAmSmallFrequencyStandby > 0.975)
                                                {
                                                    // At max value
                                                    _vhfAmSmallFrequencyStandby = 0;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfSmallFreqIncreaseChangeMonitor.Click();
                                            if (_uhfSmallFreqIncreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfSmallFrequencyStandby += 5;
                                            }
                                            else if (!_uhfSmallFrequencySkipper.ShouldSkip())
                                            {
                                                _uhfSmallFrequencyStandby++;
                                            }

                                            if (_uhfSmallFrequencyStandby >= 99)
                                            {
                                                // At max value
                                                _uhfSmallFrequencyStandby = 0;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf1SDialSkipper.Click(GetAdfCommand(AdfDigit.Digits1S, true));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_DOPPLER_COMMAND_INC);
                                            break;
                                        }
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentSA342RadioMode.VHFAM:
                                        {
                                            if (!_vhfAmRightDialSkipper.ShouldSkip())
                                            {
                                                _vhfAmSmallFrequencyStandby -= 0.025;
                                                if (_vhfAmSmallFrequencyStandby < 0.00)
                                                {
                                                    // At min value
                                                    _vhfAmSmallFrequencyStandby = 0.975;
                                                }
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.VHFFM:
                                        {
                                            break;
                                        }

                                    case CurrentSA342RadioMode.UHF:
                                        {
                                            var changeFaster = false;
                                            _uhfSmallFreqDecreaseChangeMonitor.Click();
                                            if (_uhfSmallFreqDecreaseChangeMonitor.ClickThresholdReached())
                                            {
                                                // Change faster
                                                changeFaster = true;
                                            }

                                            if (changeFaster)
                                            {
                                                _uhfSmallFrequencyStandby -= 5;
                                            }
                                            else if (!_uhfSmallFrequencySkipper.ShouldSkip())
                                            {
                                                _uhfSmallFrequencyStandby--;
                                            }

                                            if (_uhfSmallFrequencyStandby < 0)
                                            {
                                                // At max value
                                                _uhfSmallFrequencyStandby = 99;
                                            }
                                            break;
                                        }

                                    case CurrentSA342RadioMode.ADF:
                                        {
                                            _adf1SDialSkipper.Click(GetAdfCommand(AdfDigit.Digits1S, false));
                                            break;
                                        }

                                    case CurrentSA342RadioMode.NADIR:
                                        {
                                            DCSBIOS.Send(NADIR_DOPPLER_COMMAND_DEC);
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
            // VHF AM
            if (_vhfAmBigFrequencyStandby < 118)
            {
                _vhfAmBigFrequencyStandby = 118;
            }

            if (_vhfAmBigFrequencyStandby > 143)
            {
                _vhfAmBigFrequencyStandby = 143;
            }
        }

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (LockLCDUpdateObject)
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobSA342)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsSA342.UPPER_VHFAM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA342RadioMode.VHFAM;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA342RadioMode.VHFFM;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA342RadioMode.UHF;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_ADF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA342RadioMode.ADF;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_NADIR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA342RadioMode.NADIR;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.UPPER_NAV2:
                        case RadioPanelPZ69KnobsSA342.UPPER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentSA342RadioMode.NO_USE;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_VHFAM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA342RadioMode.VHFAM;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA342RadioMode.VHFFM;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA342RadioMode.UHF;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_ADF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA342RadioMode.ADF;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_NADIR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA342RadioMode.NADIR;
                                }
                                break;
                            }

                        case RadioPanelPZ69KnobsSA342.LOWER_NAV2:
                        case RadioPanelPZ69KnobsSA342.LOWER_XPDR:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentSA342RadioMode.NO_USE;
                                }
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

                    if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(
                            DCSAircraft.SelectedAircraft.Description,
                            HIDInstance,
                            PluginGamingPanelEnum.PZ69RadioPanel_PreProg_SA342,
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
            SaitekPanelKnobs = RadioPanelKnobSA342.GetRadioPanelKnobs();
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
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        return position switch
                        {
                            0 => "00",
                            16383 => "25",
                            32767 => "50",
                            49151 => "75",
                        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                    }
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
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        return position switch
                        {
                            0 => "00",
                            16383 => "25",
                            32767 => "50",
                            49151 => "75",
                        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                    }
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
            lock (_lockVhfAm10SObject)
            {
                lock (_lockVhfAm10SObject)
                {
                    lock (_lockVhfAm1SObject)
                    {
                        lock (_lockVhfAmDecimal10SObject)
                        {
                            var dial10S = GetVhfAmDialFrequencyForPosition(VhfAmDigit.Second, _vhfAmCockpit10SFrequencyValue);
                            var dial1S = GetVhfAmDialFrequencyForPosition(VhfAmDigit.Third, _vhfAmCockpit1SFrequencyValue);
                            var diald10S = GetVhfAmDialFrequencyForPosition(VhfAmDigit.Fourth, _vhfAmCockpitDecimal10SFrequencyValue);
                            var diald100S = GetVhfAmDialFrequencyForPosition(VhfAmDigit.LastTwoSpecial, _vhfAmCockpitDecimal100SFrequencyValue);

                            Debug.Print("SaveCockpitFrequencyVhfAm : 0. + diald10s + diald100s -> " + "0." + diald10S + diald100S);
                            _vhfAmSavedCockpitBigFrequency = double.Parse("1" + dial10S + dial1S, NumberFormatInfoFullDisplay);
                            _vhfAmSavedCockpitSmallFrequency = double.Parse("0." + diald10S + diald100S, NumberFormatInfoFullDisplay);
                        }
                    }
                }
            }
        }

        private void SwapCockpitStandbyFrequencyVhfAm()
        {
            /*Debug.Print("Before swap : ");
            Debug.Print("_vhfAmBigFrequencyStandby : " + _vhfAmBigFrequencyStandby);
            Debug.Print("_vhfAmSmallFrequencyStandby : " + _vhfAmSmallFrequencyStandby);
            Debug.Print("_vhfAmSavedCockpitBigFrequency : " + _vhfAmSavedCockpitBigFrequency);
            Debug.Print("_vhfAmSavedCockpitSmallFrequency : " + _vhfAmSavedCockpitSmallFrequency);*/
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

        private bool CorrectPositionDecimal100S(uint value)
        {
            for (int i = 0; i < _dialPositionsDecial100S.Length; i++)
            {
                if (_dialPositionsDecial100S[i] == value)
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

        private static bool SwitchVhfAmDecimalDirectionUp(int cockpitValue, int desiredValue)
        {
            var upCount = 0;
            var downCount = 0;
            var tmpCockpitValue = cockpitValue;
            while (tmpCockpitValue != desiredValue)
            {
                upCount++;
                tmpCockpitValue += 25;
                if (tmpCockpitValue > 975)
                {
                    tmpCockpitValue = 0;
                }
            }

            tmpCockpitValue = cockpitValue;
            while (tmpCockpitValue != desiredValue)
            {
                downCount++;
                tmpCockpitValue -= 25;
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

        private string GetAdfCommand(AdfDigit adfDigit, bool increase)
        {
            lock (_lockAdfUnitObject)
            {
                switch (adfDigit)
                {
                    case AdfDigit.Digit100S:
                        {
                            if (_adfCockpitSelectedUnitValue == 0)
                            {
                                return ADF1_UNIT100_S_INCREASE;
                            }
                            return ADF2_UNIT100_S_INCREASE;
                        }

                    case AdfDigit.Digit10S:
                        {
                            if (_adfCockpitSelectedUnitValue == 0)
                            {
                                return ADF1_UNIT10_S_INCREASE;
                            }
                            return ADF2_UNIT10_S_INCREASE;
                        }

                    case AdfDigit.Digits1S:
                        {
                            if (_adfCockpitSelectedUnitValue == 0)
                            {
                                if (increase)
                                {
                                    return ADF1_UNIT1_S_DECIMALS_INCREASE;
                                }
                                return ADF1_UNIT1_S_DECIMALS_DECREASE;
                            }
                            if (increase)
                            {
                                return ADF2_UNIT1_S_DECIMALS_INCREASE;
                            }
                            return ADF2_UNIT1_S_DECIMALS_DECREASE;
                        }
                }
            }
            return string.Empty;
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }
        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength) { }
        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence) { }
        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced) { }
        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink) { }
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand) { }
    }
}
