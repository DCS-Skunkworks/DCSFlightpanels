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

    public class BillPZ69Full : BillBaseInput
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private DCSBIOSActionBindingPZ69 _dcsbiosBindingPZ69;

        public BillPZ69Full(IGlobalHandler globalHandler, IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(globalHandler, textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override BIPLink BipLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = (BIPLinkPZ69)value;
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

        public override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                if (ContainsDCSBIOS())
                {
                    return _dcsbiosBindingPZ69.DCSBIOSInputs;
                }

                return null;
            }
            set
            {
                if (ContainsDCSBIOS())
                {
                    _dcsbiosBindingPZ69.DCSBIOSInputs = value;
                }
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ69;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingPZ69 = (DCSBIOSActionBindingPZ69)value;
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
                    TextBox.Text = string.Empty;
                }
            }
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ69 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
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
            return (_bipLinkPZ69 == null || _bipLinkPZ69.BIPLights.Count == 0) && (_dcsbiosBindingPZ69?.DCSBIOSInputs == null || _dcsbiosBindingPZ69.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingPZ69?.DCSBIOSInputs == null || _dcsbiosBindingPZ69.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void ClearAll()
        {
            _bipLinkPZ69 = null;
            KeyPress = null;
            _dcsbiosBindingPZ69 = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = string.Empty;
        }
    }
}
