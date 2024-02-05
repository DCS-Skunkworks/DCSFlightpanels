using System.Drawing;
using MEF;
using NonVisuals.Panels.StreamDeck.Panels;
using NonVisuals.Panels.StreamDeck;
using NonVisuals;
using System.Windows.Media;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckFaceTextBox : TextBoxBaseOutput
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

        public StreamDeckPanel StreamDeckPanelInstance { get; set; }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_fontColor.A, _fontColor.R, _fontColor.G, _fontColor.B)); ;
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_backgroundColor.A, _backgroundColor.R, _backgroundColor.G, _backgroundColor.B)); ;
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
                FontFamily = new System.Windows.Media.FontFamily(_textFont.Name);
                FontWeight = _textFont.Bold ? FontWeights.Bold : FontWeights.Regular;
                FontSize = _textFont.Size * 96.0 / 72.0;
                FontStyle = _textFont.Italic ? FontStyles.Italic : FontStyles.Normal;
                var textDecorationCollection = new TextDecorationCollection();
                if (_textFont.Underline) textDecorationCollection.Add(System.Windows.TextDecorations.Underline);
                if (_textFont.Strikeout) textDecorationCollection.Add(System.Windows.TextDecorations.Strikethrough);
                TextDecorations = textDecorationCollection;
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
                    Text = string.Empty;
                    return;
                }
                if (!_dcsbiosDecoder.UseFormula && _dcsbiosDecoder.DCSBIOSOutput != null)
                {
                    Text = _dcsbiosDecoder.DCSBIOSOutput.ControlId;
                }
                else if (_dcsbiosDecoder.UseFormula)
                {
                    Text = "formula";
                }
            }
        }

        public string ImageFileRelativePath
        {
            get => _imageFileRelativePath;
            set
            {
                _imageFileRelativePath = value;
                Text = !string.IsNullOrEmpty(_imageFileRelativePath) ? _imageFileRelativePath : string.Empty;
            }
        }

        public new void Clear()
        {
            _streamDeckTargetLayer = null;
            _bipLinkStreamDeck = null;
            Button = null;
            _textFont = SettingsManager.DefaultFont;
            _fontColor = SettingsManager.DefaultFontColor;
            _backgroundColor = SettingsManager.DefaultBackgroundColor;
            _dcsbiosDecoder = null;
            _textFont = SettingsManager.DefaultFont;

            Background = Brushes.LightSteelBlue;
            Text = string.Empty;

            _imageFileRelativePath = string.Empty;
        }

        public bool IsEmpty()
        {
            return (_bipLinkStreamDeck == null || _bipLinkStreamDeck.BIPLights.Count == 0) && _streamDeckTargetLayer == null;
        }

        public bool ContainsTextFace()
        {
            return _textFont != null && !string.IsNullOrEmpty(Text);
        }

        public bool ContainsImagePath()
        {
            return !string.IsNullOrEmpty(ImageFileRelativePath);
        }

        public bool ContainsDCSBIOS()
        {
            return _dcsbiosDecoder != null;
        }

        public bool ContainsStreamDeckLayer()
        {
            return _streamDeckTargetLayer != null;
        }

        public bool ContainsBIPLink()
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
