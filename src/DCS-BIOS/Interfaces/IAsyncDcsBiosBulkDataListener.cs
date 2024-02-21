using System.Threading.Tasks;

namespace DCS_BIOS.Interfaces
{
    using EventArgs;

    public interface IAsyncDcsBiosBulkDataListener
    {
        Task AsyncDcsBiosBulkDataReceived(object sender, DCSBIOSBulkDataEventArgs e);
    }
}
