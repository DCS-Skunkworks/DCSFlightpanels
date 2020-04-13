namespace NonVisuals.StreamDeck
{
    public enum LayerNavType
    {
        SwitchToSpecificLayer = 0,
        Back = 1,
        Home = 2
    }

    public class StreamDeckTargetLayer
    {
        public LayerNavType NavigationType;
        public string TargetLayer;
        private string _streamDeckInstanceId;


        public void Navigate(StreamDeckRequisites streamDeckRequisite)
        {
            if (streamDeckRequisite.StreamDeck == null)
            {
                return;
            }

            var streamDeck = streamDeckRequisite.StreamDeck;

            switch (NavigationType)
            {
                case LayerNavType.Home:
                    {
                        streamDeck.ShowHomeLayer();
                        break;
                    }
                case LayerNavType.Back:
                    {
                        streamDeck.ShowPreviousLayer();
                        break;
                    }
                case LayerNavType.SwitchToSpecificLayer:
                    {
                        streamDeck.ActiveLayer = TargetLayer;
                        break;
                    }
            }
        }
    }
}
