namespace DCS_BIOS
{
    using System.Collections.Generic;

    public class DCSBIOSString
    {
        // Use this, some strings need to be fully constructed before being broadcast
        private readonly List<uint> _receivedAddresses = new();
        private readonly string[] _internalBuffer;
        private readonly int _length;
        private uint _address;
        
        public bool IsComplete => _receivedAddresses.Count == 0;
        public string StringValue => string.Join(string.Empty, _internalBuffer);
        public uint Address
        {
            get => _address;
        }

        public DCSBIOSString(uint address, int length)
        {
            _address = address;
            for (var i = _address; i < _address + length; i += 2)
            {
                // Common.DebugP("DCSBIOSString Registering()" + address + ", total length = " + _length);
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(i);
                _receivedAddresses.Add(i);
            }

            _length = length;
            _internalBuffer = new string[_length];
        }

        public bool IsMatch(uint address)
        {
            // Common.DebugP("DCSBIOSString IsMatch()" + address + ", should be >= " + _address + " and <=" + (_address + _length));
            
            // if (_receivedAddresses.Contains(address))
            if (address >= _address && address <= _address + _length)
            {
                // Common.DebugP("DCSBIOSString IsMatch()!!!!" + address + ", should be >= " + _address + " and <=" + (_address + _length));
                return true;
            }

            // Common.DebugP("DCSBIOSString ISNOTMATCH()!!!!" + address + ", should be >= " + _address + " and <=" + (_address + _length));
            return false;
        }

        public void Add(uint address, string str1, string str2)
        {
            // Debug.WriteLine("_address : " + _address + " _length : " + _length);
            // Debug.WriteLine("address : " + address + " str1 : " + str1 + " str2 : " + str2);
            if (address >= _address && address < _address + _length)
            {
                _receivedAddresses.Remove(address);

                var offset = address - _address;

                // Debug.WriteLine("offset is : " + offset);
                /*for (int i = 0; i < _internalBuffer.Length; i++)
                {
                    // Debug.WriteLine("_internalBuffer[" + i + "] = " + _internalBuffer[i]);
                }*/

                if (!string.IsNullOrEmpty(str1))
                {
                    _internalBuffer[offset] = str1;

                    // Debug.WriteLine("str1 : " + str1 + " added to buffer[" + offset + "]");
                }

                // Debug.WriteLine("_internalBuffer.Length : " + _internalBuffer.Length + " offset : " + (offset));
                if (offset + 1 < _internalBuffer.Length && str2 != null)
                {
                    // index = 5, length = 6
                    // For example odd length strings. Only first endian byte should then be read.
                    _internalBuffer[offset + 1] = str2;

                    // Debug.WriteLine("str2 : " + str2 + " added to buffer[" + (offset) + "]");
                }

                // Debug.WriteLine("DCSBIOSString Add() Internal buffer now " + string.Join(string.Empty, _internalBuffer));
                // Debug.WriteLine("*******************************************************");
            }
        }

        public void Add(uint address, string str2)
        {
            // Debug.WriteLine("_address : " + _address + " _length : " + _length);
            if (address >= _address && address < _address + _length)
            {
                _receivedAddresses.Remove(address);

                var offset = address - _address;
                
                /*Debug.WriteLine("offset is : " + offset);
                for (int i = 0; i < _internalBuffer.Length; i++)
                {
                    //Debug.WriteLine("_internalBuffer[" + i + "] = " + _internalBuffer[i]);
                }
                */

                if (!string.IsNullOrEmpty(str2))
                {
                    _internalBuffer[offset] = str2;
                    
                    // Debug.WriteLine("str2 : " + str2 + " added to buffer[" + offset + "]");
                }

                // Debug.WriteLine("_internalBuffer.Length : " + _internalBuffer.Length + " offset : " + (offset));
                // Debug.WriteLine("DCSBIOSString Add() Internal buffer now " + string.Join(string.Empty, _internalBuffer));
                // Debug.WriteLine("*******************************************************");
            }
        }
    }
}
