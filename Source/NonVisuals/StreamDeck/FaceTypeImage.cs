using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeImage : FaceTypeBase, IStreamDeckButtonFace
    {
        public EnumStreamDeckFaceType FaceType
        {
            get { return EnumStreamDeckFaceType.Image; }
        }

        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _imageFile;


        public void Show()
        {
            /*if (streamDeckRequisite.StreamDeck != null)
            {
                ShowButtonFace(streamDeckRequisite.StreamDeck);
            }
            else if (streamDeckRequisite.StreamDeckBoard != null)
            {
                ShowButtonFace(streamDeckRequisite.StreamDeckBoard);
            }*/
        }

        private void ShowButtonFace(IStreamDeckBoard streamDeckBoard)
        {
            if (_streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            if (_refreshBitmap)
            {
                _bitmap = new Bitmap(_imageFile);
                _refreshBitmap = false;
            }

            var keyBitmap = KeyBitmap.Create.FromBitmap(_bitmap);

            streamDeckBoard.SetKeyBitmap(StreamDeckFunction.ButtonNumber(_streamDeckButtonName) - 1, keyBitmap);
        }

        private void ShowButtonFace(StreamDeckPanel streamDeckPanel)
        {
            if (_refreshBitmap)
            {
                _bitmap = new Bitmap(_imageFile);
                _refreshBitmap = false;
            }
            streamDeckPanel.SetImage(_streamDeckButtonName, _bitmap);
        }

        [JsonIgnore]
        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _refreshBitmap = true;
                _bitmap = value;
            }
        }

        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set
            {
                _streamDeckButtonName = value;
            }
        }

        [JsonIgnore]
        public string Text { get; set; }

        [JsonIgnore]
        public Font TextFont { get; set; } = Constants.DefaultStreamDeckFont;

        [JsonIgnore]
        public Color FontColor { get; set; }

        [JsonIgnore]
        public Color BackgroundColor { get; set; }

        [JsonIgnore]
        public int OffsetX { get; set; }

        [JsonIgnore]
        public int OffsetY { get; set; }

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
