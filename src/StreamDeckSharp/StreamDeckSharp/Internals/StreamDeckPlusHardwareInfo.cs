using OpenMacroBoard.SDK;
using static StreamDeckSharp.UsbConstants;

namespace StreamDeckSharp.Internals
{
    internal sealed class StreamDeckPlusHardwareInfo
        : StreamDeckJpgHardwareBase
    {
        public StreamDeckPlusHardwareInfo()
            : base(new GridKeyLayout(4, 2, 120, 100), false)
        {
        }

        public override string DeviceName => "Stream Deck Plus";
        public override int UsbProductId => ProductIds.STREAM_DECK_PLUS;
    }
}
