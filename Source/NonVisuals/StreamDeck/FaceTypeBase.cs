namespace NonVisuals.StreamDeck
{
    using System;
    using System.Drawing;

    using MEF;

    using Newtonsoft.Json;

    using NonVisuals.Interfaces;

    [Serializable]
    public abstract class FaceTypeBase
    {
        [JsonProperty("FaceType", Required = Required.Default)]
        public EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Unknown;

        [NonSerialized]protected Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;
        private StreamDeckButton _streamDeckButton;
        private volatile bool _isVisible;
        private int _offsetX;
        private int _offsetY;


        public abstract int GetHash();
        protected abstract void DrawBitmap();
        protected abstract void Show();



        [JsonIgnore]
        public Bitmap Bitmap
        {
            get
            {
                DrawBitmap();
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


        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set
            {
                _streamDeckPanel = value;
            }
        }

        [JsonIgnore]
        public StreamDeckButton StreamDeckButton
        {
            get => _streamDeckButton;
            set => _streamDeckButton = value;
        }

        [JsonIgnore]
        public virtual bool IsVisible
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

        [JsonProperty("OffsetX", Required = Required.Default)]
        public int OffsetX
        {
            get => _offsetX;
            set
            {
                _refreshBitmap = true;
                _offsetX = value;
            }
        }

        [JsonProperty("OffsetY", Required = Required.Default)]
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
