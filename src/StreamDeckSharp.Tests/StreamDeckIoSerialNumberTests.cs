using FluentAssertions;
using StreamDeckSharp.Internals;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace StreamDeckSharp.Tests
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class StreamDeckIoSerialNumberTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        [Theory]
        [MemberData(nameof(GetData))]
        internal void GettingSerialNumberWorksAsExpected(
            IHardwareInternalInfos hardware,
            byte[] featureData,
            string expectedParsedSerialNumber
        )
        {
            // Arrange
            using var context = new StreamDeckHidTestContext(hardware);

            context.Hid.ReadFeatureResonseQueue.Enqueue((hardware.SerialNumberFeatureId, true, featureData));

            // Act
            context.Board.GetSerialNumber().Should().Be(expectedParsedSerialNumber);

            // Assert
            context.Hid.ReadFeatureResonseQueue.Should().BeEmpty();
            context.Log.ToString().Should().BeEmpty();
        }

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Readability")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static IEnumerable<object[]> GetData()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            // Real world examples from my devices

            yield return new object[]
            {
                Hardware.InternalStreamDeck,
                new byte[]
                {
                    0x03, 0x55, 0xAA, 0xD3, 0x03, 0x41, 0x4C, 0x31,
                    0x35, 0x47, 0x31, 0x41, 0x30, 0x30, 0x36, 0x34,
                    0x36,
                },
                "AL15G1A00646",
            };

            yield return new object[]
            {
                Hardware.InternalStreamDeckXL,
                new byte[]
                {
                    0x06, 0x0C, 0x43, 0x4C, 0x31, 0x35, 0x4B, 0x31,
                    0x41, 0x30, 0x30, 0x31, 0x32, 0x38, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                },
                "CL15K1A00128",
            };

            yield return new object[]
            {
                Hardware.InternalStreamDeckMK2,
                new byte[]
                {
                    0x06, 0x0C, 0x44, 0x4C, 0x33, 0x30, 0x4B, 0x31,
                    0x41, 0x37, 0x39, 0x37, 0x34, 0x38, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                },
                "DL30K1A79748",
            };

            yield return new object[]
            {
                Hardware.InternalStreamDeckMini,
                new byte[]
                {
                    0x03, 0x00, 0x00, 0x00, 0x00, 0x42, 0x4C, 0x31,
                    0x39, 0x48, 0x31, 0x41, 0x30, 0x34, 0x37, 0x32,
                    0x34,
                },
                "BL19H1A04724",
            };
        }
    }
}
