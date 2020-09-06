using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using DCSFlightpanels.Interfaces;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Bills
{
    public class BillPZ69 : BillBaseInput
    {
        private PZ69SwitchOnOff _knob;
        private BIPLinkPZ69 _bipLinkPZ69;

        public BillPZ69(IGlobalHandler globalHandler, IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox, PZ69SwitchOnOff knob) : base(globalHandler, textBox, panelUI, saitekPanel)
        {
            TextBox = textBox;
            _knob = knob;
        }

        protected override void ClearDCSBIOSFromBill()
        {
        }

        public override BIPLink BipLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = (BIPLinkPZ69)value;
                if (_bipLinkPZ69 != null)
                {
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }

        public override List<DCSBIOSInput> DCSBIOSInputs
        {
            get => null;
            set
            {
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => null;
            set
            {
            }
        }

        public override bool ContainsDCSBIOS()
        {
            return false;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            throw new Exception("BillPZ69 cannot contain DCS-BIOS");
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }
        
        public override bool IsEmpty()
        {
            return _bipLinkPZ69 == null && (KeyPress == null || KeyPress.KeySequence.Count == 0);
        }

        public PZ69SwitchOnOff Knob => _knob;

        public override void Clear()
        {
            _bipLinkPZ69 = null;
            KeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
