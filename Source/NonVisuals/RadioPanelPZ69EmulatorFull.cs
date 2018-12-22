using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DCS_BIOS;
using HidLibrary;
using System.Threading;
using ClassLibraryCommon;

namespace NonVisuals
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
        private readonly HashSet<KeyBindingPZ69DialPosition> _keyBindings = new HashSet<KeyBindingPZ69DialPosition>();
        private readonly HashSet<RadioPanelPZ69DisplayValue> _displayValues = new HashSet<RadioPanelPZ69DisplayValue>();
        private HashSet<DCSBIOSBindingLCDPZ69> _dcsBiosLcdBindings = new HashSet<DCSBIOSBindingLCDPZ69>();
        private HashSet<DCSBIOSBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ69>();
        private readonly HashSet<BIPLinkPZ69> _bipLinks = new HashSet<BIPLinkPZ69>();
        private HashSet<RadioPanelPZ69KnobEmulator> _radioPanelKnobs = new HashSet<RadioPanelPZ69KnobEmulator>();
        private bool _isFirstNotification = true;
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
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69FullEmulator.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
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

        public override void ImportSettings(List<string> settings)
        {
            //Clear current bindings
            ClearSettings();
            if (settings == null || settings.Count == 0)
            {
                return;
            }
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId))
                {
                    if (setting.StartsWith("RadioPanelKeyDialPos{"))
                    {
                        var keyBinding = new KeyBindingPZ69DialPosition();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
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
                        var dcsbiosBindingLCDPZ69 = new DCSBIOSBindingLCDPZ69();
                        dcsbiosBindingLCDPZ69.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsbiosBindingLCDPZ69);
                    }
                    else if (setting.StartsWith("RadioPanelDCSBIOSControl{"))
                    {
                        var dcsbiosBindingPZ69 = new DCSBIOSBindingPZ69();
                        dcsbiosBindingPZ69.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsbiosBindingPZ69);
                    }
                }
            }
            OnSettingsApplied();
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
            e.ProfileHandlerEA.RegisterProfileData(this, ExportSettings());
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
            _displayValues.Clear();
            _bipLinks.Clear();
            _dcsBiosLcdBindings.Clear();
            _dcsBiosBindings.Clear();
        }

        public HashSet<KeyBindingPZ69DialPosition> KeyBindingsHashSet => _keyBindings;

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
                    keyBinding.OSKeyPress.Execute();
                }
            }
        }

        private void PZ69KnobChanged(IEnumerable<object> hashSet)
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
                        if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            keyBinding.OSKeyPress.Execute();
                            break;
                        }
                    }
                    foreach (var bipLinkPZ55 in _bipLinks)
                    {
                        if (bipLinkPZ55.BIPLights.Count > 0 && bipLinkPZ55.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && bipLinkPZ55.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            bipLinkPZ55.Execute();
                            break;
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
            IsDirtyMethod();
        }

        public void AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn)
        {
            var pz69DialPosition = GetDial(radioPanelPZ69Knob);
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new RadioPanelPZ69KeyOnOff(radioPanelPZ69Knob, whenTurnedOn);
                ClearAllBindings(pz69DialPosition, tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBinding.WhenTurnedOn == whenTurnedOn && keyBinding.DialPosition == pz69DialPosition)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var keyBinding = new KeyBindingPZ69DialPosition();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69Knob;
                keyBinding.DialPosition = pz69DialPosition;
                keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            Common.DebugP("RadioPanelPZ69FullEmulator _keyBindings : " + _keyBindings.Count);
            IsDirtyMethod();
        }


        public void ClearAllBindings(PZ69DialPosition pz69DialPosition, RadioPanelPZ69KeyOnOff radioPanelPZ69KnobOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.DialPosition == pz69DialPosition && keyBinding.RadioPanelPZ69Key == radioPanelPZ69KnobOnOff.RadioPanelPZ69Key && keyBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.ButtonState)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            IsDirtyMethod();
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
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn)
        {
            var pz69DialPosition = GetDial(radioPanelPZ69Knob);

            if (sortedList.Count == 0)
            {
                RemoveRadioPanelKnobFromList(ControlListPZ69.KEYS, radioPanelPZ69Knob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBinding.WhenTurnedOn == whenTurnedOn && keyBinding.DialPosition == pz69DialPosition)
                {
                    if (sortedList.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var keyBinding = new KeyBindingPZ69DialPosition();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69Knob;
                keyBinding.DialPosition = pz69DialPosition;
                keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
        }


        public void AddOrUpdateBIPLinkKeyBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, BIPLinkPZ69 bipLinkPZ69, bool whenTurnedOn)
        {
            if (bipLinkPZ69.BIPLights.Count == 0)
            {
                RemoveRadioPanelKnobFromList(ControlListPZ69.BIPS, radioPanelPZ69Knob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;

            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.RadioPanelPZ69Knob == radioPanelPZ69Knob && bipLink.WhenTurnedOn == whenTurnedOn)
                {
                    bipLink.BIPLights = bipLinkPZ69.BIPLights;
                    bipLink.Description = bipLinkPZ69.Description;
                    bipLink.RadioPanelPZ69Knob = radioPanelPZ69Knob;
                    bipLink.WhenTurnedOn = whenTurnedOn;
                    found = true;
                    break;
                }
            }
            if (!found && bipLinkPZ69.BIPLights.Count > 0)
            {
                bipLinkPZ69.RadioPanelPZ69Knob = radioPanelPZ69Knob;
                bipLinkPZ69.WhenTurnedOn = whenTurnedOn;
                _bipLinks.Add(bipLinkPZ69);
            }
            IsDirtyMethod();
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
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ69();
                dcsBiosBindingLCD.DialPosition = pz69DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                dcsBiosBindingLCD.PZ69LcdPosition = pz69LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            IsDirtyMethod();
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
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ69();
                dcsBiosBindingLCD.DialPosition = pz69DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                dcsBiosBindingLCD.PZ69LcdPosition = pz69LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            IsDirtyMethod();
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
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSBinding(RadioPanelPZ69KnobsEmulator knob, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn)
        {
            if (dcsbiosInputs.Count == 0)
            {
                RemoveRadioPanelKnobFromList(ControlListPZ69.DCSBIOS, knob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;
            var pz69DialPosition = GetDial(knob);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == pz69DialPosition && dcsBiosBinding.RadioPanelPZ69Knob == knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSBindingPZ69();
                dcsBiosBinding.RadioPanelPZ69Knob = knob;
                dcsBiosBinding.DialPosition = pz69DialPosition;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
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
            IsDirtyMethod();
        }

        private void RemoveRadioPanelKnobFromList(ControlListPZ69 controlListPZ69, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, bool whenTurnedOn)
        {
            var found = false;
            var pz69DialPosition = GetDial(radioPanelPZ69Knob);
            if (controlListPZ69 == ControlListPZ69.ALL || controlListPZ69 == ControlListPZ69.KEYS)
            {
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBinding.WhenTurnedOn == whenTurnedOn && keyBinding.DialPosition == pz69DialPosition)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    found = true;
                    break;
                }
            }
            if (controlListPZ69 == ControlListPZ69.ALL || controlListPZ69 == ControlListPZ69.BIPS)
            {
                foreach (var bipLink in _bipLinks)
                {
                    if (bipLink.RadioPanelPZ69Knob == radioPanelPZ69Knob && bipLink.WhenTurnedOn == whenTurnedOn)
                    {
                        bipLink.BIPLights.Clear();
                    }
                    found = true;
                    break;
                }
            }

            if (found)
            {
                IsDirtyMethod();
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
        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        public void Clear(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            IsDirtyMethod();
        }

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 3)
            {
                Array.Copy(_newRadioPanelValue, _oldRadioPanelValue, 3);
                Array.Copy(report.Data, _newRadioPanelValue, 3);
                var hashSet = GetHashSetOfSwitchedKeys(_oldRadioPanelValue, _newRadioPanelValue);
                PZ69KnobChanged(hashSet);
                OnSwitchesChanged(hashSet);
                _isFirstNotification = false;
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



        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }


        private HashSet<object> GetHashSetOfSwitchedKeys(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            for (var i = 0; i < 3; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var radioPanelKey in _radioPanelKnobs)
                {
                    if (radioPanelKey.Group == i && (FlagHasChanged(oldByte, newByte, radioPanelKey.Mask) || _isFirstNotification))
                    {
                        radioPanelKey.IsOn = FlagValue(newValue, radioPanelKey);
                        result.Add(radioPanelKey);
                    }
                }
            }
            return result;
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelPZ69KnobEmulator radioPanelKey)
        {
            return (currentValue[radioPanelKey.Group] & radioPanelKey.Mask) > 0;
        }

        private void CreateSwitchKeys()
        {
            _radioPanelKnobs = RadioPanelPZ69KnobEmulator.GetRadioPanelKnobs();
        }

        /*public HashSet<DCSBIOSBindingPZ69> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
            set { _dcsBiosBindings = value; }
        }*/

        public override string SettingsVersion()
        {
            return "0X";
        }

        public HashSet<DCSBIOSBindingLCDPZ69> LCDBindings
        {
            get => _dcsBiosLcdBindings;
            set => _dcsBiosLcdBindings = value;
        }

        public HashSet<DCSBIOSBindingPZ69> DCSBIOSBindings
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
