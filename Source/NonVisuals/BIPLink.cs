﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClassLibraryCommon;

namespace NonVisuals
{
    public abstract class BIPLink
    {
        /*
         This class binds a physical switch with a BIP LED
         */
        internal SortedList<int, BIPLight> _bipLights = new SortedList<int, BIPLight>();
        internal bool _whenOnTurnedOn = true;
        internal const string SeparatorChars = "\\o/";
        internal string _description;
        internal Thread _executingThread;
        internal long _abortCurrentSequence;
        internal long _threadHasFinished = 1;
        public abstract string ExportSettings();
        public abstract void ImportSettings(string settings);

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
                            Common.DebugP("Aborting BIPLink thread routine (AbortThread) #1");
                            break;
                        }
                        var bipLight = bipLights[i];
                        Thread.Sleep((int)bipLight.DelayBefore);
                        if (AbortThread())
                        {
                            Common.DebugP("Aborting BIPLink thread routine (AbortThread) #2");
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
        
        public SortedList<int, BIPLight> BIPLights
        {
            get { return _bipLights; }
            set { _bipLights = value; }
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
