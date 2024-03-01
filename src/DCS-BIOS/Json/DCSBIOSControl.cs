using System;
using System.Collections.Generic;
using System.Linq;
using DCS_BIOS.Serialized;
using Newtonsoft.Json;

namespace DCS_BIOS.Json
{
    /// <summary>
    /// This is a base class for the DCS-BIOS Control as specified in lua / JSON.
    /// This class is used when reading the JSON.
    /// </summary>
    public class DCSBIOSControl
    {
        [JsonProperty("category", Required = Required.Default)]
        public string Category { get; set; }

        [JsonProperty("control_type", Required = Required.Default)]
        public string ControlType { get; set; }

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty("identifier", Required = Required.Default)]
        public string Identifier { get; set; }

        [JsonProperty("inputs", Required = Required.Default)]
        public List<DCSBIOSControlInput> Inputs { get; set; }
        
        [JsonProperty("outputs", Required = Required.Default)]
        public List<DCSBIOSControlOutput> Outputs { get; set; }
        

        public bool HasOutput()
        {
            return Outputs.Count > 0;
        }

        public bool HasStringInput()
        {
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(this);
            return dcsbiosInput.DCSBIOSInputInterfaces.Any(o => o.Interface == DCSBIOSInputType.SET_STRING);
        }

        public DCSBIOSOutput GetUIntOutput()
        {
            foreach (var dcsbiosControlOutput in Outputs)
            {
                if (dcsbiosControlOutput.OutputDataType == DCSBiosOutputType.IntegerType)
                {
                    var dcsbiosOutput = new DCSBIOSOutput();
                    dcsbiosOutput.Consume(this, DCSBiosOutputType.IntegerType);
                    return dcsbiosOutput;
                }
            }

            throw new Exception($"Control {Identifier} did not have uint output.");
        }

        public DCSBIOSOutput GetStringOutput()
        {
            foreach (var dcsbiosControlOutput in Outputs)
            {
                if (dcsbiosControlOutput.OutputDataType == DCSBiosOutputType.StringType)
                {
                    var dcsbiosOutput = new DCSBIOSOutput();
                    dcsbiosOutput.Consume(this, DCSBiosOutputType.StringType);
                    return dcsbiosOutput;
                }
            }

            throw new Exception($"Control {Identifier} did not have string output.");
        }
    }
}
