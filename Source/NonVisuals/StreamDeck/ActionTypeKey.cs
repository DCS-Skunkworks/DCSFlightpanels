using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class ActionTypeKey : KeyBinding, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.KeyPress;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _panelHash;




        public new int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                result = (result * 397) ^ base.GetHash();
                return result;
            }
        }


        public string Description { get => "Key press"; }


        public bool IsRunning()
        {
            return OSKeyPress.IsRunning();
        }

        public void Execute(CancellationToken threadCancellationToken)
        {
            OSKeyPress.Execute(threadCancellationToken);
        }


        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public static HashSet<ActionTypeKey> SetNegators(HashSet<ActionTypeKey> keyBindings)
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

        [JsonIgnore]
        public string PanelHash
        {
            get => _panelHash;
            set => _panelHash = value;
        }
    }
}
