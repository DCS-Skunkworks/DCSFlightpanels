using System;
using System.IO;

namespace DCS_BIOS
{
    public class DBCommon
    {

        public static string PrintBitStrings(byte[] array)
        {
            var result = "";
            for (int i = 0; i < array.Length; i++)
            {
                var str = Convert.ToString(array[i], 2).PadLeft(8, '0');
                result = result + "  " + str;
            }
            return result;
        }

        public static string ToPaddedHexString(int i, int padLength = 4)
        {
            return "0x" + i.ToString("x").PadLeft(padLength, '0');
        }

        public static string ToPaddedHexString(uint i, int padLength = 4)
        {
            return "0x" + i.ToString("x").PadLeft(padLength, '0');
        }

        private static int SetBitToZeroAtPosition(int value, int position)
        {
            return value & ~(1 << position);
        }


        public static string GetDCSBIOSDirectory(string directory)
        {
            var replaceString = "USERDIRECTORY$$$###";
            //Cannot use %USERPROFILE%, DirectoryInfo gets crazy
            if (!string.IsNullOrEmpty(directory))
            {
                var path = directory;
                if (path.Contains(replaceString))
                {
                    path = path.Replace(replaceString, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                }
                return path;
            }
            return null;
        }

        public static bool VerifyDCSBIOSRootDirectory(string dcsbiosRootDirectory)
        {
            var result = Directory.Exists(dcsbiosRootDirectory);

            if (!Directory.Exists(dcsbiosRootDirectory + "//doc//json"))
            {
                result = false;
            }

            if (!Directory.Exists(dcsbiosRootDirectory + "//lib"))
            {
                result = false;
            }

            return result;
        }
    }
}
