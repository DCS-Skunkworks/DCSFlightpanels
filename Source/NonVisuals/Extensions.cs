namespace NonVisuals
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;

    using NonVisuals.StreamDeck;

    using TextBox = System.Windows.Controls.TextBox;

    public static class Extensions
    {

        public static void SetPanel(this List<StreamDeckLayer> list, StreamDeckPanel streamDeckPanel)
        {
            foreach (var streamDeckLayer in list)
            {
                streamDeckLayer.StreamDeckPanelInstance = streamDeckPanel;
            }
        }

        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static bool ValidateDouble(this TextBox textBox, bool ignoreIfEmpty)
        {
            if (ignoreIfEmpty && string.IsNullOrEmpty(textBox.Text))
            {
                return true;
            }
            
            if (!double.TryParse(textBox.Text, NumberStyles.Number, StreamDeckConstants.DoubleCultureInfo, out var result))
            {
                MessageBox.Show("Please enter valid number.", "Invalid number.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox.SelectAll();
                return false;
            }

            return true;
        }
    }
}
