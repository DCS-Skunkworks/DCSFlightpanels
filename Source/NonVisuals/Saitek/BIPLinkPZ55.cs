namespace NonVisuals.Saitek
{
    using System;
    using System.Linq;
    using System.Text;

    using MEF;

    [Serializable]
    public class BIPLinkPZ55 : BIPLink
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
                // 0 1 2 3
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("SwitchPanelBIPLink{", string.Empty).Replace("}", string.Empty).Trim();

                // 1KNOB_ENGINE_LEFT
                WhenOnTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), param0);

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
                        var tmp = parameters[i].Replace("Description[", string.Empty).Replace("]", string.Empty);
                        _description = tmp;
                    }
                }
            }
        }

        public override string ExportSettings()
        {
            // SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }

            var onStr = WhenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("SwitchPanelBIPLink{" + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}");
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
        
        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }
    }
}
