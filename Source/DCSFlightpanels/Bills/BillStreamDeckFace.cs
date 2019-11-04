using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NonVisuals;
using NonVisuals.StreamDeck;
using Color = System.Drawing.Color;

namespace DCSFlightpanels.Bills
{
    public class BillStreamDeckFace
    {
        public EnumStreamDeckButtonNames StreamDeckButtonName;
        public StreamDeckButton Button;
        private Font _textFont = Constants.DefaultStreamDeckFont;
        private Color _fontColor = Color.Black;
        private Color _backgroundColor = ColorTranslator.FromHtml(Constants.COLOR_DEFAULT_WHITE);
        private bool _isSelected = false;
        public TextBox ParentTextBox { get; set; }
        public BitmapImage SelectedImage { get; set; }
        public BitmapImage DeselectedImage { get; set; }






        public bool ContainsTextFace()
        {
            return _textFont != null && !string.IsNullOrEmpty(ParentTextBox.Text); 
        }

        public Font TextFont
        {
            get => _textFont;
            set
            {
                _textFont = value;
                ParentTextBox.FontFamily = new System.Windows.Media.FontFamily(_textFont.Name);
                ParentTextBox.FontWeight = _textFont.Bold ? FontWeights.Bold : FontWeights.Regular;
                ParentTextBox.FontSize = _textFont.Size * 96.0 / 72.0;
                ParentTextBox.FontStyle = _textFont.Italic ? FontStyles.Italic : FontStyles.Normal;
                var textDecorationCollection = new TextDecorationCollection();
                if (_textFont.Underline) textDecorationCollection.Add(TextDecorations.Underline);
                if (_textFont.Strikeout) textDecorationCollection.Add(TextDecorations.Strikethrough);
                ParentTextBox.TextDecorations = textDecorationCollection;
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                ParentTextBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_fontColor.A, _fontColor.R, _fontColor.G, _fontColor.B)); ;
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                ParentTextBox.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_backgroundColor.A, _backgroundColor.R, _backgroundColor.G, _backgroundColor.B)); ;
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }


        public int OffsetX { get; set; } = 10;
        public int OffsetY { get; set; } = 24;
        

        public int ButtonNumber()
        {
            if (StreamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return 0;
            }
            
            return int.Parse(StreamDeckButtonName.ToString().Replace("BUTTON", ""));
            
        }

        public bool IsClean => OffsetX == 0 && OffsetY == 0 && BackgroundColor == ColorTranslator.FromHtml(Constants.COLOR_DEFAULT_WHITE) && FontColor == Color.Black && TextFont.Name == Constants.DEFAULT_FONT;

        public string BackgroundHex => "#" + _backgroundColor.R.ToString("X2") + _backgroundColor.G.ToString("X2") + _backgroundColor.B.ToString("X2");

        public void Clear()
        {
            Button = null;
            ParentTextBox?.Clear();
        }
    }

}
