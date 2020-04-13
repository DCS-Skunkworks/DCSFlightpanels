using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeBase
    {
        public EnumStreamDeckFaceType FaceType
        {
            get { return EnumStreamDeckFaceType.Unknown; }
        }
        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private StreamDeckPanel _streamDeck;
        private StreamDeckButton _streamDeckButton;
        private bool _isVisible;
        private int _offsetX;
        private int _offsetY;

        [JsonIgnore]
        public Bitmap Bitmap
        {
            get => _bitmap;
            set => _bitmap = value;
        }

        [JsonIgnore]
        public bool RefreshBitmap
        {
            get => _refreshBitmap;
            set => _refreshBitmap = value;
        }

        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }
        
        [JsonIgnore]
        public StreamDeckPanel StreamDeck
        {
            get => _streamDeck;
            set => _streamDeck = value;
        }

        [JsonIgnore]
        public StreamDeckButton StreamDeckButton
        {
            get => _streamDeckButton;
            set => _streamDeckButton = value;
        }

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public int OffsetX
        {
            get => _offsetX;
            set
            {
                _refreshBitmap = true;
                _offsetX = value;
            }
        }

        public int OffsetY
        {
            get => _offsetY;
            set
            {
                _refreshBitmap = true;
                _offsetY = value;
            }
        }
    }
}
