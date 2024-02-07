using System;
using ClassLibraryCommon;
using DCS_BIOS.Json;

namespace DCS_BIOS.Serialized
{

    /// <summary>
    /// This class takes care of a DCS-BIOS Control Input and also
    /// provides the command string and or sends the command
    /// that is sent to DCS-BIOS when it should trigger (e.g. user switches a switch).
    /// </summary>
    [Serializable]
    [SerializeCritical]
    public class DCSBIOSInputInterface
    {
        private string _specifiedActionArgument;
        private uint _specifiedSetStateArgument;
        private int _specifiedVariableStepArgument;
        private DCSBIOSFixedStepInput _specifiedFixedStepArgument;
        private string _specifiedSetStringArgument;

        public void Consume(string controlId, DCSBIOSControlInput dcsbiosControlInput)
        {
            ControlId = controlId;
            Description = dcsbiosControlInput.Description;

            Interface = dcsbiosControlInput.ControlInterface switch
            {
                "fixed_step" => DCSBIOSInputType.FIXED_STEP,
                "set_state" => DCSBIOSInputType.SET_STATE,
                "action" => DCSBIOSInputType.ACTION,
                "variable_step" => DCSBIOSInputType.VARIABLE_STEP,
                "set_string" => DCSBIOSInputType.SET_STRING,
                _ => throw new SystemException($"Unexpected ControlInterface value [{dcsbiosControlInput.ControlInterface}]")
            };
            MaxValue = dcsbiosControlInput.MaxValue.GetValueOrDefault();
            SuggestedStep = dcsbiosControlInput.SuggestedStep.GetValueOrDefault();
            SpecifiedActionArgument = dcsbiosControlInput.Argument;
        }

        public string GetDCSBIOSCommand()
        {
            var command = Interface switch
            {
                DCSBIOSInputType.FIXED_STEP => $"{ControlId} {SpecifiedFixedStepArgument}\n",
                DCSBIOSInputType.SET_STATE => $"{ControlId} {SpecifiedSetStateArgument}\n",
                DCSBIOSInputType.ACTION => $"{ControlId} {SpecifiedActionArgument}\n",
                DCSBIOSInputType.VARIABLE_STEP =>
                    SpecifiedVariableStepArgument > 0 ?
                      $"{ControlId} +{SpecifiedVariableStepArgument}\n"
                    : $"{ControlId} {SpecifiedVariableStepArgument}\n",
                DCSBIOSInputType.SET_STRING => $"{ControlId} {SpecifiedSetStringArgument}\n",
                _ => throw new Exception("Unexpected DCSBIOSInputType value")
            };

            if (string.IsNullOrWhiteSpace(command))
            {
                throw new Exception($"Error getting DCS-BIOSInput command. ControlId = {ControlId} Interface = {Interface}");
            }
            return command;
        }

        public void SendCommand()
        {
            DCSBIOS.Send(GetDCSBIOSCommand());
        }

        public int Delay { get; set; }

        public string ControlId { get; set; }

        /// <summary>
        /// e.g. "description": "switch to previous or next state",
        /// </summary>
        public string Description { get; set; }

        public DCSBIOSInputType Interface { get; set; }

        /// <summary>
        /// Relevant when _interface = set_state
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// Relevant when _interface = variable_step
        /// </summary>
        public int SuggestedStep { get; set; }

        /// <summary>
        /// e.g. TOGGLE
        /// </summary>
        public string SpecifiedActionArgument
        {
            get => _specifiedActionArgument;
            set
            {
                SelectedArgumentValue = value;
                _specifiedActionArgument = value;
            }
        }

        /// <summary>
        /// set_state = integer
        /// </summary>
        public uint SpecifiedSetStateArgument
        {
            get => _specifiedSetStateArgument;
            set
            {
                SelectedArgumentValue = value.ToString();
                _specifiedSetStateArgument = value;
            }
        }

        /// <summary>
        /// variable_step = new_value|-decrease_by|+increase_by 
        /// </summary>
        public int SpecifiedVariableStepArgument
        {
            get => _specifiedVariableStepArgument;
            set
            {
                SelectedArgumentValue = value.ToString();
                _specifiedVariableStepArgument = value;
            }
        }

        /// <summary>
        /// fixed_step = INC/DEC
        /// </summary>
        public DCSBIOSFixedStepInput SpecifiedFixedStepArgument
        {
            get => _specifiedFixedStepArgument;
            set
            {
                SelectedArgumentValue = value.ToString();
                _specifiedFixedStepArgument = value;
            }
        }

        /// <summary>
        /// set_string = some string
        /// </summary>
        public string SpecifiedSetStringArgument
        {
            get => _specifiedSetStringArgument;
            set
            {
                SelectedArgumentValue = value;
                _specifiedSetStringArgument = value;
            }
        }

        /// <summary>
        /// Shows selected argument regardless which interface was chosen.
        /// </summary>
        public string SelectedArgumentValue { get; private set; }
    }
}
