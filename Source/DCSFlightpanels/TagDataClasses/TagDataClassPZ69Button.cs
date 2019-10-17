using System.Windows.Controls;
using NonVisuals.DCSBIOSBindings;

namespace DCSFlightpanels.TagDataClasses
{
    internal class TagDataClassPZ69Button
    {
        private DCSBIOSOutputBindingPZ69 _dcsbiosBindingLCDPZ69;
        private readonly Button _button;

        public TagDataClassPZ69Button(Button button)
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
