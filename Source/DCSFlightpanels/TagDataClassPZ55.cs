using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ55 : TagDataClassBase
    {
        private DCSBIOSBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;

        public TagDataClassPZ55(TextBox textBox, SwitchPanelPZ55KeyOnOff key) : base()
        {
            _textBox = textBox;
            Key = key;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ55 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ55 != null && _bipLinkPZ55.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
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
                if (_dcsbiosBindingPZ55 != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingPZ55.Description))
                    {
                        _textBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        _textBox.Text = _dcsbiosBindingPZ55.Description;
                    }
                }
                else
                {
                    _textBox.Text = "";
                }
            }
        }

        public BIPLinkPZ55 BIPLink
        {
            get => _bipLinkPZ55;
            set
            {
                _bipLinkPZ55 = value;
                if (_bipLinkPZ55 != null)
                {
                    _textBox.Background = Brushes.Bisque;
                }
                else
                {
                    _textBox.Background = Brushes.White;
                }
            }
        }

        public SwitchPanelPZ55KeyOnOff Key { get; set; }


        public override void ClearAll()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            _osKeyPress = null;
            _textBox.Background = Brushes.White;
            _textBox.Text = "";
        }
    }
}
