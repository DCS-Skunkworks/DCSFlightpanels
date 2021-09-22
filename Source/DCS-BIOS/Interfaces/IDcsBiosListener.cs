namespace DCS_BIOS.Interfaces
{
    using DCS_BIOS.EventArgs;

    public interface IDcsBiosDataListener
    {
        void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
    }
}
