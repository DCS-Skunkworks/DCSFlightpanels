namespace NonVisuals
{
    using System.Drawing;

    using Properties;

    /*
     * Used for accessing main project's settings from auxiliary projects.
     */
    public static class SettingsManager
    {
        public static Font DefaultFont
        {
            get => Settings.Default.StreamDeckDefaultFont;
            set
            {
                Settings.Default.StreamDeckDefaultFont = value;
                Settings.Default.Save();
            }
        }

        public static Color DefaultFontColor
        {
            get => Settings.Default.StreamDeckDefaultFontColor;
            set
            {
                Settings.Default.StreamDeckDefaultFontColor = value;
                Settings.Default.Save();
            }
        }

        public static Color DefaultBackgroundColor
        {
            get => Settings.Default.StreamDeckDefaultBackgroundColor;
            set
            {
                Settings.Default.StreamDeckDefaultBackgroundColor = value;
                Settings.Default.Save();
            }
        }

        public static string LastImageFileDirectory
        {
            get => Settings.Default.LastProfileDialogLocation;
            set
            {
                Settings.Default.LastProfileDialogLocation = value;
                Settings.Default.Save();
            }
        }

        public static string LastSoundFileDirectory
        {
            get => Settings.Default.LastProfileDialogLocation;
            set
            {
                Settings.Default.LastProfileDialogLocation = value;
                Settings.Default.Save();
            }
        }

        public static int OffsetX
        {
            get => Settings.Default.OffsetX;
            set
            {
                Settings.Default.OffsetX = value;
                Settings.Default.Save();
            }
        }

        public static int OffsetY
        {
            get => Settings.Default.OffsetY;
            set
            {
                Settings.Default.OffsetY = value;
                Settings.Default.Save();
            }
        }
    }
}
