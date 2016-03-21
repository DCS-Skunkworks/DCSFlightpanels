using System;
using DCS_BIOS;

namespace NonVisuals
{
    public enum PZ70LCDPosition
    {
        UpperALT,
        LowerALT,
        UpperVS,
        LowerVS
    }

    public class DCSBIOSBindingLCDPZ70
    {
        /*
         * This class binds a LCD row on the PZ70 with a DCSBIOSOutput
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDPZ70, all data will be shown
         */
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private const string SeparatorChars = "\\o/";
        private PZ70LCDPosition _pz70LCDPosition;

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutput{"))
            {
                //MultiPanelDCSBIOSControlLCD{KNOB_ALT|UpperALT}\o/DCSBiosOutput{AAP_EGIPWR|Equals|0}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelDCSBIOSControlLCD{KNOB_ALT|UpperALT}
                var param0 = parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1);
                //KNOB_ALT|UpperALT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //KNOB_ALT|UpperALT
                var knobAndLcd = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), knobAndLcd[0]);
                _pz70LCDPosition = (PZ70LCDPosition)Enum.Parse(typeof(PZ70LCDPosition), knobAndLcd[1]);

                //DCSBiosOutput{AAP_EGIPWR|Equals|0}
                _dcsbiosOutput = new DCSBIOSOutput();
                _dcsbiosOutput.ImportString(parameters[1]);
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutputFormula{"))
            {
                //MultiPanelDCSBIOSFormulaLCD{KNOB_ALT|UpperALT}\o/DCSBiosOutputFormula{(AAP_EGIPWR+1)/2}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelDCSBIOSFormulaLCD{KNOB_ALT|UpperALT}
                var param0 = parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1);
                //KNOB_ALT|UpperALT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //KNOB_ALT|UpperALT
                var knobAndLcd = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), knobAndLcd[0]);
                _pz70LCDPosition = (PZ70LCDPosition)Enum.Parse(typeof(PZ70LCDPosition), knobAndLcd[1]);

                //DCSBiosOutputFormula{(AAP_EGIPWR+1)/2}
                _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                _dcsbiosOutputFormula.ImportString(parameters[1]);
            }
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get { return _multiPanelPZ70Knob; }
            set { _multiPanelPZ70Knob = value; }
        }

        public DCSBIOSOutput DCSBIOSOutputObject
        {
            get { return _dcsbiosOutput; }
            set
            {
                _dcsbiosOutput = value;
                _dcsbiosOutputFormula = null;
            }
        }

        public DCSBIOSOutputFormula DCSBIOSOutputFormulaObject
        {
            get { return _dcsbiosOutputFormula; }
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
                return "MultiPanelDCSBIOSControlLCD{" + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "|" + _pz70LCDPosition + "}" + SeparatorChars + _dcsbiosOutputFormula.ToString();
            }
            return "MultiPanelDCSBIOSControlLCD{" + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "|" + _pz70LCDPosition + "}" + SeparatorChars + _dcsbiosOutput.ToString();
        }

        public PZ70LCDPosition PZ70LCDPosition
        {
            get { return _pz70LCDPosition; }
            set { _pz70LCDPosition = value; }
        }

        public bool HasBinding
        {
            get { return _dcsbiosOutput != null || _dcsbiosOutputFormula != null; }
        }

        public bool UseFormula
        {
            get { return _dcsbiosOutputFormula != null; }
        }
    }
}
