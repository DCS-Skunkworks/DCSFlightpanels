using DCSFlightpanels.CustomControls;
using NonVisuals.DCSBIOSBindings;

namespace DCSFlightpanels.Bills
{
    public class BillPZ69Button
    {
        private readonly PZ69Button _button;

        public DCSBIOSOutputBindingPZ69 DCSBIOSBindingLCD { get; set; }

        public BillPZ69Button(PZ69Button button)
        {
            _button = button;
        }
        
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
