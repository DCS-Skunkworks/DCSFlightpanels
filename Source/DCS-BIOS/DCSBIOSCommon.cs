using System;
using System.Diagnostics;
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

            Debug.WriteLine($"\nFolder exists? {Directory.Exists(jsonDirectory)}    {jsonDirectory}\n");
            if (string.IsNullOrEmpty(jsonDirectory) || !Directory.Exists(jsonDirectory))
            {
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
            var jsonFileCount = 0;
            foreach (var filename in files)
            {
                if (filename.ToLower().EndsWith(".json"))
                {
                    jsonFileCount++;
                }
            }

            return jsonFileCount >= 10 ? new Tuple<bool, bool>(true, true) : new Tuple<bool, bool>(true, false);
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
