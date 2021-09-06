namespace DCSFlightpanels.CustomControls
{
    using System.Windows.Controls;

    using DCSFlightpanels.Bills;

    public abstract class TextBoxBaseStreamDeckInput : TextBox
    {
        public BillBaseInput Bill { get; set; }
    }
}
