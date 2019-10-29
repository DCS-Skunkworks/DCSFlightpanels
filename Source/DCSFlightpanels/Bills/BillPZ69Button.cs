using System.Windows.Controls;
using DCSFlightpanels.CustomControls;
using NonVisuals.DCSBIOSBindings;

namespace DCSFlightpanels.Bills
{
    public class BillPZ69Button
    {
        private DCSBIOSOutputBindingPZ69 _dcsbiosBindingLCDPZ69;
        private readonly PZ69Button _button;

        public BillPZ69Button(PZ69Button button)
        {
            _button = button;
        }

        
        public bool ContainsLCDBinding()
        {
            return _dcsbiosBindingLCDPZ69 != null && _dcsbiosBindingLCDPZ69.HasBinding;
        }

        
        public bool IsEmpty()
        {
            return _dcsbiosBindingLCDPZ69.DCSBIOSOutputObject == null;
        }
        
        public DCSBIOSOutputBindingPZ69 DCSBIOSBindingLCD
        {
            get => _dcsbiosBindingLCDPZ69;
            set => _dcsbiosBindingLCDPZ69 = value;
        }
        
        public void ClearAll()
        {
            _dcsbiosBindingLCDPZ69 = null;
        }
    }
}
