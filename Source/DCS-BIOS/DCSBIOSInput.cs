using System;
using System.Collections.Generic;
using ClassLibraryCommon;

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

    public class DCSBIOSInputObject
    {
        private string _controlId;
        private string _description; //e.g. "description": "switch to previous or next state",
        private DCSBIOSInputType _interface; //e.g. fixed_step, set_state, action
        /*
         * fixed_step = <INC/DEC>
         * set_state = <integer>
         * action = TOGGLE
         * variable_step = <new_value>|-<decrease_by>|+<increase_by>
         */
        private int _maxValue; //Relevand when _interface = set_state
        private string _specifiedActionArgument; //e.g. TOGGLE
        private uint _specifiedSetStateArgument;
        private int _specifiedVariableStepArgument;
        private DCSBIOSFixedStepInput _specifiedFixedStepArgument;
        private int _delay;

        public void Consume(string controlId, DCSBIOSControlInput dcsbiosControlInput)
        {
            _controlId = controlId;
            _description = dcsbiosControlInput.description;

            if (dcsbiosControlInput.@interface.Equals("fixed_step"))
            {
                _interface = DCSBIOSInputType.FIXED_STEP;
            }
            else if (dcsbiosControlInput.@interface.Equals("set_state"))
            {
                _interface = DCSBIOSInputType.SET_STATE;
            }
            else if (dcsbiosControlInput.@interface.Equals("action"))
            {
                _interface = DCSBIOSInputType.ACTION;
            }
            else if (dcsbiosControlInput.@interface.Equals("variable_step"))
            {
                _interface = DCSBIOSInputType.VARIABLE_STEP;
            }

            _maxValue = dcsbiosControlInput.max_value.GetValueOrDefault();
            _specifiedActionArgument = dcsbiosControlInput.argument;
            //Set by user
            //_specifiedSetStateArgument
            //Set by user
            //_specifiedFixedStepInput
        }

        public string GetDCSBIOSCommand()
        {
            var result = "";
            switch (_interface)
            {
                case DCSBIOSInputType.FIXED_STEP:
                    {
                        result = _controlId + " " + _specifiedFixedStepArgument + "\n";
                        break;
                    }
                case DCSBIOSInputType.SET_STATE:
                    {
                        result = _controlId + " " + _specifiedSetStateArgument + "\n";
                        break;
                    }
                case DCSBIOSInputType.ACTION:
                    {
                        result = _controlId + " " + _specifiedActionArgument + "\n";
                        break;
                    }
                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        if (_specifiedVariableStepArgument > 0)
                        {
                            result = _controlId + " +" + _specifiedVariableStepArgument + "\n";
                        }
                        else
                        {
                            result = _controlId + " " + _specifiedVariableStepArgument + "\n";
                        }
                        break;
                    }
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception("Error getting DCS-BIOSInput command. ControlId = " + _controlId + " Interface = " + _interface);
            }
            return result;
        }

        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public string ControlId
        {
            get { return _controlId; }
            set { _controlId = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public DCSBIOSInputType Interface
        {
            get { return _interface; }
            set { _interface = value; }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        public string SpecifiedActionArgument
        {
            get { return _specifiedActionArgument; }
            set { _specifiedActionArgument = value; }
        }

        public uint SpecifiedSetStateArgument
        {
            get { return _specifiedSetStateArgument; }
            set { _specifiedSetStateArgument = value; }
        }

        public int SpecifiedVariableStepArgument
        {
            get { return _specifiedVariableStepArgument; }
            set { _specifiedVariableStepArgument = value; }
        }

        public DCSBIOSFixedStepInput SpecifiedFixedStepArgument
        {
            get { return _specifiedFixedStepArgument; }
            set { _specifiedFixedStepArgument = value; }
        }
    }

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
            return "";
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

        public DCSBIOSInputObject SelectedDCSBIOSInput
        {
            get { return _selectedDCSBIOSInput; }
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

                switch (_selectedDCSBIOSInput.Interface)
                {
                    case DCSBIOSInputType.FIXED_STEP:
                        {
                            return "DCSBIOSInput{" + _controlId + "|FIXED_STEP|" + _selectedDCSBIOSInput.SpecifiedFixedStepArgument + "|" + _selectedDCSBIOSInput.Delay + "}";
                        }
                    case DCSBIOSInputType.SET_STATE:
                        {
                            return "DCSBIOSInput{" + _controlId + "|SET_STATE|" + _selectedDCSBIOSInput.SpecifiedSetStateArgument + "|" + _selectedDCSBIOSInput.Delay + "}";
                        }
                    case DCSBIOSInputType.ACTION:
                        {
                            return "DCSBIOSInput{" + _controlId + "|ACTION|" + _selectedDCSBIOSInput.SpecifiedActionArgument + "|" + _selectedDCSBIOSInput.Delay + "}";
                        }
                    case DCSBIOSInputType.VARIABLE_STEP:
                        {
                            return "DCSBIOSInput{" + _controlId + "|VARIABLE_STEP|" + _selectedDCSBIOSInput.SpecifiedVariableStepArgument + "|" + _selectedDCSBIOSInput.Delay + "}";
                        }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(78311, ex, "Error in DCSBIOSInput.ToString(), ControlId = " + _controlId);
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
                                _selectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    _selectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    _selectedDCSBIOSInput.Delay = 0;
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
                                _selectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    _selectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    _selectedDCSBIOSInput.Delay = 0;
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
                                _selectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    _selectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    _selectedDCSBIOSInput.Delay = 0;
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
                                _selectedDCSBIOSInput = dcsbiosInputObject;
                                if (entries.Length == 4)
                                {
                                    _selectedDCSBIOSInput.Delay = int.Parse(entries[3]);
                                }
                                else
                                {
                                    _selectedDCSBIOSInput.Delay = 0;
                                }
                                break;
                            }
                        }
                        break;
                    }
            }
        }

        public string ControlId
        {
            get { return _controlId; }
            set { _controlId = value; }
        }

        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

        public string ControlDescription
        {
            get { return _controlDescription; }
            set { _controlDescription = value; }
        }

        public string ControlType
        {
            get { return _controlType; }
            set { _controlType = value; }
        }
    }
}
