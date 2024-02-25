namespace NonVisuals.Panels.Saitek {
    using System;
    using System.Text;
    using ClassLibraryCommon;
    using DCS_BIOS.Serialized;
    using Panels;

    [SerializeCritical]
    public class DcsOutputAndColorBindingBIP : DcsOutputAndColorBinding
    {
        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }

            // PanelBIP{Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}}\o/\\?\hid#vid_06a3&pid_0b4e#9&1f079469&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            settings = settings.Split(Separator, StringSplitOptions.RemoveEmptyEntries)[0];

            // PanelBIP{Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}}
            settings = settings.Substring(settings.IndexOf("{", StringComparison.InvariantCulture) + 1);

            // Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}}
            settings = settings.Substring(0, settings.Length - 1);

            // Position_1_4|GREEN|DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}
            var dcsBiosOutputString = settings.Substring(settings.IndexOf("DCSBiosOutput{", StringComparison.InvariantCulture));

            // DCSBiosOutput{INTEGER_TYPE|Equals|0x14be|0x4000|14|1}
            settings = settings.Remove(settings.IndexOf("DCSBiosOutput{", StringComparison.InvariantCulture));

            // Position_1_4|GREEN|
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
