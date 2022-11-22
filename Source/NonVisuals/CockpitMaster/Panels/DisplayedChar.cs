namespace NonVisuals.CockpitMaster.Panels
{
    public enum CDUColor {

        WHITE= 0b000,
        CYAN= 0b001,
        GREEN = 0b010,
        PURPLE = 0b011,
        YELLOW = 0b100,
        RED = 0b101,
        BLUE = 0b110,
        DARK_GREEN =0b111

    }

    // This is the available charset
    // combined with Inverted or Big it makes the 255 values

    public enum Modifier
    {
        Big = 1<<6,
        Inverted = 1 << 7,
    }
    public enum CDUCharset
    {
        Space=0,
        Lower = 0x01,
        Greater = 0x02,
        Percent = 0x03,
        OpenParent = 0x04,
        CloseParent = 0x05,
        Dot = 0x06,
        Dash = 0x07,
        Slash = 0x08,
        Plus= 0x09,
        Colon = 0x0a,
        Semicolon = 0x0b,
        EmptySquare = 0x0c,
        UpArrow = 0x0d,
        DownArrow = 0xe,
        LeftArrow = 0x0f, 
        RightArrow = 0x10,
        Degree =0x11,
        DegCelsius =0x12,
        DegFarenheit = 0x13,
        Check = 0x14,
        NeedInvertedBig01 = 0x15,
        NeedInvertedBig02 = 0x16,
        NeedInvertedBig03 = 0x17,
        NeedInvertedBig04 = 0x18,
        NeedInvertedBig05 = 0x19,
        NeedInvertedBig06 = 0x1a,
        NeedInvertedBig07 = 0x1b,
        Zero = 0x1c,
        One = 0x1d,
        Two = 0x1e,
        Three = 0x1f,
        Four =0x20,
        Five = 0x21,
        Six =0x22, 
        Seven= 0x23,
        Eight = 0x24,
        Nine = 0x25,
        A = 0x26,
        B =0x27,
        C =0x28,
        D = 0x29,
        E =0x2a,
        F =0x2b,
        G = 0x2c,
        H =0x2d,
        I =0x2e,
        J =0x2f,
        K =0x30,
        L = 0x31,
        M = 0x32, 
        N= 0x33,
        O = 0x34,
        P = 0x35,
        Q = 0x36, 
        R = 0x37, 
        S = 0x38, 
        T = 0x39, 
        U = 0x3a,
        V = 0x3b,
        W = 0x3c,
        X = 0x3d,
        Y = 0x3e,
        Z = 0x3f,

    }

    public class DisplayedChar
    {

        private CDUCharset _value;

        public bool Inverted { get; set; }
        public bool Big { get; set; }
        
        public CDUColor Color { get; set; }

        public DisplayedChar(CDUCharset cduChar, 
            CDUColor color = CDUColor.GREEN,
            bool big =false, 
            bool inverted = false)
        {
            Big = big;
            Inverted = inverted;
            _value = cduChar;
            Color = color;

        }
   
        public byte GetCode()
        {
            byte _code = (byte) _value;
            if (Inverted) _code |= (byte) Modifier.Inverted;
            if (Big) _code |= (byte) Modifier.Big;
            return _code;

        }
                
        static public byte[] Encode2Chars(DisplayedChar char1, DisplayedChar char2)
        {
            // encode 2 consecutive chars 
            // respecting the HIDreport structure
            // Simply put two consecutive chars are encoded that way.
            // Assume they both are 0x47 0x47  (which is a dash '-')
            // red is 0x03 green is 0x02
            //  => 0x47 0x7(seven of the 0x47 second char)(col1=3) 0x(col2=2)4(four of the 47 2nd char)

            // method returns => 0x47 0x73 0x24 

            byte[] encoded = new byte[3];
            encoded[0]= char1.GetCode();
            encoded[1] = (byte)((char2.GetCode() << 4) | (byte) char1.Color);
            encoded[2] = (byte)((char2.GetCode() >> 4) | ((byte)char2.Color) << 4);
            return encoded;

        }

        
    }
}
