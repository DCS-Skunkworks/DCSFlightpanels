using System;
using System.Globalization;
using DCS_BIOS;
using MEF;
using NonVisuals.Radios;
using NonVisuals.Saitek;

namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    [Serializable]
    public class DCSBIOSOutputBindingPZ69 : DCSBIOSOutputBindingBase
    {
        /*
         * This class binds a LCD on the PZ69 with a DCSBIOSOutput
         * This is for the Full Emulator of the PZ69
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDPZ69, all data will be shown
         */
        private PZ69DialPosition _pz69DialPosition;
        private PZ69LCDPosition _pz69LCDPosition;
        

        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }

        public PZ69LCDPosition PZ69LcdPosition
        {
            get => _pz69LCDPosition;
            set => _pz69LCDPosition = value;
        }

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ69)");
            }

            if (settings.StartsWith("RadioPanelDCSBIOSLCD{"))
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
                    DCSBIOSOutputFormulaObject = new DCSBIOSOutputFormula();
                    DCSBIOSOutputFormulaObject.ImportString(parameters[2]);
                }
                else
                {
                    // [2]
                    // DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                    DCSBIOSOutputObject = new DCSBIOSOutput();
                    DCSBIOSOutputObject.ImportString(parameters[2]);
                }
                
                // [3]
                // {False|0}
                /*
                 * This is a new setting so it may not exist in user's current configuration
                 */
                if (parameters.Length == 4 && !string.IsNullOrEmpty(parameters[3]))
                {
                    var decimalSettings = parameters[3].Replace("{", String.Empty).Replace("}", String.Empty);
                    var decimalSettingValues = decimalSettings.Split('|');
                    LimitDecimalPlaces = bool.Parse(decimalSettingValues[0]);
                    if (LimitDecimalPlaces)
                    {
                        NumberFormatInfoDecimals = new NumberFormatInfo
                        {
                            NumberDecimalSeparator = ".",
                            NumberDecimalDigits = int.Parse(decimalSettingValues[1])
                        };
                    }
                }
            }
        }

        public override string ExportSettings()
        {
            if (DCSBIOSOutputObject == null && DCSBIOSOutputFormulaObject == null)
            {
                return null;
            }

            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in DCSBIOSBindingLCDPZ69 for LCD " + _pz69LCDPosition + ". Cannot export.");
            }

            if (DCSBIOSOutputFormulaObject != null)
            {
                // RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}\o/{True|1}
                // RadioPanelDCSBIOSLCD{COM1}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}\o/{False|0}
                return "RadioPanelDCSBIOSLCD{" +
                    Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" +
                    SaitekConstants.SEPARATOR_SYMBOL + "{" + _pz69LCDPosition + "}" +
                    SaitekConstants.SEPARATOR_SYMBOL + DCSBIOSOutputFormulaObject +
                    SaitekConstants.SEPARATOR_SYMBOL + "{" + LimitDecimalPlaces + "|" + (NumberFormatInfoDecimals == null ? "0" : NumberFormatInfoDecimals.NumberDecimalDigits.ToString()) + "}";
            }

            return "RadioPanelDCSBIOSLCD{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" + 
                   SaitekConstants.SEPARATOR_SYMBOL + "{" + _pz69LCDPosition + "}" + 
                   SaitekConstants.SEPARATOR_SYMBOL + DCSBIOSOutputObject +
                   SaitekConstants.SEPARATOR_SYMBOL + "{" + LimitDecimalPlaces + "|" + (NumberFormatInfoDecimals == null ? "0" : NumberFormatInfoDecimals.NumberDecimalDigits.ToString()) + "}";
        }
    }
}
