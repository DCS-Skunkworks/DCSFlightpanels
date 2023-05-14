using System.Drawing;

namespace NonVisuals
{
    using System;
    using System.Globalization;
    using TextBox = System.Windows.Controls.TextBox;
    using Newtonsoft.Json;
    using Panels.StreamDeck;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp;
    using System.IO;

    public static class Extensions
    {
        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            //Note to devs: Use Newtonsoft.Json to clone the object, not System.Text.Json; because there are a lot of Newtonsoft.Json [JsonIgnore] attributes that are ignored
            //by the System.Text.Json serializer. This could lead to recursive serialization of unwanted properties that raises an exception.

            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException($"DeepClone error. The type must be serializable");
            }

            //Following line fixes "Could not create an instance of type xxx Type is an interface or abstract class and cannot be instantiated."
            //when deep cloning IKeyPressInfo from  AddKeySequence(string description, SortedList<int, IKeyPressInfo> keySequence)
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            var jsonString = JsonConvert.SerializeObject(source, settings);
            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }
        
        /// <summary>
        /// US Culture used. Decimal separator is '.'
        /// </summary>
        public static bool ValidateDouble(this TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                return true;
            }
            
            if (!double.TryParse(textBox.Text, NumberStyles.Number, StreamDeckConstants.DoubleCultureInfo, out _))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get MemoryStream from Bitmap
        /// </summary>
        public static MemoryStream GetMemoryStream(this Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Position = 0;
            return memoryStream;
        }


        /// <summary>
        /// Creates a <see cref="Image"/> from a given <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        public static Image ToImageSharpImage(this System.Drawing.Bitmap bitmap)
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return Image.Load(memoryStream);
        }
    }
}
