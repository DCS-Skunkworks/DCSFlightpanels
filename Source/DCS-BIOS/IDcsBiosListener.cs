using System;

namespace DCS_BIOS
{
    public interface IDcsBiosDataListener
    {
        void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
    }

    public class DCSBIOSDataEventArgs : EventArgs
    {
        public uint Address { get; set; }
        public uint Data { get; set; }
    }
}
