using System;
using DCS_BIOS.Json;

namespace DCS_BIOS
{

    [Serializable]
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
            _description = dcsbiosControlInput.Description;

            if (dcsbiosControlInput.ControlInterface.Equals("fixed_step"))
            {
                _interface = DCSBIOSInputType.FIXED_STEP;
            }
            else if (dcsbiosControlInput.ControlInterface.Equals("set_state"))
            {
                _interface = DCSBIOSInputType.SET_STATE;
            }
            else if (dcsbiosControlInput.ControlInterface.Equals("action"))
            {
                _interface = DCSBIOSInputType.ACTION;
            }
            else if (dcsbiosControlInput.ControlInterface.Equals("variable_step"))
            {
                _interface = DCSBIOSInputType.VARIABLE_STEP;
            }

            _maxValue = dcsbiosControlInput.MaxValue.GetValueOrDefault();
            _specifiedActionArgument = dcsbiosControlInput.Argument;
            //Set by user
            //_specifiedSetStateArgument
            //Set by user
            //_specifiedFixedStepInput
        }

        public string GetDCSBIOSCommand()
        {
            var result = string.Empty;
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

}
