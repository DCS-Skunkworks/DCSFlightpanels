using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Saitek;

namespace NonVisuals.Radios
{

    public class RadioPanelPZ69Emulator : RadioPanelPZ69Base
    {

        /*
         * For a specific toggle/switch/lever/knob the PZ69 can have :
         * - single key binding
         * - sequenced key binding
         * - DCS-BIOS control
         */
        //private HashSet<DCSBIOSBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ69>();
        private HashSet<KeyBindingPZ69> _keyBindings = new HashSet<KeyBindingPZ69>();
        private HashSet<OSCommandBindingPZ69Emulator> _osCommandBindings = new HashSet<OSCommandBindingPZ69Emulator>();
        private readonly HashSet<RadioPanelPZ69DisplayValue> _displayValues = new HashSet<RadioPanelPZ69DisplayValue>();
        private readonly HashSet<BIPLinkPZ69> _bipLinks = new HashSet<BIPLinkPZ69>();
        private readonly byte[] _oldRadioPanelValue = { 0, 0, 0 };
        private readonly byte[] _newRadioPanelValue = { 0, 0, 0 };
        private readonly object _dcsBiosDataReceivedLock = new object();

        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesUpper = new List<RadioPanelPZ69KnobsEmulator>() { RadioPanelPZ69KnobsEmulator.UpperCOM1, RadioPanelPZ69KnobsEmulator.UpperCOM2, RadioPanelPZ69KnobsEmulator.UpperNAV1, RadioPanelPZ69KnobsEmulator.UpperNAV2, RadioPanelPZ69KnobsEmulator.UpperADF, RadioPanelPZ69KnobsEmulator.UpperDME, RadioPanelPZ69KnobsEmulator.UpperXPDR };
        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesLower = new List<RadioPanelPZ69KnobsEmulator>() { RadioPanelPZ69KnobsEmulator.LowerCOM1, RadioPanelPZ69KnobsEmulator.LowerCOM2, RadioPanelPZ69KnobsEmulator.LowerNAV1, RadioPanelPZ69KnobsEmulator.LowerNAV2, RadioPanelPZ69KnobsEmulator.LowerADF, RadioPanelPZ69KnobsEmulator.LowerDME, RadioPanelPZ69KnobsEmulator.LowerXPDR };
        private double _upperActive = -1;
        private double _upperStandby = -1;
        private double _lowerActive = -1;
        private double _lowerStandby = -1;

        public RadioPanelPZ69Emulator(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                    ReadBindingHash(setting);

                    if (setting.StartsWith("RadioPanelKey{"))
                    {
                        var keyBinding = new KeyBindingPZ69();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("RadioPanelOSPZ69"))
                    {
                        var osCommand = new OSCommandBindingPZ69Emulator();
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
                }
            }

            _keyBindings = KeyBindingPZ69.SetNegators(_keyBindings);
            SettingsApplied();
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
            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterProfileData(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {

            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(e.Address, e.Data);
            }

        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _osCommandBindings.Clear();
            _displayValues.Clear();
            _bipLinks.Clear();
        }

        public HashSet<KeyBindingPZ69> KeyBindingsHashSet => _keyBindings;

        public HashSet<OSCommandBindingPZ69Emulator> OSCommandHashSet
        {
            get => _osCommandBindings;
            set => _osCommandBindings = value;
        }

        public HashSet<BIPLinkPZ69> BipLinkHashSet => _bipLinks;

        public HashSet<RadioPanelPZ69DisplayValue> DisplayValueHashSet => _displayValues;


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
                    foreach (var keyBinding in _keyBindings)
                    {
                        if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            keyBinding.OSKeyPress.Execute(new CancellationToken());
                            break;
                        }
                    }
                    foreach (var osCommand in _osCommandBindings)
                    {
                        if (osCommand.OSCommandObject != null && osCommand.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && osCommand.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            osCommand.OSCommandObject.Execute(new CancellationToken());
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

        public void AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn)
        {
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new RadioPanelPZ69KeyOnOff(radioPanelPZ69Knob, whenTurnedOn);
                ClearAllBindings(tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new KeyPress(keys, keyPressLength);
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var keyBinding = new KeyBindingPZ69();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69Knob;
                keyBinding.OSKeyPress = new KeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            _keyBindings = KeyBindingPZ69.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public void AddOrUpdateOSCommandBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, OSCommand osCommand, bool whenTurnedOn)
        {
            //This must accept lists
            var found = false;

            foreach (var osCommandBinding in _osCommandBindings)
            {
                if (osCommandBinding.RadioPanelPZ69Key == radioPanelPZ69Knob && osCommandBinding.WhenTurnedOn == whenTurnedOn)
                {
                    osCommandBinding.OSCommandObject = osCommand;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var osCommandBindingPZ69 = new OSCommandBindingPZ69Emulator();
                osCommandBindingPZ69.RadioPanelPZ69Key = radioPanelPZ69Knob;
                osCommandBindingPZ69.OSCommandObject = osCommand;
                osCommandBindingPZ69.WhenTurnedOn = whenTurnedOn;
                _osCommandBindings.Add(osCommandBindingPZ69);
            }
            SetIsDirty();
        }

        public void ClearAllBindings(RadioPanelPZ69KeyOnOff radioPanelPZ69KnobOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KnobOnOff.RadioPanelPZ69Key && keyBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.ButtonState)
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

        public void AddOrUpdateSequencedKeyBinding(string information, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn)
        {
            if (sortedList.Count == 0)
            {
                RemoveRadioPanelKnobFromList(ControlListPZ69.KEYS, radioPanelPZ69Knob, whenTurnedOn);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new KeyPress(information, sortedList);
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var keyBinding = new KeyBindingPZ69();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69Knob;
                keyBinding.OSKeyPress = new KeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            _keyBindings = KeyBindingPZ69.SetNegators(_keyBindings);
            SetIsDirty();
        }


        public void AddOrUpdateBIPLinkKeyBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, BIPLinkPZ69 bipLinkPZ69, bool whenTurnedOn)
        {
            if (bipLinkPZ69.BIPLights.Count == 0)
            {
                RemoveRadioPanelKnobFromList(ControlListPZ69.BIPS, radioPanelPZ69Knob, whenTurnedOn);
                SetIsDirty();
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
            SetIsDirty();
        }

        private void RemoveRadioPanelKnobFromList(ControlListPZ69 controlListPZ69, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, bool whenTurnedOn)
        {
            var found = false;
            if (controlListPZ69 == ControlListPZ69.ALL || controlListPZ69 == ControlListPZ69.KEYS)
            {
                foreach (var keyBindingPZ55 in _keyBindings)
                {
                    if (keyBindingPZ55.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBindingPZ55.WhenTurnedOn == whenTurnedOn)
                    {
                        keyBindingPZ55.OSKeyPress = null;
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
                SetIsDirty();
            }
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
        
    }

    public enum ControlListPZ69 : byte
    {
        ALL,
        KEYS,
        BIPS,
        DCSBIOS,
        LCD
    }

}
