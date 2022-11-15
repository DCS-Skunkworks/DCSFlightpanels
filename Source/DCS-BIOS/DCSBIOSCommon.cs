using System;
using System.IO;

namespace DCS_BIOS
{
    public static class DCSBIOSCommon
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonDirectory"></param>
        /// <returns>
        /// 0, 0 : folder found, json found
        /// 1, 0 : folder found, json not found
        /// 1, 1 : folder not found, (json not found)
        /// </returns>
        public static Tuple<bool, bool> CheckJSONDirectory(string jsonDirectory)
        {
            jsonDirectory = GetDCSBIOSJSONDirectory(jsonDirectory);

            //Debug.WriteLine($"\nFolder exists? {Directory.Exists(jsonDirectory)}    {jsonDirectory}\n");
            if (string.IsNullOrEmpty(jsonDirectory) || !Directory.Exists(jsonDirectory))
            {
                return new Tuple<bool, bool>(false, false);
            }

            var files = Directory.EnumerateFiles(jsonDirectory);

            foreach (var filename in files)
            {
                if (filename.ToLower().EndsWith(".json"))
                {
                    return new Tuple<bool, bool>(true, true);
                }
            }

            return new Tuple<bool, bool>(true, false);
        }

        public static string PrintBitStrings(byte[] array)
        {
            var result = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                var str = Convert.ToString(array[i], 2).PadLeft(8, '0');
                result = result + "  " + str;
            }
            return result;
        }

        public static string GetDCSBIOSJSONDirectory(string jsonDirectory)
        {
            var replaceString = "$USERDIRECTORY$";

            // Cannot use %USERPROFILE%, DirectoryInfo gets crazy
            if (!string.IsNullOrEmpty(jsonDirectory))
            {
                if (jsonDirectory.Contains(replaceString))
                {
                    jsonDirectory = jsonDirectory.Replace(replaceString, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                }

                return jsonDirectory;
            }

            return null;
        }
    }
}
