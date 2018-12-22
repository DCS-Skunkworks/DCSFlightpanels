using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{

    public class DCSBIOSBindingPZ69
    {
        /*
         This class binds a physical switch on the PZ69 with a DCSBIOSInput
         */
        private PZ69DialPosition _pz69DialPosition;
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;
        private List<DCSBIOSInput> _dcsbiosInputs;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";
        private string _description;

        private Thread _sendDCSBIOSCommandsThread;
        private bool _cancelSendDCSBIOSCommands;

        ~DCSBIOSBindingPZ69()
        {
            _cancelSendDCSBIOSCommands = true;
            _sendDCSBIOSCommandsThread?.Abort();
        }

        internal void ImportSettings(string settings)
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
                _whenOnTurnedOn = (param1.Substring(0, 1) == "1");
                if (param1.Contains("|"))
                {
                    param1 = param1.Substring(1);
                    var stringArray = param1.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                    _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator) Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), stringArray[0]);
                    _description = stringArray[1];
                }
                else
                {
                    param1 = param1.Substring(1);
                    _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator) Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param1);
                }

                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
                _dcsbiosInputs = new List<DCSBIOSInput>();
                for (var i = 2; i < parameters.Length - 1; i++)
                {
                    if (parameters[i].StartsWith("DCSBIOSInput{"))
                    {
                        var dcsbiosInput = new DCSBIOSInput();
                        dcsbiosInput.ImportString(parameters[i]);
                        _dcsbiosInputs.Add(dcsbiosInput);
                    }
                }
            }
        }

        public void SendDCSBIOSCommands()
        {
            _cancelSendDCSBIOSCommands = true;
            _sendDCSBIOSCommandsThread = new Thread(() => SendDCSBIOSCommandsThread(_dcsbiosInputs));
            _sendDCSBIOSCommandsThread.Start();
        }

        private void SendDCSBIOSCommandsThread(List<DCSBIOSInput> dcsbiosInputs)
        {
            _cancelSendDCSBIOSCommands = false;
            try
            {
                try
                {
                    foreach (var dcsbiosInput in dcsbiosInputs)
                    {
                        if (_cancelSendDCSBIOSCommands)
                        {
                            return;
                        }

                        var command = dcsbiosInput.SelectedDCSBIOSInput.GetDCSBIOSCommand();
                        Thread.Sleep(dcsbiosInput.SelectedDCSBIOSInput.Delay);
                        if (_cancelSendDCSBIOSCommands)
                        {
                            return;
                        }

                        DCSBIOS.Send(command);
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Common.LogError(34172, ex);
                }
            }
            finally
            {
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

        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get => _dcsbiosInputs;
            set => _dcsbiosInputs = value;
        }


        public string ExportSettings()
        {
            if (_dcsbiosInputs.Count == 0)
            {
                return null;
            }

            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in DCSBIOSBindingPZ69 for knob " + RadioPanelPZ69Knob + ". Cannot export.");
            }
            Common.DebugP(Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in _dcsbiosInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }

            if (!string.IsNullOrWhiteSpace(_description))
            {
                //RadioPanelDCSBIOSControl{0COM1_BUTTON|Oxygen System Test}\o/\o/DCSBIOSInput{ENVCP_OXY_TEST|SET_STATE|0}
                return "RadioPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" +
                       SeparatorChars + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) +
                       "|" + _description + "}" + SeparatorChars + stringBuilder.ToString();
            }

            return "RadioPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition) + "}" +
                   SeparatorChars + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "}" +
                   SeparatorChars + stringBuilder.ToString();
        }

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "DCS-BIOS" : _description;
            set => _description = value;
        }

        public bool HasBinding()
        {
            return DCSBIOSInputs != null && DCSBIOSInputs.Count > 0;
        }
    }
}
