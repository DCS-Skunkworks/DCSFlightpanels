namespace NonVisuals.Saitek.BindingClasses
{
    using System;

    using Newtonsoft.Json;

    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.Properties;
    using NonVisuals.Saitek.Switches;


    [Serializable]
    public abstract class KeyBindingBase
    {
        /*
         This is the base class for all the key binding classes
         that binds a physical switch to a user made virtual 
         keypress in Windows or other functionality.
         */
        private KeyPress _keyPress;
        private bool _whenOnTurnedOn = true;

        public int GetHash()
        {
            unchecked
            {
                var result = _keyPress?.GetHash() ?? 0;
                return result;
            }
        }

        internal abstract void ImportSettings(string settings);

        [JsonProperty("OSKeyPress", Required = Required.Always)]
        public KeyPress OSKeyPress
        {
            get => _keyPress;
            set => _keyPress = value;
        }

        public abstract string ExportSettings();

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public Tuple<string, string> ParseSettingV1(string config)
        {
            var mode = "";
            var key = "";

            if (string.IsNullOrEmpty(config))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            // RadioPanelKeyDialPos{LowerCOM1}\o/{0LowerFreqSwitch}\o/OSKeyPress{ThirtyTwoMilliSec,VK_A}
            // MultiPanelKnob{ALT}\o/{1LCD_WHEEL_DEC}\o/OSKeyPress{ThirtyTwoMilliSec,VK_A}
            // FarmingPanelKey{1KNOB_ENGINE_OFF}\o/OSKeyPress{HalfSecond,VK_I}
            // FarmingPanelKey{0SWITCHKEY_CLOSE_COWL}\o/OSKeyPress{INFORMATION=^key press sequence^[ThirtyTwoMilliSec,VK_A,ThirtyTwoMilliSec][ThirtyTwoMilliSec,VK_B,ThirtyTwoMilliSec]}
            var parameters = config.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

            if (config.Contains("MultiPanel") || config.Contains("RadioPanel")) // Has additional setting which tells which position leftmost dial is in
            {
                // RadioPanelKeyDialPos{LowerCOM1}
                // MultiPanelKnob{ALT}
                mode = Common.RemoveCurlyBrackets(parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture))).Trim();

                // {0LowerFreqSwitch}
                // {1LCD_WHEEL_DEC}
                WhenTurnedOn = Common.RemoveCurlyBrackets(parameters[1]).Substring(0, 1) == "1";
                key = Common.RemoveCurlyBrackets(parameters[1]).Substring(1).Trim();

                // OSKeyPress{ThirtyTwoMilliSec,VK_A}
                // OSKeyPress{ThirtyTwoMilliSec,VK_A}
                OSKeyPress = new KeyPress();
                OSKeyPress.ImportString(parameters[2]);
            }
            else
            {
                // FarmingPanelKey{1KNOB_ENGINE_OFF}
                var param = Common.RemoveCurlyBrackets(parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture))).Trim();

                // 1KNOB_ENGINE_OFF
                WhenTurnedOn = Common.RemoveCurlyBrackets(param).Substring(0, 1) == "1";
                key = Common.RemoveCurlyBrackets(param).Substring(1).Trim();

                // OSKeyPress{HalfSecond,VK_I}    
                OSKeyPress = new KeyPress();
                OSKeyPress.ImportString(parameters[1]);
            }

            return Tuple.Create(mode, key);
        }

        public string GetExportString(string header, string mode, string keyName)
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }

            var onStr = WhenTurnedOn ? "1" : "0";

            if (!string.IsNullOrEmpty(mode))
            {
                //Multipanel/Radio has one additional setting
                // RadioPanelKeyDialPos{LowerCOM1}\o/{0LowerFreqSwitch}\o/OSKeyPress{ThirtyTwoMilliSec,VK_A}
                return header + "{" + mode + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + keyName + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSKeyPress.ExportString();
            }

            // FarmingPanelKey{1KNOB_ENGINE_OFF}\o/OSKeyPress{HalfSecond,VK_I}
            return header + "{" + onStr + keyName + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSKeyPress.ExportString();
        }
    }
}
