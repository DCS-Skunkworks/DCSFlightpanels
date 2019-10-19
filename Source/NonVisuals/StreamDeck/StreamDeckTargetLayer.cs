namespace NonVisuals.StreamDeck
{
    public enum LayerNavType 
    {
        SwitchToSpecificLayer,
        Back
    }

    public class StreamDeckTargetLayer
    {
        public LayerNavType NavigationType;
        public string TargetLayer;
    }
}
