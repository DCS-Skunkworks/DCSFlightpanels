using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using System.Collections.Generic;
using System;
using System.Windows.Media;

namespace DCSFlightpanels.CustomControls
{
    public class PZ55TextBox : TextBoxBaseInput
    {
        private DCSBIOSActionBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;

        public PZ55TextBox()
        {
            SetContextMenu();
        }

        public override BIPLinkBase BipLink
        {
            get => _bipLinkPZ55;
            set
            {
                _bipLinkPZ55 = (BIPLinkPZ55)value;
                Background = _bipLinkPZ55 != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
            }
        }

        protected override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                return ContainsDCSBIOS() ? _dcsbiosBindingPZ55.DCSBIOSInputs : null;
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ55;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBox already containsKeyPress " + Name);
                }
                _dcsbiosBindingPZ55 = (DCSBIOSActionBindingPZ55)value;
                SetTextBoxText(value);
            }
        }

        protected override void ClearDCSBIOS()
        {
            DCSBIOSBinding = null;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ55 != null;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ55 != null && _bipLinkPZ55.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            _dcsbiosBindingPZ55 ??= new DCSBIOSActionBindingPZ55();
            _dcsbiosBindingPZ55.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBindingPZ55.IsSequenced = isSequenced;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            KeyPress = null;
            OSCommandObject = null;
            Background = DarkMode.TextBoxUnselectedBackgroundColor;
            Text = string.Empty;
        }
    }
}