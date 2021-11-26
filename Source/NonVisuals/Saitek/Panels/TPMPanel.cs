namespace NonVisuals.Saitek.Panels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;

    using MEF;
    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.EventArgs;
    using NonVisuals.Plugin;
    using NonVisuals.Saitek.Switches;

    public class TPMPanel : SaitekPanel
    {
        private readonly object _dcsBiosDataReceivedLock = new object();
        private HashSet<DCSBIOSActionBindingTPM> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingTPM>();
        private HashSet<KeyBindingTPM> _keyBindings = new HashSet<KeyBindingTPM>();
        private List<OSCommandBindingTPM> _operatingSystemCommandBindings = new List<OSCommandBindingTPM>();
        private HashSet<BIPLinkTPM> _bipLinks = new HashSet<BIPLinkTPM>();


        public TPMPanel(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.TPM, hidSkeleton)
        {
            if (hidSkeleton.PanelInfo.GamingPanelType != GamingPanelEnum.TPM)
            {
                throw new ArgumentException();
            }

            // Fixed values
            VendorId = 0x6A3;
            ProductId = 0xB4D;
            CreateSwitchKeys();
            Startup();
        }

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


        public sealed override void Startup()
        {
            try
            {
                StartListeningForPanelChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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

                    if (setting.StartsWith("TPMPanelSwitch{"))
                    {
                        var keyBinding = new KeyBindingTPM();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("TPMPanelOSCommand"))
                    {
                        var operatingSystemCommand = new OSCommandBindingTPM();
                        operatingSystemCommand.ImportSettings(setting);
                        _operatingSystemCommandBindings.Add(operatingSystemCommand);
                    }
                    else if (setting.StartsWith("TPMPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingTPM = new DCSBIOSActionBindingTPM();
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

            _keyBindings = KeyBindingTPM.SetNegators(_keyBindings);
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

            foreach (var operatingSystemCommand in _operatingSystemCommandBindings)
            {
                if (!operatingSystemCommand.OSCommandObject.IsEmpty)
                {
                    result.Add(operatingSystemCommand.ExportSettings());
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
            e.ProfileHandlerEA.RegisterPanelBinding(this, ExportSettings());
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e) { }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {

            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(e.Address, e.Data);
            }

        }

        public override void Identify()
        {
            // ignored
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _keyBindings.Clear();
            _operatingSystemCommandBindings.Clear();
            _dcsBiosBindings.Clear();
            _bipLinks.Clear();

            if (setIsDirty)
            {
                SetIsDirty();
            }
        }

        public HashSet<KeyBindingTPM> KeyBindingsHashSet
        {
            get => _keyBindings;
            set => _keyBindings = value;
        }

        public List<OSCommandBindingTPM> OSCommandHashSet
        {
            get => _operatingSystemCommandBindings;
            set => _operatingSystemCommandBindings = value;
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
                    keyBinding.OSKeyPress.Execute(new CancellationToken());
                }
            }
        }

        private void TPMSwitchChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }

            foreach (var tpmPanelSwitchObject in hashSet)
            {
                var tpmPanelSwitch = (TPMPanelSwitch)tpmPanelSwitchObject;
                var found = false;

                var keyBindingFound = false;
                foreach (var keyBinding in _keyBindings)
                {
                    if (!isFirstReport && keyBinding.OSKeyPress != null && keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                    {
                        found = true;
                        keyBindingFound = true;
                        if (!PluginManager.DisableKeyboardAPI)
                        {
                            keyBinding.OSKeyPress.Execute(new CancellationToken());
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                ProfileHandler.SelectedProfile().Description, 
                                HIDInstanceId, 
                                (int)PluginGamingPanelEnum.TPM, 
                                (int)tpmPanelSwitch.TPMSwitch, 
                                tpmPanelSwitch.IsOn,
                                keyBinding.OSKeyPress.KeyPressSequence);
                        }

                        break;
                    }
                }
                
                if (!isFirstReport && !keyBindingFound && PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    PluginManager.DoEvent(
                        ProfileHandler.SelectedProfile().Description,
                        HIDInstanceId,
                        (int)PluginGamingPanelEnum.TPM,
                        (int)tpmPanelSwitch.TPMSwitch,
                        tpmPanelSwitch.IsOn,
                        null);
                }

                foreach (var operatingSystemCommand in _operatingSystemCommandBindings)
                {
                    if (!isFirstReport && operatingSystemCommand.OSCommandObject != null && operatingSystemCommand.TPMSwitch == tpmPanelSwitch.TPMSwitch && operatingSystemCommand.WhenTurnedOn == tpmPanelSwitch.IsOn)
                    {
                        operatingSystemCommand.OSCommandObject.Execute(new CancellationToken());
                        found = true;
                        break;
                    }
                }

                foreach (var bipLinkTPM in _bipLinks)
                {
                    if (!isFirstReport && bipLinkTPM.BIPLights.Count > 0 && bipLinkTPM.TPMSwitch == tpmPanelSwitch.TPMSwitch && bipLinkTPM.WhenTurnedOn == tpmPanelSwitch.IsOn)
                    {
                        bipLinkTPM.Execute();
                        break;
                    }
                }

                if (!isFirstReport && !found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands(new CancellationToken());
                            break;
                        }
                    }
                }
            }
        }

        public string GetKeyPressForLoggingPurposes(TPMPanelSwitch tpmPanelSwitch)
        {
            var result = string.Empty;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }

            return result;
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
            var tpmPanelSwitchOnOff = (TPMSwitchOnOff)panelSwitchOnOff;
            if (string.IsNullOrEmpty(keyPress))
            {
                ClearAllBindings(tpmPanelSwitchOnOff);
                return;
            }

            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && keyBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
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
                var keyBinding = new KeyBindingTPM();
                keyBinding.TPMSwitch = tpmPanelSwitchOnOff.Switch;
                keyBinding.OSKeyPress = new KeyPress(keyPress, keyPressLength);
                keyBinding.WhenTurnedOn = tpmPanelSwitchOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingTPM.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
            var tpmPanelSwitchOnOff = (TPMSwitchOnOff)panelSwitchOnOff;

            // This must accept lists
            var found = false;

            foreach (var operatingSystemCommandBinding in _operatingSystemCommandBindings)
            {
                if (operatingSystemCommandBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && operatingSystemCommandBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    operatingSystemCommandBinding.OSCommandObject = operatingSystemCommand;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var operatingSystemCommandBindingTPM = new OSCommandBindingTPM();
                operatingSystemCommandBindingTPM.TPMSwitch = tpmPanelSwitchOnOff.Switch;
                operatingSystemCommandBindingTPM.OSCommandObject = operatingSystemCommand;
                operatingSystemCommandBindingTPM.WhenTurnedOn = tpmPanelSwitchOnOff.ButtonState;
                _operatingSystemCommandBindings.Add(operatingSystemCommandBindingTPM);
            }

            SetIsDirty();
        }

        public void ClearAllBindings(TPMSwitchOnOff tpmPanelSwitchOnOff)
        {
            // This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && keyBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    keyBinding.OSKeyPress = null;
                }
            }

            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }

            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
            var tpmPanelSwitchOnOff = (TPMSwitchOnOff)panelSwitchOnOff;
            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlListTPM.KEYS, tpmPanelSwitchOnOff);
                SetIsDirty();
                return;
            }

            // This must accept lists
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && keyBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
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
                var keyBinding = new KeyBindingTPM();
                keyBinding.TPMSwitch = tpmPanelSwitchOnOff.Switch;
                keyBinding.OSKeyPress = new KeyPress(description, keySequence);
                keyBinding.WhenTurnedOn = tpmPanelSwitchOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingTPM.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
            var tpmPanelSwitchOnOff = (TPMSwitchOnOff)panelSwitchOnOff;
            var bipLinkTPM = (BIPLinkTPM)bipLink;

            if (bipLinkTPM.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlListTPM.BIPS, tpmPanelSwitchOnOff);
                SetIsDirty();
                return;
            }

            // This must accept lists
            var found = false;

            foreach (var tmpBipLink in _bipLinks)
            {
                if (tmpBipLink.TPMSwitch == tpmPanelSwitchOnOff.Switch && tmpBipLink.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    tmpBipLink.BIPLights = bipLinkTPM.BIPLights;
                    tmpBipLink.Description = bipLinkTPM.Description;
                    found = true;
                    break;
                }
            }

            if (!found && bipLinkTPM.BIPLights.Count > 0)
            {
                bipLinkTPM.TPMSwitch = tpmPanelSwitchOnOff.Switch;
                bipLinkTPM.WhenTurnedOn = tpmPanelSwitchOnOff.ButtonState;
                _bipLinks.Add(bipLinkTPM);
            }

            SetIsDirty();
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
            var tpmPanelSwitchOnOff = (TPMSwitchOnOff)panelSwitchOnOff;
            if (dcsbiosInputs.Count == 0)
            {
                RemoveSwitchFromList(ControlListTPM.DCSBIOS, tpmPanelSwitchOnOff);
                SetIsDirty();
                return;
            }

            // !!!!!!!
            // If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            // This must accept lists
            var found = false;
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSActionBindingTPM();
                dcsBiosBinding.TPMSwitch = tpmPanelSwitchOnOff.Switch;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = tpmPanelSwitchOnOff.ButtonState;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }

            SetIsDirty();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
            var tpmPanelSwitchOnOff = (TPMSwitchOnOff)panelSwitchOnOff;
            var controlListTPM = (ControlListTPM) controlList;

            var found = false;
            if (controlListTPM == ControlListTPM.ALL || controlListTPM == ControlListTPM.KEYS)
            {
                foreach (var keyBindingTPM in _keyBindings)
                {
                    if (keyBindingTPM.TPMSwitch == tpmPanelSwitchOnOff.Switch && keyBindingTPM.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
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
                    if (dcsBiosBinding.TPMSwitch == tpmPanelSwitchOnOff.Switch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
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
                    if (bipLink.TPMSwitch == tpmPanelSwitchOnOff.Switch && bipLink.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                    {
                        bipLink.BIPLights.Clear();
                        found = true;
                    }
                }
            }

            if (controlListTPM == ControlListTPM.ALL || controlListTPM == ControlListTPM.OSCOMMAND)
            {
                OSCommandBindingTPM operatingSystemCommandBindingTPM = null;
                for (int i = 0; i < _operatingSystemCommandBindings.Count; i++)
                {
                    var operatingSystemCommand = _operatingSystemCommandBindings[i];

                    if (operatingSystemCommand.TPMSwitch == tpmPanelSwitchOnOff.Switch && operatingSystemCommand.WhenTurnedOn == tpmPanelSwitchOnOff.ButtonState)
                    {
                        operatingSystemCommandBindingTPM = _operatingSystemCommandBindings[i];
                        found = true;
                    }
                }

                if (operatingSystemCommandBindingTPM != null)
                {
                    _operatingSystemCommandBindings.Remove(operatingSystemCommandBindingTPM);
                }
            }

            if (found)
            {
                SetIsDirty();
            }
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

            SetIsDirty();
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            TPMSwitchChanged(isFirstReport, hashSet);
        }

        private void DeviceAttachedHandler()
        {
            Startup();
            DeviceAttached();
        }

        private void DeviceRemovedHandler()
        {
            Dispose();
            DeviceDetached();
        }

        private void CreateSwitchKeys()
        {
            SaitekPanelKnobs = TPMPanelSwitch.GetTPMPanelSwitches();
        }

        public HashSet<DCSBIOSActionBindingTPM> DCSBiosBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }
    }



    public enum ControlListTPM : byte
    {
        ALL,
        DCSBIOS,
        KEYS,
        BIPS,
        OSCOMMAND
    }
}
