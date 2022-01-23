using NonVisuals.StreamDeck.Panels;

namespace NonVisuals.StreamDeck
{
    using System;
    using System.Drawing;
    using System.Text;

    using Newtonsoft.Json;

    using NonVisuals.Interfaces;

    using OpenMacroBoard.SDK;

    [Serializable]
    public class FaceTypeImage : FaceTypeBase, IStreamDeckButtonFace
    {
        [JsonProperty("FaceType", Required = Required.Default)]
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Image;
        private string _imageFile;
        [NonSerialized]private KeyBitmap _keyBitmap;
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;


        [JsonIgnore]
        public bool ConfigurationOK => !string.IsNullOrEmpty(_imageFile);




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
        public string FaceDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Face Image");
                if (!string.IsNullOrEmpty(_imageFile))
                {
                    stringBuilder.Append(" ").Append(_imageFile);
                }

                return stringBuilder.ToString();
            }
        }

        protected override void DrawBitmap()
        {
            if (_bitmap == null || RefreshBitmap)
            {
                _bitmap = StreamDeckPanel.Validate(_imageFile);
                
                if (BitMapCreator.IsSmallerThanStreamdeckDefault(_bitmap))
                {
                    _bitmap = BitMapCreator.AdjustBitmap(_bitmap, 1.0f, 4.0f, 1.0f);
                    _bitmap = BitMapCreator.EnlargeBitmapCanvas(_bitmap);
                }

                RefreshBitmap = false;
            }

            if (_keyBitmap == null)
            {
                _keyBitmap = KeyBitmap.Create.FromBitmap(_bitmap);
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            if (StreamDeckPanelInstance == null)
            {
                throw new Exception("StreamDeckPanelInstance is not set, cannot show image [FaceTypeImage]");
            }

            StreamDeckPanelInstance.StreamDeckBoard.SetKeyBitmap(StreamDeckCommon.ButtonNumber(StreamDeckButtonName) - 1, _keyBitmap);
        }


        public override int GetHash()
        {
            unchecked
            {
                var result = string.IsNullOrWhiteSpace(_imageFile) ? 0 : _imageFile.GetHashCode();
                result = (result * 397) ^ (_textFont?.GetHashCode() ?? 0);
                result = (result * 397) ^ OffsetX;
                result = (result * 397) ^ OffsetY;
                result = (result * 397) ^ _fontColor.GetHashCode();
                result = (result * 397) ^ _backgroundColor.GetHashCode();
                result = (result * 397) ^ StreamDeckButtonName.GetHashCode();
                return result;
            }
        }

        [JsonIgnore]
        public string Text { get; set; }

        [JsonProperty("TextFont", Required = Required.Default)]
        public Font TextFont
        {
            get => _textFont;
            set
            {
                SettingsManager.DefaultFont = value;
                _textFont = value;
            }
        }

        [JsonProperty("FontColor", Required = Required.Default)]
        public Color FontColor
        {
            get => _fontColor;
            set
            {
                SettingsManager.DefaultFontColor = value;
                _fontColor = value;
            }
        }

        [JsonProperty("BackgroundColor", Required = Required.Default)]
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                SettingsManager.DefaultBackgroundColor = value;
                _backgroundColor = value;
            }
        }

        [JsonProperty("ImageFile", Required = Required.Default)]
        public string ImageFile
        {
            get => _imageFile;
            set
            {
                RefreshBitmap = true;
                _imageFile = value;
            }
        }
        
        public void AfterClone()
        { }
    }
}
