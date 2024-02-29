/*
 * naming of all variables can not be changed because these classes are instantiated from Json based on DCS-BIOS naming standard. *
 */

using DCS_BIOS.Json;

namespace DCS_BIOS.Serialized
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ClassLibraryCommon;
    using ControlLocator;
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
        VARIABLE_STEP,
        SET_STRING
    }


    /// <summary>
    /// This class is a holder for all input information for a DCSBIOSControl.
    /// They are used for manipulating a specific cockpit control.
    /// See DCSBIOSInputType for the input types.
    /// When the user specifies a DCS-BIOS Control they also select a specific input type.
    /// </summary>
    [Serializable]
    [SerializeCritical]
    public class DCSBIOSInput
    {
        internal static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// All the interfaces this particular DCSBIOSInput offers.
        /// </summary>
        private List<DCSBIOSInputInterface> _dcsbiosInputInterfaces = new();

        /// <summary>
        /// The interface the user has chosen.
        /// </summary>
        [JsonProperty("SelectedDCSBIOSInterface", Required = Required.Default)]
        public DCSBIOSInputInterface SelectedDCSBIOSInterface { get; set; }

        [JsonProperty("ControlId", Required = Required.Default)]
        public string ControlId { get; set; }

        [JsonProperty("Delay", Required = Required.Default)]
        public int Delay { get; set; }

        [Obsolete]
        [JsonProperty("Debug", Required = Required.Default)]
        public bool Debug { get; set; }

        [JsonProperty("ControlDescription", Required = Required.Default)]
        public string ControlDescription { get; set; }

        [Obsolete]
        [JsonIgnore]
        public string ControlType { get; set; }

        public string GetDescriptionForInterface(DCSBIOSInputType dcsbiosInputType)
        {
            var dcsbiosInputInterface = _dcsbiosInputInterfaces.FirstOrDefault(x => x.Interface == dcsbiosInputType);

            return dcsbiosInputInterface != null ? dcsbiosInputInterface.Description : string.Empty;
        }

        public int GetMaxValueForInterface(DCSBIOSInputType dcsbiosInputType)
        {
            if (dcsbiosInputType == DCSBIOSInputType.SET_STRING)
            {
                return 0;
            }

            var searched = _dcsbiosInputInterfaces.FirstOrDefault(x => x.Interface == dcsbiosInputType);

            return searched?.MaxValue ?? 0;
        }

        public void Consume(DCSBIOSControl dcsbiosControl)
        {
            ControlId = dcsbiosControl.Identifier;
            ControlDescription = dcsbiosControl.Description;

            try
            {
                foreach (var dcsbiosControlInput in dcsbiosControl.Inputs)
                {
                    DCSBIOSInputInterface inputInterface = new();
                    inputInterface.Consume(ControlId, dcsbiosControlInput);
                    _dcsbiosInputInterfaces.Add(inputInterface);
                }

                if (_dcsbiosInputInterfaces.Count == 1)
                {
                    SelectedDCSBIOSInterface = _dcsbiosInputInterfaces[0];
                }
            }
            catch (Exception)
            {
                throw new Exception($"Failed to copy control {ControlId}. Control input is missing.{Environment.NewLine}");
            }
        }


        public List<DCSBIOSInputInterface> DCSBIOSInputInterfaces
        {
            get => _dcsbiosInputInterfaces;
            set => _dcsbiosInputInterfaces = value;
        }

        public void SetSelectedInterface(DCSBIOSInputType dcsbiosInputType)
        {
            foreach (var dcsbiosInputInterface in DCSBIOSInputInterfaces)
            {
                if (dcsbiosInputInterface.Interface == dcsbiosInputType)
                {
                    SelectedDCSBIOSInterface = dcsbiosInputInterface;
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
                return SelectedDCSBIOSInterface.Interface switch
                {
                    DCSBIOSInputType.FIXED_STEP => "DCSBIOSInput{" + ControlId + "|FIXED_STEP|" + SelectedDCSBIOSInterface.SpecifiedFixedStepArgument + "|" + SelectedDCSBIOSInterface.Delay + "}",
                    DCSBIOSInputType.SET_STATE => "DCSBIOSInput{" + ControlId + "|SET_STATE|" + SelectedDCSBIOSInterface.SpecifiedSetStateArgument + "|" + SelectedDCSBIOSInterface.Delay + "}",
                    DCSBIOSInputType.ACTION => "DCSBIOSInput{" + ControlId + "|ACTION|" + SelectedDCSBIOSInterface.SpecifiedActionArgument + "|" + SelectedDCSBIOSInterface.Delay + "}",
                    DCSBIOSInputType.VARIABLE_STEP => "DCSBIOSInput{" + ControlId + "|VARIABLE_STEP|" + SelectedDCSBIOSInterface.SpecifiedVariableStepArgument + "|" + SelectedDCSBIOSInterface.Delay + "}",
                    DCSBIOSInputType.SET_STRING => "DCSBIOSInput{" + ControlId + "|SET_STRING|" + SelectedDCSBIOSInterface.SpecifiedSetStringArgument + "|" + SelectedDCSBIOSInterface.Delay + "}",
                    _ => throw new Exception()
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in DCSBIOSInput.ToString(), ControlId = {ControlId}");
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
            ControlId = entries[0];
            Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;

            var dcsBIOSControl = DCSBIOSControlLocator.GetControl(ControlId);
            Consume(dcsBIOSControl);

            var type = Enum.Parse<DCSBIOSInputType>(entries[1]);
            switch (type)
            {
                case DCSBIOSInputType.FIXED_STEP:
                    {
                        foreach (var dcsbiosInputInterface in _dcsbiosInputInterfaces.Where(dcsbiosInputInterface => dcsbiosInputInterface.Interface == DCSBIOSInputType.FIXED_STEP))
                        {
                            dcsbiosInputInterface.SpecifiedFixedStepArgument = (DCSBIOSFixedStepInput)Enum.Parse(typeof(DCSBIOSFixedStepInput), entries[2]);
                            SelectedDCSBIOSInterface = dcsbiosInputInterface;
                            SelectedDCSBIOSInterface.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
                            break;
                        }

                        break;
                    }

                case DCSBIOSInputType.SET_STATE:
                    {
                        foreach (var dcsbiosInputInterface in _dcsbiosInputInterfaces.Where(dcsbiosInputInterface => dcsbiosInputInterface.Interface == DCSBIOSInputType.SET_STATE))
                        {
                            dcsbiosInputInterface.SpecifiedSetStateArgument = uint.Parse(entries[2]);
                            SelectedDCSBIOSInterface = dcsbiosInputInterface;
                            SelectedDCSBIOSInterface.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
                            break;
                        }

                        break;
                    }

                case DCSBIOSInputType.ACTION:
                    {
                        foreach (var dcsbiosInputInterFace in _dcsbiosInputInterfaces.Where(dcsbiosInputInterFace => dcsbiosInputInterFace.Interface == DCSBIOSInputType.ACTION))
                        {
                            dcsbiosInputInterFace.SpecifiedActionArgument = entries[2];
                            SelectedDCSBIOSInterface = dcsbiosInputInterFace;
                            SelectedDCSBIOSInterface.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
                            break;
                        }

                        break;
                    }

                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        foreach (var dcsbiosInputInterface in _dcsbiosInputInterfaces)
                        {
                            if (dcsbiosInputInterface.Interface == DCSBIOSInputType.VARIABLE_STEP)
                            {
                                dcsbiosInputInterface.SpecifiedVariableStepArgument = int.Parse(entries[2]);
                                SelectedDCSBIOSInterface = dcsbiosInputInterface;
                                SelectedDCSBIOSInterface.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
                                break;
                            }
                        }
                        break;
                    }

                case DCSBIOSInputType.SET_STRING:
                    {
                        foreach (var dcsbiosInputInterface in _dcsbiosInputInterfaces)
                        {
                            if (dcsbiosInputInterface.Interface == DCSBIOSInputType.SET_STRING)
                            {
                                dcsbiosInputInterface.SpecifiedSetStringArgument = entries[2];
                                SelectedDCSBIOSInterface = dcsbiosInputInterface;
                                SelectedDCSBIOSInterface.Delay = entries.Length == 4 ? int.Parse(entries[3]) : 0;
                                break;
                            }
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception($"Failed to determine input interface type {type}.");
                    }
            }
        }
    }
}
