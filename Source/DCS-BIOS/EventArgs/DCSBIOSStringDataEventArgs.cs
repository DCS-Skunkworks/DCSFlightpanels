namespace DCS_BIOS.EventArgs
{
    public class DCSBIOSStringDataEventArgs : System.EventArgs                 
    {
        public uint Address { get; init; }

        public string StringData { get; init; }
    }
}
