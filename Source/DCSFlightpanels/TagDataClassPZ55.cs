using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ55
    {
        private SwitchPanelPZ55KeyOnOff _key;
        private DCSBIOSBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;
        private OSKeyPress _osKeyPress;
        private readonly TextBox _textBox;

        public TagDataClassPZ55(TextBox textBox, SwitchPanelPZ55KeyOnOff key)
        {
            _textBox = textBox;
            _key = key;
        }

        public bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ55 != null;// && _dcsbiosInputs.Count > 0;
        }

        public bool ContainsBIPLink()
        {
            return _bipLinkPZ55 != null && _bipLinkPZ55.BIPLights.Count > 0;
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
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55 == null || _dcsbiosBindingPZ55.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }

        public void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ55 == null)
            {
                _dcsbiosBindingPZ55 = new DCSBIOSBindingPZ55();
            }

            _dcsbiosBindingPZ55.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSBindingPZ55 DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ55;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosBindingPZ55 = value;
                if (string.IsNullOrEmpty(_dcsbiosBindingPZ55.Description))
                {
                    _textBox.Text = "DCS-BIOS";
                }
                else
                {
                    _textBox.Text = _dcsbiosBindingPZ55.Description;
                }
            }
        }

        public BIPLinkPZ55 BIPLink
        {
            get => _bipLinkPZ55;
            set
            {
                _bipLinkPZ55 = value;
                _textBox.Background = Brushes.Bisque;
            }
        }

        public SwitchPanelPZ55KeyOnOff Key
        {
            get => _key;
            set => _key = value;
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

        public void ClearAll()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            _osKeyPress = null;
            _textBox.Background = Brushes.White;
            _textBox.Text = "";
        }
    }
}
