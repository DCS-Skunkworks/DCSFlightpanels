using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Interfaces;
using NonVisuals.Radios.Knobs;
using NonVisuals.Saitek;


namespace NonVisuals.Radios
{
    using MEF;

    using NonVisuals.Plugin;

    public class RadioPanelPZ69P51D : RadioPanelPZ69Base, IRadioPanel
    {
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
        private const string VHF1_VOLUME_KNOB_COMMAND_INC = "RADIO_VOLUME +2000\n";
        private const string VHF1_VOLUME_KNOB_COMMAND_DEC = "RADIO_VOLUME -2000\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69P51D(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                Common.LogError( ex);
            }
        }

        public void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            try
            {
                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                lock (LockLCDUpdateObject)
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
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE0:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE1:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE4:
                            case RadioPanelPZ69KnobsP51D.UPPER_NO_USE5:
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
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE0:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE1:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE4:
                            case RadioPanelPZ69KnobsP51D.LOWER_NO_USE5:
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

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(ProfileHandler.SelectedProfile().Description, HIDInstanceId, (int)PluginGamingPanelEnum.PZ69RadioPanel, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }

                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_INC);
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
                                                DCSBIOS.Send(VHF1_VOLUME_KNOB_COMMAND_DEC);
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
                Common.LogError( ex);
            }
        }


        private bool SkipVhf1PresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentP51DRadioMode.VHF || _currentLowerRadioMode == CurrentP51DRadioMode.VHF)
                {
                    if (_vhf1PresetDialSkipper > 2)
                    {
                        _vhf1PresetDialSkipper = 0;
                        return false;
                    }
                    _vhf1PresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
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

                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentP51DRadioMode.VHF:
                            {
                                //Pos     0    1    2    3    4

                                var channelAsString = string.Empty;
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

                                var channelAsString = string.Empty;
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
                Common.LogError( ex);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
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

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        public sealed override void Startup()
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


                StartListeningForPanelChanges();
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
            }
        }

        public override void Dispose()
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

        public override void ClearSettings(bool setIsDirty = false) { }

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
            SaitekPanelKnobs = RadioPanelKnobP51D.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentP51DRadioMode currentP51DRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentP51DRadioMode;
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
            }
        }

        private void SetLowerRadioMode(CurrentP51DRadioMode currentP51DRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentP51DRadioMode;
                //If NOUSE then send next round of e.Data to the panel in order to clear the LCD.
                //_sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Common.LogError( ex);
            }
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

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }


    }
}
