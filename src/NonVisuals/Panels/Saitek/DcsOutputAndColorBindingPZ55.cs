namespace NonVisuals.Panels.Saitek {
    using System;
    using System.Text;
    using ClassLibraryCommon;
    using DCS_BIOS.Serialized;
    using Panels;
   
    [SerializeCriticalCustom]
    public class DcsOutputAndColorBindingPZ55 : DcsOutputAndColorBinding
    {
        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }

            if (settings.Contains("SwitchPanelLedUp"))
            {
                SaitekLEDPosition = new SaitekPanelLEDPosition((int)SwitchPanelPZ55LEDPosition.UP);
            }
            else if (settings.Contains("SwitchPanelLedLeft"))
            {
                SaitekLEDPosition = new SaitekPanelLEDPosition((int)SwitchPanelPZ55LEDPosition.LEFT);
            }
            else if (settings.Contains("SwitchPanelLedRight"))
            {
                SaitekLEDPosition = new SaitekPanelLEDPosition((int)SwitchPanelPZ55LEDPosition.RIGHT);
            }

            // SwitchPanelLedUp{DARK|DCSBiosOutput{INTEGER_TYPE|Equals|0x0000|0x0000|0|0}}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            settings = settings.Split(Separator, StringSplitOptions.RemoveEmptyEntries)[0];

            // SwitchPanelLedUp{DARK|DCSBiosOutput{INTEGER_TYPE|Equals|0x0000|0x0000|0|0}}
            settings = settings.Substring(settings.IndexOf("{", StringComparison.InvariantCulture) + 1);
            settings = settings.Substring(0, settings.Length - 1);
            var color = settings.Substring(0, settings.IndexOf('|'));
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
}
