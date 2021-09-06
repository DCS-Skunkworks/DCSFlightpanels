using System;
using System.Collections.Generic;
using System.Text;
using DCS_BIOS;
using NonVisuals.Saitek;

namespace NonVisuals.DCSBIOSBindings
{
    using MEF;

    [Serializable]
    public class DCSBIOSActionBindingPZ55 : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the PZ55 with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;

        ~DCSBIOSActionBindingPZ55()
        {
            CancelSendDCSBIOSCommands = true;
            DCSBIOSCommandsThread?.Abort();
        }

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ55)");
            }
            if (settings.StartsWith("SwitchPanelDCSBIOSControl{"))
            {
                //SwitchPanelDCSBIOSControl{1KNOB_ENGINE_OFF}\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                //SwitchPanelDCSBIOSControl{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1);
                //1KNOB_ENGINE_LEFT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1KNOB_ENGINE_LEFT
                WhenOnTurnedOn = (param0.Substring(0, 1) == "1");
                if (param0.Contains("|"))
                {
                    //1KNOB_ALT|Landing gear up and blablabla description
                    param0 = param0.Substring(1);
                    //KNOB_ALT|Landing gear up and blablabla description
                    var stringArray = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), stringArray[0]);
                    Description = stringArray[1];
                }
                else
                {
                    param0 = param0.Substring(1);
                    _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), param0);
                }
                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
                DCSBIOSInputs = new List<DCSBIOSInput>();
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].StartsWith("DCSBIOSInput{"))
                    {
                        var dcsbiosInput = new DCSBIOSInput();
                        dcsbiosInput.ImportString(parameters[i]);
                        DCSBIOSInputs.Add(dcsbiosInput);
                    }
                }
            }
        }

        public override string ExportSettings()
        {
            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }
            var onStr = WhenOnTurnedOn ? "1" : "0";

            //\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                return "SwitchPanelDCSBIOSControl{" + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "|" + Description + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder.ToString();
            }
            return "SwitchPanelDCSBIOSControl{" + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder.ToString();
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }
    }
}
