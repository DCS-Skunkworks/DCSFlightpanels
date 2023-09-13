using Newtonsoft.Json;

namespace DCS_BIOS.Json
{
    /// <summary>
    /// Used when reading the JSON to create list of the inputs
    /// that a DCS-BIOS Control has.
    /// </summary>
    public class DCSBIOSControlInput
    {

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }
        
        [JsonProperty("interface", Required = Required.Default)]
        public string ControlInterface { get; set; }

        [JsonProperty("max_value", Required = Required.Default)]
        public int? MaxValue { get; set; }

        [JsonProperty("suggested_step", Required = Required.Default)]
        public int? SuggestedStep { get; set; }

        [JsonProperty("argument", Required = Required.Default)]
        public string Argument { get; set; }
    }
}
