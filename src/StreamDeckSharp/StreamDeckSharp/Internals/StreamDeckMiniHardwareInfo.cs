using OpenMacroBoard.SDK;
using System;
using static StreamDeckSharp.UsbConstants;

namespace StreamDeckSharp.Internals
{
    internal sealed class StreamDeckMiniHardwareInfo
        : IHardwareInternalInfos
    {
        private const int IMG_WIDTH = 80;
        private const int COLOR_CHANNELS = 3;

        private static readonly GridKeyLayout KeyPositions = new(3, 2, IMG_WIDTH, 32);

        private static readonly byte[] BmpHeader = new byte[]
        {
            0x42, 0x4d, 0x36, 0x4b, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
            0x00, 0x00, 0x50, 0x00, 0x00, 0x00, 0x50, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x4b, 0x00, 0x00, 0xc4, 0x0e,
            0x00, 0x00, 0xc4, 0x0e, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        public StreamDeckMiniHardwareInfo(int usbProductId, string deviceName)
        {
            UsbProductId = usbProductId;
            DeviceName = deviceName;
        }

        public int KeyCount => KeyPositions.Count;
        public int IconSize => IMG_WIDTH;
        public int HeaderSize => 16;
        public int ReportSize => 1024;
        public int ExpectedFeatureReportLength => 17;
        public int ExpectedOutputReportLength => 1024;
        public int ExpectedInputReportLength => 17;

        public int KeyReportOffset => 1;
        public int UsbVendorId => VendorIds.ELGATO_SYSTEMS_GMBH;
        public int UsbProductId { get; }
        public string DeviceName { get; }
        public byte FirmwareVersionFeatureId => 4;
        public byte SerialNumberFeatureId => 3;
        public int FirmwareReportSkip => 5;
        public int SerialNumberReportSkip => 5;

        public GridKeyLayout Keys
            => KeyPositions;

        /// <inheritdoc />
        /// <remarks>
        /// Unlimited because during my tests I couldn't see any glitches.
        /// See <see cref="StreamDeckHidWrapper"/> for details.
        /// </remarks>
        public double BytesPerSecondLimit => double.PositiveInfinity;

        public byte[] GeneratePayload(KeyBitmap keyBitmap)
        {
            var rawData = keyBitmap.GetScaledVersion(IMG_WIDTH, IMG_WIDTH);
            var bmp = new byte[IMG_WIDTH * IMG_WIDTH * COLOR_CHANNELS + BmpHeader.Length];

            Array.Copy(BmpHeader, 0, bmp, 0, BmpHeader.Length);

            if (rawData.Length != 0)
            {
                for (var y = 0; y < IMG_WIDTH; y++)
                {
                    for (var x = 0; x < IMG_WIDTH; x++)
                    {
                        var src = (y * IMG_WIDTH + x) * COLOR_CHANNELS;
                        var tar = ((IMG_WIDTH - x - 1) * IMG_WIDTH + y) * COLOR_CHANNELS + BmpHeader.Length;

                        bmp[tar + 0] = rawData[src + 0];
                        bmp[tar + 1] = rawData[src + 1];
                        bmp[tar + 2] = rawData[src + 2];
                    }
                }
            }

            return bmp;
        }

        public int ExtKeyIdToHardwareKeyId(int extKeyId)
        {
            return extKeyId;
        }

        public int HardwareKeyIdToExtKeyId(int hardwareKeyId)
        {
            return hardwareKeyId;
        }

        public void PrepareDataForTransmittion(
            byte[] data,
            int pageNumber,
            int payloadLength,
            int keyId,
            bool isLast
        )
        {
            data[0] = 2; // Report ID ?
            data[1] = 1; // ?
            data[2] = (byte)pageNumber;
            data[4] = (byte)(isLast ? 1 : 0);
            data[5] = (byte)(keyId + 1);
        }

        public byte[] GetBrightnessMessage(byte percent)
        {
            if (percent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percent));
            }

            var buffer = new byte[]
            {
                0x05, 0x55, 0xaa, 0xd1, 0x01, 0x64, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00,
            };

            buffer[5] = percent;
            return buffer;
        }

        public byte[] GetLogoMessage()
        {
            return new byte[] { 0x0B, 0x63 };
        }
    }
}
