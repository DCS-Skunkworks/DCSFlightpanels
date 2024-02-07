using System;
using System.Globalization;
using DCS_BIOS;
using DCS_BIOS.Serialized;
using NonVisuals.Panels.Saitek;
using NonVisuals.Panels.Saitek.Panels;

namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    public enum PZ70LCDPosition
    {
        UpperLCD,
        LowerLCD
    }

    [Serializable]
    public class DCSBIOSOutputBindingPZ70 : DCSBIOSOutputBindingBase
    {
        /*
         * This class binds a LCD row on the PZ70 with a DCSBIOSOutput
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDPZ70, all data will be shown
         */
        private PZ70DialPosition _pz70DialPosition;
        private PZ70LCDPosition _pz70LCDPosition;

        public PZ70DialPosition DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public PZ70LCDPosition PZ70LCDPosition
        {
            get => _pz70LCDPosition;
            set => _pz70LCDPosition = value;
        }

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }

            if (settings.StartsWith("MultiPanelDCSBIOSControlLCD{"))
            {
                // MultiPanelDCSBIOSControlLCD{ALT}\o/{LowerLCD}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}\o/{False,0}
                // MultiPanelDCSBIOSControlLCD{ALT}\o/{UpperLCD}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}\o/{False,0}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // [0]
                // MultiPanelDCSBIOSControlLCD{ALT}
                var param0 = parameters[0].Replace("MultiPanelDCSBIOSControlLCD{", string.Empty).Replace("}", string.Empty);
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                // [1]
                // {LowerLCD}
                var param1 = parameters[1].Replace("{", string.Empty).Replace("}", string.Empty).Trim();
                _pz70LCDPosition = (PZ70LCDPosition)Enum.Parse(typeof(PZ70LCDPosition), param1);

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

            if (DCSBIOSOutputFormulaObject != null)
            {
                // MultiPanelDCSBIOSControlLCD{ALT}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}
                return "MultiPanelDCSBIOSControlLCD{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + _pz70LCDPosition + "}" + SaitekConstants.SEPARATOR_SYMBOL + DCSBIOSOutputFormulaObject;
            }

            return "MultiPanelDCSBIOSControlLCD{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + _pz70LCDPosition + "}" + SaitekConstants.SEPARATOR_SYMBOL + DCSBIOSOutputObject;
        }
    }
}
