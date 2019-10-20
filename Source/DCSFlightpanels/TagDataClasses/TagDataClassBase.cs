using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels.TagDataClasses
{
    public abstract class TagDataClassBase
    {
        private KeyPress _keyPress;
        private OSCommand _osCommand;
        private TextBox _textBox;

        public abstract bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        public abstract void Consume(List<DCSBIOSInput> dcsBiosInputs);
        public abstract void ClearAll();


        public bool ContainsOSCommand()
        {
            return _osCommand != null;
        }

        public bool ContainsKeyPress()
        {
            return _keyPress != null && _keyPress.KeySequence.Count > 0;
        }

        public bool ContainsKeySequence()
        {
            return _keyPress != null && _keyPress.IsMultiSequenced();
        }

        public bool ContainsSingleKey()
        {
            return _keyPress != null && !_keyPress.IsMultiSequenced() && _keyPress.KeySequence.Count > 0;
        }

        public KeyPress KeyPress
        {
            get => _keyPress;
            set
            {
                if (ContainsDCSBIOS())
                {
                    throw new Exception("Cannot insert KeyPress, TextBoxTagHolderClass already contains DCSBIOSInputs");
                }
                _keyPress = value;
                if (_keyPress != null)
                {
                    _textBox.Text = _keyPress.GetKeyPressInformation();
                }
                else
                {
                    _textBox.Text = "";
                }
            }
        }

        public SortedList<int, KeyPressInfo> GetKeySequence()
        {
            return _keyPress.KeySequence;
        }

        public OSCommand OSCommandObject
        {
            get => _osCommand;
            set
            {
                _osCommand = value;
                _textBox.Text = _osCommand.Name;
            }
        }

        public TextBox TextBox
        {
            get => _textBox;
            set => _textBox = value;
        }
    }

}
