using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class PZ69GenericTextBox : TextBoxBaseInput
    {
        public new BillPZ69Generic Bill { get; set; }
        protected override BillBaseInput GetBill => Bill;
    }
}
