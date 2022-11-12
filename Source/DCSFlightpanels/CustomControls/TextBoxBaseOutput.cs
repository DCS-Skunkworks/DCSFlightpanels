namespace DCSFlightpanels.CustomControls
{
    using System.Windows.Controls;

    using Bills;

    public class TextBoxBaseOutput : TextBox
    {
        public BillBaseOutput Bill { get; set; }
    }
}
