using System.Threading.Tasks;

namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using Interfaces;
    using Events;
    using Panels;

    public enum LayerNavType
    {
        None = 3, // Do not change value because of JSON.
        SwitchToSpecificLayer = 0,
        Back = 1,
        Home = 2
    }

    [Serializable]
    [SerializeCritical]
    public class ActionTypeLayer : IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.LayerNavigation;
        public bool IsRepeatable() => false;
        [JsonIgnore] public bool HasSequence => false;
        [JsonIgnore] public bool IsSequenced => false;
        private volatile bool _isRunning;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private EnumStreamDeckPushRotaryNames _streamDeckPushRotaryName;

        [JsonProperty("NavigationType", Required = Required.Default)]
        public LayerNavType NavigationType;
        [JsonProperty("TargetLayer", Required = Required.Default)]
        public string TargetLayer;
        [JsonProperty("RemoteStreamdeckBindingHash", Required = Required.Default)]
        public string RemoteStreamdeckBindingHash = string.Empty;
        [JsonProperty("RemoteStreamdeckTargetLayer", Required = Required.Default)]
        public string RemoteStreamdeckTargetLayer = string.Empty;

        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;

        public ActionTypeLayer() {
        }

        public ActionTypeLayer(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        private bool _disposed;
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            //base.Dispose(disposing);
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        public int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                result = result * 397 ^ NavigationType.GetHashCode();
                return result;
            }
        }

        [JsonIgnore]
        public string ActionDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Layer Nav.");
                if (!string.IsNullOrEmpty(TargetLayer))
                {
                    stringBuilder.Append(' ').Append(TargetLayer);
                }

                return stringBuilder.ToString();
            }
        }

        public bool IsRunning()
        {
            return _isRunning;
        }


        public Task ExecuteAsync(CancellationToken threadCancellationToken, bool executeOnce = false)
        {
            _isRunning = true;
            Common.PlaySoundFile(SoundFile, Volume);
            Navigate(threadCancellationToken);
            _isRunning = false;
            return Task.CompletedTask;
        }


        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        [JsonProperty("StreamDeckButtonPushRotaryName", Required = Required.Default)]
        public EnumStreamDeckPushRotaryNames StreamDeckPushRotaryName
        {
            get => _streamDeckPushRotaryName;
            set => _streamDeckPushRotaryName = value;
        }

        public void Navigate(CancellationToken threadCancellationToken)
        {
            switch (NavigationType)
            {
                case LayerNavType.None:
                    {
                        break;
                    }

                case LayerNavType.Home:
                    {
                        _streamDeckPanel.ShowHomeLayer();
                        break;
                    }

                case LayerNavType.Back:
                    {
                        _streamDeckPanel.ShowPreviousLayer();
                        break;
                    }

                case LayerNavType.SwitchToSpecificLayer:
                    {
                        _streamDeckPanel.SwitchToLayer(TargetLayer, true, false);
                        break;
                    }
            }

            if (!string.IsNullOrEmpty(RemoteStreamdeckBindingHash) && !string.IsNullOrEmpty(RemoteStreamdeckTargetLayer))
            {
                SDEventHandler.RemoteLayerSwitch(this, RemoteStreamdeckBindingHash, RemoteStreamdeckTargetLayer);
            }
        }

        [JsonIgnore]
        public bool ControlsRemoteStreamDeck
        {
            get => !string.IsNullOrEmpty(RemoteStreamdeckBindingHash) && !string.IsNullOrEmpty(RemoteStreamdeckTargetLayer);
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set => _streamDeckPanel = value;
        }


        [JsonProperty("SoundFile", Required = Required.Default)]
        public string SoundFile { get; set; }

        [JsonProperty("Volume", Required = Required.Default)]
        public double Volume { get; set; }

        [JsonProperty("Delay", Required = Required.Default)]
        public int Delay { get; set; }


        [Obsolete]
        [JsonProperty("HasSound", Required = Required.Default)]
        public bool HasSound => !string.IsNullOrEmpty(SoundFile) && File.Exists(SoundFile);
        public void PlaySound()
        {
            Common.PlaySoundFile(SoundFile, Volume);
        }
    }
}
