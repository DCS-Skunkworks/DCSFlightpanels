using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DCS_BIOS;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Radios.Knobs;
using NonVisuals.Radios.Misc;
using NonVisuals.Saitek;


namespace NonVisuals.Radios
{

    public class RadioPanelPZ69EmulatorFull : RadioPanelPZ69Base
    {

        /*
         * Emulator profile for PZ69 containing DCS-BIOS
         * For a specific toggle/switch/lever/knob the PZ69 can have :
         * - single key binding
         * - sequenced key binding
         * - DCS-BIOS control
         * - BIP Link.
         */
        //private HashSet<DCSBIOSBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ69>();
        private HashSet<KeyBindingPZ69DialPosition> _keyBindings = new HashSet<KeyBindingPZ69DialPosition>();
        private List<OSCommandBindingPZ69FullEmulator> _osCommandBindings = new List<OSCommandBindingPZ69FullEmulator>();
        private readonly HashSet<RadioPanelPZ69DisplayValue> _displayValues = new HashSet<RadioPanelPZ69DisplayValue>();
        private HashSet<DCSBIOSOutputBindingPZ69> _dcsBiosLcdBindings = new HashSet<DCSBIOSOutputBindingPZ69>();
        private HashSet<DCSBIOSActionBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingPZ69>();
        private readonly HashSet<BIPLinkPZ69> _bipLinks = new HashSet<BIPLinkPZ69>();
        private readonly byte[] _oldRadioPanelValue = { 0, 0, 0 };
        private readonly byte[] _newRadioPanelValue = { 0, 0, 0 };
        private readonly object _lcdDataVariablesLockObject = new object();

        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesUpper = new List<RadioPanelPZ69KnobsEmulator>() { RadioPanelPZ69KnobsEmulator.UpperCOM1, RadioPanelPZ69KnobsEmulator.UpperCOM2, RadioPanelPZ69KnobsEmulator.UpperNAV1, RadioPanelPZ69KnobsEmulator.UpperNAV2, RadioPanelPZ69KnobsEmulator.UpperADF, RadioPanelPZ69KnobsEmulator.UpperDME, RadioPanelPZ69KnobsEmulator.UpperXPDR };
        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesLower = new List<RadioPanelPZ69KnobsEmulator>() { RadioPanelPZ69KnobsEmulator.LowerCOM1, RadioPanelPZ69KnobsEmulator.LowerCOM2, RadioPanelPZ69KnobsEmulator.LowerNAV1, RadioPanelPZ69KnobsEmulator.LowerNAV2, RadioPanelPZ69KnobsEmulator.LowerADF, RadioPanelPZ69KnobsEmulator.LowerDME, RadioPanelPZ69KnobsEmulator.LowerXPDR };
        private double _upperActive = -1;
        private double _upperStandby = -1;
        private double _lowerActive = -1;
        private double _lowerStandby = -1;
        private PZ69DialPosition _pz69UpperDialPosition = PZ69DialPosition.UpperCOM1;
        private PZ69DialPosition _pz69LowerDialPosition = PZ69DialPosition.LowerCOM1;
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69EmulatorFull(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateSwitchKeys();
            Startup();
        }

        public sealed override void Startup()
        {
            try
            {
                StartListeningForPanelChanges();
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public override void Dispose()
        {
            try
            {
                Closed = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            var settings = genericPanelBinding.Settings;
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2)
                {

                    if (setting.StartsWith("RadioPanelKeyDialPos{"))
                    {
                        var keyBinding = new KeyBindingPZ69DialPosition();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("RadioPanelOSPZ69Full"))
                    {
                        var osCommand = new OSCommandBindingPZ69FullEmulator();
                        osCommand.ImportSettings(setting);
                        _osCommandBindings.Add(osCommand);
                    }
                    else if (setting.StartsWith("PZ69DisplayValue{"))
                    {
                        var radioPanelPZ69DisplayValue = new RadioPanelPZ69DisplayValue();
                        radioPanelPZ69DisplayValue.ImportSettings(setting);
                        _displayValues.Add(radioPanelPZ69DisplayValue);
                    }
                    else if (setting.StartsWith("RadioPanelBIPLink{"))
                    {
                        var bipLinkPZ69 = new BIPLinkPZ69();
                        bipLinkPZ69.ImportSettings(setting);
                        _bipLinks.Add(bipLinkPZ69);
                    }
                    else if (setting.StartsWith("RadioPanelDCSBIOSLCD{"))
                    {
                        var dcsbiosBindingLCDPZ69 = new DCSBIOSOutputBindingPZ69();
                        dcsbiosBindingLCDPZ69.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsbiosBindingLCDPZ69);
                    }
                    else if (setting.StartsWith("RadioPanelDCSBIOSControl{"))
                    {
                        var dcsbiosBindingPZ69 = new DCSBIOSActionBindingPZ69();
                        dcsbiosBindingPZ69.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsbiosBindingPZ69);
                    }
                }

                _keyBindings = KeyBindingPZ69DialPosition.SetNegators(_keyBindings);
                SettingsApplied();
            }
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            var result = new List<string>();

            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null)
                {
                    result.Add(keyBinding.ExportSettings());
                }
            }
            foreach (var osCommand in _osCommandBindings)
            {
                if (!osCommand.OSCommandObject.IsEmpty)
                {
                    result.Add(osCommand.ExportSettings());
                }
            }
            foreach (var displayValue in _displayValues)
            {
                var tmp = displayValue.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }
            foreach (var bipLink in _bipLinks)
            {
                var tmp = bipLink.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }
            foreach (var dcsBiosLcdBindings in _dcsBiosLcdBindings)
            {
                var tmp = dcsBiosLcdBindings.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }
            foreach (var dcsBiosBindings in _dcsBiosBindings)
            {
                var tmp = dcsBiosBindings.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }
            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterPanelBinding(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateCounter(e.Address, e.Data);
            foreach (var dcsbiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (!dcsbiosBindingLCD.UseFormula && e.Address == dcsbiosBindingLCD.DCSBIOSOutputObject.Address)
                {
                    lock (_lcdDataVariablesLockObject)
                    {
                        var tmp = dcsbiosBindingLCD.CurrentValue;
                        dcsbiosBindingLCD.CurrentValue = (int)dcsbiosBindingLCD.DCSBIOSOutputObject.GetUIntValue(e.Data);
                        if (tmp != dcsbiosBindingLCD.CurrentValue && (dcsbiosBindingLCD.DialPosition == _pz69UpperDialPosition || dcsbiosBindingLCD.DialPosition == _pz69LowerDialPosition))
                        {
                            //Update only if this LCD binding is in current use
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }
                else if (dcsbiosBindingLCD.UseFormula)
                {
                    if (dcsbiosBindingLCD.DCSBIOSOutputFormulaObject.CheckForMatch(e.Address, e.Data))
                    {
                        lock (_lcdDataVariablesLockObject)
                        {
                            var tmp = dcsbiosBindingLCD.CurrentValue;
                            dcsbiosBindingLCD.CurrentValue = dcsbiosBindingLCD.DCSBIOSOutputFormulaObject.Evaluate();
                            if (tmp != dcsbiosBindingLCD.CurrentValue && (dcsbiosBindingLCD.DialPosition == _pz69UpperDialPosition || dcsbiosBindingLCD.DialPosition == _pz69LowerDialPosition))
                            {
                                //Update only if this LCD binding is in current use
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            }
                        }
                    }
                }
            }
            ShowFrequenciesOnPanel();
        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _osCommandBindings.Clear();
            _displayValues.Clear();
            _bipLinks.Clear();
            _dcsBiosLcdBindings.Clear();
            _dcsBiosBindings.Clear();
        }

        public HashSet<KeyBindingPZ69DialPosition> KeyBindingsHashSet => _keyBindings;

        public List<OSCommandBindingPZ69FullEmulator> OSCommandHashSet
        {
            get => _osCommandBindings;
            set => _osCommandBindings = value;
        }

        public HashSet<BIPLinkPZ69> BipLinkHashSet => _bipLinks;

        public HashSet<RadioPanelPZ69DisplayValue> DisplayValueHashSet => _displayValues;

        private void PZ69KnobChanged(RadioPanelPZ69KnobEmulator radioPanelKey)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                {
                    keyBinding.OSKeyPress.Execute(new CancellationToken());
                }
            }
        }

        private void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (ForwardPanelEvent)
            {
                foreach (var radioPanelKeyObject in hashSet)
                {
                    //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                    var radioPanelKey = (RadioPanelPZ69KnobEmulator)radioPanelKeyObject;

                    if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec)
                    {
                        if (SkipCurrentFrequencyChange())
                        {
                            return;
                        }
                    }

                    if (radioPanelKey.IsOn)
                    {
                        if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperCOM1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperCOM2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperNAV1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperNAV2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperADF;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperDME;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperXPDR;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerCOM1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerCOM2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerNAV1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerNAV2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerADF;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerDME;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerXPDR;
                        }
                    }

                    foreach (var keyBinding in _keyBindings)
                    {
                        if (keyBinding.DialPosition == _pz69UpperDialPosition || keyBinding.DialPosition == _pz69LowerDialPosition)
                        {
                            if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                            {
                                keyBinding.OSKeyPress.Execute(new CancellationToken());
                                break;
                            }
                        }
                    }

                    foreach (var osCommand in _osCommandBindings)
                    {
                        if (osCommand.DialPosition == _pz69UpperDialPosition || osCommand.DialPosition == _pz69LowerDialPosition)
                        {
                            if (osCommand.OSCommandObject != null && osCommand.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && osCommand.WhenTurnedOn == radioPanelKey.IsOn)
                            {
                                osCommand.OSCommandObject.Execute(new CancellationToken());
                                break;
                            }
                        }
                    }

                    foreach (var bipLinkPZ55 in _bipLinks)
                    {
                        //if (bipLinkPZ55.DialPosition == _pz69UpperDialPosition || keyBinding.DialPosition == _pz69LowerDialPosition)
                        //{
                        if (bipLinkPZ55.BIPLights.Count > 0 && bipLinkPZ55.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && bipLinkPZ55.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            bipLinkPZ55.Execute();
                            break;
                        }
                        //}
                    }
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DialPosition == _pz69UpperDialPosition || dcsBiosBinding.DialPosition == _pz69LowerDialPosition)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && dcsBiosBinding.WhenTurnedOn == radioPanelKey.IsOn)
                            {
                                dcsBiosBinding.SendDCSBIOSCommands(new CancellationToken());
                                break;
                            }
                        }
                    }

                }
            }

            foreach (var radioPanelKeyObject in hashSet)
            {
                //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var radioPanelKey = (RadioPanelPZ69KnobEmulator)radioPanelKeyObject;
                if (radioPanelKey.IsOn)
                {
                    if (_panelPZ69DialModesUpper.Contains(radioPanelKey.RadioPanelPZ69Knob))
                    {
                        _upperActive = -1;
                        _upperStandby = -1;
                    }

                    if (_panelPZ69DialModesLower.Contains(radioPanelKey.RadioPanelPZ69Knob))
                    {
                        _lowerActive = -1;
                        _lowerStandby = -1;
                    }

                    foreach (var displayValue in _displayValues)
                    {
                        if (displayValue.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob)
                        {
                            if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                            {
                                _upperActive = double.Parse(displayValue.Value, Common.GetPZ69FullDisplayNumberFormat());
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                            {
                                _upperStandby = double.Parse(displayValue.Value, Common.GetPZ69FullDisplayNumberFormat());
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                            {
                                _lowerActive = double.Parse(displayValue.Value, Common.GetPZ69FullDisplayNumberFormat());
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                            {
                                _lowerStandby = double.Parse(displayValue.Value, Common.GetPZ69FullDisplayNumberFormat());
                            }
                        }
                    }
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    ShowFrequenciesOnPanel();
                }
            }
        }

        private void ShowFrequenciesOnPanel()
        {
            if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
            {
                return;
            }
            lock (_lcdDataVariablesLockObject)
            {
                var bytes = new byte[21];
                bytes[0] = 0x0;
                //Find upper left => lower right data from the DCS-BIOS LCD holders
                foreach (var dcsbiosBindingLCDPZ69 in LCDBindings)
                {
                    if (dcsbiosBindingLCDPZ69.DialPosition == _pz69UpperDialPosition)
                    {
                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                        {
                            if (_upperActive.ToString(CultureInfo.InvariantCulture).Length > 5)
                            {
                                _upperActive = int.Parse(dcsbiosBindingLCDPZ69.CurrentValue.ToString().Substring(0, 5));
                            }
                            else
                            {
                                _upperActive = dcsbiosBindingLCDPZ69.CurrentValue;
                            }
                        }

                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                        {
                            if (_upperStandby.ToString(CultureInfo.InvariantCulture).Length > 5)
                            {
                                _upperStandby =
                                    int.Parse(dcsbiosBindingLCDPZ69.CurrentValue.ToString().Substring(0, 5));
                            }
                            else
                            {
                                _upperStandby = dcsbiosBindingLCDPZ69.CurrentValue;
                            }
                        }
                    }
                    if (dcsbiosBindingLCDPZ69.DialPosition == _pz69LowerDialPosition)
                    {
                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                        {
                            if (_lowerActive.ToString(CultureInfo.InvariantCulture).Length > 5)
                            {
                                _lowerActive = int.Parse(dcsbiosBindingLCDPZ69.CurrentValue.ToString().Substring(0, 5));
                            }
                            else
                            {
                                _lowerActive = dcsbiosBindingLCDPZ69.CurrentValue;
                            }
                        }
                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                        {
                            if (_lowerStandby.ToString(CultureInfo.InvariantCulture).Length > 5)
                            {
                                _lowerStandby = int.Parse(dcsbiosBindingLCDPZ69.CurrentValue.ToString().Substring(0, 5));
                            }
                            else
                            {
                                _lowerStandby = dcsbiosBindingLCDPZ69.CurrentValue;
                            }
                        }
                    }
                }
                if (_upperActive < 0)
                {
                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                }
                else
                {
                    SetPZ69DisplayBytesDefault(ref bytes, _upperActive, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                }

                if (_upperStandby < 0)
                {
                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                }
                else
                {
                    SetPZ69DisplayBytesDefault(ref bytes, _upperStandby, PZ69LCDPosition.UPPER_STBY_RIGHT);
                }

                if (_lowerActive < 0)
                {
                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                }
                else
                {
                    SetPZ69DisplayBytesDefault(ref bytes, _lowerActive, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                }

                if (_lowerStandby < 0)
                {
                    SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                }
                else
                {
                    SetPZ69DisplayBytesDefault(ref bytes, _lowerStandby, PZ69LCDPosition.LOWER_STBY_RIGHT);
                }

                SendLCDData(bytes);
            }

            Interlocked.Add(ref _doUpdatePanelLCD, -1);
        }


        public string GetKeyPressForLoggingPurposes(RadioPanelPZ69KnobEmulator radioPanelKey)
        {
            var result = "";
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        public void AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, string valueAsString, RadioPanelPZ69Display radioPanelDisplay)
        {
            if (string.IsNullOrEmpty(valueAsString))
            {
                ClearDisplayValue(radioPanelPZ69Knob, radioPanelDisplay);
                return;
            }
            var value = double.Parse(valueAsString, Common.GetPZ69FullDisplayNumberFormat());
            if (value < 0)
            {
                ClearDisplayValue(radioPanelPZ69Knob, radioPanelDisplay);
                return;
            }
            var found = false;
            foreach (var displayValue in _displayValues)
            {
                if (displayValue.RadioPanelPZ69Knob == radioPanelPZ69Knob && displayValue.RadioPanelDisplay == radioPanelDisplay)
                {
                    displayValue.Value = valueAsString;
                    found = true;
                }
            }
            if (!found)
            {
                var displayValue = new RadioPanelPZ69DisplayValue();
                displayValue.RadioPanelPZ69Knob = radioPanelPZ69Knob;
                displayValue.RadioPanelDisplay = radioPanelDisplay;
                displayValue.Value = valueAsString;
                _displayValues.Add(displayValue);
            }
            SetIsDirty();
        }

        public override void AddOrUpdateSingleKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff) panelSwitchOnOff;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            if (string.IsNullOrEmpty(keyPress))
            {
                var tmp = new PZ69SwitchOnOff(radioPanelPZ69KeyOnOff.Switch, radioPanelPZ69KeyOnOff.ButtonState);
                ClearAllBindings(pz69DialPosition, tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState && keyBinding.DialPosition == pz69DialPosition)
                {
                    if (string.IsNullOrEmpty(keyPress))
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new KeyPress(keyPress, keyPressLength);
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keyPress))
            {
                var keyBinding = new KeyBindingPZ69DialPosition();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69KeyOnOff.Switch;
                keyBinding.DialPosition = pz69DialPosition;
                keyBinding.OSKeyPress = new KeyPress(keyPress, keyPressLength);
                keyBinding.WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }
            _keyBindings = KeyBindingPZ69DialPosition.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand osCommand)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;

            //This must accept lists
            var found = false;

            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            foreach (var osCommandBinding in _osCommandBindings)
            {
                if (osCommandBinding.DialPosition == pz69DialPosition && osCommandBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && osCommandBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    osCommandBinding.OSCommandObject = osCommand;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var osCommandBindingPZ69Full = new OSCommandBindingPZ69FullEmulator();
                osCommandBindingPZ69Full.DialPosition = pz69DialPosition;
                osCommandBindingPZ69Full.RadioPanelPZ69Key = radioPanelPZ69KeyOnOff.Switch;
                osCommandBindingPZ69Full.OSCommandObject = osCommand;
                osCommandBindingPZ69Full.WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState;
                _osCommandBindings.Add(osCommandBindingPZ69Full);
            }
            SetIsDirty();
        }


        public void ClearAllBindings(PZ69DialPosition pz69DialPosition, PZ69SwitchOnOff radioPanelPZ69KnobOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.DialPosition == pz69DialPosition && keyBinding.RadioPanelPZ69Key == radioPanelPZ69KnobOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.ButtonState)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            SetIsDirty();
        }

        public void ClearDisplayValue(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, RadioPanelPZ69Display radioPanelPZ69Display)
        {
            //This must accept lists
            /*foreach (var displayValue in _displayValues)
            {
                if (displayValue.RadioPanelPZ69Knob == radioPanelPZ69Knob && displayValue.RadioPanelDisplay == radioPanelPZ69Display)
                {
                    displayValue.Value = null;
                }
            }*/
            _displayValues.RemoveWhere(x => x.RadioPanelPZ69Knob == radioPanelPZ69Knob && x.RadioPanelDisplay == radioPanelPZ69Display);
            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, KeyPressInfo> keySequence)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);

            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlListPZ69.KEYS, radioPanelPZ69KeyOnOff);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState && keyBinding.DialPosition == pz69DialPosition)
                {
                    if (keySequence.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new KeyPress(description, keySequence);
                    }
                    found = true;
                    break;
                }
            }
            if (!found && keySequence.Count > 0)
            {
                var keyBinding = new KeyBindingPZ69DialPosition();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69KeyOnOff.Switch;
                keyBinding.DialPosition = pz69DialPosition;
                keyBinding.OSKeyPress = new KeyPress(description, keySequence);
                keyBinding.WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }
            _keyBindings = KeyBindingPZ69DialPosition.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var bipLinkPZ69 = (BIPLinkPZ69) bipLink;

            if (bipLinkPZ69.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlListPZ69.BIPS, radioPanelPZ69KeyOnOff);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;

            foreach (var tmpBipLink in _bipLinks)
            {
                if (tmpBipLink.RadioPanelPZ69Knob == radioPanelPZ69KeyOnOff.Switch && tmpBipLink.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    tmpBipLink.BIPLights = bipLinkPZ69.BIPLights;
                    tmpBipLink.Description = bipLinkPZ69.Description;
                    tmpBipLink.RadioPanelPZ69Knob = radioPanelPZ69KeyOnOff.Switch;
                    found = true;
                    break;
                }
            }
            if (!found && bipLinkPZ69.BIPLights.Count > 0)
            {
                bipLinkPZ69.RadioPanelPZ69Knob = radioPanelPZ69KeyOnOff.Switch;
                bipLinkPZ69.WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState;
                _bipLinks.Add(bipLinkPZ69);
            }
            SetIsDirty();
        }
        
        public void AddOrUpdateLCDBinding(DCSBIOSOutput dcsbiosOutput, PZ69LCDPosition pz69LCDPosition)
        {
            var found = false;
            var pz69DialPosition = _pz69UpperDialPosition;
            if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT || pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
            {
                pz69DialPosition = _pz69LowerDialPosition;
            }
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == pz69DialPosition && dcsBiosBindingLCD.PZ69LcdPosition == pz69LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSOutputBindingPZ69();
                dcsBiosBindingLCD.DialPosition = pz69DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                dcsBiosBindingLCD.PZ69LcdPosition = pz69LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            SetIsDirty();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutputFormula dcsbiosOutputFormula, PZ69LCDPosition pz69LCDPosition)
        {
            var found = false;
            var pz69DialPosition = _pz69UpperDialPosition;
            if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT || pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
            {
                pz69DialPosition = _pz69LowerDialPosition;
            }
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == pz69DialPosition && dcsBiosBindingLCD.PZ69LcdPosition == pz69LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                    Debug.Print("3 found");
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSOutputBindingPZ69();
                dcsBiosBindingLCD.DialPosition = pz69DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                dcsBiosBindingLCD.PZ69LcdPosition = pz69LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            SetIsDirty();
        }

        public void DeleteDCSBIOSLcdBinding(PZ69LCDPosition pz69LCDPosition)
        {
            var pz69DialPosition = _pz69UpperDialPosition;
            if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT || pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
            {
                pz69DialPosition = _pz69LowerDialPosition;
            }
            //Removes config
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == pz69DialPosition && dcsBiosBindingLCD.PZ69LcdPosition == pz69LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputObject = null;
                    break;
                }
            }
            SetIsDirty();
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            if (dcsbiosInputs.Count == 0)
            {
                RemoveSwitchFromList(ControlListPZ69.DCSBIOS, radioPanelPZ69KeyOnOff);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == pz69DialPosition && dcsBiosBinding.RadioPanelPZ69Knob == radioPanelPZ69KeyOnOff.Switch && dcsBiosBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSActionBindingPZ69();
                dcsBiosBinding.RadioPanelPZ69Knob = radioPanelPZ69KeyOnOff.Switch;
                dcsBiosBinding.DialPosition = pz69DialPosition;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            SetIsDirty();
        }

        public void DeleteDCSBIOSBinding(RadioPanelPZ69KnobsEmulator knob, bool whenTurnedOn)
        {
            var pz69DialPosition = GetDial(knob);
            //Removes config
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == pz69DialPosition && dcsBiosBinding.RadioPanelPZ69Knob == knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                    break;
                }
            }
            SetIsDirty();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var controlListPZ69 = (ControlListPZ69) controlList;

            var found = false;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            if (controlListPZ69 == ControlListPZ69.ALL || controlListPZ69 == ControlListPZ69.KEYS)
            {
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState && keyBinding.DialPosition == pz69DialPosition)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    found = true;
                    break;
                }
            }
            if (controlListPZ69 == ControlListPZ69.ALL || controlListPZ69 == ControlListPZ69.DCSBIOS)
            {
                foreach (var dcsBiosBinding in _dcsBiosBindings)
                {
                    if (dcsBiosBinding.RadioPanelPZ69Knob == radioPanelPZ69KeyOnOff.Switch && dcsBiosBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState && dcsBiosBinding.DialPosition == pz69DialPosition)
                    {
                        dcsBiosBinding.DCSBIOSInputs.Clear();
                    }
                    found = true;
                    break;
                }
            }
            if (controlListPZ69 == ControlListPZ69.ALL || controlListPZ69 == ControlListPZ69.BIPS)
            {
                foreach (var bipLink in _bipLinks)
                {
                    if (bipLink.RadioPanelPZ69Knob == radioPanelPZ69KeyOnOff.Switch && bipLink.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                    {
                        bipLink.BIPLights.Clear();
                    }
                    found = true;
                    break;
                }
            }
            for (int i = 0; i < _osCommandBindings.Count; i++)
            {
                var osCommand = _osCommandBindings[i];

                if (osCommand.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && osCommand.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    _osCommandBindings[i] = null;
                    found = true;
                }
            }

            if (found)
            {
                SetIsDirty();
            }
        }

        public bool IsBindingActive(KeyBindingPZ69DialPosition keyBindingPZ69DialPosition)
        {
            var dial = GetDial(keyBindingPZ69DialPosition.RadioPanelPZ69Key);
            if (dial == keyBindingPZ69DialPosition.DialPosition)
            {
                return true;
            }
            return false;
        }

        private PZ69DialPosition GetDial(RadioPanelPZ69KnobsEmulator knob)
        {
            if (knob.ToString().Contains("Upper"))
            {
                return _pz69UpperDialPosition;
            }
            return _pz69LowerDialPosition;
        }

        /*
        private bool IsLowerArea(RadioPanelPZ69KnobsEmulator knob)
        {
            if (knob.ToString().Contains("Upper"))
            {
                return false;
            }
            return true;
        }

        private bool IsUpperArea(RadioPanelPZ69KnobsEmulator knob)
        {
            if (knob.ToString().Contains("Upper"))
            {
                return true;
            }
            return false;
        }
        */

        public void Clear(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            SetIsDirty();
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }



        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateSwitchKeys()
        {
            SaitekPanelKnobs = RadioPanelPZ69KnobEmulator.GetRadioPanelKnobs();
        }

        public HashSet<DCSBIOSOutputBindingPZ69> LCDBindings
        {
            get => _dcsBiosLcdBindings;
            set => _dcsBiosLcdBindings = value;
        }

        public HashSet<DCSBIOSActionBindingPZ69> DCSBIOSBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

        public PZ69DialPosition PZ69UpperDialPosition
        {
            get => _pz69UpperDialPosition;
        }

        public PZ69DialPosition PZ69LowerDialPosition
        {
            get => _pz69LowerDialPosition;
        }
    }
}
