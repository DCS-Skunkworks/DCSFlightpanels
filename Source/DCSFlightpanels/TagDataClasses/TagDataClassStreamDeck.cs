using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;

namespace DCSFlightpanels.TagDataClasses
{
    internal class TagDataClassStreamDeck : TagDataClassBase
    {
        private StreamDeckButtonOnOff _button;
        private DCSBIOSActionBindingStreamDeck _dcsbiosBindingStreamDeck;
        private BIPLinkStreamDeck _bipLinkStreamDeck;

        public TagDataClassStreamDeck(TextBox textBox, StreamDeckButtonOnOff button)
        {
            TextBox = textBox;
            _button = button;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingStreamDeck != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkStreamDeck != null && _bipLinkStreamDeck.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkStreamDeck == null || _bipLinkStreamDeck.BIPLights.Count == 0) && (_dcsbiosBindingStreamDeck?.DCSBIOSInputs == null || _dcsbiosBindingStreamDeck.DCSBIOSInputs.Count == 0) && (OSKeyPress == null || OSKeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingStreamDeck == null)
            {
                _dcsbiosBindingStreamDeck = new DCSBIOSActionBindingStreamDeck();
            }

            _dcsbiosBindingStreamDeck.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSActionBindingStreamDeck DCSBIOSBinding
        {
            get => _dcsbiosBindingStreamDeck;
            set
            {
                if (ContainsOSKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, TextBoxTagHolderClass already contains KeyPress");
                }
                _dcsbiosBindingStreamDeck = value;
                if (_dcsbiosBindingStreamDeck != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingStreamDeck.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingStreamDeck.Description;
                    }
                }
                else
                {
                    TextBox.Text = "";
                }
            }
        }

        public BIPLinkStreamDeck BIPLink
        {
            get => _bipLinkStreamDeck;
            set
            {
                _bipLinkStreamDeck = value;
                if (_bipLinkStreamDeck != null)
                {
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }
        
        public StreamDeckButtonOnOff Key
        {
            get => _button;
            set => _button = value;
        }

        public override void ClearAll()
        {
            _dcsbiosBindingStreamDeck = null;
            _bipLinkStreamDeck = null;
            OSKeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
