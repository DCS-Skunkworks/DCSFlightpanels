using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassLibraryCommon;
using CommonClassLibraryJD;
using Newtonsoft.Json;

namespace DCS_BIOS
{
    public static class DCSBIOSControlLocator
    {
        private static readonly object LockObject = new object();
        private static DCSAirframe _airframe;
        private static string _jsonDirectory;
        private static List<DCSBIOSControl> _dcsbiosControls = new List<DCSBIOSControl>();

        public static void Reset()
        {
            DCSBIOSProfileLoadStatus.Clear();
            _dcsbiosControls.Clear();
        }

        public static DCSBIOSControl GetControl(string controlId)
        {
            lock (LockObject)
            {
                if (Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
                {
                    return null;
                }
                try
                {
                    LoadControls();
                    //PrintDuplicateControlIdentifiers(_dcsbiosControls, true);
                    return _dcsbiosControls.Single(controlObject => controlObject.identifier.Equals(controlId));
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception("Check DCS-BIOS version. Failed to find control " + controlId + " for airframe " + Airframe.GetDescription() + " (" + Airframe.GetDescription() + ".json). Did you switch airframe type for the profile and have existing control(s) for the previous type saved?" + Environment.NewLine + ioe.Message);
                }
            }
        }

        static void PrintDuplicateControlIdentifiers(List<DCSBIOSControl> dcsbiosControls, bool printAll = false)
        {
            var result = new List<string>();
            var dupes = new List<string>();
            foreach (var dcsbiosControl in dcsbiosControls)
            {
                if (printAll)
                {
                    result.Add(dcsbiosControl.identifier);
                }

                //Debug.Print(dcsbiosControl.identifier);
                var found = false;
                foreach (var str in result)
                {
                    if (str.Trim() == dcsbiosControl.identifier.Trim())
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    result.Add(dcsbiosControl.identifier);
                }
                if (found)
                {
                    dupes.Add(dcsbiosControl.identifier);
                }

            }
            if (dupes.Count > 0)
            {
                var message = "Below is a list of duplicate identifiers found in the " + Airframe.GetDescription() + ".json profile (DCS-BIOS)\n";
                message = message + "The identifier must be unique, please correct the profile " + Airframe.GetDescription() + ".lua in the DCS-BIOS lib folder\n";
                message = message + "---------------------------------------------\n";
                foreach (var dupe in dupes)
                {
                    message = message + dupe + "\n";
                }
                message = message + "---------------------------------------------\n";
                Common.LogError(2000, message);
            }
        }

        public static DCSBIOSOutput GetDCSBIOSOutput(string controlId)
        {
            lock (LockObject)
            {
                if (Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
                {
                    throw new Exception("DCSBIOSControlLocator.GetDCSBIOSOutput() Should not be called when only key emulator is active");
                }
                try
                {
                    var control = GetControl(controlId);
                    var dcsBIOSOutput = new DCSBIOSOutput();
                    dcsBIOSOutput.Consume(control);
                    return dcsBIOSOutput;
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception("Check DCS-BIOS version. Failed to create DCSBIOSOutput based on control " + controlId + " for airframe " + Airframe.GetDescription() + " ( " + Airframe.GetDescription() + ".json)." + Environment.NewLine + ioe.Message);
                }
            }
        }

        public static void LoadMetaDataEnd(string jsonDirectory)
        {
            if (DCSBIOSProfileLoadStatus.IsLoaded("MetadataEnd") || Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly) || _airframe == DCSAirframe.NOFRAMELOADEDYET)
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    var metaDataEndText = File.ReadAllText(jsonDirectory + "\\MetadataEnd.json");
                    var metaDataEndControlsText = DCSBIOSJsonFormatterVersion1.Format(metaDataEndText);
                    var metaDataEndControls = JsonConvert.DeserializeObject<DCSBIOSControlRootObject>(metaDataEndControlsText);
                    _dcsbiosControls.AddRange(metaDataEndControls.DCSBIOSControls);
                    DCSBIOSProfileLoadStatus.SetLoaded("MetadataEnd", true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading DCS-BIOS. Check that the DCS-BIOS location setting points to the JSON directory. " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static void LoadCommonData(string jsonDirectory)
        {
            if (DCSBIOSProfileLoadStatus.IsLoaded("CommonData") || Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly) || _airframe == DCSAirframe.NOFRAMELOADEDYET)
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    var commonDataText = File.ReadAllText(jsonDirectory + "\\CommonData.json");
                    var commonDataControlsText = DCSBIOSJsonFormatterVersion1.Format(commonDataText);
                    var commonDataControls = JsonConvert.DeserializeObject<DCSBIOSControlRootObject>(commonDataControlsText);
                    _dcsbiosControls.AddRange(commonDataControls.DCSBIOSControls);
                    DCSBIOSProfileLoadStatus.SetLoaded("CommonData", true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading DCS-BIOS. Check that the DCS-BIOS location setting points to the JSON directory. " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static void LoadControls(string jsonDirectory, string profile)
        {
            if (DCSBIOSProfileLoadStatus.IsLoaded(profile) || profile == DCSAirframe.KEYEMULATOR.GetDescription() || profile == DCSAirframe.KEYEMULATOR_SRS.GetDescription() || profile == DCSAirframe.NOFRAMELOADEDYET.GetDescription())
            {
                return;
            }

            try
            {
                lock (LockObject)
                {
                    var directoryInfo = new DirectoryInfo(jsonDirectory);
                    IEnumerable<FileInfo> files;
                    Common.DebugP("Searching for " + profile + ".json in directory " + jsonDirectory);
                    try
                    {
                        files = directoryInfo.EnumerateFiles(profile + ".json", SearchOption.TopDirectoryOnly);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to find DCS-BIOS files. -> " + Environment.NewLine + ex.Message);
                    }

                    foreach (var file in files)
                    {
                        Common.DebugP("Opening " + file.DirectoryName + "\\" + file.Name);
                        var reader = file.OpenText();
                        string text;
                        try
                        {
                            text = reader.ReadToEnd();
                            //Debug.Print(text);
                        }
                        finally
                        {
                            reader.Close();
                        }

                        var jsonData = DCSBIOSJsonFormatterVersion1.Format(text);
                        //Debug.Print("\n--------------------------\n" + jsonData);
                        /*var newfile = File.CreateText(@"e:\temp\regexp_debug_output.txt.txt");
                        newfile.Write(jsonData);
                        newfile.Close();*/
                        var dcsBiosControlList = JsonConvert.DeserializeObject<DCSBIOSControlRootObject>(jsonData);
                        /*foreach (var control in dcsBiosControlList.DCSBIOSControls)
                        {
                            Debug.Print(control.description);
                        }*/
                        //Debug.Print("\n--------------------------\n" + jsonData);
                        _dcsbiosControls.AddRange(dcsBiosControlList.DCSBIOSControls);
                        PrintDuplicateControlIdentifiers(dcsBiosControlList.DCSBIOSControls);
                        /*foreach (var control in _dcsbiosControls)
                        {
                            Debug.Print(control.identifier);
                        }*/
                    }

                    DCSBIOSProfileLoadStatus.SetLoaded(profile, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading DCS-BIOS. Check that the DCS-BIOS location setting points to the JSON directory. " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
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
                if (_airframe == DCSAirframe.NOFRAMELOADEDYET ||
                    Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
                {
                    return;
                }

                if (!Common.IsOperationModeFlagSet(OperationFlag.NS430Enabled))
                {
                    if (DCSBIOSProfileLoadStatus.IsLoaded(DCSAirframe.NS430.GetDescription()))
                    {
                        //Discard all DCS-BIOS controls if user "unloaded" NS430. Not possible to remove them specifically
                        //Better to force load all controls
                        Reset();
                    }
                    else
                    {
                        DCSBIOSProfileLoadStatus.Remove(DCSAirframe.NS430.GetDescription());
                    }
                }

                if (Common.IsOperationModeFlagSet(OperationFlag.NS430Enabled))
                {
                    LoadControls(_jsonDirectory, DCSAirframe.NS430.GetDescription());
                }

                if (_airframe == DCSAirframe.FC3_CD_SRS)
                {
                    LoadCommonData(_jsonDirectory);
                    LoadMetaDataEnd(_jsonDirectory);
                }
                else
                {
                    LoadCommonData(_jsonDirectory);
                    LoadMetaDataEnd(_jsonDirectory);
                    //Load the controls for the actual aircraft/helicopter
                    LoadControls(_jsonDirectory, _airframe.GetDescription());
                }

                //Remove duplicates which may come from loading NS430 or other additional profiles
                while (_dcsbiosControls.Count(controlObject => controlObject.identifier.Equals("_UPDATE_COUNTER")) > 1)
                {
                    _dcsbiosControls.Remove(_dcsbiosControls.FindLast(controlObject =>
                        controlObject.identifier.Equals("_UPDATE_COUNTER")));
                }

                while (_dcsbiosControls.Count(controlObject =>
                           controlObject.identifier.Equals("_UPDATE_SKIP_COUNTER")) > 1)
                {
                    _dcsbiosControls.Remove(_dcsbiosControls.FindLast(controlObject =>
                        controlObject.identifier.Equals("_UPDATE_SKIP_COUNTER")));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error loading DCS-BIOS. Check that the DCS-BIOS location setting points to the JSON directory. " + e.Message);
            }
        }

        public static DCSAirframe Airframe
        {
            get { return _airframe; }
            set
            {
                if (_airframe != value)
                {
                    _airframe = value;
                    Reset();
                }
            }
        }

        public static string JSONDirectory
        {
            get { return _jsonDirectory; }
            set { _jsonDirectory = DBCommon.GetDCSBIOSJSONDirectory(value); }
        }

        public static IEnumerable<DCSBIOSControl> GetControls()
        {
            LoadControls();
            return _dcsbiosControls;
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
            if (!Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.outputs.Count > 0) && controlObject.outputs[0].OutputDataType == DCSBiosOutputType.STRING_TYPE);
        }

        public static IEnumerable<DCSBIOSControl> GetIntegerOutputControls()
        {
            if (!Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.outputs.Count > 0) && controlObject.outputs[0].OutputDataType == DCSBiosOutputType.INTEGER_TYPE);
        }

        public static IEnumerable<DCSBIOSControl> GetInputControls()
        {
            if (!Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSInputEnabled))
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.inputs.Count > 0));
        }
    }


    internal class DCSBIOSProfileLoadStatus
    {
        private string Profile { get; set; }
        private bool Loaded { get; set; }
        private static List<DCSBIOSProfileLoadStatus> _loadStatusList = new List<DCSBIOSProfileLoadStatus>();

        private DCSBIOSProfileLoadStatus(string profile, bool loaded)
        {
            Profile = profile;
            Loaded = loaded;
        }

        public static void Clear()
        {
            _loadStatusList.Clear();
        }

        public static void SetLoaded(string profile, bool loaded)
        {
            if (!IsRegistered(profile))
            {
               _loadStatusList.Add(new DCSBIOSProfileLoadStatus(profile, loaded));
                return;
            }

            foreach (var loadStatus in _loadStatusList)
            {
                if (loadStatus.Profile == profile)
                {
                    loadStatus.Loaded = loaded;
                }
            }
        }

        public static bool IsLoaded(string profile)
        {
            foreach (var loadStatus in _loadStatusList)
            {
                if (loadStatus.Profile == profile && loadStatus.Loaded)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsRegistered(string profile)
        {
            foreach (var loadStatus in _loadStatusList)
            {
                if (loadStatus.Profile == profile)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Remove(string profile)
        {
            var itemToRemove = _loadStatusList.SingleOrDefault(r => r.Profile == profile);
            if (itemToRemove != null)
            {
                _loadStatusList.Remove(itemToRemove);
            }
        }
    }
}
