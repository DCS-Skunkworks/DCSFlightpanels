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
         Pressing this button shows this layer on the Stream Deck.
         */
        private StreamDeckButtons _streamDeckButton;

        private StreamDeckLayer _streamDeckLayer;
        private List<StreamDeckButton> _streamDeckButtons = new List<StreamDeckButton>();
        
        public EnumStreamDeckButtonActionType GetActionType()
        {
            return EnumStreamDeckButtonActionType.OSCommand;
        }

        public void Execute()
        {
        
        }
    }
}
