namespace NonVisuals.Saitek.BindingClasses
{
    using System;
    using System.Collections.Generic;

    using MEF;

    [Serializable]
    public class KeyBindingPZ69 : KeyBindingBase
    {
        /*
         This class binds a physical switch on the PZ69 with a user made virtual keypress in Windows.
         */
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;
        

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("RadioPanelKey{"))
            {
                // RadioPanelKey{1KNOB_ENGINE_OFF}\o/OSKeyPress{HalfSecond,VK_I}
                // RadioPanelKey{0SWITCHKEY_CLOSE_COWL}\o/OSKeyPress{INFORMATION=^key press sequence^[ThirtyTwoMilliSec,VK_A,ThirtyTwoMilliSec][ThirtyTwoMilliSec,VK_B,ThirtyTwoMilliSec]}

                var result = ParseSettingV1(settings);
                
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), result.Item2);
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

            return GetExportString("RadioPanelKey", null, Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key));
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }

        public static HashSet<KeyBindingPZ69> SetNegators(HashSet<KeyBindingPZ69> knobBindings)
        {
            if (knobBindings == null)
            {
                return null;
            }

            foreach (var keyBindingPZ69 in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingPZ69.OSKeyPress == null)
                {
                    continue;
                }

                // Clear all negators
                keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Clear();

                if (keyBindingPZ69.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch || keyBindingPZ69.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
                {
                    foreach (var keyBinding in knobBindings)
                    {
                        if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == keyBindingPZ69.RadioPanelPZ69Key && keyBinding.WhenTurnedOn != keyBindingPZ69.WhenTurnedOn)
                        {
                            keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                        }
                    }
                }
            }

            foreach (var keyBindingPZ69 in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingPZ69.OSKeyPress == null)
                {
                    continue;
                }

                if (keyBindingPZ69.RadioPanelPZ69Key != RadioPanelPZ69KnobsEmulator.UpperFreqSwitch &&
                    keyBindingPZ69.RadioPanelPZ69Key != RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
                {
                    switch (keyBindingPZ69.RadioPanelPZ69Key)
                    {
                        case RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperCOM1:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperCOM2:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperNAV1:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperNAV2:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperADF)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperADF:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperDME)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperDME:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperADF || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.UpperXPDR:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperDME)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerCOM1:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerCOM2:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerNAV1:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerNAV2:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerADF)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerADF:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerDME)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerDME:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerADF || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                                    }
                                }

                                break;
                            }

                        case RadioPanelPZ69KnobsEmulator.LowerXPDR:
                            {
                                foreach (var keyBinding in knobBindings)
                                {
                                    if (keyBinding != keyBindingPZ69 && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerDME)
                                    {
                                        keyBindingPZ69.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
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
