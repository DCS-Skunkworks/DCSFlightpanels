using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using DCSFlightpanels.TagDataClasses;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;

namespace DCSFlightpanels.TagDataClasses
{
    internal class TagDataClassPZ55 : TagDataClassBase
    {
        private DCSBIOSActionBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;

        public TagDataClassPZ55(TextBox textBox, SwitchPanelPZ55KeyOnOff key) : base()
        {
            TextBox = textBox;
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
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (OSKeyPress == null || OSKeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ55 == null)
            {
                _dcsbiosBindingPZ55 = new DCSBIOSActionBindingPZ55();
            }
            _dcsbiosBindingPZ55.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSActionBindingPZ55 DCSBIOSBinding
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
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingPZ55.Description;
                    }
                }
                else
                {
                    TextBox.Text = "";
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
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }

        public SwitchPanelPZ55KeyOnOff Key { get; set; }


        public override void ClearAll()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            OSKeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
