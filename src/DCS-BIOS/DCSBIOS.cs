
// ReSharper disable All
/*
 * Do not adhere to naming standard in DCS-BIOS code, standard are based on DCS-BIOS json files and byte streamnaming
 */

using System.Diagnostics;
using DCS_BIOS.EventArgs;
using DCS_BIOS.StringClasses;
using NLog;
namespace DCS_BIOS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Timers;

    [Flags]
    public enum DcsBiosNotificationMode
    {
        Parse = 2,
        PassThrough = 4
    }

    /// <summary>
    /// Main class in project. Sends commands to DCS-BIOS and receives data about all cockpit controls
    /// in the aircraft.
    /// </summary>
    public class DCSBIOS : IDisposable
    {
        internal static readonly Logger logger = LogManager.GetCurrentClassLogger();
        //public delegate void DcsDataReceivedEventHandler(byte[] bytes);
        //public event DcsDataReceivedEventHandler OnDcsDataReceived;

        private static DCSBIOS _dcsBIOSInstance;

        /************************
        **********UDP************
        ************************/
        private UdpClient _udpReceiveClient;
        private UdpClient _udpSendClient;
        private Thread _dcsbiosListeningThread;
        private System.Timers.Timer _udpReceiveThrottleTimer = new(10) { AutoReset = true }; //Throttle UDP receive every 10 ms in case nothing is available
        private AutoResetEvent _udpReceiveThrottleAutoResetEvent = new(false);
        public string ReceiveFromIpUdp { get; set; } = "239.255.50.10";
        public string SendToIpUdp { get; set; } = "127.0.0.1";
        public int ReceivePortUdp { get; set; } = 5010;
        public int SendPortUdp { get; set; } = 7778;
        private IPEndPoint _ipEndPointReceiverUdp;
        private IPEndPoint _ipEndPointSenderUdp;
        public string ReceivedDataUdp { get; } = null;
        /************************
        *************************
        ************************/

        private readonly object _lockExceptionObject = new();
        private Exception _lastException;
        private DCSBIOSProtocolParser _dcsProtocolParser;
        private readonly DcsBiosNotificationMode _dcsBiosNotificationMode;
        private readonly object _lockObjectForSendingData = new();
        private volatile bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
        }

        public DCSBIOS(string ipFromUdp, string ipToUdp, int portFromUdp, int portToUdp, DcsBiosNotificationMode dcsNotificationMode)
        {

            if (!string.IsNullOrEmpty(ipFromUdp) && IPAddress.TryParse(ipFromUdp, out _))
            {
                ReceiveFromIpUdp = ipFromUdp;
            }

            if (!string.IsNullOrEmpty(ipToUdp) && IPAddress.TryParse(ipToUdp, out _))
            {
                SendToIpUdp = ipToUdp;
            }

            if (portFromUdp > 0)
            {
                ReceivePortUdp = portFromUdp;
            }

            if (portToUdp > 0)
            {
                SendPortUdp = portToUdp;
            }

            _dcsBiosNotificationMode = dcsNotificationMode;
            _dcsBIOSInstance = this;

            Startup();
        }

        public void Dispose()
        {
            DCSBIOSStringManager.Close();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _udpReceiveClient?.Dispose();
                _udpSendClient?.Dispose();
                _dcsProtocolParser?.Dispose();
                Shutdown();
            }
        }

        public void Startup()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _dcsProtocolParser = DCSBIOSProtocolParser.GetParser();

                _ipEndPointReceiverUdp = new IPEndPoint(IPAddress.Any, ReceivePortUdp);
                _ipEndPointSenderUdp = new IPEndPoint(IPAddress.Parse(SendToIpUdp), SendPortUdp);

                _udpReceiveClient = new UdpClient();
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 200);
                _udpReceiveClient.Client.Bind(_ipEndPointReceiverUdp);
                _udpReceiveClient.JoinMulticastGroup(IPAddress.Parse(ReceiveFromIpUdp));

                _udpSendClient = new UdpClient();
                _udpSendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSendClient.EnableBroadcast = true;

                _udpReceiveThrottleTimer.Elapsed += UdpReceiveThrottleTimer_Elapsed;
                _udpReceiveThrottleTimer.Start();
                _dcsbiosListeningThread = new Thread(ReceiveDataUdp);

                _isRunning = true;
                _dcsProtocolParser.Startup();
                _dcsbiosListeningThread.Start();
            }
            catch (Exception ex)
            {
                SetLastException(ex);
                logger.Error(ex, "DCSBIOS.Startup()");
                if (_udpReceiveClient != null && _udpReceiveClient.Client.Connected)
                {
                    _udpReceiveClient.Close();
                    _udpReceiveClient = null;
                }
                if (_udpSendClient != null && _udpSendClient.Client != null && _udpSendClient.Client.Connected)
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
                _isRunning = false;
                _udpReceiveThrottleAutoResetEvent.Set();
                _udpReceiveClient?.Close();
                _dcsProtocolParser?.Shutdown();

                _udpReceiveClient = null;
                _udpReceiveClient = null;
                _dcsProtocolParser = null;
                _udpReceiveThrottleTimer.Stop();
            }
            catch (Exception ex)
            {
                SetLastException(ex);
                logger.Error(ex, "DCSBIOS.Shutdown()");
            }
        }

        private void UdpReceiveThrottleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _udpReceiveThrottleAutoResetEvent.Set();
        }

        public void ReceiveDataUdp()
        {
            try
            {
                while (_isRunning)
                {
                    try
                    {
                        if (_udpReceiveClient.Available > 0)
                        {
                            BIOSEventHandler.ConnectionActive(this);
                            var byteData = _udpReceiveClient.Receive(ref _ipEndPointReceiverUdp);
                            if ((_dcsBiosNotificationMode & DcsBiosNotificationMode.Parse) == DcsBiosNotificationMode.Parse)
                            {
                                _dcsProtocolParser.AddArray(byteData);
                            }
                            if ((_dcsBiosNotificationMode & DcsBiosNotificationMode.PassThrough) == DcsBiosNotificationMode.PassThrough)
                            {
                                BIOSEventHandler.AsyncDCSBIOSBulkDataAvailable(this, byteData);
                            }
                            continue;
                        }
                        _udpReceiveThrottleAutoResetEvent.WaitOne(); // Minimizes CPU hit
                    }
                    catch (SocketException)
                    {
                        continue;
                    }
                }

            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("WSACancelBlockingCall"))
                {
                    SetLastException(ex);
                    logger.Error(ex, "DCSBIOS.ReceiveData()");
                }
            }
        }

        public static DCSBIOS GetInstance()
        {
            return _dcsBIOSInstance;
        }

        public static int Send(string stringData)
        {
            Debug.WriteLine($"Sending command : {stringData}");
            return _dcsBIOSInstance.SendDataFunction(stringData);
        }

        public static void Send(string[] stringArray)
        {
            if (stringArray != null)
            {
                Send(stringArray.ToList());
            }
        }

        public static void Send(List<string> stringList)
        {
            if (stringList != null)
            {
                stringList.ForEach(s => _dcsBIOSInstance.SendDataFunction(s));
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
                catch (Exception ex)
                {
                    SetLastException(ex);
                    logger.Error(ex, "DCSBIOS.SendDataFunction()");
                }
            }
            return result;
        }

        private void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }
                logger.Error(ex, "Via DCSBIOS.SetLastException()");
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
    }
}
