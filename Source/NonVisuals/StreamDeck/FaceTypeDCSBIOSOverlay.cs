using System;
using System.Drawing;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class FaceTypeDCSBIOSOverlay : FaceTypeDCSBIOS
    {
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.DCSBIOSOverlay;
        private string _backgroundBitmapPath = "";
        [NonSerialized] private Bitmap _backgroundBitmap = null;




        protected override void Show()
        {
            if (_backgroundBitmap == null)
            {
                _backgroundBitmap = StreamDeckPanel.Validate(_backgroundBitmapPath);
                RefreshBitmap = true;
            }
            
            DrawBitmap();
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, Bitmap);
        }

        public string BackgroundBitmapPath
        {
            get => _backgroundBitmapPath;
            set => _backgroundBitmapPath = value;
        }
        
        public Bitmap BackgroundBitmap
        {
            get => _backgroundBitmap;
            set => _backgroundBitmap = value;
        }
    }
}
