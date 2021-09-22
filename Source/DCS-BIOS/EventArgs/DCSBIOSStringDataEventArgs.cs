namespace DCS_BIOS.EventArgs
{
    public class DCSBIOSStringDataEventArgs : System.EventArgs                 
    {
        public uint Address { get; set; }

        public string StringData { get; set; }
    }
}
