using ClassLibraryCommon;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using DCS_BIOS.Serialized;

namespace DCSFlightpanels.CustomControls
{
    public class FarmingPanelTextBox : TextBoxBaseInput
    {
        private DCSBIOSActionBindingFarmingPanel _dcsbiosBinding;
        private BIPLinkFarmingPanel _bipLink;

        public FarmingPanelTextBox()
        {
            SetContextMenu();
        }

        public override BIPLinkBase BipLink
        {
            get => _bipLink;
            set
            {
                _bipLink = (BIPLinkFarmingPanel)value;
                Background = _bipLink != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
            }
        }

        protected override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                return ContainsDCSBIOS() ? _dcsbiosBinding.DCSBIOSInputs : null;
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBinding;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBox already contains KeyPress");
                }
                _dcsbiosBinding = (DCSBIOSActionBindingFarmingPanel)value;
                SetTextBoxText(value);
            }
        }

        protected override void ClearDCSBIOS()
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

        protected override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBinding?.DCSBIOSInputs == null || _dcsbiosBinding.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            _dcsbiosBinding ??= new DCSBIOSActionBindingFarmingPanel();
            _dcsbiosBinding.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBinding.IsSequenced = isSequenced;
        }

        public override void ClearAll()
        {
            _dcsbiosBinding = null;
            _bipLink = null;
            KeyPress = null;
            OSCommandObject = null;
            Background = DarkMode.TextBoxUnselectedBackgroundColor;
            Text = string.Empty;
        }
    }
}