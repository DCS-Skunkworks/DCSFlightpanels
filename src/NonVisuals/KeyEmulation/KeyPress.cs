using System.Diagnostics;

namespace NonVisuals.KeyEmulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;


    /// <summary>
    /// This class handles key emulation. It is mapped together with a physical switch/key
    /// by one of the binding classes.
    /// This class if responsible for importing, exporting settings for a certain keypress
    /// as well as executing the key press using a Windows API.
    /// </summary>
    [Serializable]
    [SerializeCritical]
    public class KeyPress
    {
        private const int SLEEP_VALUE = 32;
        private SortedList<int, IKeyPressInfo> _sortedKeyPressInfoList = new();
        private string _description = "Key press sequence";
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        [NonSerialized] private Thread _executingThread;
        [JsonIgnore] public bool HasSequence => _sortedKeyPressInfoList.Count > 1;

        /*
         * When this OSKeyPress Executes it should cancel any execution _negatorOSKeyPress does.
         * No need for constructor of this. It is not known at startup whether there are negators,
         * depends on what the user has configured.
         * It is the binding class that must make sure to set these.
         */
        private List<KeyPress> _negatorOSKeyPresses = new();
        private volatile bool _abort;



        public int GetHash()
        {
            unchecked
            {
                var result = 0;
                foreach (var tuple in _sortedKeyPressInfoList)
                {
                    result = result * 397 ^ tuple.Value.GetHash();
                }

                result = result * 397 ^ (string.IsNullOrWhiteSpace(_description) ? 0 : _description.GetHashCode());
                return result;
            }
        }

        public KeyPress() { }

        public KeyPress(string keycodes, KeyPressLength keyPressLength = KeyPressLength.FiftyMilliSec, string description = null)
        {
            try
            {
                var keyPressInfo = new KeyPressInfo
                {
                    VirtualKeyCodes = SplitStringKeyCodes(keycodes)
                };
                _sortedKeyPressInfoList.Add(GetNewKeyValue(), keyPressInfo);
                keyPressInfo.LengthOfKeyPress = keyPressLength;
                _description = description;
            }
            catch (Exception e)
            {
                throw new Exception($"Error creating KeyPress object. {e.Message}");
            }
        }

        public KeyPress(string information, SortedList<int, IKeyPressInfo> sortedList)
        {
            _description = information;
            _sortedKeyPressInfoList = sortedList;
        }


        ~KeyPress()
        {
            Abort = true;
        }


        [JsonIgnore]
        public List<KeyPress> NegatorOSKeyPresses
        {
            get => _negatorOSKeyPresses;
            set => _negatorOSKeyPresses = value;
        }

        public bool IsRunning()
        {
            return _executingThread != null && (_executingThread.ThreadState == ThreadState.Running ||
                                                _executingThread.ThreadState == ThreadState.WaitSleepJoin ||
                                                _executingThread.ThreadState == ThreadState.Unstarted);
        }

        public void Execute(CancellationToken outerCancellationToken, bool useThread = true)
        {
            try
            {
                Debug.WriteLine($"Looking for negators. Count is {_negatorOSKeyPresses.Count}");
                foreach (var negatorOSKeyPress in _negatorOSKeyPresses)
                {
                    negatorOSKeyPress.Abort = true;
                }

                // Check for already executing key sequence which may use long timings and breaks
                if (IsRunning() && _executingThread != null)
                {
                    Abort = true;
                    while (IsRunning())
                    {
                        Thread.Sleep(SLEEP_VALUE);
                    }
                }

                if (useThread)
                {
                    Abort = false;
                    _executingThread = new Thread(() => ExecuteKeyPresses(_sortedKeyPressInfoList, outerCancellationToken));
                    _executingThread.Start();
                }
                else
                {
                    ExecuteKeyPresses(_sortedKeyPressInfoList, outerCancellationToken);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ExecuteKeyPresses(SortedList<int, IKeyPressInfo> sortedList, CancellationToken outerCancellationToken)
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

                    if (Common.APIModeUsed == APIModeEnum.KeybdEvent)
                    {
                        KeyBdEvenAPI.Press(keyPressInfo.LengthOfBreak, keyPressInfo.VirtualKeyCodes.ToArray(), keyPressInfo.LengthOfKeyPress, _cancellationTokenSource.Token,outerCancellationToken, SLEEP_VALUE);
                        // Common.DebugP("KeyBdEventAPI result code -----------------------------------> " + Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        SendKeys.Press(keyPressInfo.LengthOfBreak, keyPressInfo.VirtualKeyCodes.ToArray(), keyPressInfo.LengthOfKeyPress, _cancellationTokenSource.Token, outerCancellationToken, SLEEP_VALUE);
                    }

                    if (Abort)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        public static string GetVirtualKeyCodesAsString(IKeyPressInfo keyPressInfo)
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

            // Insert 1. or 2. indicating API used
            result.Insert(0, Common.APIModeUsed == APIModeEnum.KeybdEvent ? "1. " : "2. ");
            return result.ToString();
        }

        /*
         * Added because Stream Deck (JSON) had this property before. Dirty fix.
         */
        [JsonProperty("Information", Required = Required.Default)]
        public string Information
        {
            get; set;
        }

        public string GetKeyPressInformation()
        {
            if (IsMultiSequenced())
            {
                if (!string.IsNullOrWhiteSpace(Description))
                {
                    return Description;
                }

                return "key press sequence";
            }

            return GetSimpleVirtualKeyCodesAsString();
        }

        private string GetSimpleVirtualKeyCodesAsString()
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

            if (str.Contains('['))
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

                // OSKeyPress{1000,VK_D + RETURN + ...}
                var dataString = str.Remove(0, 11);

                // 1000,VK_D + RETURN + ...}
                dataString = dataString.Remove(dataString.Length - 1, 1);

                // 1000,VK_D + RETURN + ... + ...
                keyPressInfo.LengthOfKeyPress = (KeyPressLength)Enum.Parse(typeof(KeyPressLength), dataString.AsSpan(0, dataString.IndexOf(",", StringComparison.Ordinal)));
                dataString = dataString.Substring(dataString.IndexOf(",", StringComparison.Ordinal) + 1);

                // VK_D + RETURN + ... + ...
                keyPressInfo.VirtualKeyCodes = SplitStringKeyCodes(dataString);
                _sortedKeyPressInfoList.Add(GetNewKeyValue(), keyPressInfo);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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

                // OSKeyPress{INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]}
                var dataString = str.Remove(0, 11);

                // INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]}
                dataString = dataString.Remove(dataString.Length - 1, 1);

                // INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]
                if (dataString.Contains("INFORMATION=^"))
                {
                    // INFORMATION=^DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]
                    var temp = dataString.Remove(0, 13);

                    // DENNA ÄR BLABLABLA^[FiftyMilliSec,VK_A,FiftyMilliSec][FiftyMilliSec,VK_B,FiftyMilliSec][FiftyMilliSec,VK_C,FiftyMilliSec][FiftyMilliSec,VK_D,FiftyMilliSec]
                    _description = temp.Substring(0, temp.IndexOf("^", StringComparison.InvariantCultureIgnoreCase));
                    dataString = temp.Remove(0, _description.Length + 1);
                }

                var array = dataString.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries);

                // [FiftyMilliSec,VK_A,FiftyMilliSec]
                // [FiftyMilliSec,VK_B,FiftyMilliSec]
                // [FiftyMilliSec,VK_C,FiftyMilliSec]
                // [FiftyMilliSec,VK_D,FiftyMilliSec]}
                // ...
                for (int i = 0; i < array.Length; i++)
                {
                    var keyPressInfo = new KeyPressInfo();
                    var entry = array[i];
                    entry = entry.Replace("[", string.Empty);
                    entry = entry.Replace("]", string.Empty);

                    // FiftyMilliSec,VK_A,FiftyMilliSec
                    keyPressInfo.LengthOfBreak = (KeyPressLength)Enum.Parse(typeof(KeyPressLength), entry.AsSpan(0, entry.IndexOf(",", StringComparison.Ordinal)));
                    entry = entry.Substring(entry.IndexOf(",", StringComparison.Ordinal) + 1);
                    var keys = entry.Substring(0, entry.IndexOf(",", StringComparison.Ordinal));
                    keyPressInfo.VirtualKeyCodes = SplitStringKeyCodes(keys);
                    keyPressInfo.LengthOfKeyPress = (KeyPressLength)Enum.Parse(typeof(KeyPressLength), entry.AsSpan(entry.IndexOf(",", StringComparison.Ordinal) + 1));
                    _sortedKeyPressInfoList.Add(GetNewKeyValue(), keyPressInfo);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);

            }
        }

        public KeyPressLength GetLengthOfKeyPress()
        {
            if (IsMultiSequenced())
            {
                throw new Exception("Key press is multisequenced. Cannot query single key stroke length as it may contain many such values. (OSKeyPress.GetLengthOfKeyPress())");
            }

            return _sortedKeyPressInfoList[0].LengthOfKeyPress;
        }

        public void SetLengthOfKeyPress(KeyPressLength keyPressLength)
        {
            if (IsMultiSequenced())
            {
                throw new Exception("Key press is multisequenced. Cannot set single key stroke length as it may contain many such values. (OSKeyPress.SetLengthOfKeyPress())");
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
                Common.ShowErrorMessageBox(ex);
            }

            return result;
        }

        [JsonProperty("Description", Required = Required.Default)]
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public bool IsEmpty()
        {
            return _sortedKeyPressInfoList == null || _sortedKeyPressInfoList.Count == 0;
        }

        private SortedList<int, KeyPressInfo> _oldSortedKeyPressInfoList = new();
        [Obsolete]
        [JsonProperty("KeySequence", Required = Required.Default)]
        public SortedList<int, KeyPressInfo> KeySequenceObsolete
        {
            // get => _sortedKeyPressInfoList;
            set
            {
                _oldSortedKeyPressInfoList = value;
                if (_oldSortedKeyPressInfoList != null)
                {
                    for (var i = 0; i < _oldSortedKeyPressInfoList.Count; i++)
                    {
                        var keyPressInfo = _oldSortedKeyPressInfoList[i];
                        _sortedKeyPressInfoList.Add(i, keyPressInfo);
                    }
                }
            }
        }

        [JsonProperty("KeyPressSequence", Required = Required.Default)]
        public SortedList<int, IKeyPressInfo> KeyPressSequence
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
            if (!string.IsNullOrEmpty(_description))
            {
                result.Append("INFORMATION=^" + _description + "^");
            }

            for (var i = 0; i < _sortedKeyPressInfoList.Count; i++)
            {
                var keyPressInfo = _sortedKeyPressInfoList[i];
                result.Append("[" + Enum.GetName(typeof(KeyPressLength), keyPressInfo.LengthOfBreak) + "," + GetVirtualKeyCodesAsString(keyPressInfo) + "," + Enum.GetName(typeof(KeyPressLength), keyPressInfo.LengthOfKeyPress) + "]");
            }

            result.Append('}');
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
        
        [JsonProperty("Abort", Required = Required.Default)]
        public bool Abort
        {
            get => _abort;
            set
            {
                _abort = value;
                if (_abort)
                {
                    _cancellationTokenSource.Cancel();
                }
                else
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }
            }
        }
    }


}

