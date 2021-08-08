using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class FarmingPanelTextBox : TextBoxBaseInput
    {
        public new BillPFarmingPanel Bill { get; set; }
        protected override BillBaseInput GetBill => Bill;
    }
}