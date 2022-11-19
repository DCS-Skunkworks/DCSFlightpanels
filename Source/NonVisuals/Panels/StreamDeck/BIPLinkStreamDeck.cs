using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Panels.StreamDeck
{
    using System.Linq;

    using MEF;

    using Newtonsoft.Json;

    public class BIPLinkStreamDeck : BIPLinkBase
    {
        /*
         This class binds a physical key on a Stream Deck with a BIP LED
         */

        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        public string Layer { get; set; } = string.Empty;

        public override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return string.Empty;
        }

        private int GetNewKeyValue()
        {
            if (_bipLights.Count == 0)
            {
                return 0;
            }

            return _bipLights.Keys.Max() + 1;
        }

    }
}
