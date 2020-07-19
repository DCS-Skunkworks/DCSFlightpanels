using System.Collections.Generic;

// ReSharper disable All
/*
 * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
 */
namespace DCS_BIOS
{
    public class DCSBIOSControl
    {
        public string category { get; set; }
        public string control_type { get; set; }
        public string description { get; set; }
        public string identifier { get; set; }
        public List<DCSBIOSControlInput> inputs { get; set; }
        public string momentary_positions { get; set; }
        public List<DCSBIOSControlOutput> outputs { get; set; }
        public string physical_variant { get; set; }
    }

    public class DCSBIOSControlRootObject
    {
        public List<DCSBIOSControl> DCSBIOSControls { get; set; }
    }


}
