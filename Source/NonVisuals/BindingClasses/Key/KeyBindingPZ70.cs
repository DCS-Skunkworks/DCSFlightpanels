using System;
using System.Collections.Generic;
using MEF;
using NonVisuals.Panels.Saitek.Panels;

namespace NonVisuals.BindingClasses.Key
{
    [Serializable]
    public class KeyBindingPZ70 : KeyBindingBase
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
                // MultiPanelKnob{ALT}\o/{1LCD_WHEEL_DEC}\o/OSKeyPress{ThirtyTwoMilliSec,VK_A}

                var result = ParseSettingV1(settings);

                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), result.Item1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), result.Item2);
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

            return GetExportString("MultiPanelKnob", Enum.GetName(typeof(PZ70DialPosition), DialPosition), Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob));
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

        public static void SetNegators(ref HashSet<KeyBindingPZ70> knobBindings)
        {
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

                if (keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT
                 || keyBindingPZ70.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS
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

                    /*
                     * This is actually broken, the dial on PZ70, you should be able to dial in two directions from IAS and get negation.
                     */
                    negatorKnob = keyBindingPZ70.MultiPanelPZ70Knob switch
                    {
                        MultiPanelPZ70Knobs.KNOB_ALT => MultiPanelPZ70Knobs.KNOB_VS,
                        MultiPanelPZ70Knobs.KNOB_VS => MultiPanelPZ70Knobs.KNOB_ALT,
                        MultiPanelPZ70Knobs.KNOB_IAS => MultiPanelPZ70Knobs.KNOB_HDG,
                        MultiPanelPZ70Knobs.KNOB_HDG => MultiPanelPZ70Knobs.KNOB_CRS,
                        MultiPanelPZ70Knobs.KNOB_CRS => MultiPanelPZ70Knobs.KNOB_HDG,
                        MultiPanelPZ70Knobs.LCD_WHEEL_INC => MultiPanelPZ70Knobs.LCD_WHEEL_DEC,
                        MultiPanelPZ70Knobs.LCD_WHEEL_DEC => MultiPanelPZ70Knobs.LCD_WHEEL_INC,
                        MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP => MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN,
                        MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN => MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP,
                        _ => throw new Exception($"Unexpected MultiPanelPZ70Knob value for negator [{keyBindingPZ70.MultiPanelPZ70Knob}]")
                    };

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
        }
    }
}
