using System;
using System.Collections.Generic;
using DCS_BIOS;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.DCSBIOSBindings;

namespace NonVisuals.Saitek
{
    public class MultiPanelPZ70 : SaitekPanel
    {
        private int _lcdKnobSensitivity;
        private volatile byte _knobSensitivitySkipper;
        private HashSet<DCSBIOSActionBindingPZ70> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingPZ70>();
        private HashSet<DCSBIOSOutputBindingPZ70> _dcsBiosLcdBindings = new HashSet<DCSBIOSOutputBindingPZ70>();
        private HashSet<KeyBindingPZ70> _knobBindings = new HashSet<KeyBindingPZ70>();
        private HashSet<OSCommandBindingPZ70> _osCommandBindings = new HashSet<OSCommandBindingPZ70>();
        private HashSet<BIPLinkPZ70> _bipLinks = new HashSet<BIPLinkPZ70>();
        private PZ70DialPosition _pz70DialPosition = PZ70DialPosition.ALT;

        private readonly object _lcdLockObject = new object();
        private readonly object _lcdDataVariablesLockObject = new object();
        private readonly PZ70LCDButtonByteList _lcdButtonByteListHandler = new PZ70LCDButtonByteList();
        //private volatile int _upperLcdValue = 0;
        //private volatile int _lowerLcdValue = 0;


        //0 - 40000
        private int _altLCDKeyEmulatorValue = 0;
        //-6000 - 6000
        private int _vsLCDKeyEmulatorValue = 0;
        //0-600
        private int _iasLCDKeyEmulatorValue = 0;
        //0-360
        private int _hdgLCDKeyEmulatorValue = 0;
        //0-360
        private int _crsLCDKeyEmulatorValue = 0;

        private readonly ClickSpeedDetector _altLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _vsLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _iasLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _hdgLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);
        private readonly ClickSpeedDetector _crsLCDKeyEmulatorValueChangeMonitor = new ClickSpeedDetector(15);

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
                Common.DebugP("MultiPanelPZ70.StartUp() : " + ex.Message);
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

        public override void ImportSettings(List<string> settings)
        {
            SettingsLoading = true;
            //Clear current bindings
            ClearSettings();
            if (settings == null || settings.Count == 0)
            {
                return;
            }

            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId) && setting.Contains(SettingsVersion()))
                {
                    if (setting.StartsWith("MultiPanelKnob{"))
                    {
                        var knobBinding = new KeyBindingPZ70();
                        knobBinding.ImportSettings(setting);
                        _knobBindings.Add(knobBinding);
                    }
                    else if (setting.StartsWith("MultiPanelOSPZ70"))
                    {
                        var osCommand = new OSCommandBindingPZ70();
                        osCommand.ImportSettings(setting);
                        _osCommandBindings.Add(osCommand);
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
            var result = "";
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
            e.ProfileHandlerEA.RegisterProfileData(this, ExportSettings());
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e) { }

        public override void ClearSettings()
        {
            _knobBindings.Clear();
            _osCommandBindings.Clear();
            _dcsBiosBindings.Clear();
            _dcsBiosLcdBindings.Clear();
            _bipLinks.Clear();
        }

        protected override void GamingPanelKnobChanged(IEnumerable<object> hashSet)
        {
            //Set _selectedMode and LCD button statuses
            //and performs the actual actions for key presses
            PZ70SwitchChanged(hashSet);
        }

        public void AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn)
        {
            if (string.IsNullOrEmpty(keys))
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.KEYS, multiPanelPZ70Knob, whenTurnedOn);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new KeyPress(keys, keyPressLength);
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var knobBinding = new KeyBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new KeyPress(keys, keyPressLength);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _knobBindings.Add(knobBinding);
            }
            Common.DebugP("MultiPanelPZ70 _knobBindings : " + _knobBindings.Count);
            _knobBindings = KeyBindingPZ70.SetNegators(_knobBindings);
            SetIsDirty();
        }

        public void AddOrUpdateOSCommandBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, OSCommand osCommand, bool whenTurnedOn)
        {
            //This must accept lists
            var found = false;

            foreach (var osCommandBinding in _osCommandBindings)
            {
                if (osCommandBinding.DialPosition == _pz70DialPosition && osCommandBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && osCommandBinding.WhenTurnedOn == whenTurnedOn)
                {
                    osCommandBinding.OSCommandObject = osCommand;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var osCommandBindingPZ70 = new OSCommandBindingPZ70();
                osCommandBindingPZ70.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                osCommandBindingPZ70.OSCommandObject = osCommand;
                osCommandBindingPZ70.WhenTurnedOn = whenTurnedOn;
                _osCommandBindings.Add(osCommandBindingPZ70);
            }
            SetIsDirty();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, MultiPanelPZ70Knobs multiPanelPZ70Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn)
        {
            if (sortedList.Count == 0)
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.KEYS, multiPanelPZ70Knob, whenTurnedOn);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new KeyPress(information, sortedList);
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var knobBinding = new KeyBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new KeyPress(information, sortedList);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _knobBindings.Add(knobBinding);
            }
            _knobBindings = KeyBindingPZ70.SetNegators(_knobBindings);
            SetIsDirty();
        }

        public void AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn)
        {
            if (dcsbiosInputs.Count == 0)
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.DCSBIOS, multiPanelPZ70Knob, whenTurnedOn);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                dcsBiosBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                dcsBiosBinding.DialPosition = _pz70DialPosition;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
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
            //Removes config
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

        public void AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs multiPanelKnob, BIPLinkPZ70 bipLinkPZ70, bool whenTurnedOn)
        {
            if (bipLinkPZ70.BIPLights.Count == 0)
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.BIPS, multiPanelKnob, whenTurnedOn);
                SetIsDirty();
                return;
            }
            //This must accept lists
            var found = false;

            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.MultiPanelPZ70Knob == multiPanelKnob && bipLink.WhenTurnedOn == whenTurnedOn)
                {
                    bipLink.BIPLights = bipLinkPZ70.BIPLights;
                    bipLink.Description = bipLinkPZ70.Description;
                    bipLink.MultiPanelPZ70Knob = multiPanelKnob;
                    found = true;
                    break;
                }
            }
            if (!found && bipLinkPZ70.BIPLights.Count > 0)
            {
                bipLinkPZ70.MultiPanelPZ70Knob = multiPanelKnob;
                bipLinkPZ70.WhenTurnedOn = whenTurnedOn;
                _bipLinks.Add(bipLinkPZ70);
            }
            SetIsDirty();
        }

        public void RemoveMultiPanelKnobFromList(ControlListPZ70 controlListPZ70, MultiPanelPZ70Knobs multiPanelPZ70Knob, bool whenTurnedOn)
        {
            var found = false;
            if (controlListPZ70 == ControlListPZ70.ALL || controlListPZ70 == ControlListPZ70.KEYS)
            {
                foreach (var knobBindingPZ70 in _knobBindings)
                {
                    if (knobBindingPZ70.DialPosition == _pz70DialPosition && knobBindingPZ70.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBindingPZ70.WhenTurnedOn == whenTurnedOn)
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
                    if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                    if (bipLink.DialPosition == _pz70DialPosition && bipLink.MultiPanelPZ70Knob == multiPanelPZ70Knob && bipLink.WhenTurnedOn == whenTurnedOn)
                    {
                        bipLink.BIPLights.Clear();
                        found = true;
                    }
                }
            }

            if (found)
            {
                SetIsDirty();
            }
        }

        private void PZ70SwitchChanged(IEnumerable<object> hashSet)
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
                if (Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly) && multiPanelKnob.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC || multiPanelKnob.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                {
                    Interlocked.Add(ref _doUpdatePanelLCD, 1);
                    LCDDialChangesHandle(multiPanelKnob);
                    UpdateLCD();
                }

                var found = false;
                foreach (var knobBinding in _knobBindings)
                {
                    if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.OSKeyPress != null && knobBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        if (knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC || knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                        {
                            if (!SkipCurrentLcdKnobChange())
                            {
                                knobBinding.OSKeyPress.Execute(new CancellationToken());
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
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
                foreach (var osCommand in _osCommandBindings)
                {
                    if (osCommand.DialPosition == _pz70DialPosition && osCommand.OSCommandObject != null && osCommand.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && osCommand.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        osCommand.OSCommandObject.Execute(new CancellationToken());
                        found = true;
                        break;
                    }
                }
                foreach (var bipLinkPZ70 in _bipLinks)
                {
                    if (bipLinkPZ70.BIPLights.Count > 0 && bipLinkPZ70.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && bipLinkPZ70.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        bipLinkPZ70.Execute();
                        break;
                    }
                }
            }
        }

        private void LCDDialChangesHandle(MultiPanelKnob multiPanelKnob)
        {
            if (SkipCurrentLcdKnobChangeLCD(true))
            {
                return;
            }

            bool increase = multiPanelKnob.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC;
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
                        //Do nothing all manipulation is let through
                        break;
                    }
                case -1:
                    {
                        //Skip every 2 manipulations
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
                        //Skip every 4 manipulations
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
            //Skip every 3 manipulations
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
            //345
            //15600
            //
            //[0x0]
            //[1] [2] [3] [4] [5]
            //[6] [7] [8] [9] [10]
            //[11 BUTTONS]


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
                if (Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
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
                    //    3 0 0
                    //1 5 6 0 0
                    //1 2 3 4 5    
                    bytes[arrayPosition] = (byte)dataAsString[i - 1];
                    arrayPosition--;
                    i--;
                } while (i > 0);
            }
            if (foundLowerValue)
            {
                //Important!
                //Lower LCD will show a dash "-" for 0xEE.
                //Smallest negative value that can be shown is -9999
                //Largest positive value that can be shown is 99999
                if (lowerValue < -9999)
                {
                    lowerValue = -9999;
                }
                var dataAsString = lowerValue.ToString();

                var i = dataAsString.Length;
                var arrayPosition = 10;
                do
                {
                    //    3 0 0
                    //1 5 6 0 0
                    //1 2 3 4 5    
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
                } while (i > 0);
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
                //Common.DebugP("HIDWriteDevice writing feature data " + TypeOfSaitekPanel + " " + GuidString);
                HIDSkeletonBase.HIDWriteDevice?.WriteFeatureData(array);
            }
            catch (Exception e)
            {
                Common.DebugP("SendLEDData() :\n" + e.Message + e.StackTrace);
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
            //IsAttached = true;
        }

        private void DeviceRemovedHandler()
        {
            Shutdown();
            //IsAttached = false;
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

        public HashSet<OSCommandBindingPZ70> OSCommandHashSet
        {
            get => _osCommandBindings;
            set => _osCommandBindings = value;
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

        public override string SettingsVersion()
        {
            return "2X";
        }

        public PZ70LCDButtonByteList LCDButtonByteListHandler => _lcdButtonByteListHandler;
    }


    public class PZ70LCDButtonByteList
    {
        /*
            LCD Button Byte
            00000000
            ||||||||_ AP_BUTTON
            |||||||_ HDG_BUTTON
            ||||||_ NAV_BUTTON
            |||||_ IAS_BUTTON
            ||||_ ALT_BUTTON
            |||_ VS_BUTTON
            ||_ APR_BUTTON
            |_ REV_BUTTON
        */
        private const byte AP_MASK = 1;
        private const byte HDG_MASK = 2;
        private const byte NAV_MASK = 4;
        private const byte IAS_MASK = 8;
        private const byte ALT_MASK = 16;
        private const byte VS_MASK = 32;
        private const byte APR_MASK = 64;
        private const byte REV_MASK = 128;
        private const int DIAL_ALT_MASK = 256;
        private const int DIAL_VS_MASK = 512;
        private const int DIAL_IAS_MASK = 1024;
        private const int DIAL_HDG_MASK = 2048;
        private const int DIAL_CRS_MASK = 4096;
        private const int BUTTON_IS_ON_MASK = 8192;

        //bool isSet = (b & mask) != 0
        //Set to 1" b |= mask
        //Set to zero
        //b &= ~mask
        //Toggle
        //b ^= mask
        private readonly byte[] _buttonBytes = new byte[8];
        private readonly int[] _buttonDialPosition = new int[8];

        public PZ70LCDButtonByteList()
        {
            _buttonDialPosition[0] |= DIAL_ALT_MASK;
            _buttonDialPosition[1] |= DIAL_VS_MASK;
            _buttonDialPosition[2] |= DIAL_IAS_MASK;
            _buttonDialPosition[3] |= DIAL_HDG_MASK;
            _buttonDialPosition[4] |= DIAL_CRS_MASK;
        }

        public bool FlipButton(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            try
            {
                return FlipButton(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob));
            }
            catch (Exception e)
            {
                Common.LogError(e, "Flipbutton()");
                throw;
            }
        }

        public bool IsOn(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knobs)
        {
            try
            {
                var dialMask = GetMaskForDialPosition(pz70DialPosition);
                var buttonMask = GetMaskForButton(multiPanelPZ70Knobs);
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & dialMask) != 0)
                    {
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel IsOn : Failed to find Mask for dial position " + pz70DialPosition + " knob " + multiPanelPZ70Knobs);
            }
            catch (Exception e)
            {
                Common.LogError(e, "IsOn()");
                throw;
            }
        }

        public int GetMaskForDialPosition(PZ70DialPosition pz70DialPosition)
        {
            try
            {

                switch (pz70DialPosition)
                {
                    case PZ70DialPosition.ALT:
                        {
                            return DIAL_ALT_MASK;
                        }
                    case PZ70DialPosition.VS:
                        {
                            return DIAL_VS_MASK;
                        }
                    case PZ70DialPosition.IAS:
                        {
                            return DIAL_IAS_MASK;
                        }
                    case PZ70DialPosition.HDG:
                        {
                            return DIAL_HDG_MASK;
                        }
                    case PZ70DialPosition.CRS:
                        {
                            return DIAL_CRS_MASK;
                        }
                }
                throw new Exception("Multipanel : Failed to find Mask for dial position " + pz70DialPosition);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetMaskForButton(MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            try
            {
                switch (multiPanelPZ70Knob)
                {
                    case MultiPanelPZ70Knobs.AP_BUTTON:
                        {
                            return AP_MASK;
                        }
                    case MultiPanelPZ70Knobs.HDG_BUTTON:
                        {
                            return HDG_MASK;
                        }
                    case MultiPanelPZ70Knobs.NAV_BUTTON:
                        {
                            return NAV_MASK;
                        }
                    case MultiPanelPZ70Knobs.IAS_BUTTON:
                        {
                            return IAS_MASK;
                        }
                    case MultiPanelPZ70Knobs.ALT_BUTTON:
                        {
                            return ALT_MASK;
                        }
                    case MultiPanelPZ70Knobs.VS_BUTTON:
                        {
                            return VS_MASK;
                        }
                    case MultiPanelPZ70Knobs.APR_BUTTON:
                        {
                            return APR_MASK;
                        }
                    case MultiPanelPZ70Knobs.REV_BUTTON:
                        {
                            return REV_MASK;
                        }
                }
                throw new Exception("Multipanel : Failed to find Mask for button " + multiPanelPZ70Knob);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool FlipButton(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] ^= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel FlipButton : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool SetButtonOnOrOff(int buttonDialMask, byte buttonMask, bool on)
        {
            if (on)
            {
                return SetButtonOn(buttonDialMask, buttonMask);
            }
            return SetButtonOff(buttonDialMask, buttonMask);
        }

        public bool SetButtonOff(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] &= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel ButtonOff : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool SetButtonOn(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] |= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel ButtonOn : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetButtonByte(PZ70DialPosition pz70DialPosition)
        {
            try
            {
                var mask = GetMaskForDialPosition(pz70DialPosition);
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    //(b & mask) != 0
                    if ((_buttonDialPosition[i] & mask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }
                throw new Exception("Multipanel : Failed to find button byte for " + pz70DialPosition);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetButtonByte(int buttonDialMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }
                throw new Exception("Multipanel GetButtonByte : Failed to find Mask for dial " + buttonDialMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool SetButtonByte(PZ70DialPosition pz70DialPosition, MultiPanelKnob multiPanelPZ70Knob)
        {
            return SetButtonOnOrOff(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob.MultiPanelPZ70Knob), multiPanelPZ70Knob.IsOn);
        }


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
        BIPS
    }

}
