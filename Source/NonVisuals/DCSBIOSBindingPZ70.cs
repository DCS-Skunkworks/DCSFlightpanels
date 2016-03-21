using System;
using System.Collections.Generic;
using System.Text;
using DCS_BIOS;

namespace NonVisuals
{
    public class DCSBIOSBindingPZ70
    {
        /*
         This class binds a physical switch on the PZ70 with a DCSBIOSInput
         */
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        private List<DCSBIOSInput> _dcsbiosInputs;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";
        private string _description;

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControl{"))
            {
                //MultiPanelDCSBIOSControl{1KNOB_ALT}\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelDCSBIOSControl{1KNOB_ALT}
                //or
                //MultiPanelDCSBIOSControl{1KNOB_ALT|Landing gear up and blablabla description}
                var param0 = parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1);
                //1KNOB_ALT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1KNOB_ALT
                //or
                //1KNOB_ALT|Landing gear up and blablabla description
                _whenOnTurnedOn = (param0.Substring(0, 1) == "1");
                if (param0.Contains("|"))
                {
                    //1KNOB_ALT|Landing gear up and blablabla description
                    param0 = param0.Substring(1);
                    //KNOB_ALT|Landing gear up and blablabla description
                    var stringArray = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), stringArray[0]);
                    _description = stringArray[1];
                }
                else
                {
                    param0 = param0.Substring(1);
                    _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param0);
                }
                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
                _dcsbiosInputs = new List<DCSBIOSInput>();
                for (int i = 1; i < parameters.Length - 1; i++)
                {
                    var dcsbiosInput = new DCSBIOSInput();
                    dcsbiosInput.ImportString(parameters[i]);
                    _dcsbiosInputs.Add(dcsbiosInput);
                }
            }
        }

        public void SendDCSBIOSCommands()
        {
            foreach (var dcsbiosInput in _dcsbiosInputs)
            {
                var command = dcsbiosInput.SelectedDCSBIOSInput.GetDCSBIOSCommand();
                DCSBIOS.Send(command);
            }
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get { return _multiPanelPZ70Knob; }
            set { _multiPanelPZ70Knob = value; }
        }

        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get { return _dcsbiosInputs; }
            set { _dcsbiosInputs = value; }
        }


        public string ExportSettings()
        {
            if (_dcsbiosInputs.Count == 0)
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            //\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in _dcsbiosInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(_description))
            {
                return "MultiPanelDCSBIOSControl{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "|" + _description + "}" + SeparatorChars + stringBuilder.ToString();
            }
            return "MultiPanelDCSBIOSControl{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}" + SeparatorChars + stringBuilder.ToString();
        }

        public bool WhenTurnedOn
        {
            get { return _whenOnTurnedOn; }
            set { _whenOnTurnedOn = value; }
        }

        public string Description
        {
            get { return string.IsNullOrWhiteSpace(_description) ? "DCS-BIOS" : _description; }
            set { _description = value; }
        }
    }
}
