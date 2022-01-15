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

    public class FarmingSidePanel : SaitekPanel
    {
        private HashSet<DCSBIOSActionBindingFarmingPanel> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingFarmingPanel>();
        private HashSet<KeyBindingFarmingPanel> _keyBindings = new HashSet<KeyBindingFarmingPanel>();
        private List<OSCommandBindingFarmingPanel> _operatingSystemCommandBindings = new List<OSCommandBindingFarmingPanel>();
        private HashSet<BIPLinkFarmingPanel> _bipLinks = new HashSet<BIPLinkFarmingPanel>();
        private readonly object _dcsBiosDataReceivedLock = new object();

        public FarmingSidePanel(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.FarmingPanel, hidSkeleton)
        {
            if (hidSkeleton.GamingPanelType != GamingPanelEnum.FarmingPanel)
            {
                throw new ArgumentException();
            }

            // Fixed values
            VendorId = 0x0738;
            ProductId = 0x2218;
            CreateKeys();
            Startup();
            BIOSEventHandler.AttachDataListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    BIOSEventHandler.DetachDataListener(this);
                    Closed = true;
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
                StartListeningForHidPanelChanges();
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

                    if (setting.StartsWith("FarmingPanelKey{"))
                    {
                        var keyBinding = new KeyBindingFarmingPanel();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("FarmingPanelOS"))
                    {
                        var operatingSystemCommand = new OSCommandBindingFarmingPanel();
                        operatingSystemCommand.ImportSettings(setting);
                        _operatingSystemCommandBindings.Add(operatingSystemCommand);
                    }
                    else if (setting.StartsWith("FarmingPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBinding = new DCSBIOSActionBindingFarmingPanel();
                        dcsBIOSBinding.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBinding);
                    }
                    else if (setting.StartsWith("FarmingPanelBIPLink{"))
                    {
                        var bipLinkFarmingPanel = new BIPLinkFarmingPanel();
                        bipLinkFarmingPanel.ImportSettings(setting);
                        _bipLinks.Add(bipLinkFarmingPanel);
                    }
                }
            }

            AppEventHandler.SettingsApplied(this, HIDSkeletonBase.HIDInstance, TypeOfPanel);
            _keyBindings = KeyBindingFarmingPanel.SetNegators(_keyBindings);

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
            e.ProfileHandlerCaller.RegisterPanelBinding(this, ExportSettings());
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
            try
            {
                var thread = new Thread(ShowIdentifyingValue);
                thread.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ShowIdentifyingValue()
        {
            // ignored
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

        public HashSet<KeyBindingFarmingPanel> KeyBindingsHashSet
        {
            get => _keyBindings;
            set => _keyBindings = value;
        }

        public HashSet<BIPLinkFarmingPanel> BIPLinkHashSet
        {
            get => _bipLinks;
            set => _bipLinks = value;
        }

        public List<OSCommandBindingFarmingPanel> OSCommandList
        {
            get => _operatingSystemCommandBindings;
            set => _operatingSystemCommandBindings = value;
        }

        private void FarmingSidePanelSwitchChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }

            foreach (var farmingPanelKeyObject in hashSet)
            {
                // Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var farmingPanelKey = (FarmingPanelKey)farmingPanelKeyObject;
                var found = false;

                var keyBindingFound = false;
                foreach (var keyBinding in _keyBindings)
                {
                    if (!isFirstReport && keyBinding.OSKeyPress != null && keyBinding.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && keyBinding.WhenTurnedOn == farmingPanelKey.IsOn)
                    {
                        keyBindingFound = true;
                        if (!PluginManager.DisableKeyboardAPI)
                        {
                            keyBinding.OSKeyPress.Execute(new CancellationToken());
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                DCSFPProfile.SelectedProfile.Description,
                                HIDInstance,
                                (int)PluginGamingPanelEnum.FarmingPanel,
                                (int)farmingPanelKey.FarmingPanelMKKey,
                                farmingPanelKey.IsOn,
                                keyBinding.OSKeyPress.KeyPressSequence);
                        }

                        found = true;
                        break;
                    }
                }

                if (!isFirstReport && !keyBindingFound && PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    PluginManager.DoEvent(
                        DCSFPProfile.SelectedProfile.Description,
                        HIDInstance,
                        (int)PluginGamingPanelEnum.FarmingPanel,
                        (int)farmingPanelKey.FarmingPanelMKKey,
                        farmingPanelKey.IsOn,
                        null);
                }

                foreach (var operatingSystemCommand in _operatingSystemCommandBindings)
                {
                    if (!isFirstReport && operatingSystemCommand.OSCommandObject != null && operatingSystemCommand.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && operatingSystemCommand.WhenTurnedOn == farmingPanelKey.IsOn)
                    {
                        operatingSystemCommand.OSCommandObject.Execute(new CancellationToken());
                        found = true;
                        break;
                    }
                }

                foreach (var bipLink in _bipLinks)
                {
                    if (!isFirstReport && bipLink.BIPLights.Count > 0 && bipLink.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && bipLink.WhenTurnedOn == farmingPanelKey.IsOn)
                    {
                        bipLink.Execute();
                        break;
                    }
                }

                if (!isFirstReport && !found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && dcsBiosBinding.WhenTurnedOn == farmingPanelKey.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands(new CancellationToken());
                            break;
                        }
                    }
                }
            }
        }


        public string GetKeyPressForLoggingPurposes(FarmingPanelKey farmingPanelKey)
        {
            var result = string.Empty;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && keyBinding.WhenTurnedOn == farmingPanelKey.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }

            return result;
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
            var farmingPanelSwitchOnOff = (FarmingPanelOnOff) panelSwitchOnOff;

            if (string.IsNullOrEmpty(keyPress))
            {
                RemoveSwitchFromList(ControlList.KEYS, farmingPanelSwitchOnOff);
                SetIsDirty();
                return;
            }

            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.FarmingPanelKey == farmingPanelSwitchOnOff.Switch && keyBinding.WhenTurnedOn == farmingPanelSwitchOnOff.ButtonState)
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
                var keyBinding = new KeyBindingFarmingPanel
                {
                    FarmingPanelKey = farmingPanelSwitchOnOff.Switch,
                    OSKeyPress = new KeyPress(keyPress, keyPressLength),
                    WhenTurnedOn = farmingPanelSwitchOnOff.ButtonState
                };
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingFarmingPanel.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;
            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlList.KEYS, farmingPanelOnOff);
                SetIsDirty();
                return;
            }

            // This must accept lists
            var found = false;

            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.FarmingPanelKey == farmingPanelOnOff.Switch && keyBinding.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                {
                    if (keySequence.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        var keyPress = new KeyPress(description, keySequence);
                        keyBinding.OSKeyPress = keyPress;
                    }

                    found = true;
                    break;
                }
            }

            if (!found && keySequence.Count > 0)
            {
                var keyBinding = new KeyBindingFarmingPanel
                {
                    FarmingPanelKey = farmingPanelOnOff.Switch
                };
                var keyPress = new KeyPress(description, keySequence);
                keyBinding.OSKeyPress = keyPress;
                keyBinding.WhenTurnedOn = farmingPanelOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingFarmingPanel.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;

            // This must accept lists
            var found = false;

            foreach (var operatingSystemCommandBinding in _operatingSystemCommandBindings)
            {
                if (operatingSystemCommandBinding.FarmingPanelKey == farmingPanelOnOff.Switch && operatingSystemCommandBinding.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                {
                    operatingSystemCommandBinding.OSCommandObject = operatingSystemCommand;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var operatingSystemCommandBindingFarmingPanel = new OSCommandBindingFarmingPanel
                {
                    FarmingPanelKey = farmingPanelOnOff.Switch,
                    OSCommandObject = operatingSystemCommand,
                    WhenTurnedOn = farmingPanelOnOff.ButtonState
                };
                _operatingSystemCommandBindings.Add(operatingSystemCommandBindingFarmingPanel);
            }

            SetIsDirty();
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;
            if (dcsbiosInputs.Count == 0)
            {
                RemoveSwitchFromList(ControlList.DCSBIOS, farmingPanelOnOff);
                SetIsDirty();
                return;
            }

            // !!!!!!!
            // If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            // This must accept lists
            var found = false;
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.FarmingPanelKey == farmingPanelOnOff.Switch && dcsBiosBinding.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSActionBindingFarmingPanel
                {
                    FarmingPanelKey = farmingPanelOnOff.Switch,
                    DCSBIOSInputs = dcsbiosInputs,
                    WhenTurnedOn = farmingPanelOnOff.ButtonState,
                    Description = description
                };
                _dcsBiosBindings.Add(dcsBiosBinding);
            }

            SetIsDirty();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;
            var bipLinkFarmingPanel = (BIPLinkFarmingPanel)bipLink;
            if (bipLinkFarmingPanel.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlList.BIPS, farmingPanelOnOff);
                SetIsDirty();
                return;
            }

            // This must accept lists
            var found = false;

            foreach (var tmpBipLink in _bipLinks)
            {
                if (tmpBipLink.FarmingPanelKey == farmingPanelOnOff.Switch && tmpBipLink.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                {
                    tmpBipLink.BIPLights = bipLinkFarmingPanel.BIPLights;
                    tmpBipLink.Description = bipLinkFarmingPanel.Description;
                    found = true;
                    break;
                }
            }

            if (!found && bipLinkFarmingPanel.BIPLights.Count > 0)
            {
                bipLinkFarmingPanel.FarmingPanelKey = farmingPanelOnOff.Switch;
                bipLinkFarmingPanel.WhenTurnedOn = farmingPanelOnOff.ButtonState;
                _bipLinks.Add(bipLinkFarmingPanel);
            }

            SetIsDirty();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
            var controlListFarmingPanel = (ControlList) controlList;
            var farmingPanelOnOff = (FarmingPanelOnOff) panelSwitchOnOff;

            var  found = false;
            if (controlListFarmingPanel == ControlList.ALL || controlListFarmingPanel == ControlList.KEYS)
            {
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.FarmingPanelKey == farmingPanelOnOff.Switch && keyBinding.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                    {
                        keyBinding.OSKeyPress = null;
                        found = true;
                    }
                }
            }

            if (controlListFarmingPanel == ControlList.ALL || controlListFarmingPanel == ControlList.DCSBIOS)
            {
                foreach (var dcsBiosBinding in _dcsBiosBindings)
                {
                    if (dcsBiosBinding.FarmingPanelKey == farmingPanelOnOff.Switch && dcsBiosBinding.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                    {
                        dcsBiosBinding.DCSBIOSInputs.Clear();
                        found = true;
                    }
                }
            }

            if (controlListFarmingPanel == ControlList.ALL || controlListFarmingPanel == ControlList.BIPS)
            {
                foreach (var bipLink in _bipLinks)
                {
                    if (bipLink.FarmingPanelKey == farmingPanelOnOff.Switch && bipLink.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                    {
                        bipLink.BIPLights.Clear();
                        found = true;
                    }
                }
            }
            
            if (controlListFarmingPanel == ControlList.ALL || controlListFarmingPanel == ControlList.OSCOMMANDS)
            {
                OSCommandBindingFarmingPanel operatingSystemCommandBindingFarmingPanel  = null;
                for (int i = 0; i < _operatingSystemCommandBindings.Count; i++)
                {
                    var operatingSystemCommand = _operatingSystemCommandBindings[i];

                    if (operatingSystemCommand.FarmingPanelKey == farmingPanelOnOff.Switch && operatingSystemCommand.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                    {
                        operatingSystemCommandBindingFarmingPanel = _operatingSystemCommandBindings[i];
                        found = true;
                    }
                }

                if (operatingSystemCommandBindingFarmingPanel != null)
                {
                    _operatingSystemCommandBindings.Remove(operatingSystemCommandBindingFarmingPanel);
                }
            }

            if (found)
            {
                SetIsDirty();
            }
        }


        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            FarmingSidePanelSwitchChanged(isFirstReport, hashSet);
        }
        
        private void CreateKeys()
        {
            SaitekPanelKnobs = FarmingPanelKey.GetPanelFarmingPanelKeys();
        }

        public HashSet<DCSBIOSActionBindingFarmingPanel> DCSBiosBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

    }

    
}

