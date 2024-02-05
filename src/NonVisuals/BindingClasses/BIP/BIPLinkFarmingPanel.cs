using System;
using MEF;

namespace NonVisuals.BindingClasses.BIP
{
    [Serializable]
    public class BIPLinkFarmingPanel : BIPLinkBase
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
                // FarmingPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}

                var result = ParseSettingV1(settings);
                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            // FarmingPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }

            return GetExportString("FarmingPanelBIPLink", null, Enum.GetName(typeof(FarmingPanelMKKeys), FarmingPanelKey));
        }

        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
        }
    }
}
