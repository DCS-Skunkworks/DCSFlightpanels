using System;
using System.Collections.Generic;
using System.Linq;
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

        public void RemoveEmptyButtons()
        {
            _streamDeckButtons.RemoveAll(o => !o.HasConfig);
        }

        [JsonIgnore]
        public bool HasConfig
        {
            get
            {
                return _streamDeckButtons.Any(o => o.HasConfig);
            }
        }

        public List<StreamDeckButton> GetButtonsWithConfig()
        {
            return (List<StreamDeckButton>)_streamDeckButtons.Where(o => o.HasConfig);
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

        public StreamDeckButton GetStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in _streamDeckButtons)
            {
                if (streamDeckButton.StreamDeckButtonName == streamDeckButtonName)
                {
                    return streamDeckButton;
                }
            }
            return new StreamDeckButton(streamDeckButtonName);
        }

        public bool ContainStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName)
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

        public StreamDeckButton GetStreamDeckButtonName(EnumStreamDeckButtonNames streamDeckButtonName)
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
