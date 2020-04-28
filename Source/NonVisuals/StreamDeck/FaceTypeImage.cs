using System;
using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using OpenMacroBoard.SDK;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class FaceTypeImage : FaceTypeBase, IStreamDeckButtonFace
    {
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Image;
        private bool _refreshBitmap = true;
        private string _imageFile;
        [NonSerialized]private KeyBitmap _keyBitmap;
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;



        public bool ConfigurationOK => !string.IsNullOrEmpty(_imageFile);


        public virtual void Destroy() {}

        protected override void DrawBitmap()
        {
            if (_bitmap == null || RefreshBitmap)
            {
                _bitmap = StreamDeckPanel.Validate(_imageFile);
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
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).StreamDeckBoard.SetKeyBitmap(StreamDeckCommon.ButtonNumber(StreamDeckButtonName) - 1, _keyBitmap);
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
        
        public Font TextFont
        {
            get => _textFont;
            set
            {
                SettingsManager.DefaultFont = value;
                _textFont = value;
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                SettingsManager.DefaultFontColor = value;
                _fontColor = value;
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                SettingsManager.DefaultBackgroundColor = value;
                _backgroundColor = value;
            }
        }

        public string ImageFile
        {
            get => _imageFile;
            set
            {
                _refreshBitmap = true;
                _imageFile = value;
            }
        }

    }
}
