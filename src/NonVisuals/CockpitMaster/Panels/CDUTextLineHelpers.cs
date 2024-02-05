using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Panels
{
    public static class CDUTextLineHelpers
    {

        public static readonly Dictionary<char, CDUCharset> defaultConvertTable =
            new()
            {
                { ' ' , CDUCharset.Space },
                { '.' , CDUCharset.Dot },
                { '*' , CDUCharset.Check },
                { '0' , CDUCharset.Zero | (CDUCharset) Modifier.Big },
                { '1' , CDUCharset.One | (CDUCharset) Modifier.Big },
                { '2' , CDUCharset.Two |(CDUCharset) Modifier.Big  },
                { '3' , CDUCharset.Three |(CDUCharset) Modifier.Big  },
                { '4' , CDUCharset.Four |(CDUCharset) Modifier.Big  },
                { '5' , CDUCharset.Five |(CDUCharset) Modifier.Big  },
                { '6' , CDUCharset.Six |(CDUCharset) Modifier.Big  },
                { '7' , CDUCharset.Seven |(CDUCharset) Modifier.Big  },
                { '8' , CDUCharset.Eight |(CDUCharset) Modifier.Big  },
                { '9' , CDUCharset.Nine |(CDUCharset) Modifier.Big  },
                { 'a' , CDUCharset.A },
                { 'b' , CDUCharset.B },
                { 'c' , CDUCharset.C},
                { 'd' , CDUCharset.D },
                { 'e' , CDUCharset.E },
                { 'f' , CDUCharset.F },
                { 'g' , CDUCharset.G },
                { 'h' , CDUCharset.H },
                { 'i' , CDUCharset.I },
                { 'j' , CDUCharset.J },
                { 'k' , CDUCharset.K },
                { 'l' , CDUCharset.L },
                { 'm' , CDUCharset.M },
                { 'n' , CDUCharset.N },
                { 'o' , CDUCharset.O },
                { 'p' , CDUCharset.P },
                { 'q' , CDUCharset.Q },
                { 'r' , CDUCharset.R },
                { 's' , CDUCharset.S },
                { 't' , CDUCharset.T },
                { 'u' , CDUCharset.U },
                { 'v' , CDUCharset.V },
                { 'w' , CDUCharset.W },
                { 'x' , CDUCharset.X },
                { 'y' , CDUCharset.Y },
                { 'z' , CDUCharset.Z },
                { '(' , CDUCharset.OpenParent },
                { ')' , CDUCharset.CloseParent },
                { '<' , CDUCharset.Lower },
                { '>' , CDUCharset.Greater },
                { '-' , CDUCharset.Dash },
                { '/' , CDUCharset.Slash },
                //{ ',' , CDUCharset.comas },
                { ':' , CDUCharset.Colon },
                { '%' , CDUCharset.Percent },
                { '+' , CDUCharset.Plus },
                { ';' , CDUCharset.Semicolon },
                { '=' , CDUCharset.Colon },
                { 'A' , CDUCharset.A | (CDUCharset) Modifier.Big },
                { 'B' , CDUCharset.B | (CDUCharset) Modifier.Big},
                { 'C' , CDUCharset.C | (CDUCharset) Modifier.Big},
                { 'D' , CDUCharset.D | (CDUCharset) Modifier.Big },
                { 'E' , CDUCharset.E | (CDUCharset) Modifier.Big},
                { 'F' , CDUCharset.F | (CDUCharset) Modifier.Big },
                { 'G' , CDUCharset.G | (CDUCharset) Modifier.Big },
                { 'H' , CDUCharset.H | (CDUCharset) Modifier.Big },
                { 'I' , CDUCharset.I | (CDUCharset) Modifier.Big },
                { 'J' , CDUCharset.J | (CDUCharset) Modifier.Big},
                { 'K' , CDUCharset.K | (CDUCharset) Modifier.Big},
                { 'L' , CDUCharset.L | (CDUCharset) Modifier.Big},
                { 'M' , CDUCharset.M | (CDUCharset) Modifier.Big},
                { 'N' , CDUCharset.N | (CDUCharset) Modifier.Big},
                { 'O' , CDUCharset.O | (CDUCharset) Modifier.Big},
                { 'P' , CDUCharset.P | (CDUCharset) Modifier.Big},
                { 'Q' , CDUCharset.Q | (CDUCharset) Modifier.Big},
                { 'R' , CDUCharset.R | (CDUCharset) Modifier.Big},
                { 'S' , CDUCharset.S | (CDUCharset) Modifier.Big},
                { 'T' , CDUCharset.T | (CDUCharset) Modifier.Big},
                { 'U' , CDUCharset.U | (CDUCharset) Modifier.Big},
                { 'V' , CDUCharset.V | (CDUCharset) Modifier.Big},
                { 'W' , CDUCharset.W | (CDUCharset) Modifier.Big},
                { 'X' , CDUCharset.X | (CDUCharset) Modifier.Big},
                { 'Y' , CDUCharset.Y | (CDUCharset) Modifier.Big},
                { 'Z' , CDUCharset.Z | (CDUCharset) Modifier.Big},
                { '[' , CDUCharset.OpenParent | (CDUCharset) Modifier.Big },
                { ']' , CDUCharset.CloseParent | (CDUCharset) Modifier.Big },

                //
                { '?' , CDUCharset.Plus| (CDUCharset) Modifier.Inverted },

                // Special chars 
                { (char)187 , CDUCharset.RightArrow | (CDUCharset) Modifier.Big },
                { (char)171 , CDUCharset.LeftArrow | (CDUCharset) Modifier.Big },

                // up&down arrow in same char => most "same looking" : 
                { (char)174 , CDUCharset.Colon | (CDUCharset) Modifier.Big  },

                // 2 circles in front of a Line 
                { (char)169 , CDUCharset.O| (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted },
                { (char)176 , CDUCharset.Degree | (CDUCharset) Modifier.Big },

                // Selection Box [] in front of lines
                { (char)161 , CDUCharset.EmptySquare |(CDUCharset) Modifier.Big },

                // +/- in a char
                { (char)177 , CDUCharset.Plus},

                {'~' , CDUCharset.NeedInvertedBig01 | (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted },

                {(char) 182 , CDUCharset.NeedInvertedBig01 | (CDUCharset) Modifier.Inverted },

            };

        public static readonly Dictionary<char, CDUCharset> AH64ConvertTable =
            new()
            {
                { ' ' , CDUCharset.Space },
                { '.' , CDUCharset.Dot },
                { '*' , CDUCharset.Check },
                { '0' , CDUCharset.Zero | (CDUCharset) Modifier.Big },
                { '1' , CDUCharset.One | (CDUCharset) Modifier.Big },
                { '2' , CDUCharset.Two |(CDUCharset) Modifier.Big  },
                { '3' , CDUCharset.Three |(CDUCharset) Modifier.Big  },
                { '4' , CDUCharset.Four |(CDUCharset) Modifier.Big  },
                { '5' , CDUCharset.Five |(CDUCharset) Modifier.Big  },
                { '6' , CDUCharset.Six |(CDUCharset) Modifier.Big  },
                { '7' , CDUCharset.Seven |(CDUCharset) Modifier.Big  },
                { '8' , CDUCharset.Eight |(CDUCharset) Modifier.Big  },
                { '9' , CDUCharset.Nine |(CDUCharset) Modifier.Big  },
                { 'a' , CDUCharset.A },
                { 'b' , CDUCharset.B },
                { 'c' , CDUCharset.C},
                { 'd' , CDUCharset.D },
                { 'e' , CDUCharset.E },
                { 'f' , CDUCharset.F },
                { 'g' , CDUCharset.G },
                { 'h' , CDUCharset.H },
                { 'i' , CDUCharset.I },
                { 'j' , CDUCharset.J },
                { 'k' , CDUCharset.K },
                { 'l' , CDUCharset.L },
                { 'm' , CDUCharset.M },
                { 'n' , CDUCharset.N },
                { 'o' , CDUCharset.O },
                { 'p' , CDUCharset.P },
                { 'q' , CDUCharset.Q },
                { 'r' , CDUCharset.R },
                { 's' , CDUCharset.S },
                { 't' , CDUCharset.T },
                { 'u' , CDUCharset.U },
                { 'v' , CDUCharset.V },
                { 'w' , CDUCharset.W },
                { 'x' , CDUCharset.X },
                { 'y' , CDUCharset.Y },
                { 'z' , CDUCharset.Z },
                { '(' , CDUCharset.OpenParent },
                { ')' , CDUCharset.CloseParent },
                { '<' , CDUCharset.Lower },
                { '>' , CDUCharset.Greater },
                { '-' , CDUCharset.Dash },
                { '/' , CDUCharset.Slash },
                { ':' , CDUCharset.Colon },
                { '%' , CDUCharset.Percent },
                { '+' , CDUCharset.Plus },
                { ';' , CDUCharset.Semicolon },
                { '=' , CDUCharset.Degree },
                { 'A' , CDUCharset.A | (CDUCharset) Modifier.Big },
                { 'B' , CDUCharset.B | (CDUCharset) Modifier.Big},
                { 'C' , CDUCharset.C | (CDUCharset) Modifier.Big},
                { 'D' , CDUCharset.D | (CDUCharset) Modifier.Big },
                { 'E' , CDUCharset.E | (CDUCharset) Modifier.Big},
                { 'F' , CDUCharset.F | (CDUCharset) Modifier.Big },
                { 'G' , CDUCharset.G | (CDUCharset) Modifier.Big },
                { 'H' , CDUCharset.H | (CDUCharset) Modifier.Big },
                { 'I' , CDUCharset.I | (CDUCharset) Modifier.Big },
                { 'J' , CDUCharset.J | (CDUCharset) Modifier.Big},
                { 'K' , CDUCharset.K | (CDUCharset) Modifier.Big},
                { 'L' , CDUCharset.L | (CDUCharset) Modifier.Big},
                { 'M' , CDUCharset.M | (CDUCharset) Modifier.Big},
                { 'N' , CDUCharset.N | (CDUCharset) Modifier.Big},
                { 'O' , CDUCharset.O | (CDUCharset) Modifier.Big},
                { 'P' , CDUCharset.P | (CDUCharset) Modifier.Big},
                { 'Q' , CDUCharset.Q | (CDUCharset) Modifier.Big},
                { 'R' , CDUCharset.R | (CDUCharset) Modifier.Big},
                { 'S' , CDUCharset.S | (CDUCharset) Modifier.Big},
                { 'T' , CDUCharset.T | (CDUCharset) Modifier.Big},
                { 'U' , CDUCharset.U | (CDUCharset) Modifier.Big},
                { 'V' , CDUCharset.V | (CDUCharset) Modifier.Big},
                { 'W' , CDUCharset.W | (CDUCharset) Modifier.Big},
                { 'X' , CDUCharset.X | (CDUCharset) Modifier.Big},
                { 'Y' , CDUCharset.Y | (CDUCharset) Modifier.Big},
                { 'Z' , CDUCharset.Z | (CDUCharset) Modifier.Big},
                { '[' , CDUCharset.OpenParent | (CDUCharset) Modifier.Big },
                { ']' , CDUCharset.CloseParent | (CDUCharset) Modifier.Big },
                { '|' , CDUCharset.OpenParent | (CDUCharset) Modifier.Big },

                { '?' , CDUCharset.Plus| (CDUCharset) Modifier.Inverted },

                // Special chars 
                { (char)187 , CDUCharset.RightArrow | (CDUCharset) Modifier.Big },
                { (char)171 , CDUCharset.LeftArrow | (CDUCharset) Modifier.Big },
                
                // Blinking Cursor
                { (char)182 , CDUCharset.NeedInvertedBig01 | (CDUCharset) Modifier.Inverted },
                { '#' , CDUCharset.NeedInvertedBig01 | (CDUCharset) Modifier.Inverted },

                // up&down arrow in same char => most "same looking" : 
                { (char)174 , CDUCharset.Colon | (CDUCharset) Modifier.Big  },

                // 2 circles in front of a Line 
                { (char)169 , CDUCharset.O| (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted },
                { (char)176 , CDUCharset.Degree | (CDUCharset) Modifier.Big },

                // Selection Box [] in front of lines
                { (char)161 , CDUCharset.EmptySquare |(CDUCharset) Modifier.Big },

                // +/- in a char
                { (char)177 , CDUCharset.Plus},

                {'~' , CDUCharset.NeedInvertedBig02 | (CDUCharset) Modifier.Big | (CDUCharset) Modifier.Inverted }

            };


    }
}