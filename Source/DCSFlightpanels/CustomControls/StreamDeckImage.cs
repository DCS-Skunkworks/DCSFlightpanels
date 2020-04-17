using System.Windows.Controls;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckImage : Image
    {
        public BillStreamDeckFace Bill { get; set; }
    }
}
