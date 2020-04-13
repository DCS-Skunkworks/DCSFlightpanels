using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckButtonTypeBase
    {
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        StreamDeckPanel StreamDeck { get; set; }
        StreamDeckButton StreamDeckButton { get; set; }
    }
}
