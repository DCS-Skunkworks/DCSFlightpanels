namespace ClassLibraryCommon
{
    using ClassLibraryCommon.Enums;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DCSFPProfile
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        private static object _lock = new object();
        private static readonly List<DCSFPModule> ModulesList = new();
        public static DCSFPModule SelectedProfile { get; set; }

        public static void Init()
        {
            AddInternalModules();
        }

        public static List<DCSFPModule> Modules
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
                if (!ModulesList.Exists(o => o.ID == (int)ManagedModule.NoFrameLoadedYet))
                {
                    var module = new DCSFPModule(1, "NoFrameLoadedYet", "NOFRAMELOADEDYET");
                    ModulesList.Add(module);
                }

                if (!ModulesList.Exists(o => o.ID == (int)ManagedModule.KeyEmulator))
                {
                    var module = new DCSFPModule(2, "Key Emulation", "KEYEMULATOR");
                    ModulesList.Add(module);
                }
            }
        }

        public static void FillModulesListFromDcsBios(string dcsbiosJsonFolder)
        {
            var modules = new DcsBiosLuaFileReader().GetModulesListFromDcsBiosLua(dcsbiosJsonFolder);
            foreach (var module in modules)
            {
                lock (_lock)
                {
                    ModulesList.Add(module);
                }
            }      
        }

        private static void LogErrorAndThrowException(string message)
        {
            logger.Error(message);
            throw new Exception(message);
        }

        public static DCSFPModule GetProfile(int id)
        {
            var module = Modules.FirstOrDefault(x => x.ID == id);
            if (module == null)
            {
                LogErrorAndThrowException($"Failed to determine profile ID ({id}) in your bindings file.");
            }
            return module;
        }

        public static void SetNoFrameLoadedYetAsProfile()
        {
            var module = Modules.FirstOrDefault(x => x.IsModule(ManagedModule.NoFrameLoadedYet));
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}");
            }

            SelectedProfile = module;
        }


        public static DCSFPModule GetNoFrameLoadedYet()
        {
            var module = Modules.FirstOrDefault(x => x.IsModule(ManagedModule.NoFrameLoadedYet));
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module NoFrameLoadedYet. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static DCSFPModule GetKeyEmulator()
        {
            var module = Modules.FirstOrDefault(x => x.IsModule(ManagedModule.KeyEmulator));
            if (module == null)
            {
                LogErrorAndThrowException($"DCSFPProfile : Failed to find internal module KeyEmulator. Modules loaded : {Modules.Count}");
            }
            return module;
        }

        public static bool HasNS430()
        {
            return Modules.Exists(x => x.IsModule(ManagedModule.NS430));
        }

        public static DCSFPModule GetNS430()
        {
            return Modules.FirstOrDefault(x => x.IsModule(ManagedModule.NS430));
        }

        public static DCSFPModule GetBackwardCompatible(string oldEnumValue)
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
                LogErrorAndThrowException("Failed to determine  profile ID (null) in your bindings file.");
                return null; //just to avoid compilation problem "error CS0161 not all code paths return a value"
            }
        }
    }
}
