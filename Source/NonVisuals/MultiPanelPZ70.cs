using System;
using System.Collections.Generic;
using System.Text;
using DCS_BIOS;
using HidLibrary;
using System.Threading;

namespace NonVisuals
{
    public enum PZ70DialPosition
    {
        ALT = 0,
        VS = 2,
        IAS = 4,
        HDG = 8,
        CRS = 16
    }

    public class MultiPanelPZ70 : SaitekPanel
    {
        private int _lcdKnobSensitivity;
        protected volatile byte KnobSensitivitySkipper;
        private HashSet<DCSBIOSBindingPZ70> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ70>();
        private HashSet<DCSBIOSBindingLCDPZ70> _dcsBiosLcdBindings = new HashSet<DCSBIOSBindingLCDPZ70>();
        private HashSet<KnobBindingPZ70> _knobBindings = new HashSet<KnobBindingPZ70>();
        private HashSet<MultiPanelKnob> _multiPanelKnobs = new HashSet<MultiPanelKnob>();
        private bool _isFirstNotification = true;
        private byte[] _oldMultiPanelValue = { 0, 0, 0 };
        private byte[] _newMultiPanelValue = { 0, 0, 0 };
        private PZ70DialPosition _pz70DialPosition = PZ70DialPosition.ALT;
        private readonly object _lcdLockObject = new object();

        private volatile int _upperLcdValue = 0;
        private volatile int _lowerLcdValue = 0;

        private bool _apButtonLightOn;
        private bool _hdgButtonLightOn;
        private bool _navButtonLightOn;
        private bool _iasButtonLightOn;
        private bool _altButtonLightOn;
        private bool _vsButtonLightOn;
        private bool _aprButtonLightOn;
        private bool _revButtonLightOn;
        private byte _buttonByte;

        private long _doUpdatePanelLCD;

        public MultiPanelPZ70(HIDSkeleton hidSkeleton)
            : base(SaitekPanelsEnum.PZ70MultiPanel, hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD06;
            CreateMultiKnobs();
            Startup();
        }

        public override sealed void Startup()
        {
            try
            {


                //Altitude
                //_dcsbiosOutputAltitude = DCSBIOSControlLocator.GetDCSBIOSOutput("ALT_MSL_FT");
                //_dcsbiosOutputs.Add(DCSBIOSControlLocator.GetDCSBIOSOutput("ALT_MSL_FT"));
                //Heading
                //_dcsbiosOutputHeading = DCSBIOSControlLocator.GetDCSBIOSOutput("HDG_DEG");
                //_dcsbiosOutputs.Add(DCSBIOSControlLocator.GetDCSBIOSOutput("HDG_DEG"));



                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
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
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId) && setting.Contains(SettingsVersion()))
                {
                    if (setting.StartsWith("MultiPanelKnob{"))
                    {
                        var knobBinding = new KnobBindingPZ70();
                        knobBinding.ImportSettings(setting);
                        _knobBindings.Add(knobBinding);
                    }
                    else if (setting.StartsWith("MultiPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingPZ70 = new DCSBIOSBindingPZ70();
                        dcsBIOSBindingPZ70.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingPZ70);
                    }
                    else if (setting.StartsWith("MultiPanelDCSBIOSControlLCD{"))
                    {
                        var dcsBIOSBindingLCDPZ70 = new DCSBIOSBindingLCDPZ70();
                        dcsBIOSBindingLCDPZ70.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsBIOSBindingLCDPZ70);
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

            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.OSKeyPress != null)
                {
                    result.Add(knobBinding.ExportSettings());
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

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            UpdateCounter(address, data);
            foreach (var dcsbiosBindingLCDPZ70 in _dcsBiosLcdBindings)
            {
                if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && address == dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.Address)
                {
                    if (!dcsbiosBindingLCDPZ70.UseFormula)
                    {
                        switch (dcsbiosBindingLCDPZ70.PZ70LCDPosition)
                        {
                            case PZ70LCDPosition.UpperLCD:
                                {
                                    var tmp = _upperLcdValue;
                                    _upperLcdValue = (int)dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.GetUIntValue(data);
                                    if (tmp != _upperLcdValue)
                                    {
                                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                    }
                                    break;
                                }
                            case PZ70LCDPosition.LowerLCD:
                                {
                                    var tmp = _lowerLcdValue;
                                    _lowerLcdValue = (int)dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.GetUIntValue(data);
                                    if (tmp != _lowerLcdValue)
                                    {
                                        Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                    }
                                    break;
                                }
                        }
                        UpdateLCD(_upperLcdValue, _lowerLcdValue);
                    }
                    else if (dcsbiosBindingLCDPZ70.UseFormula)
                    {
                        switch (dcsbiosBindingLCDPZ70.PZ70LCDPosition)
                        {
                            case PZ70LCDPosition.UpperLCD:
                                {
                                    if (dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.CheckForMatch(address, data))
                                    {
                                        var tmp = _upperLcdValue;
                                        _upperLcdValue = dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.Evaluate();
                                        if (tmp != _upperLcdValue)
                                        {
                                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                        }
                                    }
                                    break;
                                }
                            case PZ70LCDPosition.LowerLCD:
                                {
                                    if (dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.CheckForMatch(address, data))
                                    {
                                        var tmp = _lowerLcdValue;
                                        _lowerLcdValue = dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.Evaluate();
                                        if (tmp != _lowerLcdValue)
                                        {
                                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                        }
                                    }
                                    break;
                                }
                        }
                        UpdateLCD(_upperLcdValue, _lowerLcdValue);
                    }
                }
            }
        }

        public override void ClearSettings()
        {
            _knobBindings.Clear();
            _dcsBiosBindings.Clear();
            _dcsBiosLcdBindings.Clear();
        }

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 3)
            {
                Array.Copy(_newMultiPanelValue, _oldMultiPanelValue, 3);
                Array.Copy(report.Data, _newMultiPanelValue, 3);
                var hashSet = GetHashSetOfChangedKnobs(_oldMultiPanelValue, _newMultiPanelValue);

                //Set _selectedMode and LCD button statuses
                //and performs the actual actions for key presses
                PZ70SwitchChanged(hashSet);
                //Sends event
                OnSwitchesChanged(hashSet);


                _isFirstNotification = false;
                if (Common.DebugOn)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        stringBuilder.Append(Convert.ToString(report.Data[i], 2).PadLeft(8, '0') + "  ");
                    }
                    Common.DebugP(stringBuilder.ToString());
                    if (hashSet.Count > 0)
                    {
                        Common.DebugP("\nFollowing knobs has been changed:\n");
                        foreach (var multiPanelKnob in hashSet)
                        {
                            Common.DebugP(((MultiPanelKnob)multiPanelKnob).MultiPanelPZ70Knob + ", value is " + FlagValue(_newMultiPanelValue, ((MultiPanelKnob)multiPanelKnob)));
                        }
                    }
                }
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

        public void AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn = true)
        {
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
                        knobBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        knobBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var knobBinding = new KnobBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _knobBindings.Add(knobBinding);
            }
            Common.DebugP("MultiPanelPZ70 _knobBindings : " + _knobBindings.Count);
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, MultiPanelPZ70Knobs multiPanelPZ70Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveMultiPanelKnobFromList(2, multiPanelPZ70Knob, whenTurnedOn);
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
                        knobBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        knobBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var knobBinding = new KnobBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _knobBindings.Add(knobBinding);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveMultiPanelKnobFromList(1, multiPanelPZ70Knob, whenTurnedOn);
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

        public void AddOrUpdateDCSBIOSLcdBinding(DCSBIOSOutput dcsbiosOutput, PZ70LCDPosition pz70LCDPosition)
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

        public void AddOrUpdateDCSBIOSLcdBinding(DCSBIOSOutputFormula dcsbiosOutputFormula, PZ70LCDPosition pz70LCDPosition)
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
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ70();
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
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

        private void RemoveMultiPanelKnobFromList(int list, MultiPanelPZ70Knobs multiPanelPZ70Knob, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var knobBindingPZ70 in _knobBindings)
                        {
                            if (knobBindingPZ70.DialPosition == _pz70DialPosition && knobBindingPZ70.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBindingPZ70.WhenTurnedOn == whenTurnedOn)
                            {
                                knobBindingPZ70.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }
                        break;
                    }
            }
        }


        private void PZ70SwitchChanged(IEnumerable<object> hashSet)
        {
            Interlocked.Add(ref _doUpdatePanelLCD, 1);
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
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                _pz70DialPosition = PZ70DialPosition.VS;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                _pz70DialPosition = PZ70DialPosition.IAS;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                _pz70DialPosition = PZ70DialPosition.HDG;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                _pz70DialPosition = PZ70DialPosition.CRS;
                                break;
                            }
                    }
                }
            }
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var o in hashSet)
            {
                var multiPanelKnob = (MultiPanelKnob)o;
                if (multiPanelKnob.IsOn)
                {
                    switch (multiPanelKnob.MultiPanelPZ70Knob)
                    {
                        case MultiPanelPZ70Knobs.AP_BUTTON:
                            {
                                _apButtonLightOn = !_apButtonLightOn;
                                multiPanelKnob.IsOn = _apButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.HDG_BUTTON:
                            {
                                _hdgButtonLightOn = !_hdgButtonLightOn;
                                multiPanelKnob.IsOn = _hdgButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.NAV_BUTTON:
                            {
                                _navButtonLightOn = !_navButtonLightOn;
                                multiPanelKnob.IsOn = _navButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.IAS_BUTTON:
                            {
                                _iasButtonLightOn = !_iasButtonLightOn;
                                multiPanelKnob.IsOn = _iasButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.ALT_BUTTON:
                            {
                                _altButtonLightOn = !_altButtonLightOn;
                                multiPanelKnob.IsOn = _altButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.VS_BUTTON:
                            {
                                _vsButtonLightOn = !_vsButtonLightOn;
                                multiPanelKnob.IsOn = _vsButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.APR_BUTTON:
                            {
                                _aprButtonLightOn = !_aprButtonLightOn;
                                multiPanelKnob.IsOn = _aprButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                        case MultiPanelPZ70Knobs.REV_BUTTON:
                            {
                                _revButtonLightOn = !_revButtonLightOn;
                                multiPanelKnob.IsOn = _revButtonLightOn;
                                SetButtonByte();
                                break;
                            }
                    }
                }

                /*
                 * IMPORTANT
                 * ---------
                 * The LCD buttons toggle between on and off. It is the toggle value that defines if the button is OFF, not the fact that the user releases the button.
                 * Therefore the forementioned buttons cannot be used as usual in a loop with knobBinding.WhenTurnedOn
                 * Instead the buttons global bool value must be used!
                 * 
                 */
                var found = false;
                foreach (var knobBinding in _knobBindings)
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

            }
        }

        protected bool SkipCurrentLcdKnobChange()
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
                        KnobSensitivitySkipper++;
                        if (KnobSensitivitySkipper <= 2)
                        {
                            return true;
                        }
                        KnobSensitivitySkipper = 0;
                        break;
                    }
                case -2:
                    {
                        //Skip every 4 manipulations
                        KnobSensitivitySkipper++;
                        if (KnobSensitivitySkipper <= 4)
                        {
                            return true;
                        }
                        KnobSensitivitySkipper = 0;
                        break;
                    }
            }
            return false;
        }

        public void ClearAllBindings(MultiPanelPZ70KnobOnOff multiPanelPZ70KnobOnOff)
        {
            //This must accept lists
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.MultiPanelPZ70Knob == multiPanelPZ70KnobOnOff.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelPZ70KnobOnOff.On)
                {
                    knobBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70KnobOnOff.MultiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == multiPanelPZ70KnobOnOff.On)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("MultiPanelPZ70 _knobBindings : " + _knobBindings.Count);
            Common.DebugP("MultiPanelPZ70 _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }

        public void SetButtonByte()
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
            var apMask = 1;
            var hdgMask = 2;
            var navMask = 4;
            var iasMask = 8;
            var altMask = 16;
            var vsMask = 32;
            var aprMask = 64;
            var revMask = 128;

            //bool isSet = (b & mask) != 0
            //Set to 1" b |= mask
            //Set to zero
            //b &= ~mask
            //Toggle
            //b ^= mask

            if (_apButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | apMask);
            }
            else
            {
                _buttonByte &= (byte)~apMask;
            }
            if (_hdgButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | hdgMask);
            }
            else
            {
                _buttonByte &= (byte)~hdgMask;
            }
            if (_navButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | navMask);
            }
            else
            {
                _buttonByte &= (byte)~navMask;
            }
            if (_iasButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | iasMask);
            }
            else
            {
                _buttonByte &= (byte)~iasMask;
            }
            if (_altButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | altMask);
            }
            else
            {
                _buttonByte &= (byte)~altMask;
            }
            if (_vsButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | vsMask);
            }
            else
            {
                _buttonByte &= (byte)~vsMask;
            }
            if (_aprButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | aprMask);
            }
            else
            {
                _buttonByte &= (byte)~aprMask;
            }
            if (_revButtonLightOn)
            {
                _buttonByte = (byte)(_buttonByte | revMask);
            }
            else
            {
                _buttonByte &= (byte)~revMask;
            }
            UpdateLCD(_upperLcdValue, _lowerLcdValue);
        }

        public void UpdateLCD(int upperValue, int lowerValue)
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
            bytes[11] = _buttonByte;

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

            //Important!
            //Lower LCD will show a dash "-" for 0xEE.
            //Smallest negative value that can be shown is -9999
            //Largest positive value that can be shown is 99999
            if (lowerValue < -9999)
            {
                lowerValue = -9999;
            }
            dataAsString = lowerValue.ToString();

            i = dataAsString.Length;
            arrayPosition = 10;
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
                if (HIDSkeletonBase.HIDWriteDevice != null)
                {
                    Common.DebugP("HIDWriteDevice writing feature data " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDWriteDevice.WriteFeatureData(array);
                }
                //if (IsAttached)
                //{
                //Common.DebugP("Write ending to PZ70");
                //}w
            }
            catch (Exception e)
            {
                Common.DebugP("SendLEDData() :\n" + e.Message + e.StackTrace);
                SetLastException(e);
            }
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();


            //Common.DebugP("Old: " + Convert.ToString(oldValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[2], 2).PadLeft(8, '0'));
            //Common.DebugP("New: " + Convert.ToString(newValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[2], 2).PadLeft(8, '0'));
            for (var i = 0; i < 3; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var multiPanelKnob in _multiPanelKnobs)
                {
                    if (multiPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, multiPanelKnob.Mask) || _isFirstNotification))
                    {
                        multiPanelKnob.IsOn = FlagValue(newValue, multiPanelKnob);
                        //Do not add OFF signals for LCD buttons. They are not used. The OFF value is a TOGGLE value and respective button's bool value must be read instead.
                        if (!multiPanelKnob.IsOn)
                        {
                            switch (multiPanelKnob.MultiPanelPZ70Knob)
                            {
                                case MultiPanelPZ70Knobs.AP_BUTTON:
                                case MultiPanelPZ70Knobs.HDG_BUTTON:
                                case MultiPanelPZ70Knobs.NAV_BUTTON:
                                case MultiPanelPZ70Knobs.IAS_BUTTON:
                                case MultiPanelPZ70Knobs.ALT_BUTTON:
                                case MultiPanelPZ70Knobs.VS_BUTTON:
                                case MultiPanelPZ70Knobs.APR_BUTTON:
                                case MultiPanelPZ70Knobs.REV_BUTTON:
                                    {
                                        //Do not add OFF values for these buttons! Read comment above.
                                        break;
                                    }
                                default:
                                    {
                                        result.Add(multiPanelKnob);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            result.Add(multiPanelKnob);
                        }
                        //Common.DebugP("Following knob has changed : " + multiPanelKnob.MultiPanelPZ70Knob);
                    }
                }
            }
            return result;
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        private void CreateMultiKnobs()
        {
            _multiPanelKnobs = MultiPanelKnob.GetMultiPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, MultiPanelKnob multiPanelKnob)
        {
            return (currentValue[multiPanelKnob.Group] & multiPanelKnob.Mask) > 0;
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

        public HashSet<DCSBIOSBindingPZ70> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
            set { _dcsBiosBindings = value; }
        }

        public HashSet<KnobBindingPZ70> KeyBindings
        {
            get { return _knobBindings; }
            set { _knobBindings = value; }
        }

        public HashSet<MultiPanelKnob> MultiPanelKnobs
        {
            get { return _multiPanelKnobs; }
            set { _multiPanelKnobs = value; }
        }

        public HashSet<KnobBindingPZ70> KeyBindingsHashSet
        {
            get { return _knobBindings; }
            set { _knobBindings = value; }
        }

        public HashSet<DCSBIOSBindingLCDPZ70> LCDBindings
        {
            get { return _dcsBiosLcdBindings; }
            set { _dcsBiosLcdBindings = value; }
        }

        public int LCDKnobSensitivity
        {
            get { return _lcdKnobSensitivity; }
            set { _lcdKnobSensitivity = value; }
        }

        public PZ70DialPosition PZ70_DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public override String SettingsVersion()
        {
            return "2X";
        }
    }


}
