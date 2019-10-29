using System.Drawing;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeText : IStreamDeckButtonFace
    {
        public EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Text;
        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private StreamDeckButtonNames _streamDeckButtonName;
        private string _text;
        private Font _textFont;
        private Color _fontColor;
        private Color _backgroundColor;
        public StreamDeckPanel ParentPanel { get; set; }
        private int _offsetX;
        private int _offsetY;
        private bool _whenTurnedOn;





        public void Execute()
        {
            ShowButtonFace();
        }

        private void ShowButtonFace()
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(_text, _textFont, _fontColor, _backgroundColor, _offsetX, _offsetY);
            ParentPanel.SetImage(_streamDeckButtonName, bitmap);
        }

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _refreshBitmap = true;
                _bitmap = value;
            }
        }

        public StreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set
            {
                _refreshBitmap = true;
                _streamDeckButtonName = value;
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _refreshBitmap = true;
                _text = value;
            }
        }

        public Font TextFont
        {
            get => _textFont;
            set
            {
                _refreshBitmap = true;
                _textFont = value;
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set => _fontColor = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _refreshBitmap = true;
                _backgroundColor = value;
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

        public bool WhenTurnedOn
        {
            get => _whenTurnedOn;
            set => _whenTurnedOn = value;
        }
    }
}
