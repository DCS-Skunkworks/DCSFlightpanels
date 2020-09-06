using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Interfaces;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Bills
{
    public class BillPZ55 : BillBaseInput
    {
        private DCSBIOSActionBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;
        private SwitchPanelPZ55 _switchPanelPZ55;

        public BillPZ55(IGlobalHandler globalHandler, IPanelUI panelUI, SwitchPanelPZ55 switchPanelPZ55, PZ55TextBox textBox, PZ55SwitchOnOff key) : base(switchPanelPZ55)
        {
            GlobalHandler = globalHandler;
            PanelUIParent = panelUI;
            _switchPanelPZ55 = switchPanelPZ55;
            TextBox = textBox;
            Key = key;
            SetContextMenu();
        }

        protected override void ClearDCSBIOSFromBill()
        {
            DCSBIOSBinding = null;
        }

        public override BIPLink BipLink
        {
            get
            {
                if (ContainsBIPLink())
                {
                    return _bipLinkPZ55;
                }

                return null;
            }
            set => _bipLinkPZ55 = (BIPLinkPZ55)value;
        }

        public override List<DCSBIOSInput> DCSBIOSInputs
        {
            get
            {
                if (ContainsDCSBIOS())
                {
                    return _dcsbiosBindingPZ55.DCSBIOSInputs;
                }

                return null;
            }
            set
            {
                if (ContainsDCSBIOS())
                {
                    _dcsbiosBindingPZ55.DCSBIOSInputs = value;
                }
            }
        }

        public override DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ55;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingPZ55 = (DCSBIOSActionBindingPZ55)value;
                if (_dcsbiosBindingPZ55 != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingPZ55.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingPZ55.Description;
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
            return _dcsbiosBindingPZ55 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ55 != null && _bipLinkPZ55.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ55 == null)
            {
                _dcsbiosBindingPZ55 = new DCSBIOSActionBindingPZ55();
            }
            _dcsbiosBindingPZ55.DCSBIOSInputs = dcsBiosInputs;
        }

        public BIPLinkPZ55 BIPLink
        {
            get => _bipLinkPZ55;
            set
            {
                _bipLinkPZ55 = value;
                if (_bipLinkPZ55 != null)
                {
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }

        public PZ55SwitchOnOff Key { get; set; }


        public override void Clear()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            KeyPress = null;
            OSCommandObject = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
