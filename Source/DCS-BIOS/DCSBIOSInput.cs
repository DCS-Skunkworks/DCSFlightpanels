using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClassLibraryCommon;
using Newtonsoft.Json;

// ReSharper disable All
/*
 * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
 */
namespace DCS_BIOS
{

    public enum DCSBIOSFixedStepInput
    {
        INC,
        DEC
    }

    public enum DCSBIOSInputType
    {
        FIXED_STEP,
        SET_STATE,
        ACTION,
        VARIABLE_STEP
    }

    [Serializable]
    public static class DCSBIOSInputHandler
    {
        public static DCSBIOSInput GetDCSBIOSInput(string controlId)
        {
            var result = new DCSBIOSInput();
            var control = DCSBIOSControlLocator.GetControl(controlId);
            result.Consume(control);
            return result;
        }
    }

    [Serializable]
    public class DCSBIOSInput
    {
        //These are loaded and saved, all the rest are fetched from DCS-BIOS
        private string _controlId;
        //TODO Can a DCSBIOSInput have multiple _dcsbiosInputObjects????
        //The user has entered these two depending on type
        private List<DCSBIOSInputObject> _dcsbiosInputObjects = new List<DCSBIOSInputObject>();
        private DCSBIOSInputObject _selectedDCSBIOSInput;

        private string _controlDescription;
        private string _controlType; //display button toggle etc
        private int _delay;
        private bool _debug;

        public string GetDescriptionForInterface(DCSBIOSInputType dcsbiosInputType)
        {
            foreach (var dcsbiosInputObject in _dcsbiosInputObjects)
            {
                if (dcsbiosInputObject.Interface == dcsbiosInputType)
                {
                    return dcsbiosInputObject.Description;
                }
            }
            return string.Empty;
        }

        public int GetMaxValueForInterface(DCSBIOSInputType dcsbiosInputType)
        {
            if (dcsbiosInputType == DCSBIOSInputType.ACTION || dcsbiosInputType == DCSBIOSInputType.FIXED_STEP)
            {
                return -99;
            }
            foreach (var dcsbiosInputObject in _dcsbiosInputObjects)
            {
                if (dcsbiosInputObject.Interface == dcsbiosInputType)
                {
                    return dcsbiosInputObject.MaxValue;
                }
            }
            return -99;
        }

        public void Consume(DCSBIOSControl dcsbiosControl)
        {
            _controlId = dcsbiosControl.identifier;
            _controlDescription = dcsbiosControl.description;
            _controlType = dcsbiosControl.physical_variant;
            try
            {
                foreach (var dcsbiosControlInput in dcsbiosControl.inputs)
                {
                    var inputObject = new DCSBIOSInputObject();
                    inputObject.Consume(_controlId, dcsbiosControlInput);
                    _dcsbiosInputObjects.Add(inputObject);
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed to copy control " + _controlId + ". Control input is missing." + Environment.NewLine);
            }
        }

        [JsonProperty("SelectedDCSBIOSInput", Required = Required.Default)]
        public DCSBIOSInputObject SelectedDCSBIOSInput
        {
            get
            {
                /*
                 * This is an ugly fix. I do not remember anymore whether multiple input objects can be used or only one.
                 * This is some kind of bug but  I won't touch it no more.
                 */
                if (_selectedDCSBIOSInput == null && _dcsbiosInputObjects.Count > 0)
                {
                    return _dcsbiosInputObjects[0];
                }
                return _selectedDCSBIOSInput;
            }
            set { _selectedDCSBIOSInput = value; }
        }

        public List<DCSBIOSInputObject> DCSBIOSInputObjects
        {
            get { return _dcsbiosInputObjects; }
            set { _dcsbiosInputObjects = value; }
        }

        public void SetSelectedInputBasedOnInterfaceType(DCSBIOSInputType dcsbiosInputType)
        {
            foreach (var dcsbiosInputObject in DCSBIOSInputObjects)
            {
                if (dcsbiosInputObject.Interface == dcsbiosInputType)
                {
                    SelectedDCSBIOSInput = dcsbiosInputObject;
                    break;
                }
            }
        }

        public override string ToString()
        {
            /*
            * fixed_step = <INC/DEC>
            * set_state = <integer>
            * action = TOGGLE
            */
            try
            {
                if (SelectedDCSBIOSInput == null || SelectedDCSBIOSInput.Interface == null)
                {
                    Debugger.Break();
                }
                switch (SelectedDCSBIOSInput.Interface)
                {
                    case DCSBIOSInputType.FIXED_STEP:
                        {
                            return "DCSBIOSInput{" + _controlId + "|FIXED_STEP|" + SelectedDCSBIOSInput.SpecifiedFixedStepArgument + "|" + SelectedDCSBIOSInput.Delay + "}";
                        }
                    case DCSBIOSInputType.SET_STATE:
                        {
                            return "DCSBIOSInput{" + _controlId + "|SET_STATE|" + SelectedDCSBIOSInput.SpecifiedSetStateArgument + "|" + SelectedDCSBIOSInput.Delay + "}";
                        }
                    case DCSBIOSInputType.ACTION:
                        {
                            return "DCSBIOSInput{" + _controlId + "|ACTION|" + SelectedDCSBIOSInput.SpecifiedActionArgument + "|" + SelectedDCSBIOSInput.Delay + "}";
                        }
                    case DCSBIOSInputType.VARIABLE_STEP:
                        {
                            return "DCSBIOSInput{" + _controlId + "|VARIABLE_STEP|" + SelectedDCSBIOSInput.SpecifiedVariableStepArgument + "|" + SelectedDCSBIOSInput.Delay + "}";
                        }
                }
            }
            catch (Exception ex)
            {
                Common.LogError( ex, "Error in DCSBIOSInput.ToString(), ControlId = " + _controlId);
                throw;
            }
            return "SHOULD NEVER ARRIVE HERE";
        }

        public void ImportString(string str)
        {
            //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC|0}
            //DCSBIOSInput{AAP_EGIPWR|SET_STATE|65535|0}
            //DCSBIOSInput{AAP_EGIPWR|ACTION|TOGGLE|0}
            var value = str;
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("DCSBIOSInput cannot import null string.");
            }
            if (!str.StartsWith("DCSBIOSInput{") || !str.EndsWith("}"))
            {
                throw new Exception("DCSBIOSInput cannot import string : " + str);
            }
            value = value.Substring(value.IndexOf("{", StringComparison.InvariantCulture) + 1);
            //AAP_EGIPWR|FIXED_STEP|INC}
            //AAP_EGIPWR|SET_STATE|65535}
            //AAP_EGIPWR|ACTION|TOGGLE}
            value = value.Substring(0, value.Length - 1);
            //AAP_EGIPWR|FIXED_STEP|INC
            //AAP_EGIPWR|SET_STATE|65535
            //AAP_EGIPWR|ACTION|TOGGLE
            var entries = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            _controlId = entries[0];
            if (entries.Length == 4)
            {
                Delay = int.Parse(entries[3]);
            }
            else
            {
                Delay = 0;
            }
            var dcsBIOSControl = DCSBIOSControlLocator.GetControl(_controlId);
            Consume(dcsBIOSControl);

            switch (entries[1])
            {
                case "FIXED_STEP":
                    {
                        foreach (var dcsbiosInputObject in _dcsbiosInputObjects)
                        {
                            if (dcsbiosInputObject.Interface == DCSBIOSInputType.FIXED_STEP)
                            {
                                dcsbiosInputObject.SpecifiedFixedStepArgument = (DCSBIOSFixedStepInput)Enum.Parse(typeof(DCSBIOSFixedStepInput), entries[2]);
                                SelectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    SelectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    SelectedDCSBIOSInput.Delay = 0;
                                }
                                break;
                            }
                        }
                        break;
                    }
                case "SET_STATE":
                    {
                        foreach (var dcsbiosInputObject in _dcsbiosInputObjects)
                        {
                            if (dcsbiosInputObject.Interface == DCSBIOSInputType.SET_STATE)
                            {
                                dcsbiosInputObject.SpecifiedSetStateArgument = uint.Parse(entries[2]);
                                SelectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    SelectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    SelectedDCSBIOSInput.Delay = 0;
                                }
                                break;
                            }
                        }
                        break;
                    }
                case "ACTION":
                    {
                        foreach (var dcsbiosInputObject in _dcsbiosInputObjects)
                        {
                            if (dcsbiosInputObject.Interface == DCSBIOSInputType.ACTION)
                            {
                                dcsbiosInputObject.SpecifiedActionArgument = entries[2];
                                SelectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    SelectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    SelectedDCSBIOSInput.Delay = 0;
                                }
                                break;
                            }
                        }
                        break;
                    }
                case "VARIABLE_STEP":
                    {
                        foreach (var dcsbiosInputObject in _dcsbiosInputObjects)
                        {
                            if (dcsbiosInputObject.Interface == DCSBIOSInputType.VARIABLE_STEP)
                            {
                                dcsbiosInputObject.SpecifiedVariableStepArgument = int.Parse(entries[2]);
                                SelectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    SelectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    SelectedDCSBIOSInput.Delay = 0;
                                }
                                break;
                            }
                        }
                        break;
                    }
            }
        }

        [JsonProperty("ControlId", Required = Required.Default)]
        public string ControlId
        {
            get { return _controlId; }
            set { _controlId = value; }
        }

        [JsonProperty("Delay", Required = Required.Default)]
        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        [Obsolete]
        [JsonProperty("Debug", Required = Required.Default)]
        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

        [JsonProperty("ControlDescription", Required = Required.Default)]
        public string ControlDescription
        {
            get { return _controlDescription; }
            set { _controlDescription = value; }
        }

        [JsonProperty("ControlType", Required = Required.Default)]
        public string ControlType
        {
            get { return _controlType; }
            set { _controlType = value; }
        }
    }
}
