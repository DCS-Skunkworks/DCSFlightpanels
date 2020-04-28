using System;
using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class FaceTypeDCSBIOSOverlay : FaceTypeDCSBIOS
    {
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.DCSBIOSOverlay;
        private string _backgroundBitmapPath = "";
        [NonSerialized] private Bitmap _backgroundBitmap = null;
        private bool _loadBackground = false;
        private double _dcsbiosValue = 0;


        protected override void DrawBitmap()
        {
            if (_backgroundBitmap == null || _loadBackground)
            {
                _backgroundBitmap = StreamDeckPanel.Validate(_backgroundBitmapPath);
                RefreshBitmap = true;
            }

            if (_bitmap == null || RefreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(ButtonFinalText, TextFont, FontColor, OffsetX, OffsetY, _backgroundBitmap);
                RefreshBitmap = false;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, Bitmap);
        }

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
