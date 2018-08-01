using System;
using System.ComponentModel;
using ClassLibraryCommon;
// ReSharper disable All
/*
 * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
 */

namespace DCS_BIOS
{
    public enum DCSBiosOutputType
    {
        STRING_TYPE,
        INTEGER_TYPE
    }

    public enum DCSBiosOutputComparison
    {
        [Description("Equals")]
        Equals,
        [Description("Less than")]
        LessThan,
        [Description("Bigger than")]
        BiggerThan,
        [Description("Not equals")]
        NotEquals
    }

    public class DCSBIOSOutput
    {
        //These are loaded and saved, all the rest are fetched from DCS-BIOS
        private string _controlId;
        //The user has entered these two depending on type
        private uint _specifiedValueInt;
        private string _specifiedValueString = "";

        private string _controlDescription;
        private int _maxValue;
        private uint _address;
        private uint _mask;
        private int _shiftvalue;
        private int _maxLength;
        private volatile uint _lastIntValue = uint.MaxValue;
        private string _controlType; //display button toggle etc
        private DCSBiosOutputType _dcsBiosOutputType = DCSBiosOutputType.INTEGER_TYPE;
        private DCSBiosOutputComparison _dcsBiosOutputComparison = DCSBiosOutputComparison.Equals;
        //private String _formula;
        //private Expression _expression;
        private bool _debug;
        private readonly object _lockObject = new object();

        public static DCSBIOSOutput GetUpdateCounter()
        {
            var counter = DCSBIOSControlLocator.GetDCSBIOSOutput("_UPDATE_COUNTER");
            return counter;
        }
        /*
        private int Evaluate(uint data)
        {
            try
            {
                var value = GetUIntValue(data);
                _expression.Parameters["x"] = value;
                var result = _expression.Evaluate();
                return Convert.ToInt32(Math.Abs((double) result));
            }
            catch (Exception ex)
            {
                Common.LogError(124874, ex, "Evaluate() function");
                throw;
            }
        }
        */
        public bool CheckForValueMatchAndChange(object data)
        {
            //todo change not processed
            lock (_lockObject)
            {
                var result = false;
                if (DCSBiosOutputType == DCSBiosOutputType.INTEGER_TYPE && data is uint)
                {
                    result = CheckForValueMatchAndChange((uint)data);
                }
                else if (DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE && data is string)
                {
                    result = CheckForValueMatch((string)data);
                }
                else
                {
                    throw new Exception("Invalid DCSBiosOutput. Data is of type " + data.GetType() + " but DCSBiosOutputType set to " + DCSBiosOutputType);
                }
                return result;
            }
        }

        private bool CheckForValueMatchAndChange(uint data)
        {
            var tmpData = data;
            var value = (tmpData & Mask) >> Shiftvalue;
            var resultComparison = false;
            switch (DCSBiosOutputComparison)
            {
                case DCSBiosOutputComparison.BiggerThan:
                    {
                        resultComparison = value > _specifiedValueInt;
                        break;
                    }
                case DCSBiosOutputComparison.LessThan:
                    {
                        resultComparison = value < _specifiedValueInt;
                        break;
                    }
                case DCSBiosOutputComparison.NotEquals:
                    {
                        resultComparison = value != _specifiedValueInt;
                        break;
                    }
                case DCSBiosOutputComparison.Equals:
                    {
                        resultComparison = value == _specifiedValueInt;
                        break;
                    }
            }
            var resultChange = !value.Equals(_lastIntValue);
            if (resultChange)
            {
                _lastIntValue = value;
            }
            return resultComparison && resultChange;
        }

        public bool CheckForValueMatch(object data)
        {
            //todo change not processed
            lock (_lockObject)
            {
                var result = false;
                if (DCSBiosOutputType == DCSBiosOutputType.INTEGER_TYPE && data is uint)
                {
                    result = CheckForValueMatch((uint)data);
                }
                else if (DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE && data is string)
                {
                    result = CheckForValueMatch((string)data);
                }
                else
                {
                    throw new Exception("Invalid DCSBiosOutput. Data is of type " + data.GetType() + " but DCSBiosOutputType set to " + DCSBiosOutputType);
                }
                return result;
            }
        }

        private bool CheckForValueMatch(uint data)
        {
            var tmpData = data;
            var value = (tmpData & Mask) >> Shiftvalue;
            var result = false;
            switch (DCSBiosOutputComparison)
            {
                case DCSBiosOutputComparison.BiggerThan:
                    {
                        result = value > _specifiedValueInt;
                        break;
                    }
                case DCSBiosOutputComparison.LessThan:
                    {
                        result = value < _specifiedValueInt;
                        break;
                    }
                case DCSBiosOutputComparison.NotEquals:
                    {
                        result = value != _specifiedValueInt;
                        break;
                    }
                case DCSBiosOutputComparison.Equals:
                    {
                        result = value == _specifiedValueInt;
                        break;
                    }
            }
            return result;
        }

        public uint GetUIntValue(uint data)
        {
            if (_debug)
            {
                Common.DebugP(ToDebugString() + "    >>        Data is : " + data);
            }
            lock (_lockObject)
            {
                return (data & Mask) >> Shiftvalue;
            }
        }

        private bool CheckForValueMatch(string data)
        {
            lock (_lockObject)
            {
                var result = false;
                if (!string.IsNullOrEmpty(_specifiedValueString) && !string.IsNullOrEmpty(data))
                {
                    result = _specifiedValueString.Equals(data);
                }
                return result;
            }
        }

        public void Consume(DCSBIOSControl dcsbiosControl)
        {

            _controlId = dcsbiosControl.identifier;
            _controlDescription = dcsbiosControl.description;
            _controlType = dcsbiosControl.physical_variant;
            try
            {
                _address = dcsbiosControl.outputs[0].address;
                _mask = dcsbiosControl.outputs[0].mask;
                _maxValue = dcsbiosControl.outputs[0].max_value;
                _shiftvalue = dcsbiosControl.outputs[0].shift_by;
                if (dcsbiosControl.outputs[0].type.Equals("string"))
                {
                    _dcsBiosOutputType = DCSBiosOutputType.STRING_TYPE;
                }
                else if (dcsbiosControl.outputs[0].type.Equals("integer"))
                {
                    _dcsBiosOutputType = DCSBiosOutputType.INTEGER_TYPE;
                }
                //TODO Denna borde göras så att förutom _address så är mottagarens unika ID med så slipper alla lyssna eller ..? (prestanda)
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
            catch (Exception)
            {
                throw new Exception("Failed to copy control " + _controlId + ". Control output is missing." + Environment.NewLine);
            }
        }

        public string ToDebugString()
        {
            if (_dcsBiosOutputType == DCSBiosOutputType.STRING_TYPE)
            {
                return "DCSBiosOutput{" + _controlId + "|0x" + _address.ToString("x") + "|0x" + _mask.ToString("x") + "|" + _shiftvalue + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueString + "}";
            }
            return "DCSBiosOutput{" + _controlId + "|0x" + _address.ToString("x") + "|0x" + _mask.ToString("x") + "|" + _shiftvalue + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueInt + "}";
        }

        public override string ToString()
        {
            if (_dcsBiosOutputType == DCSBiosOutputType.STRING_TYPE)
            {
                return "DCSBiosOutput{" + _controlId + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueString + "}";
            }
            return "DCSBiosOutput{" + _controlId + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueInt + "}";
        }

        public void ImportString(string str)
        {
            //DCSBiosOutput{AAP_EGIPWR|Equals|0}
            var value = str;
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("DCSBiosOutput cannot import null string.");
            }
            if (!str.StartsWith("DCSBiosOutput{") || !str.EndsWith("}"))
            {
                throw new Exception("DCSBiosOutput cannot import string : " + str);
            }
            value = value.Replace("DCSBiosOutput{", "").Replace("}", "");
            //AAP_EGIPWR|Equals|0
            var entries = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            _controlId = entries[0];
            var dcsBIOSControl = DCSBIOSControlLocator.GetControl(_controlId);
            Consume(dcsBIOSControl);
            _dcsBiosOutputComparison = (DCSBiosOutputComparison)Enum.Parse(typeof(DCSBiosOutputComparison), entries[1]);
            if (DCSBiosOutputType == DCSBiosOutputType.INTEGER_TYPE)
            {
                _specifiedValueInt = (uint)int.Parse(entries[2]);
            }
            else if (DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE)
            {
                _specifiedValueString = entries[2];
            }
        }

        public string ControlId
        {
            get { return _controlId; }
            set { _controlId = value; }
        }

        public uint Address
        {
            get { return _address; }
            set
            {
                _address = value;
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
        }

        public uint Mask
        {
            get { return _mask; }
            set
            {
                _mask = value;
            }
        }

        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

        public int Shiftvalue
        {
            get { return _shiftvalue; }
            set
            {
                _shiftvalue = value;
            }
        }

        public DCSBiosOutputType DCSBiosOutputType
        {
            get { return _dcsBiosOutputType; }
            set { _dcsBiosOutputType = value; }
        }

        public DCSBiosOutputComparison DCSBiosOutputComparison
        {
            get { return _dcsBiosOutputComparison; }
            set { _dcsBiosOutputComparison = value; }
        }

        public uint SpecifiedValueInt
        {
            get
            {
                return _specifiedValueInt;
            }
            set
            {
                if (DCSBiosOutputType != DCSBiosOutputType.INTEGER_TYPE)
                {
                    throw new Exception("Invalid DCSBiosOutput. Specified value (trigger value) set to [int] but DCSBiosOutputType set to " + DCSBiosOutputType);
                }
                _specifiedValueInt = value;
            }
        }

        public string SpecifiedValueString
        {
            get
            {
                return _specifiedValueString;
            }
            set
            {
                if (DCSBiosOutputType != DCSBiosOutputType.STRING_TYPE)
                {
                    throw new Exception("Invalid DCSBiosOutput. Specified value (trigger value) set to [String] but DCSBiosOutputType set to " + DCSBiosOutputType);
                }
                _specifiedValueString = value;
            }
        }

        public string ControlDescription
        {
            get { return _controlDescription; }
            set { _controlDescription = value; }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; }
        }

        public string ControlType
        {
            get { return _controlType; }
            set { _controlType = value; }
        }
        /*
        public string Formula
        {
            get { return _formula; }
            set
            {
                _formula = value;
                _expression = new Expression(_formula);
            }
        }
        */
        public uint LastIntValue
        {
            get { return _lastIntValue; }
            set { _lastIntValue = value; }
        }

    }
}
