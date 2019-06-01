using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Documents;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class KeyBindingPZ55
    {
        /*
         This class binds a physical switch on the PZ55 with a user made virtual keypress in Windows.
         */
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;
        private OSKeyPress _osKeyPress;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("SwitchPanelKey{"))
            {
                //SwitchPanelKey{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //SwitchPanelKey{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Trim().Substring(15);
                //1KNOB_ENGINE_LEFT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1KNOB_ENGINE_LEFT
                _whenOnTurnedOn = (param0.Substring(0, 1) == "1");
                param0 = param0.Substring(1);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), param0);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                _osKeyPress = new OSKeyPress();
                _osKeyPress.ImportString(parameters[1]);
            }
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }

        public OSKeyPress OSKeyPress
        {
            get => _osKeyPress;
            set => _osKeyPress = value;
        }


        public string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            return "SwitchPanelKey{" + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}" + SeparatorChars + _osKeyPress.ExportString();
        }

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public static HashSet<KeyBindingPZ55> SetNegators(HashSet<KeyBindingPZ55> knobBindings)
        {
            if (knobBindings == null)
            {
                return knobBindings;
            }
            foreach (var keyBindingPZ55 in knobBindings)
            {
                //Clear all negators
                keyBindingPZ55.OSKeyPress.NegatorOSKeyPresses.Clear();

                if (keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF)
                {
                    //We have to deal with them separately
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
                if (keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START ||
                    keyBindingPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF)
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
                    }
                }
            }
            return knobBindings;
        }
    }
}
