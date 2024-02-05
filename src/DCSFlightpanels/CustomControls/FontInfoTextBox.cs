namespace DCSFlightpanels.CustomControls
{
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using NonVisuals;

    using Color = System.Drawing.Color;

    public class FontInfoTextBox : TextBox
    {
        private Font _font = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;

        public FontInfoTextBox()
        {
            Height = 45;
            Width = 210;
            IsReadOnly = true;
            FontSize = 10;
            FontStyle = FontStyles.Italic;
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#f2f5f3");
        }

        public void UpdateFontInfo()
        {
            Text = "Font : " + TargetFont.Name + " " + TargetFont.Size + " " + (TargetFont.Bold ? "Bold" : "Regular");
            Text = Text + "\n" + "Font Color : " + TargetFontColor;
            Text = Text + "\n" + "Background Color : " + TargetBackgroundColor;
        }

        public Font TargetFont
        {
            get => _font;
            set {
                _font = value;
                UpdateFontInfo();
            }
        }

        public Color TargetFontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                UpdateFontInfo();
            }
        }
        
        public Color TargetBackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                UpdateFontInfo();
            }
        }
    }
}
