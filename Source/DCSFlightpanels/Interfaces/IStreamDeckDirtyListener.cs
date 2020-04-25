using NonVisuals.StreamDeck.Events;

namespace DCSFlightpanels.Interfaces
{
    public interface IStreamDeckDirtyListener
    {
        void IsDirtyControl(object sender, StreamDeckUIControlDirtyChangeArgs e);
    }
}
