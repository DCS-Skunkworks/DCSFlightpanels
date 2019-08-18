using System;
using System.Collections.Generic;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class KeyBindingStreamDeck : KeyBinding
    {
        /*
         This class binds a physical button on a Stream Deck with a user made virtual keypress in Windows.
         */
        private StreamDeck35Buttons _streamDeckButton;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("StreamDeckButton{"))
            {
                //StreamDeckButton{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //StreamDeckButton{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("StreamDeckButton{", "").Replace("}", "");

                //1KNOB_ENGINE_LEFT
                WhenTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _streamDeckButton = (StreamDeck35Buttons)Enum.Parse(typeof(StreamDeck35Buttons), param0);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                OSKeyPress = new OSKeyPress();
                OSKeyPress.ImportString(parameters[1]);
            }
        }
        
        public StreamDeck35Buttons StreamDeckButton
        {
            get => _streamDeckButton;
            set => _streamDeckButton = value;
        }

        public override string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(StreamDeck35Buttons), StreamDeckButton) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            return "StreamDeckButton{" + onStr + Enum.GetName(typeof(StreamDeck35Buttons), StreamDeckButton) + "}" + SeparatorChars + OSKeyPress.ExportString();
        }

        public static HashSet<KeyBindingStreamDeck> SetNegators(HashSet<KeyBindingStreamDeck> keyBindings)
        {
            if (keyBindings == null)
            {
                return keyBindings;
            }
            foreach (var keyBindingStreamDeck in keyBindings)
            {
                //Clear all negators
                keyBindingStreamDeck.OSKeyPress.NegatorOSKeyPresses.Clear();

                foreach (var keyBinding in keyBindings)
                {
                    if (keyBinding != keyBindingStreamDeck && keyBinding.StreamDeckButton == keyBindingStreamDeck.StreamDeckButton && keyBinding.WhenTurnedOn != keyBindingStreamDeck.WhenTurnedOn)
                    {
                        keyBindingStreamDeck.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }

            return keyBindings;
        }
    }
}
