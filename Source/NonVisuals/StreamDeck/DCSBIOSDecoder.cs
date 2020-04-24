using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using ClassLibraryCommon;
using DCS_BIOS;
using Newtonsoft.Json;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class DCSBIOSDecoder : FaceTypeDCSBIOS, IDcsBiosDataListener, IDCSBIOSStringListener
    {
        private string _formula = "";
        private DCSBIOSOutput _dcsbiosOutput = null;
        private List<DCSBIOSConverter> _dcsbiosConverters = new List<DCSBIOSConverter>();
        private volatile bool _valueUpdated;
        private string _lastFormulaError = "";
        private bool _useFormula = false;
        private double _formulaResult = 0;
        [NonSerialized] private int _jaceId = 0;
        private DCSBiosOutputType _dcsBiosOutputType = DCSBiosOutputType.INTEGER_TYPE;
        private bool _treatStringAsNumber = false;
        private EnumDCSBIOSDecoderOutputType _decoderOutputType = EnumDCSBIOSDecoderOutputType.Raw;



        public DCSBIOSDecoder()
        {
            DCSBIOS.GetInstance().AttachDataReceivedListener(this);
            
            _jaceId = RandomFactory.Get();
        }

        ~DCSBIOSDecoder()
        {
            DCSBIOSStringManager.Detach(this);
            DCSBIOS.GetInstance()?.DetachDataReceivedListener(this);
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                if (_dcsBiosOutputType == DCSBiosOutputType.STRING_TYPE)
                {
                    return;
                }

                if (_dcsbiosOutput?.Address == e.Address)
                {
                    if (!Equals(UintDcsBiosValue, e.Data))
                    {
                        UintDcsBiosValue = e.Data;
                        ButtonText = e.Data.ToString(CultureInfo.InvariantCulture);
                        HandleNewDCSBIOSValue();
                        _valueUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "DcsBiosDataReceived()");
            }
        }


        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if ((!_treatStringAsNumber && _dcsBiosOutputType == DCSBiosOutputType.INTEGER_TYPE) || string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (_dcsbiosOutput?.Address == e.Address)
                {
                    StringDcsBiosValue = e.StringData;
                    ButtonText = e.StringData;

                    if (_treatStringAsNumber && _dcsBiosOutputType == DCSBiosOutputType.STRING_TYPE && uint.TryParse(e.StringData.Substring(0, _dcsbiosOutput.MaxLength), out var tmpUint))
                    {
                        UintDcsBiosValue = tmpUint;
                        HandleNewDCSBIOSValue();
                    }
                    _valueUpdated = true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "DCSBIOSStringReceived()");
            }
        }

        /*
         * 1) integer
         * 2) string but treat as integer
         * 3) string and treat it as string (no formulas, no converters)
         */
        public void HandleNewDCSBIOSValue()
        {
            try
            {

                Bitmap converterBitmap = null;

                if (UseFormula)
                {
                    _formulaResult = EvaluateFormula();
                    ButtonText = _formulaResult.ToString(CultureInfo.InvariantCulture);
                }

                if (_dcsbiosConverters.Count > 0 && (_dcsBiosOutputType == DCSBiosOutputType.STRING_TYPE && _treatStringAsNumber || _dcsBiosOutputType == DCSBiosOutputType.INTEGER_TYPE))
                {
                    foreach (var dcsbiosConverter in _dcsbiosConverters)
                    {
                        dcsbiosConverter.Set(UseFormula ? FormulaResult : UintDcsBiosValue);
                        if (dcsbiosConverter.CriteriaFulfilled)
                        {
                            converterBitmap = dcsbiosConverter.Get();
                            break;
                        }
                    }
                }

                if (IsVisible)
                {
                    if (converterBitmap != null)
                    {
                        ShowBitmap(converterBitmap);
                    }
                    else
                    {
                        Show();
                    }
                }
                _lastFormulaError = "";
            }
            catch (Exception exception)
            {
                _lastFormulaError = exception.Message;
            }
        }

        /*private bool UseFormula
        {
            get => !string.IsNullOrEmpty(_formula) && (_dcsBiosOutputType == DCSBiosOutputType.INTEGER_TYPE || _dcsBiosOutputType == DCSBiosOutputType.STRING_TYPE && _treatStringAsNumber);
        }*/

        public bool UseFormula
        {
            get => _useFormula;
            set => _useFormula = value;
        }

        private void ShowBitmap(Bitmap bitmap)
        {
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, bitmap);
        }

        public void RemoveDCSBIOSOutput()
        {
            _dcsbiosOutput = null;
        }

        public void Clear()
        {
            _formula = "";
            _dcsbiosOutput = null;
            _dcsbiosConverters.Clear();
            _valueUpdated = false;
            _lastFormulaError = "";
            _formulaResult = 0;
        }

        private double EvaluateFormula()
        {
            //360 - floor((HSI_HDG / 65535) * 360)
            var variables = new Dictionary<string, double>();
            variables.Add(_dcsbiosOutput.ControlId, 0);
            variables[_dcsbiosOutput.ControlId] = UintDcsBiosValue;
            return JaceExtendedFactory.Instance(ref _jaceId).CalculationEngine.Calculate(_formula, variables);
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
                UintDcsBiosValue = UInt32.MaxValue;
            }
        }
        
        public void Add(DCSBIOSConverter dcsbiosConverter)
        {
            _dcsbiosConverters.Add(dcsbiosConverter);
        }

        public void Replace(DCSBIOSConverter oldDcsBiosValueToFaceConverter, DCSBIOSConverter newDcsBiosValueToFaceConverter)
        {
            Remove(oldDcsBiosValueToFaceConverter);
            Add(newDcsBiosValueToFaceConverter);
        }

        public void Remove(DCSBIOSConverter dcsbiosConverter)
        {
            _dcsbiosConverters.Remove(dcsbiosConverter);
        }

        public List<DCSBIOSConverter> DCSBIOSConverters
        {
            get => _dcsbiosConverters;
            set => _dcsbiosConverters = value;
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

        public string GetFriendlyInfo()
        {
            return _dcsbiosOutput.ControlId;
        }

        public bool TreatStringAsNumber
        {
            get => _treatStringAsNumber;
            set => _treatStringAsNumber = value;
        }

        public Font RawTextFont
        {
            get => TextFont;
            set => TextFont = value;
        }

        public Color RawFontColor
        {
            get => FontColor;
            set => FontColor = value;
        }

        public Color RawBackgroundColor
        {
            get => BackgroundColor;
            set => BackgroundColor = value;
        }

        public DCSBiosOutputType DCSBiosOutputType
        {
            get => _dcsBiosOutputType;
            set => _dcsBiosOutputType = value;
        }

        public EnumDCSBIOSDecoderOutputType DecoderOutputType
        {
            get => _decoderOutputType;
            set => _decoderOutputType = value;
        }


        /*
         * It can have integer | string + treat as number | string input
         * It can have raw / converter output
         */
        public bool ConfigurationIsOK()
        {
            var convertersOK = _dcsbiosConverters.FindAll(o => o.FaceConfigurationIsOK == false).Count == 0;
            var outputIsOK = _decoderOutputType == EnumDCSBIOSDecoderOutputType.Raw ? base.ConfigurationOK : convertersOK;
            var formulaIsOK = _useFormula ? !string.IsNullOrEmpty(_formula) : true;
            var sourceIsOK = _dcsbiosOutput != null;

            return convertersOK && outputIsOK && formulaIsOK && sourceIsOK;
        }


    }

    public enum EnumDCSBIOSDecoderOutputType
    {
        Raw,
        Converter
    }
}
