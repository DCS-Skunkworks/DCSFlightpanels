using OpenMacroBoard.SDK;
using static StreamDeckSharp.UsbConstants;

namespace StreamDeckSharp.Internals
{
    internal sealed class StreamDeckXlRev2HardwareInfo
        : StreamDeckJpgHardwareBase
    {
        public StreamDeckXlRev2HardwareInfo()
            : base(new GridKeyLayout(8, 4, 96, 38), true)
        {
        }

        public override string DeviceName => "Stream Deck XL Rev2";
        public override int UsbProductId => ProductIds.STREAM_DECK_XL;
    }
}
