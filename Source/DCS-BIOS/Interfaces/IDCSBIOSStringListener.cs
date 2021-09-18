namespace DCS_BIOS.Interfaces
{
    using DCS_BIOS.EventArgs;

    public interface IDCSBIOSStringListener
    {
        void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e);
    }
}
