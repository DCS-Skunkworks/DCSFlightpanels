namespace ClassLibraryCommon
{
    using System;
    using System.ComponentModel;

    public static class Extensions
    {
        public static string GetEnumDescriptionField(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribs = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attribs.Length > 0)
            {
                return ((DescriptionAttribute)attribs[0]).Description;
            }
            return string.Empty;
        }
    }
}
