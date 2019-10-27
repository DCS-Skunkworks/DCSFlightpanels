using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.StreamDeck
{
    public class KeyBindingStreamDeck : KeyBinding, IStreamDeckButtonAction
    {
        public bool UseExecutionDelay { get; set; } = false;
        public int ExecutionDelay { get; set; } = 1000;
        private Thread _delayedExecutionThread;

        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.KeyPress;







        ~KeyBindingStreamDeck()
        {
            _delayedExecutionThread?.Abort();
        }

        public void Execute()
        {
            if (!UseExecutionDelay)
            {
                OSKeyPress.Execute();
            }
            else
            {
                _delayedExecutionThread = new Thread(DelayedExecution);
                _delayedExecutionThread.Start();
            }
        }

        private void DelayedExecution()
        {
            try
            {
                Thread.Sleep(ExecutionDelay);
                OSKeyPress.Execute();
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox(e);
            }
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

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
    }
}
