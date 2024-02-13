namespace ClassLibraryCommon
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Holds information about currently selected aircraft / module (DCS aircraft/helicopter).
    /// This class reads all modules from dcs-bios_modules.txt and the user can then select between these
    /// when creating a new DCSFP profile.
    /// </summary>
    public class DCSAircraft
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly object Lock = new();
        private static List<DCSAircraft> _modulesList = new();

        public static DCSAircraft SelectedAircraft { get; set; }
        public const string DCSBIOS_META_DATA_START_FILE_NAME = "MetadataStart.json";
        public const string DCSBIOS_META_DATA_END_FILE_NAME = "MetadataEnd.json";
        public const string DCSBIOS_COMMON_DATA_FILE_NAME = "CommonData.json";
        private const string DCSBIOS_META_DATA_START_MODULE = "MetadataStart";
        private const string DCSBIOS_META_DATA_END_MODULE = "MetadataEnd";
        private const string DCSBIOS_COMMON_DATA_MODULE = "CommonData";

        private DCSAircraft(int id, string description, string jsonFilename)
        {
            ID = id;
            JSONFilename = jsonFilename;
            Description = description;
        }

        public static void Init()
        {
            AddInternalModules();
        }

        public int ID { get; }

        public string JSONFilename { get; }

        public string LuaFilename
        {
            get => JSONFilename.Replace(".json", ".lua");
        }

        public bool IsMetaModule
        {
            get => JSONFilename.Contains(DCSBIOS_META_DATA_END_MODULE) || JSONFilename.Contains(DCSBIOS_META_DATA_START_MODULE) || JSONFilename.Contains(DCSBIOS_COMMON_DATA_MODULE);
        }

        /// <summary>
        /// This is not exact science
        /// </summary>
        public string ModuleLuaName
        {
            get => JSONFilename.Replace(".json", "").Replace("-", "_").Replace(" ", "_");
        }

        public string DCSName
        {
            get => JSONFilename.Replace(".json", "");
        }

        public string Description { get; }

        public bool UseGenericRadio { get; set; }

        /// <summary>
        /// This can be used to any purpose.
        /// Right now used that is true then A-10C II, false => A-10C.
        /// They have differing radios which means different pre-programmed radios.
        /// </summary>
        public bool Option1 { get; set; }

        public static List<DCSAircraft> Modules
        {
            get
            {
                lock (Lock)
                {
                    return _modulesList;
                }
            }
        }

        public static bool HasDCSBIOSModules
        {
            get
            {
                lock (Lock)
                {
                    return _modulesList.Count - 3 > 0; // Three modules are not DCS-BIOS
                }
            }
        }

        public static int DCSBIOSModulesCount
        {
            get
            {
                lock (Lock)
                {
                    return _modulesList.Count - 3; // Three modules are not DCS-BIOS
                }
            }
        }

        private static void AddInternalModules()
        {
            lock (Lock)
            {
                if (!_modulesList.Exists(o => o.ID == 1))
                {
                    var module = new DCSAircraft(1, "NoFrameLoadedYet", "NOFRAMELOADEDYET");
                    _modulesList.Add(module);
                }

                if (!_modulesList.Exists(o => o.ID == 2))
                {
                    var module = new DCSAircraft(2, "Key Emulation", "KEYEMULATOR");
                    _modulesList.Add(module);
                }
                if (!_modulesList.Exists(o => o.ID == 3))
                {
                    var module = new DCSAircraft(3, "Key Emulation with SRS support", "KEYEMULATOR_SRS");
                    _modulesList.Add(module);
                }
            }
        }

        public static void FillModulesListFromDcsBios(string dcsbiosJsonFolder, bool loadInternalModules)
        {
            lock (Lock)
            {
                _modulesList.Clear();
                if (loadInternalModules)
                {
                    AddInternalModules();
                }
            }

            var dcsbiosConfigFile = $"{AppDomain.CurrentDomain.BaseDirectory}dcs-bios_modules.txt";
            if (!File.Exists(dcsbiosConfigFile))
            {
                LogErrorAndThrowException($"Failed to find {dcsbiosConfigFile} in base directory.");
                return;
            }

            var result = Common.CheckJSONDirectory(dcsbiosJsonFolder);
            if (result.Item1 == false && result.Item2 == false)
            {
                Logger.Error("Failed to find DCS-BIOS JSON location.");
                return;
            }

            var stringArray = File.ReadAllLines(dcsbiosConfigFile);

            //Inject these static DCS-BIOS modules

            lock (Lock)
            {
                _modulesList.Add(new DCSAircraft(500, DCSBIOS_META_DATA_END_MODULE, DCSBIOS_META_DATA_END_FILE_NAME));
                _modulesList.Add(new DCSAircraft(501, DCSBIOS_META_DATA_START_MODULE, DCSBIOS_META_DATA_START_FILE_NAME));
                _modulesList.Add(new DCSAircraft(502, DCSBIOS_COMMON_DATA_MODULE, DCSBIOS_COMMON_DATA_FILE_NAME));
            }

            // A-10C|5|A-10C Thunderbolt/II
            foreach (var s in stringArray)
            {
                if (s.StartsWith("--") || !s.Contains('|'))
                {
                    continue;
                }

                var parts = s.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);


                var json = parts[0] + ".json";
                var id = int.Parse(parts[1]);
                var properName = parts[2];

                lock (Lock)
                {
                    _modulesList.Add(new DCSAircraft(id, properName, json));
                }
            }

            lock (Lock)
            {
                _modulesList = _modulesList.OrderBy(o => o.Description).ToList();
            }
        }

        private static void LogErrorAndThrowException(string message)
        {
            Logger.Error(message);
            throw new Exception(message);
        }

        public static DCSAircraft GetAircraft(int id)
        {
            var module = Modules.FirstOrDefault(x => x.ID == id);
            if (module == null)
            {
                LogErrorAndThrowException($"Failed to determine aircraft ID ({id}) in your bindings file.");
            }
            return module;
        }

        public static void SetNoFrameLoadedYetAsProfile()
        {
            var module = Modules.FirstOrDefault(IsNoFrameLoadedYet);
            if (module == null)
            {
                LogErrorAndThrowException($"DCSAircraft : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}");
            }

            SelectedAircraft = module;
        }


        public static DCSAircraft GetNoFrameLoadedYet()
        {
            var module = Modules.FirstOrDefault(IsNoFrameLoadedYet);
            if (module == null)
            {
                LogErrorAndThrowException($"DCSAircraft : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static DCSAircraft GetKeyEmulator()
        {
            var module = Modules.FirstOrDefault(IsKeyEmulator);
            if (module == null)
            {
                LogErrorAndThrowException($"DCSAircraft : Failed to find internal module KeyEmulator. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static DCSAircraft GetKeyEmulatorSRS()
        {
            var module = Modules.FirstOrDefault(IsKeyEmulatorSRS);
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module KeyEmulatorSRS. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static bool HasNS430()
        {
            return Modules.Exists(IsNS430);
        }

        public static DCSAircraft GetNS430()
        {
            return Modules.FirstOrDefault(IsNS430);
        }


        public static bool IsNoFrameLoadedYet(DCSAircraft dcsfpModule)
        {
            /*if (dcsfpModule == null)
            {
                LogErrorAndThrowException("DCSAircraft IsNoFrameLoadedYet : Parameter dcsfpModule is null.");
            }*/
            return dcsfpModule.ID == 1;
        }

        public static bool IsKeyEmulator(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 2;
        }

        public static bool IsKeyEmulatorSRS(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 3;
        }

        public static bool IsFlamingCliff(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 4;
        }

        public static bool IsA10C(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 5;
        }

        public static bool IsA4E(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 6;
        }

        public static bool IsAH6(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 7;
        }

        public static bool IsAJS37(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 8;
        }

        public static bool IsAlphajet(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 9;
        }

        public static bool IsAV8B(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 10;
        }

        public static bool IsBf109K4(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 11;
        }

        public static bool IsC101(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 12;
        }

        public static bool IsC130(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 13;
        }

        public static bool IsChristenEagleII(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 14;
        }

        public static bool IsEdge540(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 15;
        }

        public static bool IsF14B(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 16;
        }

        public static bool IsF15E(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 44;
        }

        public static bool IsF16C(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 17;
        }

        public static bool IsF5E(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 18;
        }

        public static bool IsF86F(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 19;
        }

        public static bool IsFA18C(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 20;
        }

        public static bool IsFW190A8(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 21;
        }

        public static bool IsFW190D9(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 22;
        }

        public static bool IsI16(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 23;
        }

        public static bool IsJF17(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 24;
        }

        public static bool IsKa50(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 25;
        }

        public static bool IsL39(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 26;
        }

        public static bool IsM2000C(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 27;
        }

        public static bool IsMB339PAN(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 28;
        }

        public static bool IsMi8MT(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 29;
        }

        public static bool IsMiG15bis(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 30;
        }

        public static bool IsMiG19P(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 31;
        }

        public static bool IsMiG21Bis(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 32;
        }

        public static bool IsNS430(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 33;
        }

        public static bool IsP47D(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 34;
        }

        public static bool IsP51D(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 35;
        }

        public static bool IsSA342(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 36;
        }

        public static bool IsSpitfireLFMkIX(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 37;
        }

        public static bool IsUH1H(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 38;
        }

        public static bool IsYak52(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 39;
        }
        public static bool IsT45C(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 43;
        }
        public static bool IsMi24P(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 42;
        }

        public static bool IsMosquito(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 45;
        }

        public static bool IsAH64D(DCSAircraft dcsfpModule)
        {
            return dcsfpModule.ID == 46;
        }


        public static DCSAircraft GetBackwardCompatible(string oldEnumValue)
        {
            int? moduleNumber = oldEnumValue switch
            {
                "KEYEMULATOR" => 2,
                "KEYEMULATOR_SRS" => 3,
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

            LogErrorAndThrowException("Failed to determine  profile ID (null) in your bindings file.");
            return null; //just to avoid compilation problem "error CS0161 not all code paths return a value"
        }
    }
}
