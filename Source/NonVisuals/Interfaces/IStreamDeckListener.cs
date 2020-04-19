using NonVisuals.StreamDeck.Events;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckListener
    {
        void LayerChanged(object sender, StreamDeckLayerChange e);
    }
}
