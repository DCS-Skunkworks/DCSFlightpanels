using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum StreamDeck35Buttons
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

    public class StreamDeck35Button
    {
        private StreamDeck35Buttons _streamDeck35Button;
        private bool _isPressed = false;

        public StreamDeck35Button(bool isPressed, StreamDeck35Buttons streamDeck35Button)
        {
            _streamDeck35Button = streamDeck35Button;
            _isPressed = isPressed;
        }

        public StreamDeck35Buttons Button
        {
            get => _streamDeck35Button;
            set => _streamDeck35Button = value;
        }

        public bool IsPressed
        {
            get => _isPressed;
            set => _isPressed = value;
        }

        public string ExportString()
        {
            return "StreamDeck35Button{" + Enum.GetName(typeof(StreamDeck35Buttons), _streamDeck35Button) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (StreamDeck35Button)");
            }
            if (!str.StartsWith("StreamDeck35Button{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (StreamDeck35Button) >" + str + "<");
            }
            //StreamDeck35Button{BUTTON11}
            var dataString = str.Remove(0, 15);
            //BUTTON11}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //BUTTON11
            _streamDeck35Button = (StreamDeck35Buttons)Enum.Parse(typeof(StreamDeck35Buttons), dataString.Trim());
        }

        public static HashSet<StreamDeck35Button> GetMultiPanelKnobs()
        {
            var result = new HashSet<StreamDeck35Button>();

            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON11));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON12));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON13));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON14));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON15));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON21));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON22));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON23));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON24));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON25));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON31));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON32));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON33));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON34));
            result.Add(new StreamDeck35Button(true, StreamDeck35Buttons.BUTTON35));
            return result;
        }


    }

}
