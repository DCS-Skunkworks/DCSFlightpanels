using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.Interfaces
{
    public interface IStreamDeckUI : IStreamDeckListener
    {
        StreamDeckButton PastedStreamDeckButton { get; set; }
    }
}
