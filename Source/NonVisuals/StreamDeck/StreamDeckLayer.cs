using NonVisuals.StreamDeck.Panels;

namespace NonVisuals.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using MEF;

    using Newtonsoft.Json;

    using NonVisuals.Interfaces;
    using NonVisuals.StreamDeck.Events;

    public class StreamDeckLayer : IDisposable
    {
        private volatile bool _isVisible;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;

        [JsonProperty("Name", Required = Required.Default)]
        public string Name { get; set; } = string.Empty;
        public List<StreamDeckButton> StreamDeckButtons { get; set; } = new List<StreamDeckButton>();

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                StreamDeckButtons.ForEach(button => button.IsVisible = value);
            }
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set
            {
                _streamDeckPanel = value;
                StreamDeckButtons.ForEach(button => button.StreamDeckPanelInstance = value);
            }
        }

        [JsonIgnore]
        public bool HasAtLeastOneButtonConfig
        {
            get
            {
                return StreamDeckButtons.Any(o => o.HasConfig);
            }
        }

        [JsonIgnore]
        public bool HasButtons
        {
            get
            {
                return StreamDeckButtons.Count > 0;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamDeckPanel"></param>
        public StreamDeckLayer(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }


        public void Dispose()
        {
            foreach (var streamDeckButton in StreamDeckButtons)
            {
                streamDeckButton?.Dispose();
            }
        }

        public void ImportButtons(EnumButtonImportMode importMode, List<ButtonExport> buttonExports)
        {
            var streamDeckButtons = buttonExports.Where(o => o.LayerName == Name).Select(m => m.Button).ToList();
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

                foreach (var oldStreamDeckButton in StreamDeckButtons)
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
                    StreamDeckButtons.Add(newStreamDeckButton);
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
            SDEventHandler.NotifyStreamDeckConfigurationChange(this, _streamDeckPanel.BindingHash);
        }

        public void RegisterStreamDeckButtons()
        {
            StreamDeckButtons.ForEach(button => button.RegisterButtonToStaticList());
        }

        [JsonProperty("TextFont", Required = Required.Default)]
        public Font TextFont
        {
            set
            {
                foreach (var streamDeckButton in StreamDeckButtons.Where(x => x.Face != null))
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
        }

        [JsonProperty("FontColor", Required = Required.Default)]
        public Color FontColor
        {
            set
            {
                foreach (var streamDeckButton in StreamDeckButtons.Where(x=> x.Face != null))
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
        }

        [JsonProperty("BackgroundColor", Required = Required.Default)]
        public Color BackgroundColor
        {
            set
            {
                foreach (var streamDeckButton in StreamDeckButtons.Where(x => x.Face != null))
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
        }

        public void AddButton(StreamDeckButton streamDeckButton, bool silently = false)
        {
            streamDeckButton.RegisterButtonToStaticList();
            streamDeckButton.IsVisible = _isVisible;

            var found = false;
            foreach (var button in StreamDeckButtons)
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
                StreamDeckButtons.Add(streamDeckButton);
            }

            if (!silently)
            {
                NotifyChanges();
            }
        }


        public void RemoveButton(StreamDeckButton streamDeckButton)
        {
            streamDeckButton.Dispose();
            StreamDeckButtons.Remove(streamDeckButton);
            NotifyChanges();
        }

        public void RemoveButtons(bool sendNotification)
        {
            StreamDeckButtons.ForEach(button => button.Dispose());

            StreamDeckButtons.RemoveAll(o => o != null);

            if (sendNotification)
            {
                NotifyChanges();
            }
        }

        public void RemoveEmptyButtons()
        {
            foreach (var streamDeckButton in StreamDeckButtons.Where(o => o.HasConfig == false))
            {
                streamDeckButton.Dispose();
            }

            StreamDeckButtons.RemoveAll(o => !o.HasConfig);
        }

        public List<StreamDeckButton> GetButtonsWithConfig()
        {
            return StreamDeckButtons.Where(o => o.HasConfig).ToList();
        }

        public StreamDeckButton GetStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in StreamDeckButtons)
            {
                if (streamDeckButton.StreamDeckButtonName == streamDeckButtonName)
                {
                    return streamDeckButton;
                }
            }

            var newButton = new StreamDeckButton(streamDeckButtonName, _streamDeckPanel);
            StreamDeckButtons.Add(newButton);
            return newButton;
        }

        public bool ContainStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return StreamDeckButtons.Exists(x => x.StreamDeckButtonName == streamDeckButtonName);
        }

        public StreamDeckButton GetStreamDeckButtonName(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            foreach (var streamDeckButton in StreamDeckButtons)
            {
                if (streamDeckButton.StreamDeckButtonName == streamDeckButtonName)
                {
                    return streamDeckButton;
                }
            }

            throw new Exception($"StreamDeckLayer [{Name}] does not contain button [{streamDeckButtonName}].");
        }

    }

    public enum EnumButtonImportMode
    {
        None,
        Overwrite,
        Replace
    }
}
