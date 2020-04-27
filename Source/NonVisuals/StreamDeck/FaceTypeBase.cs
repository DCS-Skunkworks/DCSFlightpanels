using System;
using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public abstract class FaceTypeBase
    {
        public EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Unknown;
        [NonSerialized]private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _streamDeckInstanceId;
        private StreamDeckButton _streamDeckButton;
        private bool _isVisible;
        private int _offsetX = 0;
        private int _offsetY = 0;


        public abstract int GetHash();
        protected abstract void DrawBitmap();
        protected abstract void Show();

        [JsonIgnore]
        public Bitmap Bitmap
        {
            get
            {
                return _bitmap;
            }
            set
            {
                _bitmap = value;
                _refreshBitmap = true;
            }
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
        
        
        public string StreamDeckInstanceId
        {
            get => _streamDeckInstanceId;
            set => _streamDeckInstanceId = value;
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
            set
            {
                _isVisible = value;
                if (IsVisible)
                {
                    Show();
                }
            }
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
