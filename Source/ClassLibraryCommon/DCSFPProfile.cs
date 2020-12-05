using System;
using System.Collections.Generic;
using System.IO;

namespace ClassLibraryCommon
{
    public class DCSFPProfile
    {
        private int _id;
        private string _description;
        private string _jsonFilename;
        private static readonly List<DCSFPProfile> ModulesList = new List<DCSFPProfile>();



        public DCSFPProfile(int id, string description, string jsonFilename)
        {
            _id = id;
            _jsonFilename = jsonFilename;
            _description = description;
        }

        public int ID
        {
            get => _id;
            set => _id = value;
        }

        public string JSONFilename
        {
            get => _jsonFilename;
            set => _jsonFilename = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public static List<DCSFPProfile> Modules => ModulesList;

        public static void ParseSettings(string dcsbiosJsonFolder, string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }

            var stringArray = File.ReadAllLines(filename);

            foreach (var s in stringArray)
            {
                if (string.IsNullOrEmpty(s) || s.Trim().StartsWith("#"))
                {
                    continue;
                }

                var settings = s.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var dcsBiosModule = new DCSFPProfile(int.Parse(settings[0]), settings[1].Trim(), settings[2].Trim());
                ModulesList.Add(dcsBiosModule);
            }

            var biosLua = Path.Combine(dcsbiosJsonFolder, "..\\..\\", "BIOS.lua");

            if (!File.Exists(biosLua))
            {
                return;
            }
            stringArray = File.ReadAllLines(biosLua);
            //dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]]) -- ID = 5, ProperName = A-10C Thunderbolt II

            foreach (var s in stringArray)
            {
                if (s.ToLower().Contains(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower()) && s.Contains("ProperName"))
                {
                    var parts = s.Split(new string[]{"--"}, StringSplitOptions.RemoveEmptyEntries);
                    //dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]])
                    var json = parts[0].ToLower().Replace(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower(), "").Replace(".lua]])", "").Trim() + ".json";

                    //ID = 5, ProperName = A-10C Thunderbolt II
                    var info = parts[1].Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    //ID = 5
                    var id = int.Parse(info[0].Split(new[] {"="}, StringSplitOptions.None)[1]);
                    //ProperName = A-10C Thunderbolt II
                    var properName = info[1].Split(new[] {"="}, StringSplitOptions.None)[1].Trim();

                    var dcsFPProfile = new DCSFPProfile(id, properName, json);
                    ModulesList.Add(dcsFPProfile);
                }
            }
        }

        public static DCSFPProfile GetProfile(int id)
        {
            foreach (var dcsfpModule in Modules)
            {
                if (dcsfpModule.ID == id)
                {
                    return dcsfpModule;
                }
            }

            throw new Exception("Failed to determine airplane/helicopter in your bindings file.\nPlease check file [Settings\\dcsfp_profiles.txt] and update your bindings file line : [Profile=] with the correct [JSON File] from [Settings\\dcsfp_profiles.txt]");
        }

        public static DCSFPProfile GetProfileBackwardCompat(string oldEnumValue)
        {
            foreach (var dcsfpModule in Modules)
            {
                var description = dcsfpModule.Description.ToLower().Replace(" ", "").Replace("-", "");
                if (description.Contains(oldEnumValue.ToLower().Replace("-", "")))
                {
                    return dcsfpModule;
                }
            }

            throw new Exception("Failed to determine airplane/helicopter in your bindings file.\nPlease check file [Settings\\dcsfp_profiles.txt] and update your bindings file line : [Airframe=] with the correct [description] from [Settings\\dcsfp_profiles.txt]");
        }

        public static bool IsNoFrameLoadedYet(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 1;
        }

        public static DCSFPProfile GetNoFrameLoadedYet()
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsNoFrameLoadedYet(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }

        public static bool IsKeyEmulator(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 2;
        }
        
        public static DCSFPProfile GetKeyEmulator()
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsKeyEmulator(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }
        
        public static bool IsKeyEmulatorSRS(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 3;
        }

        public static DCSFPProfile GetKeyEmulatorSRS()
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsKeyEmulatorSRS(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }
        
        

        public static bool IsFlamingCliff(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 4;
        }

        public static DCSFPProfile GetNS430()
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsNS430(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }

        public static bool IsA10C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 5;
        }

        public static bool IsA4E(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 6;
        }

        public static bool IsAH6(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 7;
        }

        public static bool IsAJS37(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 8;
        }

        public static bool IsAlphajet(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 9;
        }

        public static bool IsAV8B(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 10;
        }

        public static bool IsBf109K4(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 11;
        }

        public static bool IsC101(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 12;
        }

        public static bool IsC130(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 13;
        }

        public static bool IsChristenEagleII(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 14;
        }

        public static bool IsEdge540(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 15;
        }

        public static bool IsF14B(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 16;
        }

        public static bool IsF16C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 17;
        }

        public static bool IsF5E(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 18;
        }

        public static bool IsF86F(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 19;
        }

        public static bool IsFA18C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 20;
        }

        public static bool IsFW190A8(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 21;
        }

        public static bool IsFW190D9(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 22;
        }

        public static bool IsI16(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 23;
        }

        public static bool IsJF17(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 24;
        }

        public static bool IsKa50(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 25;
        }

        public static bool IsL39(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 26;
        }

        public static bool IsM2000C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 27;
        }

        public static bool IsMB339PAN(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 28;
        }

        public static bool IsMi8MT(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 29;
        }

        public static bool IsMiG15bis(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 30;
        }

        public static bool IsMiG19P(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 31;
        }

        public static bool IsMiG21Bis(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 32;
        }

        public static bool IsNS430(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 33;
        }

        public static bool IsP47D(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 34;
        }

        public static bool IsP51D(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 35;
        }

        public static bool IsSA342(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 36;
        }

        public static bool IsSpitfireLFMkIX(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 37;
        }

        public static bool IsUH1H(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 38;
        }

        public static bool IsYak52(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 39;
        }
    }
}
