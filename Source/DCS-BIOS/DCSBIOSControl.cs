using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DCS_BIOS
{
    public class DCSBIOSControlInput
    {
        public string description { get; set; }
        public string @interface { get; set; }
        public int? max_value { get; set; }
        public string argument { get; set; }
    }

    public class DCSBIOSControlOutput
    {
        private string _type;
        public uint address { get; set; }
        public string description { get; set; }
        public uint mask { get; set; }
        public int max_value { get; set; }
        public int shift_by { get; set; }
        public string suffix { get; set; }

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

    public static class DCSBIOSJsonFormatterVersion1
    {

        public static string Format(string fileText)
        {
            //make it easier to know what line endings are used.
            var result = Regex.Replace(fileText, "\r", "");
            //Replacing all dangling Control Type Strings  e.g. "ADI_CRSWARN_FLAG": {  , next line contains "category" or "api_variant"  [POSITIVE LOOKAHEAD]
            var strRegex = @"^[\s]+""[A-Za-z0-9_\s()-:]+"":\s\{(?=\n^[\s]+""category"")";
            result = Regex.Replace(result, strRegex, "                                                               {", RegexOptions.Multiline | RegexOptions.CultureInvariant);
            strRegex = @"^[\s]+""[A-Za-z0-9_\s()-:]+"":\s\{(?=\n^[\s]+""api_variant"")";
            result = Regex.Replace(result, strRegex, "                                                               {", RegexOptions.Multiline | RegexOptions.CultureInvariant);

            //Replacing first category entry with array start of DCSBIOSControls[
            strRegex = @"^[\s]+""[A-Za-z0-9_\s()-:]+"":\s\{(?<=^\{\n[\s]*""[A-Za-z0-9_\s]+"":\s\{)";
            result = Regex.Replace(result, strRegex, "   	\"DCSBIOSControls\": [", RegexOptions.Multiline | RegexOptions.CultureInvariant);

            //Replacing all trailing category entries
            strRegex = @"},\n^[\s]+""[A-Za-z0-9_\s()-:&]+"":\s\{(?=\n^[\s]+\{)";
            result = Regex.Replace(result, strRegex, "                                 ,", RegexOptions.Multiline | RegexOptions.CultureInvariant);

            //Add array end to the end of the file
            strRegex = @"}(?=\n^\})";
            result = Regex.Replace(result, strRegex, "         ]", RegexOptions.Multiline | RegexOptions.CultureInvariant);

            return result;
        }
    }
}
