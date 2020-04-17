using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeDCSBIOS : FaceTypeBase, IStreamDeckButtonFace, IFontFace
    {
        public new EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.DCSBIOS;
        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private string _buttonText;
        private Font _textFont = Constants.DefaultStreamDeckFont;
        private Color _fontColor;
        private Color _backgroundColor;
        private uint _dcsBiosValue = 0;

        protected override void Show()
        {
            if (_refreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                _refreshBitmap = false;
            }
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, _bitmap);
        }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _refreshBitmap = true;
                _buttonText = value;
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

        public uint DCSBiosValue
        {
            get => _dcsBiosValue;
            set => _dcsBiosValue = value;
        }

        [JsonIgnore]
        public Bitmap ButtonBitmap
        {
            get => BitMapCreator.CreateStreamDeckBitmap(_buttonText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
        }
        
    }
}
