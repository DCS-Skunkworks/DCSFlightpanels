namespace DCS_BIOS.EventArgs
{
    public class DCSBIOSDataEventArgs : System.EventArgs
    {
        public uint Address { get; set; }

        public uint Data { get; set; }
    }
}
