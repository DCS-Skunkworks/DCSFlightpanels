using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Interfaces;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.Saitek.Panels;

namespace DCSFlightpanels.Bills
{
    public class BillPZ70 : BillBaseInput
    {
        private DCSBIOSActionBindingPZ70 _dcsbiosBindingPZ70;
        private BIPLinkPZ70 _bipLinkPZ70;

        public BillPZ70(IGlobalHandler globalHandler, IPanelUI panelUI, SaitekPanel saitekPanel, TextBox textBox) : base(globalHandler, textBox, panelUI, saitekPanel)
        {
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override BIPLink BipLink
        {
            get => _bipLinkPZ70;
            set
            {
                _bipLinkPZ70 = (BIPLinkPZ70)value;
                if (_bipLinkPZ70 != null)
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
            get
            {
                if (ContainsDCSBIOS())
                {
                    return _dcsbiosBindingPZ70.DCSBIOSInputs;
                }

                return null;
            }
            set
            {
                if (ContainsDCSBIOS())
                {
                    _dcsbiosBindingPZ70.DCSBIOSInputs = value;
                }
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ70;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingPZ70 = (DCSBIOSActionBindingPZ70)value;
                if (_dcsbiosBindingPZ70 != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingPZ70.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingPZ70.Description;
                    }
                }
                else
                {
                    TextBox.Text = "";
                }
            }
        }
        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ70 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ70 != null && _bipLinkPZ70.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ70 == null || _bipLinkPZ70.BIPLights.Count == 0) && (_dcsbiosBindingPZ70?.DCSBIOSInputs == null || _dcsbiosBindingPZ70.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeySequence.Count == 0) && OSCommandObject == null;
        }

        public override bool IsEmptyNoCareBipLink()
        {
            return (_dcsbiosBindingPZ70?.DCSBIOSInputs == null || _dcsbiosBindingPZ70.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeySequence.Count == 0) && OSCommandObject == null;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ70 == null)
            {
                _dcsbiosBindingPZ70 = new DCSBIOSActionBindingPZ70();
            }

            _dcsbiosBindingPZ70.DCSBIOSInputs = dcsBiosInputs;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingPZ70 = null;
            _bipLinkPZ70 = null;
            KeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
