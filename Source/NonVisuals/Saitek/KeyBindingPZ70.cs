using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Threading;
using Microsoft.VisualBasic.Logging;

namespace NonVisuals.Saitek
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Saitek.Panels;

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
                // MultiPanelKey{ALT}\o/{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // MultiPanelKey{ALT}
                var param0 = parameters[0].Replace("MultiPanelKnob{", string.Empty).Replace("}", string.Empty);
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                // {1KNOB_ENGINE_LEFT}
                var param1 = parameters[1].Replace("{", string.Empty).Replace("}", string.Empty);

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param1);

                // OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
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

        private static long _checker = 0;
        public static void SetNegators(ref HashSet<KeyBindingPZ70> knobBindings)
        {
            Interlocked.Increment(ref _checker);

            if (knobBindings == null)
            {
                return;
            }

            foreach (var keyBindingPZ70 in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingPZ70.OSKeyPress == null)
                {
                    continue;
                }

                // Clear all negators
                keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Clear();

                if (keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_IAS
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_HDG
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_CRS
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP
                                                                                      || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN)
                {
                    // We have to deal with them separately
                    continue;
                }

                foreach (var keyBinding in knobBindings)
                {
                    if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == keyBindingPZ70.MultiPanelPZ70Knob && keyBinding.WhenTurnedOn != keyBindingPZ70.WhenTurnedOn)
                    {
                        /*if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP ||
                            keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN)
                        {
                            Debug.WriteLine("Adding " + keyBinding.MultiPanelPZ70Knob + "(" + keyBinding.WhenTurnedOn + ") as negator to " +
                                            keyBindingPZ70.MultiPanelPZ70Knob + "(" + keyBindingPZ70.WhenTurnedOn + ")");
                        }
                        if (keyBinding.OSKeyPress.KeyPressSequence.Count == 0)
                        {
                            Debugger.Break();
                        }*/
                        keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }

            foreach (var keyBindingPZ70 in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingPZ70.OSKeyPress == null)
                {
                    continue;
                }

                if (keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_IAS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_HDG ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_CRS ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP ||
                    keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN
                   )
                {
                    MultiPanelPZ70Knobs negatorKnob = MultiPanelPZ70Knobs.KNOB_ALT;

                    switch (keyBindingPZ70.MultiPanelPZ70Knob)
                    {
                        /*
                         * This is actually broken, the dial on PZ70, you should be able to dial in two directions from IAS and get negation.
                         */
                        case MultiPanelPZ70Knobs.KNOB_ALT:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.KNOB_VS;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.KNOB_HDG;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.KNOB_ALT;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.KNOB_HDG;
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.KNOB_CRS;
                                break;
                            }

                        case MultiPanelPZ70Knobs.LCD_WHEEL_INC:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.LCD_WHEEL_DEC;
                                break;
                            }

                        case MultiPanelPZ70Knobs.LCD_WHEEL_DEC:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.LCD_WHEEL_INC;
                                break;
                            }
                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN;
                                break;
                            }

                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN:
                            {
                                negatorKnob = MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP;
                                break;
                            }
                    }

                    foreach (var keyBinding in knobBindings)
                    {
                        if (keyBinding != keyBindingPZ70 && keyBinding.MultiPanelPZ70Knob == negatorKnob)
                        {
                            /*if (keyBinding.OSKeyPress.KeyPressSequence.Count == 0)
                            {
                                Debugger.Break();
                            }*/
                            keyBindingPZ70.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                        }
                    }
                }
            }
            
            Interlocked.Decrement(ref _checker);
        }
    }
}
