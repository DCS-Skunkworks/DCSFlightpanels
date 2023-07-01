﻿namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Drawing;
    using System.Text;

    using Newtonsoft.Json;

    using Interfaces;

    [Serializable]
    public class FaceTypeDCSBIOS : FaceTypeBase, IStreamDeckButtonFace, IFontFace
    {
        [JsonProperty("FaceType", Required = Required.Default)]
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.DCSBIOS;
        private string _buttonTextTemplate = string.Empty;
        private string _buttonFinalText = string.Empty;
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;
        private uint _uintDcsBiosValue = uint.MaxValue;
        private string _stringDcsBiosValue = "PÖLKASD2!";

        public FaceTypeDCSBIOS()
        { }

        [JsonIgnore]
        public bool ConfigurationOK => !string.IsNullOrEmpty(_buttonTextTemplate) && _textFont != null;


        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public new void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
        }

        [JsonIgnore]
        public virtual string FaceDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Face DCS-BIOS");

                return stringBuilder.ToString();
            }
        }

        protected override void DrawBitmap()
        {
            if (string.IsNullOrEmpty(_buttonFinalText))
            {
                return;
            }

            if (_bitmap == null || RefreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonFinalText, _textFont, _fontColor, OffsetX, OffsetY, _backgroundColor);
                RefreshBitmap = false;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            if (Bitmap == null)
            {
                return;
            }

            if (StreamDeckPanelInstance == null)
            {
                throw new Exception("StreamDeckPanelInstance is not set, cannot show image [FaceTypeDCSBIOS]");
            }

            StreamDeckPanelInstance.SetImage(StreamDeckButtonName, Bitmap);
        }

        public override int GetHash()
        {
            unchecked
            {
                var result = string.IsNullOrWhiteSpace(_buttonTextTemplate) ? 0 : _buttonTextTemplate.GetHashCode();
                result = result * 397 ^ (_textFont?.GetHashCode() ?? 0);
                result = result * 397 ^ OffsetX;
                result = result * 397 ^ OffsetY;
                result = result * 397 ^ _fontColor.GetHashCode();
                result = result * 397 ^ _backgroundColor.GetHashCode();
                result = result * 397 ^ StreamDeckButtonName.GetHashCode();
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
            set => _fontColor = value;
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

        public uint UIntDcsBiosValue
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

        public virtual void AfterClone()
        { }
    }
}
