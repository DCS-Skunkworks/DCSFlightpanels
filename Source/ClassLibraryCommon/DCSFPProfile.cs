using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ClassLibraryCommon
{
    public class DCSFPProfile
    {
        private string _jsonFilename;
        private string _description;
        private static readonly List<DCSFPProfile> ModulesList = new List<DCSFPProfile>();
        


        public DCSFPProfile(string luaFilename, string description)
        {
            _jsonFilename = luaFilename;
            _description = description;
        }
        
        public static void ParseSettings(string filename)
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

                var settings = s.Trim().Split(new char[]{ ':' }, StringSplitOptions.RemoveEmptyEntries);
                //A-4E Skyhawk : A-4E-C.lua
                var dcsBiosModule = new DCSFPProfile(settings[1], settings[0]);
                ModulesList.Add(dcsBiosModule);
            }
        }

        public static DCSFPProfile GetProfile(string filename)
        {
            foreach (var dcsfpModule in Modules)
            {
                if (dcsfpModule.JSONFilename.Equals(filename))
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
                if (description.Contains(oldEnumValue.ToLower().Replace("-","")))
                {
                    return dcsfpModule;
                }
            }

            throw new Exception("Failed to determine airplane/helicopter in your bindings file.\nPlease check file [Settings\\dcsfp_profiles.txt] and update your bindings file line : [Airframe=] with the correct [description] from [Settings\\dcsfp_profiles.txt]");
        }

        public static bool IsKeyEmulatorSRS(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "KeyEmulator_SRS";
        }

        public static bool IsKeyEmulatorSRS(string filename)
        {
            return GetKeyEmulator().JSONFilename == filename;
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

        public static bool IsKeyEmulator(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "KeyEmulator";
        }

        public static bool IsKeyEmulator(string filename)
        {
            return GetKeyEmulator().JSONFilename == filename;
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

        public static bool IsNoFrameLoadedYet(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "NoFrameLoadedYet";
        }

        public static bool IsNoFrameLoadedYet(string filename)
        {
            return GetNoFrameLoadedYet().JSONFilename == filename;
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
        
        public static bool IsFlamingCliff(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Flaming Cliff 3 & SRS";
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
        
        
        public static DCSFPProfile GetL39ZA() //Needed for backward compability
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsL39(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }
        
        public static DCSFPProfile GetP51D() //Needed for backward compability
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsP51D(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }
        
        public static DCSFPProfile GetSA342M() //Needed for backward compability
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsSA342(dcsfpModule))
                {
                    return dcsfpModule;
                }
            }

            return null;
        }

        public static bool IsA10C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "A-10C Thunderbolt II";
        }

        public static bool IsA4E(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "A-4E Skyhawk";
        }

        public static bool IsAH6(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Littlebird AH-6";
        }

        public static bool IsAJS37(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "AJS 37 Viggen";
        }

        public static bool IsAlphajet(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Alphajet";
        }

        public static bool IsAV8B(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "AV-8B Night Attack";
        }

        public static bool IsBf109K4(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Bf 109 K-4 Kurfurst";
        }

        public static bool IsC101(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "C-101 Aviojet";
        }

        public static bool IsC130(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "C-130 Hercules";
        }

        public static bool IsChristenEagleII(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Christen Eagle II";
        }

        public static bool IsEdge540(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Edge 540";
        }

        public static bool IsF14B(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "F-14B Tomcat";
        }

        public static bool IsF16C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "F-16C Viper";
        }

        public static bool IsF5E(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "F-5E Tiger II";
        }

        public static bool IsF86F(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "F-86F Sabre";
        }

        public static bool IsFA18C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "FA-18C Hornet";
        }

        public static bool IsFW190A8(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Fw 190 A-8";
        }

        public static bool IsFW190D9(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Fw 190 D-9 Dora";
        }

        public static bool IsI16(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "I-16";
        }

        public static bool IsJF17(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "JF-17 Thunder";
        }

        public static bool IsKa50(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Ka-50 Black Shark";
        }

        public static bool IsL39(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "L-39 Albatros";
        }

        public static bool IsM2000C(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "M-2000C";
        }

        public static bool IsMB339PAN(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "MB-339PAN";
        }

        public static bool IsMi8MT(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Mi-8MT";
        }

        public static bool IsMiG15bis(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "MiG-15bis";
        }

        public static bool IsMiG19P(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "MiG-19P Farmer";
        }

        public static bool IsMiG21Bis(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "MiG-21Bis";
        }

        public static bool IsNS430(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "NS 430 GPS";
        }

        public static bool IsP47D(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "P-47D Thunderbolt";
        }

        public static bool IsP51D(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "P-51D Mustang";
        }

        public static bool IsSA342(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "SA342 Gazelle";
        }

        public static bool IsSpitfireLFMkIX(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Spitfire LF Mk. IX";
        }

        public static bool IsUH1H(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "UH-1H Huey";
        }

        public static bool IsYak52(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.Description == "Yak-52";
        }
    }
}
