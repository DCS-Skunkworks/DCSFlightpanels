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

    public class BillTPM : BillBaseInput
    {
        private DCSBIOSActionBindingTPM _dcsbiosBindingTPM;
        private BIPLinkTPM _bipLinkTPM;
        
        public BillTPM(IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
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
            return (_bipLinkTPM == null || _bipLinkTPM.BIPLights.Count == 0) && (_dcsbiosBindingTPM?.DCSBIOSInputs == null || _dcsbiosBindingTPM.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingTPM?.DCSBIOSInputs == null || _dcsbiosBindingTPM.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingTPM == null)
            {
                _dcsbiosBindingTPM = new DCSBIOSActionBindingTPM();
            }

            _dcsbiosBindingTPM.DCSBIOSInputs = dcsBiosInputs;
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }
        
        public override BIPLink BipLink
        {
            get => _bipLinkTPM;
            set
            {
                _bipLinkTPM = (BIPLinkTPM)value;
                if (_bipLinkTPM != null)
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
                    return _dcsbiosBindingTPM.DCSBIOSInputs;
                }

                return null;
            }
            set
            {
                if (ContainsDCSBIOS())
                {
                    _dcsbiosBindingTPM.DCSBIOSInputs = value;
                }
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingTPM;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingTPM = (DCSBIOSActionBindingTPM)value;
                if (_dcsbiosBindingTPM != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingTPM.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingTPM.Description;
                    }
                }
                else
                {
                    TextBox.Text = string.Empty;
                }
            }
        }

        public override void ClearAll()
        {
            _dcsbiosBindingTPM = null;
            _bipLinkTPM = null;
            KeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = string.Empty;
        }
    }
}
