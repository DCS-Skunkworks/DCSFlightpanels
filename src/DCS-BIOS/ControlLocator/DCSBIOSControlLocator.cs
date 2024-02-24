using System.Globalization;
using DCS_BIOS.Json;
using DCS_BIOS.StringClasses;


namespace DCS_BIOS.ControlLocator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ClassLibraryCommon;
    using misc;
    using Serialized;
    using Newtonsoft.Json;
    using NLog;

    /// <summary>
    /// Reads the aircraft's / helicopter's JSON file containing the cockpit controls.
    /// Whenever a class needs a specific DCS-BIOS control it asks for the control using
    /// this centralized class. There are separate functions for getting an input or output control.
    /// </summary>
    public static class DCSBIOSControlLocator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static List<DCSBIOSControl> _dcsbiosControls = new();
        private static readonly object LockObject = new();
        private static DCSAircraft _dcsAircraft;
        private static string _jsonDirectory;
        private const string DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE = "Error loading DCS-BIOS JSON. Check that the DCS-BIOS location setting points to the JSON directory.";

        public static DCSAircraft DCSAircraft
        {
            get => _dcsAircraft;
            set
            {
                if (_dcsAircraft != value)
                {
                    _dcsAircraft = value;
                    LuaAssistant.DCSAircraft = _dcsAircraft;
                    Reset();
                }
            }
        }

        public static string JSONDirectory
        {
            get => _jsonDirectory;
            set
            {
                _jsonDirectory = Environment.ExpandEnvironmentVariables(value);
                LuaAssistant.JSONDirectory = _jsonDirectory;
            }
        }

        public static DCSBIOSControl GetControl(string controlId)
        {
            lock (LockObject)
            {
                if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    return null;
                }

                try
                {
                    LoadControls();

                    if (!_dcsbiosControls.Exists(controlObject => controlObject.Identifier.Equals(controlId)))
                    {
                        throw new Exception($"Error, control {controlId} does not exist. ({DCSAircraft.Description})");
                    }

                    return _dcsbiosControls.Single(controlObject => controlObject.Identifier.Equals(controlId));
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception($"Check DCS-BIOS version. Failed to find control {controlId} for airframe {DCSAircraft.Description} ({DCSAircraft.JSONFilename}). Did you switch airframe type for the profile and have existing control(s) for the previous type saved?{Environment.NewLine}{ioe.Message}");
                }
            }
        }

        public static DCSBIOSOutput GetDCSBIOSOutput(string controlId, DCSBiosOutputType dcsBiosOutputType)
        {
            return dcsBiosOutputType == DCSBiosOutputType.IntegerType ? GetUIntDCSBIOSOutput(controlId) : GetStringDCSBIOSOutput(controlId);
        }

        public static DCSBIOSOutput GetUIntDCSBIOSOutput(string controlId)
        {
            lock (LockObject)
            {
                if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    throw new Exception("DCSBIOSControlLocator.GetDCSBIOSOutput() Should not be called when only key emulator is active");
                }

                try
                {
                    var control = GetControl(controlId);
                    var dcsBIOSOutput = new DCSBIOSOutput();
                    dcsBIOSOutput.Consume(control, DCSBiosOutputType.IntegerType);
                    return dcsBIOSOutput;
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception($"Check DCS-BIOS version. Failed to create DCSBIOSOutput based on control {controlId} for profile {DCSAircraft.JSONFilename}{Environment.NewLine}{ioe.Message}");
                }
            }
        }

        public static DCSBIOSOutput GetStringDCSBIOSOutput(string controlId)
        {
            lock (LockObject)
            {
                if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    throw new Exception("DCSBIOSControlLocator.GetDCSBIOSOutput() Should not be called when only key emulator is active");
                }

                try
                {
                    var control = GetControl(controlId);
                    var dcsBIOSOutput = new DCSBIOSOutput();
                    dcsBIOSOutput.Consume(control, DCSBiosOutputType.StringType);
                    DCSBIOSStringManager.AddListeningAddress(dcsBIOSOutput);
                    return dcsBIOSOutput;
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception($"Check DCS-BIOS version. Failed to create DCSBIOSOutput based on control {controlId} for profile {DCSAircraft.JSONFilename}{Environment.NewLine}{ioe.Message}");
                }
            }
        }

        /// <summary>
        /// Simple loading of module's controls regardless of current context (chosen airframe, profile etc.).
        /// Member DCSAircraft is not taken into account.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static List<DCSBIOSControl> GetModuleControlsFromJson(string filename, bool onlyDirectResult = false)//variable name is terrible, should be changed.
        {
            var result = new List<DCSBIOSControl>();

            try
            {
                lock (LockObject)
                {
                    var directoryInfo = new DirectoryInfo(_jsonDirectory);
                    IEnumerable<FileInfo> files;
                    try
                    {
                        files = directoryInfo.EnumerateFiles(filename, SearchOption.TopDirectoryOnly);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to find DCS-BIOS json files. -> {Environment.NewLine}{ex.Message}");
                    }

                    foreach (var file in files)
                    {
                        var controls = ReadControlsFromDocJson(file.FullName);
                        if (!onlyDirectResult)
                        {
                            _dcsbiosControls.AddRange(controls);
                        }
                        result.AddRange(controls);
                    }

                    DCSBIOSAircraftLoadStatus.SetLoaded(filename, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return result;
        }

        /// <summary>
        /// Loads meta controls and returns them in a list.
        /// </summary>
        public static List<DCSBIOSControl> GetMetaControls()
        {
            var controlList = GetModuleControlsFromJson(DCSAircraft.DCSBIOS_META_DATA_START_FILE_NAME, true);
            controlList.AddRange(GetModuleControlsFromJson(DCSAircraft.DCSBIOS_META_DATA_END_FILE_NAME, true));
            controlList.AddRange(GetModuleControlsFromJson(DCSAircraft.DCSBIOS_COMMON_DATA_FILE_NAME, true));
            return controlList;
        }

        public static IEnumerable<DCSBIOSControl> GetControls()
        {
            LoadControls();

            // Remove duplicates which may come from loading NS430 or other additional profiles
            return _dcsbiosControls.Distinct(new DCSBIOSControlComparer()).ToList();
        }

        public static IEnumerable<DCSBIOSControl> GetOutputControls(DCSBiosOutputType dcsBiosOutputType)
        {
            /*"CM_CHAFFCNT_DISPLAY": {
                   "category": "Countermeasures",
               "control_type": "display",
                "description": "Chaff Counter",
                 "identifier": "CM_CHAFFCNT_DISPLAY",
                     "inputs": [  ],
                    "outputs": [ {
                                       "address": 5408,
                                   "description": "Chaff Counter",
                                    "max_length": 2, 
                                        "suffix": "",
                                          "type": "string"
                               } ]
                },*/
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                return null;
            }

            LoadControls();
            return _dcsbiosControls.Where(o => o.Outputs.Count > 0 && o.Outputs.Any(x => x.OutputDataType == dcsBiosOutputType));
        }

        public static IEnumerable<DCSBIOSControl> GetInputControls()
        {
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled))
            {
                return null;
            }

            LoadControls();
            return _dcsbiosControls.Where(controlObject => controlObject.Inputs.Count > 0);
        }

        public static List<Tuple<string, DCSBIOSControl>> GlobalControlSearch(string keyword)
        {
            var returnList = new List<Tuple<string, DCSBIOSControl>>();
            try
            {

                lock (LockObject)
                {
                    var directoryInfo = new DirectoryInfo(_jsonDirectory);
                    IEnumerable<FileInfo> files;
                    try
                    {
                        files = directoryInfo.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to find DCS-BIOS json files. -> {Environment.NewLine}{ex.Message}");
                    }

                    foreach (var file in files)
                    {
                        var controls = ReadControlsFromDocJson(file.FullName);

                        if(controls == null || controls.Count == 0)  continue; 

                        foreach (var dcsbiosControl in controls)
                        {
                            if(dcsbiosControl.Identifier.ToLower(CultureInfo.InvariantCulture).Contains(keyword.ToLower(CultureInfo.InvariantCulture)) ||
                               dcsbiosControl.Description.ToLower(CultureInfo.InvariantCulture).Contains(keyword.ToLower(CultureInfo.InvariantCulture)))
                            {
                                returnList.Add(new Tuple<string, DCSBIOSControl>(file.Name, dcsbiosControl));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return returnList;
        }

        public static string GetLuaCommand(string dcsbiosIdentifier, bool includeSignature)
        {
            return LuaAssistant.GetLuaCommand(dcsbiosIdentifier, includeSignature);
        }

        /// <summary>
        /// Checks DCSAircraft and EmulationMode loads appropriate modules (NS430 or FC3 +/- Meta modules).
        /// Set DCSAircraft and JSON path before calling this.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void LoadControls()
        {
            /*
             * Load profile for current airframe
             * Remove/Add NS430
             * Load Common Data
             * Load Metadata End
             *
             * This function will have to change when (if) new types of profiles are added to DCS-BIOS
             */


            /*
             * Check if already loaded.
             */
            if (_dcsbiosControls.Count > 0)
            {
                return;
            }

            try
            {
                if (DCSAircraft.IsNoFrameLoadedYet(_dcsAircraft) ||
                    Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    return;
                }

                if (!Common.IsEmulationModesFlagSet(EmulationMode.NS430Enabled) && DCSAircraft.HasNS430())
                {
                    if (DCSBIOSAircraftLoadStatus.IsLoaded(DCSAircraft.GetNS430().JSONFilename))
                    {
                        //Discard all DCS-BIOS controls if user "unloaded" NS430. Not possible to remove them specifically
                        //Better to force load all controls
                        Reset();
                    }
                    else
                    {
                        DCSBIOSAircraftLoadStatus.Remove(DCSAircraft.GetNS430().JSONFilename);
                    }
                }

                if (Common.IsEmulationModesFlagSet(EmulationMode.NS430Enabled))
                {
                    ReadDataFromJsonFile(DCSAircraft.GetNS430().JSONFilename);
                }

                if (DCSAircraft.IsFlamingCliff(DCSAircraft))
                {
                    LoadCommonData(_jsonDirectory);
                    LoadMetaDataEnd(_jsonDirectory);
                }
                else
                {
                    LoadCommonData(_jsonDirectory);
                    LoadMetaDataEnd(_jsonDirectory);

                    // Load the controls for the actual aircraft/helicopter
                    ReadDataFromJsonFile(DCSAircraft.JSONFilename);
                }

                // Remove duplicates which may come from loading NS430 or other additional profiles
                _dcsbiosControls = _dcsbiosControls.Distinct(new DCSBIOSControlComparer()).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<==", ex);
            }
        }

        private static List<DCSBIOSControl> ReadControlsFromDocJson(string fileFullPath)
        {
            // input is a map from category string to a map from key string to control definition
            // we read it all then flatten the grand children (the control definitions)
            var input = File.ReadAllText(fileFullPath);
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, DCSBIOSControl>>>(input)!
                    .Values
                    .SelectMany(category => category.Values)
                    .ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e, "ReadControlsFromDocJson : Failed to read DCS-BIOS JSON.");
            }

            return null;
        }

        private static void LoadMetaDataEnd(string jsonDirectory)
        {
            if (DCSBIOSAircraftLoadStatus.IsLoaded(DCSAircraft.DCSBIOS_META_DATA_END_FILE_NAME) || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly) || DCSAircraft.IsNoFrameLoadedYet(_dcsAircraft))
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    _dcsbiosControls.AddRange(ReadControlsFromDocJson(jsonDirectory + $"\\{DCSAircraft.DCSBIOS_META_DATA_END_FILE_NAME}"));

                    DCSBIOSAircraftLoadStatus.SetLoaded(DCSAircraft.DCSBIOS_META_DATA_END_FILE_NAME, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE} ==>[{jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private static void LoadCommonData(string jsonDirectory)
        {
            if (DCSBIOSAircraftLoadStatus.IsLoaded(DCSAircraft.DCSBIOS_COMMON_DATA_FILE_NAME) || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly) || DCSAircraft.IsNoFrameLoadedYet(_dcsAircraft))
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    _dcsbiosControls.AddRange(ReadControlsFromDocJson(jsonDirectory + $"\\{DCSAircraft.DCSBIOS_COMMON_DATA_FILE_NAME}"));
                    DCSBIOSAircraftLoadStatus.SetLoaded(DCSAircraft.DCSBIOS_COMMON_DATA_FILE_NAME, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE} ==>[{jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private static void ReadDataFromJsonFile(string filename)
        {
            if (DCSBIOSAircraftLoadStatus.IsLoaded(filename) || filename == DCSAircraft.GetKeyEmulator().JSONFilename || filename == DCSAircraft.GetNoFrameLoadedYet().JSONFilename)
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    var directoryInfo = new DirectoryInfo(_jsonDirectory);
                    IEnumerable<FileInfo> files;
                    try
                    {
                        files = directoryInfo.EnumerateFiles(filename, SearchOption.TopDirectoryOnly);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to find DCS-BIOS json files. -> {Environment.NewLine}{ex.Message}");
                    }

                    foreach (var file in files)
                    {
                        var controls = ReadControlsFromDocJson(file.FullName);
                        _dcsbiosControls.AddRange(controls);
                    }

                    DCSBIOSAircraftLoadStatus.SetLoaded(filename, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_JSON_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private static void Reset()
        {
            DCSBIOSAircraftLoadStatus.Clear();
            _dcsbiosControls.Clear();
            LuaAssistant.Reset();
        }
    }
}
