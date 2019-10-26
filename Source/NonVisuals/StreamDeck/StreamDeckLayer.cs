using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayer
    {
        private bool _isActive = false;
        private bool _isHomeLayer = false;
        private string _name = "";
        private List<StreamDeckButton> _streamDeckButtons = new List<StreamDeckButton>();

        [JsonIgnore]
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public void AddButton(StreamDeckButton streamDeckButton)
        {
            _streamDeckButtons.Add(streamDeckButton);
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

        public StreamDeckLayer GetEmptyLayer()
        {
            var result = new StreamDeckLayer();
            result.Name = Name;
            result.IsHomeLayer = IsHomeLayer;
            result.IsActive = IsActive;
            return result;
        }

        public List<StreamDeckButton> StreamDeckButtons
        {
            get => _streamDeckButtons;
            set => _streamDeckButtons = value;
        }

        public StreamDeckButton GetStreamDeckButton(StreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in _streamDeckButtons)
            {
                if (streamDeckButton.StreamDeckButtonName == streamDeckButtonName)
                {
                    return streamDeckButton;
                }
            }
            return new StreamDeckButton(false, streamDeckButtonName);
        }

        public bool ContainStreamDeckButton(StreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in _streamDeckButtons)
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
            foreach (var streamDeckButton in _streamDeckButtons)
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
