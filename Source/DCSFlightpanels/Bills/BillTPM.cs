using ClassLibraryCommon;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;

namespace DCSFlightpanels.Bills
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media;

    using DCS_BIOS;
    using Interfaces;
    using NonVisuals.Panels.Saitek.Panels;

    public class BillTPM : BillBaseInput
    {
        private DCSBIOSActionBindingTPM _dcsbiosBindingTPM;
        private BIPLinkTPM _bipLinkTPM;

        public override BIPLinkBase BipLink
        {
            get => _bipLinkTPM;
            set
            {
                _bipLinkTPM = (BIPLinkTPM)value;
                TextBox.Background = _bipLinkTPM != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
            }
        }

        protected override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                return ContainsDCSBIOS() ? _dcsbiosBindingTPM.DCSBIOSInputs : null;
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
                SetTextBoxText(value);
            }
        }

        public BillTPM(IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingTPM != null;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkTPM != null && _bipLinkTPM.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkTPM == null || _bipLinkTPM.BIPLights.Count == 0) && (_dcsbiosBindingTPM?.DCSBIOSInputs == null || _dcsbiosBindingTPM.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingTPM?.DCSBIOSInputs == null || _dcsbiosBindingTPM.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            if (_dcsbiosBindingTPM == null)
            {
                _dcsbiosBindingTPM = new DCSBIOSActionBindingTPM();
            }

            _dcsbiosBindingTPM.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBindingTPM.IsSequenced = isSequenced;
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingTPM = null;
            _bipLinkTPM = null;
            KeyPress = null;
            TextBox.Background = DarkMode.TextBoxUnselectedBackgroundColor;
            TextBox.Text = string.Empty;
        }
    }
}
