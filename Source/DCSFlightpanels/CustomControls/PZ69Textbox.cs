using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class PZ69TextBox : TextBoxBaseInput
    {
        public new BillPZ69 Bill { get; set; }
        protected override BillBaseInput GetBill => Bill;
    }
}
