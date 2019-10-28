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
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.LayerNavigation;








        public void Execute(CancellationToken cancellationToken)
        {
            //todo
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
