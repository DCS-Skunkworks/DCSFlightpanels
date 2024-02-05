namespace DCS_BIOS.Interfaces
{
    using EventArgs;

    public interface IDCSBIOSStringListener
    {
        void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e);
    }
}
