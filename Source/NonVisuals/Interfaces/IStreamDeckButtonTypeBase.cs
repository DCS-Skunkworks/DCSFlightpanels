namespace NonVisuals.Interfaces
{
    using MEF;
    using NonVisuals.Panels.StreamDeck.Panels;

    public interface IStreamDeckButtonTypeBase
    {
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        StreamDeckPanel StreamDeckPanelInstance { get; set; }
    }
}
