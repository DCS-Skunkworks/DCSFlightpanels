using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public class StreamDeck35 : GamingPanel
    {
        private int _lcdKnobSensitivity;
        private volatile byte _knobSensitivitySkipper;
        private HashSet<DCSBIOSBindingStreamDeck> _dcsBiosBindings = new HashSet<DCSBIOSBindingStreamDeck>();
        private HashSet<DCSBIOSBindingLCDStreamDeck> _dcsBiosLcdBindings = new HashSet<DCSBIOSBindingLCDStreamDeck>();
        private HashSet<KeyBindingStreamDeck> _keyBindings = new HashSet<KeyBindingStreamDeck>();
        private HashSet<OSCommandBindingStreamDeck> _osCommandBindings = new HashSet<OSCommandBindingStreamDeck>();
        private HashSet<BIPLinkStreamDeck> _bipLinks = new HashSet<BIPLinkStreamDeck>();

        private readonly object _lcdLockObject = new object();
        private readonly object _lcdDataVariablesLockObject = new object();
        
        private long _doUpdatePanelLCD;

        public StreamDeck35():base(GamingPanelEnum.StreamDeck35, new HIDSkeleton(GamingPanelEnum.StreamDeck35, "TOBEDONECORRECTLYLATER"))
        {
            /*if (hidSkeleton.PanelType != GamingPanelEnum.PZ70MultiPanel)
            {
                throw new ArgumentException();
            }
            VendorId = 0x6A3;
            ProductId = 0xD06;*/
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
                Common.DebugP("StreamDeck35.StartUp() : " + ex.Message);
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

        protected override void StartListeningForPanelChanges()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Common.DebugP(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (SettingsLoading)
            {
                return;
            }
            UpdateCounter(e.Address, e.Data);
            foreach (var dcsbiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (!dcsbiosBindingLCD.UseFormula && e.Address == dcsbiosBindingLCD.DCSBIOSOutputObject.Address)
                {
                    lock (_lcdDataVariablesLockObject)
                    {
                        var tmp = dcsbiosBindingLCD.CurrentValue;
                        dcsbiosBindingLCD.CurrentValue = (int)dcsbiosBindingLCD.DCSBIOSOutputObject.GetUIntValue(e.Data);
                        if (tmp != dcsbiosBindingLCD.CurrentValue)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }
                else if (dcsbiosBindingLCD.UseFormula)
                {
                    if (dcsbiosBindingLCD.DCSBIOSOutputFormulaObject.CheckForMatch(e.Address, e.Data))
                    {
                        lock (_lcdDataVariablesLockObject)
                        {
                            var tmp = dcsbiosBindingLCD.CurrentValue;
                            dcsbiosBindingLCD.CurrentValue = dcsbiosBindingLCD.DCSBIOSOutputFormulaObject.Evaluate();
                            if (tmp != dcsbiosBindingLCD.CurrentValue)
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
                    if (setting.StartsWith("StreamDeckButton{"))
                    {
                        var knobBinding = new KeyBindingStreamDeck();
                        knobBinding.ImportSettings(setting);
                        _keyBindings.Add(knobBinding);
                    }
                    else if (setting.StartsWith("StreamDeckOS"))
                    {
                        var osCommand = new OSCommandBindingStreamDeck();
                        osCommand.ImportSettings(setting);
                        _osCommandBindings.Add(osCommand);
                    }
                    else if (setting.StartsWith("StreamDeckDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingStreamDeck = new DCSBIOSBindingStreamDeck();
                        dcsBIOSBindingStreamDeck.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingStreamDeck);
                    }
                    else if (setting.StartsWith("StreamDeckBIPLink{"))
                    {
                        var bipLinkStreamDeck = new BIPLinkStreamDeck();
                        bipLinkStreamDeck.ImportSettings(setting);
                        _bipLinks.Add(bipLinkStreamDeck);
                    }
                    else if (setting.StartsWith("StreamDeckDCSBIOSControlLCD{"))
                    {
                        var dcsBIOSBindingLCDStreamDeck = new DCSBIOSBindingLCDStreamDeck();
                        dcsBIOSBindingLCDStreamDeck.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsBIOSBindingLCDStreamDeck);
                    }
                }
            }
            SettingsLoading = false;
            _keyBindings = KeyBindingStreamDeck.SetNegators(_keyBindings);
            OnSettingsApplied();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            var result = new List<string>();

            foreach (var knobBinding in _keyBindings)
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

        public string GetKeyPressForLoggingPurposes(StreamDeck35Button streamDeckButton)
        {
            var result = "";
            foreach (var knobBinding in _keyBindings)
            {
                if (knobBinding.OSKeyPress != null && knobBinding.StreamDeckButton == streamDeckButton.Button && knobBinding.WhenTurnedOn == streamDeckButton.IsPressed)
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

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _osCommandBindings.Clear();
            _dcsBiosBindings.Clear();
            _dcsBiosLcdBindings.Clear();
            _bipLinks.Clear();
        }

        protected override void GamingPanelKnobChanged(IEnumerable<object> hashSet)
        {
            //Set _selectedMode and LCD button statuses
            //and performs the actual actions for key presses
            // ADD METHOD ?
        }

        public void AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn)
        {
            if (string.IsNullOrEmpty(keys))
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.KEYS, multiPanelPZ70Knob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var knobBinding in _keyBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        knobBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var knobBinding = new KeyBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(knobBinding);
            }
            Common.DebugP("MultiPanelPZ70 _knobBindings : " + _keyBindings.Count);
            _keyBindings = KeyBindingPZ70.SetNegators(_keyBindings);
            IsDirtyMethod();
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
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, MultiPanelPZ70Knobs multiPanelPZ70Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn)
        {
            if (sortedList.Count == 0)
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.KEYS, multiPanelPZ70Knob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var knobBinding in _keyBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        knobBinding.WhenTurnedOn = whenTurnedOn;
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
                knobBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(knobBinding);
            }
            _keyBindings = KeyBindingPZ70.SetNegators(_keyBindings);
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn)
        {
            if (dcsbiosInputs.Count == 0)
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.DCSBIOS, multiPanelPZ70Knob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                var dcsBiosBinding = new DCSBIOSBindingPZ70();
                dcsBiosBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                dcsBiosBinding.DialPosition = _pz70DialPosition;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
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
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ70();
                dcsBiosBindingLCD.DialPosition = _pz70DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutputFormula dcsbiosOutputFormula, PZ70LCDPosition pz70LCDPosition)
        {
            var found = false;
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                    Debug.Print("3 found");
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ70();
                dcsBiosBindingLCD.DialPosition = _pz70DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            IsDirtyMethod();
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
            IsDirtyMethod();
        }

        public void AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs multiPanelKnob, BIPLinkPZ70 bipLinkStreamDeck, bool whenTurnedOn)
        {
            if (bipLinkStreamDeck.BIPLights.Count == 0)
            {
                RemoveMultiPanelKnobFromList(ControlListPZ70.BIPS, multiPanelKnob, whenTurnedOn);
                IsDirtyMethod();
                return;
            }
            //This must accept lists
            var found = false;

            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.MultiPanelPZ70Knob == multiPanelKnob && bipLink.WhenTurnedOn == whenTurnedOn)
                {
                    bipLink.BIPLights = bipLinkStreamDeck.BIPLights;
                    bipLink.Description = bipLinkStreamDeck.Description;
                    bipLink.MultiPanelPZ70Knob = multiPanelKnob;
                    bipLink.WhenTurnedOn = whenTurnedOn;
                    found = true;
                    break;
                }
            }
            if (!found && bipLinkStreamDeck.BIPLights.Count > 0)
            {
                bipLinkStreamDeck.MultiPanelPZ70Knob = multiPanelKnob;
                bipLinkStreamDeck.WhenTurnedOn = whenTurnedOn;
                _bipLinks.Add(bipLinkStreamDeck);
            }
            IsDirtyMethod();
        }

        public void RemoveMultiPanelKnobFromList(ControlListPZ70 controlListPZ70, MultiPanelPZ70Knobs multiPanelPZ70Knob, bool whenTurnedOn)
        {
            var found = false;
            if (controlListPZ70 == ControlListPZ70.ALL || controlListPZ70 == ControlListPZ70.KEYS)
            {
                foreach (var knobBindingPZ70 in _keyBindings)
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
                IsDirtyMethod();
            }
        }

        private void StreamDeckButtonChanged(IEnumerable<object> hashSet)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var streamDeck35Button = (StreamDeck35Button)o;

                var found = false;
                foreach (var knobBinding in _keyBindings)
                {
                    if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.OSKeyPress != null && knobBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        if (knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC || knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                        {
                            if (!SkipCurrentLcdKnobChange())
                            {
                                knobBinding.OSKeyPress.Execute();
                            }
                            found = true;
                        }
                        else
                        {
                            knobBinding.OSKeyPress.Execute();
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
                        osCommand.OSCommandObject.Execute();
                        found = true;
                        break;
                    }
                }
                foreach (var bipLinkStreamDeck in _bipLinks)
                {
                    if (bipLinkStreamDeck.BIPLights.Count > 0 && bipLinkStreamDeck.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && bipLinkStreamDeck.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        bipLinkStreamDeck.Execute();
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

            bytes[11] = _lcdButtonByteListHandler.GetButtonByte(PZ70_DialPosition);

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
                    foreach (var dcsbiosBindingLCD in _dcsBiosLcdBindings)
                    {
                        if (dcsbiosBindingLCD.DialPosition == _pz70DialPosition && dcsbiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.UpperLCD)
                        {
                            foundUpperValue = true;
                            upperValue = dcsbiosBindingLCD.CurrentValue;
                        }

                        if (dcsbiosBindingLCD.DialPosition == _pz70DialPosition && dcsbiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.LowerLCD)
                        {
                            foundLowerValue = true;
                            lowerValue = dcsbiosBindingLCD.CurrentValue;
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

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
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

        public DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public HashSet<DCSBIOSBindingPZ70> DCSBiosBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

        public HashSet<KeyBindingPZ70> KeyBindings
        {
            get => _keyBindings;
            set => _keyBindings = value;
        }

        public HashSet<BIPLinkPZ70> BIPLinkHashSet
        {
            get => _bipLinks;
            set => _bipLinks = value;
        }

        public HashSet<KeyBindingPZ70> KeyBindingsHashSet
        {
            get => _keyBindings;
            set => _keyBindings = value;
        }

        public HashSet<OSCommandBindingPZ70> OSCommandHashSet
        {
            get => _osCommandBindings;
            set => _osCommandBindings = value;
        }

        public HashSet<DCSBIOSBindingLCDPZ70> LCDBindings
        {
            get => _dcsBiosLcdBindings;
            set => _dcsBiosLcdBindings = value;
        }

        public int LCDKnobSensitivity
        {
            get => _lcdKnobSensitivity;
            set => _lcdKnobSensitivity = value;
        }

        public PZ70DialPosition PZ70_DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public override string SettingsVersion()
        {
            return "2X";
        }
    }
}
