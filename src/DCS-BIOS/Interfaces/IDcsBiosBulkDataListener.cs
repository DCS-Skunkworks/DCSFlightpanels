using System.Threading.Tasks;

namespace DCS_BIOS.Interfaces
{
    using EventArgs;

    public interface IDcsBiosBulkDataListener
    {
        void DcsBiosBulkDataReceived(object sender, DCSBIOSBulkDataEventArgs e);
    }
}
