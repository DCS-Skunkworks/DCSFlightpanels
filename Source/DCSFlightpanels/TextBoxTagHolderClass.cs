using System;
using System.Collections.Generic;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TextBoxTagHolderClass
    {
        private List<DCSBIOSInput> _dcsbiosInputs;
        private SortedList<int, BIPLight> _bipSequence;
        private OSKeyPress _osKeyPress;


        public bool ContainsDCSBIOS()
        {
            return _dcsbiosInputs != null;
        }

        public bool ContainsBIPLight()
        {
            return _bipSequence != null;
        }

        public bool ContainsOSKeyPress()
        {
            return _osKeyPress != null;
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
            return _bipSequence == null && _dcsbiosInputs == null && _osKeyPress == null;
        }


        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get => _dcsbiosInputs;
            set
            {
                if (ContainsBIPLight())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains BIPLights");
                }
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosInputs = value;
            }
        }

        public SortedList<int, BIPLight> BIPSequence
        {
            get => _bipSequence;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert BIPSequence, TextBoxTagHolderClass already contains KeyPress");
                }
                if (ContainsDCSBIOS())
                {
                    throw new Exception("Cannot insert BIPSequence, TextBoxTagHolderClass already contains DCSBIOSInputs");
                }
                _bipSequence = value;
            }
        }

        public OSKeyPress KeyPress
        {
            get => _osKeyPress;
            set
            {
                if (ContainsBIPLight())
                {
                    throw new Exception("Cannot insert KeyPress, TextBoxTagHolderClass already contains BIPLights");
                }
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
            _bipSequence = null;
            _osKeyPress = null;
        }
    }
}
