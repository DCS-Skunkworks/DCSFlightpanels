using System;
using System.Diagnostics;
using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class FaceTypeDCSBIOS : FaceTypeBase, IStreamDeckButtonFace, IFontFace
    {
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.DCSBIOS;
        private string _buttonTextTemplate ="";
        private string _buttonFinalText = "";
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;
        private uint _uintDcsBiosValue = 0;
        private string _stringDcsBiosValue = "";

        public bool ConfigurationOK => !string.IsNullOrEmpty(_buttonTextTemplate) && _textFont != null;


        public virtual void Destroy()
        {
            Debugger.Break();
        }

        protected override void DrawBitmap()
        {
            if (_bitmap == null || RefreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonFinalText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                RefreshBitmap = false;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, Bitmap);
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

        public Font TextFont
        {
            get => _textFont;
            set
            {
                RefreshBitmap = true;
                _textFont = value;
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set => _fontColor = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                RefreshBitmap = true;
                _backgroundColor = value;
            }
        }

        public uint UintDcsBiosValue
        {
            get => _uintDcsBiosValue;
            set
            {
                _uintDcsBiosValue = value;
                RefreshBitmap = true;
            }
        }

        public string StringDcsBiosValue
        {
            get => _stringDcsBiosValue;
            set
            {
                _stringDcsBiosValue = value;
                RefreshBitmap = true;
            }
        }
    }
}
