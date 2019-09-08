using System;
using System.Collections.Generic;
using NonVisuals.StreamDeck;

namespace NonVisuals
{
    public enum StreamDeckButtons
    {
        BUTTON11,
        BUTTON12,
        BUTTON13,
        BUTTON14,
        BUTTON15,
        BUTTON21,
        BUTTON22,
        BUTTON23,
        BUTTON24,
        BUTTON25,
        BUTTON31,
        BUTTON32,
        BUTTON33,
        BUTTON34,
        BUTTON35
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

            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON11));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON12));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON13));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON14));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON15));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON21));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON22));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON23));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON24));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON25));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON31));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON32));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON33));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON34));
            result.Add(new StreamDeckButton(true, StreamDeckButtons.BUTTON35));
            return result;
        }


    }

}
