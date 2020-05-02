using System;
using System.Collections.Generic;
using System.Text;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;

namespace NonVisuals.DCSBIOSBindings
{
    public class DCSBIOSActionBindingPZ70 : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the PZ70 with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        
        ~DCSBIOSActionBindingPZ70()
        {
            CancelSendDCSBIOSCommands = true;
            DCSBIOSCommandsThread?.Abort();
        }

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControl{"))
            {
                //MultiPanelDCSBIOSControl{HDG}\\o/{1LCD_WHEEL_DEC|DCS-BIOS}\\o/\\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|0}\\o/\\\\?\\hid#vid_06a3&pid_0d06#9&244b4bcc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}\\o/PanelSettingsVersion=2X"
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelDCSBIOSControl{ALT}
                var param0 = parameters[0].Replace("MultiPanelDCSBIOSControl{", "").Replace("}", "");
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                //{1KNOB_ALT}
                //or
                //{1KNOB_ALT|Landing gear up and blablabla description}
                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                //1KNOB_ALT
                //or
                //1KNOB_ALT|Landing gear up and blablabla description
                WhenTurnedOn = (param1.Substring(0, 1) == "1");
                if (param1.Contains("|"))
                {
                    //1KNOB_ALT|Landing gear up and blablabla description
                    param1 = param1.Substring(1);
                    //KNOB_ALT|Landing gear up and blablabla description
                    var stringArray = param1.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), stringArray[0]);
                    Description = stringArray[1];
                }
                else
                {
                    param1 = param1.Substring(1);
                    _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param1);
                }
                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
                DCSBIOSInputs = new List<DCSBIOSInput>();
                for (int i = 2; i < parameters.Length - 1; i++)
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
        
        public PZ70DialPosition DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get => _multiPanelPZ70Knob;
            set => _multiPanelPZ70Knob = value;
        }
        
        public override string ExportSettings()
        {
            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }
            var onStr = WhenTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                //MultiPanelDCSBIOSControl{0ALT_BUTTON|Oxygen System Test}\o/\o/DCSBIOSInput{ENVCP_OXY_TEST|SET_STATE|0}
                return "MultiPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "|" + Description + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder.ToString();
            }
            return "MultiPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder.ToString();
        }

    }
}
