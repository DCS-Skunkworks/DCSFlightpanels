using System.Collections.Generic;

namespace DCS_BIOS.StringClasses
{
    /// <summary>
    /// Wrapper for DCS-BIOS strings which has to be constructed
    /// from several packets sent from DCS-BIOS.
    /// </summary>
    public class DCSBIOSString
    {
        /// <summary>
        /// Use this, some strings need to be fully constructed before being broadcast.
        /// Once it has once been complete no further checks is made that all the addresses
        /// has been received. It will be sent regardless.
        /// Only the first "build up" of the string uses this.
        /// </summary>
        private readonly List<uint> _receivedAddresses = new();
        private readonly string[] _internalBuffer;
        private readonly int _length;
        private readonly uint _address;

        //private readonly uint _debugAddress = 10244; //->10251 Mi-8MT R863, Frequency
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
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(i);
                _receivedAddresses.Add(i);
            }
            _length = length;
            _internalBuffer = new string[_length];
        }

        /// <summary>
        /// Addresses received containing empty payload to be
        /// considered as unnecessary, for example a string has too
        /// big memory reserved and the last addresses contain nothing.
        /// </summary>
        /// <param name="address"></param>
        public void RemoveAddress(uint address)
        {
            _receivedAddresses.Remove(address);
        }

        public bool IsMatch(uint address)
        {
            if (address >= _address && address <= _address + _length)
            {
                return true;
            }
            return false;
        }

        public void Add(uint address, string str1, string str2)
        {
            if (address < _address || address >= _address + _length)
            {
                return;
            }

            _receivedAddresses.Remove(address);

            var offset = address - _address;

            if (!string.IsNullOrEmpty(str1))
            {
                _internalBuffer[offset] = str1;
            }

            if (offset + 1 < _internalBuffer.Length && str2 != null)
            {
                // index = 5, length = 6
                // For example odd length strings. Only first endian byte should then be read.
                _internalBuffer[offset + 1] = str2;
            }
        }

        public void Add(uint address, string str2)
        {
            if (address >= _address && address < _address + _length)
            {
                _receivedAddresses.Remove(address);

                var offset = address - _address;

                if (!string.IsNullOrEmpty(str2))
                {
                    _internalBuffer[offset] = str2;
                }
            }
        }
    }
}
