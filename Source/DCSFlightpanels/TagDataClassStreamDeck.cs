using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassStreamDeck : TagDataClassBase
    {
        private StreamDeckKeyOnOff _key;
        private DCSBIOSBindingStreamDeck _dcsbiosBindingStreamDeck;
        private BIPLinkStreamDeck _bipLinkStreamDeck;

        public TagDataClassStreamDeck(TextBox textBox, StreamDeckKeyOnOff key)
        {
            TextBox = textBox;
            _key = key;
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
                _dcsbiosBindingStreamDeck = new DCSBIOSBindingStreamDeck();
            }

            _dcsbiosBindingStreamDeck.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSBindingStreamDeck DCSBIOSBinding
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
        
        public StreamDeckKeyOnOff Key
        {
            get => _key;
            set => _key = value;
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
