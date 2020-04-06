using System;
using System.Globalization;
using System.Windows.Markup;

namespace NonVisuals.StreamDeck
{
    public enum EnumComparator
    {
        Equals = 0,
        NotEquals = 1,
        LessThan = 2,
        LessThanEqual = 3,
        GreaterThan = 4,
        GreaterThanEqual = 5,
        Always = 6
    }

    public class DCSBIOSNumberToText
    {
        private EnumComparator _comparator = EnumComparator.Equals;
        private double _referenceValue = 0;
        private string _outputText;

        public string ConvertNumber(double comparisonValue, out bool resultFound)
        {
            string result = null;
            resultFound = false;

            switch (_comparator)
            {
                case EnumComparator.Equals:
                    {
                        if (Math.Abs(comparisonValue - _referenceValue) < 1)
                        {
                            result = ReplacePlaceHolder(comparisonValue, _outputText);
                            resultFound = true;
                        }
                        break;
                    }
                case EnumComparator.NotEquals:
                    {
                        if (Math.Abs(comparisonValue - _referenceValue) > 0.001)
                        {
                            result = ReplacePlaceHolder(comparisonValue, _outputText);
                            resultFound = true;
                        }
                        break;
                    }
                case EnumComparator.LessThan:
                    {
                        if (comparisonValue < _referenceValue)
                        {
                            result = ReplacePlaceHolder(comparisonValue, _outputText);
                            resultFound = true;
                        }
                        break;
                    }
                case EnumComparator.LessThanEqual:
                    {
                        if (comparisonValue <= _referenceValue)
                        {
                            result = ReplacePlaceHolder(comparisonValue, _outputText);
                            resultFound = true;
                        }
                        break;
                    }
                case EnumComparator.GreaterThan:
                    {
                        if (comparisonValue > _referenceValue)
                        {
                            result = ReplacePlaceHolder(comparisonValue, _outputText);
                            resultFound = true;
                        }
                        break;
                    }
                case EnumComparator.GreaterThanEqual:
                    {
                        if (comparisonValue >= _referenceValue)
                        {
                            result = ReplacePlaceHolder(comparisonValue, _outputText);
                            resultFound = true;
                        }
                        break;
                    }
                case EnumComparator.Always:
                    {
                        result = ReplacePlaceHolder(comparisonValue, _outputText);
                        resultFound = true;
                        break;
                    }
            }

            return result;
        }

        private string ReplacePlaceHolder(double currentValue, string output)
        {
            //"Course {dcsbios}°
            if (output.Contains(CommonStreamDeck.DCSBIOS_PLACE_HOLDER))
            {
                output = output.Replace(CommonStreamDeck.DCSBIOS_PLACE_HOLDER, currentValue.ToString(CultureInfo.InvariantCulture));
            }

            return output;
        }

        public string FriendlyInfo
        {
            get
            {
                var comparator = "";

                switch (_comparator)
                {
                    case EnumComparator.Equals:
                        {
                            comparator = "==";
                            break;
                        }
                    case EnumComparator.NotEquals:
                        {
                            comparator = "!=";
                            break;
                        }
                    case EnumComparator.LessThan:
                        {
                            comparator = "<";
                            break;
                        }
                    case EnumComparator.LessThanEqual:
                        {
                            comparator = "<=";
                            break;
                        }
                    case EnumComparator.GreaterThan:
                        {
                            comparator = ">";
                            break;
                        }
                    case EnumComparator.GreaterThanEqual:
                        {
                            comparator = ">=";
                            break;
                        }
                    case EnumComparator.Always:
                        {
                            comparator = "Always";
                            break;
                        }
                }
                return "IF {dcsbios} " + Comparator + (Comparator == EnumComparator.Always ? "" : " " + _referenceValue) + " THEN " + OutputText;
            }
        }

        public EnumComparator Comparator
        {
            get => _comparator;
            set => _comparator = value;
        }

        public double ReferenceValue
        {
            get => _referenceValue;
            set => _referenceValue = value;
        }

        public string OutputText
        {
            get => _outputText;
            set => _outputText = value;
        }
    }
}

