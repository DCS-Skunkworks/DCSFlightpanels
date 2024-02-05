namespace DCSFlightpanels
{
    using CustomControls;
    using NonVisuals.Panels.StreamDeck;
    using NonVisuals.Panels.StreamDeck.Panels;

    public static class Extensions
    {
        public static void TestImage(this StreamDeckFaceTextBox textBox, StreamDeckPanel streamDeckPanel)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.TextFont, textBox.FontColor, textBox.OffsetX, textBox.OffsetY, textBox.BackgroundColor);
            streamDeckPanel.SetImage(streamDeckPanel.SelectedButtonName,bitmap);
        }
    }
}
