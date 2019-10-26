using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class LayerBindingStreamDeck : IStreamDeckButtonAction
    {

        private StreamDeckTargetLayer _streamDeckLayerTarget;
        private bool _whenTurnedOn;
        
        public int ExecutionDelay { get; set; } = 0;
        private Thread _delayedExecutionThread;

        public EnumStreamDeckButtonActionType ActionType => EnumStreamDeckButtonActionType.LayerNavigation;













        ~LayerBindingStreamDeck()
        {
            _delayedExecutionThread?.Abort();
        }

        public void Execute()
        {
            if (ExecutionDelay == 0)
            {
                //todo
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
                //todo
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox(e);
            }
        }
        
        public bool WhenTurnedOn
        {
            get => _whenTurnedOn;
            set => _whenTurnedOn = value;
        }

        public StreamDeckTargetLayer StreamDeckLayerTarget
        {
            get => _streamDeckLayerTarget;
            set => _streamDeckLayerTarget = value;
        }
    }
}
