using System;

namespace NonVisuals.StreamDeck.Events
{
    
    public class StreamDeckLayerChange : EventArgs
    {
        public string ActiveLayerName { get; set; }
    }
}
