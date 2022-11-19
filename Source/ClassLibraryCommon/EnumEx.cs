namespace ClassLibraryCommon
{
    using System;
    using System.ComponentModel;

    public static class EnumEx
    {
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException();
            }

            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(description))
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name.Equals(description))
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            throw new ArgumentException($"Could not find enum value {description} in enum {typeof(T)}", nameof(description));
        }
    }
}
