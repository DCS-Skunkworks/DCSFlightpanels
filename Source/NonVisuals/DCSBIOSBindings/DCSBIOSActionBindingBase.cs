using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals.DCSBIOSBindings
{
    [Serializable]
    public abstract class DCSBIOSActionBindingBase
    {
        private bool _whenOnTurnedOn = true;
        private string _description;
        [NonSerialized] private Thread _sendDCSBIOSCommandsThread;
        private volatile List<DCSBIOSInput> _dcsbiosInputs;
        
        internal abstract void ImportSettings(string settings);
        public abstract string ExportSettings();

        private bool _isSequenced = false;
        private int _sequenceIndex = 0;


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

        protected bool CancelSendDCSBIOSCommands { get; set; }


        public void SendDCSBIOSCommands(CancellationToken cancellationToken)
        {
            CancelSendDCSBIOSCommands = true;
            _sendDCSBIOSCommandsThread = new Thread(() => SendDCSBIOSCommandsThread(DCSBIOSInputs, cancellationToken));
            _sendDCSBIOSCommandsThread.Start();
        }

        private void SendDCSBIOSCommandsThread(List<DCSBIOSInput> dcsbiosInputs, CancellationToken cancellationToken)
        {
            CancelSendDCSBIOSCommands = false;
            try
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
                            var command = dcsbiosInputs[_sequenceIndex].SelectedDCSBIOSInput.GetDCSBIOSCommand();
                            Thread.Sleep(dcsbiosInputs[_sequenceIndex].SelectedDCSBIOSInput.Delay);
                            if (CancelSendDCSBIOSCommands || cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
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
                            if (CancelSendDCSBIOSCommands || cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            var command = dcsbiosInput.SelectedDCSBIOSInput.GetDCSBIOSCommand();
                            Thread.Sleep(dcsbiosInput.SelectedDCSBIOSInput.Delay);
                            if (CancelSendDCSBIOSCommands || cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            DCSBIOS.Send(command);
                        }
                    }
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    Common.LogError( ex);
                }
            }
            finally
            {
            }
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

        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get => _dcsbiosInputs;
            set => _dcsbiosInputs = value;
        }

        protected bool WhenOnTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public bool HasBinding()
        {
            return DCSBIOSInputs != null && DCSBIOSInputs.Count > 0;
        }

        public bool IsSequenced
        {
            get => _isSequenced;
            set => _isSequenced = value;
        }
        /*
         * 
         * Cannot use this, if a switch is toggled which includes a long delay then all subsequent switch toggles will have to wait for the first one before being executed.
         * 
         *
        private readonly ConcurrentQueue<DCSBIOSInput> _dcsbiosInputsToSend = new ConcurrentQueue<DCSBIOSInput>();

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private bool _doShutdownThread;
        private bool _cancelSendDCSBIOSCommands;

        public DCSBIOSBindingBase()
        {
            var sendDCSBIOSCommandsThread = new Thread(SendDCSBIOSCommandsThread);
            sendDCSBIOSCommandsThread.Start();
        }

        ~DCSBIOSBindingBase()
        {
            _doShutdownThread = true;
            _cancelSendDCSBIOSCommands = true;
            _autoResetEvent.Set();
        }

        protected void SendDCSBIOSCommands(List<DCSBIOSInput> dcsbiosInputsToSend)
        {
            foreach (var dcsbiosInput in dcsbiosInputsToSend)
            {
                _dcsbiosInputsToSend.Enqueue(dcsbiosInput);
            }
            _autoResetEvent.Set();
        }

        private void SendDCSBIOSCommandsThread()
        {
            _cancelSendDCSBIOSCommands = false;
            try
            {
                try
                {
                    while (!_doShutdownThread)
                    {
                        while(_dcsbiosInputsToSend.TryDequeue(out var dcsbiosInput))
                        {
                            if (_cancelSendDCSBIOSCommands)
                            {
                                break;
                            }
                            var command = dcsbiosInput.SelectedDCSBIOSInput.GetDCSBIOSCommand();
                            Thread.Sleep(dcsbiosInput.SelectedDCSBIOSInput.Delay);
                            if (_cancelSendDCSBIOSCommands)
                            {
                                break;
                            }
                            DCSBIOS.Send(command);
                        }
                        _autoResetEvent.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(34912, ex);
                }
            }
            finally
            {
            }
        }*/

    }
}
