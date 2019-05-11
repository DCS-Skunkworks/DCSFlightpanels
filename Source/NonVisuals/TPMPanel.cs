using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{

    public class TPMPanel : SaitekPanel
    {

        /*
         * For a switch the TPM can have :
         * - single key binding
         * - seqenced key binding
         * - DCS-BIOS control
         */
        private HashSet<DCSBIOSBindingTPM> _dcsBiosBindings = new HashSet<DCSBIOSBindingTPM>();
        private HashSet<KeyBindingTPM> _keyBindings = new HashSet<KeyBindingTPM>();
        private HashSet<BIPLinkTPM> _bipLinks = new HashSet<BIPLinkTPM>();
        private HashSet<TPMPanelSwitch> _tpmPanelSwitches = new HashSet<TPMPanelSwitch>();
        private bool _isFirstNotification = true;
        private readonly byte[] _oldTPMPanelValue = { 0, 0, 0, 0, 0 };
        private readonly byte[] _newTPMPanelValue = { 0, 0, 0, 0, 0 };
        //private HidDevice _hidReadDevice;
        private readonly object _dcsBiosDataReceivedLock = new object();

        public TPMPanel(HIDSkeleton hidSkeleton) : base(SaitekPanelsEnum.TPM, hidSkeleton)
        {
            if (hidSkeleton.PanelType != SaitekPanelsEnum.TPM)
            {
                throw new ArgumentException();
            }
            //Fixed values
            VendorId = 0x6A3;
            ProductId = 0xB4D;
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
                Common.DebugP("TPMPanel.StartUp() : " + ex.Message);
                Common.LogError(321654, ex);
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
                    if (setting.StartsWith("TPMPanelSwitch{"))
                    {
                        var keyBinding = new KeyBindingTPM();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("TPMPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingTPM = new DCSBIOSBindingTPM();
                        dcsBIOSBindingTPM.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingTPM);
                    }
                    else if (setting.StartsWith("TPMPanelBipLink{"))
                    {
                        var tmpBipLink = new BIPLinkTPM();
                        tmpBipLink.ImportSettings(setting);
                        _bipLinks.Add(tmpBipLink);
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
            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.BIPLights.Count > 0)
                {
                    result.Add(bipLink.ExportSettings());
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

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _dcsBiosBindings.Clear();
            _bipLinks.Clear();
        }

        public HashSet<KeyBindingTPM> KeyBindingsHashSet
        {
            get => _keyBindings;
            set => _keyBindings = value;
        }

        public HashSet<BIPLinkTPM> BipLinkHashSet
        {
            get => _bipLinks;
            set => _bipLinks = value;
        }

        private void TPMSwitchChanged(TPMPanelSwitch tpmPanelSwitch)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                {
                    keyBinding.OSKeyPress.Execute();
                }
            }
        }

        private void TPMSwitchChanged(IEnumerable<object> hashSet)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }
            foreach (var tpmPanelSwitchObject in hashSet)
            {
                var tpmPanelSwitch = (TPMPanelSwitch)tpmPanelSwitchObject;
                var found = false;
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.OSKeyPress != null && keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                    {
                        keyBinding.OSKeyPress.Execute();
                        found = true;
                        break;
                    }
                }
                foreach (var bipLinkTPM in _bipLinks)
                {
                    if (bipLinkTPM.BIPLights.Count > 0 && bipLinkTPM.TPMSwitch == tpmPanelSwitch.TPMSwitch && bipLinkTPM.WhenTurnedOn == tpmPanelSwitch.IsOn)
                    {
                        bipLinkTPM.Execute();
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
            }
        }

        public string GetKeyPressForLoggingPurposes(TPMPanelSwitch tpmPanelSwitch)
        {
            var result = "";
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        public void AddOrUpdateSingleKeyBinding(TPMPanelSwitches tpmPanelSwitch, string keys, KeyPressLength keyPressLength, bool whenTurnedOn)
        {
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new TPMPanelSwitchOnOff(tpmPanelSwitch, whenTurnedOn);
                ClearAllBindings(tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch && keyBinding.WhenTurnedOn == whenTurnedOn)
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
                var keyBinding = new KeyBindingTPM();
                keyBinding.TPMSwitch = tpmPanelSwitch;
                keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            Common.DebugP("TPMPanel _keyBindings : " + _keyBindings.Count);
            IsDirtyMethod();
        }

        public void ClearAllBindings(TPMPanelSwitchOnOff tpmPanelSwitchOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitchOnOff.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.TPMSwitch == tpmPanelSwitchOnOff.TPMSwitch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("TPMPanel _keyBindings : " + _keyBindings.Count);
            Common.DebugP("TPMPanel _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, TPMPanelSwitches tpmPanelSwitch, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn)
        {
            if (sortedList.Count == 0)
            {
                RemoveTPMPanelSwitchFromList(ControlListTPM.KEYS, tpmPanelSwitch, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch && keyBinding.WhenTurnedOn == whenTurnedOn)
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
                var keyBinding = new KeyBindingTPM();
                keyBinding.TPMSwitch = tpmPanelSwitch;
                keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
        }


        public void AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches tpmPanelSwitch, BIPLinkTPM bipLinkTPM, bool whenTurnedOn)
        {
            if (bipLinkTPM.BIPLights.Count == 0)
            {
                RemoveTPMPanelSwitchFromList(ControlListTPM.BIPS, tpmPanelSwitch, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;

            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.TPMSwitch == tpmPanelSwitch && bipLink.WhenTurnedOn == whenTurnedOn)
                {
                    bipLink.BIPLights = bipLinkTPM.BIPLights;
                    bipLink.Description = bipLinkTPM.Description;
                    bipLink.TPMSwitch = tpmPanelSwitch;
                    bipLink.WhenTurnedOn = whenTurnedOn;
                    found = true;
                    break;
                }
            }
            if (!found && bipLinkTPM.BIPLights.Count > 0)
            {
                bipLinkTPM.TPMSwitch = tpmPanelSwitch;
                bipLinkTPM.WhenTurnedOn = whenTurnedOn;
                _bipLinks.Add(bipLinkTPM);
            }
            IsDirtyMethod();
        }


        public void AddOrUpdateDCSBIOSBinding(TPMPanelSwitches tpmPanelSwitch, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn)
        {
            if (dcsbiosInputs.Count == 0)
            {
                RemoveTPMPanelSwitchFromList(ControlListTPM.DCSBIOS, tpmPanelSwitch, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
            var found = false;
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.TPMSwitch == tpmPanelSwitch && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                var dcsBiosBinding = new DCSBIOSBindingTPM();
                dcsBiosBinding.TPMSwitch = tpmPanelSwitch;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }
        
        public void RemoveTPMPanelSwitchFromList(ControlListTPM controlListTPM, TPMPanelSwitches tpmPanelSwitch, bool whenTurnedOn)
        {
            var found = false;
            if (controlListTPM == ControlListTPM.ALL || controlListTPM == ControlListTPM.KEYS)
            {
                foreach (var keyBindingTPM in _keyBindings)
                {
                    if (keyBindingTPM.TPMSwitch == tpmPanelSwitch && keyBindingTPM.WhenTurnedOn == whenTurnedOn)
                    {
                        keyBindingTPM.OSKeyPress = null;
                        found = true;
                    }
                }
            }
            if (controlListTPM == ControlListTPM.ALL || controlListTPM == ControlListTPM.DCSBIOS)
            {
                foreach (var dcsBiosBinding in _dcsBiosBindings)
                {
                    if (dcsBiosBinding.TPMSwitch == tpmPanelSwitch && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                    {
                        dcsBiosBinding.DCSBIOSInputs.Clear();
                        found = true;
                    }
                }
            }
            if (controlListTPM == ControlListTPM.ALL || controlListTPM == ControlListTPM.BIPS)
            {
                foreach (var bipLink in _bipLinks)
                {
                    if (bipLink.TPMSwitch == tpmPanelSwitch && bipLink.WhenTurnedOn == whenTurnedOn)
                    {
                        bipLink.BIPLights.Clear();
                        found = true;
                    }
                }
            }

            if (found)
            {
                IsDirtyMethod();
            }
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        public void Clear(TPMPanelSwitches tpmPanelSwitch)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            IsDirtyMethod();
        }

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 5)
            {
                Array.Copy(_newTPMPanelValue, _oldTPMPanelValue, 5);
                Array.Copy(report.Data, _newTPMPanelValue, 5);
                var hashSet = GetHashSetOfChangedSwitches(_oldTPMPanelValue, _newTPMPanelValue);
                TPMSwitchChanged(hashSet);
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
                        foreach (var tpmPanelSwitch in hashSet)
                        {
                            Common.DebugP(((TPMPanelSwitch)tpmPanelSwitch).TPMSwitch + ", value is " + FlagValue(_newTPMPanelValue, ((TPMPanelSwitch)tpmPanelSwitch)));
                        }
                    }
                }*/
                Common.DebugP("\r\nDone!\r\n");
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

        private HashSet<object> GetHashSetOfChangedSwitches(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();



            for (var i = 0; i < 5; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var tpmPanelSwitch in _tpmPanelSwitches)
                {
                    if (tpmPanelSwitch.Group == i && (FlagHasChanged(oldByte, newByte, tpmPanelSwitch.Mask) || _isFirstNotification))
                    {
                        tpmPanelSwitch.IsOn = FlagValue(newValue, tpmPanelSwitch);
                        result.Add(tpmPanelSwitch);
                    }
                }
            }
            return result;
        }

        private static bool FlagValue(byte[] currentValue, TPMPanelSwitch tpmPanelSwitch)
        {
            return (currentValue[tpmPanelSwitch.Group] & tpmPanelSwitch.Mask) > 0;
        }

        private void CreateSwitchKeys()
        {
            _tpmPanelSwitches = TPMPanelSwitch.GetTPMPanelSwitches();
        }

        public HashSet<DCSBIOSBindingTPM> DCSBiosBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

        public override string SettingsVersion()
        {
            return "0X";
        }
    }



    public enum ControlListTPM : byte
    {
        ALL,
        DCSBIOS,
        KEYS,
        BIPS
    }
}
