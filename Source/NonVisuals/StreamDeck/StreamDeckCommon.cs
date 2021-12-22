namespace NonVisuals.StreamDeck
{
    using System;
    using System.IO;
    using System.Windows.Controls;

    using ClassLibraryCommon;

    using MEF;

    public static class StreamDeckCommon
    {

        public static void CleanDCSFPTemporaryFolder()
        {
            var dcsfpTempFolder = GetDCSFPTemporaryFolder();
            
            if (!Directory.Exists(dcsfpTempFolder))
            {
                Directory.CreateDirectory(dcsfpTempFolder);
                return;
            }

            var dcsfpTempDirectoryInfo = new DirectoryInfo(dcsfpTempFolder);

            var folders = Directory.GetDirectories(dcsfpTempFolder);

            foreach (var subfolder in folders)
            {
                DeleteFolder(new DirectoryInfo(subfolder));
            }

            var files = dcsfpTempDirectoryInfo.GetFiles();
            foreach (var fileInfo in files)
            {
                fileInfo.Delete();
            }
        }

        private static void DeleteFolder(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.FullName.Contains(GetDCSFPTemporaryFolder()))
            {
                // Safety that we are only recursing in our own temp data folder
                return;
            }

            var directories = directoryInfo.EnumerateDirectories();
            foreach (var directory in directories)
            {
                DeleteFolder(directory);
            }

            var files = directoryInfo.GetFiles();
            foreach (var fileInfo in files)
            {
                fileInfo.Delete();
            }
        }

        public static PluginGamingPanelEnum ConvertEnum(GamingPanelEnum gamingPanel)
        {
            return gamingPanel switch { 
                GamingPanelEnum.StreamDeckMini => PluginGamingPanelEnum.StreamDeckMini,
                GamingPanelEnum.StreamDeck => PluginGamingPanelEnum.StreamDeck,
                GamingPanelEnum.StreamDeckV2 => PluginGamingPanelEnum.StreamDeckV2,
                GamingPanelEnum.StreamDeckMK2 => PluginGamingPanelEnum.StreamDeckMK2,
                GamingPanelEnum.StreamDeckXL => PluginGamingPanelEnum.StreamDeckXL,
                _ => PluginGamingPanelEnum.Unknown
                };
        }

        public static string GetDCSFPTemporaryFolder()
        {
            var folder = Path.GetTempPath() + "DCSFP";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        public static int ButtonNumber(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return -1;
            }

            return int.Parse(streamDeckButtonName.ToString().Replace("BUTTON", string.Empty));
        }

        public static EnumStreamDeckButtonNames ButtonName(int streamDeckButtonNumber)
        {
            if (streamDeckButtonNumber == 0)
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }

        public static EnumStreamDeckButtonNames ButtonName(string streamDeckButtonNumber)
        {
            if (string.IsNullOrEmpty(streamDeckButtonNumber) || streamDeckButtonNumber == "0")
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }

        public static EnumComparator GetComparatorValue(string text)
        {
            return text switch {
                "NotSet" => EnumComparator.NotSet,
                "==" => EnumComparator.Equals,
                "!=" => EnumComparator.NotEquals,
                "<" => EnumComparator.LessThan,
                "<=" => EnumComparator.LessThanEqual,
                ">" => EnumComparator.GreaterThan,
                ">=" => EnumComparator.GreaterThanEqual,
                "Always" => EnumComparator.Always,
                _ => throw new Exception($"Failed to decode comparison type [{text}]")
            };
        }

        public static void SetComparatorValue(ComboBox comboBox, EnumComparator comparator)
        {
            comboBox.Text = comparator switch
            {
                EnumComparator.NotSet => "NotSet",
                EnumComparator.Equals => "==",
                EnumComparator.NotEquals => "!=",
                EnumComparator.LessThan => "<",
                EnumComparator.LessThanEqual => "<=",
                EnumComparator.GreaterThan => ">",
                EnumComparator.GreaterThanEqual => ">=",
                EnumComparator.Always => "Always",
                _ => throw new Exception($"Failed to decode comparison type [{comparator}]")
            };
        }
    }
}
