using ClassLibraryCommon;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using DCS_BIOS.Serialized;

namespace DCSFlightpanels.CustomControls
{
    public class PZ69TextBox : TextBoxBaseInput
    {
        private BIPLinkPZ69 _bipLinkPZ69;

        public PZ69TextBox()
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
            get => null;
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => null;
            set { }
        }
        
        protected override void ClearDCSBIOS()
        {
        }

        public override bool ContainsDCSBIOS()
        {
            return false;
        }

        protected override void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            throw new Exception("PZ69 cannot contain DCS-BIOS");
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return _bipLinkPZ69 == null && (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        protected override bool IsEmptyNoCareBipLink()
        {
            return (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) && OSCommandObject == null;
        }

        public override void ClearAll()
        {
            _bipLinkPZ69 = null;
            KeyPress = null;
            Background = DarkMode.TextBoxUnselectedBackgroundColor;
            Text = string.Empty;
        }
    }
}
