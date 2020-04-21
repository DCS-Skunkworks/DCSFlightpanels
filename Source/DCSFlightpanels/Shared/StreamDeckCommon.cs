using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Properties;
using NonVisuals.StreamDeck;
using Color = System.Drawing.Color;

namespace DCSFlightpanels.Shared
{
    public static class StreamDeckCommon
    {

        public static DialogResult SetFontStyle(StreamDeckFaceTextBox textBox)
        {
            var font = Settings.Default.ButtonTextFaceFont;
            var result = SetFontStyle(ref font);

            if (result == DialogResult.OK)
            {
                textBox.Bill.TextFont = font;
            }

            return result;
        }

        public static DialogResult SetFontStyle(ref Font font)
        {
            var fontDialog = new FontDialog();

            fontDialog.FixedPitchOnly = true;
            fontDialog.FontMustExist = true;
            fontDialog.MinSize = 6;

            if (Settings.Default.ButtonTextFaceFont != null)
            {
                fontDialog.Font = Settings.Default.ButtonTextFaceFont;
            }

            var result = fontDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                font = fontDialog.Font;
                Settings.Default.ButtonTextFaceFont = fontDialog.Font;
                Settings.Default.Save();
            }

            return result;
        }

        public static DialogResult SetFontColor(StreamDeckFaceTextBox textBox)
        {

            var color = Color.Transparent;
            var result = SetFontColor(ref color);

            if (result == DialogResult.OK)
            {
                textBox.Bill.FontColor = color;
            }

            return result;
        }

        public static DialogResult SetFontColor(ref Color color)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextFaceFontColor;
            colorDialog.CustomColors = StreamDeckConstants.GetOLEColors();

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog.Color;
                Settings.Default.ButtonTextFaceFontColor = colorDialog.Color;
                Settings.Default.Save();
            }

            return result;
        }
        
        public static DialogResult SetBackgroundColor(StreamDeckFaceTextBox textBox)
        {
            var color = Color.Transparent;
            var result = SetBackgroundColor(ref color);

            if (result == DialogResult.OK)
            {
                textBox.Bill.BackgroundColor =  color;
            }

            return result;
        }

        public static DialogResult SetBackgroundColor(ref Color color)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextFaceBackgroundColor;
            colorDialog.CustomColors = StreamDeckConstants.GetOLEColors();
            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog.Color;

                Settings.Default.ButtonTextFaceBackgroundColor = colorDialog.Color;
                Settings.Default.Save();
            }

            return result;
        }

        public static void TestImage(StreamDeckFaceTextBox textBox, IStreamDeckUIParent sdUIParent)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.BackgroundColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY);
            sdUIParent.TestImage(bitmap);
        }
    }
}
