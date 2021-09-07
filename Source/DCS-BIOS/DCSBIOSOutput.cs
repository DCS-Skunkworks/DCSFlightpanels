namespace DCS_BIOS
{
    using System;
    using System.ComponentModel;

    using Newtonsoft.Json;

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

    [Serializable]
    public class DCSBIOSOutput
    {
        // These are loaded and saved, all the rest are fetched from DCS-BIOS
        private string _controlId;

        // The user has entered these two depending on type
        private uint _specifiedValueInt;
        private string _specifiedValueString = string.Empty;

        private string _controlDescription;
        private int _maxValue;
        private uint _address;
        private uint _mask;
        private int _shiftValue;
        private int _maxLength;
        private volatile uint _lastIntValue = uint.MaxValue;
        private string _controlType; // display button toggle etc
        private DCSBiosOutputType _dcsBiosOutputType = DCSBiosOutputType.INTEGER_TYPE;
        private DCSBiosOutputComparison _dcsBiosOutputComparison = DCSBiosOutputComparison.Equals;

        [NonSerialized] private object _lockObject = new object();
        
        public static DCSBIOSOutput CreateCopy(DCSBIOSOutput dcsbiosOutput)
        {
            var tmp = new DCSBIOSOutput();
            tmp.DCSBiosOutputType = dcsbiosOutput.DCSBiosOutputType;
            tmp.ControlId = dcsbiosOutput.ControlId;
            tmp.Address = dcsbiosOutput.Address;
            tmp.ControlDescription = dcsbiosOutput.ControlDescription;
            tmp.ControlType = dcsbiosOutput.ControlType;
            tmp.DCSBiosOutputComparison = dcsbiosOutput.DCSBiosOutputComparison;
            tmp.Mask = dcsbiosOutput.Mask;
            tmp.MaxLength = dcsbiosOutput.MaxLength;
            tmp.MaxValue = dcsbiosOutput.MaxValue;
            tmp.Shiftvalue = dcsbiosOutput.Shiftvalue;
            if (tmp.DCSBiosOutputType == DCSBiosOutputType.INTEGER_TYPE)
            {
                tmp.SpecifiedValueInt = dcsbiosOutput.SpecifiedValueInt;
            }
            if (tmp.DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE)
            {
                tmp.SpecifiedValueString = dcsbiosOutput.SpecifiedValueString;
            }
            return tmp;
        }

        public void Copy(DCSBIOSOutput dcsbiosOutput)
        {
            DCSBiosOutputType = dcsbiosOutput.DCSBiosOutputType;
            ControlId = dcsbiosOutput.ControlId;
            Address = dcsbiosOutput.Address;
            ControlDescription = dcsbiosOutput.ControlDescription;
            ControlType = dcsbiosOutput.ControlType;
            DCSBiosOutputComparison = dcsbiosOutput.DCSBiosOutputComparison;
            Mask = dcsbiosOutput.Mask;
            MaxLength = dcsbiosOutput.MaxLength;
            MaxValue = dcsbiosOutput.MaxValue;
            Shiftvalue = dcsbiosOutput.Shiftvalue;
            if (DCSBiosOutputType == DCSBiosOutputType.INTEGER_TYPE)
            {
                SpecifiedValueInt = dcsbiosOutput.SpecifiedValueInt;
            }
            if (DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE)
            {
                SpecifiedValueString = dcsbiosOutput.SpecifiedValueString;
            }
        }

        public bool CheckForValueMatchAndChange(object data)
        {
            // todo change not processed
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
            /*
             * Fugly workaround, side effect of using deep clone DCSBIOSDecoder is that this is null
             */
            if (_lockObject == null)
            {
                _lockObject = new object();
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
                _maxLength = dcsbiosControl.outputs[0].max_length;
                this._shiftValue = dcsbiosControl.outputs[0].shift_by;
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
                return "DCSBiosOutput{" + _controlId + "|0x" + _address.ToString("x") + "|0x" + _mask.ToString("x") + "|" + this._shiftValue + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueString + "}";
            }
            return "DCSBiosOutput{" + _controlId + "|0x" + _address.ToString("x") + "|0x" + _mask.ToString("x") + "|" + this._shiftValue + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueInt + "}";
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
            // DCSBiosOutput{AAP_EGIPWR|Equals|0}
            var value = str;
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("DCSBiosOutput cannot import null string.");
            }
            if (!str.StartsWith("DCSBiosOutput{") || !str.EndsWith("}"))
            {
                throw new Exception("DCSBiosOutput cannot import string : " + str);
            }
            value = value.Replace("DCSBiosOutput{", string.Empty).Replace("}", string.Empty);

            // AAP_EGIPWR|Equals|0
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

        [JsonProperty("ControlId", Required = Required.Default)]
        public string ControlId
        {
            get { return _controlId; }
            set { _controlId = value; }
        }

        [JsonProperty("Address", Required = Required.Default)]
        public uint Address
        {
            get { return _address; }
            set
            {
                _address = value;
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
        }

        [JsonProperty("Mask", Required = Required.Default)]
        public uint Mask
        {
            get { return _mask; }
            set
            {
                _mask = value;
            }
        }

        [JsonProperty("Shiftvalue", Required = Required.Default)]
        public int Shiftvalue
        {
            get { return this._shiftValue; }
            set
            {
                this._shiftValue = value;
            }
        }

        [JsonProperty("DCSBiosOutputType", Required = Required.Default)]
        public DCSBiosOutputType DCSBiosOutputType
        {
            get { return _dcsBiosOutputType; }
            set { _dcsBiosOutputType = value; }
        }

        [JsonProperty("DCSBiosOutputComparison", Required = Required.Default)]
        public DCSBiosOutputComparison DCSBiosOutputComparison
        {
            get { return _dcsBiosOutputComparison; }
            set { _dcsBiosOutputComparison = value; }
        }

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonProperty("ControlDescription", Required = Required.Default)]
        public string ControlDescription
        {
            get { return _controlDescription; }
            set { _controlDescription = value; }
        }

        [JsonProperty("MaxValue", Required = Required.Default)]
        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        [JsonProperty("MaxLength", Required = Required.Default)]
        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; }
        }

        [JsonProperty("ControlType", Required = Required.Default)]
        public string ControlType
        {
            get { return _controlType; }
            set { _controlType = value; }
        }

        [JsonIgnore]
        public uint LastIntValue
        {
            get { return _lastIntValue; }
            set { _lastIntValue = value; }
        }


        public static DCSBIOSOutput GetUpdateCounter()
        {
            var counter = DCSBIOSControlLocator.GetDCSBIOSOutput("_UPDATE_COUNTER");
            return counter;
        }
    }
}
