using System.Windows.Controls;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class TextBoxBaseInput : TextBox
    {
        public BillBaseInput Bill { get; set; }
    }
}


namespace DCSFlightpanels.CustomControls
{
    public class TextBoxBaseOutput : TextBox
    {
        public BillBaseOutput Bill { get; set; }
    }
}