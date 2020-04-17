using System.Windows.Controls;

namespace DCSFlightpanels.Bills
{
    public abstract class BillBaseOutput
    {
        private TextBox _textBox;

        public abstract bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        public abstract void Clear();

        public TextBox TextBox
        {
            get => _textBox;
            set => _textBox = value;
        }
    }

}
