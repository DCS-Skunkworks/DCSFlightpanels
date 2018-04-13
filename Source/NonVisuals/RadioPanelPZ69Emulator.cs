using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{

    public class RadioPanelPZ69Emulator : RadioPanelPZ69Base
    {

        /*
         * For a specific toggle/switch/lever/knob the PZ55 can have :
         * - single key binding
         * - seqenced key binding
         * - DCS-BIOS control
         */
        //private HashSet<DCSBIOSBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ69>();
        private HashSet<KeyBindingPZ69> _keyBindings = new HashSet<KeyBindingPZ69>();
        private HashSet<RadioPanelPZ69DisplayValue> _displayValues = new HashSet<RadioPanelPZ69DisplayValue>();
        private HashSet<RadioPanelPZ69KnobEmulator> _radioPanelKnobs = new HashSet<RadioPanelPZ69KnobEmulator>();
        private bool _isFirstNotification = true;
        private byte[] _oldRadioPanelValue = { 0, 0, 0 };
        private byte[] _newRadioPanelValue = { 0, 0, 0 };
        private object _dcsBiosDataReceivedLock = new object();

        private List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesUpper = new List<RadioPanelPZ69KnobsEmulator>(){RadioPanelPZ69KnobsEmulator.UpperCOM1, RadioPanelPZ69KnobsEmulator.UpperCOM2, RadioPanelPZ69KnobsEmulator.UpperNAV1, RadioPanelPZ69KnobsEmulator.UpperNAV2, RadioPanelPZ69KnobsEmulator.UpperADF, RadioPanelPZ69KnobsEmulator.UpperDME, RadioPanelPZ69KnobsEmulator.UpperXPDR};
        private List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesLower = new List<RadioPanelPZ69KnobsEmulator>() { RadioPanelPZ69KnobsEmulator.LowerCOM1, RadioPanelPZ69KnobsEmulator.LowerCOM2, RadioPanelPZ69KnobsEmulator.LowerNAV1, RadioPanelPZ69KnobsEmulator.LowerNAV2, RadioPanelPZ69KnobsEmulator.LowerADF, RadioPanelPZ69KnobsEmulator.LowerDME, RadioPanelPZ69KnobsEmulator.LowerXPDR };
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
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69Emulator.StartUp() : " + ex.Message);
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
                    if (setting.StartsWith("RadioPanelKey{"))
                    {
                        var keyBinding = new KeyBindingPZ69();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("PZ69DisplayValue{"))
                    {
                        var radioPanelPZ69DisplayValue = new RadioPanelPZ69DisplayValue();
                        radioPanelPZ69DisplayValue.ImportSettings(setting);
                        _displayValues.Add(radioPanelPZ69DisplayValue);
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
            _displayValues.Clear();
        }

        public HashSet<KeyBindingPZ69> KeyBindingsHashSet
        {
            get { return _keyBindings; }
        }

        public HashSet<RadioPanelPZ69DisplayValue> DisplayValueHashSet
        {
            get { return _displayValues; }
        }

        private void PZ69SwitchChanged(RadioPanelPZ69KnobEmulator radioPanelKey)
        {
            if (!ForwardKeyPresses)
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

        private void PZ69SwitchChanged(IEnumerable<object> hashSet)
        {
            if (ForwardKeyPresses)
            {
                foreach (var radioPanelKeyObject in hashSet)
                {
                    //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                    var radioPanelKey = (RadioPanelPZ69KnobEmulator)radioPanelKeyObject;

                    foreach (var keyBinding in _keyBindings)
                    {
                        if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            keyBinding.OSKeyPress.Execute();
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
                                _upperActive =  double.Parse(displayValue.Value, Common.GetPZ69FullDisplayNumberFormat());
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
            IsDirtyMethod();
        }

        public void AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn = true)
        {
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(radioPanelPZ69Knob, whenTurnedOn);
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
                        keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var keyBinding = new KeyBindingPZ69();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69Knob;
                keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            Common.DebugP("RadioPanelPZ69Emulator _keyBindings : " + _keyBindings.Count);
            IsDirtyMethod();
        }

        public void ClearAllBindings(RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff radioPanelPZ69KnobOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KnobOnOff.RadioPanelPZ69Key && keyBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.On)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            IsDirtyMethod();
        }

        public void ClearDisplayValue(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, RadioPanelPZ69Display radioPanelPZ69Display)
        {
            //This must accept lists
            foreach (var displayValue in _displayValues)
            {
                if (displayValue.RadioPanelPZ69Knob == radioPanelPZ69Knob && displayValue.RadioPanelDisplay == radioPanelPZ69Display)
                {
                    displayValue.Value = null;
                }
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveRadioPanelKeyFromList(2, radioPanelPZ69Knob, whenTurnedOn);
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
                        keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var keyBinding = new KeyBindingPZ69();
                keyBinding.RadioPanelPZ69Key = radioPanelPZ69Knob;
                keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
        }
        private void RemoveRadioPanelKeyFromList(int list, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var keyBindingPZ55 in _keyBindings)
                        {
                            if (keyBindingPZ55.RadioPanelPZ69Key == radioPanelPZ69Knob && keyBindingPZ55.WhenTurnedOn == whenTurnedOn)
                            {
                                keyBindingPZ55.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        break;
                    }
            }
        }

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
                PZ69SwitchChanged(hashSet);
                OnSwitchesChanged(hashSet);
                _isFirstNotification = false;
                /*if (Common.Debug)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        stringBuilder.Append(report.Data[i] + " ");
                    }
                    Common.DebugP(stringBuilder.ToString());
                    if (hashSet.Count > 0)
                    {
                        Common.DebugP("\nFollowing switches has been changed:\n");
                        foreach (var radioPanelKey in hashSet)
                        {
                            Common.DebugP(((RadioPanelKey)radioPanelKey).RadioPanelPZ69EmulatorKey + ", value is " + FlagValue(_newRadioPanelValue, ((RadioPanelKey)radioPanelKey)));
                        }
                    }
                }
                Common.DebugP("\r\nDone!\r\n");*/
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

        public override String SettingsVersion()
        {
            return "0X";
        }

       
        }


}
