using System;
using Newtonsoft.Json;

namespace NonVisuals.StreamDeck
{

    [Serializable]
    public class ButtonExport
    {
        public string LayerName { get; set; }
        public StreamDeckButton Button { get; set; }

        public ButtonExport(string layerName, StreamDeckButton button)
        {
            LayerName = layerName;
            Button = button;
        }
        
        [JsonIgnore]
        public EnumStreamDeckButtonNames ButtonName
        {
            get => Button.StreamDeckButtonName;
        }

        [JsonIgnore]
        public string ButtonDescription
        {
            get => Button.Description;
        }
    }
}
