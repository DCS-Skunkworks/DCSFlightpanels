using System.Diagnostics;
using NonVisuals.StreamDeck.Panels;

namespace NonVisuals.StreamDeck
{
    using System;
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
        [NonSerialized] private volatile CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        [NonSerialized] private Thread _keyPressedThread;
        [NonSerialized] private StreamDeckPanel _streamDeckPanel;
        private volatile bool _isVisible;



        

        /// <summary>
        /// For JSON
        /// </summary>
        public StreamDeckButton()
        { }

        /// <summary>
        /// This is used when creating buttons for the UI that wasn't in the JSON
        /// </summary>
        /// <param name="enumStreamDeckButton"></param>
        /// <param name="streamDeckPanel"></param>
        public StreamDeckButton(EnumStreamDeckButtonNames enumStreamDeckButton, StreamDeckPanel streamDeckPanel)
        {
            _streamDeckButtonName = enumStreamDeckButton;
            _streamDeckPanel = streamDeckPanel;
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
                IsVisible = false;
                _buttonFace?.Dispose();
                _buttonActionForPress?.Dispose();
                _buttonActionForRelease?.Dispose();
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
                        DCSFPProfile.SelectedProfile.Description,
                        StreamDeckPanelInstance.HIDInstance,
                        StreamDeckCommon.ConvertEnum(_streamDeckPanel.TypeOfPanel),
                        (int)StreamDeckButtonName,
                        true,
                        null);
                }

                return;
            }

            while (ActionForPress.IsRunning())
            {
                _cancellationTokenSource?.Cancel();
            }

            if (ActionForPress.IsRepeatable())
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var threadCancellationToken = _cancellationTokenSource.Token;
                Debug.WriteLine("Creating Key Press Thread for Streamdeck");
                _keyPressedThread = new Thread(() => ThreadedPress(threadCancellationToken));
                _keyPressedThread.Start();
            }
            else
            {
                ActionForPress.Execute(CancellationToken.None);
            }
        }

        private void ThreadedPress(CancellationToken threadCancellationToken)
        {
            var first = true;
            while (!threadCancellationToken.IsCancellationRequested)
            {
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

        public void DoRelease()
        {
            while (_keyPressedThread != null)
            {
                _cancellationTokenSource?.Cancel();
                _keyPressedThread = null;
            }

            if (ActionForRelease == null)
            {
                /*
                 * Must do this here as there are no ActionTypeKey for this button, otherwise Plugin would never get any event
                 */
                if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                {
                    PluginGamingPanelEnum pluginPanel = _streamDeckPanel.TypeOfPanel switch
                    {
                        GamingPanelEnum.StreamDeckMini => PluginGamingPanelEnum.StreamDeckMini,
                        GamingPanelEnum.StreamDeck => PluginGamingPanelEnum.StreamDeck,
                        GamingPanelEnum.StreamDeckV2 => PluginGamingPanelEnum.StreamDeckV2,
                        GamingPanelEnum.StreamDeckMK2 => PluginGamingPanelEnum.StreamDeckMK2,
                        GamingPanelEnum.StreamDeckXL => PluginGamingPanelEnum.StreamDeckXL,
                        _ => PluginGamingPanelEnum.Unknown
                    };

                    PluginManager.DoEvent(
                        DCSFPProfile.SelectedProfile.Description,
                        StreamDeckPanelInstance.HIDInstance,
                        pluginPanel,
                        (int)StreamDeckButtonName,
                        false,
                        null);
                }
                return;
            }

            while (ActionForRelease.IsRunning())
            {
                _cancellationTokenSource?.Cancel();
            }

            /*
             * Can ActionForRelease really be repeatable??
             */
            /*if (ActionForRelease.IsRepeatable())
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var threadCancellationToken = _cancellationTokenSource.Token;
                ActionForRelease?.Execute(threadCancellationToken);
            }
            else
            {*/
                ActionForRelease.Execute(CancellationToken.None);
            //}
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
            if (!Equals(newStreamDeckButton))
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
