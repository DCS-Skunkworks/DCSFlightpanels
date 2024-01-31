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
            if (field == null)
            {
                return string.Empty;
            }

            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : string.Empty;
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
