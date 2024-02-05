using OpenMacroBoard.SDK;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace StreamDeckSharp.Internals
{
    internal class CachedHidClient : BasicHidClient
    {
        private readonly Task _writerTask;
        private readonly ConcurrentBufferedQueue<int, byte[]> _imageQueue;
        private readonly ConditionalWeakTable<KeyBitmap, byte[]> _cacheKeyBitmaps = new();

        public CachedHidClient(IStreamDeckHid deckHid, IHardwareInternalInfos hardwareInformation)
            : base(deckHid, hardwareInformation)
        {
            _imageQueue = new ConcurrentBufferedQueue<int, byte[]>();
            _writerTask = StartBitmapWriterTask();
        }

        public override void SetKeyBitmap(int keyId, KeyBitmap bitmapData)
        {
            ThrowIfAlreadyDisposed();
            keyId = HardwareInfo.ExtKeyIdToHardwareKeyId(keyId);

            var payload = _cacheKeyBitmaps.GetValue(bitmapData, HardwareInfo.GeneratePayload);
            _imageQueue.Add(keyId, payload);
        }

        protected override void Shutdown()
        {
            _imageQueue.CompleteAdding();
            _writerTask.Wait();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _imageQueue.Dispose();
        }

        private Task StartBitmapWriterTask()
        {
            void BackgroundAction()
            {
                while (true)
                {
                    var (success, keyId, payload) = _imageQueue.Take();

                    if (!success)
                    {
                        // image queue completed
                        break;
                    }

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
            }

            return Task.Factory.StartNew(
                BackgroundAction,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }
    }
}
