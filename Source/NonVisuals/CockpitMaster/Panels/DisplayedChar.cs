using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Panels
{
    public enum CDUColors {

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

    public enum Modifiers
    {
        Big = (1<<6),
        Inverted = (1 << 7),
    }
    public enum CDUCharset
    {
        space=0,
        lower = 0x01,
        greater = 0x02,
        percent = 0x03,
        openparent = 0x04,
        closeparent = 0x05,
        dot = 0x06,
        dash = 0x07,
        slash = 0x08,
        plus= 0x09,
        colon = 0x0a,
        semicolon = 0x0b,
        emptysquare = 0x0c,
        comas = 0x0c,
        uparrow = 0x0d,
        downarrow = 0xe,
        leftarrow = 0x0f, 
        rightarrow = 0x10,
        degre =0x11,
        degcelcius =0x12,
        degfar = 0x13,
        check = 0x14,
        needInvertedBig01 = 0x15,
        needInvertedBig02 = 0x16,
        needInvertedBig03 = 0x17,
        needInvertedBig04 = 0x18,
        needInvertedBig05 = 0x19,
        needInvertedBig06 = 0x1a,
        needInvertedBig07 = 0x1b,
        zero = 0x1c,
        one = 0x1d,
        two = 0x1e,
        three = 0x1f,
        four =0x20,
        five = 0x21,
        six =0x22, 
        seven= 0x23,
        eight = 0x24,
        nine = 0x25,
        a = 0x26,
        b =0x27,
        c =0x28,
        d= 0x29,
        e =0x2a,
        f =0x2b,
        g = 0x2c,
        h =0x2d,
        i =0x2e,
        j =0x2f,
        k =0x30,
        l = 0x31,
        m = 0x32, 
        n= 0x33,
        o = 0x34,
        p = 0x35,
        q = 0x36, 
        r = 0x37, 
        s = 0x38, 
        t = 0x39, 
        u = 0x3a,
        v = 0x3b,
        w = 0x3c,
        x = 0x3d,
        y = 0x3e,
        z = 0x3f,

    }


    public class DisplayedChar
    {

        private CDUCharset _value;

        public bool Inverted { get; set; }
        public bool Big { get; set; }
        
        public CDUColors Color { get; set; }

        public DisplayedChar(CDUCharset cduChar, 
            CDUColors color = CDUColors.GREEN,
            bool big =false, 
            bool inverted = false)
        {
            Big = big;
            Inverted = inverted;
            _value = cduChar;
            Color = color;

        }
    
        public byte getCode()
        {
            byte _code = (byte) _value;
            if (Inverted) _code |= (byte) Modifiers.Inverted;
            if (Big) _code |= (byte) Modifiers.Big;
            return _code;

        }

        
        static public byte[] encode2chars(DisplayedChar char1, DisplayedChar char2)
        {
            // encode 2 consécutive chars 
            // respecting the HIDreport structure
            // Simply put two consecutive chars are encoded that way.
            // Assume they both are 0x47 0x47 - which is a dash '-'
            // red is 0x3 green is 0x2
            //  =>  0x47 0x7(seven of the 47 second char)(col1=3) 0x(col2=2)4(four of the 47 2nd char)
            // method returns => 0x47 0x73 0x24 

            // 0x0c 0x0c  => 0x0c 0 0x0c

            byte[] encoded = new byte[3];
            encoded[0]= char1.getCode();
            encoded[1] = (byte)((char2.getCode() << 4) | (byte) char1.Color);
            encoded[2] = (byte)((char2.getCode() >> 4) | ((byte)char2.Color) << 4);
            return encoded;

        }

        
    }
}
