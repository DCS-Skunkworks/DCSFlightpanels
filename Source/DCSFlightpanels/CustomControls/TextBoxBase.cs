using System.Windows.Controls;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class TextBoxBase : TextBox
    {
        public BillBase Bill { get; set; }
    }
}
