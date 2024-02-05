using StreamDeckSharp.Internals;
using System.Collections.Generic;

namespace StreamDeckSharp.Tests
{
    internal static class HardwareInfoResolver
    {
        internal static IEnumerable<IHardwareInternalInfos> GetAllHardwareInfos()
        {
            yield return Hardware.InternalStreamDeck;
            yield return Hardware.InternalStreamDeckMini;
            yield return Hardware.InternalStreamDeckMK2;
            yield return Hardware.InternalStreamDeckRev2;
            yield return Hardware.InternalStreamDeckXL;
            yield return Hardware.InternalStreamDeckMiniRev2;
        }
    }
}
