using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using DCS_BIOS;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class DCSBIOSDecoder : FaceTypeDCSBIOS
    {
        private string _formula = "";
        private StreamDeckPanel _streamDeck;
        private DCSBIOSOutput _dcsbiosOutput1 = null;
        private List<DCSBIOSNumberToText> _dcsbiosNumberToTexts = new List<DCSBIOSNumberToText>();

        public string Formula
        {
            get => _formula;
            set => _formula = value;
        }
        
        public StreamDeckPanel StreamDeck
        {
            get => _streamDeck;
            set => _streamDeck = value;
        }

        public DCSBIOSOutput DCSBIOSOutput1
        {
            get => _dcsbiosOutput1;
            set => _dcsbiosOutput1 = value;
        }

        public void Add(DCSBIOSNumberToText dcsbiosNumberToText)
        {
            _dcsbiosNumberToTexts.Add(dcsbiosNumberToText);
        }

        public void Replace(DCSBIOSNumberToText oldDCSBIOSNumberToText, DCSBIOSNumberToText newDCSBIOSNumberToText)
        {
            Remove(oldDCSBIOSNumberToText);
            Add(newDCSBIOSNumberToText);
        }

        public void Remove(DCSBIOSNumberToText dcsbiosNumberToText)
        {
            _dcsbiosNumberToTexts.Remove(dcsbiosNumberToText);
        }

        public List<DCSBIOSNumberToText> DCSBIOSDecoders
        {
            get => _dcsbiosNumberToTexts;
            set => _dcsbiosNumberToTexts = value;
        }
    }
}
