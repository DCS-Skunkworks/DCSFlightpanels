using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels.Bills
{
    public abstract class BillBaseOutput
    {
        private KeyPress _keyPress;
        private OSCommand _osCommand;
        private TextBox _textBox;

        public abstract bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        public abstract void Consume(DCSBIOSOutput dcsBiosOutput);
        public abstract void ClearAll();

        public TextBox TextBox
        {
            get => _textBox;
            set => _textBox = value;
        }
    }

}
