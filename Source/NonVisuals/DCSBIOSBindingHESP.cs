using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public class DCSBIOSBindingHESP
    {
        /*
         This class binds a physical switch on the HESP with a DCSBIOSInput
         */
        private HESPKeys _hespKey;
        private List<DCSBIOSInput> _dcsbiosInputs;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";
        private string _description;

        private Thread _sendDCSBIOSCommandsThread;
        private bool _cancelSendDCSBIOSCommands;

        ~DCSBIOSBindingHESP()
        {
            _cancelSendDCSBIOSCommands = true;
            _sendDCSBIOSCommandsThread?.Abort();
        }

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingHESP)");
            }
            if (settings.StartsWith("HESPDCSBIOSControl{"))
            {
                //HESPDCSBIOSControl{1KNOB_ENGINE_OFF}\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //HESPDCSBIOSControl{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1);
                //1KNOB_ENGINE_LEFT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1KNOB_ENGINE_LEFT
                _whenOnTurnedOn = (param0.Substring(0, 1) == "1");
                if (param0.Contains("|"))
                {
                    //1KNOB_ALT|Landing gear up and blablabla description
                    param0 = param0.Substring(1);
                    //KNOB_ALT|Landing gear up and blablabla description
                    var stringArray = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _hespKey = (HESPKeys)Enum.Parse(typeof(HESPKeys), stringArray[0]);
                    _description = stringArray[1];
                }
                else
                {
                    param0 = param0.Substring(1);
                    _hespKey = (HESPKeys)Enum.Parse(typeof(HESPKeys), param0);
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
                    Common.LogError(34912, ex);
                }
            }
            finally
            {
            }
        }

        public HESPKeys HESPKey
        {
            get { return _hespKey; }
            set { _hespKey = value; }
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
            Common.DebugP(Enum.GetName(typeof(HESPKeys), HESPKey) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";

            //\o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in _dcsbiosInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(_description))
            {
                return "HESPDCSBIOSControl{" + onStr + Enum.GetName(typeof(HESPKeys), HESPKey) + "|" + _description + "}" + SeparatorChars + stringBuilder.ToString();
            }
            return "HESPDCSBIOSControl{" + onStr + Enum.GetName(typeof(HESPKeys), HESPKey) + "}" + SeparatorChars + stringBuilder.ToString();
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
