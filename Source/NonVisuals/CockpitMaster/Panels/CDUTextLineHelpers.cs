using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Panels
{
    public static class CDUTextLineHelpers
    {

        public static readonly Dictionary<char, CDUCharset> defaultConvertTable =
            new Dictionary<char, CDUCharset>()
            {
                { ' ' , CDUCharset.space },
                { '.' , CDUCharset.dot },
                { '*' , CDUCharset.check },
                { '0' , CDUCharset.zero | (CDUCharset) Modifiers.Big },
                { '1' , CDUCharset.one | (CDUCharset) Modifiers.Big },
                { '2' , CDUCharset.two |(CDUCharset) Modifiers.Big  },
                { '3' , CDUCharset.three |(CDUCharset) Modifiers.Big  },
                { '4' , CDUCharset.four |(CDUCharset) Modifiers.Big  },
                { '5' , CDUCharset.five |(CDUCharset) Modifiers.Big  },
                { '6' , CDUCharset.six |(CDUCharset) Modifiers.Big  },
                { '7' , CDUCharset.seven |(CDUCharset) Modifiers.Big  },
                { '8' , CDUCharset.eight |(CDUCharset) Modifiers.Big  },
                { '9' , CDUCharset.nine |(CDUCharset) Modifiers.Big  },
                { 'a' , CDUCharset.a },
                { 'b' , CDUCharset.b },
                { 'c' , CDUCharset.c},
                { 'd' , CDUCharset.d },
                { 'e' , CDUCharset.e },
                { 'f' , CDUCharset.f },
                { 'g' , CDUCharset.g },
                { 'h' , CDUCharset.h },
                { 'i' , CDUCharset.i },
                { 'j' , CDUCharset.j },
                { 'k' , CDUCharset.k },
                { 'l' , CDUCharset.l },
                { 'm' , CDUCharset.m },
                { 'n' , CDUCharset.n },
                { 'o' , CDUCharset.o },
                { 'p' , CDUCharset.p },
                { 'q' , CDUCharset.q },
                { 'r' , CDUCharset.r },
                { 's' , CDUCharset.s },
                { 't' , CDUCharset.t },
                { 'u' , CDUCharset.u },
                { 'v' , CDUCharset.v },
                { 'w' , CDUCharset.w },
                { 'x' , CDUCharset.x },
                { 'y' , CDUCharset.y },
                { 'z' , CDUCharset.z },
                { '(' , CDUCharset.openparent },
                { ')' , CDUCharset.closeparent },
                { '<' , CDUCharset.lower },
                { '>' , CDUCharset.greater },
                { '-' , CDUCharset.dash },
                { '/' , CDUCharset.slash },
                { ',' , CDUCharset.comas },
                { ':' , CDUCharset.colon },
                { '%' , CDUCharset.percent },
                { '+' , CDUCharset.plus },
                { ';' , CDUCharset.semicolon },
                { '=' , CDUCharset.colon },
                { 'A' , CDUCharset.a | (CDUCharset) Modifiers.Big },
                { 'B' , CDUCharset.b | (CDUCharset) Modifiers.Big},
                { 'C' , CDUCharset.c | (CDUCharset) Modifiers.Big},
                { 'D' , CDUCharset.d | (CDUCharset) Modifiers.Big },
                { 'E' , CDUCharset.e | (CDUCharset) Modifiers.Big},
                { 'F' , CDUCharset.f | (CDUCharset) Modifiers.Big },
                { 'G' , CDUCharset.g | (CDUCharset) Modifiers.Big },
                { 'H' , CDUCharset.h | (CDUCharset) Modifiers.Big },
                { 'I' , CDUCharset.i | (CDUCharset) Modifiers.Big },
                { 'J' , CDUCharset.j | (CDUCharset) Modifiers.Big},
                { 'K' , CDUCharset.k | (CDUCharset) Modifiers.Big},
                { 'L' , CDUCharset.l | (CDUCharset) Modifiers.Big},
                { 'M' , CDUCharset.m | (CDUCharset) Modifiers.Big},
                { 'N' , CDUCharset.n | (CDUCharset) Modifiers.Big},
                { 'O' , CDUCharset.o | (CDUCharset) Modifiers.Big},
                { 'P' , CDUCharset.p | (CDUCharset) Modifiers.Big},
                { 'Q' , CDUCharset.q | (CDUCharset) Modifiers.Big},
                { 'R' , CDUCharset.r | (CDUCharset) Modifiers.Big},
                { 'S' , CDUCharset.s | (CDUCharset) Modifiers.Big},
                { 'T' , CDUCharset.t | (CDUCharset) Modifiers.Big},
                { 'U' , CDUCharset.u | (CDUCharset) Modifiers.Big},
                { 'V' , CDUCharset.v | (CDUCharset) Modifiers.Big},
                { 'W' , CDUCharset.w | (CDUCharset) Modifiers.Big},
                { 'X' , CDUCharset.x | (CDUCharset) Modifiers.Big},
                { 'Y' , CDUCharset.y | (CDUCharset) Modifiers.Big},
                { 'Z' , CDUCharset.z | (CDUCharset) Modifiers.Big},
                { '[' , CDUCharset.openparent | (CDUCharset) Modifiers.Big },
                { ']' , CDUCharset.closeparent | (CDUCharset) Modifiers.Big },

                //
                { '?' , CDUCharset.plus| (CDUCharset) Modifiers.Inverted },

                // Special chars 
                { (char)187 , CDUCharset.rightarrow | (CDUCharset) Modifiers.Big },
                { (char)171 , CDUCharset.leftarrow | (CDUCharset) Modifiers.Big },

                // up&down arrow in same char => most "same looking" : 
                { (char)174 , CDUCharset.colon | (CDUCharset) Modifiers.Big  },

                // 2 circles in front of a Line 
                { (char)169 , CDUCharset.o| (CDUCharset) Modifiers.Big | (CDUCharset) Modifiers.Inverted },
                { (char)176 , CDUCharset.degre | (CDUCharset) Modifiers.Big },

                // Selection Box [] in front of lines
                { (char)161 , CDUCharset.emptysquare |(CDUCharset) Modifiers.Big },

                // +/- in a char
                { (char)177 , CDUCharset.plus},

                {'~' , CDUCharset.needInvertedBig01 | (CDUCharset) Modifiers.Big | (CDUCharset) Modifiers.Inverted }

            };

        public static readonly Dictionary<char, CDUCharset> AH64ConvertTable =
            new Dictionary<char, CDUCharset>()
            {
                { ' ' , CDUCharset.space },
                { '.' , CDUCharset.dot },
                { '*' , CDUCharset.check },
                { '0' , CDUCharset.zero | (CDUCharset) Modifiers.Big },
                { '1' , CDUCharset.one | (CDUCharset) Modifiers.Big },
                { '2' , CDUCharset.two |(CDUCharset) Modifiers.Big  },
                { '3' , CDUCharset.three |(CDUCharset) Modifiers.Big  },
                { '4' , CDUCharset.four |(CDUCharset) Modifiers.Big  },
                { '5' , CDUCharset.five |(CDUCharset) Modifiers.Big  },
                { '6' , CDUCharset.six |(CDUCharset) Modifiers.Big  },
                { '7' , CDUCharset.seven |(CDUCharset) Modifiers.Big  },
                { '8' , CDUCharset.eight |(CDUCharset) Modifiers.Big  },
                { '9' , CDUCharset.nine |(CDUCharset) Modifiers.Big  },
                { 'a' , CDUCharset.a },
                { 'b' , CDUCharset.b },
                { 'c' , CDUCharset.c},
                { 'd' , CDUCharset.d },
                { 'e' , CDUCharset.e },
                { 'f' , CDUCharset.f },
                { 'g' , CDUCharset.g },
                { 'h' , CDUCharset.h },
                { 'i' , CDUCharset.i },
                { 'j' , CDUCharset.j },
                { 'k' , CDUCharset.k },
                { 'l' , CDUCharset.l },
                { 'm' , CDUCharset.m },
                { 'n' , CDUCharset.n },
                { 'o' , CDUCharset.o },
                { 'p' , CDUCharset.p },
                { 'q' , CDUCharset.q },
                { 'r' , CDUCharset.r },
                { 's' , CDUCharset.s },
                { 't' , CDUCharset.t },
                { 'u' , CDUCharset.u },
                { 'v' , CDUCharset.v },
                { 'w' , CDUCharset.w },
                { 'x' , CDUCharset.x },
                { 'y' , CDUCharset.y },
                { 'z' , CDUCharset.z },
                { '(' , CDUCharset.openparent },
                { ')' , CDUCharset.closeparent },
                { '<' , CDUCharset.lower },
                { '>' , CDUCharset.greater },
                { '-' , CDUCharset.dash },
                { '/' , CDUCharset.slash },
                { ',' , CDUCharset.comas },
                { ':' , CDUCharset.colon },
                { '%' , CDUCharset.percent },
                { '+' , CDUCharset.plus },
                { ';' , CDUCharset.semicolon },
                { '=' , CDUCharset.degre },
                { 'A' , CDUCharset.a | (CDUCharset) Modifiers.Big },
                { 'B' , CDUCharset.b | (CDUCharset) Modifiers.Big},
                { 'C' , CDUCharset.c | (CDUCharset) Modifiers.Big},
                { 'D' , CDUCharset.d | (CDUCharset) Modifiers.Big },
                { 'E' , CDUCharset.e | (CDUCharset) Modifiers.Big},
                { 'F' , CDUCharset.f | (CDUCharset) Modifiers.Big },
                { 'G' , CDUCharset.g | (CDUCharset) Modifiers.Big },
                { 'H' , CDUCharset.h | (CDUCharset) Modifiers.Big },
                { 'I' , CDUCharset.i | (CDUCharset) Modifiers.Big },
                { 'J' , CDUCharset.j | (CDUCharset) Modifiers.Big},
                { 'K' , CDUCharset.k | (CDUCharset) Modifiers.Big},
                { 'L' , CDUCharset.l | (CDUCharset) Modifiers.Big},
                { 'M' , CDUCharset.m | (CDUCharset) Modifiers.Big},
                { 'N' , CDUCharset.n | (CDUCharset) Modifiers.Big},
                { 'O' , CDUCharset.o | (CDUCharset) Modifiers.Big},
                { 'P' , CDUCharset.p | (CDUCharset) Modifiers.Big},
                { 'Q' , CDUCharset.q | (CDUCharset) Modifiers.Big},
                { 'R' , CDUCharset.r | (CDUCharset) Modifiers.Big},
                { 'S' , CDUCharset.s | (CDUCharset) Modifiers.Big},
                { 'T' , CDUCharset.t | (CDUCharset) Modifiers.Big},
                { 'U' , CDUCharset.u | (CDUCharset) Modifiers.Big},
                { 'V' , CDUCharset.v | (CDUCharset) Modifiers.Big},
                { 'W' , CDUCharset.w | (CDUCharset) Modifiers.Big},
                { 'X' , CDUCharset.x | (CDUCharset) Modifiers.Big},
                { 'Y' , CDUCharset.y | (CDUCharset) Modifiers.Big},
                { 'Z' , CDUCharset.z | (CDUCharset) Modifiers.Big},
                { '[' , CDUCharset.openparent | (CDUCharset) Modifiers.Big },
                { ']' , CDUCharset.closeparent | (CDUCharset) Modifiers.Big },
                { '|' , CDUCharset.openparent | (CDUCharset) Modifiers.Big },

                //
                { '?' , CDUCharset.plus| (CDUCharset) Modifiers.Inverted },

                // Special chars 
                { (char)187 , CDUCharset.rightarrow | (CDUCharset) Modifiers.Big },
                { (char)171 , CDUCharset.leftarrow | (CDUCharset) Modifiers.Big },
                
                // Blinking Cursor
                { (char)182 , CDUCharset.needInvertedBig01 | (CDUCharset) Modifiers.Inverted },
                { '#' , CDUCharset.needInvertedBig01 | (CDUCharset) Modifiers.Inverted },

                // up&down arrow in same char => most "same looking" : 
                { (char)174 , CDUCharset.colon | (CDUCharset) Modifiers.Big  },

                // 2 circles in front of a Line 
                { (char)169 , CDUCharset.o| (CDUCharset) Modifiers.Big | (CDUCharset) Modifiers.Inverted },
                { (char)176 , CDUCharset.degre | (CDUCharset) Modifiers.Big },

                // Selection Box [] in front of lines
                { (char)161 , CDUCharset.emptysquare |(CDUCharset) Modifiers.Big },

                // +/- in a char
                { (char)177 , CDUCharset.plus},

                {'~' , CDUCharset.needInvertedBig02 | (CDUCharset) Modifiers.Big | (CDUCharset) Modifiers.Inverted }

            };


    }
}