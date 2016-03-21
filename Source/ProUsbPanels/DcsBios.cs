using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using ProUsbPanels.Properties;

namespace ProUsbPanels
{
    public class DcsBios
    {
        private UdpClient _udpReceiveClient = null;
        private UdpClient _udpSendClient = null;
        private Thread _listeningThread;

        private String _receiveFromIP = "239.255.50.10";
        private String _sendToIP = "127.0.0.1";
        private int _receivePort = 5010;
        private int _sendPort = 7778;
        private IPEndPoint _ipEndPointReceiver = null;
        private IPEndPoint _ipEndPointSender = null;
        private readonly object _lockExceptionObject = new object();
        private Exception _lastException = null;
        private String _receivedData = null;
        private readonly DCSProtocolParser _dcsProtocolParser;

        public DcsBios(IDCSDataListenerInterface dcsDataListenerInterface)
        {
            _dcsProtocolParser = new DCSProtocolParser(dcsDataListenerInterface);
        }

        public void InitUDP()
        {
            try
            {
                if (!String.IsNullOrEmpty(Settings.Default.DCSBiosIPFrom))
                {
                    _receiveFromIP = Settings.Default.DCSBiosIPFrom;
                }
                else
                {
                    Settings.Default.DCSBiosIPFrom = "239.255.50.10";
                }
                if (!String.IsNullOrEmpty(Settings.Default.DCSBiosIPTo))
                {
                    _sendToIP = Settings.Default.DCSBiosIPTo;
                }
                else
                {
                    Settings.Default.DCSBiosIPTo = "127.0.0.1";
                }
                if (!String.IsNullOrEmpty(Settings.Default.DCSBiosPortFrom))
                {
                    _receivePort = Convert.ToInt32(Settings.Default.DCSBiosPortFrom);
                }
                else
                {
                    Settings.Default.DCSBiosPortFrom = Convert.ToString(5010);
                }
                if (!String.IsNullOrEmpty(Settings.Default.DCSBiosPortTo))
                {
                    _sendPort = Convert.ToInt32(Settings.Default.DCSBiosPortTo);
                }
                else
                {
                    Settings.Default.DCSBiosPortTo = Convert.ToString(7778);
                }
                Settings.Default.Save();
                _ipEndPointReceiver = new IPEndPoint(IPAddress.Any, ReceivePort);
                _ipEndPointSender = new IPEndPoint(IPAddress.Parse(SendToIp), SendPort);

                _udpReceiveClient = new UdpClient();
                _udpReceiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpReceiveClient.Client.Bind(_ipEndPointReceiver);
                _udpReceiveClient.JoinMulticastGroup(IPAddress.Parse(ReceiveFromIp));

                _udpSendClient = new UdpClient();
                _udpSendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSendClient.EnableBroadcast = true;

                _listeningThread = new Thread(ReceiveData);
                _listeningThread.Start();
            }
            catch (Exception e)
            {
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
                SetLastException(e);
            }
        }

        public void Shutdown()
        {
            try
            {
                if (_listeningThread != null)
                {
                    _listeningThread.Abort();
                    _listeningThread = null;
                }
                _udpReceiveClient.Close();
                _udpSendClient.Close();
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public void ReceiveData()
        {
            try
            {
                while (true)
                {
                    var byteData = _udpReceiveClient.Receive(ref _ipEndPointReceiver);
                    for (int i = 0; i < byteData.Length; i++)
                    {
                        _dcsProtocolParser.ProcessByte(byteData[i]);
                    }
                }
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public void SendDataFunction(string stringData)
        {
            try
            {
                byte[] unicodeBytes = Encoding.Unicode.GetBytes(stringData);
                var asciiBytes = new List<byte>(stringData.Length);
                asciiBytes.AddRange(Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes));
                _udpSendClient.Send(asciiBytes.ToArray(), asciiBytes.ToArray().Length, _ipEndPointSender);
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length/sizeof (char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public bool Running()
        {
            //Crude....
            var result = _udpReceiveClient != null && _udpSendClient != null && _listeningThread != null;
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
                var message = ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace;
                if (Common.Debug)
                {
                    Common.DebugP(message);
                }
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
            get { return _receivedData; }
        }

        public int SendPort
        {
            get { return _sendPort; }
            set { _sendPort = value; }
        }

        public int ReceivePort
        {
            get { return _receivePort; }
            set { _receivePort = value; }
        }

        public string SendToIp
        {
            get { return _sendToIP; }
            set { _sendToIP = value; }
        }

        public string ReceiveFromIp
        {
            get { return _receiveFromIP; }
            set { _receiveFromIP = value; }
        }
    }

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

    /*
    internal static class DCSBiosOutputLocator
    {

        public enum JsonDcsBiosEnum
        {
            NONE,
            IDENTIFIER_FOUND,
            WAIT_FOR_ADDRESS,
            WAIT_FOR_CONTROL_DESC,
            WAIT_FOR_MASK,
            WAIT_FOR_SHIFT_VALUE,
            WAIT_FOR_MAX_VALUE,
            WAIT_FOR_TYPE
        }

        public static DCSBiosOutput FindInDirectory(String airframe, String directory, String controlString)
        {
            DCSBiosOutput result = new DCSBiosOutput();
            var resultCounter = 0;
            try
            {
                var directoryInfo = new DirectoryInfo(directory);
                Common.DebugP("Searching for " + airframe + ".json in directory " + directory);
                var files = directoryInfo.EnumerateFiles(airframe + ".json", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    Common.DebugP("Opening " + file.Name);
                    var reader = file.OpenText();
                    var text = reader.ReadToEnd();
                    var jsonReader = new JsonTextReader(new StringReader(text));
                    var state = JsonDcsBiosEnum.NONE;

                    /*int? address = 0;
                    var description = "";
                    int? mask = 0;
                    int? maxValue = 0;
                    int? shift_by = 0;
                    var suffix = "";
                    var controlType = "";
                    
                    while (jsonReader.Read())
                    {
                        if (jsonReader.Value != null)
                        {
                            if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("identifier"))
                            {
                                if (state == JsonDcsBiosEnum.NONE && jsonReader.ReadAsString().Equals(controlString))
                                {
                                    //We have found the correct control!
                                    Common.DebugP("Found : " + controlString + " " + jsonReader.Value);
                                    resultCounter++;
                                    state = JsonDcsBiosEnum.WAIT_FOR_ADDRESS;
                                }
                            }
                            if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("identifier"))
                            {
                                if (state == JsonDcsBiosEnum.NONE && jsonReader.ReadAsString().Equals(controlString))
                                {
                                    //We have found the correct control!
                                    Common.DebugP("Found : " + controlString + " " + jsonReader.Value);
                                    resultCounter++;
                                    state = JsonDcsBiosEnum.WAIT_FOR_ADDRESS;
                                }
                            }
                            if (state == JsonDcsBiosEnum.WAIT_FOR_ADDRESS && jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("address"))
                            {
                                var value = jsonReader.ReadAsInt32();
                                result.Address = value == null ? (uint) default(int) : (uint) value.GetValueOrDefault(0);
                                Common.DebugP("Found : " + state + " : " + value);
                                resultCounter++;
                                state = JsonDcsBiosEnum.WAIT_FOR_CONTROL_DESC;
                            }
                            if (state == JsonDcsBiosEnum.WAIT_FOR_CONTROL_DESC && jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("description"))
                            {
                                var value = jsonReader.ReadAsString();
                                result.ActionDescription = value;
                                Common.DebugP("Found : " + state + " : " + value);
                                resultCounter++;
                                state = JsonDcsBiosEnum.WAIT_FOR_MASK;
                            }
                            if (state == JsonDcsBiosEnum.WAIT_FOR_MASK && jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("mask"))
                            {
                                var value = jsonReader.ReadAsInt32();
                                result.Mask = value == null ? (uint) default(int) : (uint) value.GetValueOrDefault(0);
                                Common.DebugP("Found : " + state + " : " + value);
                                resultCounter++;
                                state = JsonDcsBiosEnum.WAIT_FOR_MAX_VALUE;
                            }
                            if (state == JsonDcsBiosEnum.WAIT_FOR_MAX_VALUE && jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("max_value"))
                            {
                                var value = jsonReader.ReadAsInt32();
                                result.MaxValue = value == null ? default(int) : value.GetValueOrDefault(0);
                                Common.DebugP("Found : " + state + " : " + value);
                                resultCounter++;
                                state = JsonDcsBiosEnum.WAIT_FOR_SHIFT_VALUE;
                            }
                            if (state == JsonDcsBiosEnum.WAIT_FOR_SHIFT_VALUE && jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("shift_by"))
                            {
                                var value = jsonReader.ReadAsInt32();
                                result.Shiftvalue = value == null ? default(int) : value.GetValueOrDefault(0);
                                Common.DebugP("Found : " + state + " : " + value);
                                resultCounter++;
                                state = JsonDcsBiosEnum.WAIT_FOR_TYPE;
                            }
                            if (state == JsonDcsBiosEnum.WAIT_FOR_TYPE && jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.Equals("type"))
                            {
                                var value = jsonReader.ReadAsString();
                                result.Type = value;
                                Common.DebugP("Found : " + state + " : " + value);
                                resultCounter++;
                                state = JsonDcsBiosEnum.NONE;
                                break;
                            }
                        }
                    }
                }
            }
            catch
                (Exception ex)
            {
                MessageBox.Show("0xFF" + ex.Message);
                return null;
            }
            if (resultCounter < 7)
            {
                return null;
            }
            return result;
        }
    }

*/
    internal class DCSBiosOutput
    {
        private String _controlTypeDescription;
        private String _actionDescription;
        private int _maxValue = 0;
        private uint _address = 0;
        private uint _mask = 0;
        private int _shiftvalue = 0;
        //private uint _data = 0;
        private uint _value = 0;
        private bool _hasBeenSet = false;
        private String _type;

        public uint Address
        {
            get { return _address; }
            set
            {
                _address = value;
                _hasBeenSet = true;
            }
        }

        public uint Mask
        {
            get { return _mask; }
            set
            {
                _mask = value;
                _hasBeenSet = true;
            }
        }

        public int Shiftvalue
        {
            get { return _shiftvalue; }
            set
            {
                _shiftvalue = value;
                _hasBeenSet = true;
            }
        }

        /*
        public uint Data
        {
            get { return _data; }
            set
            {
                _data = value;
                _hasBeenSet = true;
            }
        }
        */

        public uint Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _hasBeenSet = true;
            }
        }

        /*
        public uint ValueBasedOnData
        {
            get
            {
                return (_data & _mask) >> _shiftvalue;
            }
        }
        */

        public bool CheckForValueMatch(uint data)
        {
            var value = (data & Mask) >> Shiftvalue;
            return value == _value;
        }

        public bool HasBeenSet
        {
            get
            {
                return _hasBeenSet;
            }
        }

        public override String ToString()
        {
            return "DCSBiosOutput{" + Common.ToPaddedHexString(_address) + "|" + Common.ToPaddedHexString(_mask) + "|" + _shiftvalue + "|" + _value + "}";
        }

        public void ImportString(String str)
        {
            //DCSBiosOutput{0x1472|0x4000|14|1}
            var value = str;
            if (String.IsNullOrEmpty(str))
            {
                throw new Exception("DCSBiosOutput cannot import null string.");
            }
            if (!str.StartsWith("DCSBiosOutput{") || !str.EndsWith("}"))
            {
                throw new Exception("DCSBiosOutput cannot import string : " + str);
            }
            value = value.Substring(value.IndexOf("{", StringComparison.InvariantCulture) + 1);
            //"0x1472|0x4000|14|0x1243}";
            value = value.Substring(0, value.Length - 1);
            //"0x1472|0x4000|14|0x1243";
            var entries = value.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
            _address = (uint) Convert.ToInt32(entries[0], 16);
            _mask = (uint) Convert.ToInt32(entries[1], 16);
            _shiftvalue = int.Parse(entries[2]);
            _value = (uint) Convert.ToInt32(entries[3], 16);
            _hasBeenSet = true;
        }

        public string ActionDescription
        {
            get { return _actionDescription; }
            set { _actionDescription = value; }
        }

        public string ControlTypeDescription
        {
            get { return _controlTypeDescription; }
            set { _controlTypeDescription = value; }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }

    internal class DCSProtocolParser
    {
        private DCSBiosStateEnum _state;
        private uint _address;
        private uint _count;
        private uint _data;
        private byte _syncByteCount;
        private readonly IDCSDataListenerInterface _dcsDataListenerInterface;

        internal DCSProtocolParser(IDCSDataListenerInterface dcsDataListenerInterface)
        {
            _dcsDataListenerInterface = dcsDataListenerInterface;
            _state = DCSBiosStateEnum.WAIT_FOR_SYNC;
            _syncByteCount = 0;
        }

        internal void ProcessByte(byte b)
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
                    _address = (uint) (b << 8) | _address;
                    _state = _address != 0x5555 ? DCSBiosStateEnum.COUNT_LOW : DCSBiosStateEnum.WAIT_FOR_SYNC;
                    break;
                case DCSBiosStateEnum.COUNT_LOW:
                    _count = b;
                    _state = DCSBiosStateEnum.COUNT_HIGH;
                    break;
                case DCSBiosStateEnum.COUNT_HIGH:
                    _count = (uint) (b << 8) | _count;
                    _state = DCSBiosStateEnum.DATA_LOW;
                    break;
                case DCSBiosStateEnum.DATA_LOW:
                    _data = b;
                    _count--;
                    _state = DCSBiosStateEnum.DATA_HIGH;
                    break;
                case DCSBiosStateEnum.DATA_HIGH:
                    _data = (uint) (b << 8) | _data;
                    _count--;
                    _dcsDataListenerInterface.DcsBiosDataReceived(_address, _data);
                    _address += 2;
                    if (_count == 0)
                        _state = DCSBiosStateEnum.ADDRESS_LOW;
                    else
                        _state = DCSBiosStateEnum.DATA_LOW;
                    break;
            }
            if (b == 0x55)
                _syncByteCount++;
            else
                _syncByteCount = 0;
            if (_syncByteCount == 4)
            {
                _state = DCSBiosStateEnum.ADDRESS_LOW;
                _syncByteCount = 0;
            }
        }
    }
}
