namespace NonVisuals.Interfaces
{
    using MEF;

    using NonVisuals.StreamDeck;

    public interface IStreamDeckButtonTypeBase
    {
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        StreamDeckPanel StreamDeckPanelInstance { get; set; }
    }
}
