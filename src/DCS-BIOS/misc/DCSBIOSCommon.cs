using System;

namespace DCS_BIOS.misc
{
    public static class DCSBIOSCommon
    {

        public static string PrintBitStrings(byte[] array)
        {
            var result = string.Empty;
            foreach (var b in array)
            {
                var str = Convert.ToString(b, 2).PadLeft(8, '0');
                result = result + "  " + str;
            }
            return result;
        }
    }
}
