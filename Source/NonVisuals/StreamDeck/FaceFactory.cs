using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using NonVisuals.Interfaces;
using ColorConverter = System.Drawing.ColorConverter;

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
