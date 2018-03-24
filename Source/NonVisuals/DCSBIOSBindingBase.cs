using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public class DCSBIOSBindingBase
    {

        /*
         * 
         * Cannot use this, if a switch is toggled which includes a long delay then all subsequent switch toggles will have to wait for the first one before being executed.
         * 
         */
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
        }
        
    }
}
