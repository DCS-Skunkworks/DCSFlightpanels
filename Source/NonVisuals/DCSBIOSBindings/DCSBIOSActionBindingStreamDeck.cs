using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Interfaces;

namespace NonVisuals.DCSBIOSBindings
{
    public class DCSBIOSActionBindingStreamDeck : DCSBIOSActionBindingBase, IStreamDeckButtonAction
    {
        /*
         This class binds a physical key on the Stream Deck with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        public EnumStreamDeckButtonActionType ActionType => EnumStreamDeckButtonActionType.DCSBIOS;

        public int ExecutionDelay { get; set; } = 0;
        private Thread _delayedExecutionThread;
















        ~DCSBIOSActionBindingStreamDeck()
        {
            CancelSendDCSBIOSCommands = true;
            DCSBIOSCommandsThread?.Abort();
            _delayedExecutionThread?.Abort();
        }

        public void Execute()
        {
            if (ExecutionDelay == 0)
            {
                SendDCSBIOSCommands();
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
                SendDCSBIOSCommands();
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox(e);
            }
        }
        

        internal override void ImportSettings(string settings){}

        public override string ExportSettings()
        {
            return null;
        }
    }
}
