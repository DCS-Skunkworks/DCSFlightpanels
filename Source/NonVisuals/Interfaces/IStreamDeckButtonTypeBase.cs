using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckButtonTypeBase
    {
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        string StreamDeckInstanceId { get; set; }
    }
}
