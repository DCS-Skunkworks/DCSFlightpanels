using Newtonsoft.Json;

namespace DCS_BIOS.Json
{
    public class DCSBIOSControlInput
    {

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }
        
        [JsonProperty("interface", Required = Required.Default)]
        public string ControlInterface { get; set; }

        [JsonProperty("max_value", Required = Required.Default)]
        public int? MaxValue { get; set; }

        [JsonProperty("argument", Required = Required.Default)]
        public string Argument { get; set; }
    }
}
