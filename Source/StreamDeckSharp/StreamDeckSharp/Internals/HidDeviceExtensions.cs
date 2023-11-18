using HidSharp;
using static StreamDeckSharp.UsbConstants;

namespace StreamDeckSharp.Internals
{
    internal static class HidDeviceExtensions
    {
        public static IHardwareInternalInfos GetHardwareInformation(this HidDevice hid)
        {
            return GetDeviceDetails(hid.VendorID, hid.ProductID);
        }

        public static IHardwareInternalInfos GetDeviceDetails(int vendorId, int productId)
        {
            if (vendorId != VendorIds.ELGATO_SYSTEMS_GMBH)
            {
                return null;
            }

            return productId switch
            {
                ProductIds.STREAM_DECK => Hardware.InternalStreamDeck,
                ProductIds.STREAM_DECK_REV2 => Hardware.InternalStreamDeckRev2,
                ProductIds.STREAM_DECK_MK2 => Hardware.InternalStreamDeckMK2,
                ProductIds.STREAM_DECK_XL => Hardware.InternalStreamDeckXL,
                ProductIds.STREAM_DECK_XL_REV2 => Hardware.InternalStreamDeckXLRev2,
                ProductIds.STREAM_DECK_MINI => Hardware.InternalStreamDeckMini,
                ProductIds.STREAM_DECK_MINI_REV2 => Hardware.InternalStreamDeckMiniRev2,
                ProductIds.STREAM_DECK_PLUS => Hardware.InternalStreamDeckPlus,
                _ => null,
            };
        }
    }
}
