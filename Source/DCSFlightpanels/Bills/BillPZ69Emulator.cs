namespace DCSFlightpanels.Bills
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

    public class BillPZ69Emulator : BillBaseInput
    {
        private BIPLinkPZ69 _bipLinkPZ69;

        public override BIPLink BipLink
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
            get => null;
            set {}
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => null;
            set {}
        }

        public BillPZ69Emulator(IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
        }

        public override bool ContainsDCSBIOS()
        {
            return false;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            throw new Exception("BillPZ69 cannot contain DCS-BIOS");
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }
        
        public override bool IsEmpty()
        {
            return _bipLinkPZ69 == null && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override bool IsEmptyNoCareBipLink()
        {
            return (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void ClearAll()
        {
            _bipLinkPZ69 = null;
            KeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = string.Empty;
        }
    }
}
