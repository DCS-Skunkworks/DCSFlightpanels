using System;
using System.Net.Sockets;

namespace DCS_BIOS
{
    public interface IDCSBIOSStringListener
    {
        void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e);
    }

    public class DCSBIOSStringDataEventArgs : EventArgs
    {
        public uint Address { get; set; }
        public string StringData { get; set; }
    }
}
