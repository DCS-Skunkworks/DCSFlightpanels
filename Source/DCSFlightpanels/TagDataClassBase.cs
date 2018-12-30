using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    public abstract class TagDataClassBase
    {
        internal OSKeyPress _osKeyPress;
        internal TextBox _textBox;
        
        public abstract  bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        public abstract void Consume(List<DCSBIOSInput> dcsBiosInputs);
        public abstract void ClearAll();
        
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
            return _osKeyPress != null && !_osKeyPress.IsMultiSequenced() && _osKeyPress.KeySequence.Count > 0;
        }

        public OSKeyPress KeyPress
        {
            get => _osKeyPress;
            set
            {
                if (ContainsDCSBIOS())
                {
                    throw new Exception("Cannot insert KeyPress, TextBoxTagHolderClass already contains DCSBIOSInputs");
                }
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
        
        public SortedList<int, KeyPressInfo> GetKeySequence()
        {
            return _osKeyPress.KeySequence;
        }
    }

}
