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
        /*
         This class binds a physical button on the Stream Deck with a layer.
         Pressing this button shows the _streamDeckLayer on the Stream Deck.
         */
        private StreamDeckButtonNames _streamDeckButtonName;

        private StreamDeckTargetLayer _streamDeckLayerTarget;
        private List<StreamDeckButton> _streamDeckButtons = new List<StreamDeckButton>();
        private bool _whenTurnedOn;






        public EnumStreamDeckButtonActionType GetActionType()
        {
            return EnumStreamDeckButtonActionType.OSCommand;
        }

        public void Execute()
        {
        
        }

        public bool WhenTurnedOn
        {
            get => _whenTurnedOn;
            set => _whenTurnedOn = value;
        }

        public StreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public StreamDeckTargetLayer StreamDeckLayerTarget
        {
            get => _streamDeckLayerTarget;
            set => _streamDeckLayerTarget = value;
        }
    }
}
