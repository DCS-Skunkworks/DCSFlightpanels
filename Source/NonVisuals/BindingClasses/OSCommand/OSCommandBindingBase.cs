using System;
using ClassLibraryCommon;
using NonVisuals.Saitek;

namespace NonVisuals.BindingClasses.OSCommand
{
    [Serializable]
    public abstract class OSCommandBindingBase
    {
        /*
         This is the base class for all the OSCommand binding classes.
         It is used when a user maps a OSCommand to a physical key on a panel.
         */
        private bool _whenOnTurnedOn = true;

        private NonVisuals.OSCommand _operatingSystemCommand;

        internal abstract void ImportSettings(string settings);

        public abstract string ExportSettings();


        public Tuple<string, string> ParseSettingV1(string config)
        {
            var mode = "";
            var key = "";

            if (string.IsNullOrEmpty(config))
            {
                throw new ArgumentException("Import string empty. (OSCommandBinding)");
            }

            // SwitchPanelOSPZ55{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}
            // MultiPanelOSPZ70{ALT}\o/{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}
            // RadioPanelOSPZ69{1UpperCOM1}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}
            // RadioPanelOSPZ69Full{COM1}\o/{1UpperCOM1}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}
            var parameters = config.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

            if (config.Contains("MultiPanel") || config.Contains("RadioPanelOSPZ69Full")) // Has additional setting which tells which position leftmost dial is in
            {
                // MultiPanelOSPZ70{ALT}
                // RadioPanelOSPZ69Full{COM1}
                mode = Common.RemoveCurlyBrackets(parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture))).Trim();

                // {0LowerFreqSwitch}
                // {1LCD_WHEEL_DEC}
                WhenTurnedOn = Common.RemoveCurlyBrackets(parameters[1]).Substring(0, 1) == "1";
                key = Common.RemoveCurlyBrackets(parameters[1]).Substring(1).Trim();

                // OSKeyPress{ThirtyTwoMilliSec,VK_A}
                // OSKeyPress{ThirtyTwoMilliSec,VK_A}
                OSCommandObject = new NonVisuals.OSCommand();
                OSCommandObject.ImportString(parameters[2]);
            }
            else
            {
                // SwitchPanelOSPZ55{1KNOB_ENGINE_LEFT}
                var param = Common.RemoveCurlyBrackets(parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture))).Trim();

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = Common.RemoveCurlyBrackets(param).Substring(0, 1) == "1";
                key = Common.RemoveCurlyBrackets(param).Substring(1).Trim();

                // OSKeyPress{HalfSecond,VK_I}    
                OSCommandObject = new NonVisuals.OSCommand();
                OSCommandObject.ImportString(parameters[1]);
            }

            return Tuple.Create(mode, key);
        }

        public string GetExportString(string header, string mode, string keyName)
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }

            var onStr = WhenTurnedOn ? "1" : "0";

            if (!string.IsNullOrEmpty(mode))
            {
                //Multipanel/Radio has one additional setting
                // MultiPanelOSPZ70{ALT}\o/{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}
                return header + "{" + mode + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + keyName + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSCommandObject.ExportString();
            }

            // RadioPanelOSPZ69{1UpperCOM1}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}
            return header + "{" + onStr + keyName + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSCommandObject.ExportString();
        }

        public int GetHash()
        {
            unchecked
            {
                var result = _whenOnTurnedOn.GetHashCode();
                result = result * 397 ^ (_operatingSystemCommand?.GetHash() ?? 0);
                return result;
            }
        }

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public NonVisuals.OSCommand OSCommandObject
        {
            get => _operatingSystemCommand;
            set => _operatingSystemCommand = value;
        }
    }
}

