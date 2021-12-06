namespace NonVisuals.Saitek
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using Newtonsoft.Json;

    [Serializable]
    public abstract class BIPLink
    {
        /*
         This class binds a physical switch with a BIP LED
         */
        internal SortedList<int, BIPLight> _bipLights = new SortedList<int, BIPLight>();
        internal bool WhenOnTurnedOn = true;
        internal string _description;
        [NonSerialized]
        private Thread _executingThread;
        private long _abortCurrentSequence;
        private long _threadHasFinished = 1;
        public abstract string ExportSettings();
        public abstract void ImportSettings(string settings);

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
                Common.ShowErrorMessageBox( ex);
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
                catch (Exception )
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
