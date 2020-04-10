using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class LayerBindingStreamDeck : IStreamDeckButtonAction
    {
        public StreamDeckTargetLayer LayerTarget { get; set; }
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.LayerNavigation;
        public bool IsRepeatable() => false;
        private volatile bool _isRunning;


        public string Description { get => "Layer Navigation"; }

        public bool IsRunning()
        {
            return _isRunning;
        }


        public void Execute(StreamDeckRequisites streamDeckRequisite)
        {
            _isRunning = true;
            LayerTarget.Navigate(streamDeckRequisite);
            _isRunning = false;
        }

    }
}
