using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ClassLibraryCommon;

namespace DCS_BIOS
{
    public enum DCSBiosStateEnum
    {
        WAIT_FOR_SYNC = 0,
        ADDRESS_LOW = 1,
        ADDRESS_HIGH = 2,
        COUNT_LOW = 3,
        COUNT_HIGH = 4,
        DATA_LOW = 5,
        DATA_HIGH = 6,
    }

    internal class DCSBIOSProtocolParser : IDisposable
    {
        public delegate void DcsDataAddressValueEventHandler(object sender, DCSBIOSDataEventArgs e);
        public event DcsDataAddressValueEventHandler OnDcsDataAddressValue;

        public void Attach(IDcsBiosDataListener iDcsBiosDataListener)
        {
            OnDcsDataAddressValue += iDcsBiosDataListener.DcsBiosDataReceived;
        }

        public void Detach(IDcsBiosDataListener iDcsBiosDataListener)
        {
            OnDcsDataAddressValue -= iDcsBiosDataListener.DcsBiosDataReceived;
        }

        private DCSBiosStateEnum _state;
        private uint _address;
        private uint _count;
        private uint _data;
        private byte _syncByteCount;
        private bool _shutdown;
        private List<uint> _listOfAddressesToBroascast = new List<uint>();
        public static DCSBIOSProtocolParser DCSBIOSProtocolParserSO;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        //private object _lockArrayToProcess = new object();
        //private List<byte[]> _arraysToProcess = new List<byte[]>();
        private ConcurrentQueue<byte[]> _arraysToProcess = new ConcurrentQueue<byte[]>();
        private Thread _processingThread;

        private DCSBIOSProtocolParser()
        {
            _state = DCSBiosStateEnum.WAIT_FOR_SYNC;
            _syncByteCount = 0;
            DCSBIOSProtocolParserSO = this;
            _shutdown = false;
        }

        ~DCSBIOSProtocolParser()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _autoResetEvent?.Dispose();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Startup()
        {
            Common.DebugP("ProtocolParser starting up");
            _processingThread = new Thread(ProcessArrays);
            _processingThread.Start();
            _shutdown = false;
        }

        public void Shutdown()
        {
            Common.DebugP("ProtocolParser shutting down");
            _shutdown = true;
            try
            {
                _processingThread.Abort();
            }
            catch (Exception)
            {
            }
            _autoResetEvent.Set();
        }

        private void ProcessArrays()
        {
            try
            {
                var interval = 0;
                Common.DebugP("ProtocolParser starting processing loop");
                while (!_shutdown)
                {
                    try
                    {
                        if (interval >= 100)
                        {
                            //Debug.Print("_arraysToProcess.Count = " + _arraysToProcess.Count);
                            interval = 0;
                        }
                        //lock (_lockArrayToProcess)
                        //{
                        byte[] array = null;
                        while (_arraysToProcess.TryDequeue(out array))
                        {
                            if (array != null)
                            {
                                for (int i = 0; i < array.Length; i++)
                                {
                                    ProcessByte(array[i]);
                                }
                            }
                        }
                        //}
                        interval++;
                    }
                    catch (Exception e)
                    {
                        Common.LogError(9243, e, "DCSBIOSProtocolParser.ProcessArrays(), arrays to process : " + _arraysToProcess.Count);
                    }
                    _autoResetEvent.WaitOne();
                }
                Common.DebugP("ProtocolParser exiting processing loop");
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Common.LogError(9244, e, "DCSBIOSProtocolParser.ProcessArrays(), arrays to process : " + _arraysToProcess.Count);
            }
        }

        public static void RegisterAddressToBroadCast(uint address)
        {
            GetParser();
            if (DCSBIOSProtocolParserSO._listOfAddressesToBroascast.Any(u => u == address))
            {
                return;
            }
            DCSBIOSProtocolParserSO._listOfAddressesToBroascast.Add(address);
        }

        public static DCSBIOSProtocolParser GetParser()
        {
            if (DCSBIOSProtocolParserSO == null)
            {
                DCSBIOSProtocolParserSO = new DCSBIOSProtocolParser();
            }
            return DCSBIOSProtocolParserSO;
        }

        public void AddArray(byte[] bytes)
        {
            _arraysToProcess.Enqueue(bytes);
            _autoResetEvent.Set();
        }

        private bool IsBroadcastable(uint address)
        {
            if (_listOfAddressesToBroascast.Any(u => u == address))
            {
                return true;
            }
            /*for (var i = 0; i < _listOfAddressesToBroascast.Count; i++)
            {
                if (_listOfAddressesToBroascast[i] == address)
                {
                    return true;
                }
            }*/
            return false;
        }

        internal void ProcessByte(byte b)
        {
            try
            {
                switch (_state)
                {
                    case DCSBiosStateEnum.WAIT_FOR_SYNC:
                        /* do nothing */
                        break;
                    case DCSBiosStateEnum.ADDRESS_LOW:
                        _address = b;
                        _state = DCSBiosStateEnum.ADDRESS_HIGH;
                        break;
                    case DCSBiosStateEnum.ADDRESS_HIGH:
                        _address = (uint)(b << 8) | _address;
                        _state = _address != 0x5555 ? DCSBiosStateEnum.COUNT_LOW : DCSBiosStateEnum.WAIT_FOR_SYNC;
                        break;
                    case DCSBiosStateEnum.COUNT_LOW:
                        _count = b;
                        _state = DCSBiosStateEnum.COUNT_HIGH;
                        break;
                    case DCSBiosStateEnum.COUNT_HIGH:
                        _count = (uint)(b << 8) | _count;
                        _state = DCSBiosStateEnum.DATA_LOW;
                        break;
                    case DCSBiosStateEnum.DATA_LOW:
                        _data = b;
                        _count--;
                        _state = DCSBiosStateEnum.DATA_HIGH;
                        break;
                    case DCSBiosStateEnum.DATA_HIGH:
                        _data = (uint)(b << 8) | _data;
                        _count--;
                        //_iDcsBiosDataListener.DcsBiosDataReceived(_address, _data);
                        if (OnDcsDataAddressValue != null && IsBroadcastable(_address) && _data != 0x55)
                        {
                            /*if (_address == 25332)
                            {
                                Debug.Print("SENDING FROM DCS-BIOS address & value --> " + _address + "  " + _data);
                            }*/
                            OnDcsDataAddressValue?.Invoke(this, new DCSBIOSDataEventArgs() { Address = _address, Data = _data });
                        }
                        _address += 2;
                        if (_count == 0)
                            _state = DCSBiosStateEnum.ADDRESS_LOW;
                        else
                            _state = DCSBiosStateEnum.DATA_LOW;
                        break;
                }
                if (b == 0x55)
                {
                    //Console.WriteLine(Environment.TickCount - ticks);
                    //ticks = Environment.TickCount;
                    _syncByteCount++;
                }
                else
                {
                    _syncByteCount = 0;
                }
                if (_syncByteCount == 4)
                {
                    _state = DCSBiosStateEnum.ADDRESS_LOW;
                    _syncByteCount = 0;
                }
            }
            catch (Exception e)
            {
                Common.LogError(924094, e, "DCSBIOSProtocolParser.ProcessByte()");
            }
        }
    }
}
