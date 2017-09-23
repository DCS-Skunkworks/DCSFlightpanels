using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DCS_BIOS
{
    public class DBCommon
    {
        
        public static bool Debug = false;
        public static void DebugP(string str)
        {
            if (Debug)
            {
                Console.WriteLine(str);
            }
        }

        public static object _errorLoglockObject = new object();
        public static object _debugLoglockObject = new object();
        public static string ErrorLog = "";
        public static string DebugLog = "";

        public static void SetErrorLog(string filename)
        {
            lock (_errorLoglockObject)
            {
                ErrorLog = filename;
            }
        }

        public static void SetDebugLog(string filename)
        {
            lock (_debugLoglockObject)
            {
                DebugLog = filename;
            }
        }

        public static void LogError(uint location, Exception ex, string message = null )
        {
            lock (_errorLoglockObject)
            {
                var file = Path.GetTempPath() + "\\Flightpanels_error_log.txt";
                if (!File.Exists(file))
                {
                    File.Create(file);
                }
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var version = fileVersionInfo.FileVersion;

                var streamWriter = File.AppendText(file);
                try
                {
                    streamWriter.Write(Environment.NewLine + DateTime.Now.ToString("dd.MM.yyyy hh:mm:yy") + "  version : " + version);
                    streamWriter.Write(Environment.NewLine + location + " Custom message = [" + message + "]" + Environment.NewLine + ex.GetBaseException().GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                }
                finally
                {
                    streamWriter.Close();
                }
            }
        }


        public static void LogError(uint location, string message)
        {
            lock (_errorLoglockObject)
            {
                var file = Path.GetTempPath() + "\\Flightpanels_error_log.txt";
                if (!File.Exists(file))
                {
                    File.Create(file);
                }
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var version = fileVersionInfo.FileVersion;

                var streamWriter = File.AppendText(file);
                try
                {
                    streamWriter.Write(Environment.NewLine + DateTime.Now.ToString("dd.MM.yyyy hh:mm:yy") + "  version : " + version);
                    streamWriter.Write(Environment.NewLine + location + " Message = [" + message + "]" + Environment.NewLine);
                }
                finally
                {
                    streamWriter.Close();
                }
            }
        }

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


        public static string GetDCSBIOSJSONDirectory(string jsonDirectory)
        {
            var replaceString = "USERDIRECTORY$$$###";
            //Cannot use %USERPROFILE%, DirectoryInfo gets crazy
            if (!string.IsNullOrEmpty(jsonDirectory))
            {
                var path = jsonDirectory;
                if (path.Contains(replaceString))
                {
                    path = path.Replace(replaceString, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                }
                return path;
            }
            return null;
        }
    }
}
