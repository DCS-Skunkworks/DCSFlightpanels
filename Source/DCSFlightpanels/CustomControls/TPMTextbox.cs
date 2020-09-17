using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class TPMTextBox : TextBoxBaseInput
    {
        public new BillTPM Bill { get; set; }

        protected override BillBaseInput GetBill => Bill;
    }
}
