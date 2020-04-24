using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace NonVisuals
{
    public static class SettingsManager
    {
        public static Font DefaultFont
        {
            get => Properties.Settings.Default.StreamDeckDefaultFont;
            set
            {
                Properties.Settings.Default.StreamDeckDefaultFont = value;
                Properties.Settings.Default.Save();
            }
        }

        public static Color DefaultFontColor
        {
            get => Properties.Settings.Default.StreamDeckDefaultFontColor;
            set
            {
                Properties.Settings.Default.StreamDeckDefaultFontColor = value;
                Properties.Settings.Default.Save();
            }
        }

        public static Color DefaultBackgroundColor
        {
            get => Properties.Settings.Default.StreamDeckDefaultBackgroundColor;
            set
            {
                Properties.Settings.Default.StreamDeckDefaultBackgroundColor = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string LastImageFileDirectory
        {
            get => Properties.Settings.Default.LastImageFileDialogLocation;
            set
            {
                Properties.Settings.Default.LastImageFileDialogLocation = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int OffsetX
        {
            get => Properties.Settings.Default.OffsetX;
            set
            {
                Properties.Settings.Default.OffsetX = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int OffsetY
        {
            get => Properties.Settings.Default.OffsetY;
            set
            {
                Properties.Settings.Default.OffsetY = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
