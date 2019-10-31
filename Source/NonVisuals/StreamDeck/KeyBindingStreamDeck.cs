using System.Collections.Generic;
using System.Threading;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.StreamDeck
{
    public class KeyBindingStreamDeck : KeyBinding, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.KeyPress;







        public void Execute(StreamDeckRequisites streamDeckRequisite)
        {
            OSKeyPress.Execute(streamDeckRequisite.ThreadCancellationToken);
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
                    if (keyBinding != keyBindingStreamDeck && keyBinding.EnumStreamDeckButtonName == keyBindingStreamDeck.EnumStreamDeckButtonName && keyBinding.WhenTurnedOn != keyBindingStreamDeck.WhenTurnedOn)
                    {
                        keyBindingStreamDeck.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }*/
            return keyBindings;
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
    }
}
