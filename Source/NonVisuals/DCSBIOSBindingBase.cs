using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public abstract class DCSBIOSBindingBase
    {
        private bool _whenOnTurnedOn = true;
        internal const string SeparatorChars = "\\o/";
        private string _description;
        private Thread _sendDCSBIOSCommandsThread;

        internal abstract void ImportSettings(string settings);
        public abstract string ExportSettings();


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

        public List<DCSBIOSInput> DCSBIOSInputs { get; set; }

        protected bool WhenOnTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public bool HasBinding()
        {
            return DCSBIOSInputs != null && DCSBIOSInputs.Count > 0;
        }

        protected Thread DCSBIOSCommandsThread => _sendDCSBIOSCommandsThread;

        protected bool CancelSendDCSBIOSCommands { get; set; }


        public void SendDCSBIOSCommands()
        {
            CancelSendDCSBIOSCommands = true;
            _sendDCSBIOSCommandsThread = new Thread(() => SendDCSBIOSCommandsThread(DCSBIOSInputs));
            _sendDCSBIOSCommandsThread.Start();
        }

        private void SendDCSBIOSCommandsThread(List<DCSBIOSInput> dcsbiosInputs)
        {
            CancelSendDCSBIOSCommands = false;
            try
            {
                try
                {
                    foreach (var dcsbiosInput in dcsbiosInputs)
                    {
                        if (CancelSendDCSBIOSCommands)
                        {
                            return;
                        }
                        var command = dcsbiosInput.SelectedDCSBIOSInput.GetDCSBIOSCommand();
                        Thread.Sleep(dcsbiosInput.SelectedDCSBIOSInput.Delay);
                        if (CancelSendDCSBIOSCommands)
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
