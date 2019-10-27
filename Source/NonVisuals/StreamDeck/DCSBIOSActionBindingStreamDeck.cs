using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class DCSBIOSActionBindingStreamDeck : DCSBIOSActionBindingBase, IStreamDeckButtonAction
    {

        public bool UseExecutionDelay { get; set; } = false;
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.DCSBIOS;

        public int ExecutionDelay { get; set; } = 1000;
        private Thread _delayedExecutionThread;
















        ~DCSBIOSActionBindingStreamDeck()
        {
            CancelSendDCSBIOSCommands = true;
            DCSBIOSCommandsThread?.Abort();
            _delayedExecutionThread?.Abort();
        }

        public void Execute()
        {
            if (!UseExecutionDelay)
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
