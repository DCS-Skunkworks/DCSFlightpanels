using System;
using System.Collections.Generic;

namespace NonVisuals.Saitek
{
    [Serializable]
    public class KeyBindingPZ70 : KeyBinding
    {
        /*
         This class binds a physical switch on the PZ70 with a user made virtual keypress in Windows.
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("MultiPanelKnob{"))
            {
                //MultiPanelKey{ALT}\o/{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelKey{ALT}
                var param0 = parameters[0].Replace("MultiPanelKnob{", "").Replace("}", "");
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                //{1KNOB_ENGINE_LEFT}
                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                //1KNOB_ENGINE_LEFT
                WhenTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param1);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                OSKeyPress = new KeyPress();
                OSKeyPress.ImportString(parameters[2]);
            }
        }

        public PZ70DialPosition DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get => _multiPanelPZ70Knob;
            set => _multiPanelPZ70Knob = value;
        }

        public override string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            var onStr = WhenTurnedOn ? "1" : "0";
            return "MultiPanelKnob{" + _pz70DialPosition + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSKeyPress.ExportString();
        }

        public static HashSet<KeyBindingPZ70> SetNegators(HashSet<KeyBindingPZ70> knobBindings)
        {
            if (knobBindings == null)
            {
                return knobBindings;
            }
            foreach (var keyBindingPZ70 in knobBindings)
            {
                //Clear all negators
                keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Clear();

                if (keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_IAS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_HDG ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_CRS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN
                )
                {
                    //We have to deal with them separately
                    continue;
                }

                foreach (var keyBinding in knobBindings)
                {
                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == keyBindingPZ70.MultiPanelPZ70Knob && keyBinding.WhenTurnedOn != keyBindingPZ70.WhenTurnedOn)
                    {
                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }

            foreach (var keyBindingPZ70 in knobBindings)
            {
                if (keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_IAS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_HDG ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_CRS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN
                )
                    switch (keyBindingPZ70.MultiPanelPZ70Knob)
                    {
                        case MultiPanelPZ70Knobs.KNOB_ALT:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_HDG)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_IAS || keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS || keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_HDG)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_IAS || keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_CRS)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.LCD_WHEEL_INC:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.LCD_WHEEL_DEC:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.FLAPS_LEVER_UP:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP)
                                    {
                                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }
                    }

            }
            return knobBindings;
        }
    }
}
