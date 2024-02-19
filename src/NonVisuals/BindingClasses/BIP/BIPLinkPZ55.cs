using System;
using ClassLibraryCommon;
using MEF;

namespace NonVisuals.BindingClasses.BIP
{
    [Serializable]
    [SerializeCriticalCustom]
    public class BIPLinkPZ55 : BIPLinkBase
    {
        /*
         This class binds a physical switch on the PZ55 with a BIP LED
         */
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkPZ55)");
            }

            if (settings.StartsWith("SwitchPanelBIPLink{"))
            {
                // SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}

                var result = ParseSettingV1(settings);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            // SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }

            return GetExportString("SwitchPanelBIPLink", null, Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key));
        }


        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }
    }
}
