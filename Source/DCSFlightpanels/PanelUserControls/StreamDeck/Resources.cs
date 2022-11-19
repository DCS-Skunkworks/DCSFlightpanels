using MEF;
using NonVisuals.Panels.StreamDeck;
using System;
using System.Windows.Media.Imaging;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    internal static class Resources
    {
        public static BitmapImage GetDefaultButtonImageNamed(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return new BitmapImage(new Uri(StreamDeckConstants.NUMBER_BUTTON_LOCATION + StreamDeckCommon.ButtonNumber(streamDeckButtonName) + "_blue.png", UriKind.Absolute));
        }
        public static BitmapImage GetButtonDcsBiosDecoderRule()
        {
            return new BitmapImage(new Uri(StreamDeckConstants.StreamDeckGalleryPathMisc + "DcsBiosDecoderRule.png", UriKind.Absolute));
        }
    }
}
