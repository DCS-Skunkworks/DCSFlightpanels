using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.Panels.Saitek;

namespace NonVisuals.BindingClasses.BIP
{
    [Serializable]
    public abstract class BIPLinkBase
    {
        /*
         This class binds a physical switch with a BIP LED.
         The user can map a physical switch so that whenever it is flicked
         the BIP will light up in a position chosen by the user.
         */
        internal SortedList<int, BIPLight> _bipLights = new();
        internal bool WhenOnTurnedOn = true;
        internal string _description;
        [NonSerialized]
        private Thread _executingThread;
        private long _abortCurrentSequence;
        private long _threadHasFinished = 1;
        public abstract string ExportSettings();
        public abstract void ImportSettings(string settings);

        public Tuple<string, string> ParseSettingV1(string config)
        {
            string mode = "";
            string key;

            // SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
            // MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
            // RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
            var parameters = config.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL },
                StringSplitOptions.RemoveEmptyEntries);

            if (config.Contains("MultiPanel")) // Has additional setting which tells which position leftmost dial is in
            {
                // MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}
                var composites = Common.RemoveCurlyBrackets(parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture))).Trim().Split("|", StringSplitOptions.RemoveEmptyEntries);
                // ALT
                mode = composites[0];

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = composites[1].Substring(0, 1) == "1";
                key = composites[1].Substring(1).Trim();
            }
            else
            {
                // RadioPanelBIPLink{1UpperCOM1}
                var param = Common.RemoveCurlyBrackets(parameters[0].Substring(parameters[0].IndexOf("{", StringComparison.InvariantCulture))).Trim();
                // 1UpperCOM1
                WhenTurnedOn = Common.RemoveCurlyBrackets(param).Substring(0, 1) == "1";
                key = Common.RemoveCurlyBrackets(param).Substring(1).Trim();
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].StartsWith("BIPLight"))
                {
                    var tmpBipLight = new BIPLight();
                    _bipLights.Add(GetNewKeyValue(), tmpBipLight);
                    tmpBipLight.ImportSettings(parameters[i]);
                }

                if (parameters[i].StartsWith("Description["))
                {
                    var tmp = parameters[i].Replace("Description[", string.Empty).Replace("]", string.Empty);
                    _description = tmp;
                }
            }

            return Tuple.Create(mode, key);
        }

        public string GetExportString(string header, string mode, string keyName)
        {
            // MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
            // RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]

            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }

            var onStr = WhenOnTurnedOn ? "1" : "0";

            if (!string.IsNullOrEmpty(mode))
            {
                // MultipanelBIPLink{ALT|1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]

                var stringBuilder = new StringBuilder();
                stringBuilder.Append(header + "{" + mode + "|" + onStr + keyName + "}");
                foreach (var bipLight in _bipLights)
                {
                    stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + bipLight.Value.ExportSettings());
                }

                if (!string.IsNullOrWhiteSpace(_description))
                {
                    stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + "Description[" + _description + "]");
                }

                return stringBuilder.ToString();
            }
            else
            {
                // RadioPanelBIPLink{1UpperCOM1}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(header + "{" + onStr + keyName + "}");
                foreach (var bipLight in _bipLights)
                {
                    stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + bipLight.Value.ExportSettings());
                }

                if (!string.IsNullOrWhiteSpace(_description))
                {
                    stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + "Description[" + _description + "]");
                }

                return stringBuilder.ToString();
            }
        }

        private int GetNewKeyValue()
        {
            if (_bipLights.Count == 0)
            {
                return 0;
            }

            return _bipLights.Keys.Max() + 1;
        }

        public void Execute()
        {
            try
            {
                // Check for already executing thread
                if (!ThreadHasFinished() && _executingThread != null)
                {
                    SetAbortThreadState();
                    while (!ThreadHasFinished())
                    {
                        Thread.Sleep(50);
                    }

                    ResetAbortThreadState();
                    ResetThreadHasFinishedState();
                    _executingThread = new Thread(() => ExecuteThreaded(_bipLights));
                    _executingThread.Start();
                }
                else
                {
                    ResetThreadHasFinishedState();
                    _executingThread = new Thread(() => ExecuteThreaded(_bipLights));
                    _executingThread.Start();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void ExecuteThreaded(SortedList<int, BIPLight> bipLights)
        {
            try
            {

                try
                {
                    if (bipLights == null)
                    {
                        return;
                    }

                    var bipEventHandlerManager = BipFactory.GetBipEventHandlerManager();
                    if (!BipFactory.HasBips())
                    {
                        return;
                    }

                    for (var i = 0; i < bipLights.Count; i++)
                    {
                        if (AbortThread())
                        {
                            break;
                        }

                        var bipLight = bipLights[i];
                        Thread.Sleep((int)bipLight.DelayBefore);
                        if (AbortThread())
                        {
                            break;
                        }

                        bipEventHandlerManager.ShowLight(bipLight);
                    }
                }
                catch (Exception)
                { }
            }
            finally
            {
                SignalThreadHasFinished();
            }
        }


        private void SetAbortThreadState()
        {
            Interlocked.Exchange(ref _abortCurrentSequence, 1);
        }

        private void ResetAbortThreadState()
        {
            Interlocked.Exchange(ref _abortCurrentSequence, 0);
        }

        private bool AbortThread()
        {
            return Interlocked.Read(ref _abortCurrentSequence) == 1;
        }

        private bool ThreadHasFinished()
        {
            return Interlocked.Read(ref _threadHasFinished) == 1;
        }

        private void SignalThreadHasFinished()
        {
            Interlocked.Exchange(ref _threadHasFinished, 1);
        }

        private void ResetThreadHasFinishedState()
        {
            Interlocked.Exchange(ref _threadHasFinished, 0);
        }

        [JsonProperty("BIPLights", Required = Required.Default)]
        public SortedList<int, BIPLight> BIPLights
        {
            get => _bipLights;
            set => _bipLights = value;
        }
        /*
        private int GetNewKeyValue()
        {
            if (_bipLights.Count == 0)
            {
                return 0;
            }

            return _bipLights.Keys.Max() + 1;
        }
        */
        [JsonProperty("WhenTurnedOn", Required = Required.Default)]
        public bool WhenTurnedOn
        {
            get => WhenOnTurnedOn;
            set => WhenOnTurnedOn = value;
        }

        [JsonProperty("Description", Required = Required.Default)]
        public string Description
        {
            get => _description;
            set => _description = value;
        }
    }
}
