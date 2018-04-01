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

    public class RadioPanelPZ69Emulator : SaitekPanel
    {

        /*
         * For a specific toggle/switch/lever/knob the PZ55 can have :
         * - single key binding
         * - seqenced key binding
         * - DCS-BIOS control
         */
        //private HashSet<DCSBIOSBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ69>();
        private HashSet<KeyBindingPZ69> _keyBindings = new HashSet<KeyBindingPZ69>();
        private HashSet<RadioPanelPZ69KnobEmulator> _radioPanelKnobs = new HashSet<RadioPanelPZ69KnobEmulator>();
        private bool _isFirstNotification = true;
        private byte[] _oldRadioPanelValue = { 0, 0, 0 };
        private byte[] _newRadioPanelValue = { 0, 0, 0 };
        private object _dcsBiosDataReceivedLock = new object();

        private int _frequencyKnobSensitivity;

        protected volatile byte FrequencySensitivitySkipper;

        public RadioPanelPZ69Emulator(HIDSkeleton hidSkeleton) : base(SaitekPanelsEnum.PZ69RadioPanel, hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateSwitchKeys();
            Startup();
        }

        public override sealed void Startup()
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
/*                    else if (setting.StartsWith("RadioPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingPZ55 = new DCSBIOSBindingPZ69();
                        dcsBIOSBindingPZ55.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingPZ55);
                    }*/
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
/*            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                {
                    result.Add(dcsBiosBinding.ExportSettings());
                }
            }*/
            return result;
        }

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {

            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(address, data);
            }

        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            //_dcsBiosBindings.Clear();
        }

        public HashSet<KeyBindingPZ69> KeyBindingsHashSet
        {
            get { return _keyBindings; }
            set { _keyBindings = value; }
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
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var radioPanelKeyObject in hashSet)
            {
                //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var radioPanelKey = (RadioPanelPZ69KnobEmulator)radioPanelKeyObject;
                var found = false;

                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                    {
                        keyBinding.OSKeyPress.Execute();
                        found = true;
                        break;
                    }
                }
                /*if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && dcsBiosBinding.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }*/
            }
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
            /*foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.RadioPanelPZ69Knob == radioPanelPZ69KnobOnOff.RadioPanelPZ69Key && dcsBiosBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.On)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }*/
            Common.DebugP("RadioPanelPZ69Emulator _keyBindings : " + _keyBindings.Count);
            //Common.DebugP("RadioPanelPZ69Emulator _dcsBiosBindings : " + _dcsBiosBindings.Count);
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
        /*
        public void AddOrUpdateDCSBIOSBinding(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
            var found = false;
            RemoveRadioPanelKeyFromList(1, radioPanelPZ69Knob, whenTurnedOn);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.RadioPanelPZ69Knob == radioPanelPZ69Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                dcsBiosBinding.RadioPanelPZ69Knob = radioPanelPZ69Knob;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }
        */
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
                        /*foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.RadioPanelPZ69Knob == radioPanelPZ69Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }*/
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

        private void DeviceAttachedHandler()
        {
            Startup();
            OnDeviceAttached();
        }

        private void DeviceRemovedHandler()
        {
            Shutdown();
            OnDeviceDetached();
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

        public int FrequencyKnobSensitivity
        {
            get { return _frequencyKnobSensitivity; }
            set { _frequencyKnobSensitivity = value; }
        }


        protected bool SkipCurrentFrequencyChange()
        {
            switch (FrequencyKnobSensitivity)
            {
                case 0:
                    {
                        //Do nothing all manipulation is let through
                        break;
                    }
                case -1:
                    {
                        //Skip every 2 manipulations
                        FrequencySensitivitySkipper++;
                        if (FrequencySensitivitySkipper <= 2)
                        {
                            return true;
                        }
                        FrequencySensitivitySkipper = 0;
                        break;
                    }
                case -2:
                    {
                        //Skip every 4 manipulations
                        FrequencySensitivitySkipper++;
                        if (FrequencySensitivitySkipper <= 4)
                        {
                            return true;
                        }
                        FrequencySensitivitySkipper = 0;
                        break;
                    }
            }
            return false;
        }
    }


}
