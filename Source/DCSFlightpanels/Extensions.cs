namespace DCSFlightpanels
{
    using CustomControls;
    using NonVisuals.Panels.StreamDeck;
    using NonVisuals.Panels.StreamDeck.Panels;

    public static class Extensions
    {

        public static void TestImage(this StreamDeckFaceTextBox textBox, StreamDeckPanel streamDeckPanel)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY, textBox.Bill.BackgroundColor);
            streamDeckPanel.SetImage(streamDeckPanel.SelectedButtonName,bitmap);
        }
    }
}
