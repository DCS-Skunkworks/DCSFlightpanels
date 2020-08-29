using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class PZ55TextBox : TextBoxBaseInput
    {
        public new BillPZ55 Bill { get; set; }
        public PZ55TextBox Previous { get; set; }
        public PZ55TextBox Next { get; set; }
        public string Description { get; set; }
    }
}
