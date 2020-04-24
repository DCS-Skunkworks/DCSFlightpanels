using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class DCSBIOSConverter
    {
        private EnumComparator _comparator1 = EnumComparator.NotSet;
        private EnumComparator _comparator2 = EnumComparator.NotSet;
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
            if (!_criteria1IsOk || (_comparator2 != EnumComparator.NotSet && !_criteria2IsOk))
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
                case EnumConverterOutputType.Image:
                    {
                        result = _faceTypeImage.Bitmap;
                        break;
                    }
                case EnumConverterOutputType.ImageOverlay:
                    {
                        result = _faceTypeDCSBIOSOverlay.Bitmap;
                        break;
                    }
                case EnumConverterOutputType.Raw:
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

                if (_comparator2 == EnumComparator.NotSet)
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

                if (Comparator2 != EnumComparator.NotSet)
                {
                    stringBuilder.Append("and " + StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + GetComparatorAsString(Comparator2) + ReferenceValue2 + " ");
                }

                if (Comparator1 != EnumComparator.Always)
                {
                    stringBuilder.Append("then ");
                }

                stringBuilder.Append("output " + GetOutputAsString(OutputType));

                if (Comparator1 == EnumComparator.Always)
                {
                    stringBuilder.Append(" : " + ButtonText.Replace(Environment.NewLine, " "));
                }

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
                case EnumConverterOutputType.Raw:
                    {
                        result = "raw";
                        break;
                    }
                case EnumConverterOutputType.Image:
                    {
                        result = "image";
                        break;
                    }
                case EnumConverterOutputType.ImageOverlay:
                    {
                        result = "image overlay";
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
                case EnumComparator.NotSet:
                    {
                        result = "NotSet";
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

        [JsonIgnore]
        public string ImageFileRelativePath
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.Raw:
                        {
                            throw new Exception("DCSBIOSConverter.ImageFileRelativePath: FaceTypeText have no property ImageFileRelativePath. DCSBIOSConverter.ImageFileRelativePath.");
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.ImageFile;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.BackgroundBitmapPath;
                        }
                }
                throw new Exception("DCSBIOSConverter.ImageFileRelativePath: Exception. OutputType not known " + _outputType);
            }
            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.ImageFileRelativePath:  OutputType is [NotSet]. Can not add ImageFileRelativePath.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            throw new Exception("DCSBIOSConverter.ImageFileRelativePath: FaceTypeText have no property ImageFileRelativePath. DCSBIOSConverter.ImageFileRelativePath.");
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.ImageFile = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.BackgroundBitmapPath = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.ImageFileRelativePath: OutputType not known " + _outputType);
                        }
                }
            }
        }

        [JsonIgnore]
        public string ButtonText
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            return "";
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.ButtonText;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.Text;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.ButtonText;
                        }
                }
                return "";
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.ButtonText: OutputType is [NotSet]. Can not add Text.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.ButtonText = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.Text = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.ButtonText = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.ButtonText: OutputType not known " + _outputType + ".");
                        }
                }
            }
        }

        [JsonIgnore]
        public int OffsetX
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            return 0;
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.OffsetX;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.OffsetX;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.OffsetX;
                        }
                }
                return 0;
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.OffsetX:  OutputType is [NotSet]. Can not add OffsetX.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.OffsetX = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.OffsetX = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.OffsetX = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.OffsetX: OutputType not known " + _outputType + ".");
                        }
                }
            }
        }

        [JsonIgnore]
        public int OffsetY
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            return 0;
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.OffsetY;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.OffsetY;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.OffsetY;
                        }
                }
                return 0;
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.OffsetY:  OutputType is [NotSet]. Can not add OffsetY.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.OffsetY = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.OffsetY = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.OffsetY = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.OffsetY: Exception. OutputType not known " + _outputType + ".");
                        }
                }
            }
        }


        [JsonIgnore]
        public Color FontColor
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            return Color.Transparent;
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.FontColor;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.FontColor;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.FontColor;
                        }
                }
                return Color.Transparent;
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.FontColor:  OutputType is [NotSet]. Can not add FontColor.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.FontColor = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.FontColor = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.FontColor = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.FontColor:  FaceTypeText OutputType not known " + _outputType + ".");
                        }
                }
            }
        }

        [JsonIgnore]
        public Color BackgroundColor
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            return Color.Transparent;
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.BackgroundColor;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.BackgroundColor;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.BackgroundColor;
                        }
                }
                return Color.Transparent;
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.BackgroundColor: OutputType is [NotSet]. Can not add BackgroundColor.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.BackgroundColor = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.BackgroundColor = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.BackgroundColor: Exception. FaceTypeText OutputType not known " + _outputType + ".");
                        }
                }
            }
        }


        [JsonIgnore]
        public Font TextFont
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.TextFont:  OutputType is [NotSet]. Can not retrieve Font.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.TextFont;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.TextFont;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.TextFont;
                        }
                }
                throw new Exception("DCSBIOSConverter.TextFont: Exception. FaceTypeText OutputType not known " + _outputType + ".");
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.TextFont:  OutputType is [NotSet]. Can not add TextFont.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.TextFont = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.TextFont = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.TextFont = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.TextFont: Exception. FaceTypeText OutputType not known " + _outputType + ".");
                        }
                }
            }
        }


        [JsonIgnore]
        public Bitmap Bitmap
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter.Bitmap: OutputType is [NotSet]. Can not retrieve Bitmap.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.Bitmap;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.Bitmap;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.Bitmap;
                        }
                }
                throw new Exception("DCSBIOSConverter.Bitmap: Exception. FaceTypeText OutputType not known " + _outputType + ".");
            }

            set
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            throw new Exception("DCSBIOSConverter OutputType is [NotSet]. Can not add Bitmap.");
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            _faceTypeText.Bitmap = value;
                            break;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            _faceTypeImage.Bitmap = value;
                            break;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            _faceTypeDCSBIOSOverlay.Bitmap = value;
                            break;
                        }
                    default:
                        {
                            throw new Exception("DCSBIOSConverter.Bitmap: Exception. FaceTypeText OutputType not known " + _outputType + ".");
                        }
                }
            }
        }


        [JsonIgnore]
        public bool FaceConfigurationIsOK
        {
            get
            {
                switch (OutputType)
                {
                    case EnumConverterOutputType.NotSet:
                        {
                            return false;
                        }
                    case EnumConverterOutputType.Raw:
                        {
                            return _faceTypeText.ConfigurationOK;
                        }
                    case EnumConverterOutputType.Image:
                        {
                            return _faceTypeImage.ConfigurationOK;
                        }
                    case EnumConverterOutputType.ImageOverlay:
                        {
                            return _faceTypeDCSBIOSOverlay.ConfigurationOK;
                        }
                }
                throw new Exception("FaceConfigurationIsOK: Exception. FaceTypeText OutputType not known " + _outputType + ".");
            }
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
        NotSet = 10
    }
}

