using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.Bills
{
    using System.Drawing;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using MEF;

    using NonVisuals;
    using NonVisuals.StreamDeck;

    using Brushes = System.Windows.Media.Brushes;
    using Color = System.Drawing.Color;

    public class BillStreamDeckFace : BillBaseOutput
    {
        public EnumStreamDeckButtonNames StreamDeckButtonName;
        private ActionTypeLayer _streamDeckTargetLayer;
        private BIPLinkStreamDeck _bipLinkStreamDeck;
        public StreamDeckButton Button;
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor; 
        private DCSBIOSDecoder _dcsbiosDecoder;
        private string _imageFileRelativePath;
        
        public string BackgroundHex 
        { 
            get { return $"#{_backgroundColor.R:X2}{_backgroundColor.G:X2}{_backgroundColor.B:X2}"; } 
        }

        public BitmapImage SelectedImage { get; set; }
        public BitmapImage DeselectedImage { get; set; }
        public StreamDeckPanel StreamDeckPanelInstance { get; set; }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                TextBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_fontColor.A, _fontColor.R, _fontColor.G, _fontColor.B)); ;
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                TextBox.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_backgroundColor.A, _backgroundColor.R, _backgroundColor.G, _backgroundColor.B)); ;
            }
        }

        public int OffsetX { get; set; } = SettingsManager.OffsetX;

        public int OffsetY { get; set; } = SettingsManager.OffsetX;

        public Font TextFont
        {
            get => _textFont;
            set
            {
                _textFont = value;
                if (_textFont == null)
                {
                    return;
                }
                TextBox.FontFamily = new System.Windows.Media.FontFamily(_textFont.Name);
                TextBox.FontWeight = _textFont.Bold ? FontWeights.Bold : FontWeights.Regular;
                TextBox.FontSize = _textFont.Size * 96.0 / 72.0;
                TextBox.FontStyle = _textFont.Italic ? FontStyles.Italic : FontStyles.Normal;
                var textDecorationCollection = new TextDecorationCollection();
                if (_textFont.Underline) textDecorationCollection.Add(TextDecorations.Underline);
                if (_textFont.Strikeout) textDecorationCollection.Add(TextDecorations.Strikethrough);
                TextBox.TextDecorations = textDecorationCollection;
            }
        }

        public DCSBIOSDecoder DCSBIOSDecoder
        {
            get => _dcsbiosDecoder;
            set
            {
                _dcsbiosDecoder = value;
                if (_dcsbiosDecoder == null)
                {
                    TextBox.Text = string.Empty;
                    return;
                }
                if (!_dcsbiosDecoder.UseFormula && _dcsbiosDecoder.DCSBIOSOutput != null)
                {
                    TextBox.Text = _dcsbiosDecoder.DCSBIOSOutput.ControlId;
                }
                else if (_dcsbiosDecoder.UseFormula)
                {
                    TextBox.Text = "formula";
                }
            }
        }

        public string ImageFileRelativePath
        {
            get => _imageFileRelativePath;
            set
            {
                _imageFileRelativePath = value;
                TextBox.Text = !string.IsNullOrEmpty(_imageFileRelativePath) ? _imageFileRelativePath : string.Empty;
            }
        }

        public override void Clear()
        {
            _streamDeckTargetLayer = null;
            _bipLinkStreamDeck = null;
            Button = null;
            _textFont = SettingsManager.DefaultFont;
            _fontColor = SettingsManager.DefaultFontColor;
            _backgroundColor = SettingsManager.DefaultBackgroundColor;
            _dcsbiosDecoder = null;
            _textFont = SettingsManager.DefaultFont;

            if (TextBox != null)
            {
                TextBox.Background = Brushes.LightSteelBlue;
                TextBox.Text = string.Empty;
            }
            _imageFileRelativePath = string.Empty;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkStreamDeck == null || _bipLinkStreamDeck.BIPLights.Count == 0) && _streamDeckTargetLayer == null;
        }

        public bool ContainsTextFace()
        {
            return _textFont != null && !string.IsNullOrEmpty(TextBox.Text);
        }

        public bool ContainsImageFace()
        {
            return !string.IsNullOrEmpty(ImageFileRelativePath);
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosDecoder != null;
        }

        public bool ContainsStreamDeckLayer()
        {
            return _streamDeckTargetLayer != null;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkStreamDeck != null && _bipLinkStreamDeck.BIPLights.Count > 0;
        }

        public int ButtonNumber()
        {
            if (StreamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return 0;
            }
            return int.Parse(StreamDeckButtonName.ToString().Replace("BUTTON", string.Empty));
        }
    }
}
