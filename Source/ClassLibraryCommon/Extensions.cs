namespace ClassLibraryCommon
{
    using System;
    using System.ComponentModel;
    using System.Text;

    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribs = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attribs.Length > 0)
            {
                return ((DescriptionAttribute)attribs[0]).Description;
            }
            return string.Empty;
        }


        public static void Populate(this byte[] array, int value)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (byte)value;
            }
        }

        public static String GetDebugString(this byte[] array)
        {
            var result = new StringBuilder();
            for (var i = 0; i < array.Length; i++)
            {
                result.Append("[" + i + "] = 0x" + array[i].ToString("x") + Environment.NewLine);
            }
            return result.ToString();
        }
    }
}
