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
        public static readonly List<GamingPanelSkeleton> GamingPanelSkeletons = new List<GamingPanelSkeleton>
        {
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ69RadioPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.BackLitPanel),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.TPM),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckMini),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeck),
            new GamingPanelSkeleton(GamingPanelVendorEnum.Elgato, GamingPanelEnum.StreamDeckXL),
        };

        private static NumberFormatInfo _pz69NumberFormatInfoFullDisplay;
        private static NumberFormatInfo _pz69NumberFormatInfoEmpty;

        private static int _operationLevelFlag = 0;


        public static bool UseGenericRadio = false;

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
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
            return string.Empty;
        }

        public static bool DebugOn { get; set; } = false;
        public static bool DebugToFile = false;
        public static APIModeEnum APIMode = 0;
        private static readonly object ErrorLogLockObject = new object();
        private static readonly object DebugLogLockObject = new object();
        private static string _errorLog = "";
        private static string _debugLog = "";

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
                    Debug.WriteLine(str);
                }
            }
        }

        public static void SetErrorLog(string filename)
        {
            lock (ErrorLogLockObject)
            {
                _errorLog = filename;
            }
        }

        public static void SetDebugLog(string filename)
        {
            lock (DebugLogLockObject)
            {
                _debugLog = filename;
            }
        }

        public static void Log(string message)
        {
            try
            {
                lock (ErrorLogLockObject)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    var version = fileVersionInfo.FileVersion;

                    if (!File.Exists(_errorLog))
                    {
                        var fileStream = File.Create(_errorLog);
                        fileStream.Close();
                    }
                    var tempFile = Path.GetTempFileName();
                    using (var streamWriter = new StreamWriter(tempFile))
                    using (var streamReader = File.OpenText(_errorLog))
                    {
                        streamWriter.Write(Environment.NewLine + DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + "  version : " + version);
                        streamWriter.Write(message + Environment.NewLine);
                        while (!streamReader.EndOfStream)
                        {
                            streamWriter.WriteLine(streamReader.ReadLine());
                        }
                    }
                    File.Copy(tempFile, _errorLog, true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error writing to error log. Please restart DCSFP. " + e.Message);
            }
        }

        public static void LogError(Exception ex, string message = null)
        {
            LogError(0, ex, message);
        }

        public static void LogError(uint location, Exception ex, string message = null)
        {
            Log(Environment.NewLine + location + " Custom message = [" + message + "]" + Environment.NewLine + ex.GetBaseException().GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
        }


        public static void LogError(uint location, string message)
        {
            Log(Environment.NewLine + location + " Message = [" + message + "]" + Environment.NewLine);
        }

        public static void ShowErrorMessageBox(uint location, Exception ex, string message = null)
        {
            LogError(location, ex, message);
            MessageBox.Show(location + " " + ex.Message, "Details logged to error log.", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrorMessageBox(Exception ex, string message = null)
        {
            LogError(ex, message);
            MessageBox.Show(ex.Message, "Details logged to error log.\n" + ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowMessageBox(string text, string header = "Information")
        {
            MessageBox.Show(text, header, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string GetStackTrace(string[] traceLineMustInclude, string header = "Stacktrace")
        {
            var stacktrace = Environment.StackTrace;
            var lines = stacktrace.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (var line in lines)
            {
                bool found = false;
                foreach (var mustIncludeProject in traceLineMustInclude)
                {
                    if (line.Contains(mustIncludeProject))
                    {
                        found = true;
                        break;
                    }
                }

                if (found && !line.Contains("LogStackTrace") && !line.Contains("GetStackTrace") && !line.Contains("ShowStackTraceBox"))
                {
                    result.Append(line);
                }
            }

            return result.ToString();
        }

        public static void LogStackTrace(string[] traceLineMustInclude, string header = "Stacktrace")
        {
            var stackTrace = GetStackTrace(traceLineMustInclude, header);
            Log("  This is a logged Stacktrace\n" + stackTrace);
        }

        public static void ShowStackTraceBox(string[] traceLineMustInclude, string header = "Stacktrace")
        {
            MessageBox.Show(GetStackTrace(traceLineMustInclude, header), header, MessageBoxButton.OK, MessageBoxImage.Information);
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

        public static void LogToDebugFile(string message = null)
        {
            lock (DebugLogLockObject)
            {
                if (!File.Exists(_debugLog))
                {
                    File.Create(_debugLog);
                }

                var debugStreamWriter = File.AppendText(_debugLog);
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


    public class GamingPanelSkeleton
    {
        private GamingPanelEnum _gamingPanelsEnum = GamingPanelEnum.Unknown;
        private int _vendorId;
        private int _productId;
        private string _serialNumber;

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum)
        {
            _gamingPanelsEnum = gamingPanelsEnum;
            _vendorId = (int)gamingPanelVendor;
            _productId = (int)gamingPanelsEnum;
        }

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum, string serialNumber)
        {
            _gamingPanelsEnum = gamingPanelsEnum;
            _vendorId = (int)gamingPanelVendor;
            _productId = (int)gamingPanelsEnum;
            _serialNumber = serialNumber;
        }

        public bool HasSerialNumber()
        {
            return !string.IsNullOrEmpty(_serialNumber) && !_serialNumber.Equals("0");
        }

        public GamingPanelEnum GamingPanelType
        {
            get { return _gamingPanelsEnum; }
            set { _gamingPanelsEnum = value; }
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
