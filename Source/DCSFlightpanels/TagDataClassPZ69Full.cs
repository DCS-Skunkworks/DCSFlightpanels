using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ69Full
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private OSKeyPress _osKeyPress;
        private DCSBIOSBindingPZ69 _dcsbiosBindingPZ69;
        private readonly TextBox _textBox;

        public TagDataClassPZ69Full(TextBox textBox)
        {
            _textBox = textBox;
        }

        public bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ69 != null;// && _dcsbiosInputs.Count > 0;
        }

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
            return _osKeyPress != null && !_osKeyPress.IsMultiSequenced() && _osKeyPress.KeySequence.Count > 0;
        }

        public bool ContainsDCSBIOSBinding()
        {
            return _dcsbiosBindingPZ69 != null && _dcsbiosBindingPZ69.HasBinding();
        }

        public SortedList<int, KeyPressInfo> GetKeySequence()
        {
            return _osKeyPress.KeySequence;
        }

        /*public void SetKeySequence(SortedList<int, KeyPressInfo> sortedList)
        {
            _osKeyPress.KeySequence = sortedList;
        }*/

        public void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ69 == null)
            {
                _dcsbiosBindingPZ69 = new DCSBIOSBindingPZ69();
            }

            _dcsbiosBindingPZ69.DCSBIOSInputs = dcsBiosInputs;
        }

        public bool IsEmpty()
        {
            return (_bipLinkPZ69 == null || _bipLinkPZ69.BIPLights.Count == 0) && (_dcsbiosBindingPZ69 == null || _dcsbiosBindingPZ69.DCSBIOSInputs == null || _dcsbiosBindingPZ69.DCSBIOSInputs.Count == 0) && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }

        public BIPLinkPZ69 BIPLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = value;
                if (_bipLinkPZ69 != null)
                {
                    _textBox.Background = Brushes.Bisque;
                }
                else
                {
                    _textBox.Background = Brushes.White;
                }
            }
        }

        public OSKeyPress KeyPress
        {
            get => _osKeyPress;
            set
            {
                if (ContainsDCSBIOSBinding())
                {
                    throw new Exception("Cannot insert KeyPress, TextBoxTagHolderClass already contains DCSBIOSInputs");
                }
                _osKeyPress = value;
                if (_osKeyPress != null)
                {
                    _textBox.Text = _osKeyPress.GetKeyPressInformation();
                }
                else
                {
                    _textBox.Text = "";
                }
            }
        }

        public DCSBIOSBindingPZ69 DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ69;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosBindingPZ69 = value;
                if (_dcsbiosBindingPZ69 != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingPZ69.Description))
                    {
                        _textBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        _textBox.Text = _dcsbiosBindingPZ69.Description;
                    }
                }
                else
                {
                    _textBox.Text = "";
                }
            }
        }

        public void ClearAll()
        {
            _bipLinkPZ69 = null;
            _osKeyPress = null;
            _dcsbiosBindingPZ69 = null;
            _textBox.Background = Brushes.White;
            _textBox.Text = "";
        }
    }
}
