using System;
using System.Drawing;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class FaceTypeDCSBIOS : IStreamDeckButtonFace
    {
        public EnumStreamDeckFaceType FaceType => EnumStreamDeckFaceType.Text;
        private Bitmap _bitmap;
        private bool _refreshBitmap = true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _text;
        private Font _textFont = Constants.DefaultStreamDeckFont;
        private Color _fontColor;
        private Color _backgroundColor;
        private int _offsetX;
        private int _offsetY;
        private DCSBIOSFaceBindingStreamDeck _dcsbiosFaceBindingStreamDeck;
        private uint _dcsBiosValue = 0;
        

        public void Show(StreamDeckRequisites streamDeckRequisite)
        {
            if (streamDeckRequisite.StreamDeck != null)
            {
                ShowButtonFace(streamDeckRequisite.StreamDeck);
            }
            else if (streamDeckRequisite.StreamDeckBoard != null)
            {
                ShowButtonFace(streamDeckRequisite.StreamDeckBoard);
            }
        }

        private void ShowButtonFace(IStreamDeckBoard streamDeckBoard)
        {
            if (_streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            if (_refreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_text, _textFont, _fontColor, _backgroundColor, _offsetX, _offsetY);
                _refreshBitmap = false;
            }

            var keyBitmap = KeyBitmap.Create.FromBitmap(_bitmap);

            streamDeckBoard.SetKeyBitmap(StreamDeckFunction.ButtonNumber(_streamDeckButtonName) - 1, keyBitmap);
        }

        private void ShowButtonFace(StreamDeckPanel streamDeckPanel)
        {
            if (_refreshBitmap)
            {
                _bitmap = BitMapCreator.CreateStreamDeckBitmap(_text, _textFont, _fontColor, _backgroundColor, _offsetX, _offsetY);
                _refreshBitmap = false;
            }
            streamDeckPanel.SetImage(_streamDeckButtonName, _bitmap);
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


        public DCSBIOSFaceBindingStreamDeck DCSBIOSFaceBinding
        {
            get
            {
                return _dcsbiosFaceBindingStreamDeck;
            }
            set
            {
                _dcsbiosFaceBindingStreamDeck = DCSBIOSFaceBinding;
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

    }
}
