namespace NonVisuals.Saitek.Panels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.Plugin;
    using NonVisuals.Saitek.Switches;

    public class MultiPanelPZ70 : SaitekPanel
    {
        private readonly object _lcdLockObject = new object();
        private readonly object _lcdDataVariablesLockObject = new object();
        private readonly PZ70LCDButtonByteList _lcdButtonByteListHandler = new PZ70LCDButtonByteList();
        private readonly ClickSpeedDetector _altLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _vsLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _iasLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _hdgLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _crsLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);

        private int _lcdKnobSensitivity;
        private volatile byte _knobSensitivitySkipper;
        private HashSet<DCSBIOSActionBindingPZ70> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingPZ70>();
        private HashSet<DCSBIOSOutputBindingPZ70> _dcsBiosLcdBindings = new HashSet<DCSBIOSOutputBindingPZ70>();
        private HashSet<KeyBindingPZ70> _knobBindings = new HashSet<KeyBindingPZ70>();
        private List<OSCommandBindingPZ70> _operatingSystemCommandBindings = new List<OSCommandBindingPZ70>();
        private HashSet<BIPLinkPZ70> _bipLinks = new HashSet<BIPLinkPZ70>();
        private PZ70DialPosition _pz70DialPosition = PZ70DialPosition.ALT;
        
        // 0 - 40000
        private int _altLCDKeyEmulatorValue;

        // -6000 - 6000
        private int _vsLCDKeyEmulatorValue;

        // 0-600
        private int _iasLCDKeyEmulatorValue;

        // 0-360
        private int _hdgLCDKeyEmulatorValue;

        // 0-360
        private int _crsLCDKeyEmulatorValue;
        
        private long _doUpdatePanelLCD;

        public MultiPanelPZ70(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.PZ70MultiPanel, hidSkeleton)
        {
            if (hidSkeleton.PanelInfo.GamingPanelType != GamingPanelEnum.PZ70MultiPanel)
            {
                throw new ArgumentException();
            }

            VendorId = 0x6A3;
            ProductId = 0xD06;
            CreateMultiKnobs();
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

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (SettingsLoading)
            {
                return;
            }

            UpdateCounter(e.Address, e.Data);
            foreach (var dcsbiosBindingLCDPZ70 in _dcsBiosLcdBindings)
            {
                if (!dcsbiosBindingLCDPZ70.UseFormula && dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && e.Address == dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.Address)
                {
                    lock (_lcdDataVariablesLockObject)
                    {
                        var tmp = dcsbiosBindingLCDPZ70.CurrentValue;
                        dcsbiosBindingLCDPZ70.CurrentValue = (int)dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.GetUIntValue(e.Data);
                        if (tmp != dcsbiosBindingLCDPZ70.CurrentValue)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }
                else if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && dcsbiosBindingLCDPZ70.UseFormula)
                {
                    if (dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.CheckForMatch(e.Address, e.Data))
                    {
                        lock (_lcdDataVariablesLockObject)
                        {
                            var tmp = dcsbiosBindingLCDPZ70.CurrentValue;
                            dcsbiosBindingLCDPZ70.CurrentValue = dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.Evaluate();
                            if (tmp != dcsbiosBindingLCDPZ70.CurrentValue)
                            {
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            }
                        }
                    }
                }
            }

            UpdateLCD();
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            var settings = genericPanelBinding.Settings;
            SettingsLoading = true;
            
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2)
                {

                    if (setting.StartsWith("MultiPanelKnob{"))
                    {
                        var knobBinding = new KeyBindingPZ70();
                        knobBinding.ImportSettings(setting);
                        _knobBindings.Add(knobBinding);
                    }
                    else if (setting.StartsWith("MultiPanelOSPZ70"))
                    {
                        var operatingSystemCommand = new OSCommandBindingPZ70();
                        operatingSystemCommand.ImportSettings(setting);
                        _operatingSystemCommandBindings.Add(operatingSystemCommand);
                    }
                    else if (setting.StartsWith("MultiPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingPZ70 = new DCSBIOSActionBindingPZ70();
                        dcsBIOSBindingPZ70.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingPZ70);
                    }
                    else if (setting.StartsWith("MultipanelBIPLink{"))
                    {
                        var bipLinkPZ70 = new BIPLinkPZ70();
                        bipLinkPZ70.ImportSettings(setting);
                        _bipLinks.Add(bipLinkPZ70);
                    }
                    else if (setting.StartsWith("MultiPanelDCSBIOSControlLCD{"))
                    {
                        var dcsBIOSBindingLCDPZ70 = new DCSBIOSOutputBindingPZ70();
                        dcsBIOSBindingLCDPZ70.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsBIOSBindingLCDPZ70);
                    }
                }
            }

            SettingsLoading = false;
            _knobBindings = KeyBindingPZ70.SetNegators(_knobBindings);
            SettingsApplied();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }

            var result = new List<string>();

            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.OSKeyPress != null)
                {
                    result.Add(knobBinding.ExportSettings());
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

            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.HasBinding)
                {
                    result.Add(dcsBiosBindingLCD.ExportSettings());
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

        public string GetKeyPressForLoggingPurposes(MultiPanelKnob multiPanelKnob)
        {
            var result = string.Empty;
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.OSKeyPress != null && knobBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                {
                    result = knobBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }

            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterPanelBinding(this, ExportSettings());
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e) { }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _knobBindings.Clear();
            _operatingSystemCommandBindings.Clear();
            _dcsBiosBindings.Clear();
            _dcsBiosLcdBindings.Clear();
            _bipLinks.Clear();

            if (setIsDirty)
            {
                SetIsDirty();
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
            }
        }

        private void ShowIdentifyingValue()
        {
            try
            {
                var spins = 40;
                var random = new Random();
                var buttonList = new List<MultiPanelPZ70Knobs>();
                buttonList.Add(MultiPanelPZ70Knobs.AP_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.HDG_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.NAV_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.IAS_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.ALT_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.VS_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.APR_BUTTON);
                buttonList.Add(MultiPanelPZ70Knobs.REV_BUTTON);

                while (spins > 0)
                {
                    var randomButton = (MultiPanelPZ70Knobs)buttonList.ToArray().GetValue(random.Next(buttonList.ToArray().Length));
                    var onOrOff = random.Next(0, 1);
                    _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, randomButton);

                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    UpdateLCD();

                    Thread.Sleep(50);
                    spins--;
                }

                foreach (var multiPanelPZ70Knob in buttonList)
                {
                    _lcdButtonByteListHandler.SetButtonOff(PZ70DialPosition, multiPanelPZ70Knob);
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            // Set _selectedMode and LCD button statuses
            // and performs the actual actions for key presses
            PZ70SwitchChanged(isFirstReport, hashSet);
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
            var pz70SwitchOnOff = (PZ70SwitchOnOff) panelSwitchOnOff;
            if (string.IsNullOrEmpty(keyPress))
            {
                RemoveSwitchFromList(ControlListPZ70.KEYS, pz70SwitchOnOff);
                SetIsDirty();
                return;
            }

            // This must accept lists
            var found = false;
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && knobBinding.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                {
                    if (string.IsNullOrEmpty(keyPress))
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new KeyPress(keyPress, keyPressLength);
                    }

                    found = true;
                }
            }

            if (!found && !string.IsNullOrEmpty(keyPress))
            {
                var knobBinding = new KeyBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = pz70SwitchOnOff.Switch;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new KeyPress(keyPress, keyPressLength);
                knobBinding.WhenTurnedOn = pz70SwitchOnOff.ButtonState;
                _knobBindings.Add(knobBinding);
            }

            _knobBindings = KeyBindingPZ70.SetNegators(_knobBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
            var pz70SwitchOnOff = (PZ70SwitchOnOff)panelSwitchOnOff;

            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlListPZ70.KEYS, pz70SwitchOnOff);
                SetIsDirty();
                return;
            }

            var found = false;
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && knobBinding.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                {
                    if (keySequence.Count == 0)
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new KeyPress(description, keySequence);
                    }

                    found = true;
                    break;
                }
            }

            if (!found && keySequence.Count > 0)
            {
                var knobBinding = new KeyBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = pz70SwitchOnOff.Switch;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new KeyPress(description, keySequence);
                knobBinding.WhenTurnedOn = pz70SwitchOnOff.ButtonState;
                _knobBindings.Add(knobBinding);
            }

            _knobBindings = KeyBindingPZ70.SetNegators(_knobBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
            var pz70SwitchOnOff = (PZ70SwitchOnOff)panelSwitchOnOff;

            if (dcsbiosInputs.Count == 0)
            {
                RemoveSwitchFromList(ControlListPZ70.DCSBIOS, pz70SwitchOnOff);
                SetIsDirty();
                return;
            }

            var found = false;
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && dcsBiosBinding.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSActionBindingPZ70();
                dcsBiosBinding.MultiPanelPZ70Knob = pz70SwitchOnOff.Switch;
                dcsBiosBinding.DialPosition = _pz70DialPosition;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = pz70SwitchOnOff.ButtonState;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }

            SetIsDirty();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutput dcsbiosOutput, PZ70LCDPosition pz70LCDPosition)
        {
            var found = false;
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSOutputBindingPZ70();
                dcsBiosBindingLCD.DialPosition = _pz70DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }

            SetIsDirty();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutputFormula dcsbiosOutputFormula, PZ70LCDPosition pz70LCDPosition)
        {
            var found = false;
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSOutputBindingPZ70();
                dcsBiosBindingLCD.DialPosition = _pz70DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }

            SetIsDirty();
        }

        public void AddOrUpdateDCSBIOSLcdBinding(PZ70LCDPosition pz70LCDPosition)
        {
            // Removes config
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputObject = null;
                    break;
                }
            }

            SetIsDirty();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
            var pz70SwitchOnOff = (PZ70SwitchOnOff)panelSwitchOnOff;
            var bipLinkPZ70 = (BIPLinkPZ70) bipLink;

            if (bipLinkPZ70.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlListPZ70.BIPS, pz70SwitchOnOff);
                SetIsDirty();
                return;
            }

            var found = false;

            foreach (var tmpBipLink in _bipLinks)
            {
                if (tmpBipLink.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && tmpBipLink.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                {
                    tmpBipLink.BIPLights = bipLinkPZ70.BIPLights;
                    tmpBipLink.Description = bipLinkPZ70.Description;
                    tmpBipLink.MultiPanelPZ70Knob = pz70SwitchOnOff.Switch;
                    found = true;
                    break;
                }
            }

            if (!found && bipLinkPZ70.BIPLights.Count > 0)
            {
                bipLinkPZ70.MultiPanelPZ70Knob = pz70SwitchOnOff.Switch;
                bipLinkPZ70.WhenTurnedOn = pz70SwitchOnOff.ButtonState;
                _bipLinks.Add(bipLinkPZ70);
            }

            SetIsDirty();
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
            var pz70SwitchOnOff = (PZ70SwitchOnOff)panelSwitchOnOff;

            var found = false;

            foreach (var operatingSystemCommandBinding in _operatingSystemCommandBindings)
            {
                if (operatingSystemCommandBinding.DialPosition == _pz70DialPosition && operatingSystemCommandBinding.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && operatingSystemCommandBinding.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                {
                    operatingSystemCommandBinding.OSCommandObject = operatingSystemCommand;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var operatingSystemCommandBindingPZ70 = new OSCommandBindingPZ70();
                operatingSystemCommandBindingPZ70.MultiPanelPZ70Knob = pz70SwitchOnOff.Switch;
                operatingSystemCommandBindingPZ70.OSCommandObject = operatingSystemCommand;
                operatingSystemCommandBindingPZ70.WhenTurnedOn = pz70SwitchOnOff.ButtonState;
                _operatingSystemCommandBindings.Add(operatingSystemCommandBindingPZ70);
            }

            SetIsDirty();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
            var pz70SwitchOnOff = (PZ70SwitchOnOff)panelSwitchOnOff;
            var controlListPZ70 = (ControlListPZ70) controlList;

            var found = false;
            if (controlListPZ70 == ControlListPZ70.ALL || controlListPZ70 == ControlListPZ70.KEYS)
            {
                foreach (var knobBindingPZ70 in _knobBindings)
                {
                    if (knobBindingPZ70.DialPosition == _pz70DialPosition && knobBindingPZ70.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && knobBindingPZ70.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                    {
                        knobBindingPZ70.OSKeyPress = null;
                        found = true;
                    }
                }
            }

            if (controlListPZ70 == ControlListPZ70.ALL || controlListPZ70 == ControlListPZ70.DCSBIOS)
            {
                foreach (var dcsBiosBinding in _dcsBiosBindings)
                {
                    if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && dcsBiosBinding.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                    {
                        dcsBiosBinding.DCSBIOSInputs.Clear();
                        found = true;
                    }
                }
            }

            if (controlListPZ70 == ControlListPZ70.ALL || controlListPZ70 == ControlListPZ70.BIPS)
            {
                foreach (var bipLink in _bipLinks)
                {
                    if (bipLink.DialPosition == _pz70DialPosition && bipLink.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && bipLink.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                    {
                        bipLink.BIPLights.Clear();
                        found = true;
                    }
                }
            }

            if (controlListPZ70 == ControlListPZ70.ALL || controlListPZ70 == ControlListPZ70.OSCOMMAND)
            {
                OSCommandBindingPZ70 operatingSystemCommandBindingPZ70 = null;
                for (int i = 0; i < _operatingSystemCommandBindings.Count; i++)
                {
                    var operatingSystemCommand = _operatingSystemCommandBindings[i];

                    if (operatingSystemCommand.MultiPanelPZ70Knob == pz70SwitchOnOff.Switch && operatingSystemCommand.WhenTurnedOn == pz70SwitchOnOff.ButtonState)
                    {
                        operatingSystemCommandBindingPZ70 = _operatingSystemCommandBindings[i];
                        found = true;
                    }
                }

                if (operatingSystemCommandBindingPZ70 != null)
                {
                    _operatingSystemCommandBindings.Remove(operatingSystemCommandBindingPZ70);
                }
            }

            if (found)
            {
                SetIsDirty();
            }
        }

        private void PZ70SwitchChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            foreach (var o in hashSet)
            {
                var multiPanelKnob = (MultiPanelKnob)o;
                if (multiPanelKnob.IsOn)
                {
                    switch (multiPanelKnob.MultiPanelPZ70Knob)
                    {
                        case MultiPanelPZ70Knobs.KNOB_ALT:
                            {
                                _pz70DialPosition = PZ70DialPosition.ALT;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                _pz70DialPosition = PZ70DialPosition.VS;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                _pz70DialPosition = PZ70DialPosition.IAS;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                _pz70DialPosition = PZ70DialPosition.HDG;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                _pz70DialPosition = PZ70DialPosition.CRS;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }

                        case MultiPanelPZ70Knobs.AP_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.AP_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.HDG_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.HDG_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.NAV_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.NAV_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.IAS_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.IAS_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.ALT_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.ALT_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.VS_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.VS_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.APR_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.APR_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }

                        case MultiPanelPZ70Knobs.REV_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70DialPosition, MultiPanelPZ70Knobs.REV_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                    }
                }
            }

            UpdateLCD();
            if (!ForwardPanelEvent)
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var multiPanelKnob = (MultiPanelKnob)o;

                /*
                                 * IMPORTANT
                                 * ---------
                                 * The LCD buttons toggle between on and off. It is the toggle value that defines if the button is OFF, not the fact that the user releases the button.
                                 * Therefore the fore-mentioned buttons cannot be used as usual in a loop with knobBinding.WhenTurnedOn
                                 * Instead the buttons global bool value must be used!
                                 * 
                                 */
                if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly)
                    && (multiPanelKnob.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC || multiPanelKnob.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC))
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    LCDDialChangesHandle(multiPanelKnob);
                    UpdateLCD();
                }

                var found = false;
                var keyBindingFound = false;
                foreach (var knobBinding in _knobBindings)
                {
                    if (!isFirstReport && knobBinding.DialPosition == _pz70DialPosition && knobBinding.OSKeyPress != null && knobBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob
                        && knobBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        if (knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC || knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                        {
                            keyBindingFound = true;
                            if (!PluginManager.DisableKeyboardAPI && !SkipCurrentLcdKnobChange())
                            {
                                knobBinding.OSKeyPress.Execute(new CancellationToken());
                            }

                            if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                            {
                                PluginManager.DoEvent(
                                    ProfileHandler.SelectedProfile().Description,
                                    HIDInstanceId,
                                    (int)PluginGamingPanelEnum.PZ70MultiPanel,
                                    (int)multiPanelKnob.MultiPanelPZ70Knob,
                                    multiPanelKnob.IsOn,
                                    knobBinding.OSKeyPress.KeyPressSequence);
                            }

                            found = true;
                        }
                        else
                        {
                            knobBinding.OSKeyPress.Execute(new CancellationToken());
                            found = true;
                        }

                        break;
                    }
                }

                if (!isFirstReport && !keyBindingFound && PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    PluginManager.DoEvent(
                        ProfileHandler.SelectedProfile().Description,
                        HIDInstanceId,
                        (int)PluginGamingPanelEnum.PZ70MultiPanel,
                        (int)multiPanelKnob.MultiPanelPZ70Knob,
                        multiPanelKnob.IsOn,
                        null);
                }

                if (!isFirstReport && !found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob
                            && dcsBiosBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands(new CancellationToken());
                            break;
                        }
                    }
                }

                foreach (var operatingSystemCommand in _operatingSystemCommandBindings)
                {
                    if (!isFirstReport && operatingSystemCommand.DialPosition == _pz70DialPosition && operatingSystemCommand.OSCommandObject != null
                        && operatingSystemCommand.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && operatingSystemCommand.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        operatingSystemCommand.OSCommandObject.Execute(new CancellationToken());
                        found = true;
                        break;
                    }
                }

                foreach (var bipLinkPZ70 in _bipLinks)
                {
                    if (!isFirstReport && bipLinkPZ70.BIPLights.Count > 0 && bipLinkPZ70.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && bipLinkPZ70.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        bipLinkPZ70.Execute();
                        break;
                    }
                }
            }
        }

        private void LCDDialChangesHandle(MultiPanelKnob multiPanelKnob)
        {
            if (SkipCurrentLcdKnobChangeLCD())
            {
                return;
            }

            var increase = multiPanelKnob.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC;
            switch (_pz70DialPosition)
            {
                case PZ70DialPosition.ALT:
                    {
                        _altLCDKeyEmulatorValueChangeMonitor.Click();
                        if (_altLCDKeyEmulatorValueChangeMonitor.ClickThresholdReached())
                        {
                            if (increase)
                            {
                                ChangeAltLCDValue(500);
                            }
                            else
                            {
                                ChangeAltLCDValue(-500);
                            }
                        }
                        else
                        {
                            if (increase)
                            {
                                ChangeAltLCDValue(100);
                            }
                            else
                            {
                                ChangeAltLCDValue(-100);
                            }
                        }

                        break;
                    }

                case PZ70DialPosition.VS:
                    {
                        _vsLCDKeyEmulatorValueChangeMonitor.Click();
                        if (_vsLCDKeyEmulatorValueChangeMonitor.ClickThresholdReached())
                        {
                            if (increase)
                            {
                                ChangeVsLCDValue(100);
                            }
                            else
                            {
                                ChangeVsLCDValue(-100);
                            }
                        }
                        else
                        {
                            if (increase)
                            {
                                ChangeVsLCDValue(10);
                            }
                            else
                            {
                                ChangeVsLCDValue(-10);
                            }
                        }

                        break;
                    }

                case PZ70DialPosition.IAS:
                    {
                        _iasLCDKeyEmulatorValueChangeMonitor.Click();
                        if (_iasLCDKeyEmulatorValueChangeMonitor.ClickThresholdReached())
                        {
                            if (increase)
                            {
                                ChangeIasLCDValue(50);
                            }
                            else
                            {
                                ChangeIasLCDValue(-50);
                            }
                        }
                        else
                        {
                            if (increase)
                            {
                                ChangeIasLCDValue(5);
                            }
                            else
                            {
                                ChangeIasLCDValue(-5);
                            }
                        }

                        break;
                    }

                case PZ70DialPosition.HDG:
                    {
                        _hdgLCDKeyEmulatorValueChangeMonitor.Click();
                        if (_hdgLCDKeyEmulatorValueChangeMonitor.ClickThresholdReached())
                        {
                            if (increase)
                            {
                                ChangeHdgLCDValue(5);
                            }
                            else
                            {
                                ChangeHdgLCDValue(-5);
                            }
                        }
                        else
                        {
                            if (increase)
                            {
                                ChangeHdgLCDValue(1);
                            }
                            else
                            {
                                ChangeHdgLCDValue(-1);
                            }
                        }

                        break;
                    }

                case PZ70DialPosition.CRS:
                    {
                        _crsLCDKeyEmulatorValueChangeMonitor.Click();
                        if (_crsLCDKeyEmulatorValueChangeMonitor.ClickThresholdReached())
                        {
                            if (increase)
                            {
                                ChangeCrsLCDValue(5);
                            }
                            else
                            {
                                ChangeCrsLCDValue(-5);
                            }
                        }
                        else
                        {
                            if (increase)
                            {
                                ChangeCrsLCDValue(1);
                            }
                            else
                            {
                                ChangeCrsLCDValue(-1);
                            }
                        }

                        break;
                    }
            }
        }

        private void ChangeAltLCDValue(int value)
        {
            if (_altLCDKeyEmulatorValue + value > 40000)
            {
                _altLCDKeyEmulatorValue = 40000;
            }
            else if (_altLCDKeyEmulatorValue + value < 0)
            {
                _altLCDKeyEmulatorValue = 0;
            }
            else
            {
                _altLCDKeyEmulatorValue = _altLCDKeyEmulatorValue + value;
            }
        }

        private void ChangeVsLCDValue(int value)
        {
            if (_vsLCDKeyEmulatorValue + value > 6000)
            {
                _vsLCDKeyEmulatorValue = 6000;
            }
            else if (_vsLCDKeyEmulatorValue + value < -6000)
            {
                _vsLCDKeyEmulatorValue = -6000;
            }
            else
            {
                _vsLCDKeyEmulatorValue = _vsLCDKeyEmulatorValue + value;
            }
        }

        private void ChangeIasLCDValue(int value)
        {
            if (_iasLCDKeyEmulatorValue + value > 600)
            {
                _iasLCDKeyEmulatorValue = 600;
            }
            else if (_iasLCDKeyEmulatorValue + value < 0)
            {
                _iasLCDKeyEmulatorValue = 0;
            }
            else
            {
                _iasLCDKeyEmulatorValue = _iasLCDKeyEmulatorValue + value;
            }
        }

        private void ChangeHdgLCDValue(int value)
        {
            if (_hdgLCDKeyEmulatorValue + value > 360)
            {
                _hdgLCDKeyEmulatorValue = 0;
            }
            else if (_hdgLCDKeyEmulatorValue + value < 0)
            {
                _hdgLCDKeyEmulatorValue = 360;
            }
            else
            {
                _hdgLCDKeyEmulatorValue = _hdgLCDKeyEmulatorValue + value;
            }
        }

        private void ChangeCrsLCDValue(int value)
        {
            if (_crsLCDKeyEmulatorValue + value > 360)
            {
                _crsLCDKeyEmulatorValue = 0;
            }
            else if (_crsLCDKeyEmulatorValue + value < 0)
            {
                _crsLCDKeyEmulatorValue = 360;
            }
            else
            {
                _crsLCDKeyEmulatorValue = _crsLCDKeyEmulatorValue + value;
            }
        }

        protected bool SkipCurrentLcdKnobChange(bool change = true)
        {
            switch (_lcdKnobSensitivity)
            {
                case 0:
                    {
                        // Do nothing all manipulation is let through
                        break;
                    }

                case -1:
                    {
                        // Skip every 2 manipulations
                        if (change)
                        {
                            _knobSensitivitySkipper++;
                        }

                        if (_knobSensitivitySkipper <= 2)
                        {
                            return true;
                        }

                        _knobSensitivitySkipper = 0;
                        break;
                    }

                case -2:
                    {
                        // Skip every 4 manipulations
                        if (change)
                        {
                            _knobSensitivitySkipper++;
                        }

                        if (_knobSensitivitySkipper <= 4)
                        {
                            return true;
                        }

                        _knobSensitivitySkipper = 0;
                        break;
                    }
            }
            return false;
        }

        protected bool SkipCurrentLcdKnobChangeLCD(bool change = true)
        {
            // Skip every 3 manipulations
            if (change)
            {
                _knobSensitivitySkipper++;
            }

            if (_knobSensitivitySkipper <= 3)
            {
                return true;
            }

            _knobSensitivitySkipper = 0;
            return false;
        }

        private void UpdateLCD()
        {
            // 345
            // 15600
            // [0x0]
            // [1] [2] [3] [4] [5]
            // [6] [7] [8] [9] [10]
            // [11 BUTTONS]

            if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
            {
                return;
            }

            var bytes = new byte[12];
            bytes[0] = 0x0;
            for (var ii = 1; ii < bytes.Length - 1; ii++)
            {
                bytes[ii] = 0xFF;
            }

            bytes[11] = _lcdButtonByteListHandler.GetButtonByte(PZ70DialPosition);

            var foundUpperValue = false;
            var foundLowerValue = false;

            var upperValue = 0;
            var lowerValue = 0;
            lock (_lcdDataVariablesLockObject)
            {
                if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    switch (_pz70DialPosition)
                    {
                        case PZ70DialPosition.ALT:
                            {
                                upperValue = _altLCDKeyEmulatorValue;
                                lowerValue = _vsLCDKeyEmulatorValue;
                                foundUpperValue = true;
                                foundLowerValue = true;
                                break;
                            }

                        case PZ70DialPosition.VS:
                            {
                                upperValue = _altLCDKeyEmulatorValue;
                                lowerValue = _vsLCDKeyEmulatorValue;
                                foundUpperValue = true;
                                foundLowerValue = true;
                                break;
                            }

                        case PZ70DialPosition.IAS:
                            {
                                upperValue = _iasLCDKeyEmulatorValue;
                                foundUpperValue = true;
                                break;
                            }

                        case PZ70DialPosition.HDG:
                            {
                                upperValue = _hdgLCDKeyEmulatorValue;
                                foundUpperValue = true;
                                break;
                            }

                        case PZ70DialPosition.CRS:
                            {
                                upperValue = _crsLCDKeyEmulatorValue;
                                foundUpperValue = true;
                                break;
                            }
                    }
                }
                else
                {
                    foreach (var dcsbiosBindingLCDPZ70 in _dcsBiosLcdBindings)
                    {
                        if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && dcsbiosBindingLCDPZ70.PZ70LCDPosition == PZ70LCDPosition.UpperLCD)
                        {
                            foundUpperValue = true;
                            upperValue = dcsbiosBindingLCDPZ70.CurrentValue;
                        }

                        if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && dcsbiosBindingLCDPZ70.PZ70LCDPosition == PZ70LCDPosition.LowerLCD)
                        {
                            foundLowerValue = true;
                            lowerValue = dcsbiosBindingLCDPZ70.CurrentValue;
                        }
                    }
                }
            }

            if (foundUpperValue)
            {
                if (upperValue < 0)
                {
                    upperValue = Math.Abs(upperValue);
                }

                var dataAsString = upperValue.ToString();

                var i = dataAsString.Length;
                var arrayPosition = 5;
                do
                {
                    // 3 0 0
                    // 1 5 6 0 0
                    // 1 2 3 4 5    
                    bytes[arrayPosition] = (byte)dataAsString[i - 1];
                    arrayPosition--;
                    i--;
                }
 while (i > 0);
            }

            if (foundLowerValue)
            {
                // Important!
                // Lower LCD will show a dash "-" for 0xEE.
                // Smallest negative value that can be shown is -9999
                // Largest positive value that can be shown is 99999
                if (lowerValue < -9999)
                {
                    lowerValue = -9999;
                }

                var dataAsString = lowerValue.ToString();

                var i = dataAsString.Length;
                var arrayPosition = 10;
                do
                {
                    // 3 0 0
                    // 1 5 6 0 0
                    // 1 2 3 4 5    
                    var s = dataAsString[i - 1];
                    if (s == '-')
                    {
                        bytes[arrayPosition] = 0xEE;
                    }
                    else
                    {
                        bytes[arrayPosition] = (byte)s;
                    }

                    arrayPosition--;
                    i--;
                }
 while (i > 0);
            }

            lock (_lcdLockObject)
            {
                SendLEDData(bytes);
            }

            Interlocked.Add(ref _doUpdatePanelLCD, -1);
        }

        public void SendLEDData(byte[] array)
        {
            try
            {
                // Common.DebugP("HIDWriteDevice writing feature data " + TypeOfSaitekPanel + " " + GuidString);
                HIDSkeletonBase.HIDWriteDevice?.WriteFeatureData(array);
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        private void CreateMultiKnobs()
        {
            SaitekPanelKnobs = MultiPanelKnob.GetMultiPanelKnobs();
        }

        private void DeviceAttachedHandler()
        {
            Startup();

            // IsAttached = true;
        }

        private void DeviceRemovedHandler()
        {
            Dispose();

            // IsAttached = false;
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public HashSet<DCSBIOSActionBindingPZ70> DCSBiosBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

        public HashSet<KeyBindingPZ70> KeyBindings
        {
            get => _knobBindings;
            set => _knobBindings = value;
        }

        public HashSet<BIPLinkPZ70> BIPLinkHashSet
        {
            get => _bipLinks;
            set => _bipLinks = value;
        }

        public HashSet<KeyBindingPZ70> KeyBindingsHashSet
        {
            get => _knobBindings;
            set => _knobBindings = value;
        }

        public List<OSCommandBindingPZ70> OSCommandHashSet
        {
            get => _operatingSystemCommandBindings;
            set => _operatingSystemCommandBindings = value;
        }

        public HashSet<DCSBIOSOutputBindingPZ70> LCDBindings
        {
            get => _dcsBiosLcdBindings;
            set => _dcsBiosLcdBindings = value;
        }

        public int LCDKnobSensitivity
        {
            get => _lcdKnobSensitivity;
            set => _lcdKnobSensitivity = value;
        }

        public PZ70DialPosition PZ70DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public PZ70LCDButtonByteList LCDButtonByteListHandler => _lcdButtonByteListHandler;
    }










    public enum PZ70DialPosition
    {
        ALT = 0,
        VS = 2,
        IAS = 4,
        HDG = 8,
        CRS = 16
    }



    public enum ControlListPZ70 : byte
    {
        ALL,
        DCSBIOS,
        KEYS,
        BIPS,
        OSCOMMAND
    }

}
