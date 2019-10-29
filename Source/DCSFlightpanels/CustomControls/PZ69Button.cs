using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class PZ69Button : Button
    {
        public BillPZ69Button Bill { get; set; }
    }
}
