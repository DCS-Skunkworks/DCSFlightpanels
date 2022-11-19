using NonVisuals.Panels.StreamDeck.Events;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckButtonActionListener
    {
        void ActionTypeChangedEvent(object sender, ActionTypeChangedEventArgs e);
    }
}
