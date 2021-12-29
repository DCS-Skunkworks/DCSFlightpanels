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

    public static class DCSBIOSControlLocator
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly List<DCSBIOSControl> DCSBIOSControls = new();
        private static readonly object LockObject = new();
        private static DCSFPProfile _dcsfpProfile;
        private static string _jsonDirectory;
        public static readonly string DCSBIOSNotFoundErrorMessage = "Error loading DCS-BIOS. Check that the DCS-BIOS location setting points to the JSON directory.";

        private static void Reset()
        {
            DCSBIOSProfileLoadStatus.Clear();
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
                    /*if (_listOnce)
                    {
                        _listOnce = false;
                        foreach (var dcsbiosControl in _dcsbiosControls)
                        {
                            if (dcsbiosControl.outputs.Count > 0)
                            {
                                Console.WriteLine(dcsbiosControl.identifier + " " + dcsbiosControl.outputs[0].address);
                            }
                            else
                            {
                                Console.WriteLine(dcsbiosControl.identifier);
                            }
                        }
                    }
                    PrintDuplicateControlIdentifiers(_dcsbiosControls, true);*/
                    if (!DCSBIOSControls.Exists(controlObject => controlObject.Identifier.Equals(controlId)))
                    {
                        throw new Exception($"Error, control {controlId} does not exist. ({Profile.Description})");
                    }

                    return DCSBIOSControls.Single(controlObject => controlObject.Identifier.Equals(controlId));
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception($"Check DCS-BIOS version. Failed to find control {controlId} for airframe {Profile.Description} ({Profile.JSONFilename}). Did you switch airframe type for the profile and have existing control(s) for the previous type saved?{Environment.NewLine}{ioe.Message}");
                }
            }
        }

        public static DCSBIOSOutput GetDCSBIOSOutput(string controlId)
        {
            lock (LockObject)
            {
                if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    throw new Exception("DCSBIOSControlLocator.GetDCSBIOSOutput() Should not be called when only key emulator is active");
                }

                try
                {
                    DCSBIOSControl control = GetControl(controlId);
                    var dcsBIOSOutput = new DCSBIOSOutput();
                    dcsBIOSOutput.Consume(control);
                    return dcsBIOSOutput;
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception($"Check DCS-BIOS version. Failed to create DCSBIOSOutput based on control {controlId} for profile {Profile.JSONFilename}{Environment.NewLine}{ioe.Message}");
                }
            }
        }

        private static void ReadDataFromJsonFile(string filename)
        {
            if (DCSBIOSProfileLoadStatus.IsLoaded(filename) || filename == DCSFPProfile.GetKeyEmulator().JSONFilename || filename == DCSFPProfile.GetKeyEmulatorSRS().JSONFilename || filename == DCSFPProfile.GetNoFrameLoadedYet().JSONFilename)
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
                        List<DCSBIOSControl> controls = ReadControlsFromDocJson(file.FullName);
                        DCSBIOSControls.AddRange(controls);
                        PrintDuplicateControlIdentifiers(controls);
                    }

                    DCSBIOSProfileLoadStatus.SetLoaded(filename, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOSNotFoundErrorMessage} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
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
            try
            {
                if (DCSFPProfile.IsNoFrameLoadedYet(_dcsfpProfile) ||
                    Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
                {
                    return;
                }

                if (!Common.IsEmulationModesFlagSet(EmulationMode.NS430Enabled) && DCSFPProfile.HasNS430())
                {
                    if (DCSBIOSProfileLoadStatus.IsLoaded(DCSFPProfile.GetNS430().JSONFilename))
                    {
                        //Discard all DCS-BIOS controls if user "unloaded" NS430. Not possible to remove them specifically
                        //Better to force load all controls
                        Reset();
                    }
                    else
                    {
                        DCSBIOSProfileLoadStatus.Remove(DCSFPProfile.GetNS430().JSONFilename);
                    }
                }

                if (Common.IsEmulationModesFlagSet(EmulationMode.NS430Enabled))
                {
                    ReadDataFromJsonFile(DCSFPProfile.GetNS430().JSONFilename);
                }

                if (DCSFPProfile.IsFlamingCliff(Profile))
                {
                    LoadCommonData(_jsonDirectory);
                    LoadMetaDataEnd(_jsonDirectory);
                }
                else
                {
                    LoadCommonData(_jsonDirectory);
                    LoadMetaDataEnd(_jsonDirectory);

                    // Load the controls for the actual aircraft/helicopter
                    ReadDataFromJsonFile(Profile.JSONFilename);
                }

                // Remove duplicates which may come from loading NS430 or other additional profiles
                while (DCSBIOSControls.Count(controlObject => controlObject.Identifier.Equals("_UPDATE_COUNTER")) > 1)
                {
                    DCSBIOSControls.Remove(DCSBIOSControls.FindLast(controlObject =>
                        controlObject.Identifier.Equals("_UPDATE_COUNTER")));
                }

                while (DCSBIOSControls.Count(controlObject =>
                           controlObject.Identifier.Equals("_UPDATE_SKIP_COUNTER")) > 1)
                {
                    DCSBIOSControls.Remove(DCSBIOSControls.FindLast(controlObject =>
                        controlObject.Identifier.Equals("_UPDATE_SKIP_COUNTER")));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOSNotFoundErrorMessage} ==>[{_jsonDirectory}]<==", ex);
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
                bool found = false;
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
                message.AppendLine($"Below is a list of duplicate identifiers found in the {Profile.JSONFilename} profile (DCS-BIOS)");
                message.AppendLine($"The identifier must be unique, please correct the profile {Profile.JSONFilename} in the DCS-BIOS lib folder");
                message.AppendLine("---------------------------------------------");
                dupes.ForEach(dupe => message.AppendLine(dupe));
                message.AppendLine("---------------------------------------------");
                logger.Error(message);
            }
        }
        
        private static void LoadMetaDataEnd(string jsonDirectory)
        {
            if (DCSBIOSProfileLoadStatus.IsLoaded("MetadataEnd") || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly) || DCSFPProfile.IsNoFrameLoadedYet(_dcsfpProfile))
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    DCSBIOSControls.AddRange(ReadControlsFromDocJson(jsonDirectory + "\\MetadataEnd.json"));

                    DCSBIOSProfileLoadStatus.SetLoaded("MetadataEnd", true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOSNotFoundErrorMessage} ==>[{jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private static List<DCSBIOSControl> ReadControlsFromDocJson(string inputPath)
        {
            // input is a map from category string to a map from key string to control definition
            // we read it all then flatten the grand children (the control definitions)
            string input = File.ReadAllText(inputPath);
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, DCSBIOSControl>>>(input)
                .Values
                .SelectMany(category => category.Values)
                .ToList();
        }

        private static void LoadCommonData(string jsonDirectory)
        {
            if (DCSBIOSProfileLoadStatus.IsLoaded("CommonData") || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly) || DCSFPProfile.IsNoFrameLoadedYet(_dcsfpProfile))
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    DCSBIOSControls.AddRange(ReadControlsFromDocJson(jsonDirectory + "\\CommonData.json"));
                    DCSBIOSProfileLoadStatus.SetLoaded("CommonData", true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOSNotFoundErrorMessage} ==>[{jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        public static DCSFPProfile Profile
        {
            get => _dcsfpProfile;
            set
            {
                if (_dcsfpProfile != value)
                {
                    _dcsfpProfile = value;
                    Reset();
                }
            }
        }

        public static string JSONDirectory
        {
            get => _jsonDirectory;
            set => _jsonDirectory = DBCommon.GetDCSBIOSJSONDirectory(value);
        }
        
        public static IEnumerable<DCSBIOSControl> GetControls()
        {
            LoadControls();
            return DCSBIOSControls;
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
            return DCSBIOSControls.Where(controlObject => (controlObject.Outputs.Count > 0) && controlObject.Outputs[0].OutputDataType == DCSBiosOutputType.StringType);
        }

        public static IEnumerable<DCSBIOSControl> GetIntegerOutputControls()
        {
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                return null;
            }

            LoadControls();
            return DCSBIOSControls.Where(controlObject => (controlObject.Outputs.Count > 0) && controlObject.Outputs[0].OutputDataType == DCSBiosOutputType.IntegerType);
        }

        public static IEnumerable<DCSBIOSControl> GetInputControls()
        {
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSInputEnabled))
            {
                return null;
            }

            LoadControls();
            return DCSBIOSControls.Where(controlObject => (controlObject.Inputs.Count > 0));
        }
    }
}
