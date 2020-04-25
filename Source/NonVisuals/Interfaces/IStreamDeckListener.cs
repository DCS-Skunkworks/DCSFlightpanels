using NonVisuals.StreamDeck.Events;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckListener
    {
        void LayerSwitched(object sender, StreamDeckLayerSwitchArgs e);
        void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangeArgs e);
        void SelectedButtonChangePreview(object sender, StreamDeckSelectedButtonChangePreviewArgs e);
    }
}
