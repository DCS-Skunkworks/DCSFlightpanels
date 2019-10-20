using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class LayerBindingStreamDeck : IStreamDeckButtonAction
    {

        private StreamDeckTargetLayer _streamDeckLayerTarget;
        private bool _whenTurnedOn;




        public EnumStreamDeckButtonActionType ActionType => EnumStreamDeckButtonActionType.LayerNavigation;

        

        public void Execute()
        {
        
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
