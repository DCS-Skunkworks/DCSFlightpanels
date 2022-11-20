namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;

    public static class StreamDeckConstants
    {
        public static readonly string StreamDeckGalleryPath = AppDomain.CurrentDomain.BaseDirectory + "\\StreamDeckGallery\\";

        public static readonly string StreamDeckGalleryPathSymbols = AppDomain.CurrentDomain.BaseDirectory + "\\StreamDeckGallery\\Symbols\\";

        public static readonly string StreamDeckGalleryPathMisc = AppDomain.CurrentDomain.BaseDirectory + "\\StreamDeckGallery\\Misc\\";

        public static readonly string StreamDeckGalleryHomeWhite = "home_white.png";

        public static readonly string StreamDeckGalleryBackWhite = "back_white.png";

        public static readonly string DCSBIOSValuePlaceHolder = "{dcsbios}";

        public static readonly string DCSBIOSValuePlaceHolderNoBrackets = "dcsbios";

        public static readonly CultureInfo DoubleCultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        public const string NO_ACTION = "No action";

        public const string BACK = "BACK";

        public const string GO_BACK_ONE_LAYER_STRING = "Go Back";

        public const string GO_TO_HOME_LAYER_STRING = "Go to Home";

        public const string HOME = "HOME";

        public const string HOME_LAYER_NAME = "Home";

        public const string NO_HOME_LAYER_FOUND = "No home layer specified. Home layer must be set.";

        public const string SEVERAL_HOME_LAYER_FOUND = "Several layers has been marked as Home Layer.\n Only Home layer must be set.";

        public const string BUTTON_EXPORT_FILENAME = "dcsfp_export_buttons.txt";

        public const int IMAGE_UPDATING_THREAD_SLEEP_TIME = 100;

        public const int ADJUST_OFFSET_CHANGE_VALUE = 2;

        // public const string DEFAULT_FONT = "Lucida Console";
        public const int STREAMDECK_ICON_HEIGHT = 72;

        public const int STREAMDECK_ICON_WIDTH = 72;

        public const string NUMBER_BUTTON_LOCATION = @"pack://application:,,,/dcsfp;component/StreamDeckGallery/NumberButtons/";

        public const string NUMBER_BUTTON_PREFIX = "BUTTON";

        public const string COLOR_MILITARY_GRAY = "#e6e6e6";

        public const string COLOR_DEFAULT_WHITE = "#f0e9ee";

        public const string COLOR_DEFAULT_WHITE2 = "#f2ebf0";

        public const string COLOR_MI8_BLUE = "#00cccc";

        public const string COLOR_MILITARY_DARK_GREEN = "#4d784e";

        public const string COLOR_MILITARY_LIGHT_GREEN = "#6ea171";

        public const string COLOR_MILITARY_SAND = "#e1d798";

        public const string COLOR_NAVY_DARK_BLUE = "#424756";

        public const string COLOR_ALERT_YELLOW = "#ffff33";

        public const string COLOR_RAF_BLUE = "#5d8aa8";

        public const string COLOR_TEAK_BROWN = "#a27557";

        public const string COLOR_WARNING_RED = "#ff0000";

        public const string COLOR_AIRCRAFT_GRAY = "#92989F";

        public const string COLOR_SANDSTONE_CAMO = "#B49D80";

        public const string COLOR_DRAB_CAMO = "#A27C52";

        public const string COLOR_SAND_CAMO = "#AB9381";

        public const string COLOR_CAMO_GRAY = "#9495a5";

        public const string COLOR_CAMO_EARTH = "#ac7e54";

        public const string COLOR_INSIGNIA_BLUE = "#172035";

        public const string COLOR_BLUE = "#09568d";

        public const string COLOR_INSTRUMENT_BLACK = "#1c1d22";

        public const string COLOR_GUNSHIP_GREEN = "#46554f";

        public const string COLOR_INTERIOR_GREEN = "#65623c";

        public const string COLOR_NAVY_BLUE = "#646e83";

        public const string COLOR_SKY = "#acac9a";

        public const string COLOR_AIRCRAFT_YELLOW = "#ffaa07";

        public static int[] GetOLEColors()
        {
            return new[]
                       {
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_MILITARY_GRAY)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_DEFAULT_WHITE2)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_NAVY_BLUE)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_MILITARY_SAND)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_AIRCRAFT_YELLOW)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_WARNING_RED)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_INSIGNIA_BLUE)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_INSTRUMENT_BLACK)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_INTERIOR_GREEN)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_GUNSHIP_GREEN)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_CAMO_EARTH)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_DRAB_CAMO)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_CAMO_GRAY)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_SKY)),
                           ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_MI8_BLUE)), ColorTranslator.ToOle(ColorTranslator.FromHtml(COLOR_MILITARY_LIGHT_GREEN)),
                       };
        }

        public static List<Color> GetColors()
        {
            var result = new List<Color>
            {
                ColorTranslator.FromHtml(COLOR_MILITARY_GRAY),
                ColorTranslator.FromHtml(COLOR_DEFAULT_WHITE),
                ColorTranslator.FromHtml(COLOR_DEFAULT_WHITE2),
                ColorTranslator.FromHtml(COLOR_MI8_BLUE),
                ColorTranslator.FromHtml(COLOR_MILITARY_DARK_GREEN),
                ColorTranslator.FromHtml(COLOR_MILITARY_LIGHT_GREEN),
                ColorTranslator.FromHtml(COLOR_MILITARY_SAND),
                ColorTranslator.FromHtml(COLOR_NAVY_DARK_BLUE),
                ColorTranslator.FromHtml(COLOR_ALERT_YELLOW),
                ColorTranslator.FromHtml(COLOR_RAF_BLUE),
                ColorTranslator.FromHtml(COLOR_TEAK_BROWN),
                ColorTranslator.FromHtml(COLOR_WARNING_RED),
                ColorTranslator.FromHtml(COLOR_AIRCRAFT_GRAY),
                ColorTranslator.FromHtml(COLOR_SANDSTONE_CAMO),
                ColorTranslator.FromHtml(COLOR_DRAB_CAMO),
                ColorTranslator.FromHtml(COLOR_SAND_CAMO),
                ColorTranslator.FromHtml(COLOR_CAMO_GRAY),
                ColorTranslator.FromHtml(COLOR_CAMO_EARTH),
                ColorTranslator.FromHtml(COLOR_INSIGNIA_BLUE),
                ColorTranslator.FromHtml(COLOR_BLUE),
                ColorTranslator.FromHtml(COLOR_INSTRUMENT_BLACK),
                ColorTranslator.FromHtml(COLOR_GUNSHIP_GREEN),
                ColorTranslator.FromHtml(COLOR_INTERIOR_GREEN),
                ColorTranslator.FromHtml(COLOR_NAVY_BLUE),
                ColorTranslator.FromHtml(COLOR_SKY),
                ColorTranslator.FromHtml(COLOR_AIRCRAFT_YELLOW)
            };
            return result;
        }

        public static string TranslateLayerName(string layerName)
        {
            return layerName switch
            {
                NO_ACTION => "NOACTION",
                GO_TO_HOME_LAYER_STRING => "HOME",
                GO_BACK_ONE_LAYER_STRING => "BACK",
                _ => layerName
            };
        }

        public static Color HexColorToColor(string hexColor)
        {
            return ColorTranslator.FromHtml(hexColor);
        }
    }
}
