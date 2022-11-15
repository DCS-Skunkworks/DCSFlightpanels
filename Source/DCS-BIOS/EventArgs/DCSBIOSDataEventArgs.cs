namespace DCS_BIOS.EventArgs
{
    public class DCSBIOSDataEventArgs : System.EventArgs
    {
        public uint Address { get; init; }

        public uint Data { get; init; }
    }
}
