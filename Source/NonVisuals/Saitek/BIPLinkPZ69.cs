using System;
using System.Linq;
using System.Text;
using NonVisuals.Radios;

namespace NonVisuals.Saitek
{
    public class BIPLinkPZ69 : BIPLink
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
                //RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                // 0 1 2 3
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                //RadioPanelBIPLink{1UpperCOM1}
                var param0 = parameters[0].Replace("RadioPanelBIPLink{", "").Replace("}", "").Trim();
                //1UpperCOM1
                WhenOnTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param0);

                for (var i = 0; i < parameters.Length; i++)
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
            //RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }
            var onStr = WhenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("RadioPanelBIPLink{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "}");
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

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }
    }
}
