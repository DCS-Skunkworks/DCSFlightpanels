using System;
using System.Collections.Generic;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassTPM
    {
        private List<DCSBIOSInput> _dcsbiosInputs;
        private BIPLinkTPM _bipLinkTPM;
        private OSKeyPress _osKeyPress;


        public bool ContainsDCSBIOS()
        {
            return _dcsbiosInputs != null;// && _dcsbiosInputs.Count > 0;
        }

        public bool ContainsBIPLink()
        {
            return _bipLinkTPM != null && _bipLinkTPM.BIPLights.Count > 0;
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
            return (_bipLinkTPM == null || _bipLinkTPM.BIPLights.Count == 0) && (_dcsbiosInputs == null || _dcsbiosInputs.Count == 0) && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }


        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get => _dcsbiosInputs;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosInputs = value;
            }
        }

        public BIPLinkTPM BIPLink
        {
            get => _bipLinkTPM;
            set
            {
                _bipLinkTPM = value;
            }
        }

        public OSKeyPress KeyPress
        {
            get => _osKeyPress;
            set
            {
                if (ContainsDCSBIOS())
                {
                    throw new Exception("Cannot insert KeyPress, TextBoxTagHolderClass already contains DCSBIOSInputs");
                }
                _osKeyPress = value;
            }
        }

        public void ClearAll()
        {
            _dcsbiosInputs = null;
            _bipLinkTPM = null;
            _osKeyPress = null;
        }
    }
}
