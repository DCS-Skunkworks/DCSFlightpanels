using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using CommonClassLibraryJD;

namespace DCS_BIOS
{
    public enum DCSAirframe
    {
        [Description("NoFrameLoadedYet")]
        NOFRAMELOADEDYET,
        [Description("KeyEmulator")]
        KEYEMULATOR,
        [Description("KeyEmulator_SRS")]
        KEYEMULATOR_SRS,
        [Description("A-10C")]
        A10C,
        [Description("UH-1H")]
        UH1H,
        [Description("MiG-21bis")]
        Mig21Bis,
        [Description("Ka-50")]
        Ka50,
        [Description("Mi-8MT")]
        Mi8,
        [Description("Bf-109K-4")]
        Bf109,
        [Description("FW-190D9")]
        Fw190,
        [Description("P-51D")]
        P51D,
        [Description("F-86F Sabre")]
        F86F,
        [Description("AJS37")]
        AJS37,
        [Description("SpitfireLFMkIX")]
        SpitfireLFMkIX,
        [Description("SA342L")]
        SA342L,
        [Description("SA342M")]
        SA342M,
        [Description("SA342Mistral")]
        SA342Mistral
    }

    public static class DCSBIOSControlLocator
    {
        private static object _lockObject = new object();
        private static DCSAirframe _airframe;
        private static string _jsonDirectory;
        private static bool _airFrameChanged;
        private static List<DCSBIOSControl> _dcsbiosControls = new List<DCSBIOSControl>();

        public static DCSBIOSControl GetControl(string controlId)
        {
            lock (_lockObject)
            {
                if (_airframe == DCSAirframe.KEYEMULATOR || _airframe == DCSAirframe.KEYEMULATOR_SRS)
                {
                    return null;
                }
                try
                {
                    LoadControls();
                    return _dcsbiosControls.Single(controlObject => controlObject.identifier.Equals(controlId));
                }
                catch (InvalidOperationException ioe)
                {
                    throw new Exception("Check DCS-BIOS version. Failed to find control " + controlId + " for airframe " + Airframe.GetDescription() + " (" + Airframe.GetDescription() + ".json). Did you switch airframe type for the profile and have existing control(s) for the previous type saved?" + Environment.NewLine + ioe.Message);
                }
            }
        }

        static void PrintDuplicateControlIdentifiers(List<DCSBIOSControl> dcsbiosControls)
        {
            var result = new List<string>();
            var dupes = new List<string>();
            foreach (var dcsbiosControl in dcsbiosControls)
            {
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
                DBCommon.LogError(2000, message);
            }
        }

        public static DCSBIOSOutput GetDCSBIOSOutput(string controlId)
        {
            lock (_lockObject)
            {
                if (_airframe == DCSAirframe.KEYEMULATOR || _airframe == DCSAirframe.KEYEMULATOR_SRS)
                {
                    return null;
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

        public static void LoadControls()
        {
            if (_airframe == DCSAirframe.KEYEMULATOR || _airframe == DCSAirframe.KEYEMULATOR_SRS || _airframe == DCSAirframe.NOFRAMELOADEDYET)
            {
                return;
            }
            if (_dcsbiosControls.Count > 0 && !_airFrameChanged)
            {
                return;
            }
            _dcsbiosControls = new List<DCSBIOSControl>();
            try
            {
                lock (_lockObject)
                {
                    //Always read CommonData.json
                    var directoryInfo = new DirectoryInfo(_jsonDirectory);
                    IEnumerable<FileInfo> files;
                    DBCommon.DebugP("Searching for " + _airframe.GetDescription() + ".json in directory " + _jsonDirectory);
                    try
                    {
                        files = directoryInfo.EnumerateFiles(_airframe.GetDescription() + ".json", SearchOption.TopDirectoryOnly);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to find DCS-BIOS files. -> " + Environment.NewLine + ex.Message);
                    }
                    foreach (var file in files)
                    {
                        DBCommon.DebugP("Opening " + file.DirectoryName + "\\" + file.Name);
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
                    var commonDataText = File.ReadAllText(_jsonDirectory + "\\CommonData.json");
                    var commonDataControlsText = DCSBIOSJsonFormatterVersion1.Format(commonDataText);
                    var commonDataControls = JsonConvert.DeserializeObject<DCSBIOSControlRootObject>(commonDataControlsText);
                    _dcsbiosControls.AddRange(commonDataControls.DCSBIOSControls);


                    var metaDataEndText = File.ReadAllText(_jsonDirectory + "\\MetadataEnd.json");
                    var metaDataEndControlsText = DCSBIOSJsonFormatterVersion1.Format(metaDataEndText);
                    var metaDataEndControls = JsonConvert.DeserializeObject<DCSBIOSControlRootObject>(metaDataEndControlsText);
                    _dcsbiosControls.AddRange(metaDataEndControls.DCSBIOSControls);
                    _airFrameChanged = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LoadControls() : " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static void LoadControls(string jsonDirectory, DCSAirframe airframe)
        {
            JSONDirectory = jsonDirectory;
            Airframe = airframe;
            LoadControls();
        }

        public static DCSAirframe Airframe
        {
            get { return _airframe; }
            set
            {
                if (_airframe != value)
                {
                    _airframe = value;
                    _airFrameChanged = true;
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

        public static IEnumerable<DCSBIOSControl> GetIntegerOutputControls()
        {
            if (_airframe == DCSAirframe.KEYEMULATOR || _airframe == DCSAirframe.KEYEMULATOR_SRS)
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.outputs.Count > 0) && controlObject.outputs[0].OutputDataType == DCSBiosOutputType.INTEGER_TYPE);
        }

        public static IEnumerable<DCSBIOSControl> GetInputControls()
        {
            if (_airframe == DCSAirframe.KEYEMULATOR || _airframe == DCSAirframe.KEYEMULATOR_SRS)
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.inputs.Count > 0));
        }
    }
}
