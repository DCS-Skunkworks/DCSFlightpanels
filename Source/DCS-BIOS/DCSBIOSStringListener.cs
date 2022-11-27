namespace DCS_BIOS
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using EventArgs;
    using Interfaces;
    using NLog;

    /// <summary>
    /// Classes reports which strings they want to get updates for.
    /// This class handles the listening and broadcasts the strings
    /// once they are fully received.
    /// Used by DCSBIOSStringManager.
    /// </summary>
    internal class DCSBIOSStringListener : IDcsBiosDataListener, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<KeyValuePair<uint, DCSBIOSString>> _dcsBiosStrings = new();
        private readonly object _lockObject = new();
        private readonly Encoding _iso88591 = Encoding.GetEncoding("ISO-8859-1");
        

        public DCSBIOSStringListener()
        {
            BIOSEventHandler.AttachDataListener(this);
        }

        public void Dispose()
        {
            BIOSEventHandler.DetachDataListener(this);
            GC.SuppressFinalize(this);
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
                    foreach (var kvp in _dcsBiosStrings)
                    {
                        //kvp.Value.Address == start address for the string
                        if (kvp.Value.IsComplete)
                        {
                            //Once it is "complete" then just send it each time, do not reset it as there will be no updates from DCS-BIOS unless cockpit value changes.
                            BIOSEventHandler.DCSBIOSStringAvailable(this, kvp.Value.Address, kvp.Value.StringValue);
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

                                /*
                                25.7.2018
                                Was the TACAN problem related to something else? Perhaps to the flickering which was caused in mainwindow's constructor (wrong order instantiation)? Wrong fix which didn't help??

                                if (hex.Length < 4)
                                {
                                    return;
                                }*/
                                //Little Endian !
                                var secondByte = new[] { Convert.ToByte(hex.Substring(0, 2), 16) };
                                var firstChar = string.Empty;
                                byte[] firstByte = new byte[10];
                                if (hex.Length == 3)
                                {
                                    //this is really ugly, will it work ?? keep geting 0x730 from MI-8 R863 where I would except last digit (uneven 7 long frequency)
                                    //so let's try and just ignore the for number, in this case the 7.
                                    //28.04.2020 JDA
                                    firstByte = new[] { Convert.ToByte(hex.Substring(1, 2), 16) };
                                    firstChar = _iso88591.GetString(firstByte);
                                }
                                else if (hex.Length == 4)
                                {
                                    firstByte = new[] { Convert.ToByte(hex.Substring(2, 2), 16) };
                                    firstChar = _iso88591.GetString(firstByte);
                                }
                                var secondChar = _iso88591.GetString(secondByte);

                                if (!string.IsNullOrEmpty(firstChar))
                                {
                                    kvp.Value.Add(address, firstChar, secondChar);
                                }
                                else
                                {
                                    kvp.Value.Add(address, secondChar);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, $"**********Received (0x{data:X}");
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
