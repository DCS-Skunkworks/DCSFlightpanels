using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ69Button
    {
        private DCSBIOSBindingLCDPZ69 _dcsbiosBindingLCDPZ69;
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
        
        public DCSBIOSBindingLCDPZ69 DCSBIOSBindingLCD
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
