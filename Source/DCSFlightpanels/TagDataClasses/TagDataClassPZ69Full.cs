using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;

namespace DCSFlightpanels.TagDataClasses
{
    internal class TagDataClassPZ69Full : TagDataClassBase
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private DCSBIOSActionBindingPZ69 _dcsbiosBindingPZ69;

        public TagDataClassPZ69Full(TextBox textBox)
        {
            TextBox = textBox;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ69 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }

        public bool ContainsDCSBIOSBinding()
        {
            return _dcsbiosBindingPZ69 != null && _dcsbiosBindingPZ69.HasBinding();
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ69 == null)
            {
                _dcsbiosBindingPZ69 = new DCSBIOSActionBindingPZ69();
            }

            _dcsbiosBindingPZ69.DCSBIOSInputs = dcsBiosInputs;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ69 == null || _bipLinkPZ69.BIPLights.Count == 0) && (_dcsbiosBindingPZ69?.DCSBIOSInputs == null || _dcsbiosBindingPZ69.DCSBIOSInputs.Count == 0) && (OSKeyPress == null || OSKeyPress.KeySequence.Count == 0);
        }

        public BIPLinkPZ69 BIPLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = value;
                if (_bipLinkPZ69 != null)
                {
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }
        
        public DCSBIOSActionBindingPZ69 DCSBIOSBinding
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
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingPZ69.Description;
                    }
                }
                else
                {
                    TextBox.Text = "";
                }
            }
        }

        public override void ClearAll()
        {
            _bipLinkPZ69 = null;
            OSKeyPress = null;
            _dcsbiosBindingPZ69 = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
