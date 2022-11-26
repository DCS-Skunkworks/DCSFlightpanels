using System.Collections.Generic;
using Newtonsoft.Json;

namespace DCS_BIOS.Json
{
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

        [JsonProperty("momentary_positions", Required = Required.Default)]
        public string MomentaryPositions { get; set; }

        [JsonProperty("outputs", Required = Required.Default)]
        public List<DCSBIOSControlOutput> Outputs { get; set; }

        [JsonProperty("physical_variant", Required = Required.Default)]
        public string PhysicalVariant { get; set; }
    }
}
