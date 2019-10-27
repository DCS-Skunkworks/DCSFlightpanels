using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class LayerBindingStreamDeck : IStreamDeckButtonAction
    {

        public bool UseExecutionDelay { get; set; } = false;
        private StreamDeckTargetLayer _streamDeckLayerTarget;
        private bool _whenTurnedOn;
        

        public int ExecutionDelay { get; set; } = 1000;
        private Thread _delayedExecutionThread;

        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.LayerNavigation;













        ~LayerBindingStreamDeck()
        {
            _delayedExecutionThread?.Abort();
        }

        public void Execute()
        {
            if (!UseExecutionDelay)
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
