using System.Windows.Controls;
using NonVisuals.BindingClasses.DCSBIOSBindings;

namespace DCSFlightpanels.CustomControls
{
    public class PZ69Button : Button
    {
        public DCSBIOSOutputBindingPZ69 DCSBIOSBindingLCD { get; set; }

        public bool ContainsLCDBinding()
        {
            return DCSBIOSBindingLCD != null && DCSBIOSBindingLCD.HasBinding;
        }

        public bool IsEmpty()
        {
            return DCSBIOSBindingLCD.DCSBIOSOutputObject == null;
        }

        public void ClearAll()
        {
            DCSBIOSBindingLCD = null;
        }
    }
}
