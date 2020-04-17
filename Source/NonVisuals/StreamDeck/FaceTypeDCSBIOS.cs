using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeDCSBIOS : FaceTypeBase, IStreamDeckButtonFace, IFontFace
    {
        public new EnumStreamDeckFaceType FaceType
        {
            get  => EnumStreamDeckFaceType.DCSBIOS;
        }
        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _buttonText;
        private Font _textFont = Constants.DefaultStreamDeckFont;
        private Color _fontColor;
        private Color _backgroundColor;
        private bool _isVisible;
        private uint _dcsBiosValue = 0;
        private string _streamDeckInstanceId;

        public void Show()
        {
            if (_refreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                _refreshBitmap = false;
            }
            StreamDeckPanel.GetInstance(_streamDeckInstanceId).SetImage(_streamDeckButtonName, _bitmap);
        }
        
        [JsonIgnore]
        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _refreshBitmap = true;
                _bitmap = value;
            }
        }


        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set
            {
                _refreshBitmap = true;
                _streamDeckButtonName = value;
            }
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

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public string StreamDeckInstanceId
        {
            get => _streamDeckInstanceId;
            set => _streamDeckInstanceId = value;
        }
    }
}
