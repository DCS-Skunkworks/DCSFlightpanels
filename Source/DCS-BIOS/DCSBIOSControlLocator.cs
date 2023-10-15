using DCS_BIOS.Json;

namespace DCS_BIOS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ClassLibraryCommon;

    using Newtonsoft.Json;
    using NLog;

    /// <summary>
    /// Reads the aircraft's / helicopter's JSON file containing the cockpit controls.
    /// Whenever a class needs a specific DCS-BIOS control it asks for the control using
    /// this class. There are separate functions for getting a input or output control.
    /// </summary>
    public static class DCSBIOSControlLocator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static List<DCSBIOSControl> DCSBIOSControls = new();
        private static readonly object LockObject = new();
        private static DCSAircraft _dcsAircraft;
        private static string _jsonDirectory;
        private const string DCSBIOS_NOT_FOUND_ERROR_MESSAGE = "Error loading DCS-BIOS. Check that the DCS-BIOS location setting points to the JSON directory.";


        public static DCSAircraft DCSAircraft
        {
            get => _dcsAircraft;
            set
            {
                if (_dcsAircraft != value)
                {
                    _dcsAircraft = value;
                    Reset();
                }
            }
        }

        public static string JSONDirectory
        {
            get => _jsonDirectory;
            set => _jsonDirectory = value;
        }

        private static void Reset()
        {
            DCSBIOSAircraftLoadStatus.Clear();
            DCSBIOSControls.Clear();
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

                    if (!DCSBIOSControls.Exists(controlObject => controlObject.Identifier.Equals(controlId)))
                    {
                        throw new Exception($"Error, control {controlId} does not exist. ({DCSAircraft.Description})");
                    }

                    return DCSBIOSControls.Single(controlObject => controlObject.Identifier.Equals(controlId));
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
        /// Simple loading, not bothered with DCSFP various key emulator stuff and such
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static List<DCSBIOSControl> ReadDataFromJsonFileSimple(string filename, bool onlyDirectResult = false)
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
                        throw new Exception($"Failed to find DCS-BIOS files. -> {Environment.NewLine}{ex.Message}");
                    }

                    foreach (var file in files)
                    {
                        var controls = ReadControlsFromDocJson(file.FullName);
                        if (!onlyDirectResult)
                        {
                            DCSBIOSControls.AddRange(controls);
                        }
                        result.AddRange(controls);
                        PrintDuplicateControlIdentifiers(controls);
                    }

                    DCSBIOSAircraftLoadStatus.SetLoaded(filename, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return result;
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
                        throw new Exception($"Failed to find DCS-BIOS files. -> {Environment.NewLine}{ex.Message}");
                    }

                    foreach (var file in files)
                    {
                        var controls = ReadControlsFromDocJson(file.FullName);
                        DCSBIOSControls.AddRange(controls);
                        PrintDuplicateControlIdentifiers(controls);
                    }

                    DCSBIOSAircraftLoadStatus.SetLoaded(filename, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Loads meta controls and returns them in a list.
        /// </summary>
        public static List<DCSBIOSControl> LoadMetaControls()
        {
            var controlList = ReadDataFromJsonFileSimple("MetadataStart.json", true);
            controlList.AddRange(ReadDataFromJsonFileSimple("MetadataEnd.json", true));
            controlList.AddRange(ReadDataFromJsonFileSimple("CommonData.json", true));
            return controlList;
        }

        public static void LoadControls()
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
            if (DCSBIOSControls.Count > 0)
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
                DCSBIOSControls = DCSBIOSControls.Distinct(new DCSBIOSControlComparer()).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<==", ex);
            }
        }

        private static void PrintDuplicateControlIdentifiers(List<DCSBIOSControl> dcsbiosControls, bool printAll = false)
        {
            List<string> result = new();
            List<string> dupes = new();
            foreach (var dcsbiosControl in dcsbiosControls)
            {
                if (printAll)
                {
                    result.Add(dcsbiosControl.Identifier);
                }

                // Debug.Print(dcsbiosControl.identifier);
                var found = false;
                foreach (var str in result)
                {
                    if (str.Trim() == dcsbiosControl.Identifier.Trim())
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    result.Add(dcsbiosControl.Identifier);
                }

                if (found)
                {
                    dupes.Add(dcsbiosControl.Identifier);
                }
            }

            if (dupes.Count > 0)
            {
                StringBuilder message = new();
                message.AppendLine($"Below is a list of duplicate identifiers found in the {DCSAircraft.JSONFilename} profile (DCS-BIOS)");
                message.AppendLine($"The identifier must be unique, please correct the profile {DCSAircraft.JSONFilename} in the DCS-BIOS lib folder");
                message.AppendLine("---------------------------------------------");
                dupes.ForEach(dupe => message.AppendLine(dupe));
                message.AppendLine("---------------------------------------------");
                Logger.Error(message);
            }
        }

        private static void LoadMetaDataEnd(string jsonDirectory)
        {
            if (DCSBIOSAircraftLoadStatus.IsLoaded("MetadataEnd") || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly) || DCSAircraft.IsNoFrameLoadedYet(_dcsAircraft))
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    DCSBIOSControls.AddRange(ReadControlsFromDocJson(jsonDirectory + "\\MetadataEnd.json"));

                    DCSBIOSAircraftLoadStatus.SetLoaded("MetadataEnd", true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_NOT_FOUND_ERROR_MESSAGE} ==>[{jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private static List<DCSBIOSControl> ReadControlsFromDocJson(string inputPath)
        {
            // input is a map from category string to a map from key string to control definition
            // we read it all then flatten the grand children (the control definitions)
            var input = File.ReadAllText(inputPath);
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

        private static void LoadCommonData(string jsonDirectory)
        {
            if (DCSBIOSAircraftLoadStatus.IsLoaded("CommonData") || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly) || DCSAircraft.IsNoFrameLoadedYet(_dcsAircraft))
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    DCSBIOSControls.AddRange(ReadControlsFromDocJson(jsonDirectory + "\\CommonData.json"));
                    DCSBIOSAircraftLoadStatus.SetLoaded("CommonData", true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_NOT_FOUND_ERROR_MESSAGE} ==>[{jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        public static IEnumerable<DCSBIOSControl> GetControls()
        {
            LoadControls();

            // Remove duplicates which may come from loading NS430 or other additional profiles
            return DCSBIOSControls.Distinct(new DCSBIOSControlComparer()).ToList();
        }

        public static IEnumerable<DCSBIOSControl> GetStringOutputControls()
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
            return DCSBIOSControls.Where(o => o.Outputs.Count > 0 && o.Outputs.Any(x => x.OutputDataType == DCSBiosOutputType.StringType));
        }

        public static IEnumerable<DCSBIOSControl> GetIntegerOutputControls()
        {
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                return null;
            }

            LoadControls();
            return DCSBIOSControls.Where(o => o.Outputs.Count > 0 && o.Outputs.Any(x => x.OutputDataType == DCSBiosOutputType.IntegerType));
        }

        public static IEnumerable<DCSBIOSControl> GetInputControls()
        {
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled))
            {
                return null;
            }

            LoadControls();
            return DCSBIOSControls.Where(controlObject => controlObject.Inputs.Count > 0);
        }
    }
}
