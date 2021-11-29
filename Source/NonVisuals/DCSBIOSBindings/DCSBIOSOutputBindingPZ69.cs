using System.Globalization;

namespace NonVisuals.DCSBIOSBindings
{
    using System;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.Radios;
    using NonVisuals.Saitek;

    [Serializable]
    public class DCSBIOSOutputBindingPZ69
    {
        /*
         * This class binds a LCD on the PZ69 with a DCSBIOSOutput
         * This is for the Full Emulator of the PZ69
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDPZ69, all data will be shown
         */
        private double _currentValue;
        private PZ69DialPosition _pz69DialPosition;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; // If this is set to !null value then ignore the _dcsbiosOutput
        private PZ69LCDPosition _pz69LCDPosition;

        private bool _limitDecimalPlaces = false;
        private NumberFormatInfo _numberFormatInfoDecimals;


        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ69)");
            }

            if (settings.StartsWith("RadioPanelDCSBIOSLCD{") && settings.Contains("DCSBiosOutput{"))
            {
                // RadioPanelDCSBIOSLCD{COM1}\o/{LowerLCD}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}\o/{False|0}
                // RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCD}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}\o/{False|0}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // [0]
                // RadioPanelDCSBIOSLCD{COM1}
                var param0 = parameters[0].Replace("RadioPanelDCSBIOSLCD{", string.Empty).Replace("}", string.Empty);
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);

                // [1]
                // {LowerLCD}
                var param1 = parameters[1].Replace("{", string.Empty).Replace("}", string.Empty).Trim();
                _pz69LCDPosition = (PZ69LCDPosition)Enum.Parse(typeof(PZ69LCDPosition), param1);

                if (settings.Contains("DCSBiosOutputFormula{"))
                {
                    // [2]
                    // DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                    _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                    _dcsbiosOutputFormula.ImportString(parameters[2]);
                }
                else
                {
                    // [2]
                    // DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                    _dcsbiosOutput = new DCSBIOSOutput();
                    _dcsbiosOutput.ImportString(parameters[2]);
                }
                
                // [3]
                // {False|0}
                /*
                 * This is a new setting so it may not exist in user's current configuration
                 */
                if (!string.IsNullOrEmpty(parameters[3]))
                {
                    var decimalSettings = parameters[3].Replace("{", String.Empty).Replace("}", String.Empty);
                    var decimalSettingValues = decimalSettings.Split('|');
                    _limitDecimalPlaces = bool.Parse(decimalSettingValues[0]);
                    if (_limitDecimalPlaces)
                    {
                        _numberFormatInfoDecimals = new NumberFormatInfo();
                        _numberFormatInfoDecimals.NumberDecimalSeparator = ".";
                        _numberFormatInfoDecimals.NumberDecimalDigits = int.Parse(decimalSettingValues[1]);
                    }
                }
            }
        }

        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }
        
        public void SetNumberOfDecimals(bool limitDecimals, int decimalPlaces = 0)
        {
            _limitDecimalPlaces = limitDecimals;
            _numberFormatInfoDecimals = new NumberFormatInfo();
            _numberFormatInfoDecimals.NumberDecimalSeparator = ".";
            _numberFormatInfoDecimals.NumberDecimalDigits = decimalPlaces;
        }
        
        public double CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                if (UseFormula)
                {
                    _currentValue = DCSBIOSOutputFormulaObject.Evaluate(false);
                }
            }
        }

        public string CurrentValueAsString
        {
            get
            {
                if (_limitDecimalPlaces)
                {
                    return string.Format(_numberFormatInfoDecimals, "{0:N}", _currentValue);
                }
                return _currentValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        public DCSBIOSOutput DCSBIOSOutputObject
        {
            get => _dcsbiosOutput;
            set
            {
                _dcsbiosOutput = value;
                _dcsbiosOutputFormula = null;
            }
        }

        public DCSBIOSOutputFormula DCSBIOSOutputFormulaObject
        {
            get => _dcsbiosOutputFormula;
            set
            {
                _dcsbiosOutputFormula = value;
                _dcsbiosOutput = null;
            }
        }


        public string ExportSettings()
        {
            if (DCSBIOSOutputObject == null && DCSBIOSOutputFormulaObject == null)
            {
                return null;
            }

            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in DCSBIOSBindingLCDPZ69 for LCD " + _pz69LCDPosition + ". Cannot export.");
            }

            if (_dcsbiosOutputFormula != null)
            {
                // RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}\o/{True|1}
                // RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}\o/{False|0}
                return "RadioPanelDCSBIOSLCD{" +
                    Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" +
                    SaitekConstants.SEPARATOR_SYMBOL + "{" + _pz69LCDPosition + "}" +
                    SaitekConstants.SEPARATOR_SYMBOL + _dcsbiosOutputFormula +
                    SaitekConstants.SEPARATOR_SYMBOL + "{" + _limitDecimalPlaces + "|" + (_numberFormatInfoDecimals == null ? "0" : _numberFormatInfoDecimals.NumberDecimalDigits.ToString()) + "}";
            }

            return "RadioPanelDCSBIOSLCD{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" + 
                   SaitekConstants.SEPARATOR_SYMBOL + "{" + _pz69LCDPosition + "}" + 
                   SaitekConstants.SEPARATOR_SYMBOL + _dcsbiosOutput +
                   SaitekConstants.SEPARATOR_SYMBOL + "{" + _limitDecimalPlaces + "|" + (_numberFormatInfoDecimals == null ? "0" : _numberFormatInfoDecimals.NumberDecimalDigits.ToString()) + "}";
        }

        public PZ69LCDPosition PZ69LcdPosition
        {
            get => _pz69LCDPosition;
            set => _pz69LCDPosition = value;
        }

        public bool HasBinding => _dcsbiosOutput != null || _dcsbiosOutputFormula != null;

        public bool UseFormula => _dcsbiosOutputFormula != null;
    }
}
