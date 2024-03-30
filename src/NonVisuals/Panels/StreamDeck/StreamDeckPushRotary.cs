namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using Interfaces;
    using Plugin;
    using Panels;

    [Serializable]
    [SerializeCritical]
    public class StreamDeckPushRotary : IDisposable
    {
        private EnumStreamDeckPushRotaryNames _streamDeckPushRotaryName;
        private IStreamDeckButtonAction _buttonActionForPress;
        private IStreamDeckButtonAction _buttonActionForRelease;

        [NonSerialized] private volatile CancellationTokenSource _cancellationTokenSource = new();
        [NonSerialized] private StreamDeckPanel _streamDeckPanel;
        private volatile bool _isVisible;


        /// <summary>
        /// For JSON
        /// </summary>
        public StreamDeckPushRotary()
        { }

        /// <summary>
        /// This is used when creating buttons for the UI that wasn't in the JSON
        /// </summary>
        /// <param name="enumStreamDeckPushRotary"></param>
        /// <param name="streamDeckPanel"></param>
        public StreamDeckPushRotary(EnumStreamDeckPushRotaryNames enumStreamDeckPushRotary, StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPushRotaryName = enumStreamDeckPushRotary;
            _streamDeckPanel = streamDeckPanel;
        }

        private void ReleaseUnmanagedResources()
        {
            // release unmanaged resources here
        }


        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                IsVisible = false;
                _buttonActionForPress?.Dispose();
                _buttonActionForRelease?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StreamDeckPushRotary()
        {
            Dispose(false);
        }

        public void ClearConfiguration()
        {
            _buttonActionForPress = null;
            _buttonActionForRelease = null;
        }

        public void DoPress(bool executeOnce)
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
                        DCSAircraft.SelectedAircraft.Description,
                        StreamDeckPanelInstance.HIDInstance,
                        StreamDeckCommon.ConvertEnum(_streamDeckPanel.TypeOfPanel),
                        (int)StreamDeckPushRotaryName,
                        true,
                        null);
                }

                return;
            }

            /*
             * We have several options here.
             * 1) There is only 1 command configured and not repeatable => execute it without thread.
             * 2) There are multiple commands.
             *   2a) If Sequenced Execution is configured execute each one in the sequence
             *       when user keeps pressing the button. No Thread.
             *   2b) The multiple commands can be seen as a batch and should be all executed
             *       regardless if the user releases the button. Use Thread.
             *
             * 3) Command is repeatable, for single commands. Just keep executing them, use thread.
             *
             * Commands that must be cancelled by the release of the button are :
             * 1) Repeatable single command.
             */
            if (!executeOnce)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                if (ActionForPress.IsRepeatable() && !ActionForPress.HasSequence)
                {
                    ActionForPress.ExecuteAsync(_cancellationTokenSource.Token);
                }
                else
                {
                    ActionForPress.ExecuteAsync(CancellationToken.None);
                }
            }
            else
            {
                ActionForPress.ExecuteAsync(new CancellationToken(), true);
            }
        }

        public void DoRelease()
        {
            _cancellationTokenSource?.Cancel();

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
                        GamingPanelEnum.StreamDeckMiniV2 => PluginGamingPanelEnum.StreamDeckMini,
                        GamingPanelEnum.StreamDeck => PluginGamingPanelEnum.StreamDeck,
                        GamingPanelEnum.StreamDeckV2 => PluginGamingPanelEnum.StreamDeckV2,
                        GamingPanelEnum.StreamDeckMK2 => PluginGamingPanelEnum.StreamDeckMK2,
                        GamingPanelEnum.StreamDeckXL => PluginGamingPanelEnum.StreamDeckXL,
                        GamingPanelEnum.StreamDeckXLRev2 => PluginGamingPanelEnum.StreamDeckXL,
                        GamingPanelEnum.StreamDeckPlus => PluginGamingPanelEnum.StreamDeckPlus,
                        _ => PluginGamingPanelEnum.Unknown
                    };

                    PluginManager.DoEvent(
                        DCSAircraft.SelectedAircraft.Description,
                        StreamDeckPanelInstance.HIDInstance,
                        pluginPanel,
                        (int)StreamDeckPushRotaryName,
                        false,
                        null);
                }
                return;
            }

            while (ActionForRelease.IsRunning())
            {
                _cancellationTokenSource?.Cancel();
            }

            ActionForRelease.ExecuteAsync(CancellationToken.None);
        }

        public bool CheckIfWouldOverwrite(StreamDeckPushRotary newStreamDeckPushRotary)
        {
            var result = _buttonActionForPress != null && newStreamDeckPushRotary.ActionForPress != null ||
                         _buttonActionForRelease != null && newStreamDeckPushRotary.ActionForRelease != null;
            return result;
        }

        public void Consume(StreamDeckPushRotary newStreamDeckPushRotary)
        {
            if (!Equals(newStreamDeckPushRotary))
            {
                ClearConfiguration();
            }

            ActionForPress = newStreamDeckPushRotary.ActionForPress;
            ActionForRelease = newStreamDeckPushRotary.ActionForRelease;
        }

        public bool Consume(bool overwrite, StreamDeckPushRotary streamDeckPushRotary)
        {
            var result = false;

            if (_buttonActionForPress != null && streamDeckPushRotary.ActionForPress != null)
            {
                if (overwrite)
                {
                    _buttonActionForPress = streamDeckPushRotary.ActionForPress;
                    result = true;
                }
            }
            else if (_buttonActionForPress == null && streamDeckPushRotary.ActionForPress != null)
            {
                _buttonActionForPress = streamDeckPushRotary.ActionForPress;
                result = true;
            }


            if (_buttonActionForRelease != null && streamDeckPushRotary.ActionForRelease != null)
            {
                if (overwrite)
                {
                    _buttonActionForRelease = streamDeckPushRotary.ActionForRelease;
                    result = true;
                }
            }
            else if (_buttonActionForRelease == null && streamDeckPushRotary.ActionForRelease != null)
            {
                _buttonActionForRelease = streamDeckPushRotary.ActionForRelease;
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
            }
        }

        [Obsolete]
        [JsonIgnore]
        public string Description;


        [JsonProperty("StreamDeckPushRotaryName", Required = Required.Default)]
        public EnumStreamDeckPushRotaryNames StreamDeckPushRotaryName
        {
            get => _streamDeckPushRotaryName;
            set => _streamDeckPushRotaryName = value;
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

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set
            {
                _streamDeckPanel = value;

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
                var result = _buttonActionForPress?.GetHash() ?? 0;
                result = result * 397 ^ (_buttonActionForRelease?.GetHash() ?? 0);
                return result;
            }
        }
    }
}
