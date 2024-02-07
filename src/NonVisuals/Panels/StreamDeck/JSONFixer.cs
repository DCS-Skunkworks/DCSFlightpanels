namespace NonVisuals.Panels.StreamDeck
{
    public static class JSONFixer
    {

        public static string Fix(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            /*
             * 14.09.2021, moved NonVisuals.VirtualKeyCode to MEF.VirtualKeyCode
             */
            json = json.Replace("NonVisuals.VirtualKeyCode, NonVisuals", "MEF.VirtualKeyCode, MEF");

            /*
             * 15.09.2023, moved key emulation classes to NonVisuals.KeyEmulation
             */
            json = json.Replace("NonVisuals.KeyPress, NonVisuals", "NonVisuals.KeyEmulation.KeyPress, NonVisuals");
            json = json.Replace("NonVisuals.KeyPressInfo, NonVisuals", "NonVisuals.KeyEmulation.KeyPressInfo, NonVisuals");

            /*
             * 07.02.2024, organized DCS-BIOS classes
             */
            json = json.Replace("DCS_BIOS.DCSBIOSInput, DCS-BIOS", "DCS_BIOS.Serialized.DCSBIOSInput, DCS-BIOS");
            json = json.Replace("DCS_BIOS.DCSBIOSOutput, DCS-BIOS", "DCS_BIOS.Serialized.DCSBIOSOutput, DCS-BIOS");
            json = json.Replace("DCS_BIOS.DCSBIOSOutputFormula, DCS-BIOS", "DCS_BIOS.Serialized.DCSBIOSOutputFormula, DCS-BIOS");
            json = json.Replace("DCS_BIOS.DCSBIOSInputInterface, DCS-BIOS", "DCS_BIOS.Serialized.DCSBIOSInputInterface, DCS-BIOS");



            return json;
        }

    }
}
