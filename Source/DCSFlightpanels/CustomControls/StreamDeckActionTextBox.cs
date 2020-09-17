using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckActionTextBox : TextBoxBaseStreamDeckInput
    {
        public new BillStreamDeckAction Bill { get; set; }
    }
}
