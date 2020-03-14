using DCS_BIOS;

namespace NonVisuals.StreamDeck
{

    public class DCSBIOSFaceBindingStreamDeck
    {
        private int _currentValue = 0;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private string _layer = "";

        internal void ImportSettings(string settings){}

        public string Layer
        {
            get => _layer;
            set => _layer = value;
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
            return "";
        }
        
        public bool HasBinding => _dcsbiosOutput != null || _dcsbiosOutputFormula != null;

        public bool UseFormula => _dcsbiosOutputFormula != null;
    }
}
