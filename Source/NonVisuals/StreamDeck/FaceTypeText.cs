using System;
using System.Drawing;
using System.Text;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class FaceTypeText : FaceTypeBase, IStreamDeckButtonFace, IFontFace
    {
        [JsonProperty("FaceType", Required = Required.Default)]
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Text;
        private string _buttonTextTemplate = string.Empty;
        private string _buttonFinalText = string.Empty;
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;


        [JsonIgnore]
        public bool ConfigurationOK => !string.IsNullOrEmpty(_buttonTextTemplate) && _textFont != null;


        public virtual void Dispose() { }

        [JsonIgnore]
        public string FaceDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Face Text");
                if (!string.IsNullOrEmpty(_buttonTextTemplate))
                {
                    stringBuilder.Append(" ").Append(_buttonTextTemplate);
                }

                return stringBuilder.ToString();
            }
        }

        protected override void DrawBitmap()
        {
            if (_bitmap == null || RefreshBitmap)
            {
                if (string.IsNullOrEmpty(ButtonFinalText) || !_buttonTextTemplate.Contains(StreamDeckConstants.DCSBIOSValuePlaceHolder))
                {
                    _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonTextTemplate, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                }
                else
                {
                    _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonFinalText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                }
                RefreshBitmap = false;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            if (StreamDeckPanelInstance == null)
            {
                throw new Exception("StreamDeckPanelInstance is not set, cannot show image [FaceTypeDCSBIOS.]");
            }
            StreamDeckPanelInstance.SetImage(StreamDeckButtonName, Bitmap);
        }

        public override int GetHash()
        {
            unchecked
            {
                var result = string.IsNullOrWhiteSpace(_buttonTextTemplate) ? 0 : _buttonTextTemplate.GetHashCode();
                result = (result * 397) ^ (_textFont?.GetHashCode() ?? 0);
                result = (result * 397) ^ OffsetX;
                result = (result * 397) ^ OffsetY;
                result = (result * 397) ^ _fontColor.GetHashCode();
                result = (result * 397) ^ _backgroundColor.GetHashCode();
                result = (result * 397) ^ StreamDeckButtonName.GetHashCode();
                return result;
            }
        }


        [JsonProperty("ButtonTextTemplate", Required = Required.Default)]
        public string ButtonTextTemplate
        {
            get => _buttonTextTemplate;
            set
            {
                RefreshBitmap = true;
                _buttonTextTemplate = value;
            }
        }

        [JsonIgnore]
        public string ButtonFinalText
        {
            get => _buttonFinalText;
            set
            {
                RefreshBitmap = true;
                _buttonFinalText = value;
            }
        }


        [JsonProperty("TextFont", Required = Required.Default)]
        public Font TextFont
        {
            get => _textFont;
            set
            {
                RefreshBitmap = true;
                _textFont = value;
            }
        }

        [JsonProperty("FontColor", Required = Required.Default)]
        public Color FontColor
        {
            get => _fontColor;
            set
            {
                RefreshBitmap = true;
                _fontColor = value;
            }
        }

        [JsonProperty("BackgroundColor", Required = Required.Default)]
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                RefreshBitmap = true;
                _backgroundColor = value;
            }
        }
        
        public void AfterClone()
        { }
    }
}
