using System;
using System.Collections.Generic;
using System.Text;
using DCS_BIOS.EventArgs;
using DCS_BIOS.Interfaces;
using NLog;

namespace DCS_BIOS.StringClasses
{
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
        //private readonly uint _debugAddress = 10244; //->10251 Mi-8MT R863, Frequency

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
                            //Once it has been "complete" one time then just keep sending it each time, do not reset it as there will be no updates from DCS-BIOS unless cockpit value changes.
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

                                //Debug.WriteLine(hex);
                                //See comment below.
                                if (hex.Length < 2)
                                {
                                    /*
                                     * Remove address as it doesn't contain data. Maybe a dynamic string and right now the string is shorter
                                     * than the memory space reserved.
                                     * Now if the string 
                                     */
                                    kvp.Value.RemoveAddress(address);
                                    return;
                                }

                                //Little Endian !
                                byte[] secondByte;
                                byte[] firstByte;
                                var secondChar = string.Empty;
                                var firstChar = string.Empty;

                                switch (hex.Length)
                                {
                                    case 2:
                                        {
                                            secondByte = new[] { Convert.ToByte(hex.Substring(0, 2), 16) };
                                            secondChar = _iso88591.GetString(secondByte);
                                            firstChar = "";
                                            break;
                                        }
                                    case 3:
                                        {
                                            //this is really ugly, will it work ?? keep getting 0x730 from MI-8 R863 where I would except last digit (uneven 7 long frequency)
                                            //so let's try and just ignore the for number, in this case the 7.
                                            //28.04.2020 JDA
                                            secondByte = new[] { Convert.ToByte(hex.Substring(0, 2), 16) };
                                            secondChar = _iso88591.GetString(secondByte);
                                            firstByte = new[] { Convert.ToByte(hex.Substring(1, 2), 16) };
                                            firstChar = _iso88591.GetString(firstByte);
                                            break;
                                        }
                                    case 4:
                                        {
                                            secondByte = new[] { Convert.ToByte(hex.Substring(0, 2), 16) };
                                            secondChar = _iso88591.GetString(secondByte);
                                            firstByte = new[] { Convert.ToByte(hex.Substring(2, 2), 16) };
                                            firstChar = _iso88591.GetString(firstByte);
                                            break;
                                        }
                                }


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
