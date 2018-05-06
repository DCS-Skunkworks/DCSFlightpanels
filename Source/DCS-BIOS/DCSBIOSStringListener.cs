using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibraryCommon;

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
                //Common.DebugP("DCSBIOSString Registering()" + address + ", total length = " + _length);
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(i);
                _receivedAddresses.Add(i);
            }
            _length = length;
            _internalBuffer = new string[_length];
        }

        public void Reset()
        {
            for (var i = _address; i < _address + _length; i = i + 2)
            {
                //Fill list of addresses to listen for
                _receivedAddresses.Add(i);
            }
        }

        public int Length
        {
            get => _length;
            set { _length = value; }
        }

        public uint Address
        {
            get => _address;
            set
            {
                _address = value;
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
        }

        public string StringValue => string.Join("", _internalBuffer);

        public bool IsMatch(uint address)
        {
            //Common.DebugP("DCSBIOSString IsMatch()" + address + ", should be >= " + _address + " and <=" + (_address + _length));
            //
            //if (_receivedAddresses.Contains(address))
            if (address >= _address && address <= _address + _length)
            {
                //Common.DebugP("DCSBIOSString IsMatch()!!!!" + address + ", should be >= " + _address + " and <=" + (_address + _length));
                return true;
            }
            //Common.DebugP("DCSBIOSString ISNOTMATCH()!!!!" + address + ", should be >= " + _address + " and <=" + (_address + _length));
            return false;
        }

        public void Add(uint address, string str1, string str2)
        {
            // when an update is received:
            //
            //if (_receivedAddresses.Contains(address))
            if (address >= _address && address < _address + _length)
            {
                //Common.DebugP("DCSBIOSString Add()" + address + ", string1 = " + str1 + ", string2 = " + str2);
                _receivedAddresses.Remove(address);

                uint offset;
                if (_address == address)
                {
                    offset = 0;
                }
                else
                {
                    offset = address - _address;
                }

                _internalBuffer[offset] = str1;
                if (_internalBuffer.Length - 1 >= offset + 1)
                {
                    //For example odd length strings. Only first endian byte should then be read.
                    _internalBuffer[offset + 1] = str2;
                }
                //Common.DebugP("DCSBIOSString Add() Internal buffer now " + string.Join("", _internalBuffer));
            }

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
        public delegate void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e);
        public event DCSBIOSStringReceived OnDCSBIOSStringReceived;

        private List<KeyValuePair<uint, DCSBIOSString>> _dcsBiosStrings = new List<KeyValuePair<uint, DCSBIOSString>>();
        private object _lockObject = new object();
        private Encoding _iso8859_1 = Encoding.GetEncoding("ISO-8859-1");


        //For those that wants to listen to Strings received from DCS-BIOS
        public void Attach(IDCSBIOSStringListener iDCSBIOSStringListener)
        {
            OnDCSBIOSStringReceived += iDCSBIOSStringListener.DCSBIOSStringReceived;
        }

        //For those that wants to listen to this panel
        public void Detach(IDCSBIOSStringListener iDCSBIOSStringListener)
        {
            OnDCSBIOSStringReceived -= iDCSBIOSStringListener.DCSBIOSStringReceived;
        }

        public DCSBIOSStringListener()
        {

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

            //Common.DebugP("**********address = [" + address + "] ****************");
            lock (_lockObject)
            {
                if (data == 0x55)
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
                                OnDCSBIOSStringReceived(this, new DCSBIOSStringDataEventArgs() { Address = kvp.Value.Address, StringData = kvp.Value.StringValue });
                                //kvp.Value.Reset();
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
                                if (hex.Length < 4)
                                {
                                    //TODO Keep getting garbage from DCS-BIOS, especially A-10C TACAN
                                    return;
                                }
                                //Little Endian !
                                //Common.DebugP("**********data = [" + data + "] ****************");
                                //Common.DebugP("**********hex = [" + hex + "] ****************");
                                var secondByte = new[] { Convert.ToByte(hex.Substring(0, 2), 16) };
                                var firstChar = "";
                                byte[] firstByte = new byte[10];
                                if (hex.Length > 2)
                                {
                                    firstByte = new[] { Convert.ToByte(hex.Substring(2, 2), 16) };
                                    firstChar = _iso8859_1.GetString(firstByte);
                                }
                                var secondChar = _iso8859_1.GetString(secondByte);
                                //Common.DebugP("**********Received (0x" + data.ToString("x") + ") ****************");
                                //Common.DebugP("**********Received data:(0x" + data.ToString("x") + ") following from DCS : 1st : " + firstChar + "(0x" + firstByte[0].ToString("x") + "), 2nd " + secondChar + "(0x" + secondByte[0].ToString("x") + ") ****************");
                                kvp.Value.Add(address, firstChar, secondChar);
                            }
                            catch (Exception ex)
                            {
                                Common.LogError(123, "**********Received (0x" + data.ToString("x") + ") Exception = " + ex.Message + Environment.NewLine + ex.StackTrace);
                            }
                        }
                    }
                }
            }
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateStrings(e.Address, e.Data);
        }

    }
}
