using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels.TagDataClasses
{
    public abstract class TagDataClassBase
    {
        private KeyPress _osKeyPress;
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

        public KeyPress KeyPress
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

        public OSCommand OSCommandObject
        {
            get => _osCommand;
            set
            {
                _osCommand = value;
                _textBox.Text = _osCommand.Name;
            }
        }

        public KeyPress OSKeyPress
        {
            get => _osKeyPress;
            set => _osKeyPress = value;
        }

        public TextBox TextBox
        {
            get => _textBox;
            set => _textBox = value;
        }
    }

}
