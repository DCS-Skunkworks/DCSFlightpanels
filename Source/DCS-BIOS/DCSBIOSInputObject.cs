using System;
using DCS_BIOS.Json;

namespace DCS_BIOS
{

    /// <summary>
    /// This class takes care of a DCS-BIOS Control Input and also
    /// provides the command string and or sends the command
    /// that is sent to DCS-BIOS when it should trigger (e.g. user switches a switch).
    /// </summary>
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
        private int _maxValue; //Relevant when _interface = set_state
        private string _specifiedActionArgument; //e.g. TOGGLE
        private uint _specifiedSetStateArgument;
        private int _specifiedVariableStepArgument;
        private DCSBIOSFixedStepInput _specifiedFixedStepArgument;
        private int _delay;

        public void Consume(string controlId, DCSBIOSControlInput dcsbiosControlInput)
        {
            _controlId = controlId;
            _description = dcsbiosControlInput.Description;

            _interface = dcsbiosControlInput.ControlInterface switch
            {
                "fixed_step" => DCSBIOSInputType.FIXED_STEP,
                "set_state" => DCSBIOSInputType.SET_STATE,
                "action" => DCSBIOSInputType.ACTION,
                "variable_step" => DCSBIOSInputType.VARIABLE_STEP,
                _ => throw new SystemException($"Unexpected ControlInterface value [{dcsbiosControlInput.ControlInterface}]")
            };
            _maxValue = dcsbiosControlInput.MaxValue.GetValueOrDefault();
            _specifiedActionArgument = dcsbiosControlInput.Argument;
            //Set by user
            //_specifiedSetStateArgument
            //Set by user
            //_specifiedFixedStepInput
        }

        public string GetDCSBIOSCommand()
        {
            var command = _interface switch
            {
                DCSBIOSInputType.FIXED_STEP => $"{_controlId} {_specifiedFixedStepArgument}\n",
                DCSBIOSInputType.SET_STATE  => $"{_controlId} {_specifiedSetStateArgument}\n",
                DCSBIOSInputType.ACTION     => $"{_controlId} {_specifiedActionArgument}\n",
                DCSBIOSInputType.VARIABLE_STEP => 
                    _specifiedVariableStepArgument > 0 ?
                      $"{_controlId} +{_specifiedVariableStepArgument}\n"
                    : $"{_controlId} {_specifiedVariableStepArgument}\n",
                _ => throw new Exception("Unexpected DCSBIOSInputType value")
            };

            if (string.IsNullOrWhiteSpace(command))
            {
                throw new Exception($"Error getting DCS-BIOSInput command. ControlId = {_controlId} Interface = {_interface}");
            }
            return command;
        }

        public void SendCommand()
        {
            DCSBIOS.Send(GetDCSBIOSCommand());
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
