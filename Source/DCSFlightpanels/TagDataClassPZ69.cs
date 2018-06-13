using System;
using System.Collections.Generic;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ69
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private OSKeyPress _osKeyPress;
        //hit ska komma värde för LCD:n också....
        
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

        public void ClearAll()
        {
            _bipLinkPZ69 = null;
            _osKeyPress = null;
        }
    }
}
