using System;
using System.Collections.Generic;
using System.Globalization;
using DCS_BIOS;
using Newtonsoft.Json;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class DCSBIOSDecoder : FaceTypeDCSBIOS, IDcsBiosDataListener
    {
        private string _formula = "";
        private IStreamDeckBoard _streamDeckBoard;
        private DCSBIOSOutput _dcsbiosOutput = null;
        private List<DCSBIOSNumberToText> _dcsbiosNumberToTexts = new List<DCSBIOSNumberToText>();
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private volatile bool _valueUpdated;
        private string _lastFormulaError = "";
        private double _formulaResult = 0;
        private bool _isVisible = false;


        public DCSBIOSDecoder()
        {
            DCSBIOS.GetInstance().AttachDataReceivedListener(this);
        }

        ~DCSBIOSDecoder()
        {
            DCSBIOS.GetInstance()?.DetachDataReceivedListener(this);
        }

        public void SetEssentials(string streamDeckInstance, EnumStreamDeckButtonNames streamDeckButton)
        {
            StreamDeckButtonName = streamDeckButton;
            StreamDeckInstanceId = streamDeckInstance;
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (_dcsbiosOutput?.Address == e.Address)
            {
                if (!Equals(DCSBiosValue, e.Data))
                {
                    DCSBiosValue = e.Data;
                    ButtonText = e.Data.ToString(CultureInfo.InvariantCulture);
                    try
                    {
                        if (!string.IsNullOrEmpty(_formula))
                        {
                            _formulaResult = EvaluateFormula();
                            ButtonText = _formulaResult.ToString(CultureInfo.InvariantCulture); //In case string converter not used
                        }
                        if (_dcsbiosNumberToTexts.Count > 0)
                        {
                            foreach (var dcsbiosNumberToText in _dcsbiosNumberToTexts)
                            {
                                var tmp = dcsbiosNumberToText.ConvertNumber((string.IsNullOrEmpty(_formula) == false ? _formulaResult : DCSBiosValue), out var resultFound);
                                if (resultFound)
                                {
                                    ButtonText = tmp;
                                    break;
                                }
                            }
                        }
                        if (_isVisible)
                        {
                            Show();
                        }
                        _lastFormulaError = "";
                    }
                    catch (Exception exception)
                    {
                        _lastFormulaError = exception.Message;
                    }
                    _valueUpdated = true;
                }
            }
        }
        
        public void RemoveDCSBIOSOutput()
        {
            _dcsbiosOutput = null;
        }

        public void Clear()
        {
            _formula = "";
            _dcsbiosOutput = null;
            _dcsbiosNumberToTexts.Clear();
            _valueUpdated = false;
            _lastFormulaError = "";
            _formulaResult = 0;
        }
        
        private double EvaluateFormula()
        {
            //360 - floor((HSI_HDG / 65535) * 360)
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
        
        public DCSBIOSOutput DCSBIOSOutput
        {
            get => _dcsbiosOutput;
            set
            {
                _valueUpdated = true;
                _dcsbiosOutput = value;
                DCSBiosValue = UInt32.MaxValue;
            }
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

        [JsonIgnore]
        public bool ValueUpdated
        {
            get
            {
                var result = false;
                if (_valueUpdated)
                {
                    result = true;
                    _valueUpdated = false; // Reset so next read without update will give false
                }

                return result;
            }
        }

        [JsonIgnore]
        public bool HasErrors => !string.IsNullOrEmpty(_lastFormulaError);

        [JsonIgnore]
        public string LastFormulaError => _lastFormulaError;

        [JsonIgnore]
        public double FormulaResult => _formulaResult;

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        
        public string GetFriendlyInfo()
        {
            return _dcsbiosOutput.ControlId;
        }
    }
}
