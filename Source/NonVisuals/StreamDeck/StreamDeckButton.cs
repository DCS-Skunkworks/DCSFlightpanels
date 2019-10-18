using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace NonVisuals.StreamDeck
{
    public enum StreamDeckButtons 
    {
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
        private StreamDeckButtons _streamDeckButton;
        private bool _isPressed = false;
        private IStreamDeckButtonFace _streamDeckButtonFace = null;
        private IStreamDeckButtonAction _streamDeckButtonAction = null;

        public StreamDeckButton(bool isPressed, StreamDeckButtons streamDeckButton)
        {
            _streamDeckButton = streamDeckButton;
            _isPressed = isPressed;
        }

        public StreamDeckButtons Button
        {
            get => _streamDeckButton;
            set => _streamDeckButton = value;
        }

        public bool IsPressed
        {
            get => _isPressed;
            set => _isPressed = value;
        }

        public string ExportString()
        {
            return "StreamDeckButton{" + Enum.GetName(typeof(StreamDeckButtons), _streamDeckButton) + "}";
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
            _streamDeckButton = (StreamDeckButtons)Enum.Parse(typeof(StreamDeckButtons), dataString.Trim());
        }

        public static HashSet<StreamDeckButton> GetMultiPanelKnobs()
        {
            var result = new HashSet<StreamDeckButton>();

            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON1));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON2));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON3));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON4));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON5));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON6));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON7));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON8));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON9));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON10));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON11));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON12));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON13));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON14));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON15));
            return result;
        }


    }

}
