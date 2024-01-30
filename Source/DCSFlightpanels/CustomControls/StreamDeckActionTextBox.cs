using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Bills;
using NonVisuals.Panels.Saitek;
using NonVisuals.Panels.StreamDeck.Panels;
using NonVisuals.Panels.StreamDeck;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckActionTextBox : TextBoxBaseStreamDeckInput
    {
        private ActionTypeDCSBIOS _dcsbiosBindingStreamDeck;
        private BIPLinkStreamDeck _bipLinkStreamDeck;
        private StreamDeckPanel _streamDeckPanel;

        public StreamDeckButtonOnOff Key { get; set; }
        public ActionTypeLayer StreamDeckLayerTarget { get; set; }

        public BIPLinkStreamDeck BIPLink
        {
            get => _bipLinkStreamDeck;
            set
            {
                _bipLinkStreamDeck = value;
                TextBox.Background = _bipLinkStreamDeck != null ? Brushes.Bisque : DarkMode.TextBoxUnselectedBackgroundColor;
            }
        }

        public ActionTypeDCSBIOS DCSBIOSBinding
        {
            get => _dcsbiosBindingStreamDeck;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingStreamDeck = value;
                if (_dcsbiosBindingStreamDeck != null)
                {
                    TextBox.Text = string.IsNullOrEmpty(_dcsbiosBindingStreamDeck.Description) ? "DCS-BIOS" : _dcsbiosBindingStreamDeck.Description;
                }
                else
                {
                    TextBox.Text = string.Empty;
                }
            }
        }

        public void SetEnvironment(StreamDeckButtonOnOff button, StreamDeckPanel streamDeckPanel)
        {
            Key = button;
            _streamDeckPanel = streamDeckPanel;
        }

        public override void Clear()
        {
            KeyPress = null;
            OSCommandObject = null;
            Key = null;
            _dcsbiosBindingStreamDeck = null;
            _bipLinkStreamDeck = null;
            TextBox.Background = Brushes.LightSteelBlue;
            TextBox.Text = string.Empty;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingStreamDeck != null;
        }

        public bool ContainsStreamDeckLayer()
        {
            return StreamDeckLayerTarget != null;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkStreamDeck != null && _bipLinkStreamDeck.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkStreamDeck == null || _bipLinkStreamDeck.BIPLights.Count == 0) &&
                   (_dcsbiosBindingStreamDeck?.DCSBIOSInputs == null || _dcsbiosBindingStreamDeck.DCSBIOSInputs.Count == 0) &&
                   (KeyPress == null || KeyPress.KeyPressSequence.Count == 0) &&
                   StreamDeckLayerTarget == null;
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingStreamDeck == null)
            {
                _dcsbiosBindingStreamDeck = new ActionTypeDCSBIOS(_streamDeckPanel);
            }

            _dcsbiosBindingStreamDeck.DCSBIOSInputs = dcsBiosInputs;
        }
    }
}
