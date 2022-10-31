namespace NonVisuals.Saitek.BindingClasses
{
    using System;
    using System.Linq;
    using System.Text;

    using MEF;

    [Serializable]
    public class BIPLinkTPM : BIPLinkBase
    {
        private TPMPanelSwitches _tpmSwitch;

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkTPM)");
            }

            if (settings.StartsWith("TPMPanelBipLink{"))
            {
                var result = ParseSettingV1(settings);
                _tpmSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            // TPMPanelBipLink{1G1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }

            return GetExportString("TPMPanelBipLink", null, Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch));
        }
        
        public TPMPanelSwitches TPMSwitch
        {
            get => _tpmSwitch;
            set => _tpmSwitch = value;
        }
    }
}
