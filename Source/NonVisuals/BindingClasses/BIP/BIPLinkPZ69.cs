using System;
using MEF;
using NonVisuals.Panels.Saitek.Panels;

namespace NonVisuals.BindingClasses.BIP
{
    [Serializable]
    public class BIPLinkPZ69 : BIPLinkBase
    {
        /*
         This class binds a physical switch on the PZ55 with a BIP LED
         */
        public PZ70DialPosition DialPosition { get; set; }

        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkPZ69)");
            }

            if (settings.StartsWith("RadioPanelBIPLink{"))
            {
                // RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]

                var result = ParseSettingV1(settings);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            // RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }

            return GetExportString("RadioPanelBIPLink", null, Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob));
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }
    }
}
