using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;

namespace NonVisuals.StreamDeck
{
    public class KeyBindingStreamDeck : KeyBinding, IStreamDeckButtonAction
    {

        public EnumStreamDeckButtonActionType ActionType => EnumStreamDeckButtonActionType.KeyPress;

        public void Execute()
        {
            OSKeyPress.Execute();
        }

        internal override void ImportSettings(string settings)
        {
            /*if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("StreamDeckButton{"))
            {
                //StreamDeckButton{Home Layer|1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //StreamDeckButton{Home Layer|1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("StreamDeckButton{", "").Replace("}", "");

                //Home Layer|1KNOB_ENGINE_LEFT
                var param0Split = param0.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                Layer = param0Split[0];
                WhenTurnedOn = param0Split[1].Substring(0, 1) == "1";
                param0Split[1] = param0Split[1].Substring(1);
                _streamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), param0Split[1]);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                OSKeyPress = new KeyPress();
                OSKeyPress.ImportString(parameters[1]);
            }*/
        }
        
        public override string ExportSettings()
        {
            /*if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            Common.DebugP(Layer + "|" + Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            return "StreamDeckButton{"+ Layer + "|" + onStr + Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "}" + SeparatorChars + OSKeyPress.ExportString();
            */
            return "";
        }

        public static HashSet<KeyBindingStreamDeck> SetNegators(HashSet<KeyBindingStreamDeck> keyBindings)
        {
            /*if (keyBindings == null)
            {
                return null;
            }
            foreach (var keyBindingStreamDeck in keyBindings)
            {
                //Clear all negators
                keyBindingStreamDeck.OSKeyPress.NegatorOSKeyPresses.Clear();

                foreach (var keyBinding in keyBindings)
                {
                    if (keyBinding != keyBindingStreamDeck && keyBinding.StreamDeckButtonName == keyBindingStreamDeck.StreamDeckButtonName && keyBinding.WhenTurnedOn != keyBindingStreamDeck.WhenTurnedOn)
                    {
                        keyBindingStreamDeck.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }*/
            return keyBindings;
        }
    }
}
