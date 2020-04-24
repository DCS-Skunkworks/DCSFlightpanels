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
        private string _buttonText;
        private Font _textFont = SettingsManager.DefaultFont;
        private Color _fontColor = SettingsManager.DefaultFontColor;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;


        public bool ConfigurationOK => !string.IsNullOrEmpty(_buttonText) && _textFont != null;


        protected override void Initialise()
        {
            throw new NotImplementedException();
        }

        protected override void DrawBitmap()
        {
            if (Bitmap == null || RefreshBitmap)
            {
                Bitmap = BitMapCreator.CreateStreamDeckBitmap(_buttonText, _textFont, _fontColor, _backgroundColor, OffsetX, OffsetY);
                RefreshBitmap = false;
            }
        }

        protected override void Show()
        {
            DrawBitmap();
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, Bitmap);
        }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                RefreshBitmap = true;
                _buttonText = value;
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
