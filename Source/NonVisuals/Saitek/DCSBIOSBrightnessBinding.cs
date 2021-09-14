namespace NonVisuals.Saitek
{
    using System;
    using System.Text;

    using DCS_BIOS;

    public class DCSBIOSBrightnessBinding
    {
        public static readonly string Keyword = "PanelBrightness";
        private DCSBIOSOutput _dcsbiosOutput;

        public DCSBIOSOutput DCSBiosOutput
        {
            get => _dcsbiosOutput;
            set => _dcsbiosOutput = value;
        }
        
        public DCSBIOSBrightnessBinding()
        {
        }

        public DCSBIOSBrightnessBinding(DCSBIOSOutput dcsbiosOutput)
        {
            _dcsbiosOutput = dcsbiosOutput;
        }


        public string ControlId
        {
            get => _dcsbiosOutput == null ? string.Empty : _dcsbiosOutput.ControlId;
        }

        public void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }

            if (settings.Contains(Keyword))
            {
                // PanelBrightness{DCSBiosOutput{INTEGER_TYPE|Equals|0x0000|0x0000|0|0}}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                settings = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries)[0];

                // PanelBrightness{DCSBiosOutput{INTEGER_TYPE|Equals|0x0000|0x0000|0|0}}
                settings = settings.Substring(settings.IndexOf("{", StringComparison.InvariantCulture) + 1);
                settings = settings.Substring(0, settings.Length - 1);
                DCSBiosOutput = new DCSBIOSOutput();
                DCSBiosOutput.ImportString(settings);
            }
        }

        public string ExportSettings()
        {
            if (_dcsbiosOutput == null)
            {
                return null;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(Keyword + "{" + DCSBiosOutput + "}");

            return stringBuilder.ToString();
        }
    }

}
