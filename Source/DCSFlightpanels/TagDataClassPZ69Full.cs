using System.Collections.Generic;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ69Full
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private OSKeyPress _osKeyPress;
        private DCSBIOSBindingLCDPZ69 _dcsbiosBindingLCDPZ69;
        private DCSBIOSBindingPZ69 _dcsbiosBindingPZ69;
        
        public bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }

        public bool ContainsOSKeyPress()
        {
            return _osKeyPress != null && _osKeyPress.KeySequence.Count > 0;
        }

        public bool ContainsKeySequence()
        {
            return _osKeyPress != null && _osKeyPress.IsMultiSequenced();
        }

        public bool ContainsSingleKey()
        {
            return _osKeyPress != null && !_osKeyPress.IsMultiSequenced();
        }

        public bool ContainsLCDBinding()
        {
            return _dcsbiosBindingLCDPZ69 != null && _dcsbiosBindingLCDPZ69.HasBinding;
        }

        public bool ContainsDCSBIOSBinding()
        {
            return _dcsbiosBindingPZ69 != null && _dcsbiosBindingPZ69.HasBinding();
        }

        public string GetTextBoxKeyPressInfo()
        {
            if (_osKeyPress.IsMultiSequenced())
            {
                if (!string.IsNullOrWhiteSpace(_osKeyPress.Information))
                {
                    return _osKeyPress.Information;
                }
                return "key press sequence";
            }
            return _osKeyPress.GetSimpleVirtualKeyCodesAsString();
        }

        public SortedList<int, KeyPressInfo> GetKeySequence()
        {
            return _osKeyPress.KeySequence;
        }

        /*public void SetKeySequence(SortedList<int, KeyPressInfo> sortedList)
        {
            _osKeyPress.KeySequence = sortedList;
        }*/

        public bool IsEmpty()
        {
            return _bipLinkPZ69 == null && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }
        
        public BIPLinkPZ69 BIPLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = value;
            }
        }

        public OSKeyPress KeyPress
        {
            get => _osKeyPress;
            set
            {
                _osKeyPress = value;
            }
        }

        public DCSBIOSBindingLCDPZ69 DCSBIOSBindingLCD
        {
            get => _dcsbiosBindingLCDPZ69;
            set => _dcsbiosBindingLCDPZ69 = value;
        }

        public DCSBIOSBindingPZ69 DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ69;
            set => _dcsbiosBindingPZ69 = value;
        }

        public void ClearAll()
        {
            _bipLinkPZ69 = null;
            _osKeyPress = null;
            _dcsbiosBindingLCDPZ69 = null;
            _dcsbiosBindingPZ69 = null;
        }
    }
}
