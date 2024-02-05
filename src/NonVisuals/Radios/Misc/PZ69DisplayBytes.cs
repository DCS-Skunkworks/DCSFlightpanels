namespace NonVisuals.Radios.Misc
{
    using NLog;
    using System;
    using System.Globalization;

    /*
     * Used for displaying data on the PZ Radio Panel LCD display.
     *
     * LCD data sent to the PZ69 Radio Panel :
     * 1 byte (header byte 0x0)
     * 5 bytes upper left LCD
     * 5 bytes upper right LCD
     * 5 bytes lower left LCD
     * 5 bytes lower right LCD

     * 0x01 - 0x09 displays the figure 1-9
     * 0xD1 - 0xD9 displays the figure 1.-9. (figure followed by dot)
     * 0xFF -> blank, nothing is shown in that spot.
     *
     */
    public static class PZ69DisplayBytes
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Right justify, pad left with blanks.
        /// </summary>
        public static void UnsignedInteger(ref byte[] bytes, uint digits, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            var digitsAsString = digits.ToString().PadLeft(5);
           
            // D = DARK
            // 116 should become DD116!
            do
            {
                // 5 digits can be displayed
                // 12345 -> 12345
                // 116   -> DD116 
                // 1     -> DDDD1
                byte b;
                b = digitsAsString[i].ToString().Equals(" ") ? (byte)0xFF : byte.Parse(digitsAsString[i].ToString());
                bytes[arrayPosition] = b;

                arrayPosition++;
                i++;
            }
            while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        /// <summary>
        /// Expect a string of 5 chars that are going to be displayed as it.
        /// Can deal with multiple '.' chars.
        /// If size does not match 5, it will NOT replace previous characters in the array (no padding left or right).
        /// </summary>
        public static void DefaultStringAsIs(ref byte[] bytes, string digits, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;

            digits = digits.PadLeft(5); //Makes text right justified in the screen

            do
            {
                if (digits[i] == '.')
                {
                    // skip to next position, this has already been dealt with
                    i++;
                }

                if (digits[i] == ' ')
                {
                    bytes[arrayPosition] = 0xff;
                }
                else
                {
                    var b = byte.Parse(digits[i].ToString());
                    bytes[arrayPosition] = b;
                }

                if (digits.Length > i + 1 && digits[i + 1] == '.')
                {
                    // Add decimal marker
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                }

                arrayPosition++;
                i++;
            }
            while (i < digits.Length && arrayPosition < maxArrayPosition + 1);
        }
       
        public static void DoubleWithSpecifiedDecimalsPlaces(ref byte[] bytes, double digits, int decimals, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            var formatString = "0.".PadRight(decimals + 2, '0');

            var numberFormatInfoEmpty = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = string.Empty
            };

            var digitsAsString = digits.ToString(formatString, numberFormatInfoEmpty).PadLeft(6);

            do
            {
                if (digitsAsString[i] == '.')
                {
                    // skip to next position, this has already been dealt with
                    i++;
                }

                byte b;
                b = digitsAsString[i].ToString().Equals(" ") ? (byte)0xFF : byte.Parse(digitsAsString[i].ToString());
                bytes[arrayPosition] = b;
                if (digitsAsString.Length > i + 1 && digitsAsString[i + 1] == '.')
                {
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                }

                arrayPosition++;
                i++;
            }
            while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        public static void DoubleJustifyLeft(ref byte[] bytes, double digits, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;

            // Debug.WriteLine("LCD position is " + pz69LCDPosition);
            // Debug.WriteLine("Array position = " + arrayPosition);
            // Debug.WriteLine("Max array position = " + (maxArrayPosition));
            var i = 0;
            var numberFormatInfoFullDisplay = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = 4,
                NumberGroupSeparator = string.Empty
            };

            var digitsAsString = digits.ToString("0.0000", numberFormatInfoFullDisplay);
            // 116 should become 116.00!
            do
            {
                // 5 digits can be displayed
                // 1.00000011241 -> 1.0000
                // 116.0434      -> 116.04 
                // 1199330.12449 -> 11993
                if (digitsAsString[i] == '.')
                {
                    // skip to next position, this has already been dealt with
                    i++;
                }

                try
                {
                    var tmp = digitsAsString[i].ToString();
                    var b = byte.Parse(tmp);
                    bytes[arrayPosition] = b;
                    // Debug.WriteLine("Current string char is " + tmp + " from i = " + i + ", writing byte " + b + " to array position " + arrayPosition);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"SetPZ69DisplayBytesDefault() digitsAsString.Length = {digitsAsString.Length}");
                }

                if (digitsAsString.Length > i + 1 && digitsAsString[i + 1] == '.')
                {
                    // Add decimal marker
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                    // Debug.WriteLine("Writing decimal marker to array position " + arrayPosition);
                }

                arrayPosition++;
                i++;
            }
            while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        /// <summary>
        /// Sets the given position to blank without modifying the other positions in the array
        /// </summary>
        public static void SetPositionBlank(ref byte[] bytes, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;
            do
            {
                bytes[arrayPosition] = 0xFF;
                arrayPosition++;
                i++;
            }
            while (i < 5);
        }

        private static int GetArrayPosition(PZ69LCDPosition pz69LCDPosition)
        {
            return pz69LCDPosition switch
            {
                PZ69LCDPosition.UPPER_ACTIVE_LEFT => 1,
                PZ69LCDPosition.UPPER_STBY_RIGHT => 6,
                PZ69LCDPosition.LOWER_ACTIVE_LEFT => 11,
                PZ69LCDPosition.LOWER_STBY_RIGHT => 16,
                _ => 1
            };            
        }
    }
}
