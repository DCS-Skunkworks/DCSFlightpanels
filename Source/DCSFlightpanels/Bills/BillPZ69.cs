using System;
using System.Collections.Generic;
using System.Windows.Media;
using DCS_BIOS;
using DCSFlightpanels.CustomControls;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Bills
{
    public class BillPZ69 : BillBase
    {
        private RadioPanelPZ69KeyOnOff _knob;
        private BIPLinkPZ69 _bipLinkPZ69;

        public BillPZ69(PZ69TextBox textBox, RadioPanelPZ69KeyOnOff knob)
        {
            TextBox = textBox;
            _knob = knob;
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
        
        public BIPLinkPZ69 BIPLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = value;
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

        public RadioPanelPZ69KeyOnOff Knob => _knob;

        public override void ClearAll()
        {
            _bipLinkPZ69 = null;
            KeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
