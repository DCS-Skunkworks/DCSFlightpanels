using DCS_BIOS.Json;

namespace DCS_BIOS
{
    using System;
    using System.ComponentModel;

    using Newtonsoft.Json;

    public enum DCSBiosOutputType
    {
        StringType,
        IntegerType
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

    /// <summary>
    /// This class represents the output sent from DCS-BIOS.
    /// When a DCS-BIOS Control value has been sent each class
    /// listening for specific DCS-BIOS value(s) (Address & Data)
    /// can check via the Address part whether it was a match and if
    /// it was then extract the Data part. Data is bit shifted so it can't be
    /// read directly. This class holds the information on how much to shift
    /// and with what.
    /// </summary>
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
        private DCSBiosOutputType _dcsBiosOutputType = DCSBiosOutputType.IntegerType;
        private DCSBiosOutputComparison _dcsBiosOutputComparison = DCSBiosOutputComparison.Equals;

        [NonSerialized] private object _lockObject = new();
        
        public static DCSBIOSOutput CreateCopy(DCSBIOSOutput dcsbiosOutput)
        {
            var tmp = new DCSBIOSOutput
            {
                DCSBiosOutputType = dcsbiosOutput.DCSBiosOutputType,
                ControlId = dcsbiosOutput.ControlId,
                Address = dcsbiosOutput.Address,
                ControlDescription = dcsbiosOutput.ControlDescription,
                ControlType = dcsbiosOutput.ControlType,
                DCSBiosOutputComparison = dcsbiosOutput.DCSBiosOutputComparison,
                Mask = dcsbiosOutput.Mask,
                MaxLength = dcsbiosOutput.MaxLength,
                MaxValue = dcsbiosOutput.MaxValue,
                ShiftValue = dcsbiosOutput.ShiftValue
            };

            switch (tmp.DCSBiosOutputType)
            {
                case DCSBiosOutputType.IntegerType:
                    tmp.SpecifiedValueInt = dcsbiosOutput.SpecifiedValueInt;
                    break;
                case DCSBiosOutputType.StringType:
                    tmp.SpecifiedValueString = dcsbiosOutput.SpecifiedValueString;
                    break;
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
            ShiftValue = dcsbiosOutput.ShiftValue;
            if (DCSBiosOutputType == DCSBiosOutputType.IntegerType)
            {
                SpecifiedValueInt = dcsbiosOutput.SpecifiedValueInt;
            }

            if (DCSBiosOutputType == DCSBiosOutputType.StringType)
            {
                SpecifiedValueString = dcsbiosOutput.SpecifiedValueString;
            }
        }

        public bool CheckForValueMatchAndChange(object data)
        {
            // todo change not processed
            lock (_lockObject)
            {
                bool result;
                if (DCSBiosOutputType == DCSBiosOutputType.IntegerType && data is uint u)
                {
                    result = CheckForValueMatchAndChange(u);
                }
                else if (DCSBiosOutputType == DCSBiosOutputType.StringType && data is string s)
                {
                    result = CheckForValueMatch(s);
                }
                else
                {
                    throw new Exception($"Invalid DCSBiosOutput. Data is of type {data.GetType()} but DCSBiosOutputType set to {DCSBiosOutputType}");
                }

                return result;
            }
        }

        private bool CheckForValueMatchAndChange(uint data)
        {
            var tmpData = data;
            var value = (tmpData & Mask) >> ShiftValue;

            var resultComparison = DCSBiosOutputComparison switch
            {
                DCSBiosOutputComparison.BiggerThan  => value > _specifiedValueInt,
                DCSBiosOutputComparison.LessThan    => value < _specifiedValueInt,
                DCSBiosOutputComparison.NotEquals   => value != _specifiedValueInt,
                DCSBiosOutputComparison.Equals      => value == _specifiedValueInt,
                _ => throw new Exception("Unexpected DCSBiosOutputComparison value")
            };

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
                return true switch {
                  _ when DCSBiosOutputType == DCSBiosOutputType.IntegerType && data is uint u => CheckForValueMatch(u),
                  _ when DCSBiosOutputType == DCSBiosOutputType.StringType && data is string s => CheckForValueMatch(s),
                  _ => throw new Exception($"Invalid DCSBiosOutput. Data is of type {data.GetType()} but DCSBiosOutputType set to {DCSBiosOutputType}")
                };
            }
        }

        private bool CheckForValueMatch(uint data)
        {
            var tmpData = data;
            var value = (tmpData & Mask) >> ShiftValue;

            return DCSBiosOutputComparison switch {
                DCSBiosOutputComparison.BiggerThan =>   value > _specifiedValueInt,
                DCSBiosOutputComparison.LessThan =>     value < _specifiedValueInt,
                DCSBiosOutputComparison.NotEquals =>    value != _specifiedValueInt,
                DCSBiosOutputComparison.Equals =>       value == _specifiedValueInt,
                _ => throw new Exception("Unexpected DCSBiosOutputComparison value")
            };
        }

        public uint GetUIntValue(uint data)
        {
            /*
             * Fugly workaround, side effect of using deep clone DCSBIOSDecoder is that this is null
             */
            _lockObject ??= new object();

            lock (_lockObject)
            {
                return (data & Mask) >> ShiftValue;
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
            _controlId = dcsbiosControl.Identifier;
            _controlDescription = dcsbiosControl.Description;
            _controlType = dcsbiosControl.PhysicalVariant;
            try
            {
                _address = dcsbiosControl.Outputs[0].Address;
                _mask = dcsbiosControl.Outputs[0].Mask;
                _maxValue = dcsbiosControl.Outputs[0].MaxValue;
                _maxLength = dcsbiosControl.Outputs[0].MaxLength;
                _shiftValue = dcsbiosControl.Outputs[0].ShiftBy;
                if (dcsbiosControl.Outputs[0].Type.Equals("string"))
                {
                    _dcsBiosOutputType = DCSBiosOutputType.StringType;
                }
                else if (dcsbiosControl.Outputs[0].Type.Equals("integer"))
                {
                    _dcsBiosOutputType = DCSBiosOutputType.IntegerType;
                }
                
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
            catch (Exception)
            {
                throw new Exception($"Failed to copy control {_controlId}. Control output is missing.{Environment.NewLine}");
            }
        }

        public string ToDebugString()
        {
            if (_dcsBiosOutputType == DCSBiosOutputType.StringType)
            {
                return "DCSBiosOutput{" + _controlId + "|0x" + _address.ToString("x") + "|0x" + _mask.ToString("x") + "|" + _shiftValue + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueString + "}";
            }

            return "DCSBiosOutput{" + _controlId + "|0x" + _address.ToString("x") + "|0x" + _mask.ToString("x") + "|" + _shiftValue + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueInt + "}";
        }

        public override string ToString()
        {
            if (_dcsBiosOutputType == DCSBiosOutputType.StringType)
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
                throw new Exception($"DCSBiosOutput cannot import string : {str}");
            }

            value = value.Replace("DCSBiosOutput{", string.Empty).Replace("}", string.Empty);

            // AAP_EGIPWR|Equals|0
            var entries = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            _controlId = entries[0];
            var dcsBIOSControl = DCSBIOSControlLocator.GetControl(_controlId);
            Consume(dcsBIOSControl);
            _dcsBiosOutputComparison = (DCSBiosOutputComparison)Enum.Parse(typeof(DCSBiosOutputComparison), entries[1]);
            if (DCSBiosOutputType == DCSBiosOutputType.IntegerType)
            {
                _specifiedValueInt = (uint)int.Parse(entries[2]);
            }
            else if (DCSBiosOutputType == DCSBiosOutputType.StringType)
            {
                _specifiedValueString = entries[2];
            }
        }

        [JsonProperty("ControlId", Required = Required.Default)]
        public string ControlId
        {
            get => _controlId;
            set => _controlId = value;
        }

        [JsonProperty("Address", Required = Required.Default)]
        public uint Address
        {
            get => _address;
            set
            {
                _address = value;
                DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
            }
        }

        [JsonProperty("Mask", Required = Required.Default)]
        public uint Mask
        {
            get => _mask;
            set => _mask = value;
        }

        [JsonProperty("Shiftvalue", Required = Required.Default)]
        public int ShiftValue
        {
            get => _shiftValue;
            set => _shiftValue = value;
        }

        [JsonProperty("DCSBiosOutputType", Required = Required.Default)]
        public DCSBiosOutputType DCSBiosOutputType
        {
            get => _dcsBiosOutputType;
            set => _dcsBiosOutputType = value;
        }

        [JsonProperty("DCSBiosOutputComparison", Required = Required.Default)]
        public DCSBiosOutputComparison DCSBiosOutputComparison
        {
            get => _dcsBiosOutputComparison;
            set => _dcsBiosOutputComparison = value;
        }

        [JsonIgnore]
        public uint SpecifiedValueInt
        {
            get => _specifiedValueInt;
            set
            {
                if (DCSBiosOutputType != DCSBiosOutputType.IntegerType)
                {
                    throw new Exception($"Invalid DCSBiosOutput. Specified value (trigger value) set to [int] but DCSBiosOutputType set to {DCSBiosOutputType}");
                }

                _specifiedValueInt = value;
            }
        }

        [JsonIgnore]
        public string SpecifiedValueString
        {
            get => _specifiedValueString;
            set
            {
                if (DCSBiosOutputType != DCSBiosOutputType.StringType)
                {
                    throw new Exception($"Invalid DCSBiosOutput. Specified value (trigger value) set to [String] but DCSBiosOutputType set to {DCSBiosOutputType}");
                }

                _specifiedValueString = value;
            }
        }

        [JsonProperty("ControlDescription", Required = Required.Default)]
        public string ControlDescription
        {
            get => _controlDescription;
            set => _controlDescription = value;
        }

        [JsonProperty("MaxValue", Required = Required.Default)]
        public int MaxValue
        {
            get => _maxValue;
            set => _maxValue = value;
        }

        [JsonProperty("MaxLength", Required = Required.Default)]
        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = value;
        }

        [JsonProperty("ControlType", Required = Required.Default)]
        public string ControlType
        {
            get => _controlType;
            set => _controlType = value;
        }

        [JsonIgnore]
        public uint LastIntValue
        {
            get => _lastIntValue;
            set => _lastIntValue = value;
        }

        public static DCSBIOSOutput GetUpdateCounter()
        {
            var counter = DCSBIOSControlLocator.GetDCSBIOSOutput("_UPDATE_COUNTER");
            return counter;
        }
    }
}
