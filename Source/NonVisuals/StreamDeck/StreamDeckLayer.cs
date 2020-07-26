using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck.Events;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayer
    {
        private string _name = "";
        private List<StreamDeckButton> _streamDeckButtons = new List<StreamDeckButton>();
        private Font _textFont;
        private Color _fontColor;
        private Color _backgroundColor;
        private volatile bool _isVisible = false;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;

        public StreamDeckLayer(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        public void ImportButtons(EnumButtonImportMode importMode, List<ButtonExport> buttonExports)
        {
            var streamDeckButtons = buttonExports.Where(o => o.LayerName == _name).Select(m => m.Button).ToList();
            if (streamDeckButtons.Count > 0)
            {
                ImportButtons(importMode, streamDeckButtons);
            }

            RegisterStreamDeckButtons();
        }
        
        public void ImportButtons(EnumButtonImportMode importMode, List<StreamDeckButton> newStreamDeckButtons)
        {
            var changesMade = false;
            foreach (var newStreamDeckButton in newStreamDeckButtons)
            {
                var found = false;

                newStreamDeckButton.StreamDeckPanelInstance = _streamDeckPanel;

                foreach (var oldStreamDeckButton in _streamDeckButtons)
                {
                    if (oldStreamDeckButton.StreamDeckButtonName == newStreamDeckButton.StreamDeckButtonName)
                    {
                        found = true;

                        if (importMode == EnumButtonImportMode.Replace)
                        {
                            oldStreamDeckButton.ClearConfiguration();
                            oldStreamDeckButton.Consume(true, newStreamDeckButton);
                            // Let propagate down so it isn't null
                            oldStreamDeckButton.StreamDeckPanelInstance = _streamDeckPanel;

                            changesMade = true;
                        }
                        else if (importMode == EnumButtonImportMode.Overwrite)
                        {
                            oldStreamDeckButton.Consume(true, newStreamDeckButton);
                            // Let propagate down so it isn't null
                            oldStreamDeckButton.StreamDeckPanelInstance = _streamDeckPanel;

                            changesMade = true;
                        }
                        else if (importMode == EnumButtonImportMode.None)
                        {
                            if (oldStreamDeckButton.Face == null && newStreamDeckButton.Face != null)
                            {
                                var face = newStreamDeckButton.Face.DeepClone();
                                face.AfterClone();
                                oldStreamDeckButton.Face = face;
                                changesMade = true;
                            }
                            if (oldStreamDeckButton.ActionForPress == null && newStreamDeckButton.ActionForPress != null)
                            {
                                oldStreamDeckButton.ActionForPress = newStreamDeckButton.ActionForPress.DeepClone();

                                changesMade = true;
                            }
                            if (oldStreamDeckButton.ActionForRelease == null && newStreamDeckButton.ActionForRelease != null)
                            {
                                oldStreamDeckButton.ActionForRelease = newStreamDeckButton.ActionForRelease.DeepClone();

                                changesMade = true;
                            }
                            // Let propagate down so it isn't null
                            oldStreamDeckButton.StreamDeckPanelInstance = _streamDeckPanel;
                        }

                        break;
                    }
                }

                if (!found)
                {
                    _streamDeckButtons.Add(newStreamDeckButton);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                NotifyChanges();
            }
        }

        private void NotifyChanges()
        {
            EventHandlers.NotifyStreamDeckConfigurationChange(this, _streamDeckPanel.BindingHash);
        }

        public void RegisterStreamDeckButtons()
        {
            foreach (var streamDeckButton in _streamDeckButtons)
            {
                streamDeckButton.RegisterButtonToStaticList();
            }
        }

        public Font TextFont
        {
            set
            {
                foreach (var streamDeckButton in _streamDeckButtons)
                {
                    if (streamDeckButton.Face != null)
                    {
                        switch (streamDeckButton.Face.FaceType)
                        {
                            case EnumStreamDeckFaceType.DCSBIOS:
                            case EnumStreamDeckFaceType.Text:
                                {
                                    ((IFontFace)streamDeckButton.Face).TextFont = value;
                                    break;
                                }
                        }

                    }
                }

                _textFont = value;
            }
        }

        public Color FontColor
        {
            set
            {
                foreach (var streamDeckButton in _streamDeckButtons)
                {
                    if (streamDeckButton.Face != null)
                    {

                        switch (streamDeckButton.Face.FaceType)
                        {
                            case EnumStreamDeckFaceType.DCSBIOS:
                            case EnumStreamDeckFaceType.Text:
                                {
                                    ((IFontFace)streamDeckButton.Face).FontColor = value;
                                    break;
                                }
                        }
                    }
                }

                _fontColor = value;
            }
        }

        public Color BackgroundColor
        {
            set
            {
                foreach (var streamDeckButton in _streamDeckButtons)
                {
                    if (streamDeckButton.Face != null)
                    {
                        switch (streamDeckButton.Face.FaceType)
                        {
                            case EnumStreamDeckFaceType.DCSBIOS:
                            case EnumStreamDeckFaceType.Text:
                                {
                                    ((IFontFace)streamDeckButton.Face).BackgroundColor = value;
                                    break;
                                }
                        }
                    }
                }

                _backgroundColor = value;
            }
        }

        public void AddButton(StreamDeckButton streamDeckButton, bool silently = false)
        {
            streamDeckButton.RegisterButtonToStaticList();
            streamDeckButton.IsVisible = _isVisible;

            var found = false;
            foreach (var button in _streamDeckButtons)
            {
                if (button.StreamDeckButtonName == streamDeckButton.StreamDeckButtonName)
                {
                    button.Consume(streamDeckButton);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _streamDeckButtons.Add(streamDeckButton);
            }

            if (!silently)
            {
                NotifyChanges();
            }
        }


        public void RemoveButton(StreamDeckButton streamDeckButton)
        {
            streamDeckButton.Dispose();
            _streamDeckButtons.Remove(streamDeckButton);
            NotifyChanges();
        }

        public void RemoveButtons(bool sendNotification)
        {
            foreach (var streamDeckButton in _streamDeckButtons)
            {
                streamDeckButton.Dispose();
            }

            _streamDeckButtons.RemoveAll(o => o != null);

            if (sendNotification)
            {
                NotifyChanges();
            }
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public void RemoveEmptyButtons()
        {
            foreach (var streamDeckButton in _streamDeckButtons.Where(o => o.HasConfig == false))
            {
                streamDeckButton.Dispose();
            }

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

        [JsonIgnore]
        public bool HasButtons
        {
            get
            {
                return _streamDeckButtons.Count > 0;
            }
        }

        public List<StreamDeckButton> GetButtonsWithConfig()
        {
            return (List<StreamDeckButton>)_streamDeckButtons.Where(o => o.HasConfig).ToList();
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
            var newButton = new StreamDeckButton(streamDeckButtonName, _streamDeckPanel);
            _streamDeckButtons.Add(newButton);
            return newButton;
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

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                foreach (var streamDeckButton in _streamDeckButtons)
                {
                    streamDeckButton.IsVisible = _isVisible;
                    streamDeckButton.IsVisible = _isVisible;
                }
            }
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set
            {
                _streamDeckPanel = value;
                foreach (var streamDeckButton in _streamDeckButtons)
                {
                    streamDeckButton.StreamDeckPanelInstance = value;
                }
            }
        }

    }

    public enum EnumButtonImportMode
    {
        None,
        Overwrite,
        Replace
    }
}
