using System;
using System.Collections.Generic;
using MEF;

namespace NonVisuals.BindingClasses.Key
{
    [Serializable]
    public class KeyBindingPZ55 : KeyBindingBase
    {
        /*
         This class binds a physical switch on the PZ55 with a user made virtual keypress in Windows.
         */
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("SwitchPanelKey{"))
            {
                // SwitchPanelKey{1KNOB_ENGINE_OFF}\o/OSKeyPress{HalfSecond,VK_I}
                // SwitchPanelKey{0SWITCHKEY_CLOSE_COWL}\o/OSKeyPress{INFORMATION=^key press sequence^[ThirtyTwoMilliSec,VK_A,ThirtyTwoMilliSec][ThirtyTwoMilliSec,VK_B,ThirtyTwoMilliSec]}

                var result = ParseSettingV1(settings);

                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), result.Item2);
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

            return GetExportString("SwitchPanelKey", null, Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key));
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }

        public static HashSet<KeyBindingPZ55> SetNegators(HashSet<KeyBindingPZ55> knobBindings)
        {
            if (knobBindings == null)
            {
                return null;
            }

            foreach (var keyBindingPZ55 in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingPZ55.OSKeyPress == null)
                {
                    continue;
                }

                // Clear all negators
                keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Clear();

                if (keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH || keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT
                                                                                              || keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT
                                                                                              || keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START
                                                                                              || keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF
                                                                                              || keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP
                                                                                              || keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN)
                {
                    // We have to deal with them separately
                    continue;
                }

                foreach (var keyBinding in knobBindings)
                {
                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == keyBindingPZ55.SwitchPanelPZ55Key && keyBinding.WhenTurnedOn != keyBindingPZ55.WhenTurnedOn)
                    {
                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }

            foreach (var keyBindingPZ55 in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingPZ55.OSKeyPress == null)
                {
                    continue;
                }

                if (keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN)
                {
                    switch (keyBindingPZ55.SwitchPanelPZ55Key)
                    {
                        case SwitchPanelPZ55Keys.KNOB_ENGINE_OFF:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case SwitchPanelPZ55Keys.KNOB_ENGINE_START:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT || keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT || keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT || keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case SwitchPanelPZ55Keys.LEVER_GEAR_UP:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case SwitchPanelPZ55Keys.LEVER_GEAR_DOWN:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ55 && keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP)
                                    {
                                        keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
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
