using System;
using System.Globalization;
using DCS_BIOS;

namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    [Serializable]
    public abstract class DCSBIOSOutputBindingBase
    {
        private double _currentValue;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula;// If this is set to !null value then ignore the _dcsbiosOutput

        public bool LimitDecimalPlaces { get; set; }
        private NumberFormatInfo _numberFormatInfoDecimals;

        public int DecimalPlaces
        {
            get
            {
                if (_numberFormatInfoDecimals != null)
                {
                    return _numberFormatInfoDecimals.NumberDecimalDigits;
                }

                return 0;
            }
        }
        
        public NumberFormatInfo NumberFormatInfoDecimals
        {
            get => _numberFormatInfoDecimals;
            set => _numberFormatInfoDecimals = value;
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
                if (LimitDecimalPlaces)
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

        public bool HasBinding => _dcsbiosOutput != null || _dcsbiosOutputFormula != null;

        public bool UseFormula => _dcsbiosOutputFormula != null;

        public abstract void ImportSettings(string settings);
        public abstract string ExportSettings();

        public void SetNumberOfDecimals(bool limitDecimals, int decimalPlaces = 0)
        {
            LimitDecimalPlaces = limitDecimals;
            _numberFormatInfoDecimals = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = decimalPlaces
            };
        }
        

        
    }
}
