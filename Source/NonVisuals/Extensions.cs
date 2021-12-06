using Newtonsoft.Json;
using NonVisuals.StreamDeck.Panels;

namespace NonVisuals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.Json;
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

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException($"DeepClone error. The type must be serializable");
            }

            string jsonString = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        public static bool ValidateDouble(this TextBox textBox, bool ignoreIfEmpty)
        {
            if (ignoreIfEmpty && string.IsNullOrEmpty(textBox.Text))
            {
                return true;
            }
            
            if (!double.TryParse(textBox.Text, NumberStyles.Number, StreamDeckConstants.DoubleCultureInfo, out _))
            {
                MessageBox.Show("Please enter valid number.", "Invalid number.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox.SelectAll();
                return false;
            }

            return true;
        }
    }
}
