using System;
using System.Collections.Generic;
using System.IO;

namespace ClassLibraryCommon
{
    public class DcsBiosLuaFileReader
    {
        public List<DCSFPProfile> GetModulesListFromDcsBiosLua(string dcsbiosJsonFolder)
        {
            List<DCSFPProfile> dCSFPProfiles= new();
            var biosLua = Path.Combine(dcsbiosJsonFolder, "..\\..\\", "BIOS.lua");

            if (!File.Exists(biosLua))
            {
                return dCSFPProfiles; // Empty list
            }

            var stringArray = File.ReadAllLines(biosLua);

            // dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]]) -- ID = 5, ProperName = A-10C Thunderbolt II
            foreach (var s in stringArray)
            {
                if (!s.StartsWith("--") && s.ToLower().Contains(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower()) && s.Contains("ProperName"))
                {
                    var parts = s.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);

                    // dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]])
                    var json = parts[0].ToLower().Replace(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower(), string.Empty).Replace(".lua]])", string.Empty).Trim() + ".json";

                    // ID = 5, ProperName = A-10C Thunderbolt II
                    var info = parts[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    // ID = 5
                    var id = int.Parse(info[0].Split(new[] { "=" }, StringSplitOptions.None)[1]);

                    // ProperName = A-10C Thunderbolt II
                    var properName = info[1].Split(new[] { "=" }, StringSplitOptions.None)[1].Trim();

                    dCSFPProfiles.Add(new DCSFPProfile(id, properName, json));
                }
            }
            return dCSFPProfiles;
        }
    }
}
