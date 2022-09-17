using MEF;
using NonVisuals.StreamDeck;
using System;
using System.Windows.Media.Imaging;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    internal static class Resources
    {
        public static BitmapImage GetSelectedImageNamed(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return new BitmapImage(new Uri(StreamDeckConstants.NUMBER_BUTTON_LOCATION + StreamDeckCommon.ButtonNumber(streamDeckButtonName) + "_green.png", UriKind.Absolute));
        }

        public static BitmapImage GetDeselectedImageNamed(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return new BitmapImage(new Uri(StreamDeckConstants.NUMBER_BUTTON_LOCATION + StreamDeckCommon.ButtonNumber(streamDeckButtonName) + "_blue.png", UriKind.Absolute));
        }

    }
}
