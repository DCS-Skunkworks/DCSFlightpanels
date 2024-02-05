using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using System.Collections.Generic;
using System;
using System.Windows.Media;

namespace DCSFlightpanels.CustomControls
{
    public class PZ70TextBox : TextBoxBaseInput
    {
        private DCSBIOSActionBindingPZ70 _dcsbiosBindingPZ70;
        private BIPLinkPZ70 _bipLinkPZ70;

        public PZ70TextBox()
        {
            SetContextMenu();
        }

        public override BIPLinkBase BipLink
        {
            get => _bipLinkPZ70;
            set
            {
                _bipLinkPZ70 = (BIPLinkPZ70)value;
                Background = _bipLinkPZ70 != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
            }
        }

        protected override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                return ContainsDCSBIOS() ? _dcsbiosBindingPZ70.DCSBIOSInputs : null;
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ70;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBox already containsKeyPress");
                }
                _dcsbiosBindingPZ70 = (DCSBIOSActionBindingPZ70)value;
                SetTextBoxText(value);
            }
        }

        protected override void ClearDCSBIOS()
        {
            DCSBIOSBinding = null;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ70 != null;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ70 != null && _bipLinkPZ70.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ70 == null || _bipLinkPZ70.BIPLights.Count == 0) && (_dcsbiosBindingPZ70?.DCSBIOSInputs == null || _dcsbiosBindingPZ70.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingPZ70?.DCSBIOSInputs == null || _dcsbiosBindingPZ70.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            _dcsbiosBindingPZ70 ??= new DCSBIOSActionBindingPZ70();

            _dcsbiosBindingPZ70.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBindingPZ70.IsSequenced = isSequenced;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingPZ70 = null;
            _bipLinkPZ70 = null;
            KeyPress = null;
            Background = DarkMode.TextBoxUnselectedBackgroundColor;
            Text = string.Empty;
        }
    }
}
