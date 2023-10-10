using System.Diagnostics;
using DCS_BIOS.Json;

namespace DCS_BIOS
{
    using System;
    using System.ComponentModel;

    using Newtonsoft.Json;

    public enum DCSBiosOutputType
    {
        StringType,
        IntegerType,
        LED,
        ServoOutput,
        FloatBuffer,
        None
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

        // The target value used for comparison as chosen by the user
        private uint _specifiedValueUInt;

        private string _controlDescription;
        private int _maxValue;
        private uint _address;
        private uint _mask;
        private int _shiftValue;
        private int _maxLength;
        private volatile uint _lastUIntValue = uint.MaxValue;
        private volatile string _lastStringValue = "";
        private string _controlType; // display button toggle etc
        private DCSBiosOutputType _dcsBiosOutputType = DCSBiosOutputType.None;
        private DCSBiosOutputComparison _dcsBiosOutputComparison = DCSBiosOutputComparison.Equals;
        private bool _uintValueHasChanged = false;

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
                    tmp.SpecifiedValueUInt = dcsbiosOutput.SpecifiedValueUInt;
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
                SpecifiedValueUInt = dcsbiosOutput.SpecifiedValueUInt;
            }
        }
        public void Consume(DCSBIOSControl dcsbiosControl, DCSBiosOutputType dcsBiosOutputType)
        {
            _controlId = dcsbiosControl.Identifier;
            _controlDescription = dcsbiosControl.Description;
            _controlType = dcsbiosControl.PhysicalVariant;
            try
            {
                if (!dcsbiosControl.HasOutput())
                {
                    _dcsBiosOutputType = DCSBiosOutputType.None;
                    return;
                }

                foreach (var dcsbiosControlOutput in dcsbiosControl.Outputs)
                {
                    if (dcsbiosControlOutput.OutputDataType == dcsBiosOutputType)
                    {
                        _dcsBiosOutputType = dcsbiosControlOutput.OutputDataType;
                        _address = dcsbiosControlOutput.Address;
                        _mask = dcsbiosControlOutput.Mask;
                        _maxValue = dcsbiosControlOutput.MaxValue;
                        _maxLength = dcsbiosControlOutput.MaxLength;
                        _shiftValue = dcsbiosControlOutput.ShiftBy;

                        if (dcsBiosOutputType == DCSBiosOutputType.StringType)
                        {
                            DCSBIOSStringManager.AddListeningAddress(this);
                        }

                        DCSBIOSProtocolParser.RegisterAddressToBroadCast(_address);
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception($"Failed to copy control {_controlId}. Control output is missing.{Environment.NewLine}");
            }
        }

        /// <summary>
        /// Checks :
        /// <para>* if there is a there is a change in the value since last comparison</para>
        /// <para>* test is true using chosen comparison operator with new value and reference value</para>
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool UIntConditionIsMet(uint address, uint data)
        {
            _lockObject ??= new object();
            var result = false;

            lock (_lockObject)
            {
                if (Address != address)
                {
                    return false;
                }

                var newValue = (data & Mask) >> ShiftValue;

                var resultComparison = DCSBiosOutputComparison switch
                {
                    DCSBiosOutputComparison.BiggerThan => newValue > _specifiedValueUInt,
                    DCSBiosOutputComparison.LessThan => newValue < _specifiedValueUInt,
                    DCSBiosOutputComparison.NotEquals => newValue != _specifiedValueUInt,
                    DCSBiosOutputComparison.Equals => newValue == _specifiedValueUInt,
                    _ => throw new Exception("Unexpected DCSBiosOutputComparison value")
                };

                result = resultComparison && !newValue.Equals(LastUIntValue);
                //Debug.WriteLine($"(EvaluateUInt) Result={result} Target={_specifiedValueUInt} Last={LastUIntValue} New={newValue}");
                LastUIntValue = newValue;
            }

            return result;
        }

        /// <summary>
        /// Checks :
        /// <para>for address match</para>
        /// <para>that new value differs from previous</para>
        /// <para>stores new value</para>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns>Returns true when all checks are true.</returns>
        public bool UIntValueHasChanged(uint address, uint data)
        {
            _lockObject ??= new object();

            lock (_lockObject)
            {
                if (address != Address)
                {
                    // Not correct control
                    return false;
                }

                if (GetUIntValue(data) == LastUIntValue && !_uintValueHasChanged)
                {
                    // Value hasn't changed
                    return false;
                }

                LastUIntValue = GetUIntValue(data);
            }

            return true;
        }

        /// <summary>
        /// Checks :
        /// <para>for address match</para>
        /// <para>that new string value differs from previous</para>
        /// <para>stores new value</para>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="stringData"></param>
        /// <returns>Returns true when all checks are true.</returns>
        public bool StringValueHasChanged(uint address, string stringData)
        {
            _lockObject ??= new object();

            lock (_lockObject)
            {
                if (address != Address)
                {
                    // Not correct control
                    return false;
                }

                if ((_lastStringValue ?? string.Empty) != (stringData ?? string.Empty))
                {
                    _lastStringValue = stringData;
                    return true;
                }
            }

            return false;
        }

        public uint GetUIntValue(uint data)
        {
            /*
             * Fugly workaround, side effect of using deep clone DCSBIOSDecoder is that this is null
             */
            _lockObject ??= new object();

            lock (_lockObject)
            {
                LastUIntValue = (data & Mask) >> ShiftValue;
                return LastUIntValue;
            }
        }

        public override string ToString()
        {
            if (_dcsBiosOutputType == DCSBiosOutputType.StringType)
            {
                return "";
            }

            return "DCSBiosOutput{" + _controlId + "|" + _dcsBiosOutputComparison + "|" + _specifiedValueUInt + "}";
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
            Consume(dcsBIOSControl, DCSBiosOutputType.IntegerType);
            _dcsBiosOutputComparison = (DCSBiosOutputComparison)Enum.Parse(typeof(DCSBiosOutputComparison), entries[1]);
            _specifiedValueUInt = (uint)int.Parse(entries[2]);
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
        public uint SpecifiedValueUInt
        {
            get => _specifiedValueUInt;
            set
            {
                if (DCSBiosOutputType != DCSBiosOutputType.IntegerType)
                {
                    throw new Exception($"Invalid DCSBiosOutput. Specified value (trigger value) set to [int] but DCSBiosOutputType set to {DCSBiosOutputType}");
                }

                _specifiedValueUInt = value;
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
        public uint LastUIntValue
        {
            get => _lastUIntValue;
            set
            {
                if (value != _lastUIntValue)
                {
                    _uintValueHasChanged = true;
                }
                _lastUIntValue = value;
            }
        }

        [JsonIgnore]
        public string LastStringValue
        {
            get => _lastStringValue;
            set => _lastStringValue = value;
        }

        public static DCSBIOSOutput GetUpdateCounter()
        {
            var counter = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("_UPDATE_COUNTER");
            return counter;
        }
        
        public string GetOutputType()
        {
            return DCSBiosOutputType switch
            {
                DCSBiosOutputType.IntegerType => "integer",
                DCSBiosOutputType.StringType => "string",
                DCSBiosOutputType.None => "none",
                DCSBiosOutputType.FloatBuffer => "float buffer",
                DCSBiosOutputType.LED => "led",
                DCSBiosOutputType.ServoOutput => "servo output",
                _ => throw new Exception($"GetOutputType() : Failed to identify {DCSBiosOutputType} output type.")
            };
        }
    }
}
