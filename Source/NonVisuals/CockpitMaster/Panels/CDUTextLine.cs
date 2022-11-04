using System;
using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Panels
{
    public class CDUTextLine
    {
        const int MAX_CHAR = 24;

        private DisplayedChar[] displayedChars = new DisplayedChar[MAX_CHAR];

        private readonly byte[] encodedLine = new byte[36];
        private string _line;
        private CDUColor _lineColor;

        private Dictionary<char, CDUCharset> _convertTable = CDUTextLineHelpers.defaultConvertTable;
        public CDUTextLine(string line = "", CDUColor color = CDUColor.GREEN)
        {
            Line = line;
            Color = color;
        }

        public CDUTextLine(string line, Dictionary<char, CDUCharset> convertTable)
        {
            Line = line;
            _convertTable = convertTable;
        }

        public Dictionary<char, CDUCharset> ConvertTable {
            get
            {
                return _convertTable;
            }
            set
            {
                _convertTable = value;

            }
        }

        public string Line
        {
            get { return _line; }
            set
            {

                if (value.Length > MAX_CHAR) throw new ArgumentException();

                _line = value;

                // Transcode all Known Chars.
                for (int i = 0; i < _line.Length; i++)
                {
                    CDUCharset converted = convert(_line[i]);

                    // Defaults to line color, 
                    CDUColor _oldColor = _lineColor;

                    if (displayedChars[i] != null)
                    {
                        // Keep any masks Applied 
                        _oldColor = displayedChars[i].Color;
                    }
                    displayedChars[i] = new DisplayedChar(converted, _oldColor);

                }

                // Add spaces for the rest of the Line. 
                for (int i = _line.Length; i < displayedChars.Length; i++)
                {
                    displayedChars[i] = new DisplayedChar(CDUCharset.space);
                }
                encode();

            }
        }

        private CDUCharset convert(char ch)
        {
            CDUCharset converted;
            if (!_convertTable.TryGetValue(ch, out converted))
            {
                Console.WriteLine(ch); // to Debug Breakpoint un mapped chars
            }

            return converted;
        }

        public void setLineWithDisplayedChar(DisplayedChar[] line)
        {
            // Add spaces for the rest of the Line. 
            displayedChars = line;
            encode();

        }

        public void setDisplayedCharAt(DisplayedChar ch, int index)
        {
            if (index < 0 || index > MAX_CHAR - 1) throw new ArgumentException("CDUTextLine - Index out of Range 0 - 23");
            displayedChars[index] = ch;
            encode();
        }

        public byte[] getEncodedBytes()
        {
            return encodedLine;
        }

        // This sets the default for the Line 
      
        public CDUColor Color
        {
            get { return _lineColor; }
            set { 
                _lineColor = value;
                applyColorToLine(value);
            }
        }

        public void applyColorToLine(CDUColor color)
        {
            for (int i = 0; i < MAX_CHAR; i++)
            {
                displayedChars[i].Color= color;
            }
            encode();
        }

        public void applyMaskColor(CDUColor[] colorArray)
        {
            for (int i = 0; i < MAX_CHAR; i++)
            {
                displayedChars[i].Color = colorArray[i];
            }
            encode();
        }

        private void encode()
        {
            int tempIndex = 0;

            for (int c = 0; c < MAX_CHAR; c += 2)
            {

                byte[] tempo = DisplayedChar.encode2chars(displayedChars[c], displayedChars[c + 1]);

                encodedLine[tempIndex++] = tempo[0];
                encodedLine[tempIndex++] = tempo[1];
                encodedLine[tempIndex++] = tempo[2];

            }
        }
    }
}
