using System;
using System.IO;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck.Events;

namespace NonVisuals.StreamDeck
{
    public enum LayerNavType
    {
        None = 3, //Do not change value because of JSON.
        SwitchToSpecificLayer = 0,
        Back = 1,
        Home = 2
    }

    [Serializable]
    public class ActionTypeLayer : IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.LayerNavigation;
        public bool IsRepeatable() => false;
        private volatile bool _isRunning;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        public LayerNavType NavigationType;
        public string TargetLayer;
        public string RemoteStreamdeckBindingHash = "";
        public string RemoteStreamdeckTargetLayer = "";

        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;

        public ActionTypeLayer(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        public int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                result = (result * 397) ^ NavigationType.GetHashCode();
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
                    stringBuilder.Append(" ").Append(TargetLayer);
                }

                return stringBuilder.ToString();
            }
        }

        public bool IsRunning()
        {
            return _isRunning;
        }


        public void Execute(CancellationToken threadCancellationToken)
        {
            _isRunning = true;
            Common.PlaySoundFile(false, SoundFile, Volume);
            Navigate(threadCancellationToken);
            _isRunning = false;
        }
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
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
                        _streamDeckPanel.SelectedLayerName = TargetLayer;
                        break;
                    }
            }

            if (!string.IsNullOrEmpty(RemoteStreamdeckBindingHash) && !string.IsNullOrEmpty(RemoteStreamdeckTargetLayer))
            {
                EventHandlers.RemoteLayerSwitch(this, RemoteStreamdeckBindingHash, RemoteStreamdeckTargetLayer);
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

        public string SoundFile { get; set; }
        public double Volume { get; set; }
        public int Delay { get; set; }
        public bool HasSound => string.IsNullOrEmpty(SoundFile) && File.Exists(SoundFile);
        public void PlaySound()
        {
            Common.PlaySoundFile(false, SoundFile, Volume);
        }
    }
}
