using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DCS_BIOS;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayer
    {
        private string _name = "";
        private List<StreamDeckButton> _streamDeckButtons = new List<StreamDeckButton>();
        private Font _textFont;
        private Color _fontColor;
        private Color _backgroundColor;


        public Font TextFont
        {
            set
            {
                foreach (var streamDeckButton in StreamDeckButtons)
                {
                    if (streamDeckButton.Face != null)
                    {
                        streamDeckButton.Face.TextFont = value;
                    }
                }

                _textFont = value;
            }
        }

        public Color FontColor
        {
            set
            {
                foreach (var streamDeckButton in StreamDeckButtons)
                {
                    if (streamDeckButton.Face != null)
                    {
                        streamDeckButton.Face.FontColor = value;
                    }
                }

                _fontColor = value;
            }
        }

        public Color BackgroundColor
        {
            set
            {
                foreach (var streamDeckButton in StreamDeckButtons)
                {
                    if (streamDeckButton.Face != null)
                    {
                        streamDeckButton.Face.BackgroundColor = value;
                    }
                }

                _backgroundColor = value;
            }
        }

        public void SetEssentials(StreamDeckPanel streamDeckPanel)
        {
            foreach (var streamDeckButton in StreamDeckButtons)
            {
                if (streamDeckButton.FaceType == EnumStreamDeckFaceType.DCSBIOS)
                {
                    ((DCSBIOSDecoder)streamDeckButton.Face).SetEssentials(streamDeckPanel.InstanceId, streamDeckButton.StreamDeckButtonName);
                }
            }
        }

        public void AddButton(StreamDeckButton streamDeckButton)
        {
            _streamDeckButtons.Add(streamDeckButton);
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
