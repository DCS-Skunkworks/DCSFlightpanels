using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public class DCSBIOSBindingPZ70
    {
        /*
         This class binds a physical switch on the PZ70 with a DCSBIOSInput
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        private List<DCSBIOSInput> _dcsbiosInputs;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";
        private string _description;

        private Thread _sendDCSBIOSCommandsThread;
        private bool _cancelSendDCSBIOSCommands;

        ~DCSBIOSBindingPZ70()
        {
            _cancelSendDCSBIOSCommands = true;
            _sendDCSBIOSCommandsThread?.Abort();
        }

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControl{"))
            {
                //MultiPanelDCSBIOSControl{HDG}\\o/{1LCD_WHEEL_DEC|DCS-BIOS}\\o/\\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|0}\\o/\\\\?\\hid#vid_06a3&pid_0d06#9&244b4bcc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}\\o/PanelSettingsVersion=2X"
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

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
                _whenOnTurnedOn = (param1.Substring(0, 1) == "1");
                if (param1.Contains("|"))
                {
                    //1KNOB_ALT|Landing gear up and blablabla description
                    param1 = param1.Substring(1);
                    //KNOB_ALT|Landing gear up and blablabla description
                    var stringArray = param1.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), stringArray[0]);
                    _description = stringArray[1];
                }
                else
                {
                    param1 = param1.Substring(1);
                    _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param1);
                }
                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
                _dcsbiosInputs = new List<DCSBIOSInput>();
                for (int i = 2; i < parameters.Length - 1; i++)
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
                { }
                catch (Exception ex)
                {
                    Common.LogError(34172, ex);
                }
            }
            finally
            {
            }
        }

        public PZ70DialPosition DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
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
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in _dcsbiosInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(_description))
            {
                //MultiPanelDCSBIOSControl{0ALT_BUTTON|Oxygen System Test}\o/\o/DCSBIOSInput{ENVCP_OXY_TEST|SET_STATE|0}
                return "MultiPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SeparatorChars + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "|" + _description + "}" + SeparatorChars + stringBuilder.ToString();
            }
            return "MultiPanelDCSBIOSControl{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SeparatorChars + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}" + SeparatorChars + stringBuilder.ToString();
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
