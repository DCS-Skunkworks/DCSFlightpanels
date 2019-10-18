using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;
using NonVisuals.Saitek;

namespace DCSFlightpanels.TagDataClasses
{
    internal class TagDataClassPZ69 : TagDataClassBase
    {
        private RadioPanelPZ69KeyOnOff _knob;
        private BIPLinkPZ69 _bipLinkPZ69;

        public TagDataClassPZ69(TextBox textBox, RadioPanelPZ69KeyOnOff knob)
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
            throw new Exception("TagDataClassPZ69 cannot contain DCS-BIOS");
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }
        
        public override bool IsEmpty()
        {
            return _bipLinkPZ69 == null && (OSKeyPress == null || OSKeyPress.KeySequence.Count == 0);
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
            OSKeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
