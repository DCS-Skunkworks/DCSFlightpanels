using System;
using System.Collections.Generic;
using System.Text;
using DCS_BIOS;

namespace NonVisuals
{
    public class DCSBIOSBindingFIP
    {
        /*
         This class binds a physical switch on the TPM with a DCSBIOSInput
         */
        private FIPPanelButtons _fipPanelButton;
        private List<DCSBIOSInput> _dcsbiosInputs;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";
        private string _description;

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingFIP)");
            }
            if (settings.StartsWith("FIPPanelDCSBIOSControl{"))
            {
                //FIPPanelDCSBIOSControl{1SOFTBUTTON_1}\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //FIPPanelDCSBIOSControl{1SOFTBUTTON_1}
                var param0 = parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1);
                //1SOFTBUTTON_1}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1SOFTBUTTON_1
                _whenOnTurnedOn = (param0.Substring(0, 1) == "1");
                if (param0.Contains("|"))
                {
                    //1SOFTBUTTON_1|Landing gear up and blablabla description
                    param0 = param0.Substring(1);
                    //SOFTBUTTON_1|Landing gear up and blablabla description
                    var stringArray = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _fipPanelButton = (FIPPanelButtons)Enum.Parse(typeof(FIPPanelButtons), stringArray[0]);
                    _description = stringArray[1];
                }
                else
                {
                    param0 = param0.Substring(1);
                    _fipPanelButton = (FIPPanelButtons)Enum.Parse(typeof(FIPPanelButtons), param0);
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

        public FIPPanelButtons FIPButton
        {
            get { return _fipPanelButton; }
            set { _fipPanelButton = value; }
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
            Common.DebugP(Enum.GetName(typeof(FIPPanelButtons), FIPButton) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";

            //\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in _dcsbiosInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(_description))
            {
                return "FIPPanelDCSBIOSControl{" + onStr + Enum.GetName(typeof(FIPPanelButtons), FIPButton) + "|" + _description + "}" + SeparatorChars + stringBuilder.ToString();
            }
            return "FIPPanelDCSBIOSControl{" + onStr + Enum.GetName(typeof(FIPPanelButtons), FIPButton) + "}" + SeparatorChars + stringBuilder.ToString();
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
