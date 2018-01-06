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
using Jace;

namespace NonVisuals
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

        private static readonly HashSet<VirtualKeyCode> Modifiers = new HashSet<VirtualKeyCode>();
        private static bool _debugOn = false;
        public static bool DebugToFile = false;
        public static APIModeEnum APIMode = 0;
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

        public static bool DebugOn
        {
            get { return _debugOn; }
            set
            {
                DCS_BIOS.DBCommon.Debug = value;
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

        public static HashSet<VirtualKeyCode> ModifierList()
        {
            if (Modifiers.Count == 0)
            {
                PopulateModifiersHashSet();
            }
            return Modifiers;
        }

        private static void PopulateModifiersHashSet()
        {
            Modifiers.Add(VirtualKeyCode.LSHIFT);
            Modifiers.Add(VirtualKeyCode.RSHIFT);
            Modifiers.Add(VirtualKeyCode.LCONTROL);
            Modifiers.Add(VirtualKeyCode.RCONTROL);
            Modifiers.Add(VirtualKeyCode.LWIN);
            Modifiers.Add(VirtualKeyCode.RWIN);
            Modifiers.Add(VirtualKeyCode.END);
            Modifiers.Add(VirtualKeyCode.DELETE);
            Modifiers.Add(VirtualKeyCode.INSERT);
            Modifiers.Add(VirtualKeyCode.HOME);
            Modifiers.Add(VirtualKeyCode.LEFT);
            Modifiers.Add(VirtualKeyCode.RIGHT);
            Modifiers.Add(VirtualKeyCode.UP);
            Modifiers.Add(VirtualKeyCode.DOWN);
            Modifiers.Add(VirtualKeyCode.DIVIDE);
            Modifiers.Add(VirtualKeyCode.MULTIPLY);
            Modifiers.Add(VirtualKeyCode.SUBTRACT);
            Modifiers.Add(VirtualKeyCode.ADD);
            Modifiers.Add(VirtualKeyCode.RETURN);
            Modifiers.Add(VirtualKeyCode.NUMLOCK);
            Modifiers.Add(VirtualKeyCode.LMENU);
            Modifiers.Add(VirtualKeyCode.RMENU);
        }

        public static bool IsModifierKeyDown()
        {
            foreach (var modifier in Modifiers)
            {
                if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)modifier)))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsModifierKey(VirtualKeyCode virtualKeyCode)
        {
            if (Modifiers.Count == 0)
            {
                PopulateModifiersHashSet();
            }
            foreach (var modifier in Modifiers)
            {
                if (virtualKeyCode == modifier)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsExtendedKey(VirtualKeyCode virtualKeyCode)
        {
            if (Modifiers.Count == 0)
            {
                PopulateModifiersHashSet();
            }


            /*Extended-Key Flag
                The extended-key flag indicates whether the keystroke message originated from one of the additional keys on the 
             * enhanced keyboard. 
             * The extended keys consist of the 
             * ALT and CTRL keys on the RIGHT HAND side of the keyboard; 
             * The INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; 
             * the NUM LOCK key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in the numeric
             * keypad. The extended-key flag is set if the key is an extended key.*/

            if (IsModifierKey(virtualKeyCode) &&
                (virtualKeyCode == VirtualKeyCode.RMENU ||
                 virtualKeyCode == VirtualKeyCode.RCONTROL ||
                 virtualKeyCode == VirtualKeyCode.INSERT ||
                 virtualKeyCode == VirtualKeyCode.DELETE ||
                 virtualKeyCode == VirtualKeyCode.HOME ||
                 virtualKeyCode == VirtualKeyCode.END ||
                 virtualKeyCode == VirtualKeyCode.PRIOR ||
                 virtualKeyCode == VirtualKeyCode.NEXT ||
                 virtualKeyCode == VirtualKeyCode.LEFT ||
                 virtualKeyCode == VirtualKeyCode.UP ||
                 virtualKeyCode == VirtualKeyCode.RIGHT ||
                 virtualKeyCode == VirtualKeyCode.DOWN ||
                 virtualKeyCode == VirtualKeyCode.NUMLOCK ||
                 virtualKeyCode == VirtualKeyCode.PRINT ||
                 virtualKeyCode == VirtualKeyCode.DIVIDE ||
                 virtualKeyCode == VirtualKeyCode.MULTIPLY ||
                 virtualKeyCode == VirtualKeyCode.RETURN))
            {
                return true;
            }
            return false;
            //All modifiers except LSHIFT / RSHIFT are extended keys.                    
            /* EXTENDED KEYS :
                        RIGHT ALT
                        RIGHT CTRL
                        INS
                        DEL
                        HOME
                        END
                        PAGE UP
                        PAGE DOWN
                        ARROW KEYS
                        NUM LOCK
                        BREAK (CTRL+PAUSE)
                        PRINT SCRN
                        DIVIDE
                        MULTIPLY
                        ENTER
                     */
        }

        /*
        public static HashSet<VirtualKeyCode> GetPressedVirtualKeyCodesThatAreModifiers(KeyEventArgs e)
        {
            var virtualKeyCodeHolders = new HashSet<VirtualKeyCode>();

            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LMENU);
            }
            if (Keyboard.IsKeyDown(Key.RightAlt))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RMENU);
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LCONTROL);
            }
            if (Keyboard.IsKeyDown(Key.RightCtrl))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RCONTROL);
            }
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LSHIFT);
            }
            if (Keyboard.IsKeyDown(Key.RightShift))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RSHIFT);
            }
            if (Keyboard.IsKeyDown(Key.RWin))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RWIN);
            }
            if (Keyboard.IsKeyDown(Key.LWin))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LWIN);
            }
            if (Keyboard.IsKeyDown(Key.End))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.END);
            }
            if (Keyboard.IsKeyDown(Key.Delete))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DELETE);
            }
            if (Keyboard.IsKeyDown(Key.Insert))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.INSERT);
            }
            if (Keyboard.IsKeyDown(Key.Home))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.HOME);
            }
            if (Keyboard.IsKeyDown(Key.Left))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LEFT);
            }
            if (Keyboard.IsKeyDown(Key.Up))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DOWN);
            }
            if (Keyboard.IsKeyDown(Key.Divide))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DIVIDE);
            }
            if (Keyboard.IsKeyDown(Key.Multiply))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.MULTIPLY);
            }
            if (Keyboard.IsKeyDown(Key.Subtract))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.SUBTRACT);
            }
            if (Keyboard.IsKeyDown(Key.Add))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.ADD);
            }
            if (Keyboard.IsKeyDown(Key.Return))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RETURN);
            }
            if (Keyboard.IsKeyDown(Key.NumLock))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.NUMLOCK);
            }
            /*if (Keyboard.IsKeyDown(Key.))
            {
                virtualKeyCodeHolders.Add(Key.);
            }
            if (Keyboard.IsKeyDown(Key.))
            {
                virtualKeyCodeHolders.Add(Key.);
            }
            return virtualKeyCodeHolders;
        }
    */
        public static HashSet<VirtualKeyCode> GetPressedVirtualKeyCodesThatAreModifiers()
        {
            var virtualKeyCodeHolders = new HashSet<VirtualKeyCode>();

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LSHIFT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LSHIFT);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RSHIFT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RSHIFT);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LCONTROL)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LCONTROL);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RCONTROL)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RCONTROL);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LWIN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LWIN);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RWIN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RWIN);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.END)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.END);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.DELETE)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DELETE);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.INSERT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.INSERT);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.HOME)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.HOME);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LEFT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LEFT);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RIGHT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RIGHT);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.UP)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.UP);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.DOWN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DOWN);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.DIVIDE)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DIVIDE);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.MULTIPLY)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.MULTIPLY);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.SUBTRACT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.SUBTRACT);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.ADD)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.ADD);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RETURN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RETURN);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.NUMLOCK)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.NUMLOCK);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LMENU)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LMENU);
            }
            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RMENU)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RMENU);
            }
            return virtualKeyCodeHolders;
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

    public enum KeyPressLength
    {
        //Zero = 0, <-- DCS & keybd_event fungerar inte utan fördröjning mellan tangent tryck & släpp
        //Indefinite = 0,
        FiftyMilliSec = 50,
        HalfSecond = 500,
        Second = 1000,
        SecondAndHalf = 1500,
        TwoSeconds = 2000,
        ThreeSeconds = 3000,
        FourSeconds = 4000,
        FiveSecs = 5000,
        TenSecs = 10000,
        FifteenSecs = 15000,
        TwentySecs = 20000,
        ThirtySecs = 30000,
        FortySecs = 40000,
        SixtySecs = 60000
    }

    public enum APIModeEnum
    {
        keybd_event = 0,
        SendInput = 1
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
