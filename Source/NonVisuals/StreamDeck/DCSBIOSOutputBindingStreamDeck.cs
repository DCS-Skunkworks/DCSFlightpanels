using System;
using DCS_BIOS;
using NonVisuals.StreamDeck;

namespace NonVisuals.StreamDeck
{

    public class DCSBIOSOutputBindingStreamDeck
    {
        private int _currentValue = 0;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private const string SEPARATOR_CHARS = "\\o/";
        private string _layer = "";

        internal void ImportSettings(string settings)
        {
            /*
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("StreamDeckDCSBIOSOutput{") && settings.Contains("DCSBiosOutput{"))
            {
                //StreamDeckDCSBIOSOutput{Home Layer|Button1}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                var parameters = settings.Split(new[] { SEPARATOR_CHARS }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //StreamDeckDCSBIOSOutput{Home Layer|Button1}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSOutput{", "").Replace("}", "");
                //Home Layer|Button1
                var param0Split = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                Layer = param0Split[0];
                StreamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), param0Split[1]);

                //[1]
                //DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                _dcsbiosOutput = new DCSBIOSOutput();
                _dcsbiosOutput.ImportString(parameters[1]);
            }
            if (settings.StartsWith("StreamDeckDCSBIOSOutput{") && settings.Contains("DCSBiosOutputFormula{"))
            {
                //StreamDeckDCSBIOSOutput{Home Layer|Button1}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                var parameters = settings.Split(new[] { SEPARATOR_CHARS }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //StreamDeckDCSBIOSOutput{Home Layer|Button1}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSOutput{", "").Replace("}", "");
                //Home Layer|Button1
                var param0Split = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                Layer = param0Split[0];
                StreamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), param0Split[1]);

                //[1]
                //DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                _dcsbiosOutputFormula.ImportString(parameters[1]);
            }
            */
        }

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
            /*
            if (DCSBIOSOutputObject == null && DCSBIOSOutputFormulaObject == null)
            {
                return null;
            }
            if (_dcsbiosOutputFormula != null)
            {
                //StreamDeckDCSBIOSOutput{Home Layer|ALT}\o/{Button11Left}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}
                return "StreamDeckDCSBIOSOutput{" + Layer + "|" + Enum.GetName(typeof(StreamDeckButtonNames), _streamDeckButtonName) + "}" + SEPARATOR_CHARS + _dcsbiosOutputFormula.ToString();
            }
            return "StreamDeckDCSBIOSOutput{" + Layer + "|" + Enum.GetName(typeof(StreamDeckButtonNames), _streamDeckButtonName) + "}" + SEPARATOR_CHARS + _dcsbiosOutput.ToString();
            */
            return "";
        }
        
        public bool HasBinding => _dcsbiosOutput != null || _dcsbiosOutputFormula != null;

        public bool UseFormula => _dcsbiosOutputFormula != null;
    }
}
