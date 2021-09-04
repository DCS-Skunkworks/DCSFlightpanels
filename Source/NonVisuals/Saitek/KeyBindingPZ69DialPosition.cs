namespace NonVisuals.Saitek
{
    using System;
    using System.Collections.Generic;

    using MEF;

    [Serializable]
    public class KeyBindingPZ69DialPosition : KeyBinding
    {
        /*
         This class binds a physical switch on the PZ69 with a user made virtual keypress in Windows.
         This class differs that the binding is bound to what the dial position is in. So all
         dials positions COM1 -> XPDR have different bindings.
         */
        private PZ69DialPosition _pz69DialPosition;
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("RadioPanelKeyDialPos{"))
            {
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);
                var param0 = parameters[0].Replace("RadioPanelKeyDialPos{", string.Empty).Replace("}", string.Empty);
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);
                var param1 = parameters[1].Replace("{", string.Empty).Replace("}", string.Empty);
                WhenTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param1);
                OSKeyPress = new KeyPress();
                OSKeyPress.ImportString(parameters[2]);
            }
        }

        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }

        public override string ExportSettings()
        {
            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in KeyBindingPZ69DialPosition for knob " + RadioPanelPZ69Key + ". Cannot export.");
            }
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            var onStr = WhenTurnedOn ? "1" : "0";
            return "RadioPanelKeyDialPos{" + _pz69DialPosition + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSKeyPress.ExportString();
        }

        public static HashSet<KeyBindingPZ69DialPosition> SetNegators(HashSet<KeyBindingPZ69DialPosition> knobBindings)
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

                if (keyBindingPZ69.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch ||
                    keyBindingPZ69.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV1))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV2))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperADF))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperDME))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperADF || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperXPDR))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV1))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV2))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV1 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerADF))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV2 || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerDME))
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
                                    if (keyBinding != keyBindingPZ69 && (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerADF || keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerXPDR))
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
