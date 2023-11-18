using OpenMacroBoard.SDK;
using static StreamDeckSharp.UsbConstants;

namespace StreamDeckSharp.Internals
{
    internal sealed class StreamDeckXlHardwareInfo
        : StreamDeckJpgHardwareBase
    {
        public StreamDeckXlHardwareInfo()
            : base(new GridKeyLayout(8, 4, 96, 38), true)
        {
        }

        public override string DeviceName => "Stream Deck XL";
        public override int UsbProductId => ProductIds.STREAM_DECK_XL;
    }
}
