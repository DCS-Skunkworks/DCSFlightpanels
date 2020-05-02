namespace DCS_BIOS
{
    // ReSharper disable All
    /*
     * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
     */
    public class DCSBIOSControlInput
    {
        public string description { get; set; }
        public string @interface { get; set; }
        public int? max_value { get; set; }
        public string argument { get; set; }
    }
}
