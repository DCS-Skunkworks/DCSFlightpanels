using System;
using System.Collections.Generic;
using System.Text;

namespace DCS_BIOS
{

    public class DCSBIOSString
    {
        private string[] _internalBuffer;
        private int _length;
        private uint _address;

        //Use this, some strings need to be fully contructed before being broadcasted
        private List<uint> _receivedAddresses = new List<uint>();

        public DCSBIOSString(uint address, int length)
        {
            _address = address;
            for (var i = _address; i < _address + length; i = i + 2)
            {
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(i);
                _receivedAddresses.Add(i);
            }
            _length = length;
            _internalBuffer = new string[_length];
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public uint Address
        {
            get { return _address; }
            set
            {
                _address = value;
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
        }

        public string StringValue
        {
            get { return string.Join("", _internalBuffer); }
        }

        public bool IsMatch(uint address)
        {
            if (address >= _address && address <= _address + _length)
            {
                //Common.DebugP("Match! : Address");
                return true;
            }
            return false;
        }

        public void Add(uint address, string str1, string str2)
        {
            // when an update is received:
            if (address >= _address && address < _address + _length)
            {
                _receivedAddresses.Remove(address);
                //Common.DebugP("Address is : 0x" + _address.ToString("x") + "  address is 0x" + address.ToString("x"));
                uint offset;
                if (_address == address)
                {
                    offset = 0;
                }
                else
                {
                    offset = address - _address;
                }
                //Common.DebugP("DCSBIOSString offset is " + offset);
                _internalBuffer[offset] = str1;
                if (_internalBuffer.Length - 1 >= offset + 1)
                {
                    //For for example odd length strings. Only first endian byte should then be read.
                    _internalBuffer[offset + 1] = str2;
                }
            }
            //Common.DebugP("DCSBIOSString " + _address.ToString("x") + ", value is now : " + String.Join("", _internalBuffer));
        }

        public bool IsComplete
        {
            get { return _receivedAddresses.Count == 0; }
        }
    }

    public static class DCSBIOSStringListenerHandler
    {
        private static DCSBIOSStringListener _dcsbiosStringListener;

        public static void AddAddress(uint address, int length)
        {
            CheckInstance();
            _dcsbiosStringListener.AddStringAddress(address, length);
        }

        public static void AddAddress(uint address, int length, IDCSBIOSStringListener dcsbiosStringListener)
        {
            CheckInstance();
            AddAddress(address, length);
            _dcsbiosStringListener.Attach(dcsbiosStringListener);
        }

        private static void CheckInstance()
        {
            if (_dcsbiosStringListener == null)
            {
                _dcsbiosStringListener = new DCSBIOSStringListener();
            }
        }

        public static void Close()
        {
            _dcsbiosStringListener = null;
        }
    }

    public class DCSBIOSStringListener : IDcsBiosDataListener
    {
        public delegate void DCSBIOSStringReceived(uint address, string stringData);
        public event DCSBIOSStringReceived OnDCSBIOSStringReceived;

        private List<KeyValuePair<uint, DCSBIOSString>> _dcsBiosStrings = new List<KeyValuePair<uint, DCSBIOSString>>();
        private object _lockObject = new object();
        private Encoding _iso8859_1 = Encoding.GetEncoding("ISO-8859-1");
        

        //For those that wants to listen to Strings received from DCS-BIOS
        public void Attach(IDCSBIOSStringListener iDCSBIOSStringListener)
        {
            OnDCSBIOSStringReceived += new DCSBIOSStringReceived(iDCSBIOSStringListener.DCSBIOSStringReceived);
        }

        //For those that wants to listen to this panel
        public void Detach(IDCSBIOSStringListener iDCSBIOSStringListener)
        {
            OnDCSBIOSStringReceived -= new DCSBIOSStringReceived(iDCSBIOSStringListener.DCSBIOSStringReceived);
        }

        public DCSBIOSStringListener()
        {
            //Common.DebugP(Environment.NewLine + Environment.NewLine + "CREATING NEW DCSBIOSStringListener" + Environment.NewLine + Environment.NewLine);
            DCSBIOS.AttachDataReceivedListenerSO(this);
            //var bla = new DCSBIOSString(0x11c0, 24);
            //_dcsBiosStrings.Add(new KeyValuePair<uint, DCSBIOSString>(0x11c0, bla));
        }

        public void AddStringAddress(uint address, int length)
        {
            var found = false;
            lock (_lockObject)
            {
                foreach (var dcsBiosString in _dcsBiosStrings)
                {
                    if (dcsBiosString.Key == address)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    _dcsBiosStrings.Add(new KeyValuePair<uint, DCSBIOSString>(address, new DCSBIOSString(address, length)));
                }
            }
        }

        public void RemoveStringAddress(uint address)
        {
            var dcsBiosStringToRemove = new KeyValuePair<uint, DCSBIOSString>();
            var found = false;
            foreach (var dcsBiosString in _dcsBiosStrings)
            {
                if (dcsBiosString.Key == address)
                {
                    dcsBiosStringToRemove = dcsBiosString;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                _dcsBiosStrings.Remove(dcsBiosStringToRemove);
            }
        }

        private void UpdateStrings(uint address, uint data)
        {
            //Common.DebugP("**********Received address 0x" + address.ToString("x") + " ****************");
            lock (_lockObject)
            {
                if (address == 0x55)
                {
                    //start of update cycle
                    return;
                }
                if (address == 0xfffe)
                {
                    //end of update cycle, clear all existing values.
                    //broadcast every string now
                    if (OnDCSBIOSStringReceived != null)
                    {
                        foreach (var kvp in _dcsBiosStrings)
                        {
                            //kvp.Value.Address == start address for the string
                            if (kvp.Value.IsComplete)
                            {
                                OnDCSBIOSStringReceived(kvp.Value.Address, kvp.Value.StringValue);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var kvp in _dcsBiosStrings)
                    {
                        if (kvp.Value.IsMatch(address))
                        {
                            try
                            {
                                //Send "AB"
                                //0x4241
                                //41 = A
                                //42 = B
                                var hex = Convert.ToString(data, 16);
                                
                                //Little Endian !
                                var secondByte = new[] { Convert.ToByte(hex.Substring(0, 2), 16) };
                                var firstByte = new[] { Convert.ToByte(hex.Substring(2, 2), 16) };
                                var firstChar = _iso8859_1.GetString(firstByte);
                                var secondChar = _iso8859_1.GetString(secondByte);
                                kvp.Value.Add(address, firstChar, secondChar);
                                //Common.DebugP("**********Received (0x" + data.ToString("x") + ") ****************");
                                //Common.DebugP("**********Received data:(0x" + data.ToString("x") + ") following from DCS : 1st : " + firstChar + "(0x" + firstByte[0].ToString("x") + "), 2nd " + secondChar + "(0x" + secondByte[0].ToString("x") + ") ****************");
                                
                                //kvp.Value.StepUp();
                            }
                            catch (Exception ex)
                            {
                                DBCommon.DebugP("**********Received (0x" + data.ToString("x") + ") Exception = " + ex.Message + Environment.NewLine + ex.StackTrace);
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public void DcsBiosDataReceived(uint address, uint data)
        {
            UpdateStrings(address, data);
        }
        
    }
}
