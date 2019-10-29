using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class PZ70TextBox : TextBox
    {
        public BillPZ70 Bill { get; set; }
    }
}
