using System.Collections.Generic;
using System.Globalization;
using DCS_BIOS;

namespace NonVisuals.StreamDeck
{
    public class DCSBIOSDecoder : FaceTypeDCSBIOS, IDcsBiosDataListener
    {
        private string _formula = "";
        private bool _useFormula = false;
        private bool _decodeToString = false;
        private StreamDeckPanel _streamDeck;
        private DCSBIOSOutput _dcsbiosOutput = null;
        private List<DCSBIOSNumberToText> _dcsbiosNumberToTexts = new List<DCSBIOSNumberToText>();
        private bool _dataChanged = false;
        private readonly DCSBIOS _dcsbios;
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosPopupControls;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private const string DCSBIOS_PLACE_HOLDER = "{dcsbios}";

        public DCSBIOSDecoder(StreamDeckPanel streamDeck, EnumStreamDeckButtonNames streamDeckButton, DCSBIOS dcsbios)
        {
            _dcsbios = dcsbios;
            _dcsbios.AttachDataReceivedListener(this);
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosPopupControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            StreamDeckButtonName = streamDeckButton;
            _streamDeck = streamDeck;
        }

        ~DCSBIOSDecoder()
        {
            _dcsbios.DetachDataReceivedListener(this);
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (_dcsbiosOutput?.Address == e.Address)
            {
                if (!Equals(DCSBiosValue, e.Data))
                {
                    DCSBiosValue = e.Data;
                    _dataChanged = true;
                    if (_useFormula)
                    {
                        var formulaResult = EvaluateFormula();
                        if (_decodeToString)
                        {
                            var resultFound = false;
                            foreach (var dcsbiosNumberToText in _dcsbiosNumberToTexts)
                            {
                                ButtonText = dcsbiosNumberToText.ConvertNumber(DCSBiosValue, out resultFound);
                                if (resultFound)
                                {
                                    break;
                                }
                            }
                            //"Course {dcsbios}°
                            if (resultFound && ButtonText.Contains(DCSBIOS_PLACE_HOLDER))
                            {
                                ButtonText = ButtonText.Replace(DCSBIOS_PLACE_HOLDER,  formulaResult.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }
            }
        }

        private double EvaluateFormula()
        {
            var variables = new Dictionary<string, double>();
            variables.Add(_dcsbiosOutput.ControlId, 0);
            variables[_dcsbiosOutput.ControlId] = DCSBiosValue;
            return _jaceExtended.CalculationEngine.Calculate(_formula, variables);
        }

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

        public DCSBIOSOutput DCSBIOSOutput
        {
            get => _dcsbiosOutput;
            set => _dcsbiosOutput = value;
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

        public bool DecodeToString
        {
            get => _decodeToString;
            set => _decodeToString = value;
        }

        public bool UseFormula
        {
            get => _useFormula;
            set => _useFormula = value;
        }
    }
}
