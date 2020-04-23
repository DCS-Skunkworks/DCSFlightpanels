using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;

namespace ClassLibraryCommon
{
    public static class Extensions
    {

        public static bool ValidateDouble(this TextBox textBox, bool ignoreIfEmpty)
        {
            if (ignoreIfEmpty && string.IsNullOrEmpty(textBox.Text))
            {
                return true;
            }

            if (!double.TryParse(textBox.Text, out var result))
            {
                MessageBox.Show("Please enter valid number.", "Invalid number.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox.SelectAll();
                return false;
            }

            return true;
        }

    }
}
