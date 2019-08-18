using System;
using DCS_BIOS;

namespace NonVisuals
{

    public class DCSBIOSBindingLCDStreamDeck
    {
        /*
         * This class binds a DCSBIOSOutput with an LCD (key) on a Stream Deck
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDStreamDeck, all data will be shown
         */
        private StreamDeck35Buttons _streamDeck35Button;
        private int _currentValue = 0;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("StreamDeckDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutput{"))
            {
                //StreamDeckDCSBIOSControlLCD{Button11}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //StreamDeckDCSBIOSControlLCD{Button11}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSControlLCD{", "").Replace("}", "");
                //Button11
                _streamDeck35Button = (StreamDeck35Buttons)Enum.Parse(typeof(StreamDeck35Buttons), param0);

                //[1]
                //DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                _dcsbiosOutput = new DCSBIOSOutput();
                _dcsbiosOutput.ImportString(parameters[1]);
            }
            if (settings.StartsWith("StreamDeckDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutputFormula{"))
            {
                //StreamDeckDCSBIOSControlLCD{Button11}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //StreamDeckDCSBIOSControlLCD{Button11}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSControlLCD{", "").Replace("}", "");
                //Button11
                _streamDeck35Button = (StreamDeck35Buttons)Enum.Parse(typeof(StreamDeck35Buttons), param0);

                //[1]
                //DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                _dcsbiosOutputFormula.ImportString(parameters[1]);
            }
        }

        public StreamDeck35Buttons StreamDeck35Button
        {
            get => _streamDeck35Button;
            set => _streamDeck35Button = value;
        }
        
        public int CurrentValue
        {
            get => _currentValue;
            set => _currentValue = value;
        }

        public DCSBIOSOutput DCSBIOSOutputObject
        {
            get => _dcsbiosOutput;
            set
            {
                _dcsbiosOutput = value;
                _dcsbiosOutputFormula = null;
            }
        }

        public DCSBIOSOutputFormula DCSBIOSOutputFormulaObject
        {
            get => _dcsbiosOutputFormula;
            set
            {
                _dcsbiosOutputFormula = value;
                _dcsbiosOutput = null;
            }
        }


        public string ExportSettings()
        {
            if (DCSBIOSOutputObject == null && DCSBIOSOutputFormulaObject == null)
            {
                return null;
            }
            if (_dcsbiosOutputFormula != null)
            {
                //StreamDeckDCSBIOSControlLCD{ALT}\o/{Button11Left}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}
                return "StreamDeckDCSBIOSControlLCD{" + Enum.GetName(typeof(StreamDeck35Buttons), _streamDeck35Button) + "}" + SeparatorChars + _dcsbiosOutputFormula.ToString();
            }
            return "StreamDeckDCSBIOSControlLCD{" + Enum.GetName(typeof(StreamDeck35Buttons), _streamDeck35Button) + "}" + SeparatorChars + _dcsbiosOutput.ToString();
        }
        
        public bool HasBinding => _dcsbiosOutput != null || _dcsbiosOutputFormula != null;

        public bool UseFormula => _dcsbiosOutputFormula != null;
    }
}
