using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class DCSBIOSConverter : FaceTypeText
    {
        private EnumComparator _comparator1 = EnumComparator.None;
        private EnumComparator _comparator2 = EnumComparator.None;
        private double _referenceValue1 = 0;
        private double _referenceValue2 = 0;
        private double _dcsbiosValue = 0;
        private EnumConverterOutputType _outputType = EnumConverterOutputType.NotSet;
        private int _jaceId = 0;
        private bool _criteria1IsOk = false;
        private bool _criteria2IsOk = false;

        private FaceTypeText _faceTypeText = new FaceTypeText();
        private FaceTypeImage _faceTypeImage = new FaceTypeImage();
        private FaceTypeDCSBIOSOverlay _faceTypeDCSBIOSOverlay = new FaceTypeDCSBIOSOverlay();






        public void Set(double dcsbiosValue)
        {
            _dcsbiosValue = dcsbiosValue;
        }

        public Bitmap Get()
        {
            if (!_criteria1IsOk || (_comparator2 != EnumComparator.None && !_criteria2IsOk))
            {
                throw new Exception("Cannot call get when criteria(s) not fulfilled. DCSBIOSValueToFaceConverter()");
            }

            Bitmap result = null;

            switch (_outputType)
            {
                case EnumConverterOutputType.NotSet:
                    {
                        throw new Exception("Cannot call get when OutputType == NotSet. DCSBIOSValueToFaceConverter()");
                    }
                case EnumConverterOutputType.OutputImage:
                    {
                        result = _faceTypeImage.Bitmap;
                        break;
                    }
                case EnumConverterOutputType.OutputImageOverlay:
                    {
                        result = _faceTypeDCSBIOSOverlay.Bitmap;
                        break;
                    }
                case EnumConverterOutputType.OutputRaw:
                    {
                        result = _faceTypeText.Bitmap;
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown OutputType in Get(). DCSBIOSValueToFaceConverter()");
                    }
            }

            return result;
        }

        public bool CriteriaFulfilled
        {
            get
            {
                if (_comparator1 == EnumComparator.Always)
                {
                    _criteria1IsOk = true;
                    return true;
                }


                var jaceExtended = JaceExtendedFactory.Instance(ref _jaceId);

                var formula = StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + GetComparatorAsString(_comparator1) + " reference";
                var variables = new Dictionary<string, double>();
                variables.Add(StreamDeckConstants.DCSBIOSValuePlaceHolder, _referenceValue1);
                variables.Add("reference", _dcsbiosValue);
                var result1 = jaceExtended.CalculationEngine.Calculate(formula, variables);
                _criteria1IsOk = Math.Abs(result1 - 1.0) < 0.0001;

                if (_comparator2 == EnumComparator.None)
                {
                    return _criteria1IsOk;
                }

                formula = StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + GetComparatorAsString(_comparator2) + " reference";
                variables = new Dictionary<string, double>();
                variables.Add(StreamDeckConstants.DCSBIOSValuePlaceHolder, _referenceValue2);
                variables.Add("reference", _dcsbiosValue);
                var result2 = jaceExtended.CalculationEngine.Calculate(formula, variables);
                _criteria2IsOk = Math.Abs(result2 - 1.0) < 0.0001;

                return _criteria1IsOk && _criteria2IsOk;
            }
        }

        public EnumConverterOutputType OutputType
        {
            get => _outputType;
            set => _outputType = value;
        }

        [JsonIgnore]
        public string FriendlyInfo
        {
            get
            {
                var stringBuilder = new StringBuilder();
                /*
                 * always output text
                 * if {dcsbios} == ref1 then output image
                 * if {dcsbios} == ref1 and {dcsbios} == ref2 then output image
                 */
                if (Comparator1 == EnumComparator.Always)
                {
                    stringBuilder.Append("always ");
                }
                else
                {
                    stringBuilder.Append("if " + StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + GetComparatorAsString(Comparator1) + " " + ReferenceValue1 + " ");
                }

                if (Comparator2 != EnumComparator.None)
                {
                    stringBuilder.Append("and " + StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + GetComparatorAsString(Comparator2) + ReferenceValue2 + " ");
                }

                stringBuilder.Append("then output " + GetOutputAsString(OutputType));
                
                return stringBuilder.ToString();
            }
        }


        public EnumComparator Comparator1
        {
            get => _comparator1;
            set => _comparator1 = value;
        }

        public double ReferenceValue1
        {
            get => _referenceValue1;
            set => _referenceValue1 = value;
        }

        public EnumComparator Comparator2
        {
            get => _comparator2;
            set => _comparator2 = value;
        }

        public double ReferenceValue2
        {
            get => _referenceValue2;
            set => _referenceValue2 = value;
        }

        private string GetOutputAsString(EnumConverterOutputType converterOutputType)
        {
            var result = "";
            switch (converterOutputType)
            {
                case EnumConverterOutputType.NotSet:
                    {
                        result = "not set";
                        break;
                    }
                case EnumConverterOutputType.OutputRaw:
                    {
                        result = "number";
                        break;
                    }
                case EnumConverterOutputType.OutputImage:
                    {
                        result = "image";
                        break;
                    }
                case EnumConverterOutputType.OutputImageOverlay:
                    {
                        result = "dcsbios image overlay";
                        break;
                    }
                default:
                    {
                        result = "unknown?";
                        break;
                    }
            }

            return result;
        }

        private string GetComparatorAsString(EnumComparator comparator)
        {
            var result = "";

            switch (comparator)
            {
                case EnumComparator.None:
                    {
                        result = "None";
                        break;
                    }
                case EnumComparator.Equals:
                    {
                        result = "==";
                        break;
                    }
                case EnumComparator.NotEquals:
                    {
                        result = "!=";
                        break;
                    }
                case EnumComparator.LessThan:
                    {
                        result = "<";
                        break;
                    }
                case EnumComparator.LessThanEqual:
                    {
                        result = "<=";
                        break;
                    }
                case EnumComparator.GreaterThan:
                    {
                        result = ">";
                        break;
                    }
                case EnumComparator.GreaterThanEqual:
                    {
                        result = ">=";
                        break;
                    }
                case EnumComparator.Always:
                    {
                        result = "Always";
                        break;
                    }
            }

            return result;
        }
    }


    public enum EnumConverterOutputType
    {
        NotSet,
        Raw,
        Image,
        ImageOverlay
    }

    public enum EnumComparator
    {
        Equals = 0,
        NotEquals = 1,
        LessThan = 2,
        LessThanEqual = 3,
        GreaterThan = 4,
        GreaterThanEqual = 5,
        Always = 6,
        None = 10
    }
}

