
// ReSharper disable All
/*
 * Do not adhere to naming standard in DCS-BIOS code, standard are based on DCS-BIOS json files and byte streamnaming
 */

namespace DCS_BIOS
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    [Flags]
    public enum DcsBiosNotificationMode
    {
        AddressValue = 2,
        ByteArray = 4
    }


    public class DCSBIOS : IDisposable
    {
        //public delegate void DcsDataReceivedEventHandler(byte[] bytes);
        //public event DcsDataReceivedEventHandler OnDcsDataReceived;

        private static DCSBIOS _dcsBIOSInstance;

        /************************
        **********UDP************
        ************************/
        private UdpClient _udpReceiveClient;
        private UdpClient _udpSendClient;
        private Thread _dcsbiosListeningThread;
        private string _dcsbiosReceiveFromIPUdp = "239.255.50.10";
        private string _dcsbiosSendToIPUdp = "127.0.0.1";
        private int _dcsbiosReceivePortUdp = 5010;
        private int _dcsbiosSendPortUdp = 7778;
        private IPEndPoint _ipEndPointReceiverUdp;
        private IPEndPoint _ipEndPointSenderUdp;
        private readonly string _receivedDataUdp = null;
        /************************
        *************************
        ************************/

        private readonly object _lockExceptionObject = new object();
        private Exception _lastException;
        private DCSBIOSProtocolParser _dcsProtocolParser;
        private readonly DcsBiosNotificationMode _dcsBiosNotificationMode;
        private readonly IDcsBiosDataListener _iDcsBiosDataListener;
        private readonly object _lockObjectForSendingData = new object();
        private Encoding _iso8859_1 = Encoding.GetEncoding("ISO-8859-1");
        private bool _isRunning;

        public DCSBIOS(IDcsBiosDataListener iDcsBiosDataListener, string ipFromUdp, string ipToUdp, int portFromUdp, int portToUdp, DcsBiosNotificationMode dcsNoficationMode)
        {
            IPAddress ipAddress;
            _iDcsBiosDataListener = iDcsBiosDataListener;
            if (!string.IsNullOrEmpty(ipFromUdp) && IPAddress.TryParse(ipFromUdp, out ipAddress))
            {
                _dcsbiosReceiveFromIPUdp = ipFromUdp;
            }
            if (!string.IsNullOrEmpty(ipToUdp) && IPAddress.TryParse(ipToUdp, out ipAddress))
            {
                _dcsbiosSendToIPUdp = ipToUdp;
            }
            if (portFromUdp > 0)
            {
                _dcsbiosReceivePortUdp = portFromUdp;
            }
            if (portToUdp > 0)
            {
                _dcsbiosSendPortUdp = portToUdp;
            }
            _dcsBiosNotificationMode = dcsNoficationMode;
            
            _dcsProtocolParser = DCSBIOSProtocolParser.GetParser();
            _dcsProtocolParser.Attach(_iDcsBiosDataListener);

            _ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, ReceivePort);
            _ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(SendToIp), SendPort);

            _udpReceiveClient = new UdpClient();
            _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpReceiveClient.Client.Bind(_ipEndPointReceiverUdp);
            _udpReceiveClient.JoinMulticastGroup(IPAddress.Parse(ReceiveFromIp));

            _udpSendClient = new UdpClient();
            _udpSendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpSendClient.EnableBroadcast = true;

            //_tcpIpPort = tcpIpPort;
            _dcsBIOSInstance = this;
        }

        ~DCSBIOS()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _udpSendClient?.Close();
                _udpReceiveClient?.Close();
            }
            // free native resources
        }

        public void ReceiveDataUdp()
        {
            try
            {
                while (_isRunning)
                {
                    var byteData = _udpReceiveClient.Receive(ref _ipEndPointReceiverUdp);
                    if ((_dcsBiosNotificationMode & DcsBiosNotificationMode.AddressValue) == DcsBiosNotificationMode.AddressValue)
                    {
                        _dcsProtocolParser.AddArray(byteData);
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                if (!e.Message.Contains("WSACancelBlockingCall"))
                {
                    SetLastException(e);
                    Common.LogError(e, "DCSBIOS.ReceiveData()");
                }
            }
        }

        public void Startup()
        {
            try
            {
                /*
                    None, do as normal.
                */

                /*
                    Client, try establish connection to the master FP on the network every two seconds. After connection listen to data from master and add that to the arraysToProcess queue.
                */

                /*
                    Master, start listening TCP socket, for every client contacting via TCP add them as clients and send raw DCS-BIOS data to them
                */
                if (_isRunning)
                {
                    return;
                }
                _dcsbiosListeningThread = new Thread(new ThreadStart(ReceiveDataUdp));
                _isRunning = true;
                _dcsProtocolParser.Startup();
                _dcsbiosListeningThread.Start();
            }
            catch (Exception e)
            {
                SetLastException(e);
                Common.LogError(e, "DCSBIOS.Startup()");
                if (_udpReceiveClient != null && _udpReceiveClient.Client.Connected)
                {
                    _udpReceiveClient.Close();
                    _udpReceiveClient = null;
                }
                if (_udpSendClient != null && _udpSendClient.Client  != null && _udpSendClient.Client.Connected)
                {
                    _udpSendClient.Close();
                    _udpSendClient = null;
                }
            }
        }
     
        public void Shutdown()
        {
            try
            {
                _udpReceiveClient?.Close();
                _udpReceiveClient?.Close();
                _dcsProtocolParser?.Detach(_iDcsBiosDataListener);
                _dcsProtocolParser?.Shutdown();

                _udpReceiveClient = null;
                _udpReceiveClient = null;
                _dcsProtocolParser = null;
                _isRunning = false;
            }
            catch (Exception ex)
            {
                SetLastException(ex);
                Common.LogError(ex, "DCSBIOS.Shutdown()");
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
        }

        public static DCSBIOS GetInstance()
        {
            return _dcsBIOSInstance;
        }

        public static void AttachDataReceivedListenerSO(IDcsBiosDataListener iDcsBiosDataListener)
        {
            if (_dcsBIOSInstance != null)
            {
                _dcsBIOSInstance._dcsProtocolParser.OnDcsDataAddressValue += iDcsBiosDataListener.DcsBiosDataReceived;
            }
        }

        public void AttachDataReceivedListener(IDcsBiosDataListener iDcsBiosDataListener)
        {
            if (_dcsProtocolParser == null)
            {
                return;
            }
            
            _dcsProtocolParser.OnDcsDataAddressValue += iDcsBiosDataListener.DcsBiosDataReceived;
        }

        public void DetachDataReceivedListener(IDcsBiosDataListener iDcsBiosDataListener)
        {
            if (_dcsProtocolParser == null)
            {
                return;
            }
            _dcsProtocolParser.OnDcsDataAddressValue -= iDcsBiosDataListener.DcsBiosDataReceived;
        }

        public static int Send(string stringData)
        {
            return _dcsBIOSInstance.SendDataFunction(stringData);
        }

        public static void Send(string[] stringData)
        {
            if (stringData != null)
            {
                foreach (var s in stringData)
                {
                    _dcsBIOSInstance.SendDataFunction(s);
                }
            }
        }

        public static void Send(List<string> stringList)
        {
            if (stringList != null)
            {
                foreach (var s in stringList)
                {
                    _dcsBIOSInstance.SendDataFunction(s);
                }
            }
        }

        public int SendDataFunction(string stringData)
        {
            var result = 0;
            lock (_lockObjectForSendingData)
            {
                try
                {
                    //byte[] bytes = _iso8859_1.GetBytes(stringData);
                    var unicodeBytes = Encoding.Unicode.GetBytes(stringData);
                    var asciiBytes = new List<byte>(stringData.Length);
                    asciiBytes.AddRange(Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes));
                    result = _udpSendClient.Send(asciiBytes.ToArray(), asciiBytes.ToArray().Length, _ipEndPointSenderUdp);
                    //result = _udpSendClient.Send(bytes, bytes.Length, _ipEndPointSender);

                }
                catch (Exception e)
                {
                    SetLastException(e);
                    Common.LogError(e, "DCSBIOS.SendDataFunction()");
                }
            }
            return result;
        }
        /*
        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        */
        private void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }
                Common.LogError(ex, "Via DCSBIOS.SetLastException()");
                var message = ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace;
                lock (_lockExceptionObject)
                {
                    _lastException = new Exception(message);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public Exception GetLastException(bool resetException = false)
        {
            Exception result;
            lock (_lockExceptionObject)
            {
                result = _lastException;
                if (resetException)
                {
                    _lastException = null;
                }
            }
            return result;
        }

        public bool HasLastException()
        {
            lock (_lockExceptionObject)
            {
                return _lastException != null;
            }
        }

        public string ReceivedData
        {
            get { return _receivedDataUdp; }
        }

        public int SendPort
        {
            get { return _dcsbiosSendPortUdp; }
            set { _dcsbiosSendPortUdp = value; }
        }

        public int ReceivePort
        {
            get { return _dcsbiosReceivePortUdp; }
            set { _dcsbiosReceivePortUdp = value; }
        }

        public string SendToIp
        {
            get { return _dcsbiosSendToIPUdp; }
            set { _dcsbiosSendToIPUdp = value; }
        }

        public string ReceiveFromIp
        {
            get { return _dcsbiosReceiveFromIPUdp; }
            set { _dcsbiosReceiveFromIPUdp = value; }
        }
    }




}
