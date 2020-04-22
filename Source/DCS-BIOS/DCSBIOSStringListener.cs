using System;
using System.Collections.Generic;
using System.Text;
using ClassLibraryCommon;
// ReSharper disable All
/*
 * Do not adhere to naming standard in DCS-BIOS code, standard are based on DCS-BIOS json files and byte streamnaming
 */
namespace DCS_BIOS
{

    public class DCSBIOSString
    {
        private readonly string[] _internalBuffer;
        private int _length;
        private uint _address;

        //Use this, some strings need to be fully contructed before being broadcasted
        private readonly List<uint> _receivedAddresses = new List<uint>();

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
            //Debug.WriteLine("_address : " + _address + " _length : " + _length);
            //Debug.WriteLine("address : " + address + " str1 : " + str1 + " str2 : " + str2);

            if (address >= _address && address < _address + _length)
            {
                _receivedAddresses.Remove(address);

                uint offset = address - _address;
                //Debug.WriteLine("offset is : " + offset);
                for (int i = 0; i < _internalBuffer.Length; i++)
                {
                    //Debug.WriteLine("_internalBuffer[" + i + "] = " + _internalBuffer[i]);
                }
                if (!string.IsNullOrEmpty(str1))
                {
                    _internalBuffer[offset] = str1;
                    //Debug.WriteLine("str1 : " + str1 + " added to buffer[" + offset + "]");
                }

                //Debug.WriteLine("_internalBuffer.Length : " + _internalBuffer.Length + " offset : " + (offset));
                if (offset + 1 < _internalBuffer.Length && str2 != null)
                {
                    // index = 5, length = 6
                    //For example odd length strings. Only first endian byte should then be read.
                    _internalBuffer[offset + 1] = str2;
                    //Debug.WriteLine("str2 : " + str2 + " added to buffer[" + (offset) + "]");
                }
                //Debug.WriteLine("DCSBIOSString Add() Internal buffer now " + string.Join("", _internalBuffer));
                //Debug.WriteLine("*******************************************************");
            }
        }

        public void Add(uint address, string str2)
        {
            //Debug.WriteLine("_address : " + _address + " _length : " + _length);

            if (address >= _address && address < _address + _length)
            {
                _receivedAddresses.Remove(address);

                uint offset = address - _address;
                //Debug.WriteLine("offset is : " + offset);
                for (int i = 0; i < _internalBuffer.Length; i++)
                {
                    //Debug.WriteLine("_internalBuffer[" + i + "] = " + _internalBuffer[i]);
                }
                if (!string.IsNullOrEmpty(str2))
                {
                    _internalBuffer[offset] = str2;
                    //Debug.WriteLine("str2 : " + str2 + " added to buffer[" + offset + "]");
                }

                //Debug.WriteLine("_internalBuffer.Length : " + _internalBuffer.Length + " offset : " + (offset));
                //Debug.WriteLine("DCSBIOSString Add() Internal buffer now " + string.Join("", _internalBuffer));
                //Debug.WriteLine("*******************************************************");
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

        private readonly List<KeyValuePair<uint, DCSBIOSString>> _dcsBiosStrings = new List<KeyValuePair<uint, DCSBIOSString>>();
        private readonly object _lockObject = new object();
        private readonly Encoding _iso8859_1 = Encoding.GetEncoding("ISO-8859-1");


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
                                //Once it is "complete" then just send it each time, do not reset it as there will be no updates from DCS-BIOS unless cockpit value changes.
                                OnDCSBIOSStringReceived(this, new DCSBIOSStringDataEventArgs() { Address = kvp.Value.Address, StringData = kvp.Value.StringValue });
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
                                //Debug.WriteLine("hex : " + hex);

                                /*
                                25.7.2018
                                Was the TACAN problem related to something else? Perhaps to the flickering which was caused in mainwindow's constructor (wrong order instantiation)? Wrong fix which didn't help??

                                if (hex.Length < 4)
                                {
                                    //TODO Keep getting garbage from DCS-BIOS, especially A-10C TACAN
                                    return;
                                }*/
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
                                if (!string.IsNullOrEmpty(firstChar))
                                {
                                    kvp.Value.Add(address, firstChar, secondChar);
                                }
                                else
                                {
                                    kvp.Value.Add(address, secondChar);
                                }
                                ////Debug.WriteLine("firstChar : " + firstChar + " secondChar : " + secondChar);
                            }
                            catch (Exception ex)
                            {
                                Common.LogError( "**********Received (0x" + data.ToString("x") + ") Exception = " + ex.Message + Environment.NewLine + ex.StackTrace);
                            }
                        }
                    }
                }
            }

            /*foreach (var dcsBiosString in _dcsBiosStrings)
            {
                //Debug.WriteLine("Key : " + dcsBiosString.Key + " Value : " + dcsBiosString.Value.StringValue);
            }*/
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateStrings(e.Address, e.Data);
        }

    }
}
