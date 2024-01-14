using FluentAssertions;
using OpenMacroBoard.SDK;
using Xunit;

namespace StreamDeckSharp.Tests
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class StreamDeckInterfaceExtensionsTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(11, true)]
        [InlineData(200, false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void TestKeyEventArgs(int keyId, bool isDown)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var eventArg = new KeyEventArgs(keyId, isDown);
            eventArg.Key.Should().Be(keyId);
            eventArg.IsDown.Should().Be(isDown);
        }

        [Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void ConnectionEventArgsStoresTheValueAsExpected()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var eventArg = new ConnectionEventArgs(false);
            eventArg.NewConnectionState.Should().BeFalse();

            eventArg = new ConnectionEventArgs(true);
            eventArg.NewConnectionState.Should().BeTrue();
        }
    }
}
