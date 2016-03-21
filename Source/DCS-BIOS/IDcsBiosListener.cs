namespace DCS_BIOS
{
    public interface IDcsBiosDataListener
    {
        void DcsBiosDataReceived(uint address, uint data);
        //void DcsBiosDataReceived(byte[] array);
        //void GetDcsBiosData(byte[] bytes);
    }
}
