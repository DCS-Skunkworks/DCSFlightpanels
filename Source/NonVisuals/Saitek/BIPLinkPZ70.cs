using System;
using System.Linq;
using System.Text;

namespace NonVisuals.Saitek
{
    public class BIPLinkPZ70 : BIPLink
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
            if (settings.StartsWith("MultipanelBIPLink{"))
            {
                //MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                // 0 1 2 3
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("MultipanelBIPLink{", "").Replace("}", "").Trim();
                //ALT|1KNOB_ENGINE_LEFT
                var tmpArray = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                WhenOnTurnedOn = tmpArray[1].Substring(0, 1) == "1";
                MultiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), tmpArray[1].Substring(1));
                DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), tmpArray[0]);

                for (int i = 1; i < parameters.Length - 1; i++)
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
            //MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }
            var onStr = WhenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("MultipanelBIPLink{" + DialPosition + "|" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}");
            foreach (var bipLight in _bipLights)
            {
                stringBuilder.Append(SeparatorChars + bipLight.Value.ExportSettings());
            }

            if (!string.IsNullOrWhiteSpace(_description))
            {
                stringBuilder.Append(SeparatorChars + "Description[" + _description + "]");
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
        
    }

}
