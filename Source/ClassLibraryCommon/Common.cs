using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;

namespace ClassLibraryCommon
{

    public static class Common
    {
        public static readonly List<SaitekPanelSkeleton> SaitekPanelSkeletons = new List<SaitekPanelSkeleton> { new SaitekPanelSkeleton(SaitekPanelsEnum.PZ55SwitchPanel, 0x6A3, 0xD67), new SaitekPanelSkeleton(SaitekPanelsEnum.PZ69RadioPanel, 0x6A3, 0xD05), new SaitekPanelSkeleton(SaitekPanelsEnum.PZ70MultiPanel, 0x6A3, 0xD06), new SaitekPanelSkeleton(SaitekPanelsEnum.BackLitPanel, 0x6A3, 0xB4E), new SaitekPanelSkeleton(SaitekPanelsEnum.TPM, 0x6A3, 0xB4D) };

        /*
        public static int EvaluateX(int data, String formula)
        {
            return EvaluateX(data.ToString(), formula);
        }
        
        public static int EvaluateX(String data, String formula)
        {
            try
            {
                var expression = new NCalc.Expression(formula);
                expression.Parameters["x"] = int.Parse(data);
                var result = expression.Evaluate();
                return Convert.ToInt32(Math.Abs((double)result));
            }
            catch (Exception ex)
            {
                LogError(1933494, ex, "Evaluate() function");
                throw;
            }
        }

        public static int Evaluate(String expression)
        {
            try
            {
                var nCalcExpression = new NCalc.Expression(expression);
                var result = nCalcExpression.Evaluate();
                var a = double.Parse(result.ToString());
                var b = Math.Abs(a);
                return Convert.ToInt32(b);
            }
            catch (Exception ex)
            {
                LogError(1933494, ex, "Evaluate() function");
                throw;
            }
        }
        */

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
        /*
        public static double Evaluate(string expression)
        {
            try
            {
                var engine = new CalculationEngine();
                return engine.Calculate(expression);
            }
            catch (Exception ex)
            {
                LogError(1933494, ex, "Evaluate() function");
                throw;
            }
        }*/

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

        private static bool _debugOn = false;
        public static bool DebugToFile = false;
        public static APIModeEnum APIMode = 0;
        public static object _errorLoglockObject = new object();
        public static object _debugLoglockObject = new object();
        public static string ErrorLog = "";
        public static string DebugLog = "";


        public static bool IsKeyEmulationProfile(DCSAirframe dcsAirframe)
        {
            return dcsAirframe == DCSAirframe.KEYEMULATOR || dcsAirframe == DCSAirframe.KEYEMULATOR_SRS;
        }

        public static bool IsDCSBIOSProfile(DCSAirframe dcsAirframe)
        {
            return dcsAirframe != DCSAirframe.KEYEMULATOR && dcsAirframe != DCSAirframe.KEYEMULATOR_SRS;
        }

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

        public static bool DebugOn
        {
            get { return _debugOn; }
            set
            {
                _debugOn = value;
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
