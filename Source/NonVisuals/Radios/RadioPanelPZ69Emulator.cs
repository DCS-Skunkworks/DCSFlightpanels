using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.Key;
using NonVisuals.BindingClasses.OSCommand;
using NonVisuals.KeyEmulation;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;

    using MEF;

    using EventArgs;
    using Plugin;
    using Knobs;
    using Misc;
    using NonVisuals.Panels.Saitek.Panels;
    using Panels.Saitek;
    using HID;
    
    /// <summary>
    /// This is the class for the PZ69 Radio Panel that is used when the user has a key emulation only profile open.
    /// </summary>
    public class RadioPanelPZ69Emulator : RadioPanelPZ69Base
    {
        private readonly HashSet<RadioPanelPZ69DisplayValue> _displayValues = new();
        private readonly HashSet<BIPLinkPZ69> _bipLinks = new();
        private readonly object _dcsBiosDataReceivedLock = new();

        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesUpper = new() { RadioPanelPZ69KnobsEmulator.UpperCOM1, RadioPanelPZ69KnobsEmulator.UpperCOM2, RadioPanelPZ69KnobsEmulator.UpperNAV1, RadioPanelPZ69KnobsEmulator.UpperNAV2, RadioPanelPZ69KnobsEmulator.UpperADF, RadioPanelPZ69KnobsEmulator.UpperDME, RadioPanelPZ69KnobsEmulator.UpperXPDR };
        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesLower = new() { RadioPanelPZ69KnobsEmulator.LowerCOM1, RadioPanelPZ69KnobsEmulator.LowerCOM2, RadioPanelPZ69KnobsEmulator.LowerNAV1, RadioPanelPZ69KnobsEmulator.LowerNAV2, RadioPanelPZ69KnobsEmulator.LowerADF, RadioPanelPZ69KnobsEmulator.LowerDME, RadioPanelPZ69KnobsEmulator.LowerXPDR };

        private HashSet<KeyBindingPZ69> _keyBindings = new();
        private List<OSCommandBindingPZ69Emulator> _operatingSystemCommandBindings = new();

        private double _upperActive = -1;
        private double _upperStandby = -1;
        private double _lowerActive = -1;
        private double _lowerStandby = -1;

        public RadioPanelPZ69Emulator(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {}

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void InitPanel()
        {
            CreateSwitchKeys();
            StartListeningForHidPanelChanges();
        }
        
        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            List<string> settings = genericPanelBinding.Settings;

            foreach (string setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2)
                {
                    if (setting.StartsWith("RadioPanelKey{"))
                    {
                        var keyBinding = new KeyBindingPZ69();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("RadioPanelOSPZ69"))
                    {
                        var operatingSystemCommand = new OSCommandBindingPZ69Emulator();
                        operatingSystemCommand.ImportSettings(setting);
                        _operatingSystemCommandBindings.Add(operatingSystemCommand);
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
                }
            }

            _keyBindings = KeyBindingPZ69.SetNegators(_keyBindings);
            AppEventHandler.SettingsApplied(this, HIDSkeletonBase.HIDInstance, TypeOfPanel);
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }

            List<string> result = new();

            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null)
                {
                    result.Add(keyBinding.ExportSettings());
                }
            }

            foreach (var operatingSystemCommand in _operatingSystemCommandBindings)
            {
                if (!operatingSystemCommand.OSCommandObject.IsEmpty)
                {
                    result.Add(operatingSystemCommand.ExportSettings());
                }
            }

            foreach (var displayValue in _displayValues)
            {
                string tmp = displayValue.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }

            foreach (var bipLink in _bipLinks)
            {
                string tmp = bipLink.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }

            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerCaller.RegisterPanelBinding(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {

            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(e.Address, e.Data);
            }

        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _keyBindings.Clear();
            _operatingSystemCommandBindings.Clear();
            _displayValues.Clear();
            _bipLinks.Clear();

            if (setIsDirty)
            {
                SetIsDirty();
            }
        }

        public HashSet<KeyBindingPZ69> KeyBindingsHashSet => _keyBindings;

        public List<OSCommandBindingPZ69Emulator> OSCommandHashSet
        {
            get => _operatingSystemCommandBindings;
            set => _operatingSystemCommandBindings = value;
        }

        public HashSet<BIPLinkPZ69> BipLinkHashSet => _bipLinks;

        public HashSet<RadioPanelPZ69DisplayValue> DisplayValueHashSet => _displayValues;

        protected override void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            if (ForwardPanelEvent)
            {
                foreach (object radioPanelKeyObject in hashSet)
                {
                    // Looks which switches has been switched and sees whether any key emulation has been tied to them.
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

                    bool keyBindingFound = false;
                    foreach (KeyBindingPZ69 keyBinding in _keyBindings)
                    {
                        if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            keyBindingFound = true;
                            if (!PluginManager.DisableKeyboardAPI)
                            {
                                keyBinding.OSKeyPress.Execute(new CancellationToken());
                            }

                            if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                            {
                                PluginManager.DoEvent(
                                    DCSAircraft.SelectedAircraft.Description, 
                                    HIDInstance, 
                                    PluginGamingPanelEnum.PZ69RadioPanel, 
                                    (int)radioPanelKey.RadioPanelPZ69Knob, 
                                    radioPanelKey.IsOn, 
                                    keyBinding.OSKeyPress.KeyPressSequence);
                            }
                            break;
                        }
                    }

                    // This is needed because there may not be key bindings configured, plugin should get the panel event regardless
                    // Just that we don't send any keypress configs this time.
                    if (!keyBindingFound && PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        PluginManager.DoEvent(
                            DCSAircraft.SelectedAircraft.Description,
                            HIDInstance,
                            PluginGamingPanelEnum.PZ69RadioPanel,
                            (int)radioPanelKey.RadioPanelPZ69Knob,
                            radioPanelKey.IsOn,
                            null);
                    }

                    foreach (OSCommandBindingPZ69Emulator operatingSystemCommand in _operatingSystemCommandBindings)
                    {
                        if (operatingSystemCommand.OSCommandObject != null && operatingSystemCommand.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && operatingSystemCommand.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            operatingSystemCommand.OSCommandObject.ExecuteCommand(new CancellationToken());
                            break;
                        }
                    }

                    foreach (BIPLinkPZ69 bipLinkPZ69 in _bipLinks)
                    {
                        if (bipLinkPZ69.BIPLights.Count > 0 && bipLinkPZ69.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && bipLinkPZ69.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            bipLinkPZ69.Execute();
                            break;
                        }
                    }
                }
            }

            foreach (object radioPanelKeyObject in hashSet)
            {
                // Looks which switches has been switched and sees whether any key emulation has been tied to them.
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

                    foreach (RadioPanelPZ69DisplayValue displayValue in _displayValues)
                    {
                        if (displayValue.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob)
                        {
                            double parsedValue = double.Parse(displayValue.Value, NumberFormatInfoFullDisplay);

                            if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                            {
                                _upperActive = parsedValue;
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                            {
                                _upperStandby = parsedValue;
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                            {
                                _lowerActive = parsedValue;
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                            {
                                _lowerStandby = parsedValue;
                            }
                        }
                    }
                    ShowFrequenciesOnPanel();
                }
            }
        }

        private void ShowFrequenciesOnPanel()
        {
            var bytes = new byte[21];
            bytes[0] = 0x0;
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

        public string GetKeyPressForLoggingPurposes(RadioPanelPZ69KnobEmulator radioPanelKey)
        {
            string result = string.Empty;
            foreach (KeyBindingPZ69 keyBinding in _keyBindings)
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

            double value = double.Parse(valueAsString, NumberFormatInfoFullDisplay);
            if (value < 0)
            {
                ClearDisplayValue(radioPanelPZ69Knob, radioPanelDisplay);
                return;
            }

            bool found = false;
            foreach (RadioPanelPZ69DisplayValue displayValue in _displayValues)
            {
                if (displayValue.RadioPanelPZ69Knob == radioPanelPZ69Knob && displayValue.RadioPanelDisplay == radioPanelDisplay)
                {
                    displayValue.Value = valueAsString;
                    found = true;
                }
            }

            if (!found)
            {
                RadioPanelPZ69DisplayValue displayValue = new() 
                {
                    RadioPanelPZ69Knob = radioPanelPZ69Knob,
                    RadioPanelDisplay = radioPanelDisplay,
                    Value = valueAsString
                };
                _displayValues.Add(displayValue);
            }

            SetIsDirty();
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
            var pz69SwitchOnOff  = (PZ69SwitchOnOff) panelSwitchOnOff;

            if (string.IsNullOrEmpty(keyPress))
            {
                ClearAllBindings(pz69SwitchOnOff);
                return;
            }

            bool found = false;
            foreach (KeyBindingPZ69 keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && keyBinding.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
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
                KeyBindingPZ69 keyBinding = new() 
                {
                    RadioPanelPZ69Key = pz69SwitchOnOff.Switch,
                    OSKeyPress = new KeyPress(keyPress, keyPressLength),
                    WhenTurnedOn = pz69SwitchOnOff.ButtonState
                };
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingPZ69.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
            var pz69SwitchOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlList.KEYS, pz69SwitchOnOff);
                SetIsDirty();
                return;
            }

            bool found = false;
            foreach (KeyBindingPZ69 keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && keyBinding.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
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
                KeyBindingPZ69 keyBinding = new() 
                {
                    RadioPanelPZ69Key = pz69SwitchOnOff.Switch,
                    OSKeyPress = new KeyPress(description, keySequence),
                    WhenTurnedOn = pz69SwitchOnOff.ButtonState
                };
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingPZ69.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced)
        {
            throw new NotImplementedException();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink)
        {
            var pz69SwitchOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var bipLinkPZ69 = (BIPLinkPZ69) bipLink;

            if (bipLinkPZ69.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlList.BIPS, pz69SwitchOnOff);
                SetIsDirty();
                return;
            }

            bool found = false;
            foreach (BIPLinkPZ69 tmpBipLink in _bipLinks)
            {
                if (tmpBipLink.RadioPanelPZ69Knob == pz69SwitchOnOff.Switch && tmpBipLink.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                {
                    tmpBipLink.BIPLights = bipLinkPZ69.BIPLights;
                    tmpBipLink.Description = bipLinkPZ69.Description;
                    tmpBipLink.RadioPanelPZ69Knob = pz69SwitchOnOff.Switch;
                    found = true;
                    break;
                }
            }

            if (!found && bipLinkPZ69.BIPLights.Count > 0)
            {
                bipLinkPZ69.RadioPanelPZ69Knob = pz69SwitchOnOff.Switch;
                bipLinkPZ69.WhenTurnedOn = pz69SwitchOnOff.ButtonState;
                _bipLinks.Add(bipLinkPZ69);
            }

            SetIsDirty();
        }
        
        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
            var pz69SwitchOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;

            bool found = false;
            foreach (var operatingSystemCommandBinding in _operatingSystemCommandBindings)
            {
                if (operatingSystemCommandBinding.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && operatingSystemCommandBinding.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                {
                    operatingSystemCommandBinding.OSCommandObject = operatingSystemCommand;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                OSCommandBindingPZ69Emulator operatingSystemCommandBindingPZ69 = new() 
                {
                    RadioPanelPZ69Key = pz69SwitchOnOff.Switch,
                    OSCommandObject = operatingSystemCommand,
                    WhenTurnedOn = pz69SwitchOnOff.ButtonState
                };
                _operatingSystemCommandBindings.Add(operatingSystemCommandBindingPZ69);
            }

            SetIsDirty();
        }

        public void ClearAllBindings(PZ69SwitchOnOff radioPanelPZ69KnobOnOff)
        {
            foreach (KeyBindingPZ69 keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KnobOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.ButtonState)
                {
                    keyBinding.OSKeyPress = null;
                }
            }

            SetIsDirty();
        }

        public void ClearDisplayValue(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, RadioPanelPZ69Display radioPanelPZ69Display)
        {
            _displayValues.RemoveWhere(x => x.RadioPanelPZ69Knob == radioPanelPZ69Knob && x.RadioPanelDisplay == radioPanelPZ69Display);
            SetIsDirty();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
            var controlListPZ69 = (ControlList) controlList;
            var pz69SwitchOnOff = (PZ69SwitchOnOff) panelSwitchOnOff;
            bool found = false;
            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.KEYS)
            {
                foreach (KeyBindingPZ69 keyBindingPZ69 in _keyBindings)
                {
                    if (keyBindingPZ69.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && keyBindingPZ69.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                    {
                        keyBindingPZ69.OSKeyPress = null;
                    }
                    found = true;
                    break;
                }
            }

            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.BIPS)
            {
                foreach (BIPLinkPZ69 bipLink in _bipLinks)
                {
                    if (bipLink.RadioPanelPZ69Knob == pz69SwitchOnOff.Switch && bipLink.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                    {
                        bipLink.BIPLights.Clear();
                    }
                    found = true;
                    break;
                }
            }

            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.OSCOMMANDS)
            {
                OSCommandBindingPZ69Emulator operatingSystemCommandBindingPZ69 = null;
                for (int i = 0; i < _operatingSystemCommandBindings.Count; i++)
                {
                    var operatingSystemCommand = _operatingSystemCommandBindings[i];

                    if (operatingSystemCommand.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && operatingSystemCommand.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                    {
                        operatingSystemCommandBindingPZ69 = _operatingSystemCommandBindings[i];
                        found = true;
                    }
                }

                if (operatingSystemCommandBindingPZ69 != null)
                {
                    _operatingSystemCommandBindings.Remove(operatingSystemCommandBindingPZ69);
                }
            }

            if (found)
            {
                SetIsDirty();
            }
        }

        public void Clear(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob)
        {
            foreach (KeyBindingPZ69 keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            SetIsDirty();
        }
        

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateSwitchKeys()
        {
            SaitekPanelKnobs = RadioPanelPZ69KnobEmulator.GetRadioPanelKnobs();
        }
    }
}
