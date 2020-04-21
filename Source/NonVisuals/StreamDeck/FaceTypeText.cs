using System;
using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class FaceTypeText : FaceTypeBase, IStreamDeckButtonFace, IFontFace
    {
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Text;
        private string _text;
        private Font _textFont = StreamDeckConstants.DefaultStreamDeckFont;
        private Color _fontColor;
        private Color _backgroundColor;





        protected override void DrawBitmap()
        {
            if (Bitmap == null || RefreshBitmap)
            {
                Bitmap = BitMapCreator.CreateStreamDeckBitmap(_text, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                RefreshBitmap = false;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, Bitmap);
        }

        public string Text
        {
            get => _text;
            set
            {
                RefreshBitmap = true;
                _text = value;
            }
        }

        public Font TextFont
        {
            get => _textFont;
            set
            {
                RefreshBitmap = true;
                _textFont = value;
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                RefreshBitmap = true;
                _fontColor = value;
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                RefreshBitmap = true;
                _backgroundColor = value;
            }
        }

    }
}
