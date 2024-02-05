using System;
using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Panels
{
    public class CDUTextLine
    {
        private const int MAX_CHAR = 24;

        private DisplayedChar[] _displayedChars = new DisplayedChar[MAX_CHAR];

        private readonly byte[] _encodedLine = new byte[36];
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

                if (value.Length > MAX_CHAR) throw new ArgumentException($"Length must be less than or equal to {MAX_CHAR}");

                _line = value;

                // Transcode all Known Chars.
                for (int i = 0; i < _line.Length; i++)
                {
                    CDUCharset converted = Convert(_line[i]);

                    // Defaults to line color, 
                    CDUColor _oldColor = _lineColor;

                    if (_displayedChars[i] != null)
                    {
                        // Keep any masks Applied 
                        _oldColor = _displayedChars[i].Color;
                    }
                    _displayedChars[i] = new DisplayedChar(converted, _oldColor);

                }

                // Add spaces for the rest of the Line. 
                for (int i = _line.Length; i < _displayedChars.Length; i++)
                {
                    _displayedChars[i] = new DisplayedChar(CDUCharset.Space);
                }
                Encode();

            }
        }

        private CDUCharset Convert(char ch)
        {
            if (!_convertTable.TryGetValue(ch, out var converted))
            {
                Console.WriteLine(ch); // to Debug Breakpoint un mapped chars
            }

            return converted;
        }

        public void SetLineWithDisplayedChar(DisplayedChar[] line)
        {
            // Add spaces for the rest of the Line. 
            _displayedChars = line;
            Encode();

        }

        public void SetDisplayedCharAt(DisplayedChar ch, int index)
        {
            if (index < 0 || index > MAX_CHAR - 1) throw new ArgumentException("CDUTextLine - Index out of Range 0 - 23");
            _displayedChars[index] = ch;
            Encode();
        }

        public byte[] GetEncodedBytes()
        {
            return _encodedLine;
        }

        // This sets the default for the Line 
      
        public CDUColor Color
        {
            get { return _lineColor; }
            set { 
                _lineColor = value;
                ApplyColorToLine(value);
            }
        }

        public void ApplyColorToLine(CDUColor color)
        {
            for (int i = 0; i < MAX_CHAR; i++)
            {
                _displayedChars[i].Color= color;
            }
            Encode();
        }

        public void ApplyMaskColor(CDUColor[] colorArray)
        {
            for (int i = 0; i < MAX_CHAR; i++)
            {
                _displayedChars[i].Color = colorArray[i];
            }
            Encode();
        }

        private void Encode()
        {
            int tempIndex = 0;

            for (int c = 0; c < MAX_CHAR; c += 2)
            {

                byte[] tempo = DisplayedChar.Encode2Chars(_displayedChars[c], _displayedChars[c + 1]);

                _encodedLine[tempIndex++] = tempo[0];
                _encodedLine[tempIndex++] = tempo[1];
                _encodedLine[tempIndex++] = tempo[2];

            }
        }
    }
}
