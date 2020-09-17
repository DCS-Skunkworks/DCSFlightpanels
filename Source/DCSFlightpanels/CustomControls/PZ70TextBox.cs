using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class PZ70TextBox : TextBoxBaseInput
    {
        public new BillPZ70 Bill { get; set; }
        protected override BillBaseInput GetBill => Bill;
    }
}
