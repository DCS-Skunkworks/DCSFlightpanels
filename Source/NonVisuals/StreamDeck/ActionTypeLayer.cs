using System;
using System.Threading;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public enum LayerNavType
    {
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
        private string _panelHash;


        public int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                result = (result * 397) ^ NavigationType.GetHashCode();
                return result;
            }
        }

        public string Description { get => "Layer Navigation"; }

        public bool IsRunning()
        {
            return _isRunning;
        }


        public void Execute(CancellationToken threadCancellationToken)
        {
            _isRunning = true;
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
                case LayerNavType.Home:
                {
                    StreamDeckPanel.GetInstance(_panelHash).ShowHomeLayer();
                    break;
                }
                case LayerNavType.Back:
                {
                    StreamDeckPanel.GetInstance(_panelHash).ShowPreviousLayer();
                    break;
                }
                case LayerNavType.SwitchToSpecificLayer:
                {
                    StreamDeckPanel.GetInstance(_panelHash).SelectedLayerName = TargetLayer;
                    break;
                }
            }
        }

        [JsonIgnore]
        public string PanelHash
        {
            get => _panelHash;
            set => _panelHash = value;
        }
    }
}
