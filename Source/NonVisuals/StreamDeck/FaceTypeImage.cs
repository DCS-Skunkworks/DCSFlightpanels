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





        protected override void DrawBitmap()
        {
            if (Bitmap == null || RefreshBitmap)
            {
                Bitmap = new Bitmap(_imageFile);
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

        [JsonIgnore]
        public Font TextFont { get; set; } = StreamDeckConstants.DefaultStreamDeckFont;

        [JsonIgnore]
        public Color FontColor { get; set; }

        [JsonIgnore]
        public Color BackgroundColor { get; set; }

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
