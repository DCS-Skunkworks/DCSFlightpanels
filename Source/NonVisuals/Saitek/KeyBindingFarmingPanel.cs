namespace NonVisuals.Saitek
{
    using System;
    using System.Collections.Generic;

    using MEF;

    [Serializable]
    public class KeyBindingFarmingPanel : KeyBinding
    {
        /*
         This class binds a physical switch on the Farming Heavy Side Panel with a user made virtual keypress in Windows.
         */
        private FarmingPanelMKKeys _farmingPanelKey;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("FarmingPanelKey{"))
            {
                // FarmingPanelKey{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // FarmingPanelKey{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Trim().Substring(16);

                // 1KNOB_ENGINE_LEFT}
                param0 = param0.Remove(param0.Length - 1, 1);

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = (param0.Substring(0, 1) == "1");
                param0 = param0.Substring(1);
                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), param0);

                // OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                OSKeyPress = new KeyPress();
                OSKeyPress.ImportString(parameters[1]);
            }
        }

        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
        }

        public override string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }

            var onStr = WhenTurnedOn ? "1" : "0";
            return "FarmingPanelKey{" + onStr + Enum.GetName(typeof(FarmingPanelMKKeys), FarmingPanelKey) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSKeyPress.ExportString();
        }

        public static HashSet<KeyBindingFarmingPanel> SetNegators(HashSet<KeyBindingFarmingPanel> knobBindings)
        {
            if (knobBindings == null)
            {
                return null;
            }

            foreach (var keyBindingFarmingPanel in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingFarmingPanel.OSKeyPress == null)
                {
                    continue;
                }

                if (keyBindingFarmingPanel.FarmingPanelKey == FarmingPanelMKKeys.BUTTON_26 ||
                    keyBindingFarmingPanel.FarmingPanelKey == FarmingPanelMKKeys.BUTTON_27)
                {
                    switch (keyBindingFarmingPanel.FarmingPanelKey)
                    {
                        case FarmingPanelMKKeys.BUTTON_26:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingFarmingPanel && keyBinding.FarmingPanelKey == FarmingPanelMKKeys.BUTTON_27)
                                    {
                                        keyBindingFarmingPanel.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case FarmingPanelMKKeys.BUTTON_27:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingFarmingPanel && keyBinding.FarmingPanelKey == FarmingPanelMKKeys.BUTTON_26)
                                    {
                                        keyBindingFarmingPanel.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }
                    }
                }
            }

            return knobBindings;
        }
    }
}
