namespace ClassLibraryCommon
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DCSFPProfile
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        private static object _lock = new object();
        private static readonly List<DCSFPProfile> ModulesList = new();

        public static DCSFPProfile SelectedProfile { get; set; }

        private DCSFPProfile(int id, string description, string jsonFilename)
        {
            ID = id;
            JSONFilename = jsonFilename;
            Description = description;
        }

        public static void Init()
        {
            AddInternalModules();
        }

        public int ID { get; set; }

        public string JSONFilename { get; set; }

        public string Description { get; set; }

        public bool UseGenericRadio { get; set; } = false;

        public static List<DCSFPProfile> Modules
        {
            get
            {
                lock (_lock)
                {
                    return ModulesList;
                }
            }
        }
        
        public static int DCSBIOSModulesCount
        {
            get
            {
                lock (_lock)
                {
                    return ModulesList.Count - 2; // Two profiles are not DCS-BIOS
                }
            }
        }

        private static void AddInternalModules()
        {
            lock (_lock)
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
            }
        }

        public static void FillModulesListFromDcsBios(string dcsbiosJsonFolder)
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
                    var parts = s.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);

                    // dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\A-10C.lua]])
                    var json = parts[0].ToLower().Replace(@"dofile(lfs.writedir()..[[Scripts\DCS-BIOS\lib\".ToLower(), string.Empty).Replace(".lua]])", string.Empty).Trim() + ".json";

                    // ID = 5, ProperName = A-10C Thunderbolt II
                    var info = parts[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    // ID = 5
                    var id = int.Parse(info[0].Split(new[] { "=" }, StringSplitOptions.None)[1]);

                    // ProperName = A-10C Thunderbolt II
                    var properName = info[1].Split(new[] { "=" }, StringSplitOptions.None)[1].Trim();

                    lock (_lock)
                    {
                        ModulesList.Add(new DCSFPProfile(id, properName, json));
                    }
                }
            }
        }

        private static void LogErrorAndThrowException(string message)
        {
            logger.Error(message);
            throw new Exception(message);
        }

        public static DCSFPProfile GetProfile(int id)
        {
            var module = Modules.FirstOrDefault(x => x.ID == id);
            if (module == null)
            {
                LogErrorAndThrowException("Failed to determine profile ID (" + id + ") in your bindings file. Please check file BIOS.lua and update your bindings file. Example : Profile=5 equals A-10C.");
            }
            return module;
        }

        public static void SetNoFrameLoadedYetAsProfile()
        {
            var module = Modules.FirstOrDefault(x => IsNoFrameLoadedYet(x));
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}");
            }

            SelectedProfile = module;
        }


        public static DCSFPProfile GetNoFrameLoadedYet()
        {
            var module = Modules.FirstOrDefault(x => IsNoFrameLoadedYet(x));
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static DCSFPProfile GetKeyEmulator()
        {
            var module = Modules.FirstOrDefault(x => IsKeyEmulator(x));
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module KeyEmulator. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static bool HasNS430()
        {
            return Modules.Exists(x => IsNS430(x));
        }

        public static DCSFPProfile GetNS430()
        {
            return Modules.FirstOrDefault(x => IsNS430(x));
        }


        public static bool IsNoFrameLoadedYet(DCSFPProfile dcsfpModule)
        {
            /*if (dcsfpModule == null)
            {
                LogErrorAndThrowException("DCSFPProfile IsNoFrameLoadedYet : Parameter dcsfpModule is null.");
            }*/
            return dcsfpModule.ID == 1;
        }

        public static bool IsKeyEmulator(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 2;
        }

        public static bool IsFlamingCliff(DCSFPProfile dcsfpModule)
        {
            return dcsfpModule.ID == 4;
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
            int? moduleNumber = oldEnumValue switch
            {
                "KEYEMULATOR" => 2,
                "A4E" => 6,
                "A10C" => 5,
                "AH6J" => 7,
                "AJS37" => 8,
                "Alphajet" => 9,
                "AV8BNA" => 10,
                "Bf109" => 11,
                "C101CC" => 12,
                "ChristenEagle" => 14,
                "Edge540" => 15,
                "F5E" => 18,
                "F14B" => 16,
                "F16C" => 17,
                "FA18C" => 20,
                "F86F" => 19,
                "FC3" => 4,
                "Fw190a8" => 21,
                "Fw190d9" => 22,
                "Hercules" => 13,
                "I16" => 23,
                "JF17" => 24,
                "Ka50" => 25,
                "L39ZA" => 26,
                "M2000C" => 27,
                "MB339" => 28,
                "Mi8" => 29,
                "Mig15bis" => 30,
                "Mig19P" => 31,
                "Mig21Bis" => 32,
                "NS430" => 33,
                "P51D" => 35,
                "P47D" => 34,
                "SA342M" => 36,
                "SpitfireLFMkIX" => 37,
                "UH1H" => 38,
                "Yak52" => 39,
                _ => null
            };
            if (moduleNumber != null)
            {
                return Modules.Find(o => o.ID == moduleNumber);
            }
            else
            {
                LogErrorAndThrowException("Failed to determine  profile ID (null) in your bindings file. Please check file BIOS.lua and update your bindings file. Example : Profile=20 equals F-18C");
                return null; //just to avoid compilation problem "error CS0161 not all code paths return a value"
            }
        }
    }
}
