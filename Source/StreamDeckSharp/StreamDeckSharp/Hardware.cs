using StreamDeckSharp.Internals;

namespace StreamDeckSharp
{
    /// <summary>
    /// Details about different StreamDeck Hardware
    /// </summary>
    public static class Hardware
    {
        /// <summary>
        /// Details about the classic Stream Deck
        /// </summary>
        public static IUsbHidHardware StreamDeck
            => InternalStreamDeck;

        /// <summary>
        /// Details about the updated Stream Deck MK.2
        /// </summary>
        public static IUsbHidHardware StreamDeckMK2
            => InternalStreamDeckMK2;

        /// <summary>
        /// Details about the classic Stream Deck Rev 2
        /// </summary>
        public static IUsbHidHardware StreamDeckRev2
            => InternalStreamDeckRev2;

        /// <summary>
        /// Details about the Stream Deck XL
        /// </summary>
        public static IUsbHidHardware StreamDeckXL
            => InternalStreamDeckXL;

        /// <summary>
        /// Details about the Stream Deck XL Rev2
        /// </summary>
        public static IUsbHidHardware StreamDeckXLRev2
            => InternalStreamDeckXLRev2;

        /// <summary>
        /// Details about the Stream Deck Mini
        /// </summary>
        public static IUsbHidHardware StreamDeckMini
            => InternalStreamDeckMini;

        /// <summary>
        /// Details about the Stream Deck Mini Rev2
        /// </summary>
        public static IUsbHidHardware SteamDeckMiniRev2
            => InternalStreamDeckMiniRev2;

        /// <summary>
        /// Details about the Stream Deck Plus
        /// </summary>
        public static IUsbHidHardware StreamDeckPlus
            => InternalStreamDeckPlus;

        internal static IHardwareInternalInfos InternalStreamDeck { get; }
            = new StreamDeckHardwareInfo();

        internal static IHardwareInternalInfos InternalStreamDeckRev2 { get; }
            = new StreamDeckRev2HardwareInfo();

        internal static IHardwareInternalInfos InternalStreamDeckMK2 { get; }
            = new StreamDeckMK2HardwareInfo();

        internal static IHardwareInternalInfos InternalStreamDeckXL { get; }
            = new StreamDeckXlHardwareInfo();

        internal static IHardwareInternalInfos InternalStreamDeckXLRev2 { get; }
            = new StreamDeckXlRev2HardwareInfo();

        internal static IHardwareInternalInfos InternalStreamDeckMini { get; }
            = new StreamDeckMiniHardwareInfo(UsbConstants.ProductIds.STREAM_DECK_MINI, "Stream Deck Mini");

        internal static IHardwareInternalInfos InternalStreamDeckMiniRev2 { get; }
            = new StreamDeckMiniHardwareInfo(UsbConstants.ProductIds.STREAM_DECK_MINI_REV2, "Stream Deck Mini Rev2");

        internal static IHardwareInternalInfos InternalStreamDeckPlus { get; }
            = new StreamDeckPlusHardwareInfo();
    }
}
