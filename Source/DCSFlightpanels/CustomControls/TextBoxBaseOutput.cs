namespace DCSFlightpanels.CustomControls
{
    using System.Windows.Controls;

    using DCSFlightpanels.Bills;

    public class TextBoxBaseOutput : TextBox
    {
        public BillBaseOutput Bill { get; set; }
    }
}
