using System.Diagnostics;
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




    /// <summary>
    /// Pre-programmed radio panel for the Yak-52. 
    /// </summary>
    public class RadioPanelPZ69Yak52 : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentYak52RadioMode
        {
            VHF,
            ADF,
            NOUSE
        }

        private bool _upperButtonPressed;
        private bool _lowerButtonPressed;
        private bool _upperButtonPressedAndDialRotated;
        private bool _lowerButtonPressedAndDialRotated;

        private CurrentYak52RadioMode _currentUpperRadioMode = CurrentYak52RadioMode.VHF;
        private CurrentYak52RadioMode _currentLowerRadioMode = CurrentYak52RadioMode.VHF;

        /*
        *  VHF RADIO 
        *  COM1 Large Mhz
        *  COM1 Small Khz
        *  Freq. Squelch on off
         * Freq. + Small Dial : Volume
        */
        private readonly object _lockVHFRadioSquelchSwitchObject1 = new();
        private readonly object _lockVHFRadioFrequencyStringObject1 = new();
        private DCSBIOSOutput _vhfRadioSquelchSwitchDcsbiosOutput;
        private volatile uint _vhfRadioSquelchSwitchCockpitPosition;

        private DCSBIOSOutput _vhfRadioFrequencyDcsbiosOutput;
        private string _vhfRadioFrequencyString = "";

        private readonly uint _vhfRadioMhzDialChangeValue = 8000; //Values gotten from ctrlref while clicking the dial in cockpit
        private readonly uint _vhfRadioKhzDialChangeValue = 8000;
        private readonly uint _vhfRadioVolumeDialChangeValue = 1000;
        private const string VHF_RADIO_SQUELCH_TOGGLE = "FRONT_VHF_RADIO_SQ";
        private const string VHF_RADIO_SQUELCH_TOGGLE_COMMAND = "FRONT_VHF_RADIO_SQ TOGGLE\n";
        private const string VHF_RADIO_MHZ_DIAL = "FRONT_VHF_RADIO_MHZ";
        private const string VHF_RADIO_KHZ_DIAL = "FRONT_VHF_RADIO_KHZ";
        private const string VHF_RADIO_VOLUME_DIAL = "FRONT_VHF_RADIO_VOL";
        private const string VHF_RADIO_FREQUENCY = "BAKLAN5_FREQ";

        /*
         *  LF DETROLA RADIO
         *  COM1 Large : Volume Dial
         *  COM1 Small : Frequency Dial        
         */
        private readonly object _lockLFRadioFrequencyDialObject1 = new();
        private readonly object _lockLFRadioVolumeDialObject1 = new();
        private DCSBIOSOutput _lfRadioFrequencyDcsbiosOutput;
        private DCSBIOSOutput _lfRadioVolumeDcsbiosOutput;
        private volatile uint _lfRadioFrequencyDCSBIOSValue = 1;
        private volatile uint _lfRadioVolumeDCSBIOSValue;
        private volatile uint _lfRadioFrequencyCockpitValue = 1;
        private volatile uint _lfRadioVolumeCockpitValue;
        private int _lfRadioPresetDialSkipper;
        private readonly uint _lfFrequencyChangeValue = 50;
        private readonly uint _lfVolumeChangeValue = 200;
        private readonly ClickSpeedDetector _lfFrequencyDialChangeMonitor = new(15);
        private readonly ClickSpeedDetector _lfVolumeDialChangeMonitor = new(15);

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69Yak52(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    BIOSEventHandler.DetachDataListener(this);
                    BIOSEventHandler.DetachStringListener(this);
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

                if (_vhfRadioFrequencyDcsbiosOutput.StringValueHasChanged(e.Address, e.StringData))
                {
                    lock (_lockVHFRadioFrequencyStringObject1)
                    {
                        _vhfRadioFrequencyString = e.StringData;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
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

                // VHF Radio Squelch Switch
                if (_vhfRadioSquelchSwitchDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioSquelchSwitchObject1)
                    {
                        _vhfRadioSquelchSwitchCockpitPosition = _vhfRadioSquelchSwitchDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }

                // ADF
                /*
                if (_vhfRadioChannelAPresetDcsbiosOutput.UIntValueHasChanged(e.Address, e.Data))
                {
                    lock (_lockVHFRadioPresetDialObject1)
                    {
                        _vhfRadioChannelACockpitButton = _vhfRadioChannelAPresetDcsbiosOutput.LastUIntValue;
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                }
                */


                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (isFirstReport)
            {
                return;
            }

            try
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobYak52)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsYak52.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.VHF);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.UPPER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.ADF);
                                    }

                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsYak52.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentYak52RadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.VHF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_ADF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.ADF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsYak52.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentYak52RadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode != CurrentYak52RadioMode.VHF)
                                    {
                                        break;
                                    }
                                    _upperButtonPressed = radioPanelKnob.IsOn;
                                    if (!radioPanelKnob.IsOn)
                                    {
                                        if (!_upperButtonPressedAndDialRotated)
                                        {
                                            // Do not synch if user has pressed the button to configure the radio
                                            // Do when user releases button
                                            DCSBIOS.Send(VHF_RADIO_SQUELCH_TOGGLE_COMMAND);
                                        }

                                        _upperButtonPressedAndDialRotated = false;
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode != CurrentYak52RadioMode.VHF)
                                    {
                                        break;
                                    }
                                    _lowerButtonPressed = radioPanelKnob.IsOn;
                                    if (!radioPanelKnob.IsOn)
                                    {
                                        if (!_lowerButtonPressedAndDialRotated)
                                        {
                                            // Do not synch if user has pressed the button to configure the radio
                                            // Do when user releases button
                                            DCSBIOS.Send(VHF_RADIO_SQUELCH_TOGGLE_COMMAND);
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
                                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_Yak52,
                                (int)radioPanelKnob.RadioPanelPZ69Knob,
                                radioPanelKnob.IsOn,
                                null);
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobYak52 = (RadioPanelKnobYak52)o;
                    if (radioPanelKnobYak52.IsOn)
                    {
                        switch (radioPanelKnobYak52.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(true);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFVolumeCommand(true);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(false);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFVolumeCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(true);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(true);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFFrequencyCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_upperButtonPressed)
                                                {
                                                    _upperButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(false);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(false);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFFrequencyCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(true);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFVolumeCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                SendVHFMhzCommand(false);
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFVolumeCommand(false);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(true);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(true);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFFrequencyCommand(true);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentYak52RadioMode.VHF:
                                            {
                                                if (_lowerButtonPressed)
                                                {
                                                    _lowerButtonPressedAndDialRotated = true;
                                                    SendVHFVolumeCommand(false);
                                                }
                                                else
                                                {
                                                    SendVHFKhzCommand(false);
                                                }
                                                break;
                                            }
                                        case CurrentYak52RadioMode.ADF:
                                            {
                                                //SendLFFrequencyCommand(false);
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
                Logger.Error(ex);
            }
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

                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentYak52RadioMode.VHF:
                            {
                                if (_upperButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                    break;
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, _vhfRadioFrequencyString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioSquelchSwitchCockpitPosition, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                        case CurrentYak52RadioMode.ADF:
                            {
                                lock (_lockLFRadioFrequencyDialObject1)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioFrequencyCockpitValue, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioVolumeCockpitValue, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentYak52RadioMode.VHF:
                            {
                                if (_lowerButtonPressed)
                                {
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                    break;
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, _vhfRadioFrequencyString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, _vhfRadioSquelchSwitchCockpitPosition, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                        case CurrentYak52RadioMode.ADF:
                            {
                                lock (_lockLFRadioFrequencyDialObject1)
                                {
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioFrequencyCockpitValue, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                    SetPZ69DisplayBytesUnsignedInteger(ref bytes, _lfRadioVolumeCockpitValue, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                }
                                break;
                            }
                        case CurrentYak52RadioMode.NOUSE:
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
                Logger.Error(ex);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        public sealed override void Startup()
        {
            try
            {
                // VHF
                _vhfRadioSquelchSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(VHF_RADIO_SQUELCH_TOGGLE);
                _vhfRadioFrequencyDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(VHF_RADIO_FREQUENCY);
                DCSBIOSStringManager.AddListeningAddress(_vhfRadioFrequencyDcsbiosOutput);

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
            SaitekPanelKnobs = RadioPanelKnobYak52.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentYak52RadioMode currentYak52RadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentYak52RadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentYak52RadioMode currentYak52RadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentYak52RadioMode;
                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SendVHFMhzCommand(bool increase)
        {
            var s = GetVHFRadioMhzDialStringCommand(increase);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            DCSBIOS.Send(s);
        }

        private void SendVHFKhzCommand(bool increase)
        {
            var s = GetVHFRadioKhzDialStringCommand(increase);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            DCSBIOS.Send(s);
        }
        private void SendVHFVolumeCommand(bool increase)
        {
            var s = GetVHFRadioVolumeDialStringCommand(increase);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            DCSBIOS.Send(s);
        }

        private string GetVHFRadioMhzDialStringCommand(bool moveUp)
        {
            if (moveUp)
            {

                return $"{VHF_RADIO_MHZ_DIAL} {_vhfRadioMhzDialChangeValue} \n";
            }
            return $"{VHF_RADIO_MHZ_DIAL} -{_vhfRadioMhzDialChangeValue} \n";
        }

        private string GetVHFRadioKhzDialStringCommand(bool moveUp)
        {
            if (moveUp)
            {

                return $"{VHF_RADIO_KHZ_DIAL} {_vhfRadioKhzDialChangeValue}\n";
            }
            return $"{VHF_RADIO_KHZ_DIAL} -{_vhfRadioKhzDialChangeValue} \n";
        }

        private string GetVHFRadioVolumeDialStringCommand(bool moveUp)
        {
            if (moveUp)
            {

                return $"{VHF_RADIO_VOLUME_DIAL} {_vhfRadioVolumeDialChangeValue} \n";
            }
            return $"{VHF_RADIO_VOLUME_DIAL} -{_vhfRadioVolumeDialChangeValue} \n";
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff) { }

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
