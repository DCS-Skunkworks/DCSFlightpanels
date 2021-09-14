namespace NonVisuals.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;

    [Serializable]
    public class StreamDeckButton : IDisposable
    {
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private IStreamDeckButtonFace _buttonFace;
        private IStreamDeckButtonAction _buttonActionForPress;
        private IStreamDeckButtonAction _buttonActionForRelease;
        [NonSerialized] private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        [NonSerialized] private Thread _keyPressedThread;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;
        private volatile bool _isVisible;

        [NonSerialized] private static List<StreamDeckButton> _staticStreamDeckButtons = new List<StreamDeckButton>();








        public StreamDeckButton(EnumStreamDeckButtonNames enumStreamDeckButton, StreamDeckPanel streamDeckPanel)
        {
            _streamDeckButtonName = enumStreamDeckButton;
            _streamDeckPanel = streamDeckPanel;
        }

        /*
         * Used for easier access to the buttons instead of having to go through the layer
         */
        public void RegisterButtonToStaticList()
        {
            if (_staticStreamDeckButtons.Exists(o => o == this))
            {
                return;
            }

            _staticStreamDeckButtons.Add(this);
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                _staticStreamDeckButtons.Remove(this);
                IsVisible = false;
                _buttonFace?.Dispose();
                _buttonActionForPress = null;
                _buttonActionForRelease = null;
                _staticStreamDeckButtons.Remove(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StreamDeckButton()
        {
            Dispose(false);
        }

        public static StreamDeckButton GetStatic(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return _staticStreamDeckButtons.Find(o => o.StreamDeckButtonName == streamDeckButtonName);
        }

        public static void DisposeAll()
        {
            for (var i = 0; i < _staticStreamDeckButtons.Count; i++)
            {
                var streamDeckButton = _staticStreamDeckButtons[i];
                streamDeckButton.Dispose();
            }
        }

        public static List<StreamDeckButton> WarningGetStaticButtons()
        {
            return _staticStreamDeckButtons;
        }

        public static List<StreamDeckButton> GetStaticButtons(StreamDeckPanel streamDeckPanel)
        {
            if (streamDeckPanel == null)
            {
                return _staticStreamDeckButtons;
            }

            return _staticStreamDeckButtons.FindAll(o => o.StreamDeckPanelInstance.BindingHash == streamDeckPanel.BindingHash).ToList();
        }

        public void DoPress()
        {
            if (ActionForPress == null)
            {
                /*
                 * Must do this here as there are no ActionTypeKey for this button, otherwise Plugin would never get any event.
                 * Otherwise it is sent from ActionTypeKey together with key configs associated with the button.
                 */
                if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    PluginManager.DoEvent(
                        ProfileHandler.SelectedProfile().Description, 
                        StreamDeckPanelInstance.HIDInstanceId, 
                        (int)StreamDeckCommon.ConvertEnum(this._streamDeckPanel.TypeOfPanel),
                        (int)StreamDeckButtonName, 
                        true, 
                        null);
                }

                return;
            }

            while (ActionForPress.IsRunning())
            {
                _cancellationTokenSource.Cancel();
            }

            if (ActionForPress.IsRepeatable())
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var threadCancellationToken = _cancellationTokenSource.Token;
                _keyPressedThread = new Thread(() => ThreadedPress(threadCancellationToken));
                _keyPressedThread.Start();
            }
            else
            {
                ActionForPress.Execute(CancellationToken.None);
            }
        }

        public void ClearConfiguration()
        {
            _buttonActionForPress = null;
            _buttonActionForRelease = null;
            _buttonFace?.Dispose();
            _buttonFace = null;
        }

        public void ClearFace()
        {
            _streamDeckPanel.ClearFace(_streamDeckButtonName);
        }

        private void ThreadedPress(CancellationToken threadCancellationToken)
        {
            var first = true;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                if (!ActionForPress.IsRunning())
                {
                    ActionForPress?.Execute(threadCancellationToken);
                }


                if (first)
                {
                    Thread.Sleep(500);
                    first = false;
                }
                else
                {
                    Thread.Sleep(25);
                }
            }
        }

        public void DoRelease(CancellationToken threadCancellationToken)
        {
            _cancellationTokenSource.Cancel();

            if (ActionForRelease == null)
            {/*
                 * Must do this here as there are no ActionTypeKey for this button, otherwise Plugin would never get any event
                 */
                if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    var pluginPanel = PluginGamingPanelEnum.Unknown;

                    switch (this._streamDeckPanel.TypeOfPanel)
                    {
                        case GamingPanelEnum.StreamDeckMini:
                            {
                                pluginPanel = PluginGamingPanelEnum.StreamDeckMini;
                                break;
                            }

                        case GamingPanelEnum.StreamDeck:
                            {
                                pluginPanel = PluginGamingPanelEnum.StreamDeck;
                                break;
                            }

                        case GamingPanelEnum.StreamDeckV2:
                            {
                                pluginPanel = PluginGamingPanelEnum.StreamDeckV2;
                                break;
                            }

                        case GamingPanelEnum.StreamDeckMK2:
                            {
                                pluginPanel = PluginGamingPanelEnum.StreamDeckMK2;
                                break;
                            }

                        case GamingPanelEnum.StreamDeckXL:
                            {
                                pluginPanel = PluginGamingPanelEnum.StreamDeckXL;
                                break;
                            }
                    }

                    PluginManager.DoEvent(
                        ProfileHandler.SelectedProfile().Description,
                        StreamDeckPanelInstance.HIDInstanceId,
                        (int)pluginPanel,
                        (int)StreamDeckButtonName,
                        false,
                        null);
                }

                return;
            }

            if (!ActionForRelease.IsRunning())
            {
                ActionForRelease?.Execute(threadCancellationToken);
            }
        }

        public bool CheckIfWouldOverwrite(StreamDeckButton newStreamDeckButton)
        {
            var result = _buttonFace != null && newStreamDeckButton.Face != null ||
                         _buttonActionForPress != null && newStreamDeckButton.ActionForPress != null ||
                         _buttonActionForRelease != null && newStreamDeckButton.ActionForRelease != null;
            return result;
        }

        public void Consume(StreamDeckButton newStreamDeckButton)
        {
            if (!this.Equals(newStreamDeckButton))
            {
                ClearConfiguration();
            }

            ActionForPress = newStreamDeckButton.ActionForPress;
            ActionForRelease = newStreamDeckButton.ActionForRelease;
            Face = newStreamDeckButton.Face;
            if (Face != null)
            {
                Face.StreamDeckButtonName = _streamDeckButtonName;
            }
        }

        public bool Consume(bool overwrite, StreamDeckButton streamDeckButton)
        {
            var result = false;

            if (_buttonFace != null && streamDeckButton.Face != null)
            {
                if (overwrite)
                {
                    Face?.Dispose();
                    Face = streamDeckButton.Face;
                    Face.StreamDeckButtonName = _streamDeckButtonName;
                    result = true;
                }
            }
            else if (_buttonFace == null && streamDeckButton.Face != null)
            {
                Face = streamDeckButton.Face;
                Face.StreamDeckButtonName = _streamDeckButtonName;
                result = true;
            }

            if (_buttonActionForPress != null && streamDeckButton.ActionForPress != null)
            {
                if (overwrite)
                {
                    _buttonActionForPress = streamDeckButton.ActionForPress;
                    result = true;
                }
            }
            else if (_buttonActionForPress == null && streamDeckButton.ActionForPress != null)
            {
                _buttonActionForPress = streamDeckButton.ActionForPress;
                result = true;
            }


            if (_buttonActionForRelease != null && streamDeckButton.ActionForRelease != null)
            {
                if (overwrite)
                {
                    _buttonActionForRelease = streamDeckButton.ActionForRelease;
                    result = true;
                }
            }
            else if (_buttonActionForRelease == null && streamDeckButton.ActionForRelease != null)
            {
                _buttonActionForRelease = streamDeckButton.ActionForRelease;
                result = true;
            }

            return result;
        }

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                if (_buttonFace != null)
                {
                    _buttonFace.IsVisible = value;
                }
            }
        }

        [JsonProperty("Description", Required = Required.Default)]
        public string Description
        {
            get
            {
                var stringBuilder = new StringBuilder();
                if (ActionForPress != null)
                {
                    stringBuilder.Append("ActionPress : ").Append(_buttonActionForPress.ActionDescription).Append(" ");
                }

                if (ActionForRelease != null)
                {
                    stringBuilder.Append("ActionRelease : ").Append(_buttonActionForRelease.ActionDescription).Append(" ");
                }

                if (Face != null)
                {
                    stringBuilder.Append(Face.FaceDescription).Append(" ");
                }

                return stringBuilder.ToString();
            }
        }

        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        [JsonProperty("Face", Required = Required.Default)]
        public IStreamDeckButtonFace Face
        {
            get => _buttonFace;
            set => _buttonFace = value;
        }

        [JsonProperty("ActionForPress", Required = Required.Default)]
        public IStreamDeckButtonAction ActionForPress
        {
            get => _buttonActionForPress;
            set => _buttonActionForPress = value;
        }

        [JsonProperty("ActionForRelease", Required = Required.Default)]
        public IStreamDeckButtonAction ActionForRelease
        {
            get => _buttonActionForRelease;
            set => _buttonActionForRelease = value;
        }

        [JsonProperty("HasConfig", Required = Required.Default)]
        public bool HasConfig =>
            _buttonFace != null ||
            _buttonActionForPress != null ||
            _buttonActionForRelease != null;


        [JsonProperty("ActionType", Required = Required.Default)]
        public EnumStreamDeckActionType ActionType
        {
            get
            {
                var result = EnumStreamDeckActionType.Unknown;
                if (ActionForPress != null)
                {
                    result = ActionForPress.ActionType;
                }
                else if (ActionForRelease != null)
                {
                    result = ActionForRelease.ActionType;
                }

                return result;
            }
        }

        [JsonProperty("FaceType", Required = Required.Default)]
        public EnumStreamDeckFaceType FaceType
        {
            get
            {
                var result = EnumStreamDeckFaceType.Unknown;
                if (Face != null)
                {
                    result = Face.FaceType;
                }

                return result;
            }
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set
            {
                _streamDeckPanel = value;
                if (_buttonFace != null)
                {
                    _buttonFace.StreamDeckPanelInstance = value;
                }

                if (_buttonActionForPress != null)
                {
                    _buttonActionForPress.StreamDeckPanelInstance = value;
                }

                if (_buttonActionForRelease != null)
                {
                    _buttonActionForRelease.StreamDeckPanelInstance = value;
                }
            }
        }

        public int GetHash()
        {
            unchecked
            {
                var result = _buttonFace?.GetHash() ?? 0;
                result = (result * 397) ^ (_buttonActionForPress?.GetHash() ?? 0);
                result = (result * 397) ^ (_buttonActionForRelease?.GetHash() ?? 0);
                result = (result * 397) ^ StreamDeckButtonName.GetHashCode();
                return result;
            }
        }
    }




}
