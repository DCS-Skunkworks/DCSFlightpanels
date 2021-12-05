﻿namespace DCSFlightpanels.Bills
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media;

    using DCS_BIOS;
    using DCSFlightpanels.Interfaces;
    
    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.Saitek;
    using NonVisuals.Saitek.Panels;

    public class BillPFarmingPanel : BillBaseInput
    {
        private DCSBIOSActionBindingFarmingPanel _dcsbiosBinding;
        private BIPLinkFarmingPanel _bipLink;

        public override BIPLink BipLink
        {
            get => _bipLink;
            set
            {
                _bipLink = (BIPLinkFarmingPanel)value;
                TextBox.Background = _bipLink != null ? Brushes.Bisque : Brushes.White;
            }
        }

        public override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                return ContainsDCSBIOS() ? _dcsbiosBinding.DCSBIOSInputs : null;
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
                    TextBox.Text = string.IsNullOrEmpty(_dcsbiosBinding.Description) ? "DCS-BIOS" : _dcsbiosBinding.Description;
                }
                else
                {
                    TextBox.Text = string.Empty;
                }
            }
        }

        public BillPFarmingPanel(IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBinding != null;
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
