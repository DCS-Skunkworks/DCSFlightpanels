using System.Globalization;
using NonVisuals.Saitek.Panels;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;

    using MEF;

    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.EventArgs;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Radios.Misc;
    using NonVisuals.Saitek;

    public class RadioPanelPZ69Generic : RadioPanelPZ69Base
    {

        /*
         * Emulator profile for PZ69 containing DCS-BIOS
         * For a specific toggle/switch/lever/knob the PZ69 can have :
         * - single key binding
         * - sequenced key binding
         * - DCS-BIOS control
         * - BIP Link.
         */
        // private HashSet<DCSBIOSBindingPZ69> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ69>();
        private HashSet<KeyBindingPZ69DialPosition> _keyBindings = new();
        private List<OSCommandBindingPZ69FullEmulator> _operatingSystemCommandBindings = new();
        private readonly HashSet<RadioPanelPZ69DisplayValue> _displayValues = new();
        private HashSet<DCSBIOSOutputBindingPZ69> _dcsBiosLcdBindings = new();
        private HashSet<DCSBIOSActionBindingPZ69> _dcsBiosBindings = new();
        private readonly HashSet<BIPLinkPZ69> _bipLinks = new();
        private readonly object _lcdDataVariablesLockObject = new();

        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesUpper = new() { RadioPanelPZ69KnobsEmulator.UpperCOM1, RadioPanelPZ69KnobsEmulator.UpperCOM2, RadioPanelPZ69KnobsEmulator.UpperNAV1, RadioPanelPZ69KnobsEmulator.UpperNAV2, RadioPanelPZ69KnobsEmulator.UpperADF, RadioPanelPZ69KnobsEmulator.UpperDME, RadioPanelPZ69KnobsEmulator.UpperXPDR };
        private readonly List<RadioPanelPZ69KnobsEmulator> _panelPZ69DialModesLower = new() { RadioPanelPZ69KnobsEmulator.LowerCOM1, RadioPanelPZ69KnobsEmulator.LowerCOM2, RadioPanelPZ69KnobsEmulator.LowerNAV1, RadioPanelPZ69KnobsEmulator.LowerNAV2, RadioPanelPZ69KnobsEmulator.LowerADF, RadioPanelPZ69KnobsEmulator.LowerDME, RadioPanelPZ69KnobsEmulator.LowerXPDR };
        private double _upperActive = -1;
        private double _upperStandby = -1;
        private double _lowerActive = -1;
        private double _lowerStandby = -1;
        private PZ69DialPosition _pz69UpperDialPosition = PZ69DialPosition.UpperCOM1;
        private PZ69DialPosition _pz69LowerDialPosition = PZ69DialPosition.LowerCOM1;
        private long _doUpdatePanelLCD;

        private bool _settingsAreBeingImported;

        public RadioPanelPZ69Generic(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CreateSwitchKeys();
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
                SetLastException(ex);
            }
        }
        
        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            _settingsAreBeingImported = true;

            var settings = genericPanelBinding.Settings;
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2)
                {

                    if (setting.StartsWith("RadioPanelKeyDialPos{"))
                    {
                        var keyBinding = new KeyBindingPZ69DialPosition();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("RadioPanelOSPZ69Full"))
                    {
                        var operatingSystemCommand = new OSCommandBindingPZ69FullEmulator();
                        operatingSystemCommand.ImportSettings(setting);
                        _operatingSystemCommandBindings.Add(operatingSystemCommand);
                    }
                    else if (setting.StartsWith("PZ69DisplayValue{"))
                    {
                        var radioPanelPZ69DisplayValue = new RadioPanelPZ69DisplayValue();
                        radioPanelPZ69DisplayValue.ImportSettings(setting);
                        _displayValues.Add(radioPanelPZ69DisplayValue);
                    }
                    else if (setting.StartsWith("RadioPanelBIPLink{"))
                    {
                        var bipLinkPZ69 = new BIPLinkPZ69();
                        bipLinkPZ69.ImportSettings(setting);
                        _bipLinks.Add(bipLinkPZ69);
                    }
                    else if (setting.StartsWith("RadioPanelDCSBIOSLCD{"))
                    {
                        var dcsbiosBindingLCDPZ69 = new DCSBIOSOutputBindingPZ69();
                        dcsbiosBindingLCDPZ69.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsbiosBindingLCDPZ69);
                    }
                    else if (setting.StartsWith("RadioPanelDCSBIOSControl{"))
                    {
                        var dcsbiosBindingPZ69 = new DCSBIOSActionBindingPZ69();
                        dcsbiosBindingPZ69.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsbiosBindingPZ69);
                    }
                }

                _keyBindings = KeyBindingPZ69DialPosition.SetNegators(_keyBindings);
                _settingsAreBeingImported = false;
                AppEventHandler.SettingsApplied(this, HIDSkeletonBase.HIDInstance, TypeOfPanel);
            }
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

            foreach (var displayValue in _displayValues)
            {
                var tmp = displayValue.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }

            foreach (var bipLink in _bipLinks)
            {
                var tmp = bipLink.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }

            foreach (var dcsBiosLcdBindings in _dcsBiosLcdBindings)
            {
                var tmp = dcsBiosLcdBindings.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }

            foreach (var dcsBiosBindings in _dcsBiosBindings)
            {
                var tmp = dcsBiosBindings.ExportSettings();
                if (!string.IsNullOrEmpty(tmp))
                {
                    result.Add(tmp);
                }
            }

            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerCaller.RegisterPanelBinding(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (_settingsAreBeingImported)
            {
                return;
            }

            try
            {
                lock (_lcdDataVariablesLockObject)
                {
                    UpdateCounter(e.Address, e.Data);
                    foreach (var dcsbiosBindingLCD in _dcsBiosLcdBindings)
                    {
                        if (!dcsbiosBindingLCD.HasBinding)
                        {
                            return;
                        }
                        if (!dcsbiosBindingLCD.UseFormula && e.Address == dcsbiosBindingLCD.DCSBIOSOutputObject.Address)
                        {
                            lock (_lcdDataVariablesLockObject)
                            {
                                var tmp = dcsbiosBindingLCD.CurrentValue;
                                dcsbiosBindingLCD.CurrentValue =
                                    (int)dcsbiosBindingLCD.DCSBIOSOutputObject.GetUIntValue(e.Data);
                                if (tmp.CompareTo(dcsbiosBindingLCD.CurrentValue) != 0 &&
                                    (dcsbiosBindingLCD.DialPosition == _pz69UpperDialPosition ||
                                     dcsbiosBindingLCD.DialPosition == _pz69LowerDialPosition))
                                {
                                    // Update only if this LCD binding is in current use
                                    Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                }
                            }
                        }
                        else if (dcsbiosBindingLCD.UseFormula)
                        {
                            lock (_lcdDataVariablesLockObject)
                            {
                                if (dcsbiosBindingLCD.DCSBIOSOutputFormulaObject.CheckForMatch(e.Address, e.Data))
                                {
                                    var tmp = dcsbiosBindingLCD.CurrentValue;
                                    dcsbiosBindingLCD.CurrentValue =
                                        (int)dcsbiosBindingLCD.DCSBIOSOutputFormulaObject.Evaluate(false);
                                    if (tmp.CompareTo(dcsbiosBindingLCD.CurrentValue) != 0 &&
                                        (dcsbiosBindingLCD.DialPosition == _pz69UpperDialPosition ||
                                         dcsbiosBindingLCD.DialPosition == _pz69LowerDialPosition))
                                    {
                                        // Update only if this LCD binding is in current use
                                        Interlocked.Add(ref _doUpdatePanelLCD, 2);
                                    }
                                }
                            }
                        }
                    }
                }

                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DcsBiosDataReceived()");
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _keyBindings.Clear();
            _operatingSystemCommandBindings.Clear();
            _displayValues.Clear();
            _bipLinks.Clear();
            _dcsBiosLcdBindings.Clear();
            _dcsBiosBindings.Clear();

            if (setIsDirty)
            {
                SetIsDirty();
            }
        }

        public HashSet<KeyBindingPZ69DialPosition> KeyBindingsHashSet => _keyBindings;

        public List<OSCommandBindingPZ69FullEmulator> OSCommandHashSet
        {
            get => _operatingSystemCommandBindings;
            set => _operatingSystemCommandBindings = value;
        }

        public HashSet<BIPLinkPZ69> BipLinkHashSet => _bipLinks;

        public HashSet<RadioPanelPZ69DisplayValue> DisplayValueHashSet => _displayValues;


        private void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (ForwardPanelEvent)
            {
                foreach (var radioPanelKeyObject in hashSet)
                {
                    // Looks which switches has been switched and sees whether any key emulation has been tied to them.
                    var radioPanelKey = (RadioPanelPZ69KnobEmulator)radioPanelKeyObject;

                    if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc ||
                        radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec)
                    {
                        if (SkipCurrentFrequencyChange())
                        {
                            return;
                        }
                    }

                    if (radioPanelKey.IsOn)
                    {
                        if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperCOM1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperCOM2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperNAV1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperNAV2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperADF;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperDME;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                        {
                            _pz69UpperDialPosition = PZ69DialPosition.UpperXPDR;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerCOM1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerCOM2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerNAV1;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerNAV2;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerADF;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerDME;
                        }
                        else if (radioPanelKey.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                        {
                            _pz69LowerDialPosition = PZ69DialPosition.LowerXPDR;
                        }
                    }

                    var keyBindingFound = false;
                    foreach (var keyBinding in _keyBindings)
                    {
                        if (keyBinding.DialPosition == _pz69UpperDialPosition || keyBinding.DialPosition == _pz69LowerDialPosition)
                        {
                            if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
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
                                        PluginGamingPanelEnum.PZ69RadioPanel,
                                        (int)radioPanelKey.RadioPanelPZ69Knob,
                                        radioPanelKey.IsOn,
                                        keyBinding.OSKeyPress.KeyPressSequence);
                                }

                                break;
                            }
                        }
                    }

                    if (!isFirstReport && !keyBindingFound && PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                    {
                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                DCSFPProfile.SelectedProfile.Description,
                                HIDInstance,
                                PluginGamingPanelEnum.PZ69RadioPanel,
                                (int)radioPanelKey.RadioPanelPZ69Knob,
                                radioPanelKey.IsOn,
                                null);
                        }
                    }

                    foreach (var operatingSystemCommand in _operatingSystemCommandBindings)
                    {
                        if (operatingSystemCommand.DialPosition == _pz69UpperDialPosition || operatingSystemCommand.DialPosition == _pz69LowerDialPosition)
                        {
                            if (operatingSystemCommand.OSCommandObject != null && operatingSystemCommand.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && operatingSystemCommand.WhenTurnedOn == radioPanelKey.IsOn)
                            {
                                operatingSystemCommand.OSCommandObject.ExecuteCommand(new CancellationToken());
                                break;
                            }
                        }
                    }

                    foreach (var bipLinkPZ69 in _bipLinks)
                    {
                        if (bipLinkPZ69.BIPLights.Count > 0 && bipLinkPZ69.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && bipLinkPZ69.WhenTurnedOn == radioPanelKey.IsOn)
                        {
                            bipLinkPZ69.Execute();
                            break;
                        }
                    }

                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DialPosition == _pz69UpperDialPosition || dcsBiosBinding.DialPosition == _pz69LowerDialPosition)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob && dcsBiosBinding.WhenTurnedOn == radioPanelKey.IsOn)
                            {
                                dcsBiosBinding.SendDCSBIOSCommands(new CancellationToken());
                                break;
                            }
                        }
                    }
                }
            }

            foreach (var radioPanelKeyObject in hashSet)
            {
                // Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var radioPanelKey = (RadioPanelPZ69KnobEmulator)radioPanelKeyObject;
                if (radioPanelKey.IsOn)
                {
                    if (_panelPZ69DialModesUpper.Contains(radioPanelKey.RadioPanelPZ69Knob))
                    {
                        _upperActive = -1;
                        _upperStandby = -1;
                    }

                    if (_panelPZ69DialModesLower.Contains(radioPanelKey.RadioPanelPZ69Knob))
                    {
                        _lowerActive = -1;
                        _lowerStandby = -1;
                    }

                    foreach (var displayValue in _displayValues)
                    {
                        if (displayValue.RadioPanelPZ69Knob == radioPanelKey.RadioPanelPZ69Knob)
                        {
                            double parsedValue = double.Parse(displayValue.Value, NumberFormatInfoFullDisplay);

                            if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                            {
                                _upperActive = parsedValue;
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                            {
                                _upperStandby = parsedValue;
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                            {
                                _lowerActive = parsedValue;
                            }
                            else if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                            {
                                _lowerStandby = parsedValue;
                            }
                        }
                    }

                    Interlocked.Increment(ref _doUpdatePanelLCD);
                    ShowFrequenciesOnPanel();
                }
            }
        }

        private void ShowFrequenciesOnPanel()
        {
            if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
            {
                return;
            }

            lock (_lcdDataVariablesLockObject)
            {
                var bytes = new byte[21];

                for (var j = 0; j < bytes.Length; j++)
                {
                    bytes[j] = 0xFF;
                }

                /*
                 * Here we have a situation where the value to be displayed can be either just a dumb value
                 * or something from DCS-BIOS.
                 */
                var upperActiveString = _upperActive < 0 ? "" : _upperActive.ToString(CultureInfo.InvariantCulture);
                var upperStandbyString = _upperStandby < 0 ? "" : _upperStandby.ToString(CultureInfo.InvariantCulture);
                var lowerActiveString = _lowerActive < 0 ? "" : _lowerActive.ToString(CultureInfo.InvariantCulture);
                var lowerStandbyString = _lowerStandby < 0 ? "" : _lowerStandby.ToString(CultureInfo.InvariantCulture);

                var upperActiveFound = false;
                var upperStandbyFound = false;
                var lowerActiveFound = false;
                var lowerStandbyFound = false;
                // Find upper left => lower right data from the DCS-BIOS LCD holders
                foreach (var dcsbiosBindingLCDPZ69 in LCDBindings)
                {
                    if (dcsbiosBindingLCDPZ69.DialPosition == _pz69UpperDialPosition)
                    {
                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, dcsbiosBindingLCDPZ69.CurrentValueAsString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                            upperActiveFound = true;
                        }

                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, dcsbiosBindingLCDPZ69.CurrentValueAsString, PZ69LCDPosition.UPPER_STBY_RIGHT);
                            upperStandbyFound = true;
                        }
                    }

                    if (dcsbiosBindingLCDPZ69.DialPosition == _pz69LowerDialPosition)
                    {
                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, dcsbiosBindingLCDPZ69.CurrentValueAsString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            lowerActiveFound = true;
                        }

                        if (dcsbiosBindingLCDPZ69.PZ69LcdPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, dcsbiosBindingLCDPZ69.CurrentValueAsString, PZ69LCDPosition.LOWER_STBY_RIGHT);
                            lowerStandbyFound = true;
                        }
                    }
                }

                /*
                 * this is crap, must be refactored in the future
                 */
                if (!upperActiveFound)
                {
                    if (string.IsNullOrEmpty(upperActiveString))
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                    }
                    else
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, upperActiveString, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                    }
                }

                if (!upperStandbyFound)
                {

                    if (string.IsNullOrEmpty(upperStandbyString))
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                    }
                    else
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, upperStandbyString, PZ69LCDPosition.UPPER_STBY_RIGHT);
                    }
                }

                if (!lowerActiveFound)
                {
                    if (string.IsNullOrEmpty(lowerActiveString))
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                    }
                    else
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, lowerActiveString, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                    }
                }

                if (!lowerStandbyFound)
                {
                    if (string.IsNullOrEmpty(lowerStandbyString))
                    {
                        SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                    }
                    else
                    {
                        SetPZ69DisplayBytesDefault(ref bytes, lowerStandbyString, PZ69LCDPosition.LOWER_STBY_RIGHT);
                    }
                }

                SendLCDData(bytes);
            }

            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }


        public string GetKeyPressForLoggingPurposes(RadioPanelPZ69KnobEmulator radioPanelKey)
        {
            var result = string.Empty;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.RadioPanelPZ69Key == radioPanelKey.RadioPanelPZ69Knob && keyBinding.WhenTurnedOn == radioPanelKey.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }

            return result;
        }

        public void AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, string valueAsString, RadioPanelPZ69Display radioPanelDisplay)
        {
            if (string.IsNullOrEmpty(valueAsString))
            {
                ClearDisplayValue(radioPanelPZ69Knob, radioPanelDisplay);
                return;
            }

            var value = double.Parse(valueAsString, NumberFormatInfoFullDisplay);
            if (value < 0)
            {
                ClearDisplayValue(radioPanelPZ69Knob, radioPanelDisplay);
                return;
            }

            var found = false;
            foreach (var displayValue in _displayValues)
            {
                if (displayValue.RadioPanelPZ69Knob == radioPanelPZ69Knob && displayValue.RadioPanelDisplay == radioPanelDisplay)
                {
                    displayValue.Value = valueAsString;
                    found = true;
                }
            }

            if (!found)
            {
                var displayValue = new RadioPanelPZ69DisplayValue
                {
                    RadioPanelPZ69Knob = radioPanelPZ69Knob,
                    RadioPanelDisplay = radioPanelDisplay,
                    Value = valueAsString
                };
                _displayValues.Add(displayValue);
            }

            SetIsDirty();
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            if (string.IsNullOrEmpty(keyPress))
            {
                var tmp = new PZ69SwitchOnOff(radioPanelPZ69KeyOnOff.Switch, radioPanelPZ69KeyOnOff.ButtonState);
                ClearAllBindings(pz69DialPosition, tmp);
                return;
            }

            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState && keyBinding.DialPosition == pz69DialPosition)
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
                var keyBinding = new KeyBindingPZ69DialPosition
                {
                    RadioPanelPZ69Key = radioPanelPZ69KeyOnOff.Switch,
                    DialPosition = pz69DialPosition,
                    OSKeyPress = new KeyPress(keyPress, keyPressLength),
                    WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState
                };
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingPZ69DialPosition.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;

            var found = false;

            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            foreach (var operatingSystemCommandBinding in _operatingSystemCommandBindings)
            {
                if (operatingSystemCommandBinding.DialPosition == pz69DialPosition && operatingSystemCommandBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && operatingSystemCommandBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    operatingSystemCommandBinding.OSCommandObject = operatingSystemCommand;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var operatingSystemCommandBindingPZ69Full = new OSCommandBindingPZ69FullEmulator
                {
                    DialPosition = pz69DialPosition,
                    RadioPanelPZ69Key = radioPanelPZ69KeyOnOff.Switch,
                    OSCommandObject = operatingSystemCommand,
                    WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState
                };
                _operatingSystemCommandBindings.Add(operatingSystemCommandBindingPZ69Full);
            }

            SetIsDirty();
        }


        public void ClearAllBindings(PZ69DialPosition pz69DialPosition, PZ69SwitchOnOff radioPanelPZ69KnobOnOff)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.DialPosition == pz69DialPosition && keyBinding.RadioPanelPZ69Key == radioPanelPZ69KnobOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KnobOnOff.ButtonState)
                {
                    keyBinding.OSKeyPress = null;
                }
            }

            SetIsDirty();
        }

        public void ClearDisplayValue(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob, RadioPanelPZ69Display radioPanelPZ69Display)
        {
            _displayValues.RemoveWhere(x => x.RadioPanelPZ69Knob == radioPanelPZ69Knob && x.RadioPanelDisplay == radioPanelPZ69Display);
            SetIsDirty();
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);

            if (keySequence.Count == 0)
            {
                RemoveSwitchFromList(ControlList.KEYS, radioPanelPZ69KeyOnOff);
                SetIsDirty();
                return;
            }

            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69KeyOnOff.Switch && keyBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState && keyBinding.DialPosition == pz69DialPosition)
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
                var keyBinding = new KeyBindingPZ69DialPosition
                {
                    RadioPanelPZ69Key = radioPanelPZ69KeyOnOff.Switch,
                    DialPosition = pz69DialPosition,
                    OSKeyPress = new KeyPress(description, keySequence),
                    WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState
                };
                _keyBindings.Add(keyBinding);
            }

            _keyBindings = KeyBindingPZ69DialPosition.SetNegators(_keyBindings);
            SetIsDirty();
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var bipLinkPZ69 = (BIPLinkPZ69)bipLink;

            if (bipLinkPZ69.BIPLights.Count == 0)
            {
                RemoveSwitchFromList(ControlList.BIPS, radioPanelPZ69KeyOnOff);
                SetIsDirty();
                return;
            }

            var found = false;

            foreach (var tmpBipLink in _bipLinks)
            {
                if (tmpBipLink.RadioPanelPZ69Knob == radioPanelPZ69KeyOnOff.Switch && tmpBipLink.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    tmpBipLink.BIPLights = bipLinkPZ69.BIPLights;
                    tmpBipLink.Description = bipLinkPZ69.Description;
                    tmpBipLink.RadioPanelPZ69Knob = radioPanelPZ69KeyOnOff.Switch;
                    found = true;
                    break;
                }
            }

            if (!found && bipLinkPZ69.BIPLights.Count > 0)
            {
                bipLinkPZ69.RadioPanelPZ69Knob = radioPanelPZ69KeyOnOff.Switch;
                bipLinkPZ69.WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState;
                _bipLinks.Add(bipLinkPZ69);
            }

            SetIsDirty();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutput dcsbiosOutput, PZ69LCDPosition pz69LCDPosition, bool limitDecimals, int decimalPlaces)
        {

            lock (_lcdDataVariablesLockObject)
            {
                var found = false;
                var pz69DialPosition = _pz69UpperDialPosition;
                if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT ||
                    pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                {
                    pz69DialPosition = _pz69LowerDialPosition;
                }

                foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
                {
                    if (dcsBiosBindingLCD.DialPosition == pz69DialPosition &&
                        dcsBiosBindingLCD.PZ69LcdPosition == pz69LCDPosition)
                    {
                        dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                        dcsBiosBindingLCD.SetNumberOfDecimals(limitDecimals, decimalPlaces);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var dcsBiosBindingLCD = new DCSBIOSOutputBindingPZ69
                    {
                        DialPosition = pz69DialPosition,
                        DCSBIOSOutputObject = dcsbiosOutput,
                        PZ69LcdPosition = pz69LCDPosition
                    };
                    dcsBiosBindingLCD.SetNumberOfDecimals(limitDecimals, decimalPlaces);
                    _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
                }
            }

            SetIsDirty();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutputFormula dcsbiosOutputFormula, PZ69LCDPosition pz69LCDPosition, bool limitDecimals, int decimalPlaces)
        {

            lock (_lcdDataVariablesLockObject)
            {
                var found = false;
                var pz69DialPosition = _pz69UpperDialPosition;
                if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT ||
                    pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                {
                    pz69DialPosition = _pz69LowerDialPosition;
                }

                foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
                {
                    if (dcsBiosBindingLCD.DialPosition == pz69DialPosition &&
                        dcsBiosBindingLCD.PZ69LcdPosition == pz69LCDPosition)
                    {
                        dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                        dcsBiosBindingLCD.SetNumberOfDecimals(limitDecimals, decimalPlaces);
                        Debug.Print("3 found");
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var dcsBiosBindingLCD = new DCSBIOSOutputBindingPZ69
                    {
                        DialPosition = pz69DialPosition,
                        DCSBIOSOutputFormulaObject = dcsbiosOutputFormula,
                        PZ69LcdPosition = pz69LCDPosition
                    };
                    dcsBiosBindingLCD.SetNumberOfDecimals(limitDecimals, decimalPlaces);
                    _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
                }
            }

            SetIsDirty();
        }


        public void DeleteDCSBIOSLcdBinding(PZ69LCDPosition pz69LCDPosition)
        {
            lock (_lcdDataVariablesLockObject)
            {
                var pz69DialPosition = _pz69UpperDialPosition;
                if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT ||
                    pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                {
                    pz69DialPosition = _pz69LowerDialPosition;
                }
                
                foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
                {
                    if (dcsBiosBindingLCD.DialPosition == pz69DialPosition &&
                        dcsBiosBindingLCD.PZ69LcdPosition == pz69LCDPosition)
                    {
                        dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = null;
                        dcsBiosBindingLCD.DCSBIOSOutputObject = null;
                        break;
                    }
                }
            }
            
            SetIsDirty();
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
            var radioPanelPZ69KeyOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            if (dcsbiosInputs.Count == 0)
            {
                RemoveSwitchFromList(ControlList.DCSBIOS, radioPanelPZ69KeyOnOff);
                SetIsDirty();
                return;
            }

            var found = false;
            var pz69DialPosition = GetDial(radioPanelPZ69KeyOnOff.Switch);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == pz69DialPosition && dcsBiosBinding.RadioPanelPZ69Knob == radioPanelPZ69KeyOnOff.Switch && dcsBiosBinding.WhenTurnedOn == radioPanelPZ69KeyOnOff.ButtonState)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSActionBindingPZ69
                {
                    RadioPanelPZ69Knob = radioPanelPZ69KeyOnOff.Switch,
                    DialPosition = pz69DialPosition,
                    DCSBIOSInputs = dcsbiosInputs,
                    WhenTurnedOn = radioPanelPZ69KeyOnOff.ButtonState,
                    Description = description
                };
                _dcsBiosBindings.Add(dcsBiosBinding);
            }

            SetIsDirty();
        }

        public void DeleteDCSBIOSBinding(RadioPanelPZ69KnobsEmulator knob, bool whenTurnedOn)
        {
            var pz69DialPosition = GetDial(knob);

            // Removes config
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == pz69DialPosition && dcsBiosBinding.RadioPanelPZ69Knob == knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                    break;
                }
            }

            SetIsDirty();
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
            var pz69SwitchOnOff = (PZ69SwitchOnOff)panelSwitchOnOff;
            var controlListPZ69 = (ControlList)controlList;

            var found = false;
            var pz69DialPosition = GetDial(pz69SwitchOnOff.Switch);
            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.KEYS)
            {
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && keyBinding.WhenTurnedOn == pz69SwitchOnOff.ButtonState && keyBinding.DialPosition == pz69DialPosition)
                    {
                        keyBinding.OSKeyPress = null;
                    }

                    found = true;
                    break;
                }
            }

            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.DCSBIOS)
            {
                foreach (var dcsBiosBinding in _dcsBiosBindings)
                {
                    if (dcsBiosBinding.RadioPanelPZ69Knob == pz69SwitchOnOff.Switch && dcsBiosBinding.WhenTurnedOn == pz69SwitchOnOff.ButtonState && dcsBiosBinding.DialPosition == pz69DialPosition)
                    {
                        dcsBiosBinding.DCSBIOSInputs.Clear();
                    }

                    found = true;
                    break;
                }
            }

            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.BIPS)
            {
                foreach (var bipLink in _bipLinks)
                {
                    if (bipLink.RadioPanelPZ69Knob == pz69SwitchOnOff.Switch && bipLink.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                    {
                        bipLink.BIPLights.Clear();
                    }

                    found = true;
                    break;
                }
            }

            if (controlListPZ69 == ControlList.ALL || controlListPZ69 == ControlList.OSCOMMANDS)
            {
                OSCommandBindingPZ69FullEmulator operatingSystemCommandBindingPZ69 = null;
                for (int i = 0; i < _operatingSystemCommandBindings.Count; i++)
                {
                    var operatingSystemCommand = _operatingSystemCommandBindings[i];

                    if (operatingSystemCommand.RadioPanelPZ69Key == pz69SwitchOnOff.Switch && operatingSystemCommand.WhenTurnedOn == pz69SwitchOnOff.ButtonState)
                    {
                        operatingSystemCommandBindingPZ69 = _operatingSystemCommandBindings[i];
                        found = true;
                    }
                }

                if (operatingSystemCommandBindingPZ69 != null)
                {
                    _operatingSystemCommandBindings.Remove(operatingSystemCommandBindingPZ69);
                }
            }

            if (found)
            {
                SetIsDirty();
            }
        }

        public bool IsBindingActive(KeyBindingPZ69DialPosition keyBindingPZ69DialPosition)
        {
            var dial = GetDial(keyBindingPZ69DialPosition.RadioPanelPZ69Key);
            if (dial == keyBindingPZ69DialPosition.DialPosition)
            {
                return true;
            }

            return false;
        }

        private PZ69DialPosition GetDial(RadioPanelPZ69KnobsEmulator knob)
        {
            if (knob.ToString().Contains("Upper"))
            {
                return _pz69UpperDialPosition;
            }

            return _pz69LowerDialPosition;
        }

        /*
        private bool IsLowerArea(RadioPanelPZ69KnobsEmulator knob)
        {
            if (knob.ToString().Contains("Upper"))
            {
                return false;
            }
            return true;
        }

        private bool IsUpperArea(RadioPanelPZ69KnobsEmulator knob)
        {
            if (knob.ToString().Contains("Upper"))
            {
                return true;
            }
            return false;
        }
        */
        public void Clear(RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.RadioPanelPZ69Key == radioPanelPZ69Knob)
                {
                    keyBinding.OSKeyPress = null;
                }
            }

            SetIsDirty();
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }



        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55
            {
                DCSBiosOutputLED = dcsBiosOutput,
                LEDColor = panelLEDColor,
                SaitekLEDPosition = saitekPanelLEDPosition
            };
            return dcsOutputAndColorBinding;
        }

        private void CreateSwitchKeys()
        {
            SaitekPanelKnobs = RadioPanelPZ69KnobEmulator.GetRadioPanelKnobs();
        }

        public HashSet<DCSBIOSOutputBindingPZ69> LCDBindings
        {
            get => _dcsBiosLcdBindings;
            set => _dcsBiosLcdBindings = value;
        }

        public HashSet<DCSBIOSActionBindingPZ69> DCSBIOSBindings
        {
            get => _dcsBiosBindings;
            set => _dcsBiosBindings = value;
        }

        public PZ69DialPosition PZ69UpperDialPosition
        {
            get => _pz69UpperDialPosition;
        }

        public PZ69DialPosition PZ69LowerDialPosition
        {
            get => _pz69LowerDialPosition;
        }
    }
}
