using DCSFlightpanels.CustomControls;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public static class Extensions
    {

        public static void TestImage(this StreamDeckFaceTextBox textBox, StreamDeckPanel streamDeckPanel)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.BackgroundColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY);
            streamDeckPanel.SetImage(streamDeckPanel.SelectedButtonName,bitmap);
        }
    }
}
