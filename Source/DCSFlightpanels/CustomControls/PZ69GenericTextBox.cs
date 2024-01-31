using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using System.Collections.Generic;
using System;
using System.Windows.Media;

namespace DCSFlightpanels.CustomControls
{
    public class PZ69GenericTextBox : TextBoxBaseInput
    {
        private BIPLinkPZ69 _bipLinkPZ69;
        private DCSBIOSActionBindingPZ69 _dcsbiosBindingPZ69;

        public PZ69GenericTextBox()
        {
            SetContextMenu();
        }

        public override BIPLinkBase BipLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = (BIPLinkPZ69)value;
                Background = _bipLinkPZ69 != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
            }
        }

        protected override List<DCSBIOSInput> DCSBIOSInputs
        {
            get => ContainsDCSBIOS() ? _dcsbiosBindingPZ69.DCSBIOSInputs : null;
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ69;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBox already containsKeyPress");
                }
                _dcsbiosBindingPZ69 = (DCSBIOSActionBindingPZ69)value;
                SetTextBoxText(value);
            }
        }

        protected override void ClearDCSBIOS()
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

        protected override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            _dcsbiosBindingPZ69 ??= new DCSBIOSActionBindingPZ69();

            _dcsbiosBindingPZ69.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBindingPZ69.IsSequenced = isSequenced;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ69 == null || _bipLinkPZ69.BIPLights.Count == 0) && (_dcsbiosBindingPZ69?.DCSBIOSInputs == null || _dcsbiosBindingPZ69.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingPZ69?.DCSBIOSInputs == null || _dcsbiosBindingPZ69.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void ClearAll()
        {
            _bipLinkPZ69 = null;
            KeyPress = null;
            _dcsbiosBindingPZ69 = null;
            Background = DarkMode.TextBoxUnselectedBackgroundColor;
            Text = string.Empty;
        }
    }
}
