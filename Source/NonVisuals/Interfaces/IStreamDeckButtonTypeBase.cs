using NonVisuals.StreamDeck.Panels;

namespace NonVisuals.Interfaces
{
    using MEF;

    public interface IStreamDeckButtonTypeBase
    {
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        StreamDeckPanel StreamDeckPanelInstance { get; set; }
    }
}
