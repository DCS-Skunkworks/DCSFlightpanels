namespace DCSFlightpanels.Bills
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media;

    using DCS_BIOS;
    using DCSFlightpanels.Interfaces;


    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.Interfaces;
    using NonVisuals.Saitek;
    using NonVisuals.Saitek.Panels;

    public class BillPZ55 : BillBaseInput
    {
        private DCSBIOSActionBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;

        public BillPZ55(IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override BIPLink BipLink
        {
            get => _bipLinkPZ55;
            set
            {
                _bipLinkPZ55 = (BIPLinkPZ55)value;
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

        public override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                if (ContainsDCSBIOS())
                {
                    return _dcsbiosBindingPZ55.DCSBIOSInputs;
                }

                return null;
            }
            set
            {
                if (ContainsDCSBIOS())
                {
                    _dcsbiosBindingPZ55.DCSBIOSInputs = value;
                }
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ55;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingPZ55 = (DCSBIOSActionBindingPZ55)value;
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
                    TextBox.Text = string.Empty;
                }
            }
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
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ55 == null)
            {
                _dcsbiosBindingPZ55 = new DCSBIOSActionBindingPZ55();
            }
            _dcsbiosBindingPZ55.DCSBIOSInputs = dcsBiosInputs;
        }
        
        public override void ClearAll()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            KeyPress = null;
            OSCommandObject = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = string.Empty;
        }
    }
}
