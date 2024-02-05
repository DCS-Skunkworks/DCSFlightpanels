namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Drawing;

    using Newtonsoft.Json;

    using Interfaces;
    using Panels;
    using ClassLibraryCommon;

    [Serializable]
    [SerializeCritical]
    public class FaceTypeDCSBIOSOverlay : FaceTypeDCSBIOS
    {
        [JsonProperty("FaceType", Required = Required.Default)]
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.DCSBIOSOverlay;
        private string _backgroundBitmapPath = string.Empty;
        [NonSerialized] private Bitmap _backgroundBitmap;
        private bool _loadBackground;
        private double _dcsbiosValue;

        public FaceTypeDCSBIOSOverlay()
        { }

        public new void Dispose()
        {
            // Call base class implementation.
            base.Dispose();
        }

        protected override void DrawBitmap()
        {
            if (_backgroundBitmap == null || _loadBackground)
            {
                _backgroundBitmap = StreamDeckPanel.GetBitmapFromPath(_backgroundBitmapPath);
                RefreshBitmap = true;
            }

            if (_bitmap == null || RefreshBitmap)
            {
                _bitmap = string.IsNullOrEmpty(_backgroundBitmapPath) ? 
                    BitMapCreator.CreateStreamDeckBitmap(ButtonFinalText, TextFont, FontColor, OffsetX, OffsetY, Color.LightGray) 
                    : BitMapCreator.CreateStreamDeckBitmap(ButtonFinalText, TextFont, FontColor, OffsetX, OffsetY, _backgroundBitmap); //User maybe only wants text displayed.
                RefreshBitmap = true;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            if (StreamDeckPanelInstance == null)
            {
                throw new Exception("StreamDeckPanelInstance is not set, cannot show image [FaceTypeDCSBIOSOverlay]");
            }

            StreamDeckPanelInstance.SetImage(StreamDeckButtonName, Bitmap);
        }

        [JsonProperty("BackgroundBitmapPath", Required = Required.Default)]
        public string BackgroundBitmapPath
        {
            get => _backgroundBitmapPath;
            set
            {
                _backgroundBitmapPath = value;
                _loadBackground = true;
            }
        }

        [JsonIgnore]
        public Bitmap BackgroundBitmap
        {
            get => _backgroundBitmap;
            set => _backgroundBitmap = value;
        }

        [JsonIgnore]
        public double DCSBIOSValue
        {
            get => _dcsbiosValue;
            set
            {
                _dcsbiosValue = value;
                RefreshBitmap = true;
            }
        }
    }
}
