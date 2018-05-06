using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClassLibraryCommon;

namespace DCS_BIOS
{
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

        private static DCSBIOS _dcsBIOSSO;

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
        private string _receivedDataUdp = null;
        /************************
        *************************
        ************************/


        /************************
        **********TCP************
        ************************/
        //Tunnel UDP packets to other FP instance having FIPs connected.
        /*private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private TcpClient _tcpReceiveClient;
        private TcpClient _tcpSendClient;
        private int _tcpIpPort = 43734;
        private Thread _tcpListeningThread;
        private Thread _tcpClientThread;*/
        /************************
        *************************
        ************************/

        private readonly object _lockExceptionObject = new object();
        private Exception _lastException;
        private DCSBIOSProtocolParser _dcsProtocolParser;
        private DcsBiosNotificationMode _dcsBiosNotificationMode;
        private readonly IDcsBiosDataListener _iDcsBiosDataListener;
        private object _lockObjectForSendingData = new object();
        private Encoding _iso8859_1 = Encoding.GetEncoding("ISO-8859-1");
        private bool _started;
        private bool _shutdown;

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

            //_tcpIpPort = tcpIpPort;
            _dcsBIOSSO = this;
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
                Common.DebugP("DCSBIOS entering threaded receive data loop");
                while (!_shutdown)
                {
                    var byteData = _udpReceiveClient.Receive(ref _ipEndPointReceiverUdp);
                    if ((_dcsBiosNotificationMode & DcsBiosNotificationMode.AddressValue) == DcsBiosNotificationMode.AddressValue)
                    {
                        _dcsProtocolParser.AddArray(byteData);
                    }
                }
                Common.DebugP("DCSBIOS exiting threaded receive data loop");
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                SetLastException(e);
                Common.LogError(9213, e, "DCSBIOS.ReceiveData()");
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
                Shutdown();
                if (_started)
                {
                    return;
                }
                _shutdown = false;
                Common.DebugP("DCSBIOS is STARTING UP");
                _dcsProtocolParser?.Detach(_iDcsBiosDataListener);
                _dcsProtocolParser?.Shutdown();
                _dcsProtocolParser = DCSBIOSProtocolParser.GetParser();
                _dcsProtocolParser.Attach(_iDcsBiosDataListener);

                _ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, ReceivePort);
                _ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(SendToIp), SendPort);

                _udpReceiveClient?.Close();
                _udpReceiveClient = new UdpClient();
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpReceiveClient.Client.Bind(_ipEndPointReceiverUdp);
                _udpReceiveClient.JoinMulticastGroup(IPAddress.Parse(ReceiveFromIp));

                _udpSendClient?.Close();
                _udpSendClient = new UdpClient();
                _udpSendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSendClient.EnableBroadcast = true;

                _dcsbiosListeningThread?.Abort();
                _dcsbiosListeningThread = new Thread(ReceiveDataUdp);
                _dcsbiosListeningThread.Start();
                _dcsProtocolParser.Startup();

                _started = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
                Common.LogError(9211, e, "DCSBIOS.Startup()");
                if (_udpReceiveClient != null && _udpReceiveClient.Client.Connected)
                {
                    _udpReceiveClient.Close();
                    _udpReceiveClient = null;
                }
                if (_udpSendClient != null && _udpSendClient.Client.Connected)
                {
                    _udpSendClient.Close();
                    _udpSendClient = null;
                }
            }
        }

        private void TCPReaderThread(NetworkStream stream)
        {
            while (_shutdown)
            {
                try
                {
                    if (stream == null)
                    {
                        return;
                    }
                    var byteData = new byte[1024];
                    stream.Read(byteData, 0, byteData.Length);
                    try
                    {
                        Debug.Print("DCS-BIOS data received over TCP");
                        if ((_dcsBiosNotificationMode & DcsBiosNotificationMode.AddressValue) == DcsBiosNotificationMode.AddressValue)
                        {
                            _dcsProtocolParser.AddArray(byteData);
                        }

                    }
                    catch (Exception e)
                    {
                        Common.LogError(9231, e, "Encoding.Default.GetString(readArray)");
                    }
                }
                catch (Exception e)
                {
                    Common.LogError(93211, e, "ServerReadThreadProc()");
                }
            }
            Debug.Print("TCP Listener ServerReadThreadProc returning");
        }


        public void Shutdown()
        {
            try
            {
                try
                {
                    _shutdown = true;
                    Common.DebugP("DCSBIOS is SHUTTING DOWN");
                    _dcsProtocolParser?.Detach(_iDcsBiosDataListener);
                    _dcsProtocolParser?.Shutdown();
                    _dcsbiosListeningThread?.Abort();
                }
                catch (Exception)
                {
                }
                try
                {
                    _udpReceiveClient?.Close();
                }
                catch (Exception)
                {
                }
                try
                {
                    _udpSendClient?.Close();
                }
                catch (Exception)
                {
                }
                _started = false;
            }
            catch (Exception ex)
            {
                SetLastException(ex);
                Common.LogError(9212, ex, "DCSBIOS.Shutdown()");
            }
        }

        public static void AttachDataReceivedListenerSO(IDcsBiosDataListener iDcsBiosDataListener)
        {
            if (_dcsBIOSSO != null)
            {
                _dcsBIOSSO._dcsProtocolParser.OnDcsDataAddressValue += iDcsBiosDataListener.DcsBiosDataReceived;
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
            return _dcsBIOSSO.SendDataFunction(stringData);
        }

        public static void Send(string[] stringData)
        {
            if (stringData != null)
            {
                foreach (var s in stringData)
                {
                    _dcsBIOSSO.SendDataFunction(s);
                }
            }
        }

        public static void Send(List<string> stringList)
        {
            if (stringList != null)
            {
                foreach (var s in stringList)
                {
                    _dcsBIOSSO.SendDataFunction(s);
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
                    Common.DebugP("Error sending data to DCS-BIOS. " + e.Message + Environment.NewLine + e.StackTrace);
                    SetLastException(e);
                    Common.LogError(9216, e, "DCSBIOS.SendDataFunction()");
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
                Common.LogError(666, ex, "Via DCSBIOS.SetLastException()");
                var message = ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace;
                Common.DebugP(message);
                lock (_lockExceptionObject)
                {
                    _lastException = new Exception(message);
                }
            }
            catch (Exception)
            {
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
