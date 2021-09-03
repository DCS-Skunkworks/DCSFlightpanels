using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Saitek.Switches;

namespace NonVisuals.Saitek.Panels
{
    using NonVisuals.Plugin;

    public class FarmingSidePanel : SaitekPanel
    {
        private HashSet<DCSBIOSActionBindingFarmingPanel> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingFarmingPanel>();
        private HashSet<KeyBindingFarmingPanel> _keyBindings = new HashSet<KeyBindingFarmingPanel>();
        private List<OSCommandBindingFarmingPanel> _osCommandBindings = new List<OSCommandBindingFarmingPanel>();
        private HashSet<BIPLinkFarmingPanel> _bipLinks = new HashSet<BIPLinkFarmingPanel>();
        private readonly object _dcsBiosDataReceivedLock = new object();

        public FarmingSidePanel(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.FarmingPanel, hidSkeleton)
        {
            if (hidSkeleton.PanelInfo.GamingPanelType != GamingPanelEnum.FarmingPanel)
            {
                throw new ArgumentException();
            }
            //Fixed values
            VendorId = 0x0738;
            ProductId = 0x2218;
            CreateKeys();
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
                Common.LogError(ex);
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

                    if (setting.StartsWith("FarmingPanelKey{"))
                    {
                        var keyBinding = new KeyBindingFarmingPanel();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("FarmingPanelOS"))
                    {
                        var osCommand = new OSCommandBindingFarmingPanel();
                        osCommand.ImportSettings(setting);
                        _osCommandBindings.Add(osCommand);
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

            SettingsApplied();
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
            foreach (var osCommand in _osCommandBindings)
            {
                if (!osCommand.OSCommandObject.IsEmpty)
                {
                    result.Add(osCommand.ExportSettings());
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
            try
            {
                var thread = new Thread(ShowIdentifyingValue);
                thread.Start();
            }
            catch (Exception e)
            {
            }
        }

        private void ShowIdentifyingValue()
        {
            try
            {
                
            }
            catch (Exception e)
            {
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _keyBindings.Clear();
            _osCommandBindings.Clear();
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
            get => _osCommandBindings;
            set => _osCommandBindings = value;
        }

        private void FarmingSidePanelSwitchChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }

            foreach (var farmingPanelKeyObject in hashSet)
            {
                //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var farmingPanelKey = (FarmingPanelKey)farmingPanelKeyObject;
                var found = false;

                foreach (var keyBinding in _keyBindings)
                {
                    if (!isFirstReport && keyBinding.OSKeyPress != null && keyBinding.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && keyBinding.WhenTurnedOn == farmingPanelKey.IsOn)
                    {
                        keyBinding.OSKeyPress.Execute(new CancellationToken());
                        found = true;
                        break;
                    }
                }

                foreach (var osCommand in _osCommandBindings)
                {
                    if (!isFirstReport && osCommand.OSCommandObject != null && osCommand.FarmingPanelKey == farmingPanelKey.FarmingPanelMKKey && osCommand.WhenTurnedOn == farmingPanelKey.IsOn)
                    {
                        osCommand.OSCommandObject.Execute(new CancellationToken());
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

                if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    PluginManager.Get().PanelEventHandler.PanelEvent(ProfileHandler.SelectedProfile().Description, HIDInstanceId, (int)PluginGamingPanelEnum.FarmingPanel, (int)farmingPanelKey.FarmingPanelMKKey, farmingPanelKey.IsOn, 0);
                }
            }
        }


        public string GetKeyPressForLoggingPurposes(FarmingPanelKey farmingPanelKey)
        {
            var result = "";
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
                RemoveSwitchFromList(ControlListFarmingPanel.KEYS, farmingPanelSwitchOnOff);
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
                var keyBinding = new KeyBindingFarmingPanel();
                keyBinding.FarmingPanelKey = farmingPanelSwitchOnOff.Switch;
                keyBinding.OSKeyPress = new KeyPress(keyPress, keyPressLength);
                keyBinding.WhenTurnedOn = farmingPanelSwitchOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingFarmingPanel.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, KeyPressInfo> keySequence)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;
            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlListFarmingPanel.KEYS, farmingPanelOnOff);
                SetIsDirty();
                return;
            }
            //This must accept lists
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
                var keyBinding = new KeyBindingFarmingPanel();
                keyBinding.FarmingPanelKey = farmingPanelOnOff.Switch;
                var keyPress = new KeyPress(description, keySequence);
                keyBinding.OSKeyPress = keyPress;
                keyBinding.WhenTurnedOn = farmingPanelOnOff.ButtonState;
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingFarmingPanel.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand osCommand)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;
            //This must accept lists
            var found = false;

            foreach (var osCommandBinding in _osCommandBindings)
            {
                if (osCommandBinding.FarmingPanelKey == farmingPanelOnOff.Switch && osCommandBinding.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                {
                    osCommandBinding.OSCommandObject = osCommand;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var osCommandBindingFarmingPanel = new OSCommandBindingFarmingPanel();
                osCommandBindingFarmingPanel.FarmingPanelKey = farmingPanelOnOff.Switch;
                osCommandBindingFarmingPanel.OSCommandObject = osCommand;
                osCommandBindingFarmingPanel.WhenTurnedOn = farmingPanelOnOff.ButtonState;
                _osCommandBindings.Add(osCommandBindingFarmingPanel);
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
                RemoveSwitchFromList(ControlListFarmingPanel.DCSBIOS, farmingPanelOnOff);
                SetIsDirty();
                return;
            }
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
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
                var dcsBiosBinding = new DCSBIOSActionBindingFarmingPanel();
                dcsBiosBinding.FarmingPanelKey = farmingPanelOnOff.Switch;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = farmingPanelOnOff.ButtonState;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            SetIsDirty();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
            var farmingPanelOnOff = (FarmingPanelOnOff)panelSwitchOnOff;
            var bipLinkFarmingPanel = (BIPLinkFarmingPanel) bipLink;
            if (bipLinkFarmingPanel.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlListFarmingPanel.BIPS, farmingPanelOnOff);
                SetIsDirty();
                return;
            }
            //This must accept lists
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
            var controlListFarmingPanel = (ControlListFarmingPanel) controlList;
            var farmingPanelOnOff = (FarmingPanelOnOff) panelSwitchOnOff;

            var  found = false;
            if (controlListFarmingPanel == ControlListFarmingPanel.ALL || controlListFarmingPanel == ControlListFarmingPanel.KEYS)
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
            if (controlListFarmingPanel == ControlListFarmingPanel.ALL || controlListFarmingPanel == ControlListFarmingPanel.DCSBIOS)
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

            if (controlListFarmingPanel == ControlListFarmingPanel.ALL || controlListFarmingPanel == ControlListFarmingPanel.BIPS)
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
            
            if (controlListFarmingPanel == ControlListFarmingPanel.ALL || controlListFarmingPanel == ControlListFarmingPanel.OSCOMMANDS)
            {
                OSCommandBindingFarmingPanel osCommandBindingFarmingPanel  = null;
                for (int i = 0; i < _osCommandBindings.Count; i++)
                {
                    var osCommand = _osCommandBindings[i];

                    if (osCommand.FarmingPanelKey == farmingPanelOnOff.Switch && osCommand.WhenTurnedOn == farmingPanelOnOff.ButtonState)
                    {
                        osCommandBindingFarmingPanel = _osCommandBindings[i];
                        found = true;
                    }
                }

                if (osCommandBindingFarmingPanel != null)
                {
                    _osCommandBindings.Remove(osCommandBindingFarmingPanel);
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


        private void CreateKeys()
        {
            SaitekPanelKnobs = FarmingPanelKey.GetPanelFarmingPanelKeys();
        }

        public HashSet<DCSBIOSActionBindingFarmingPanel> DCSBiosBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

        public enum ControlListFarmingPanel : byte
        {
            ALL,
            DCSBIOS,
            KEYS,
            BIPS,
            OSCOMMANDS
        }
    }

    
}

