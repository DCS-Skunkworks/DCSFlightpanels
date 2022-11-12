using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using Newtonsoft.Json;
using NLog;
using NonVisuals.Saitek;
using ThreadState = System.Threading.ThreadState;

/*
 * This is the base class for dcs-bios bindings.
 * A dcs-bios binding is used when a user maps a physical
 * switch with one or several dcs-bios command(s).
 */
namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    [Serializable]
    public abstract class DCSBIOSActionBindingBase : IDisposable
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private bool _whenOnTurnedOn = true;
        private string _description;
        [NonSerialized] private Thread _sendDCSBIOSCommandsThread;
        private volatile List<DCSBIOSInput> _dcsbiosInputs;

        internal abstract void ImportSettings(string settings);
        public abstract string ExportSettings();

        private bool _isSequenced;
        private int _sequenceIndex;
        private volatile bool _shutdownCommandsThread;


        private bool _disposed;
        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _shutdownCommandsThread = true;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
        }

        public bool IsRunning()
        {
            if (_sendDCSBIOSCommandsThread != null && (_sendDCSBIOSCommandsThread.ThreadState == ThreadState.Running ||
                                                       _sendDCSBIOSCommandsThread.ThreadState == ThreadState.WaitSleepJoin ||
                                                       _sendDCSBIOSCommandsThread.ThreadState == ThreadState.Unstarted))
            {
                return true;
            }

            return false;
        }

        protected Thread DCSBIOSCommandsThread => _sendDCSBIOSCommandsThread;

        protected bool CancelSendDCSBIOSCommands
        {
            get => _shutdownCommandsThread;
            set => _shutdownCommandsThread = value;
        }

        
        public void SendDCSBIOSCommands(CancellationToken cancellationToken)
        {
            CancelSendDCSBIOSCommands = true;
            Thread.Sleep(Constants.ThreadShutDownWaitTime);
            CancelSendDCSBIOSCommands = false;
            _sendDCSBIOSCommandsThread = new Thread(() => SendDCSBIOSCommandsThread(DCSBIOSInputs, cancellationToken));
            _sendDCSBIOSCommandsThread.Start();
        }

        private void SendDCSBIOSCommandsThread(List<DCSBIOSInput> dcsbiosInputs, CancellationToken cancellationToken)
        {
            try
            {
                if (_isSequenced)
                {
                    if (dcsbiosInputs.Count == 0)
                    {
                        return;
                    }

                    if (_sequenceIndex <= dcsbiosInputs.Count - 1)
                    {
                        if (CancelSendDCSBIOSCommands || cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        var command = dcsbiosInputs[_sequenceIndex].SelectedDCSBIOSInput.GetDCSBIOSCommand();
                        Thread.Sleep(dcsbiosInputs[_sequenceIndex].SelectedDCSBIOSInput.Delay);
                        if (CancelSendDCSBIOSCommands || cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        Debug.WriteLine("Sending DCS-BIOS command " + command);
                        DCSBIOS.Send(command);
                        _sequenceIndex++;

                        if (_sequenceIndex >= dcsbiosInputs.Count)
                        {
                            _sequenceIndex = 0;
                        }
                    }
                }
                else
                {
                    foreach (var dcsbiosInput in dcsbiosInputs)
                    {
                        var command = dcsbiosInput.SelectedDCSBIOSInput.GetDCSBIOSCommand();
                        Thread.Sleep(dcsbiosInput.SelectedDCSBIOSInput.Delay);

                        DCSBIOS.Send(command);

                        if (CancelSendDCSBIOSCommands || cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public Tuple<string, string> ParseSettingV2(string config)
        {
            var mode = "";
            var key = "";

            //MultiPanelDCSBIOSControlV2{32}\o/{ALT}\o/{FLAPS_LEVER_DOWN|BESKRIVNING}\o/\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|0|0}\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|1000}
            //SwitchPanelDCSBIOSControlV2{32}\o/{SWITCHKEY_LIGHTS_PANEL|AAP_STEER}\o/\o/DCSBIOSInput{AAP_STEER|SET_STATE|1|0}
            var parameters = config.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL },
                StringSplitOptions.RemoveEmptyEntries);

            //SwitchPanelDCSBIOSControlV2{32}
            var configInt = parameters[0]
                .Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1).Replace("}", "");
            ParseSettingsInt(int.Parse(configInt));
            
            if (config.Contains("MultiPanelDCSBIOSControl") || config.Contains("RadioPanelDCSBIOSControl")) // Has additional setting which tells which position leftmost dial is in
            {
                //{ALT}
                mode = Common.RemoveCurlyBrackets(parameters[1]);
                
                //{FLAPS_LEVER_DOWN|BESKRIVNING}
                var tmpKey = Common.RemoveCurlyBrackets(parameters[2]).Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                key = tmpKey[0];
                if (tmpKey.Length > 1)
                {
                    Description = tmpKey[1];
                }
            }
            else
            {
                //{SWITCHKEY_LIGHTS_PANEL|AAP_STEER}
                var tmpKey = Common.RemoveCurlyBrackets(parameters[1]).Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                key = tmpKey[0];
                if (tmpKey.Length > 1)
                {
                    Description = tmpKey[1];
                }
            }

            // The rest of the array besides last entry are DCSBIOSInput
            // DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
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

            return Tuple.Create(mode, key);
        }

        public Tuple<string, string> ParseSettingV1(string config)
        {
            var mode = "";
            var key = "";
            //RadioPanelDCSBIOSControl{UpperCOM1}\o/{1UpperLargeFreqWheelInc|PLT_INTL_PRIMARY_L_KNB}\o/\o/DCSBIOSInput{PLT_INTL_PRIMARY_L_KNB|VARIABLE_STEP|2000|0}	
            //MultiPanelDCSBIOSControl{ALT}\o/{1FLAPS_LEVER_DOWN|BESKRIVNING}\o/\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|0|0}\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|1000}
            //SwitchPanelDCSBIOSControl{1SWITCHKEY_LIGHTS_PANEL|AAP_STEER}\o/\o/DCSBIOSInput{AAP_STEER|SET_STATE|1|0}

            var parameters = config.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);
            
            if (config.Contains("MultiPanelDCSBIOSControl") || config.Contains("RadioPanelDCSBIOSControl"))
            {
                //MultiPanelDCSBIOSControl{ALT}
                mode = parameters[0]
                    .Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1).Replace("}", "");

                //{1FLAPS_LEVER_DOWN|BESKRIVNING}
                WhenTurnedOn = Common.RemoveCurlyBrackets(parameters[1]).Substring(0, 1) == "1";
                
                var tmpKey = Common.RemoveCurlyBrackets(parameters[1]).Substring(1).Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                key = tmpKey[0];
                if (tmpKey.Length > 1)
                {
                    Description = tmpKey[1];
                }
            }
            else
            {
                //SwitchPanelDCSBIOSControl{1SWITCHKEY_LIGHTS_PANEL|AAP_STEER}
                var keyInfo = parameters[0]
                    .Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture) + 1).Replace("}", "");
                WhenTurnedOn = (keyInfo.Substring(0, 1) == "1");
                var tmpKey = keyInfo.Substring(1).Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                key = tmpKey[0];
                if (tmpKey.Length > 1)
                {
                    Description = tmpKey[1];
                }
            }

            // The rest of the array besides last entry are DCSBIOSInput
            // DCSBIOSInput{AAP_EGIPWR|FIXED_STEP|INC}
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

            return Tuple.Create(mode, key);
        }

        public Tuple<string, string> ParseSetting(string config)
        {
            if (config.Contains("DCSBIOSControlV2{")) //Check that the setting is V2    
            {
                return ParseSettingV2(config);
            }

            if (config.Contains("DCSBIOSControl{")) //First version
            {
                return ParseSettingV1(config);
            }

            return null;
        }

        public string GetExportString(string header, string mode, string keyName)
        {
            //MultiPanelDCSBIOSControlV2{32}\o/{ALT}\o/{FLAPS_LEVER_DOWN|BESKRIVNING}\o/\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|0|0}\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|1000}
            //SwitchPanelDCSBIOSControlV2{32}\o/{SWITCHKEY_LIGHTS_PANEL|AAP_STEER}\o/\o/DCSBIOSInput{AAP_STEER|SET_STATE|1|0}

            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }

            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + dcsbiosInput);
            }

            if (!string.IsNullOrEmpty(mode))
            {
                //Multipanel/Radio has one additional setting
                if (!string.IsNullOrWhiteSpace(Description))
                {
                    return header + "{" + GetSettingsInt() + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + mode + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + keyName + "|" + Description + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder;
                }

                return header + "{" + GetSettingsInt() + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + mode + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + keyName + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder;
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                return header + "{" + GetSettingsInt() + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + keyName + "|" + Description + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder;
            }

            return header + "{" + GetSettingsInt() + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + keyName + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder;
        }

        public void ParseSettingsInt(int configs)
        {
            /*
             * Bit #1 IsSequenced
             * Bit #2 WhenTurnedOn
             */
            IsSequenced = (configs & 1) == 1;
            WhenOnTurnedOn = (configs & 2) == 2;
        }

        public int GetSettingsInt()
        {
            int result = 0;
            /*
             * Bit #1 IsSequenced
             * Bit #2 WhenTurnedOn
             */
            if (IsSequenced)
            {
                result |= 1;
            }
            if (WhenOnTurnedOn)
            {
                result |= 2;
            }

            return result;
        }

        [JsonProperty("WhenTurnedOn", Required = Required.Default)]
        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        [JsonProperty("Description", Required = Required.Default)]
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? "DCS-BIOS" : _description;
            set => _description = value;
        }

        [JsonProperty("DCSBIOSInputs", Required = Required.Default)]
        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get => _dcsbiosInputs;
            set => _dcsbiosInputs = value;
        }

        [JsonProperty("WhenOnTurnedOn", Required = Required.Default)]
        protected bool WhenOnTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public bool HasBinding()
        {
            return DCSBIOSInputs != null && DCSBIOSInputs.Count > 0;
        }

        [JsonProperty("IsSequenced", Required = Required.Default)]
        public bool IsSequenced
        {
            get => _isSequenced;
            set => _isSequenced = value;
        }


    }
}
