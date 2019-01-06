using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassTPM : TagDataClassBase
    {
        private TPMPanelSwitchOnOff _key;
        private DCSBIOSBindingTPM _dcsbiosBindingTPM;
        private BIPLinkTPM _bipLinkTPM;

        public TagDataClassTPM(TextBox textBox, TPMPanelSwitchOnOff key)
        {
            _textBox = textBox;
            _key = key;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingTPM != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkTPM != null && _bipLinkTPM.BIPLights.Count > 0;
        }
        
        public override bool IsEmpty()
        {
            return (_bipLinkTPM == null || _bipLinkTPM.BIPLights.Count == 0) && (_dcsbiosBindingTPM?.DCSBIOSInputs == null || _dcsbiosBindingTPM.DCSBIOSInputs.Count == 0) && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingTPM == null)
            {
                _dcsbiosBindingTPM = new DCSBIOSBindingTPM();
            }

            _dcsbiosBindingTPM.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSBindingTPM DCSBIOSBinding
        {
            get => _dcsbiosBindingTPM;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosBindingTPM = value;
                if (_dcsbiosBindingTPM != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingTPM.Description))
                    {
                        _textBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        _textBox.Text = _dcsbiosBindingTPM.Description;
                    }
                }
                else
                {
                    _textBox.Text = "";
                }
            }
        }

        public BIPLinkTPM BIPLink
        {
            get => _bipLinkTPM;
            set
            {
                _bipLinkTPM = value;
                if (_bipLinkTPM != null)
                {
                    _textBox.Background = Brushes.Bisque;
                }
                else
                {
                    _textBox.Background = Brushes.White;
                }
            }
        }
        
        public TPMPanelSwitchOnOff Key
        {
            get => _key;
            set => _key = value;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingTPM = null;
            _bipLinkTPM = null;
            _osKeyPress = null;
            _textBox.Background = Brushes.White;
            _textBox.Text = "";
        }
    }
}
