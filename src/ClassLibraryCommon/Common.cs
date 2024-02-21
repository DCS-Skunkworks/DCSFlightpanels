using System.Windows.Input;

namespace ClassLibraryCommon
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;

    [Flags]
    public enum EmulationMode
    {
        DCSBIOSInputEnabled = 1,
        DCSBIOSOutputEnabled = 2,
        KeyboardEmulationOnly = 4,
        SRSEnabled = 8,
        NS430Enabled = 16
    }

    /// <summary>
    /// Common functions used by multiple projects. 
    /// </summary>
    public static class Common
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int _emulationModesFlag;


        /// <summary>
        /// Checks the setting "JSON Directory". This folder must contain all the JSON files
        /// </summary>
        /// <param name="jsonDirectory"></param>
        /// <returns>
        /// 0, 0 : folder not found, json not found
        /// 1, 0 : folder found, json not found
        /// 1, 1 : folder found, json found
        /// </returns>
        public static Tuple<bool, bool> CheckJSONDirectory(string jsonDirectory)
        {
            jsonDirectory = Environment.ExpandEnvironmentVariables(jsonDirectory);

            if (string.IsNullOrEmpty(jsonDirectory) || !Directory.Exists(jsonDirectory))
            {
                /*
                 * Folder not found
                 */
                return new Tuple<bool, bool>(false, false);
            }

            var files = Directory.EnumerateFiles(jsonDirectory);

            /*
             * This is not optimal, the thing is that there is no single file to rely
             * on in order to determine that this folder is the DCS-BIOS JSON directory.
             * Files can be changed (although rare) but it cannot be taken for certain
             * that this doesn't happen.
             *
             * The solution is to count the number of json files in the folder.
             * This gives a fairly certain indication that the folder is in fact
             * the JSON folder. There are JSON files in other folders but not many.
             */
            var jsonFound = files.Count(filename => filename.ToLower().EndsWith(".json")) >= 10;

            return new Tuple<bool, bool>(true, jsonFound);
        }

        public static APIModeEnum APIModeUsed { get; set; } = 0;

        public static void PlaySoundFile(string soundFile, double volume, bool showException = false) //Volume 0 - 100
        {
            try
            {
                if (string.IsNullOrEmpty(soundFile) || !File.Exists(soundFile))
                {
                    return;
                }
                MediaPlayer mediaPlayer = new();
                mediaPlayer.Open(new Uri(soundFile));
                mediaPlayer.Volume = volume / 100.0f;
                mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                if (showException)
                {
                    ShowErrorMessageBox(ex);
                }
            }
        }

        public static string GetHex(uint i, bool includePrefix = true, bool lowercase = true, bool padLengthTo4 = true)
        {
            return GetHex((int)i, includePrefix, lowercase, padLengthTo4);
        }

        public static string GetHex(int i, bool includePrefix = true, bool lowercase = true, bool padLengthTo4 = true)
        {
            var formatter = lowercase ? "x" : "X";
            var s = i.ToString(formatter);
            if (padLengthTo4)
            {
                s = s.PadLeft(4, '0');
            }
            if (includePrefix) s = s.Insert(0, "0x");

            return s;
        }

        public static string RemoveCurlyBrackets(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s.Replace("{", "").Replace("}", "");
        }

        public static string RemoveLControl(string keySequence)
        {
            return true switch
            {
                _ when keySequence.Contains("RMENU + LCONTROL") => keySequence.Replace("+ LCONTROL", string.Empty),
                _ when keySequence.Contains("LCONTROL + RMENU") => keySequence.Replace("LCONTROL +", string.Empty),
                _ => keySequence
            };
        }

        public static readonly List<GamingPanelSkeleton> GamingPanelSkeletons = new()
        {
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ69RadioPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.BackLitPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.TPM),
            new GamingPanelSkeleton(GamingPanelVendorEnum.MadCatz, GamingPanelEnum.FarmingPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMini),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMiniV2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeck),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckV2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMK2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckXL),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckXLRev2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.CockpitMaster, GamingPanelEnum.CDU737),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckPlus)
        };

        private static void ValidateEmulationModeFlag()
        {
            if (!IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly)) return;

            if (IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) ||
                IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled))
            {
                throw new Exception($"Invalid emulation modes flag : {_emulationModesFlag}");
            }
        }

        public static void SetEmulationModesFlag(int flag)
        {
            _emulationModesFlag = flag;
            ValidateEmulationModeFlag();
        }

        public static int GetEmulationModesFlag()
        {
            ValidateEmulationModeFlag();
            return _emulationModesFlag;
        }

        public static void SetEmulationModes(EmulationMode emulationMode)
        {
            _emulationModesFlag |= (int)emulationMode;
            ValidateEmulationModeFlag();
        }

        public static bool IsEmulationModesFlagSet(EmulationMode flagValue)
        {
            return (_emulationModesFlag & (int)flagValue) > 0;
        }

        public static void ClearEmulationModesFlag(EmulationMode flagValue)
        {
            _emulationModesFlag &= ~(int)flagValue;
        }

        public static void ResetEmulationModesFlag()
        {
            _emulationModesFlag = 0;
        }

        public static bool NoDCSBIOSEnabled()
        {
            ValidateEmulationModeFlag();
            return !IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled) && !IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled);
        }

        public static bool KeyEmulationOnly()
        {
            ValidateEmulationModeFlag();
            return IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly);
        }

        public static bool FullDCSBIOSEnabled()
        {
            ValidateEmulationModeFlag();
            return IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) && IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled);
        }

        public static bool PartialDCSBIOSEnabled()
        {
            ValidateEmulationModeFlag();
            return IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) || IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled);
        }

        public static string GetMd5Hash(string input)
        {
            var md5 = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpperInvariant();
        }

        public static string GetRandomMd5Hash()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }

        public static void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowErrorMessageBox(Exception ex, string message = null)
        {
            Logger.Error(ex, message);
            MessageBox.Show(ex.Message, $"Details logged to error log.{Environment.NewLine}{ex.Source}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static string GetApplicationPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Returns the relative path from "relativeTo" to "path".
        /// ATTN : relativeTo must end with a \ if it is a path only, not an object.
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRelativePath(string relativeTo, string path)
        {
            var uriRelativeTo = new Uri(relativeTo);
            var rel = Uri.UnescapeDataString(uriRelativeTo.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!rel.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                rel = $".{Path.DirectorySeparatorChar}{rel}";
            }
            return rel;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null) yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T o)
                {
                    yield return o;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }


        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null)
            {
                return null;
            }
            var parentObj = VisualTreeHelper.GetParent(child);

            return parentObj switch
            {
                //we've reached the end of the tree
                null => null,
                // check if the parent matches the type we are requested
                T parent => parent,
                // here, To find the next parent in the tree. we are using recursion until we found the requested type or reached to the end of tree.
                _ => FindVisualParent<T>(parentObj)
            };
        }

        //^[A-Za-z0-9 . \t]*(MouseEnter){1}(\s\+\=)\s*
        public static void UIElement_OnMouseEnterHandIcon(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        public static void UIElement_OnMouseLeaveNormalIcon(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        public static int GetNthIndexOf(string s, char c, int instance)
        {
            if (string.IsNullOrEmpty(s)) return -1;

            var count = 0;
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    count++;
                    if (count == instance)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
