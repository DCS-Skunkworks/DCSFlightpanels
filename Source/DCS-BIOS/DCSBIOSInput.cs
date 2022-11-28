/*
 * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
 */

using DCS_BIOS.Json;

namespace DCS_BIOS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using NLog;

    /// <summary>
    /// These are used when manipulating a DCS-BIOS Control with Increase or Decrease.
    /// </summary>
    public enum DCSBIOSFixedStepInput
    {
        INC,
        DEC
    }

    /// <summary>
    /// Type of manipulation for a DCS-BIOS Control.
    /// Fixed shifts (+/-) the control using a fixed value.
    /// Set State sets the control to a specific value.
    /// Action, e.g. TOGGLE
    /// Variable Step, you set a step value first and then shifts the control using that.
    /// </summary>
    public enum DCSBIOSInputType
    {
        FIXED_STEP,
        SET_STATE,
        ACTION,
        VARIABLE_STEP
    }


    /// <summary>
    /// This class is a holder for all input information for a DCSBIOSControl.
    /// They are used for manipulating a specific cockpit control.
    /// See DCSBIOSInputType for the input types.
    /// When the user specifies a DCS-BIOS Control they also select a specific input type.
    /// </summary>
    [Serializable]
    public class DCSBIOSInput
    {
        internal static Logger Logger = LogManager.GetCurrentClassLogger();
        // These are loaded and saved, all the rest are fetched from DCS-BIOS
        private string _controlId;
        
        // The user has entered one of these two depending on type
        private List<DCSBIOSInputObject> _dcsbiosInputObjects = new();
        private DCSBIOSInputObject _selectedDCSBIOSInput;

        private string _controlDescription;
        private string _controlType; // display button toggle etc
        private int _delay;
        private bool _debug;

        public string GetDescriptionForInterface(DCSBIOSInputType dcsbiosInputType)
        {
            var searched = _dcsbiosInputObjects.FirstOrDefault(x => x.Interface == dcsbiosInputType);

            return searched != null ? searched.Description : string.Empty;
        }

        public int GetMaxValueForInterface(DCSBIOSInputType dcsbiosInputType)
        {
            if (dcsbiosInputType == DCSBIOSInputType.ACTION || dcsbiosInputType == DCSBIOSInputType.FIXED_STEP)
            {
                return -99;
            }

            var searched = _dcsbiosInputObjects.FirstOrDefault(x => x.Interface == dcsbiosInputType);

            return searched?.MaxValue ?? -99;
        }

        public static DCSBIOSInput GetDCSBIOSInput(string controlId)
        {
            var result = new DCSBIOSInput();
            var control = DCSBIOSControlLocator.GetControl(controlId);
            result.Consume(control);
            return result;
        }

        public void Consume(DCSBIOSControl dcsbiosControl)
        {
            _controlId = dcsbiosControl.Identifier;
            _controlDescription = dcsbiosControl.Description;
            _controlType = dcsbiosControl.PhysicalVariant;
            try
            {
                foreach (var dcsbiosControlInput in dcsbiosControl.Inputs)
                {
                    DCSBIOSInputObject inputObject = new();
                    inputObject.Consume(_controlId, dcsbiosControlInput);
                    _dcsbiosInputObjects.Add(inputObject);
                }
            }
            catch (Exception)
            {
                throw new Exception($"Failed to copy control {_controlId}. Control input is missing.{Environment.NewLine}");
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

            set => _selectedDCSBIOSInput = value;
        }

        public List<DCSBIOSInputObject> DCSBIOSInputObjects
        {
            get => _dcsbiosInputObjects;
            set => _dcsbiosInputObjects = value;
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
                return SelectedDCSBIOSInput.Interface switch {
                    DCSBIOSInputType.FIXED_STEP     => "DCSBIOSInput{" + _controlId + "|FIXED_STEP|" + SelectedDCSBIOSInput.SpecifiedFixedStepArgument + "|" + SelectedDCSBIOSInput.Delay + "}",
                    DCSBIOSInputType.SET_STATE      => "DCSBIOSInput{" + _controlId + "|SET_STATE|" + SelectedDCSBIOSInput.SpecifiedSetStateArgument + "|" + SelectedDCSBIOSInput.Delay + "}",
                    DCSBIOSInputType.ACTION         => "DCSBIOSInput{" + _controlId + "|ACTION|" + SelectedDCSBIOSInput.SpecifiedActionArgument + "|" + SelectedDCSBIOSInput.Delay + "}",
                    DCSBIOSInputType.VARIABLE_STEP  => "DCSBIOSInput{" + _controlId + "|VARIABLE_STEP|" + SelectedDCSBIOSInput.SpecifiedVariableStepArgument + "|" + SelectedDCSBIOSInput.Delay + "}",
                    _ => throw new Exception()
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in DCSBIOSInput.ToString(), ControlId = {_controlId}");
                throw;
            }
        }

        public void ImportString(string str)
        {
            // DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC|0}
            // DCSBIOSInput{AAP_EGIPWR|SET_STATE|65535|0}
            // DCSBIOSInput{AAP_EGIPWR|ACTION|TOGGLE|0}
            var value = str;
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("DCSBIOSInput cannot import null string.");
            }

            if (!str.StartsWith("DCSBIOSInput{") || !str.EndsWith("}"))
            {
                throw new Exception($"DCSBIOSInput cannot import string : {str}");
            }

            value = value[(value.IndexOf("{", StringComparison.InvariantCulture) + 1)..];

            // AAP_EGIPWR|FIXED_STEP|INC}
            // AAP_EGIPWR|SET_STATE|65535}
            // AAP_EGIPWR|ACTION|TOGGLE}
            value = value.Substring(0, value.Length - 1);

            // AAP_EGIPWR|FIXED_STEP|INC
            // AAP_EGIPWR|SET_STATE|65535
            // AAP_EGIPWR|ACTION|TOGGLE
            var entries = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            _controlId = entries[0];
            Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;

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
                                SelectedDCSBIOSInput.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
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
                                SelectedDCSBIOSInput.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
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
                                SelectedDCSBIOSInput.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;         
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
                                SelectedDCSBIOSInput.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
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
            get => _controlId;
            set => _controlId = value;
        }

        [JsonProperty("Delay", Required = Required.Default)]
        public int Delay
        {
            get => _delay;
            set => _delay = value;
        }

        [Obsolete]
        [JsonProperty("Debug", Required = Required.Default)]
        public bool Debug
        {
            get => _debug;
            set => _debug = value;
        }

        [JsonProperty("ControlDescription", Required = Required.Default)]
        public string ControlDescription
        {
            get => _controlDescription;
            set => _controlDescription = value;
        }

        [JsonProperty("ControlType", Required = Required.Default)]
        public string ControlType
        {
            get => _controlType;
            set => _controlType = value;
        }
    }
}
