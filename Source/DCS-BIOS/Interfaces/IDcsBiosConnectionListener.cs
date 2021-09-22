namespace DCS_BIOS.Interfaces
{
    using DCS_BIOS.EventArgs;

    public interface IDcsBiosConnectionListener
    {
        void DcsBiosConnectionActive(object sender, DCSBIOSConnectionEventArgs e);
    }
}
