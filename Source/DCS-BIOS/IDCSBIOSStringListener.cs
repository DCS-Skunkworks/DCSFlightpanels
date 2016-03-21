namespace DCS_BIOS
{
    public interface IDCSBIOSStringListener
    {
        void DCSBIOSStringReceived(uint address, string stringData);
    }
}
