using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class OSCommandBindingStreamDeck : OSCommandBinding, IStreamDeckButtonAction
    {
        public EnumStreamDeckButtonActionType ActionType => EnumStreamDeckButtonActionType.OSCommand;
        
        public int ExecutionDelay { get; set; } = 0;
        private Thread _delayedExecutionThread;











        ~OSCommandBindingStreamDeck()
        {
            _delayedExecutionThread?.Abort();
        }

        public void Execute()
        {
            if (ExecutionDelay == 0)
            {
                OSCommandObject.Execute();
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
                OSCommandObject.Execute();
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
