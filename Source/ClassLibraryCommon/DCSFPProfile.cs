namespace ClassLibraryCommon
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DCSFPProfile
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly List<DCSFPProfile> ModulesList = new List<DCSFPProfile>();

        private DCSFPProfile(int id, string description, string jsonFilename)
        {
            this.ID = id;
            this.JSONFilename = jsonFilename;
            this.Description = description;
        }

        public static void Init()
        {
            AddInternalModules();
        }

        public int ID { get; set; }

        public string JSONFilename { get; set; }

        public string Description { get; set; }

        public bool UseGenericRadio { get; set; } = false;

        public static List<DCSFPProfile> Modules => ModulesList;

        private static void AddInternalModules()
        {
            if (!ModulesList.Exists(o => o.ID == 1))
            {
                var module = new DCSFPProfile(1, "NoFrameLoadedYet", "NOFRAMELOADEDYET");
                ModulesList.Add(module);
            }

            if (!ModulesList.Exists(o => o.ID == 2))
            {
                var module = new DCSFPProfile(2, "Key Emulation", "KEYEMULATOR");
                ModulesList.Add(module);
            }

            if (!ModulesList.Exists(o => o.ID == 3))
            {
                var module = new DCSFPProfile(3, "Key Emulation with SRS support", "KEYEMULATOR_SRS");
                ModulesList.Add(module);
            }
        }

        public static void ParseSettings(string dcsbiosJsonFolder)
        {
            var biosLua = Path.Combine(dcsbiosJsonFolder, "..\\..\\", "BIOS.lua");

            if (!File.Exists(biosLua))
            {
                return;
            }

            var stringArray = File.ReadAllLines(biosLua);

            // dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]]) -- ID = 5, ProperName = A-10C Thunderbolt II
            foreach (var s in stringArray)
            {
                if (!s.StartsWith("--") && s.ToLower().Contains(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower()) && s.Contains("ProperName"))
                {
                    var parts = s.Split(new string[]{"--"}, StringSplitOptions.RemoveEmptyEntries);

                    // dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]])
                    var json = parts[0].ToLower().Replace(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower(), string.Empty).Replace(".lua]])", string.Empty).Trim() + ".json";

                    // ID = 5, ProperName = A-10C Thunderbolt II
                    var info = parts[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    // ID = 5
                    var id = int.Parse(info[0].Split(new[] { "=" }, StringSplitOptions.None)[1]);

                    // ProperName = A-10C Thunderbolt II
                    var properName = info[1].Split(new[] { "=" }, StringSplitOptions.None)[1].Trim();

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
            string message = "Failed to determine airplane/helicopter in your bindings file. Please check file BIOS.lua and update your bindings file. Example a line in the file equal to Profile=5 equals A-10C.";
            logger.Info(message);
            throw new Exception(message);
        }
        
        public static bool IsNoFrameLoadedYet(DCSFPProfile dcsfpModule)
        {
            if (dcsfpModule == null)
            {
                string message = "DCSFPProfile : Parameter dcsfpModule is null.";
                logger.Error(message);
                throw new Exception(message);
            }
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
            string message = $"DCSFPProfile : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}";
            logger.Error(message);
            throw new Exception(message);
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
            string message = $"DCSFPProfile : Failed to find internal module KeyEmulator. Modules loaded : {Modules.Count}";
            logger.Error(message);
            throw new Exception(message);
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
            string message = $"DCSFPProfile : Failed to find internal module KeyEmulatorSRS. Modules loaded : {Modules.Count}";
            logger.Error(message);
            throw new Exception(message);
        }
        
        public static bool IsFlamingCliff(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 4;
        }

        public static bool HasNS430()
        {
            foreach (var dcsfpModule in Modules)
            {
                if (IsNS430(dcsfpModule))
                {
                    return true;
                }
            }
            return false;
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

        public static bool IsMi24P(DCSFPProfile dcsfpModule) 
        {
            return dcsfpModule.ID == 42;
        }

        public static DCSFPProfile GetBackwardCompatible(string oldEnumValue)
        {
            if ("KEYEMULATOR".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 2);
            }

            if ("KEYEMULATOR_SRS".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 3);
            }

            if ("A4E".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 6);
            }

            if ("A10C".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 5);
            }

            if ("AH6J".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 7);
            }

            if ("AJS37".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 8);
            }

            if ("Alphajet".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 9);
            }

            if ("AV8BNA".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 10);
            }

            if ("Bf109".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 11);
            }

            if ("C101CC".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 12);
            }

            if ("ChristenEagle".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 14);
            }

            if ("Edge540".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 15);
            }

            if ("F5E".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 18);
            }

            if ("F14B".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 16);
            }

            if ("F16C".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 17);
            }

            if ("FA18C".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 20);
            }

            if ("F86F".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 19);
            }

            if ("FC3_CD_SRS".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 4);
            }

            if ("Fw190a8".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 21);
            }

            if ("Fw190d9".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 22);
            }

            if ("Hercules".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 13);
            }

            if ("I16".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 23);
            }

            if ("JF17".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 24);
            }

            if ("Ka50".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 25);
            }

            if ("L39ZA".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 26);
            }

            if ("M2000C".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 27);
            }

            if ("MB339".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 28);
            }

            if ("Mi8".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 29);
            }

            if ("Mig15bis".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 30);
            }

            if ("Mig19P".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 31);
            }

            if ("Mig21Bis".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 32);
            }

            if ("NS430".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 33);
            }

            if ("P51D".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 35);
            }

            if ("P47D".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 34);
            }

            if ("SA342M".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 36);
            }

            if ("SpitfireLFMkIX".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 37);
            }

            if ("UH1H".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 38);
            }

            if ("Yak52".Equals(oldEnumValue))
            {
                return Modules.Find(o => o.ID == 39);
            }
            string message = "Failed to determine airplane/ helicopter in your bindings file. Please check file BIOS.lua and update your bindings file. Example a line in the file equal to Profile = 5 equals A-10C.";
            logger.Info(message);
            throw new Exception(message);
        }
    }
}
