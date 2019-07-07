using System;
using DCS_BIOS;
using NonVisuals.Radios;


namespace NonVisuals
{
    
    public class DCSBIOSBindingLCDPZ69
    {
        /*
         * This class binds a LCD on the PZ69 with a DCSBIOSOutput
         * This is for the Full Emulator of the PZ69
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDPZ69, all data will be shown
         */
        private int _currentValue = 0;
        private PZ69DialPosition _pz69DialPosition;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private PZ69LCDPosition _pz69LCDPosition;
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ69)");
            }
            if (settings.StartsWith("RadioPanelDCSBIOSLCD{") && settings.Contains("DCSBiosOutput{"))
            {
                //RadioPanelDCSBIOSLCD{COM1}\o/{LowerLCD}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //RadioPanelDCSBIOSLCD{COM1}
                var param0 = parameters[0].Replace("RadioPanelDCSBIOSLCD{", "").Replace("}", "");
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);

                //[1]
                //{LowerLCD}
                var param1 = parameters[1].Replace("{", "").Replace("}", "").Trim();
                _pz69LCDPosition = (PZ69LCDPosition)Enum.Parse(typeof(PZ69LCDPosition), param1);

                //[2]
                //DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                _dcsbiosOutput = new DCSBIOSOutput();
                _dcsbiosOutput.ImportString(parameters[2]);
            }
            if (settings.StartsWith("RadioPanelDCSBIOSLCD{") && settings.Contains("DCSBiosOutputFormula{"))
            {
                //RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCD}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //MultiPanelDCSBIOSFormulaLCD{COM1}
                var param0 = parameters[0].Replace("RadioPanelDCSBIOSLCD{", "").Replace("}", "").Trim();
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);

                //[1]
                //{UpperLCD}
                var param1 = parameters[1].Replace("{", "").Replace("}", "").Trim();
                _pz69LCDPosition = (PZ69LCDPosition)Enum.Parse(typeof(PZ69LCDPosition), param1);

                //[2]
                //DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                _dcsbiosOutputFormula.ImportString(parameters[2]);
            }
        }

        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }

        public int CurrentValue
        {
            get => _currentValue;
            set => _currentValue = value;
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
                //RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}
                return "RadioPanelDCSBIOSLCD{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" + SeparatorChars + "{" + _pz69LCDPosition + "}" + SeparatorChars + _dcsbiosOutputFormula.ToString();
            }
            return "RadioPanelDCSBIOSLCD{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" + SeparatorChars + "{" + _pz69LCDPosition + "}" + SeparatorChars + _dcsbiosOutput.ToString();
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
