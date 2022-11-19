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

            return json;
        }

    }
}
