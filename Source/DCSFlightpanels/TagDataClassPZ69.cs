using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using NonVisuals;

namespace DCSFlightpanels
{
    internal class TagDataClassPZ69
    {
        private RadioPanelPZ69KeyOnOff _knob;
        private BIPLinkPZ69 _bipLinkPZ69;
        private OSKeyPress _osKeyPress;
        private TextBox _textBox;

        public TagDataClassPZ69(TextBox textBox, RadioPanelPZ69KeyOnOff knob)
        {
            _textBox = textBox;
            _knob = knob;
        }

        public bool ContainsBIPLink()
        {
            return _bipLinkPZ69 != null && _bipLinkPZ69.BIPLights.Count > 0;
        }

        public bool ContainsOSKeyPress()
        {
            return _osKeyPress != null && _osKeyPress.KeySequence.Count > 0;
        }

        public bool ContainsKeySequence()
        {
            return _osKeyPress != null && _osKeyPress.IsMultiSequenced();
        }

        public bool ContainsSingleKey()
        {
            return _osKeyPress != null && !_osKeyPress.IsMultiSequenced();
        }

        public SortedList<int, KeyPressInfo> GetKeySequence()
        {
            return _osKeyPress.KeySequence;
        }

        /*public void SetKeySequence(SortedList<int, KeyPressInfo> sortedList)
        {
            _osKeyPress.KeySequence = sortedList;
        }*/

        public bool IsEmpty()
        {
            return _bipLinkPZ69 == null && (_osKeyPress == null || _osKeyPress.KeySequence.Count == 0);
        }
        
        public BIPLinkPZ69 BIPLink
        {
            get => _bipLinkPZ69;
            set
            {
                _bipLinkPZ69 = value;
                if (_bipLinkPZ69 != null)
                {
                    _textBox.Background = Brushes.Bisque;
                }
                else
                {
                    _textBox.Background = Brushes.White;
                }
            }
        }

        public OSKeyPress KeyPress
        {
            get => _osKeyPress;
            set
            {
                _osKeyPress = value;
                if (_osKeyPress != null)
                {
                    _textBox.Text = _osKeyPress.GetKeyPressInformation();
                }
                else
                {
                    _textBox.Text = "";
                }
            }
        }

        public RadioPanelPZ69KeyOnOff Knob => _knob;

        public void ClearAll()
        {
            _bipLinkPZ69 = null;
            _osKeyPress = null;
            _textBox.Background = Brushes.White;
            _textBox.Text = "";
        }
    }
}
