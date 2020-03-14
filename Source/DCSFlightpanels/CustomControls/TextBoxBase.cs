using System.Windows.Controls;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class TextBoxBaseInput : TextBox
    {
        public BillBaseInput Bill { get; set; }
    }
}
