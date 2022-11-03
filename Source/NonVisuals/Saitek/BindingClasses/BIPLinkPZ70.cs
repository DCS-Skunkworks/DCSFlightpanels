namespace NonVisuals.Saitek.BindingClasses
{
    using System;
    using System.Linq;
    using System.Text;

    using MEF;

    using NonVisuals.Saitek.Panels;

    [Serializable]
    public class BIPLinkPZ70 : BIPLinkBase
    {
        /*
         This class binds a physical switch on the PZ55 with a BIP LED
         */
        public PZ70DialPosition DialPosition { get; set; }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob { get; set; }

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkPZ70)");
            }
            
            if (settings.StartsWith("TPMPanelBipLink{"))
            {
                var result = ParseSettingV1(settings);
                DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), result.Item1);
                MultiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            // MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }
            
            return GetExportString("MultipanelBIPLink", Enum.GetName(typeof(PZ70DialPosition), DialPosition), Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob));
        }
        
    }

}
