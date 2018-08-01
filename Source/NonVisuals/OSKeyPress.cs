using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class KeyPressInfo
    {
        private KeyPressLength _lengthOfBreak = KeyPressLength.FiftyMilliSec;
        private KeyPressLength _lengthOfKeyPress = KeyPressLength.FiftyMilliSec;
        private HashSet<VirtualKeyCode> _virtualKeyCodes = new HashSet<VirtualKeyCode>();

        public KeyPressLength LengthOfBreak
        {
            get => _lengthOfBreak;
            set => _lengthOfBreak = value;
        }

        public KeyPressLength LengthOfKeyPress
        {
            get => _lengthOfKeyPress;
            set => _lengthOfKeyPress = value;
        }

        public HashSet<VirtualKeyCode> VirtualKeyCodes
        {
            get => _virtualKeyCodes;
            set => _virtualKeyCodes = value;
        }

        public string VirtualKeyCodesAsString
        {
            get
            {
                var result = new StringBuilder();
                if (_virtualKeyCodes.Count > 0)
                {
                    foreach (var virtualKeyCode in _virtualKeyCodes)
                    {
                        if (result.Length > 0)
                        {
                            result.Append(" + ");
                        }
                        result.Append(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                    }
                }
                return result.ToString();
            }
        }
    }

    public class OSKeyPress
    {
        private SortedList<int, KeyPressInfo> _sortedKeyPressInfoList = new SortedList<int, KeyPressInfo>();
        private string _information = "Key press sequence";
        private Thread _executingThread;
        private long _abortCurrentSequence;
        private long _threadHasFinished = 1;

        public OSKeyPress() { }

        public OSKeyPress(string keycodes, KeyPressLength keyPressLength = KeyPressLength.FiftyMilliSec, string information = null)
        {
            var keyPressInfo = new KeyPressInfo();
            keyPressInfo.VirtualKeyCodes = SplitStringKeyCodes(keycodes);
            _sortedKeyPressInfoList.Add(GetNewKeyValue(), keyPressInfo);
            keyPressInfo.LengthOfKeyPress = keyPressLength;
            _information = information;
        }

        public OSKeyPress(string information, SortedList<int, KeyPressInfo> sortedList)
        {
            _information = information;
            _sortedKeyPressInfoList = sortedList;
        }


        ~OSKeyPress()
        {
            _executingThread?.Abort();
        }

        private void SetAbortThreadState()
        {
            Interlocked.Exchange(ref _abortCurrentSequence, 1);
        }

        private void ResetAbortThreadState()
        {
            Interlocked.Exchange(ref _abortCurrentSequence, 0);
        }

        private bool AbortThread()
        {
            return Interlocked.Read(ref _abortCurrentSequence) == 1;
        }

        private bool ThreadHasFinished()
        {
            return Interlocked.Read(ref _threadHasFinished) == 1;
        }

        private void SignalThreadHasFinished()
        {
            Interlocked.Exchange(ref _threadHasFinished, 1);
        }

        private void ResetThreadHasFinishedState()
        {
            Interlocked.Exchange(ref _threadHasFinished, 0);
        }

        public void Execute()
        {
            try
            {
                //PrintInterlockedVars(0);
                //Check for already executing key sequence which may use long timings and breaks
                if (!ThreadHasFinished() && _executingThread != null)
                {
                    SetAbortThreadState();
                    //PrintInterlockedVars(1);
                    while (!ThreadHasFinished())
                    {
                        Thread.Sleep(50);
                    }
                    ResetAbortThreadState();
                    ResetThreadHasFinishedState();
                    //PrintInterlockedVars(2);
                    _executingThread = new Thread(() => ExecuteThreaded(_sortedKeyPressInfoList));
                    _executingThread.Start();
                }
                else
                {
                    ResetThreadHasFinishedState();
                    _executingThread = new Thread(() => ExecuteThreaded(_sortedKeyPressInfoList));
                    _executingThread.Start();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1027, ex);
            }
        }

        private void ExecuteThreaded(SortedList<int, KeyPressInfo> sortedList)
        {
            try
            {

                try
                {
                    if (sortedList == null)
                    {
                        return;
                    }
                    for (var i = 0; i < sortedList.Count; i++)
                    {
                        var keyPressInfo = sortedList[i];
                        if (keyPressInfo.VirtualKeyCodes == null || keyPressInfo.VirtualKeyCodes.Count == 0)
                        {
                            return;
                        }
                        var array = keyPressInfo.VirtualKeyCodes.ToArray();
                        Common.DebugP("-----------------------------------");
                        foreach (var virtualKeyCode in array)
                        {
                            Common.DebugP(virtualKeyCode + " " + CommonVK.IsModifierKey(virtualKeyCode));
                        }
                        Common.DebugP("-----------------------------------");
                        if (Common.APIMode == APIModeEnum.keybd_event)
                        {
                            KeyBdEventAPI(keyPressInfo.LengthOfBreak, array, keyPressInfo.LengthOfKeyPress);
                            //Common.DebugP("KeyBdEventAPI result code -----------------------------------> " + Marshal.GetLastWin32Error());
                        }
                        else
                        {
                            SendKeys(keyPressInfo.LengthOfBreak, array, keyPressInfo.LengthOfKeyPress);
                            //Common.DebugP("SendKeys result code -----------------------------------> " + Marshal.GetLastWin32Error());
                        }
                        if (AbortThread())
                        {
                            Common.DebugP("Aborting key pressing routine (AbortThread)");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message);
                }
            }
            finally
            {
                SignalThreadHasFinished();
            }
        }

        private void KeyBdEventAPI(KeyPressLength breakLength, VirtualKeyCode[] virtualKeyCodes, KeyPressLength keyPressLength)
        {
            var keyPressLengthTimeConsumed = 0;
            var breakLengthConsumed = 0;
            /*
                //keybd_event
                http://msdn.microsoft.com/en-us/library/windows/desktop/ms646304%28v=vs.85%29.aspx
            */
            while (breakLengthConsumed < (int)breakLength)
            {
                Thread.Sleep(50);
                breakLengthConsumed += 50;
                if (AbortThread())
                {
                    return;
                }
            }
            //Press modifiers
            for (var i = 0; i < virtualKeyCodes.Count(); i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (CommonVK.IsModifierKey(virtualKeyCode))
                {
                    Common.DebugP(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode) + " is MODIFIER = " + CommonVK.IsExtendedKey(virtualKeyCode));
                    if (CommonVK.IsExtendedKey(virtualKeyCode))
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode, (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), (int)NativeMethods.KEYEVENTF_EXTENDEDKEY | 0, 0);
                        //keybd_event(VK_LCONTROL, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    }
                    else
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode, (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), 0, 0);
                    }
                }
            }

            //Press normal keys
            for (var i = 0; i < virtualKeyCodes.Count(); i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (!CommonVK.IsModifierKey(virtualKeyCode))
                {
                    NativeMethods.keybd_event((byte)virtualKeyCode, (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), 0, 0);
                }
            }

            /*if (keyPressLength == KeyPressLength.Indefinite)
            {
                return;
            }*/

            while (keyPressLengthTimeConsumed < (int)keyPressLength)
            {
                Thread.Sleep(50);
                keyPressLengthTimeConsumed += 50;
                if (AbortThread())
                {
                    //If we are to cancel the whole operation. Release pressed keys ASAP and exit.
                    break;
                }
            }
            //Release normal keys
            for (var i = 0; i < virtualKeyCodes.Count(); i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (!CommonVK.IsModifierKey(virtualKeyCode))
                {
                    NativeMethods.keybd_event((byte)virtualKeyCode, (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), (int)NativeMethods.KEYEVENTF_KEYUP, 0);
                }
            }

            //Release modifiers
            for (var i = 0; i < virtualKeyCodes.Count(); i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (CommonVK.IsModifierKey(virtualKeyCode))
                {
                    Common.DebugP(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode) + " is MODIFIER = " + CommonVK.IsExtendedKey(virtualKeyCode));
                    if (CommonVK.IsExtendedKey(virtualKeyCode))
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode, (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), (int)(NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP), 0);
                    }
                    else
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode, (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), (int)NativeMethods.KEYEVENTF_KEYUP, 0);
                    }
                }
            }
        }

        public string GetVirtualKeyCodesAsString(KeyPressInfo keyPressInfo)
        {
            var result = new StringBuilder();

            if (keyPressInfo.VirtualKeyCodes.Count > 0)
            {
                foreach (var virtualKeyCode in keyPressInfo.VirtualKeyCodes)
                {
                    if (result.Length > 0)
                    {
                        result.Append(" + ");
                    }
                    result.Append(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
            }
            return result.ToString();
        }

        public string GetNonFunctioningVirtualKeyCodesAsString()
        {
            var result = new StringBuilder();

            for (var i = 0; i < _sortedKeyPressInfoList.Count; i++)
            {
                var keyPressInfo = _sortedKeyPressInfoList[i];
                if (keyPressInfo.VirtualKeyCodes.Count > 0)
                {
                    foreach (var virtualKeyCode in keyPressInfo.VirtualKeyCodes)
                    {
                        if (result.Length > 0)
                        {
                            result.Append(" + ");
                        }
                        result.Append(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                    }
                }
            }
            //Insert 1. or 2. indicating API used
            result.Insert(0, Common.APIMode == APIModeEnum.keybd_event ? "1. " : "2. ");
            return result.ToString();
        }

        public string GetSimpleVirtualKeyCodesAsString()
        {
            if (IsMultiSequenced())
            {
                throw new Exception("GetSimpleVirtualKeyCodesAsString() called for multisequenced key press. Use IsMultiSequenced() to check whether simple or multisequenced.");
            }
            var result = new StringBuilder();

            var keyPressInfo = _sortedKeyPressInfoList[0];
            if (keyPressInfo.VirtualKeyCodes.Count > 0)
            {
                foreach (var virtualKeyCode in keyPressInfo.VirtualKeyCodes)
                {
                    if (result.Length > 0)
                    {
                        result.Append(" + ");
                    }
                    result.Append(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
            }

            return result.ToString();
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (str.Contains("["))
            {
                ImportStringMultiKeySequence(str);
            }
            else
            {
                ImportStringSingleKeySequence(str);
            }
        }

        private void ImportStringSingleKeySequence(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException("Import string empty. (OSKeyPress)");
                }
                if (!str.StartsWith("OSKeyPress{") || !str.EndsWith("}"))
                {
                    throw new ArgumentException("Import string format exception. (OSKeyPress) >" + str + "<");
                }
                var keyPressInfo = new KeyPressInfo();
                //OSKeyPress{1000,VK_D + RETURN + ...}
                var dataString = str.Remove(0, 11);
                //1000,VK_D + RETURN + ...}
                dataString = dataString.Remove(dataString.Length - 1, 1);
                //1000,VK_D + RETURN + ... + ...
                keyPressInfo.LengthOfKeyPress =
                    (KeyPressLength)
                    Enum.Parse(typeof(KeyPressLength),
                               dataString.Substring(0, dataString.IndexOf(",", StringComparison.Ordinal)));
                dataString = dataString.Substring(dataString.IndexOf(",", StringComparison.Ordinal) + 1);
                //VK_D + RETURN + ... + ...
                keyPressInfo.VirtualKeyCodes = SplitStringKeyCodes(dataString);
                _sortedKeyPressInfoList.Add(GetNewKeyValue(), keyPressInfo);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1028, ex);
            }
        }

        private void ImportStringMultiKeySequence(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException("Import string empty. (OSKeyPress)");
                }
                if (!str.StartsWith("OSKeyPress{") || !str.EndsWith("}"))
                {
                    throw new ArgumentException("Import string format exception. (OSKeyPress) >" + str + "<");
                }
                //OSKeyPress{INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]}
                var dataString = str.Remove(0, 11);
                //INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]}
                dataString = dataString.Remove(dataString.Length - 1, 1);
                //INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]
                if (dataString.Contains("INFORMATION=^"))
                {
                    //INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]
                    var temp = dataString.Remove(0, 13);
                    //DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]
                    _information = temp.Substring(0, temp.IndexOf("^", StringComparison.InvariantCultureIgnoreCase));
                    dataString = temp.Remove(0, _information.Length + 1);
                }
                var array = dataString.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                //[FiftyMilliSec,VK_A,FiftyMilliSec]
                //[FiftyMilliSec,VK_B,FiftyMilliSec]
                //[FiftyMilliSec,VK_C,FiftyMilliSec]
                //[FiftyMilliSec,VK_D,FiftyMilliSec]}
                // ...
                for (int i = 0; i < array.Count(); i++)
                {
                    var keyPressInfo = new KeyPressInfo();
                    var entry = array[i];
                    entry = entry.Replace("[", "");
                    entry = entry.Replace("]", "");
                    //FiftyMilliSec,VK_A,FiftyMilliSec
                    keyPressInfo.LengthOfBreak = (KeyPressLength)Enum.Parse(typeof(KeyPressLength), entry.Substring(0, entry.IndexOf(",", StringComparison.Ordinal)));
                    entry = entry.Substring(entry.IndexOf(",", StringComparison.Ordinal) + 1);
                    var keys = entry.Substring(0, entry.IndexOf(",", StringComparison.Ordinal));
                    keyPressInfo.VirtualKeyCodes = SplitStringKeyCodes(keys);
                    keyPressInfo.LengthOfKeyPress = (KeyPressLength)Enum.Parse(typeof(KeyPressLength), entry.Substring(entry.IndexOf(",", StringComparison.Ordinal) + 1)); ;
                    _sortedKeyPressInfoList.Add(GetNewKeyValue(), keyPressInfo);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1070, ex);
                ;
            }
        }

        public KeyPressLength GetLengthOfKeyPress()
        {
            if (IsMultiSequenced())
            {
                throw new Exception("Key press is multisequenced. Cannot query single key press length as it may contain many such values. (OSKeyPress.GetLengthOfKeyPress())");
            }
            return _sortedKeyPressInfoList[0].LengthOfKeyPress;
        }

        public void SetLengthOfKeyPress(KeyPressLength keyPressLength)
        {
            if (IsMultiSequenced())
            {
                throw new Exception("Key press is multisequenced. Cannot set single key press length as it may contain many such values. (OSKeyPress.SetLengthOfKeyPress())");
            }
            _sortedKeyPressInfoList[0].LengthOfKeyPress = keyPressLength;
        }

        public static HashSet<VirtualKeyCode> SplitStringKeyCodes(string str)
        {
            var result = new HashSet<VirtualKeyCode>();
            try
            {
                var split = str.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in split)
                {
                    var virtualKeyCode = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), s.Trim());
                    result.Add(virtualKeyCode);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1080, ex);
            }
            return result;
        }

        public string Information
        {
            get => _information;
            set => _information = value;
        }

        public bool IsEmpty()
        {
            return _sortedKeyPressInfoList == null || _sortedKeyPressInfoList.Count == 0;
        }

        public SortedList<int, KeyPressInfo> KeySequence
        {
            get => _sortedKeyPressInfoList;
            set => _sortedKeyPressInfoList = value;
        }

        public string ExportString()
        {
            if (!IsEmpty() && !IsMultiSequenced())
            {
                var keyPressInfo = _sortedKeyPressInfoList[0];
                return "OSKeyPress{" + Enum.GetName(typeof(KeyPressLength), keyPressInfo.LengthOfKeyPress) + "," + GetSimpleVirtualKeyCodesAsString() + "}";
            }
            var result = new StringBuilder();
            result.Append("OSKeyPress{");
            if (!string.IsNullOrEmpty(_information))
            {
                result.Append("INFORMATION=^" + _information + "^");
            }
            for (var i = 0; i < _sortedKeyPressInfoList.Count; i++)
            {
                var keyPressInfo = _sortedKeyPressInfoList[i];
                result.Append("[" + Enum.GetName(typeof(KeyPressLength), keyPressInfo.LengthOfBreak) + "," + GetVirtualKeyCodesAsString(keyPressInfo) + "," + Enum.GetName(typeof(KeyPressLength), keyPressInfo.LengthOfKeyPress) + "]");
            }
            result.Append("}");
            return result.ToString();
        }

        public bool IsMultiSequenced()
        {
            return _sortedKeyPressInfoList.Count > 1;
        }

        private int GetNewKeyValue()
        {
            if (_sortedKeyPressInfoList.Count == 0)
            {
                return 0;
            }
            return _sortedKeyPressInfoList.Keys.Max() + 1;
        }

        /*
         * DO NOT USE BAD CODING /JERKER
        private static void WaiatMilliSeconds(KeyPressLength keyPressLength)
        {
            var once = true;
            var startMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var nowMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            while (nowMilliseconds - startMilliseconds < (int)keyPressLength)
            {
                if (Common.Debug && once)
                {
                    Common.DebugP("Waiting " + keyPressLength + " ms.");
                    once = false;
                }
                nowMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }
        }
        */
        /*
             * SendInput är den korrekta funktionen att använda idag (13.1.2014) då keybd_event inte längre ska användas.
             * DCS dock fungerar inte med SendInput, LCONTROL,RCONTROL,LSHIFT,LALT,RSHIFT,RALT tas inte emot på korrekt sätt.
             * 
             * 
        */
        public void SendKeys(KeyPressLength breakLength, VirtualKeyCode[] virtualKeyCodes, KeyPressLength keyPressLength)
        {
            var keyPressLengthTimeConsumed = 0;
            var breakLengthConsumed = 0;
            while (breakLengthConsumed < (int)breakLength)
            {
                Thread.Sleep(50);
                breakLengthConsumed += 50;
                if (AbortThread())
                {
                    return;
                }
            }
            var inputs = new NativeMethods.INPUT[virtualKeyCodes.Count()];

            var modifierCount = 0;
            foreach (var virtualKeyCode in virtualKeyCodes)
            {
                if (CommonVK.IsModifierKey(virtualKeyCode))
                {
                    modifierCount++;
                }
            }
            //Add modifiers
            for (var i = 0; i < virtualKeyCodes.Count(); i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (CommonVK.IsModifierKey(virtualKeyCode))
                {
                    Common.DebugP("INSERTING [] AT " + i + " total position are " + inputs.Count());
                    inputs[i].type = NativeMethods.INPUT_KEYBOARD;
                    inputs[i].InputUnion.ki.time = 0;
                    inputs[i].InputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_SCANCODE;
                    Common.DebugP(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode) + " is MODIFIER = " + CommonVK.IsExtendedKey(virtualKeyCode));
                    if (CommonVK.IsExtendedKey(virtualKeyCode))
                    {
                        inputs[i].InputUnion.ki.dwFlags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
                    }
                    inputs[i].InputUnion.ki.wVk = 0;
                    Common.DebugP("***********\nMapVirtualKey returned " + Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode) + " : " + NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0) + "\n************");
                    inputs[i].InputUnion.ki.wScan = (ushort)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0);
                    inputs[i].InputUnion.ki.dwExtraInfo = NativeMethods.GetMessageExtraInfo();
                }
            }
            //[x][x] [] []
            // 0  1  2  3
            // 1  2  3  4
            //Add normal keys
            for (var i = modifierCount; i < virtualKeyCodes.Count(); i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (!CommonVK.IsModifierKey(virtualKeyCode))
                {
                    Common.DebugP("INSERTING [] AT " + i + " total position are " + inputs.Count());
                    inputs[i].type = NativeMethods.INPUT_KEYBOARD;
                    inputs[i].InputUnion.ki.time = 0;
                    inputs[i].InputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_SCANCODE;

                    inputs[i].InputUnion.ki.wVk = 0;
                    Common.DebugP("***********\nMapVirtualKey returned " + Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode) + " : " + NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0) + "\n************");
                    inputs[i].InputUnion.ki.wScan = (ushort)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0);
                    inputs[i].InputUnion.ki.dwExtraInfo = NativeMethods.GetMessageExtraInfo();
                }
            }

            NativeMethods.SendInput((uint)inputs.Count(), inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

            /*if (keyPressLength == KeyPressLength.Indefinite)
            {
                return;
            }*/

            while (keyPressLengthTimeConsumed < (int)keyPressLength)
            {
                Thread.Sleep(50);
                keyPressLengthTimeConsumed += 50;
                if (AbortThread())
                {
                    //If we are to cancel the whole operation. Release pressed keys ASAP and exit.
                    break;
                }
            }
            for (var i = 0; i < inputs.Count(); i++)
            {
                inputs[i].InputUnion.ki.dwFlags |= NativeMethods.KEYEVENTF_KEYUP;
            }
            Array.Reverse(inputs);
            //Release same keys
            NativeMethods.SendInput((uint)inputs.Count(), inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }
    }


}

