namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Drawing;

    using MEF;

    using Newtonsoft.Json;

    using Interfaces;
    using Panels;
    using System.IO;
    using System.Drawing.Imaging;
    using NLog;
    using ClassLibraryCommon;

    [Serializable]
    [SerializeCritical]
    public abstract class FaceTypeBase : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [JsonProperty("FaceType", Required = Required.Default)]
        public EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Unknown;

        [NonSerialized] protected Bitmap _bitmap;
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



        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        [JsonProperty("RawBitmap", Required = Required.Default)]
        public byte[] RawBitmap
        {
            get {
                
                if (Bitmap != null)
                {
                    MemoryStream ms = new();
                    Bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                } 
                return null;
            }
            set
            {
                if (value != null)
                {
                    try
                    {
                        using MemoryStream ms = new(value);
                        Bitmap = new Bitmap(ms);
                        RefreshBitmap = false; // we already got a bitmap, no need to load from ImageFile property
                    }
                    catch(Exception ex)
                    {
                        //Maybe the serialized pic was corrupted by the user or something bad happened in the profile, handle this gracefully
                        Logger.Error($"Could not convert image stream to bitmap image: {ex.Message}. Reverting to FileNotFound image");
                        Bitmap = BitMapCreator.FileNotFoundBitmap();
                        RefreshBitmap = false; // we already got a bitmap, no need to load from ImageFile property
                    }
                }
            }
        }
    }
}
