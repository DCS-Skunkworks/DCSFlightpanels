using System;
using System.Collections.Generic;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayer
    {
        private bool _isActive = false;
        private bool _isHomeLayer = false;
        private string _name = "";
        private List<StreamDeckButton> _buttons = new List<StreamDeckButton>();

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }


        public bool IsHomeLayer
        {
            get => _isHomeLayer;
            set => _isHomeLayer = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        
        public List<StreamDeckButton> Buttons
        {
            get => _buttons;
            set => _buttons = value;
        }

        public bool ContainStreamDeckButton(StreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in _buttons)
            {
                if (streamDeckButton.StreamDeckButtonName == streamDeckButtonName)
                {
                    return true;
                }
            }

            return false;
        }

        public StreamDeckButton GetStreamDeckButtonName(StreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in _buttons)
            {
                if (streamDeckButton.StreamDeckButtonName == streamDeckButtonName)
                {
                    return streamDeckButton;
                }
            }

            throw new Exception("StreamDeckLayer " + Name + " does not contain button " + streamDeckButtonName + ".");
        }
    }
}
