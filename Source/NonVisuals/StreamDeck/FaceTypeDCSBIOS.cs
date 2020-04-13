using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeDCSBIOS : FaceTypeBase, IStreamDeckButtonFace
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
        private StreamDeckPanel _streamDeck;
        private StreamDeckButton _streamDeckButton;
        private bool _isVisible;
        private uint _dcsBiosValue = 0;
        
        public void Show()
        {
            /*if (streamDeckRequisite.StreamDeck != null)
            {
                ShowButtonFace(streamDeckRequisite.StreamDeck);
            }
            else if (streamDeckRequisite.StreamDeckBoard != null)
            {
                ShowButtonFace(streamDeckRequisite.StreamDeckBoard);
            }*/
        }

        /*protected void ShowButtonFace(IStreamDeckBoard streamDeckBoard)
        {
            if (_streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            if (_refreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonText, _textFont, _fontColor, _backgroundColor, _offsetX, _offsetY);
                _refreshBitmap = false;
            }

            var keyBitmap = KeyBitmap.Create.FromBitmap(_bitmap);

            streamDeckBoard.SetKeyBitmap(StreamDeckFunction.ButtonNumber(_streamDeckButtonName) - 1, keyBitmap);
        }*/

        protected void ShowButtonFace()
        {
            if (_refreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                _refreshBitmap = false;
            }
            StreamDeck.SetImage(_streamDeckButtonName, _bitmap);
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
        public StreamDeckPanel StreamDeck
        {
            get => _streamDeck;
            set => _streamDeck = value;
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
            set => _isVisible = value;
        }
    }
}
