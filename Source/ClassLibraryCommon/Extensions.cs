namespace ClassLibraryCommon
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

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

        public static Key RealKey(this KeyEventArgs e)
        {
            return e.Key switch
            {
                Key.System => e.SystemKey,
                Key.ImeProcessed => e.ImeProcessedKey,
                Key.DeadCharProcessed => e.DeadCharProcessedKey,
                _ => e.Key
            };
        }
    }
}
