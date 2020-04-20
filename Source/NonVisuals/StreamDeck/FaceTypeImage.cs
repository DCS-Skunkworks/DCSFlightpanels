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

        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private string _imageFile;

        protected override void Show()
        {
            if (StreamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            if (_refreshBitmap)
            {
                _bitmap = new Bitmap(_imageFile);
                _refreshBitmap = false;
            }

            var keyBitmap = KeyBitmap.Create.FromBitmap(_bitmap);

            StreamDeckPanel.GetInstance(StreamDeckInstanceId).StreamDeckBoard.SetKeyBitmap(StreamDeckFunction.ButtonNumber(StreamDeckButtonName) - 1, keyBitmap);
        }

        [JsonIgnore]
        public string Text { get; set; }

        [JsonIgnore]
        public Font TextFont { get; set; } = Constants.DefaultStreamDeckFont;

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
