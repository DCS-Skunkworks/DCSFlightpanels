using System;
using System.Collections.Generic;
using MEF;

namespace NonVisuals.BindingClasses.Key
{
    [Serializable]
    public class KeyBindingFarmingPanel : KeyBindingBase
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
                // FarmingPanelKey{1KNOB_ENGINE_OFF}\o/OSKeyPress{HalfSecond,VK_I}
                // FarmingPanelKey{0SWITCHKEY_CLOSE_COWL}\o/OSKeyPress{INFORMATION=^key press sequence^[ThirtyTwoMilliSec,VK_A,ThirtyTwoMilliSec][ThirtyTwoMilliSec,VK_B,ThirtyTwoMilliSec]}

                var result = ParseSettingV1(settings);

                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }

            return GetExportString("FarmingPanelKey", null, Enum.GetName(typeof(FarmingPanelMKKeys), FarmingPanelKey));
        }

        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
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
