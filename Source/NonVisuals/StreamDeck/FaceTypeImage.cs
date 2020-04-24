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
        private KeyBitmap _keyBitmap;
        private Font _font = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;




        public bool ConfigurationOK => !string.IsNullOrEmpty(_imageFile);



        protected override void DrawBitmap()
        {
            if (Bitmap == null || RefreshBitmap)
            {
                Bitmap = StreamDeckPanel.Validate(_imageFile);
                RefreshBitmap = false;
            }

            if (_keyBitmap == null)
            {
                _keyBitmap = KeyBitmap.Create.FromBitmap(Bitmap);
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).StreamDeckBoard.SetKeyBitmap(StreamDeckFunction.ButtonNumber(StreamDeckButtonName) - 1, _keyBitmap);
        }

        [JsonIgnore]
        public string Text { get; set; }
        
        public Font TextFont
        {
            get => _font;
            set
            {
                SettingsManager.DefaultFont = value;
                _font = value;
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
