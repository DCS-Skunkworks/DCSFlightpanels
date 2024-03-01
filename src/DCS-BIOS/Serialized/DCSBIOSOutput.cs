using DCS_BIOS.Json;
using DCS_BIOS.StringClasses;


namespace DCS_BIOS.Serialized
{
    using System;
    using System.ComponentModel;
    using ClassLibraryCommon;
    using DCS_BIOS;
    using ControlLocator;
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
    [SerializeCritical]
    public class DCSBIOSOutput
    {
        // The target value used for comparison as chosen by the user
        private uint _specifiedValueUInt;

        private uint _address;
        private volatile uint _lastUIntValue = uint.MaxValue;
        private volatile string _lastStringValue = "";
        private bool _uintValueHasChanged;

        [NonSerialized] private object _lockObject = new();

        public static DCSBIOSOutput CreateCopy(DCSBIOSOutput dcsbiosOutput)
        {
            var tmp = new DCSBIOSOutput
            {
                DCSBiosOutputType = dcsbiosOutput.DCSBiosOutputType,
                ControlId = dcsbiosOutput.ControlId,
                Address = dcsbiosOutput.Address,
                ControlDescription = dcsbiosOutput.ControlDescription,
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
            DCSBiosOutputComparison = dcsbiosOutput.DCSBiosOutputComparison;
            Mask = dcsbiosOutput.Mask;
            MaxLength = dcsbiosOutput.MaxLength;
            MaxValue = dcsbiosOutput.MaxValue;
            ShiftValue = dcsbiosOutput.ShiftValue;
            AddressIdentifier = dcsbiosOutput.AddressIdentifier;
            AddressMaskIdentifier = dcsbiosOutput.AddressMaskIdentifier;
            AddressMaskShiftIdentifier = dcsbiosOutput.AddressMaskShiftIdentifier;

            if (DCSBiosOutputType == DCSBiosOutputType.IntegerType)
            {
                SpecifiedValueUInt = dcsbiosOutput.SpecifiedValueUInt;
            }
        }
        public void Consume(DCSBIOSControl dcsbiosControl, DCSBiosOutputType dcsBiosOutputType)
        {
            ControlId = dcsbiosControl.Identifier;
            ControlDescription = dcsbiosControl.Description;
            try
            {
                if (!dcsbiosControl.HasOutput())
                {
                    DCSBiosOutputType = DCSBiosOutputType.None;
                    return;
                }

                foreach (var dcsbiosControlOutput in dcsbiosControl.Outputs)
                {
                    if (dcsbiosControlOutput.OutputDataType == dcsBiosOutputType)
                    {
                        DCSBiosOutputType = dcsbiosControlOutput.OutputDataType;
                        _address = dcsbiosControlOutput.Address;
                        Mask = dcsbiosControlOutput.Mask;
                        MaxValue = dcsbiosControlOutput.MaxValue;
                        MaxLength = dcsbiosControlOutput.MaxLength;
                        ShiftValue = dcsbiosControlOutput.ShiftBy;

                        AddressIdentifier = dcsbiosControlOutput.AddressIdentifier;
                        AddressMaskIdentifier = dcsbiosControlOutput.AddressMaskIdentifier;
                        AddressMaskShiftIdentifier = dcsbiosControlOutput.AddressMaskShiftIdentifier;

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
                throw new Exception($"Failed to copy control {ControlId}. Control output is missing.{Environment.NewLine}");
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

                _uintValueHasChanged = false;
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
            if (DCSBiosOutputType == DCSBiosOutputType.StringType)
            {
                return "";
            }

            return "DCSBiosOutput{" + ControlId + "|" + DCSBiosOutputComparison + "|" + _specifiedValueUInt + "}";
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
            ControlId = entries[0];
            var dcsBIOSControl = DCSBIOSControlLocator.GetControl(ControlId);
            Consume(dcsBIOSControl, DCSBiosOutputType.IntegerType);
            DCSBiosOutputComparison = (DCSBiosOutputComparison)Enum.Parse(typeof(DCSBiosOutputComparison), entries[1]);
            _specifiedValueUInt = (uint)int.Parse(entries[2]);
        }

        [JsonProperty("ControlId", Required = Required.Default)]
        public string ControlId { get; set; }

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
        public uint Mask { get; set; }

        [JsonProperty("Shiftvalue", Required = Required.Default)]
        public int ShiftValue { get; set; }

        [JsonProperty("DCSBiosOutputType", Required = Required.Default)]
        public DCSBiosOutputType DCSBiosOutputType { get; set; } = DCSBiosOutputType.None;

        [JsonProperty("DCSBiosOutputComparison", Required = Required.Default)]
        public DCSBiosOutputComparison DCSBiosOutputComparison { get; set; } = DCSBiosOutputComparison.Equals;

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
        public string ControlDescription { get; set; }

        [JsonProperty("MaxValue", Required = Required.Default)]
        public int MaxValue { get; set; }

        [JsonIgnore]
        public string AddressIdentifier { get; set; }

        [JsonIgnore]
        public string AddressMaskIdentifier { get; set; }

        [JsonIgnore]
        public string AddressMaskShiftIdentifier { get; set; }

        [JsonProperty("MaxLength", Required = Required.Default)]
        public int MaxLength { get; set; }

        [Obsolete]
        [JsonIgnore]
        public string ControlType { get; set; }
        
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
                DCSBiosOutputType.FloatBuffer => "float",
                DCSBiosOutputType.LED => "led",
                DCSBiosOutputType.ServoOutput => "servo output",
                _ => throw new Exception($"GetOutputType() : Failed to identify {DCSBiosOutputType} output type.")
            };
        }
    }
}
