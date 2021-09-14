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

    public class BillPFarmingPanel : BillBaseInput
    {
        private DCSBIOSActionBindingFarmingPanel _dcsbiosBinding;
        private BIPLinkFarmingPanel _bipLink;

        public BillPFarmingPanel(IGlobalHandler globalHandler, IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(globalHandler, textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override BIPLink BipLink
        {
            get => _bipLink;
            set
            {
                _bipLink = (BIPLinkFarmingPanel)value;
                if (_bipLink != null)
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
                    return _dcsbiosBinding.DCSBIOSInputs;
                }

                return null;
            }
            set
            {
                if (ContainsDCSBIOS())
                {
                    _dcsbiosBinding.DCSBIOSInputs = value;
                }
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBinding;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBinding = (DCSBIOSActionBindingFarmingPanel)value;
                if (_dcsbiosBinding != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBinding.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBinding.Description;
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
            return _dcsbiosBinding != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLink != null && _bipLink.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLink == null || _bipLink.BIPLights.Count == 0) && (_dcsbiosBinding?.DCSBIOSInputs == null || _dcsbiosBinding.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBinding?.DCSBIOSInputs == null || _dcsbiosBinding.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBinding == null)
            {
                _dcsbiosBinding = new DCSBIOSActionBindingFarmingPanel();
            }
            _dcsbiosBinding.DCSBIOSInputs = dcsBiosInputs;
        }
        
        public override void ClearAll()
        {
            _dcsbiosBinding = null;
            _bipLink = null;
            KeyPress = null;
            OSCommandObject = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = string.Empty;
        }
    }
}
