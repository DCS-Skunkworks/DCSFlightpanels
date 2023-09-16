namespace DCSFlightpanels.Bills
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;

    using DCS_BIOS;

    using MEF;

    using NonVisuals;
    using NonVisuals.KeyEmulation;


    /// <summary>
    /// BillBaseInputStreamDeck is used by descendants and added to a TextBox.
    /// </summary>
    public abstract class BillBaseInputStreamDeck
    {
        private KeyPress _keyPress;
        private OSCommand _operatingSystemCommand;

        public abstract bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        public abstract void Consume(List<DCSBIOSInput> dcsBiosInputs);
        public abstract void Clear();
        protected TextBox TextBox { get; init; }

        public OSCommand OSCommandObject
        {
            get => _operatingSystemCommand;
            set
            {
                _operatingSystemCommand = value;
                TextBox.Text = _operatingSystemCommand != null ? _operatingSystemCommand.Name : string.Empty;
            }
        }

        public KeyPress KeyPress
        {
            get => _keyPress;
            set
            {
                if (value != null && ContainsDCSBIOS())
                {
                    throw new Exception("Cannot insert KeyPress, Bill already contains DCSBIOSInputs");
                }
                _keyPress = value;
                TextBox.Text = _keyPress != null ? _keyPress.GetKeyPressInformation() : string.Empty;
            }
        }

        public bool ContainsOSCommand()
        {
            return _operatingSystemCommand != null;
        }

        public bool ContainsKeyPress()
        {
            return _keyPress != null && _keyPress.KeyPressSequence.Count > 0;
        }

        public bool ContainsKeySequence()
        {
            return _keyPress != null && _keyPress.IsMultiSequenced();
        }

        public bool ContainsKeyStroke()
        {
            return _keyPress != null && !_keyPress.IsMultiSequenced() && _keyPress.KeyPressSequence.Count > 0;
        }

        public SortedList<int, IKeyPressInfo> GetKeySequence()
        {
            return _keyPress.KeyPressSequence;
        }
    }
}

