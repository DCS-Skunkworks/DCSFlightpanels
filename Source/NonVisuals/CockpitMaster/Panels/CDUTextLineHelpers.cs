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
                { '0' , CDUCharset.zero | (CDUCharset) Modifier.Big },
                { '1' , CDUCharset.one | (CDUCharset) Modifier.Big },
                { '2' , CDUCharset.two |(CDUCharset) Modifier.Big  },
                { '3' , CDUCharset.three |(CDUCharset) Modifier.Big  },
                { '4' , CDUCharset.four |(CDUCharset) Modifier.Big  },
                { '5' , CDUCharset.five |(CDUCharset) Modifier.Big  },
                { '6' , CDUCharset.six |(CDUCharset) Modifier.Big  },
                { '7' , CDUCharset.seven |(CDUCharset) Modifier.Big  },
                { '8' , CDUCharset.eight |(CDUCharset) Modifier.Big  },
                { '9' , CDUCharset.nine |(CDUCharset) Modifier.Big  },
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
                //{ ',' , CDUCharset.comas },
                { ':' , CDUCharset.colon },
                { '%' , CDUCharset.percent },
                { '+' , CDUCharset.plus },
                { ';' , CDUCharset.semicolon },
                { '=' , CDUCharset.colon },
                { 'A' , CDUCharset.a | (CDUCharset) Modifier.Big },
                { 'B' , CDUCharset.b | (CDUCharset) Modifier.Big},
                { 'C' , CDUCharset.c | (CDUCharset) Modifier.Big},
                { 'D' , CDUCharset.d | (CDUCharset) Modifier.Big },
                { 'E' , CDUCharset.e | (CDUCharset) Modifier.Big},
                { 'F' , CDUCharset.f | (CDUCharset) Modifier.Big },
                { 'G' , CDUCharset.g | (CDUCharset) Modifier.Big },
                { 'H' , CDUCharset.h | (CDUCharset) Modifier.Big },
                { 'I' , CDUCharset.i | (CDUCharset) Modifier.Big },
                { 'J' , CDUCharset.j | (CDUCharset) Modifier.Big},
                { 'K' , CDUCharset.k | (CDUCharset) Modifier.Big},
                { 'L' , CDUCharset.l | (CDUCharset) Modifier.Big},
                { 'M' , CDUCharset.m | (CDUCharset) Modifier.Big},
                { 'N' , CDUCharset.n | (CDUCharset) Modifier.Big},
                { 'O' , CDUCharset.o | (CDUCharset) Modifier.Big},
                { 'P' , CDUCharset.p | (CDUCharset) Modifier.Big},
                { 'Q' , CDUCharset.q | (CDUCharset) Modifier.Big},
                { 'R' , CDUCharset.r | (CDUCharset) Modifier.Big},
                { 'S' , CDUCharset.s | (CDUCharset) Modifier.Big},
                { 'T' , CDUCharset.t | (CDUCharset) Modifier.Big},
                { 'U' , CDUCharset.u | (CDUCharset) Modifier.Big},
                { 'V' , CDUCharset.v | (CDUCharset) Modifier.Big},
                { 'W' , CDUCharset.w | (CDUCharset) Modifier.Big},
                { 'X' , CDUCharset.x | (CDUCharset) Modifier.Big},
                { 'Y' , CDUCharset.y | (CDUCharset) Modifier.Big},
                { 'Z' , CDUCharset.z | (CDUCharset) Modifier.Big},
                { '[' , CDUCharset.openparent | (CDUCharset) Modifier.Big },
                { ']' , CDUCharset.closeparent | (CDUCharset) Modifier.Big },

                //
                { '?' , CDUCharset.plus| (CDUCharset) Modifier.Inverted },

                // Special chars 
                { (char)187 , CDUCharset.rightarrow | (CDUCharset) Modifier.Big },
                { (char)171 , CDUCharset.leftarrow | (CDUCharset) Modifier.Big },

                // up&down arrow in same char => most "same looking" : 
                { (char)174 , CDUCharset.colon | (CDUCharset) Modifier.Big  },

                // 2 circles in front of a Line 
                { (char)169 , CDUCharset.o| (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted },
                { (char)176 , CDUCharset.degre | (CDUCharset) Modifier.Big },

                // Selection Box [] in front of lines
                { (char)161 , CDUCharset.emptysquare |(CDUCharset) Modifier.Big },

                // +/- in a char
                { (char)177 , CDUCharset.plus},

                {'~' , CDUCharset.needInvertedBig01 | (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted },

                {(char) 182 , CDUCharset.needInvertedBig01 | (CDUCharset) Modifier.Inverted },

            };

        public static readonly Dictionary<char, CDUCharset> AH64ConvertTable =
            new Dictionary<char, CDUCharset>()
            {
                { ' ' , CDUCharset.space },
                { '.' , CDUCharset.dot },
                { '*' , CDUCharset.check },
                { '0' , CDUCharset.zero | (CDUCharset) Modifier.Big },
                { '1' , CDUCharset.one | (CDUCharset) Modifier.Big },
                { '2' , CDUCharset.two |(CDUCharset) Modifier.Big  },
                { '3' , CDUCharset.three |(CDUCharset) Modifier.Big  },
                { '4' , CDUCharset.four |(CDUCharset) Modifier.Big  },
                { '5' , CDUCharset.five |(CDUCharset) Modifier.Big  },
                { '6' , CDUCharset.six |(CDUCharset) Modifier.Big  },
                { '7' , CDUCharset.seven |(CDUCharset) Modifier.Big  },
                { '8' , CDUCharset.eight |(CDUCharset) Modifier.Big  },
                { '9' , CDUCharset.nine |(CDUCharset) Modifier.Big  },
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
                { ':' , CDUCharset.colon },
                { '%' , CDUCharset.percent },
                { '+' , CDUCharset.plus },
                { ';' , CDUCharset.semicolon },
                { '=' , CDUCharset.degre },
                { 'A' , CDUCharset.a | (CDUCharset) Modifier.Big },
                { 'B' , CDUCharset.b | (CDUCharset) Modifier.Big},
                { 'C' , CDUCharset.c | (CDUCharset) Modifier.Big},
                { 'D' , CDUCharset.d | (CDUCharset) Modifier.Big },
                { 'E' , CDUCharset.e | (CDUCharset) Modifier.Big},
                { 'F' , CDUCharset.f | (CDUCharset) Modifier.Big },
                { 'G' , CDUCharset.g | (CDUCharset) Modifier.Big },
                { 'H' , CDUCharset.h | (CDUCharset) Modifier.Big },
                { 'I' , CDUCharset.i | (CDUCharset) Modifier.Big },
                { 'J' , CDUCharset.j | (CDUCharset) Modifier.Big},
                { 'K' , CDUCharset.k | (CDUCharset) Modifier.Big},
                { 'L' , CDUCharset.l | (CDUCharset) Modifier.Big},
                { 'M' , CDUCharset.m | (CDUCharset) Modifier.Big},
                { 'N' , CDUCharset.n | (CDUCharset) Modifier.Big},
                { 'O' , CDUCharset.o | (CDUCharset) Modifier.Big},
                { 'P' , CDUCharset.p | (CDUCharset) Modifier.Big},
                { 'Q' , CDUCharset.q | (CDUCharset) Modifier.Big},
                { 'R' , CDUCharset.r | (CDUCharset) Modifier.Big},
                { 'S' , CDUCharset.s | (CDUCharset) Modifier.Big},
                { 'T' , CDUCharset.t | (CDUCharset) Modifier.Big},
                { 'U' , CDUCharset.u | (CDUCharset) Modifier.Big},
                { 'V' , CDUCharset.v | (CDUCharset) Modifier.Big},
                { 'W' , CDUCharset.w | (CDUCharset) Modifier.Big},
                { 'X' , CDUCharset.x | (CDUCharset) Modifier.Big},
                { 'Y' , CDUCharset.y | (CDUCharset) Modifier.Big},
                { 'Z' , CDUCharset.z | (CDUCharset) Modifier.Big},
                { '[' , CDUCharset.openparent | (CDUCharset) Modifier.Big },
                { ']' , CDUCharset.closeparent | (CDUCharset) Modifier.Big },
                { '|' , CDUCharset.openparent | (CDUCharset) Modifier.Big },

                { '?' , CDUCharset.plus| (CDUCharset) Modifier.Inverted },

                // Special chars 
                { (char)187 , CDUCharset.rightarrow | (CDUCharset) Modifier.Big },
                { (char)171 , CDUCharset.leftarrow | (CDUCharset) Modifier.Big },
                
                // Blinking Cursor
                { (char)182 , CDUCharset.needInvertedBig01 | (CDUCharset) Modifier.Inverted },
                { '#' , CDUCharset.needInvertedBig01 | (CDUCharset) Modifier.Inverted },

                // up&down arrow in same char => most "same looking" : 
                { (char)174 , CDUCharset.colon | (CDUCharset) Modifier.Big  },

                // 2 circles in front of a Line 
                { (char)169 , CDUCharset.o| (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted },
                { (char)176 , CDUCharset.degre | (CDUCharset) Modifier.Big },

                // Selection Box [] in front of lines
                { (char)161 , CDUCharset.emptysquare |(CDUCharset) Modifier.Big },

                // +/- in a char
                { (char)177 , CDUCharset.plus},

                {'~' , CDUCharset.needInvertedBig02 | (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted }

            };


    }
}