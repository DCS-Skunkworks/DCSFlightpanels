using System;
using System.IO;
using System.Windows.Controls;

namespace NonVisuals.StreamDeck
{
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
                //Safety that we are only recursing in our own temp data folder
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

        public static PluginGamingPanelEnum ConvertEnum(GamingPanelEnum panel)
        {
            switch (panel)
            {
                case GamingPanelEnum.StreamDeckMini:
                    {
                        return PluginGamingPanelEnum.StreamDeckMini;
                    }
                case GamingPanelEnum.StreamDeck:
                    {
                        return PluginGamingPanelEnum.StreamDeck;
                    }
                case GamingPanelEnum.StreamDeckV2:
                    {
                        return PluginGamingPanelEnum.StreamDeckV2;
                    }
                case GamingPanelEnum.StreamDeckMK2:
                    {
                        return PluginGamingPanelEnum.StreamDeckMK2;
                    }
                case GamingPanelEnum.StreamDeckXL:
                    {
                        return PluginGamingPanelEnum.StreamDeckXL;
                    }
            }

            return PluginGamingPanelEnum.Unknown;
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

        public static EnumComparator ComparatorValue(string text)
        {
            if (text == "NotSet")
            {
                return EnumComparator.NotSet;
            }
            if (text == "==")
            {
                return EnumComparator.Equals;
            }
            if (text == "!=")
            {
                return EnumComparator.NotEquals;
            }
            if (text == "<")
            {
                return EnumComparator.LessThan;
            }
            if (text == "<=")
            {
                return EnumComparator.LessThanEqual;
            }
            if (text == ">")
            {
                return EnumComparator.GreaterThan;
            }
            if (text == ">=")
            {
                return EnumComparator.GreaterThanEqual;
            }
            if (text == "Always")
            {
                return EnumComparator.Always;
            }
            throw new Exception("Failed to decode comparison type.");
        }

        public static void SetComparatorValue(ComboBox comboBox, EnumComparator comparator)
        {
            switch (comparator)
            {
                case EnumComparator.NotSet:
                    {
                        comboBox.Text = "NotSet";
                        break;
                    }
                case EnumComparator.Equals:
                    {
                        comboBox.Text = "==";
                        break;
                    }
                case EnumComparator.NotEquals:
                    {
                        comboBox.Text = "!=";
                        break;
                    }
                case EnumComparator.LessThan:
                    {
                        comboBox.Text = "<";
                        break;
                    }
                case EnumComparator.LessThanEqual:
                    {
                        comboBox.Text = "<=";
                        break;
                    }
                case EnumComparator.GreaterThan:
                    {
                        comboBox.Text = ">";
                        break;
                    }
                case EnumComparator.GreaterThanEqual:
                    {
                        comboBox.Text = ">=";
                        break;
                    }
                case EnumComparator.Always:
                    {
                        comboBox.Text = "Always";
                        break;
                    }
                default:
                    {
                        throw new Exception("Failed to decode comparison type.");
                    }
            }
        }

    }
}
