using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Description("None")]
        NONE,
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
        Fw190
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
                if (_airframe == DCSAirframe.NONE)
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
                    throw new Exception("Check DCS-BIOS version. Failed to find control " + controlId + " for airframe " + Airframe.GetDescription() + " ( " + Airframe.GetDescription() + ".json). Did you switch airframe type for the profile and have existing control(s) for the previous type saved?" + Environment.NewLine + ioe.Message);
                }
            }
        }

        public static DCSBIOSOutput GetDCSBIOSOutput(string controlId)
        {
            lock (_lockObject)
            {
                if (_airframe == DCSAirframe.NONE)
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
            if (_airframe == DCSAirframe.NONE || _airframe == DCSAirframe.NOFRAMELOADEDYET)
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
                    try
                    {
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
                            }
                            finally
                            {
                                reader.Close();
                            }

                            var jsonData = DCSBIOSJsonFormatterVersion1.Format(text);
                            /*var newfile = File.CreateText(@"e:\temp\regexp_debug_output.txt.txt");
                            newfile.Write(jsonData);
                            newfile.Close();*/
                            var dcsBiosControlList = JsonConvert.DeserializeObject<DCSBIOSControlRootObject>(jsonData);
                            _dcsbiosControls.AddRange(dcsBiosControlList.DCSBIOSControls);
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
                    finally
                    {
                        //nada
                    }
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
            if (_airframe == DCSAirframe.NONE)
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.outputs.Count > 0) && controlObject.outputs[0].OutputDataType == DCSBiosOutputType.INTEGER_TYPE);
        }

        public static IEnumerable<DCSBIOSControl> GetInputControls()
        {
            if (_airframe == DCSAirframe.NONE)
            {
                return null;
            }
            LoadControls();
            return _dcsbiosControls.Where(controlObject => (controlObject.inputs.Count > 0));
        }
    }
}
