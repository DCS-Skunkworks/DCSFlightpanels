namespace ClassLibraryCommon
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
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

    public static class Common
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private static NumberFormatInfo _pz69NumberFormatInfoFullDisplay;
        private static int _emulationModesFlag = 0;
        public static APIModeEnum APIMode = 0;

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

        public static bool IsStreamDeck(GamingPanelEnum panelType)
        {
            return panelType == GamingPanelEnum.StreamDeckMini 
                || panelType == GamingPanelEnum.StreamDeck 
                || panelType == GamingPanelEnum.StreamDeckXL 
                || panelType == GamingPanelEnum.StreamDeckV2 
                || panelType == GamingPanelEnum.StreamDeckMK2;
        }

        public static Key RealKey(this KeyEventArgs e)
        {
            return e.Key switch
            {
                Key.System => e.SystemKey,
                Key.ImeProcessed => e.ImeProcessedKey,
                Key.DeadCharProcessed => e.DeadCharProcessedKey,
                _ => e.Key
            };
        }

        public static string RemoveLControl(string keySequence)
        {
            return true switch {
                _ when keySequence.Contains(@"RMENU + LCONTROL") => keySequence.Replace(@"+ LCONTROL", string.Empty),
                _ when keySequence.Contains(@"LCONTROL + RMENU") => keySequence.Replace(@"LCONTROL +", string.Empty),
                _ => keySequence,
            };  
        }

        public static readonly List<GamingPanelSkeleton> GamingPanelSkeletons = new List<GamingPanelSkeleton>
        {
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ69RadioPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.BackLitPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.TPM),
            new GamingPanelSkeleton(GamingPanelVendorEnum.MadCatz, GamingPanelEnum.FarmingPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMini),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeck),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckV2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMK2),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckXL),
        };

        private static void ValidateEmulationModeFlag()
        {
            if (IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
            {
                if (IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled) ||
                    IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled))
                {
                    throw new Exception($"Invalid emulation modes flag : {_emulationModesFlag}");
                }
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
            _emulationModesFlag &= ~((int)flagValue);
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

        public static NumberFormatInfo GetPZ69FullDisplayNumberFormat()
        {
            if (_pz69NumberFormatInfoFullDisplay == null)
            {
                _pz69NumberFormatInfoFullDisplay = new NumberFormatInfo
                {
                    NumberDecimalSeparator = ".",
                    NumberDecimalDigits = 4,
                    NumberGroupSeparator = string.Empty
                };
            }
            return _pz69NumberFormatInfoFullDisplay;
        }

        public static string GetMd5Hash(string input)
        {
            var md5 = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpperInvariant();
        }

        public static string GetRandomMd5Hash()
        {
            var bytes = new byte[16];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(bytes);
            }

            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }

        public static string GetDescriptionField(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
            return string.Empty;
        }

        public static void ShowErrorMessageBox(Exception ex, string message = null)
        {
            logger.Error(ex, message);
            MessageBox.Show(ex.Message, $"Details logged to error log.{Environment.NewLine}{ex.Source}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static string GetApplicationPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            var uriRelativeTo = new Uri(relativeTo);
            var rel = Uri.UnescapeDataString(uriRelativeTo.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!rel.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                rel = $".{ Path.DirectorySeparatorChar }{ rel }";
            }
            return rel;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
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
        }
    }
}
