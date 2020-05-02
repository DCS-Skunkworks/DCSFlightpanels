using System.Text.RegularExpressions;

namespace DCS_BIOS
{
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
