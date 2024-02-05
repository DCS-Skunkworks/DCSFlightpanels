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
            return json;
        }

    }
}
