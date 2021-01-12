using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ClassLibraryCommon
{
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


    public static class EnumEx
    {
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description.Equals(description))
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name.Equals(description))
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }


        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
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
        /*
        public static void Populate(this byte[] array, byte value)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }
        */
    }
}
