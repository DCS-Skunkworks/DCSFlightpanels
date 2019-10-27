using System.Linq;
using NonVisuals.Saitek;

namespace NonVisuals.StreamDeck
{
    public class BIPLinkStreamDeck : BIPLink
    {
        /*
         This class binds a physical key on a Stream Deck with a BIP LED
         */

        public StreamDeckButtonNames StreamDeckButtonName { get; set; }
        public string Layer { get; set; } = "";

        public override void ImportSettings(string settings){}

        public override string ExportSettings()
        {
            return "";
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
