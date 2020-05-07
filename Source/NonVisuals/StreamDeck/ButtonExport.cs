namespace NonVisuals.StreamDeck
{
    public class ButtonExport
    {
        public StreamDeckLayer Layer { get; set; }
        public StreamDeckButton Button { get; set; }

        public ButtonExport(StreamDeckLayer layer, StreamDeckButton button)
        {
            Layer = layer;
            Button = button;
        }

        public string LayerName
        {
            get => Layer.Name;
        }

        public EnumStreamDeckButtonNames ButtonName
        {
            get => Button.StreamDeckButtonName;
        }

        public string ButtonDescription
        {
            get => Button.Description;
        }
    }
}
