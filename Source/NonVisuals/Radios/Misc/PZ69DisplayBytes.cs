
using System;

namespace NonVisuals.Radios.Misc
{
    public class PZ69DisplayBytes
    {
        /// <summary>
        /// Right justify, pad left with blanks.
        /// </summary>
        public void UnsignedInteger(ref byte[] bytes, uint digits, PZ69LCDPosition pz69LCDPosition)
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
        /// Right justify, pad left with blanks.
        /// </summary>
        public void Integer(ref byte[] bytes, int digits, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 5;
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
        /// Inject the preformatted 5 bytes at position in the array.
        /// </summary>
        public void Custom5Bytes(ref byte[] bytes, byte[] bytesToBeInjected, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;
            do
            {
                // 5 digits can be displayed
                // 12345 -> 12345
                // 116   -> DD116 
                // 1     -> DDDD1
                bytes[arrayPosition] = bytesToBeInjected[i];

                arrayPosition++;
                i++;
            }
            while (i < bytesToBeInjected.Length && i < 5);
        }

        /// <summary>
        /// Expect a string of 5 chars that are going to be dispaleyd as it.
        /// Can deal with multiple '.' chars.
        /// If size does not match 5, it will NOT replace previous characters in the array (no padding left or right).
        /// </summary>
        public void DefaultStringAsIt(ref byte[] bytes, string digits, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
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

        /// <summary>
        /// Expect a string of max 5 chars that are going to be dispaleyd as it.
        /// If size does not match 5, justify the value right and pad left with blanks.
        /// </summary>
        public void BytesStringAsItOrPadLeftBlanks(ref byte[] bytes, string digitString, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            var digits = string.Empty;
            if (digitString.Length > 5)
            {
                if (digitString.Contains("."))
                {
                    digits = digitString.Substring(0, 6);
                }
                else
                {
                    digits = digitString.Substring(0, 5);
                }
            }
            else if (digitString.Length < 5)
            {
                if (digitString.Contains("."))
                {
                    digits = digitString.PadLeft(6, ' ');
                }
                else
                {
                    digits = digitString.PadLeft(5, ' ');
                }
            }
            else if (digitString.Length == 5)
            {
                if (digitString.Contains("."))
                {
                    digits = digitString.PadLeft(1, ' ');
                }
                else
                {
                    digits = digitString;
                }
            }

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

        /// <summary>
        /// Sets the given position to blank without modifying the other positions in the array
        /// </summary>
        public void SetPositionBlank(ref byte[] bytes, PZ69LCDPosition pz69LCDPosition)
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

        private int GetArrayPosition(PZ69LCDPosition pz69LCDPosition)
        {
            switch (pz69LCDPosition)
            {
                case PZ69LCDPosition.UPPER_ACTIVE_LEFT:
                    {
                        return 1;
                    }

                case PZ69LCDPosition.UPPER_STBY_RIGHT:
                    {
                        return 6;
                    }

                case PZ69LCDPosition.LOWER_ACTIVE_LEFT:
                    {
                        return 11;
                    }

                case PZ69LCDPosition.LOWER_STBY_RIGHT:
                    {
                        return 16;
                    }
            }

            return 1;
        }
    }
}
