using System;
using System.Collections.Generic;
using System.Text;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Radios;


namespace NonVisuals.DCSBIOSBindings
{

    public class DCSBIOSActionBindingPZ69 : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the PZ69 with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private PZ69DialPosition _pz69DialPosition;
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;
        
        ~DCSBIOSActionBindingPZ69()
        {
            CancelSendDCSBIOSCommands = true;
            DCSBIOSCommandsThread?.Abort();
        }

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ69)");
            }

            if (settings.StartsWith("RadioPanelDCSBIOSControl{"))
            {
                //RadioPanelDCSBIOSControl{COM1}\\o/{1UpperSmallFreqWheelInc|DCS-BIOS}\\o/\\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|0}\\o/\\\\?\\hid#vid_06a3&pid_0d06#9&244b4bcc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}\\o/PanelSettingsVersion=2X"
                var parameters = settings.Split(new[] {SeparatorChars}, StringSplitOptions.RemoveEmptyEntries);

                //RadioPanelDCSBIOSControl{COM1}
                var param0 = parameters[0].Replace("RadioPanelDCSBIOSControl{", "").Replace("}", "");
                _pz69DialPosition = (PZ69DialPosition) Enum.Parse(typeof(PZ69DialPosition), param0);

                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                WhenTurnedOn = (param1.Substring(0, 1) == "1");
                if (param1.Contains("|"))
                {
                    param1 = param1.Substring(1);
                    var stringArray = param1.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                    _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator) Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), stringArray[0]);
                    Description = stringArray[1];
                }
                else
                {
                    param1 = param1.Substring(1);
                    _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator) Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param1);
                }

                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
                DCSBIOSInputs = new List<DCSBIOSInput>();
                for (var i = 2; i < parameters.Length - 1; i++)
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
        
        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }
        
        public override string ExportSettings()
        {
            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }

            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in DCSBIOSBindingPZ69 for knob " + RadioPanelPZ69Knob + ". Cannot export.");
            }
            Common.DebugP(Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                //RadioPanelDCSBIOSControl{0COM1_BUTTON|Oxygen System Test}\o/\o/DCSBIOSInput{ENVCP_OXY_TEST|SET_STATE|0}
                return "RadioPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" +
                       SeparatorChars + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) +
                       "|" + Description + "}" + SeparatorChars + stringBuilder.ToString();
            }

            return "RadioPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" +
                   SeparatorChars + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "}" +
                   SeparatorChars + stringBuilder.ToString();
        }
    }
}
