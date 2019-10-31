using System.Threading;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class LayerBindingStreamDeck : IStreamDeckButtonAction
    {
        public StreamDeckTargetLayer LayerTarget { get; set; }
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.LayerNavigation;








        public void Execute(StreamDeckRequisites streamDeckRequisite)
        {
            LayerTarget.Navigate(streamDeckRequisite);
        }
        
        
    }
}
