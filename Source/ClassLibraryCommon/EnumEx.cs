namespace ClassLibraryCommon
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

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
        
        public static String ToPaddedHexString(int i, int padLength = 4)
        {
            return "0x" + i.ToString("x").PadLeft(padLength, '0');
        }

        public static String ToPaddedHexString(uint i, int padLength = 4)
        {
            return "0x" + i.ToString("x").PadLeft(padLength, '0');
        }

        private static int SetBitToZeroAtPosition(int value, int position)
        {
            return value & ~(1 << position);
        }
    }
}
