namespace DCSFlightpanels.Bills
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media;

    using DCS_BIOS;
    using DCSFlightpanels.Interfaces;

    using NonVisuals.DCSBIOSBindings;
    using NonVisuals.Saitek.BindingClasses;
    using NonVisuals.Saitek.Panels;

    public class BillPZ69Generic : BillBaseInput
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private DCSBIOSActionBindingPZ69 _dcsbiosBindingPZ69;

        public override BIPLinkBase BipLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = (BIPLinkPZ69)value;
                TextBox.Background = _bipLinkPZ69 != null ? Brushes.Bisque : Brushes.White;
            }
        }

        public override List<DCSBIOSInput> DCSBIOSInputs
        {
            get => ContainsDCSBIOS() ? _dcsbiosBindingPZ69.DCSBIOSInputs : null;
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
                SetTextBoxText(value);
            }
        }

        public BillPZ69Generic(IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ69 != null;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }
        
        public override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            if (_dcsbiosBindingPZ69 == null)
            {
                _dcsbiosBindingPZ69 = new DCSBIOSActionBindingPZ69();
            }

            _dcsbiosBindingPZ69.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBindingPZ69.IsSequenced = isSequenced;
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
