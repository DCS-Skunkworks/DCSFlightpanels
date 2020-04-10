using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public static class FaceFactory
    {
        public static IStreamDeckButtonFace HomButtonFace(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            var result = new FaceTypeText();

            result.StreamDeckButtonName = streamDeckButtonName;

            result.Text = "Home";
            result.FontColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            result.BackgroundColor = System.Drawing.ColorTranslator.FromHtml("#FFDFD991");

            //var colorConverter = new ColorConverter();
            //result.BackgroundColor = (Color)colorConverter.ConvertFromString("#FFDFD991");

            return result;
        }
    }
}
