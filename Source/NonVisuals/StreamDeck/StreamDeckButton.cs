using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public enum StreamDeckButtonNames 
    {
        BUTTON0_NO_BUTTON,
        BUTTON1,
        BUTTON2,
        BUTTON3,
        BUTTON4,
        BUTTON5,
        BUTTON6,
        BUTTON7,
        BUTTON8,
        BUTTON9,
        BUTTON10,
        BUTTON11,
        BUTTON12,
        BUTTON13,
        BUTTON14,
        BUTTON15
    }

    public class StreamDeckButton 
    {
        private StreamDeckButtonNames _streamDeckButtonName;
        private bool _isPressed = false;
        private IStreamDeckButtonFace _streamDeckButtonFaceForPress = null;
        private IStreamDeckButtonAction _streamDeckButtonActionForPress = null;
        private IStreamDeckButtonFace _streamDeckButtonFaceForRelease = null;
        private IStreamDeckButtonAction _streamDeckButtonActionForRelease = null;
        private string _layerName;

        public StreamDeckButton(bool isPressed, StreamDeckButtonNames streamDeckButton)
        {
            _streamDeckButtonName = streamDeckButton;
            _isPressed = isPressed;
        }

        public StreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public IStreamDeckButtonFace StreamDeckButtonFaceForPress
        {
            get => _streamDeckButtonFaceForPress;
            set => _streamDeckButtonFaceForPress = value;
        }

        public IStreamDeckButtonAction StreamDeckButtonActionForPress
        {
            get => _streamDeckButtonActionForPress;
            set => _streamDeckButtonActionForPress = value;
        }



        public IStreamDeckButtonFace StreamDeckButtonFaceForRelease
        {
            get => _streamDeckButtonFaceForRelease;
            set => _streamDeckButtonFaceForRelease = value;
        }

        public IStreamDeckButtonAction StreamDeckButtonActionForRelease
        {
            get => _streamDeckButtonActionForRelease;
            set => _streamDeckButtonActionForRelease = value;
        }

        public bool IsPressed
        {
            get => _isPressed;
            set => _isPressed = value;
        }

        public string LayerName
        {
            get => _layerName;
            set => _layerName = value;
        }

        public string ExportString()
        {
            return "StreamDeckButton{" + Enum.GetName(typeof(StreamDeckButtonNames), _streamDeckButtonName) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (StreamDeckButton)");
            }
            if (!str.StartsWith("StreamDeckButton{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (StreamDeckButton) >" + str + "<");
            }
            //StreamDeckButton{BUTTON11}
            var dataString = str.Remove(0, 15);
            //BUTTON11}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //BUTTON11
            _streamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), dataString.Trim());
        }

        public static HashSet<StreamDeckButton> GetAllStreamDeckButtonNames()
        {
            var result = new HashSet<StreamDeckButton>();

            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON1));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON2));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON3));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON4));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON5));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON6));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON7));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON8));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON9));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON10));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON11));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON12));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON13));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON14));
            result.Add(new StreamDeckButton(true, StreamDeckButtonNames.BUTTON15));
            return result;
        }


    }

}
