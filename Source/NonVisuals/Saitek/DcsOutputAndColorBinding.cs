using System;
using System.Text;
using DCS_BIOS;

namespace NonVisuals.Saitek
{

    public enum PanelLEDColor : byte
    {
        DARK = 0x0,
        GREEN = 0x1,
        YELLOW = 0x2,
        RED = 0x4
    }

    public class SaitekPanelLEDPosition
    {
        private Enum _position;

        public SaitekPanelLEDPosition(Enum position)
        {
            _position = position;
        }

        public Enum GetPosition()
        {
            return _position;
        }

        public void SetPosition(Enum position)
        {
            _position = position;
        }

        public Enum Position
        {
            get => _position;
            set => _position = value;
        }
    }

    public abstract class DcsOutputAndColorBinding
    {
        private DCSBIOSOutput _dcsBiosOutputLED;

        public abstract void ImportSettings(string settings);
        public abstract string ExportSettings();

        public PanelLEDColor LEDColor { get; set; }
        public SaitekPanelLEDPosition SaitekLEDPosition { get; set; }

        protected string[] Separator { get; } = { "\\o/" };

        public DCSBIOSOutput DCSBiosOutputLED
        {
            get => _dcsBiosOutputLED;
            set => _dcsBiosOutputLED = value;
        }
    }



    public class DcsOutputAndColorBindingPZ55 : DcsOutputAndColorBinding
    {

        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }
            var color = "";
            if (settings.Contains("SwitchPanelLedUp"))
            {
                SaitekLEDPosition = new SaitekPanelLEDPosition(SwitchPanelPZ55LEDPosition.UP);
            }
            else if (settings.Contains("SwitchPanelLedLeft"))
            {
                SaitekLEDPosition = new SaitekPanelLEDPosition(SwitchPanelPZ55LEDPosition.LEFT);
            }
            else if (settings.Contains("SwitchPanelLedRight"))
            {
                SaitekLEDPosition = new SaitekPanelLEDPosition(SwitchPanelPZ55LEDPosition.RIGHT);
            }
            //SwitchPanelLedUp{DARK|DCSBiosOutput{INTEGER_TYPE|Equals|0x0000|0x0000|0|0}}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            settings = settings.Split(Separator, StringSplitOptions.RemoveEmptyEntries)[0];
            //SwitchPanelLedUp{DARK|DCSBiosOutput{INTEGER_TYPE|Equals|0x0000|0x0000|0|0}}
            settings = settings.Substring(settings.IndexOf("{", StringComparison.InvariantCulture) + 1);
            settings = settings.Substring(0, settings.Length - 1);
            color = settings.Substring(0, settings.IndexOf('|'));
            settings = settings.Substring(settings.IndexOf('|') + 1);
            DCSBiosOutputLED = new DCSBIOSOutput();
            DCSBiosOutputLED.ImportString(settings);
            LEDColor = (PanelLEDColor)Enum.Parse(typeof(PanelLEDColor), color);
        }

        public override string ExportSettings()
        {
            var stringBuilder = new StringBuilder();
            if ((SwitchPanelPZ55LEDPosition)SaitekLEDPosition.GetPosition() == SwitchPanelPZ55LEDPosition.UP)
            {
                stringBuilder.Append("SwitchPanelLedUp{" + LEDColor + "|" + DCSBiosOutputLED + "}");
            }
            if ((SwitchPanelPZ55LEDPosition)SaitekLEDPosition.GetPosition() == SwitchPanelPZ55LEDPosition.LEFT)
            {
                stringBuilder.Append("SwitchPanelLedLeft{" + LEDColor + "|" + DCSBiosOutputLED + "}");
            }
            if ((SwitchPanelPZ55LEDPosition)SaitekLEDPosition.GetPosition() == SwitchPanelPZ55LEDPosition.RIGHT)
            {
                stringBuilder.Append("SwitchPanelLedRight{" + LEDColor + "|" + DCSBiosOutputLED + "}");
            }
            return stringBuilder.ToString();
        }
    }

    public class DcsOutputAndColorBindingBIP : DcsOutputAndColorBinding
    {
        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }
            //PanelBIP{Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}}\o/\\?\hid#vid_06a3&pid_0b4e#9&1f079469&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            settings = settings.Split(Separator, StringSplitOptions.RemoveEmptyEntries)[0];
            //PanelBIP{Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}}
            settings = settings.Substring(settings.IndexOf("{", StringComparison.InvariantCulture) + 1);
            //Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}}
            settings = settings.Substring(0, settings.Length - 1);
            //Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}
            var dcsBiosOutputString = settings.Substring(settings.IndexOf("DCSBiosOutput{", StringComparison.InvariantCulture));
            //DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}

            settings = settings.Remove(settings.IndexOf("DCSBiosOutput{", StringComparison.InvariantCulture));
            //Position_1_4|GREEN|

            var settingsArray = settings.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            SaitekLEDPosition = new SaitekPanelLEDPosition((BIPLedPositionEnum)Enum.Parse(typeof(BIPLedPositionEnum), settingsArray[0]));
            LEDColor = (PanelLEDColor)Enum.Parse(typeof(PanelLEDColor), settingsArray[1]);
            DCSBiosOutputLED = new DCSBIOSOutput();
            DCSBiosOutputLED.ImportString(dcsBiosOutputString);
        }

        public override string ExportSettings()
        {
            var stringBuilder = new StringBuilder();
            var position = (BIPLedPositionEnum)SaitekLEDPosition.Position;
            stringBuilder.Append("PanelBIP{" + position + "|" + LEDColor + "|" + DCSBiosOutputLED + "}");
            return stringBuilder.ToString();
        }
    }
}
