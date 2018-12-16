using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ70
    {
        private MultiPanelPZ70KnobOnOff _key;
        private DCSBIOSBindingPZ70 _dcsbiosBindingPZ70;
        private BIPLinkPZ70 _bipLinkPZ70;
        private OSKeyPress _osKeyPress;
        private readonly TextBox _textBox;

        public TagDataClassPZ70(TextBox textBox, MultiPanelPZ70KnobOnOff key)
        {
            _textBox = textBox;
            _key = key;
        }

        public bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ70 != null;// && _dcsbiosInputs.Count > 0;
        }

        public bool ContainsBIPLink()
        {
            return _bipLinkPZ70 != null && _bipLinkPZ70.BIPLights.Count > 0;
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
            return (_bipLinkPZ70 == null || _bipLinkPZ70.BIPLights.Count == 0) && (_dcsbiosBindingPZ70 == null || _dcsbiosBindingPZ70.DCSBIOSInputs == null || _dcsbiosBindingPZ70.DCSBIOSInputs.Count == 0) && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }


        public DCSBIOSBindingPZ70 DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ70;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosBindingPZ70 = value;
                if (string.IsNullOrEmpty(_dcsbiosBindingPZ70.Description))
                {
                    _textBox.Text = "DCS-BIOS";
                }
                else
                {
                    _textBox.Text = _dcsbiosBindingPZ70.Description;
                }
            }
        }

        public BIPLinkPZ70 BIPLink
        {
            get => _bipLinkPZ70;
            set
            {
                _bipLinkPZ70 = value;
                _textBox.Background = Brushes.Bisque;
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
                _textBox.Text = _osKeyPress.GetKeyPressInformation();
            }
        }

        public MultiPanelPZ70KnobOnOff Key
        {
            get => _key;
            set => _key = value;
        }

        public void ClearAll()
        {
            _dcsbiosBindingPZ70 = null;
            _bipLinkPZ70 = null;
            _osKeyPress = null;
            _textBox.Background = Brushes.White;
            _textBox.Text = "";
        }
    }
}
