using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ClassLibraryCommon
{
    [Flags]
    public enum OperationFlag
    {
        DCSBIOSInputEnabled = 1,
        DCSBIOSOutputEnabled = 2,
        KeyboardEmulationOnly = 4,
        SRSEnabled = 8,
        NS430Enabled = 16
    }

    public static class Common
    {
        public static readonly List<SaitekPanelSkeleton> SaitekPanelSkeletons = new List<SaitekPanelSkeleton> { new SaitekPanelSkeleton(SaitekPanelsEnum.PZ55SwitchPanel, 0x6A3, 0xD67), new SaitekPanelSkeleton(SaitekPanelsEnum.PZ69RadioPanel, 0x6A3, 0xD05), new SaitekPanelSkeleton(SaitekPanelsEnum.PZ70MultiPanel, 0x6A3, 0xD06), new SaitekPanelSkeleton(SaitekPanelsEnum.BackLitPanel, 0x6A3, 0xB4E), new SaitekPanelSkeleton(SaitekPanelsEnum.TPM, 0x6A3, 0xB4D) };

        private static NumberFormatInfo _pz69NumberFormatInfoFullDisplay;
        private static NumberFormatInfo _pz69NumberFormatInfoEmpty;

        private static int _operationLevelFlag = 0;

        public static void ValidateFlag()
        {
            if (IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
            {
                if (IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled) ||
                    IsOperationModeFlagSet(OperationFlag.DCSBIOSInputEnabled))
                {
                    throw new Exception("Invalid operation level flag : " + _operationLevelFlag);
                }
            }
        }

        public static void SetOperationModeFlag(int flag)
        {
            _operationLevelFlag = flag;
            ValidateFlag();
        }

        public static int GetOperationModeFlag()
        {
            ValidateFlag();
            return _operationLevelFlag;
        }

        public static void SetOperationModeFlag(OperationFlag flagValue)
        {
            _operationLevelFlag = _operationLevelFlag | (int)flagValue;
            ValidateFlag();
        }

        public static bool IsOperationModeFlagSet(OperationFlag flagValue)
        {
            return (_operationLevelFlag & (int)flagValue) > 0;
        }

        public static void ClearOperationModeFlag(OperationFlag flagValue)
        {
            _operationLevelFlag &= ~((int)flagValue);
        }

        public static void ResetOperationModeFlag()
        {
            _operationLevelFlag = 0;
        }

        public static bool NoDCSBIOSEnabled()
        {
            ValidateFlag();
            return !IsOperationModeFlagSet(OperationFlag.DCSBIOSInputEnabled) && !IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled);
        }

        public static bool KeyEmulationOnly()
        {
            ValidateFlag();
            return IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly);
        }

        public static bool FullDCSBIOSEnabled()
        {
            ValidateFlag();
            return IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled) && IsOperationModeFlagSet(OperationFlag.DCSBIOSInputEnabled);
        }

        public static bool PartialDCSBIOSEnabled()
        {
            ValidateFlag();
            return IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled) || IsOperationModeFlagSet(OperationFlag.DCSBIOSInputEnabled);
        }

        public static NumberFormatInfo GetPZ69FullDisplayNumberFormat()
        {
            if (_pz69NumberFormatInfoFullDisplay == null)
            {
                _pz69NumberFormatInfoFullDisplay = new NumberFormatInfo();
                _pz69NumberFormatInfoFullDisplay.NumberDecimalSeparator = ".";
                _pz69NumberFormatInfoFullDisplay.NumberDecimalDigits = 4;
                _pz69NumberFormatInfoFullDisplay.NumberGroupSeparator = "";
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
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpperInvariant();
        }

        public static NumberFormatInfo GetPZ69EmptyDisplayNumberFormat()
        {
            if (_pz69NumberFormatInfoEmpty == null)
            {
                _pz69NumberFormatInfoEmpty = new NumberFormatInfo();
                _pz69NumberFormatInfoEmpty.NumberDecimalSeparator = ".";
                _pz69NumberFormatInfoEmpty.NumberGroupSeparator = "";
            }
            return _pz69NumberFormatInfoEmpty;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }

        public static string GetDescriptionField(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribs = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attribs.Length > 0)
            {
                return ((DescriptionAttribute)attribs[0]).Description;
            }
            return string.Empty;
        }

        public static bool DebugOn { get; set; } = false;
        public static bool DebugToFile = false;
        public static APIModeEnum APIMode = 0;
        public static object _errorLoglockObject = new object();
        public static object _debugLoglockObject = new object();
        public static string ErrorLog = "";
        public static string DebugLog = "";

        public static void DebugP(string str)
        {
            if (DebugOn)
            {
                if (DebugToFile)
                {
                    LogToDebugFile(str);
                }
                else
                {
                    Console.WriteLine(str);
                }
            }
        }

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

        public static void LogError(Exception ex, string message = null)
        {
            LogError(0, ex, message);
        }

        public static void LogError(uint location, Exception ex, string message = null)
        {
            lock (_errorLoglockObject)
            {
                if (!File.Exists(ErrorLog))
                {
                    File.Create(ErrorLog);
                }
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var version = fileVersionInfo.FileVersion;

                var streamWriter = File.AppendText(ErrorLog);
                try
                {
                    streamWriter.Write(Environment.NewLine + DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + "  version : " + version);
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
                if (!File.Exists(ErrorLog))
                {
                    File.Create(ErrorLog);
                }
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var version = fileVersionInfo.FileVersion;

                var streamWriter = File.AppendText(ErrorLog);
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

        public static void ShowErrorMessageBox(uint location, Exception ex, string message = null)
        {
            LogError(location, ex, message);
            MessageBox.Show(location + " " + ex.Message, "Details logged to error log.");
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

        public static void WaitMilliSeconds(int millisecs)
        {
            var startMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var nowMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            while (nowMilliseconds - startMilliseconds < millisecs)
            {
                nowMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }
        }

        public static long MilliSecsNow()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }




        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static void LogToDebugFile(string message = null)
        {
            lock (_debugLoglockObject)
            {
                if (!File.Exists(DebugLog))
                {
                    File.Create(DebugLog);
                }

                var debugStreamWriter = File.AppendText(DebugLog);
                try
                {
                    debugStreamWriter.Write(Environment.NewLine + "Message = [" + message + "]" + Environment.NewLine);
                }
                finally
                {
                    debugStreamWriter.Close();
                }
            }
        }
    }


    public class SaitekPanelSkeleton
    {
        private SaitekPanelsEnum _saitekPanelsEnum = SaitekPanelsEnum.Unknown;
        private int _vendorId;
        private int _productId;
        private string _serialNumber;

        public SaitekPanelSkeleton(SaitekPanelsEnum saitekPanelsEnum, int vendorId, int productId)
        {
            _saitekPanelsEnum = saitekPanelsEnum;
            _vendorId = vendorId;
            _productId = productId;
        }

        public SaitekPanelSkeleton(SaitekPanelsEnum saitekPanelsEnum, int vendorId, int productId, string serialNumber)
        {
            _saitekPanelsEnum = saitekPanelsEnum;
            _vendorId = vendorId;
            _productId = productId;
            _serialNumber = serialNumber;
        }

        public bool HasSerialNumber()
        {
            return !string.IsNullOrEmpty(_serialNumber) && !_serialNumber.Equals("0");
        }

        public SaitekPanelsEnum SaitekPanelsType
        {
            get { return _saitekPanelsEnum; }
            set { _saitekPanelsEnum = value; }
        }

        public int VendorId
        {
            get { return _vendorId; }
            set { _vendorId = value; }
        }

        public int ProductId
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }
    }

}
