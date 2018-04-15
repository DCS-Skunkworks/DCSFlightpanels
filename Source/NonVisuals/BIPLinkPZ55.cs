using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class BIPLinkPZ55
    {
        /*
         This class binds a physical switch on the PZ55 with a BIP LED
         */
        SortedList<int, BIPLight> _bipLights = new SortedList<int, BIPLight>();
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";
        private string _description;
        private Thread _executingThread;
        private long _abortCurrentSequence;
        private long _threadHasFinished = 1;

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (BIPLinkPZ55)");
            }
            if (settings.StartsWith("SwitchPanelBIPLink{"))
            {
                //SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                // 0 1 2 3
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //SwitchPanelKey{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("SwitchPanelBIPLink{", "").Replace("}", "").Trim();
                //1KNOB_ENGINE_LEFT
                _whenOnTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), param0);

                for (int i = 1; i < parameters.Length -1; i++)
                {
                    if (parameters[i].StartsWith("BIPLight"))
                    {
                        var tmpBipLight = new BIPLight();
                        _bipLights.Add(GetNewKeyValue(), tmpBipLight);
                        tmpBipLight.ImportSettings(parameters[i]);
                    }
                    if (parameters[i].StartsWith("Description["))
                    {
                        var tmp = parameters[i].Replace("Description[", "").Replace("]", "");
                        _description = tmp;
                    }
                }
            }
        }

        public void Execute()
        {
            try
            {
                //Check for already executing thread
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
                Common.ShowErrorMessageBox(1027, ex);
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
                            Common.DebugP("Aborting BIPLinkPZ55 thread routine (AbortThread) #1");
                            break;
                        }
                        var bipLight = bipLights[i];
                        Thread.Sleep((int)bipLight.DelayBefore);
                        if (AbortThread())
                        {
                            Common.DebugP("Aborting BIPLinkPZ55 thread routine (AbortThread) #2");
                            break;
                        }
                        bipEventHandlerManager.ShowLight(bipLight);
                    }
                }
                catch (Exception ex)
                {
                    Common.DebugP(ex.Message);
                }
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

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get { return _switchPanelPZ55Key; }
            set { _switchPanelPZ55Key = value; }
        }

        public SortedList<int, BIPLight> BIPLights
        {
            get { return _bipLights; }
            set { _bipLights = value; }
        }


        public string ExportSettings()
        {
            //SwitchPanelBIPLink{1KNOB_ENGINE_LEFT}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}\o/Description["Set Engines On"]\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            if (_bipLights == null || _bipLights.Count == 0)
            {
                return null;
            }
            var onStr = _whenOnTurnedOn ? "1" : "0";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("SwitchPanelBIPLink{" + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}");
            foreach (var bipLight in _bipLights)
            {
                stringBuilder.Append(SeparatorChars + bipLight.Value.ExportSettings());
            }

            if (!string.IsNullOrWhiteSpace(_description))
            {
                stringBuilder.Append(SeparatorChars + "Description[" + _description + "]");
            }
            return stringBuilder.ToString();
        }

        private int GetNewKeyValue()
        {
            if (_bipLights.Count == 0)
            {
                return 0;
            }
            return _bipLights.Keys.Max() + 1;
        }

        public bool WhenTurnedOn
        {
            get { return _whenOnTurnedOn; }
            set { _whenOnTurnedOn = value; }
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }
    }
}
