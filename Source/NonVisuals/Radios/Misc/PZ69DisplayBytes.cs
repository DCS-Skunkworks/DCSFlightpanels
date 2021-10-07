
namespace NonVisuals.Radios.Misc
{
    public class PZ69DisplayBytes
    {
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
