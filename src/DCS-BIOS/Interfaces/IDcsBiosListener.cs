namespace DCS_BIOS.Interfaces
{
    using EventArgs;

    public interface IDcsBiosDataListener
    {
        void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
    }
}
