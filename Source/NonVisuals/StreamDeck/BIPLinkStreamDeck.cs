using System;
using System.Linq;
using System.Text;

namespace NonVisuals
{
    public class BIPLinkStreamDeck : BIPLink
    {
        /*
         This class binds a physical key on a Stream Deck with a BIP LED
         */
        private StreamDeckButtons _streamDeckButton;
        private string _layer = "";

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkStreamDeck)");
            }
            if (settings.StartsWith("StreamDeckBIPLink{"))
            {
                //StreamDeckBIPLink{Home Layer|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                // 0 1 2 3
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //StreamDeckBIPLink{Home Layer|1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("StreamDeckBIPLink{", "").Replace("}", "").Trim();
                //Home Layer|1KNOB_ENGINE_LEFT
                var param0Split = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                Layer = param0Split[0];

                _whenOnTurnedOn = param0Split[1].Substring(0, 1) == "1";
                param0Split[1] = param0Split[1].Substring(1);
                StreamDeckButton = (StreamDeckButtons)Enum.Parse(typeof(StreamDeckButtons), param0Split[1]);
                
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
            //StreamDeckBIPLink{Home Layer|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }
            var onStr = _whenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("StreamDeckBIPLink{" + Layer + "|" + onStr + Enum.GetName(typeof(StreamDeckButtons), StreamDeckButton) + "}");
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
        
        public StreamDeckButtons StreamDeckButton
        {
            get => _streamDeckButton;
            set => _streamDeckButton = value;
        }

        public string Layer
        {
            get => _layer;
            set => _layer = value;
        }
    }
}
