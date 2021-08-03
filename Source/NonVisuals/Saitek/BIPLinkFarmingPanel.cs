using System;
using System.Linq;
using System.Text;
using NonVisuals.Saitek.Switches;

namespace NonVisuals.Saitek
{
    [Serializable]
    public class BIPLinkFarmingPanel : BIPLink
    {
        /*
         This class binds a physical switch on the Farming Simulator Side Panel with a BIP LED
         */
        private FarmingPanelMKKeys _farmingPanelKey;
        
        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkFarmingPanel)");
            }
            if (settings.StartsWith("FarmingPanelBIPLink{"))
            {
                //FarmingPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                // 0 1 2 3
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                //FarmingPanelBIPLink{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("FarmingPanelBIPLink{", "").Replace("}", "").Trim();
                //1KNOB_ENGINE_LEFT
                WhenOnTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), param0);

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].StartsWith("BIPLight"))
                    {
                        var tmpBipLight = new BIPLight();
                        _bipLights.Add(GetNewKeyValue(), tmpBipLight);
                        tmpBipLight.ImportSettings(parameters[i]);
                    }
                    if (parameters[i].StartsWith("Description["))
                    {
                        var tmp = parameters[i].Replace("Description[", "").Replace("]", "");
                        _description = tmp;
                    }
                }
            }
        }

        public override string ExportSettings()
        {
            //FarmingPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }
            var onStr = WhenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("FarmingPanelBIPLink{" + onStr + Enum.GetName(typeof(FarmingPanelMKKeys), FarmingPanelKey) + "}");
            foreach (var bipLight in _bipLights)
            {
                stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + bipLight.Value.ExportSettings());
            }

            if (!string.IsNullOrWhiteSpace(_description))
            {
                stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + "Description[" + _description + "]");
            }
            return stringBuilder.ToString();
        }

        private int GetNewKeyValue()
        {
            if (_bipLights.Count == 0)
            {
                return 0;
            }
            return _bipLights.Keys.Max() + 1;
        }
        
        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
        }
    }
}
