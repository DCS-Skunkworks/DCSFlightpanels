namespace NonVisuals.Radios.Misc
{
    using System;
    using System.Globalization;
    using ClassLibraryCommon;

    using MEF;

    using Saitek;

    public class RadioPanelPZ69DisplayValue
    {
        public RadioPanelPZ69Display RadioPanelDisplay { get; set; }
        public string Value { get; set; }
        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob { get; set; }

        public void ImportSettings(string settings)
        {
            if (!settings.StartsWith("PZ69DisplayValue{"))
            {
                throw new Exception("Invalid setting for RadioPanelPZ69DisplayValue, cannot import.");
            }

            // PZ69DisplayValue{UpperCom1|UpperActive|124.12}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            var tmp = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("PZ69DisplayValue{", string.Empty);
            tmp = tmp.Replace("}", string.Empty);

            // UpperCom1|UpperActive|124.12
            var array = tmp.Split('|');
            try
            {
                Value = array[2];
                RadioPanelDisplay = (RadioPanelPZ69Display)Enum.Parse(typeof(RadioPanelPZ69Display), array[1]);
                RadioPanelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), array[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to import setting for RadioPanelPZ69Display{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        public string ExportSettings()
        {
            var pz69NumberFormatInfoFullDisplay = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = 4,
                NumberGroupSeparator = string.Empty
            };

            if (!string.IsNullOrEmpty(Value) && double.Parse(Value, pz69NumberFormatInfoFullDisplay) >= 0)
            {
                return "PZ69DisplayValue{" + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "|" + Enum.GetName(typeof(RadioPanelPZ69Display), RadioPanelDisplay) + "|" + Value + "}";
            }

            return null;
        }
    }
}
