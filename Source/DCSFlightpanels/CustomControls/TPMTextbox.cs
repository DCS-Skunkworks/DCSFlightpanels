using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using System.Collections.Generic;
using System;
using System.Windows.Media;

namespace DCSFlightpanels.CustomControls
{
    public class TPMTextBox : TextBoxBaseInput
    {
        private DCSBIOSActionBindingTPM _dcsbiosBindingTPM;
        private BIPLinkTPM _bipLinkTPM;

        public TPMTextBox()
        {
            SetContextMenu();
        }

        public override BIPLinkBase BipLink
        {
            get => _bipLinkTPM;
            set
            {
                _bipLinkTPM = (BIPLinkTPM)value;
                Background = _bipLinkTPM != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
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
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBox already containsKeyPress");
                }
                _dcsbiosBindingTPM = (DCSBIOSActionBindingTPM)value;
                SetTextBoxText(value);
            }
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
            _dcsbiosBindingTPM ??= new DCSBIOSActionBindingTPM();

            _dcsbiosBindingTPM.DCSBIOSInputs = dcsBiosInputs;
            _dcsbiosBindingTPM.IsSequenced = isSequenced;
        }

        protected override void ClearDCSBIOS()
        {
            DCSBIOSBinding = null;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingTPM = null;
            _bipLinkTPM = null;
            KeyPress = null;
            Background = DarkMode.TextBoxUnselectedBackgroundColor;
            Text = string.Empty;
        }
    }
}
