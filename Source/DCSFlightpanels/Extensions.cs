using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels
{
    using DCSFlightpanels.CustomControls;

    using NonVisuals.StreamDeck;

    public static class Extensions
    {

        public static void TestImage(this StreamDeckFaceTextBox textBox, StreamDeckPanel streamDeckPanel)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY, textBox.Bill.BackgroundColor);
            streamDeckPanel.SetImage(streamDeckPanel.SelectedButtonName,bitmap);
        }
    }
}
