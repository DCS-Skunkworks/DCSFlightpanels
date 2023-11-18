using OpenMacroBoard.SDK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

using static StreamDeckSharp.UsbConstants;

namespace StreamDeckSharp.Internals
{
    internal abstract class StreamDeckJpgHardwareBase
        : IHardwareInternalInfos
    {
        private readonly int _imgSize;
        private readonly JpegEncoder _jpgEncoder;

        private byte[] _cachedNullImage = null;

        private readonly bool _flipBitmapImage = true;

        protected StreamDeckJpgHardwareBase(GridKeyLayout keyPositions, bool flipBitmapImage)
        {
            _jpgEncoder = new JpegEncoder()
            {
                Quality = 100,
            };

            Keys = keyPositions;
            _imgSize = keyPositions.KeySize;
            _flipBitmapImage = flipBitmapImage;
        }

        public abstract int UsbProductId { get; }
        public abstract string DeviceName { get; }

        public int HeaderSize => 8;
        public int ReportSize => 1024;

        public int ExpectedFeatureReportLength => 32;
        public int ExpectedOutputReportLength => 1024;
        public int ExpectedInputReportLength => 512;

        public int KeyReportOffset => 4;
        public int UsbVendorId => VendorIds.ELGATO_SYSTEMS_GMBH;

        public byte FirmwareVersionFeatureId => 5;
        public byte SerialNumberFeatureId => 6;

        public int FirmwareReportSkip => 6;
        public int SerialNumberReportSkip => 2;

        public GridKeyLayout Keys { get; }

        /// <inheritdoc />
        /// <remarks>
        /// Unlimited because during my tests I couldn't see any glitches.
        /// See <see cref="StreamDeckHidWrapper"/> for details.
        /// </remarks>
        public virtual double BytesPerSecondLimit => double.PositiveInfinity;

        public int ExtKeyIdToHardwareKeyId(int extKeyId)
        {
            return extKeyId;
        }

        public byte[] GeneratePayload(KeyBitmap keyBitmap)
        {
            var rawData = keyBitmap.GetScaledVersion(_imgSize, _imgSize);

            if (rawData.Length == 0)
            {
                return GetNullImage();
            }

            return EncodeImageToJpg(rawData);
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
            data[0] = 2;
            data[1] = 7;
            data[2] = (byte)keyId;
            data[3] = (byte)(isLast ? 1 : 0);
            data[4] = (byte)(payloadLength & 255);
            data[5] = (byte)(payloadLength >> 8);
            data[6] = (byte)pageNumber;
        }

        public byte[] GetBrightnessMessage(byte percent)
        {
            if (percent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percent));
            }

            var buffer = new byte[]
            {
                0x03, 0x08, 0x64, 0x23, 0xB8, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xA5, 0x49, 0xCD, 0x02, 0xFE, 0x7F, 0x00, 0x00,
            };

            buffer[2] = percent;
            buffer[3] = 0x23;  // 0x23, sometimes 0x27

            return buffer;
        }

        public byte[] GetLogoMessage()
        {
            return new byte[] { 0x03, 0x02 };
        }

        private byte[] GetNullImage()
        {
            if (_cachedNullImage is null)
            {
                var rawNullImg = KeyBitmap.FromBgr24Array(1, 1, new byte[] { 0, 0, 0 }).GetScaledVersion(_imgSize, _imgSize);
                _cachedNullImage = EncodeImageToJpg(rawNullImg);
            }

            return _cachedNullImage;
        }

        private byte[] EncodeImageToJpg(ReadOnlySpan<byte> bgr24)
        {
            var flippedData = new byte[_imgSize * _imgSize * 3];
            if (_flipBitmapImage)
            {
                // Flip XY ... for some reason the JPEG devices have flipped x and y coordinates.
                for (var y = 0; y < _imgSize; y++)
                {
                    for (var x = 0; x < _imgSize; x++)
                    {
                        var x1 = _imgSize - 1 - x;
                        var y1 = _imgSize - 1 - y;

                        var pTarget = (y * _imgSize + x) * 3;
                        var pSource = (y1 * _imgSize + x1) * 3;

                        flippedData[pTarget + 0] = bgr24[pSource + 0];
                        flippedData[pTarget + 1] = bgr24[pSource + 1];
                        flippedData[pTarget + 2] = bgr24[pSource + 2];
                    }
                }
            }

            using var image = Image.LoadPixelData<Bgr24>(_flipBitmapImage ? flippedData : bgr24, _imgSize, _imgSize);

            using MemoryStream memStream = new();
            image.SaveAsJpeg(memStream, _jpgEncoder);
            return memStream.ToArray();
        }
    }
}
