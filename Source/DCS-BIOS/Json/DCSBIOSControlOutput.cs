namespace DCS_BIOS
{
    // ReSharper disable All
    /*
     * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
     */
    public class DCSBIOSControlOutput
    {
        private string _type;
        public uint address { get; set; }
        public string description { get; set; }
        public uint mask { get; set; }
        public int max_value { get; set; }
        public int shift_by { get; set; }
        public string suffix { get; set; }
        public int max_length { get; set; }

        public string type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (_type.Equals("string"))
                {
                    OutputDataType = DCSBiosOutputType.STRING_TYPE;
                }
                if (_type.Equals("integer"))
                {
                    OutputDataType = DCSBiosOutputType.INTEGER_TYPE;
                }
            }
        }

        public DCSBiosOutputType OutputDataType { get; set; }
    }
}
