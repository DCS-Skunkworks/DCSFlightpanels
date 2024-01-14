using OpenMacroBoard.SDK;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

namespace StreamDeckSharp.Internals
{
    internal class BasicHidClient : IMacroBoard
    {
        private enum PushRotaryAction
        {
            RotateCcw,
            Push,
            Release,
            RotateCw
        }

        private readonly byte[] _keyStates;
        private readonly byte[] _rotariesStates;
        private readonly object _disposeLock = new();
        private byte[] _ButtonsPushSignature = new byte[] { 0x01, 0x00, 0x08, 0x00 };
        private byte[] _ButtonsPushSignature2 = new byte[] { 0x01, 0x00, 0x20, 0x00 };
        private byte[] _ButtonsPushSignature3 = new byte[] { 0x01, 0x00, 0x0F, 0x00 };
        private byte[] _RotarySignature = new byte[] { 0x01, 0x03, 0x05, 0x00 };

        public BasicHidClient(IStreamDeckHid deckHid, IHardwareInternalInfos hardwareInformation)
        {
            DeckHid = deckHid;
            Keys = hardwareInformation.Keys;

            deckHid.ConnectionStateChanged += (s, e) => ConnectionStateChanged?.Invoke(this, e);
            deckHid.ReportReceived += DeckHid_ReportReceived;

            HardwareInfo = hardwareInformation;
            Buffer = new byte[deckHid.OutputReportLength];
            _keyStates = new byte[Keys.Count];
            _rotariesStates = new byte[4]; //magic number shortcut, sdPlus has 4 rotaries, not used to stores values right now, we'll see how it goes from here
        }

        public event EventHandler<KeyEventArgs> KeyStateChanged;
        public event EventHandler<ConnectionEventArgs> ConnectionStateChanged;
        public event EventHandler<PushRotaryEventArgs> PushRotaryStateChanged;

        public IKeyLayout Keys { get; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => DeckHid.IsConnected;

        protected IStreamDeckHid DeckHid { get; }
        protected IHardwareInternalInfos HardwareInfo { get; }
        protected byte[] Buffer { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string GetFirmwareVersion()
        {
            return ReadFeatureString(HardwareInfo.FirmwareVersionFeatureId, HardwareInfo.FirmwareReportSkip);
        }

        public string GetSerialNumber()
        {
            return ReadFeatureString(HardwareInfo.SerialNumberFeatureId, HardwareInfo.SerialNumberReportSkip);
        }

        public void SetBrightness(byte percent)
        {
            ThrowIfAlreadyDisposed();
            DeckHid.WriteFeature(HardwareInfo.GetBrightnessMessage(percent));
        }

        public virtual void SetKeyBitmap(int keyId, KeyBitmap bitmapData)
        {
            keyId = HardwareInfo.ExtKeyIdToHardwareKeyId(keyId);

            var payload = HardwareInfo.GeneratePayload(bitmapData);

            var reports = OutputReportSplitter.Split(
                payload,
                Buffer,
                HardwareInfo.ReportSize,
                HardwareInfo.HeaderSize,
                keyId,
                HardwareInfo.PrepareDataForTransmittion
            );

            foreach (var report in reports)
            {
                DeckHid.WriteReport(report);
            }
        }

        public void ShowLogo()
        {
            ThrowIfAlreadyDisposed();
            ShowLogoWithoutDisposeVerification();
        }

        protected virtual void Shutdown()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_disposeLock)
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    IsDisposed = true;
                }

                Shutdown();

                // Sleep to let the stream deck catch up.
                // Without this Sleep() the stream deck might set a key image after the logo was shown.
                // I've no idea why it's sometimes executed out of order even though the write is synchronized.
                Thread.Sleep(50);

                ShowLogoWithoutDisposeVerification();

                DeckHid.Dispose();
            }
        }

        protected void ThrowIfAlreadyDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicHidClient));
            }
        }

        private string ReadFeatureString(byte featureId, int skipBytes)
        {
            if (!DeckHid.ReadFeatureData(featureId, out var featureData))
            {
                return null;
            }

            return Encoding.UTF8.GetString(featureData, skipBytes, featureData.Length - skipBytes).Trim('\0');
        }

        private void DeckHid_ReportReceived(object sender, ReportReceivedEventArgs e)
        {
            ProcessKeys(e.ReportData);
            ProcessRotaries(e.ReportData);
        }

        private bool KeyReportSignatureEqual(byte[] signature, byte[] report)
        {
            bool equal = true;
            for (var i = 0; i < HardwareInfo.KeyReportOffset; i++)
            {
                if (signature[i] != report[i] && equal)
                    equal = false;
            }
            return equal;
        }

        private void ProcessKeys(byte[] newStates)
        {
            if (!KeyReportSignatureEqual(_ButtonsPushSignature, newStates) &&
                !KeyReportSignatureEqual(_ButtonsPushSignature2, newStates) &&
                !KeyReportSignatureEqual(_ButtonsPushSignature3, newStates)
                )
                return;

            for (var i = 0; i < _keyStates.Length; i++)
            {
                var newStatePos = i + HardwareInfo.KeyReportOffset;

                if (_keyStates[i] != newStates[newStatePos])
                {
                    var externalKeyId = HardwareInfo.HardwareKeyIdToExtKeyId(i);
                    KeyStateChanged?.Invoke(this, new KeyEventArgs(externalKeyId, newStates[newStatePos] != 0));
                    _keyStates[i] = newStates[newStatePos];
                }
            }
        }

        private int GetRotaryEnumKeyValue(int rotaryNum, PushRotaryAction action)
        {
            return rotaryNum switch
            {
                0 => action switch
                {
                    PushRotaryAction.RotateCcw => 1,
                    PushRotaryAction.Push or PushRotaryAction.Release => 2,
                    PushRotaryAction.RotateCw => 3,
                    _ => throw new Exception()
                },
                1 => action switch
                {
                    PushRotaryAction.RotateCcw => 4,
                    PushRotaryAction.Push or PushRotaryAction.Release => 5,
                    PushRotaryAction.RotateCw => 6,
                    _ => throw new Exception()
                },
                2 => action switch
                {
                    PushRotaryAction.RotateCcw => 7,
                    PushRotaryAction.Push or PushRotaryAction.Release => 8,
                    PushRotaryAction.RotateCw => 9,
                    _ => throw new Exception()
                },
                3 => action switch
                {
                    PushRotaryAction.RotateCcw => 10,
                    PushRotaryAction.Push or PushRotaryAction.Release => 11,
                    PushRotaryAction.RotateCw => 12,
                    _ => throw new Exception()
                },
                _ => throw new Exception()
            };
        }

        private void ProcessRotaries(byte[] newStates)
        {
            if (!KeyReportSignatureEqual(_RotarySignature, newStates))
                return;

            //pushed or rotated ?
            bool rotation = newStates[HardwareInfo.KeyReportOffset] == 0x01;

            for (var i = 0; i < _rotariesStates.Length; i++)
            {
                var newStatePos = i + HardwareInfo.KeyReportOffset+1;
             //   var externalKeyId = i+1;
                if (_rotariesStates[i] != newStates[newStatePos])
                {
                    switch (newStates[newStatePos])
                    {
                        //push or rotate cw
                        case 0x01:
                            if (rotation)
                                PushRotaryStateChanged?.Invoke(this, new PushRotaryEventArgs(GetRotaryEnumKeyValue(i, PushRotaryAction.RotateCw), false, false, true));
                            else //push
                            {
                                PushRotaryStateChanged?.Invoke(this, new PushRotaryEventArgs(GetRotaryEnumKeyValue(i, PushRotaryAction.Push), true, false, false));
                                _rotariesStates[i] = newStates[newStatePos];
                            }
                            break;

                        //ccw
                        case 0xff:
                            PushRotaryStateChanged?.Invoke(this, new PushRotaryEventArgs(GetRotaryEnumKeyValue(i, PushRotaryAction.RotateCcw), false, true, false));
                            break;

                        //release
                        case 0x00:
                            {
                                PushRotaryStateChanged?.Invoke(this, new PushRotaryEventArgs(GetRotaryEnumKeyValue(i, PushRotaryAction.Release), false, false, false));
                                _rotariesStates[i] = newStates[newStatePos];
                            }
                            break;
                    }
                }
            }
        }

        private void ShowLogoWithoutDisposeVerification()
        {
            DeckHid.WriteFeature(HardwareInfo.GetLogoMessage());
        }
    }
}
