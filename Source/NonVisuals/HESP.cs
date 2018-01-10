using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{

    public class HESP : SaitekPanel
    {

        /*
         * For a specific button the HESP can have :
         * - single key binding
         * - seqenced key binding
         * - DCS-BIOS control
         */
        private HashSet<DCSBIOSBindingHESP> _dcsBiosBindings = new HashSet<DCSBIOSBindingHESP>();
        private HashSet<KeyBindingHESP> _keyBindings = new HashSet<KeyBindingHESP>();
        //public static HESP HESPSO;
        private HashSet<HESPKey> _hespKeys = new HashSet<HESPKey>();
        private bool _isFirstNotification = true;
        private byte[] _oldHESPValue = { 0, 0, 0 };
        private byte[] _newHESPValue = { 0, 0, 0 };
        //private HidDevice _hidReadDevice;
        //private HidDevice _hidWriteDevice;
        private List<DcsOutputAndColorBindingPZ55> _listColorOutputBinding = new List<DcsOutputAndColorBindingPZ55>();
        private object _dcsBiosDataReceivedLock = new object();
        private bool _manualLandingGearLeds;
        private Thread _manualLandingGearThread;

        public HESP(HIDSkeleton hidSkeleton) : base(SaitekPanelsEnum.HESP, hidSkeleton)
        {
            //Fixed values
            VendorId = 0xDDD6A3;
            ProductId = 0xDDDD67;
            CreateHESPKeys();
            //HESPSO = this;
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
                Common.DebugP("HESP.StartUp() : " + ex.Message);
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
                    if (setting.StartsWith("HESPKey{"))
                    {
                        var keyBinding = new KeyBindingHESP();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("HESPDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingHESP = new DCSBIOSBindingHESP();
                        dcsBIOSBindingHESP.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingHESP);
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
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                {
                    result.Add(dcsBiosBinding.ExportSettings());
                }
            }
            return result;
        }

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _listColorOutputBinding.Clear();
            _dcsBiosBindings.Clear();
        }

        public HashSet<KeyBindingHESP> KeyBindingsHashSet
        {
            get { return _keyBindings; }
            set { _keyBindings = value; }
        }

        private void HESPSwitchChanged(HESPKey hespKey)
        {
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.HESPKey == hespKey.Key && keyBinding.WhenTurnedOn == hespKey.IsOn)
                {
                    keyBinding.OSKeyPress.Execute();
                }
            }
        }
        
        private void HESPSwitchChanged(IEnumerable<object> hashSet)
        {
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var hespKeyObject in hashSet)
            {
                //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var hespKey = (HESPKey)hespKeyObject;
                var found = false;
                
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.OSKeyPress != null && keyBinding.HESPKey == hespKey.Key && keyBinding.WhenTurnedOn == hespKey.IsOn)
                    {
                        keyBinding.OSKeyPress.Execute();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.HESPKey == hespKey.Key && dcsBiosBinding.WhenTurnedOn == hespKey.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
            }
        }
        
        public string GetKeyPressForLoggingPurposes(HESPKey hespKey)
        {
            var result = "";
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.HESPKey == hespKey.Key && keyBinding.WhenTurnedOn == hespKey.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        public void AddOrUpdateSingleKeyBinding(HESPKeys hespKey, string keys, KeyPressLength keyPressLength, bool whenTurnedOn = true)
        {
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new HESPKeyOnOff(hespKey, whenTurnedOn);
                ClearAllBindings(tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.HESPKey == hespKey && keyBinding.WhenTurnedOn == whenTurnedOn)
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
                var keyBinding = new KeyBindingHESP();
                keyBinding.HESPKey = hespKey;
                keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            Common.DebugP("HESP _keyBindings : " + _keyBindings.Count);
            IsDirtyMethod();
        }

        public void ClearAllBindings(HESPKeyOnOff hespKeyOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.HESPKey == hespKeyOnOff.HESPKey && keyBinding.WhenTurnedOn == hespKeyOnOff.On)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.HESPKey == hespKeyOnOff.HESPKey && dcsBiosBinding.WhenTurnedOn == hespKeyOnOff.On)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("HESP _keyBindings : " + _keyBindings.Count);
            Common.DebugP("HESP _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, HESPKeys hespKey, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveHESPKeyFromList(2, hespKey, whenTurnedOn);
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.HESPKey == hespKey && keyBinding.WhenTurnedOn == whenTurnedOn)
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
                var keyBinding = new KeyBindingHESP();
                keyBinding.HESPKey = hespKey;
                keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSBinding(HESPKeys hespKey, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
            var found = false;
            RemoveHESPKeyFromList(1, hespKey, whenTurnedOn);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.HESPKey == hespKey && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                var dcsBiosBinding = new DCSBIOSBindingHESP();
                dcsBiosBinding.HESPKey = hespKey;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }

        private void RemoveHESPKeyFromList(int list, HESPKeys hespKey, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var keyBindingHESP in _keyBindings)
                        {
                            if (keyBindingHESP.HESPKey == hespKey && keyBindingHESP.WhenTurnedOn == whenTurnedOn)
                            {
                                keyBindingHESP.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.HESPKey == hespKey && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }
                        break;
                    }
            }
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        public void Clear(HESPKeys hespKey)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.HESPKey == hespKey)
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
                Array.Copy(_newHESPValue, _oldHESPValue, 3);
                Array.Copy(report.Data, _newHESPValue, 3);
                var hashSet = GetHashSetOfSwitchedKeys(_oldHESPValue, _newHESPValue);
                HESPSwitchChanged(hashSet);
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
            return null;
        }
        
        private HashSet<object> GetHashSetOfSwitchedKeys(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();




            for (var i = 0; i < 3; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var hespKey in _hespKeys)
                {
                    if (hespKey.Group == i && (FlagHasChanged(oldByte, newByte, hespKey.Mask) || _isFirstNotification))
                    {
                        result.Add(hespKey);
                    }
                }
            }
            return result;
        }

        private static bool FlagValue(byte[] currentValue, HESPKey hespKey)
        {
            return (currentValue[hespKey.Group] & hespKey.Mask) > 0;
        }

        private void CreateHESPKeys()
        {
            _hespKeys = HESPKey.GetHESPKeys();
        }

        public HashSet<DCSBIOSBindingHESP> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
            set { _dcsBiosBindings = value; }
        }

        public bool ManualLandingGearLeds
        {
            get { return _manualLandingGearLeds; }
            set
            {
                _manualLandingGearLeds = value;
                IsDirtyMethod();
            }
        }

        public override String SettingsVersion()
        {
            return "0X";
        }
    }


}
