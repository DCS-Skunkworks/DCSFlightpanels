using NonVisuals.Radios;
using System;
using Xunit;

namespace Tests.NonVisuals
{
    public class RadioPanelPZ69A10CTests
    {

        [Theory]
        [InlineData(0, 0, "")]
        [InlineData(0, 555, "")]

        [InlineData(1, 0, "3")]
        [InlineData(1, 1, "4")]
        [InlineData(1, 2, "5")]
        [InlineData(1, 3, "6")]
        [InlineData(1, 4, "7")]
        [InlineData(1, 5, "8")]
        [InlineData(1, 6, "9")]
        [InlineData(1, 7, "10")]
        [InlineData(1, 8, "11")]
        [InlineData(1, 9, "12")]
        [InlineData(1, 10, "13")]
        [InlineData(1, 11, "14")]
        [InlineData(1, 12, "15")]

        [InlineData(2, 0, "0")]
        [InlineData(2, 1, "1")]
        [InlineData(2, 2, "2")]
        [InlineData(2, 3, "3")]
        [InlineData(2, 4, "4")]
        [InlineData(2, 5, "5")]
        [InlineData(2, 6, "6")]
        [InlineData(2, 7, "7")]
        [InlineData(2, 8, "8")]
        [InlineData(2, 9, "9")]
        [InlineData(2, 10, "10")]
        [InlineData(2, 11, "11")]
        [InlineData(2, 12, "12")]
        [InlineData(2, 13, "13")]

        [InlineData(3, 0, "0")]
        [InlineData(3, 1, "1")]
        [InlineData(3, 2, "2")]
        [InlineData(3, 3, "3")]
        [InlineData(3, 4, "4")]
        [InlineData(3, 5, "5")]
        [InlineData(3, 6, "6")]
        [InlineData(3, 7, "7")]
        [InlineData(3, 8, "8")]
        [InlineData(3, 9, "9")]
        [InlineData(3, 10, "10")]
        [InlineData(3, 11, "11")]
        [InlineData(3, 12, "12")]
        [InlineData(3, 13, "13")]

        [InlineData(4, 0, "0")]
        [InlineData(4, 1, "2")]
        [InlineData(4, 2, "5")]
        [InlineData(4, 3, "7")]

        [InlineData(5, 0, "")]
        [InlineData(6, 666, "")]
        public void GetVhfAmDialFrequencyForPosition_ShouldReturn_ExpectedValues(int dial, uint position, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10C.GetVhfAmDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 13)]
        [InlineData(1, 555)]
        [InlineData(4, 4)]
        [InlineData(4, 666)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetVhfAmDialFrequencyForPosition_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And4(int dial, uint position)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10C.GetVhfAmDialFrequencyForPosition(dial, position));
        }


        [Theory]
        [InlineData(1, 0, "2")]
        [InlineData(1, 1, "3")]
        [InlineData(1, 2, "0")]

        [InlineData(2, 0, "0")]
        [InlineData(2, 1, "1")]
        [InlineData(2, 5551, "5551")]

        [InlineData(3, 0, "0")]
        [InlineData(3, 1, "1")]
        [InlineData(3, 6662, "6662")]

        [InlineData(4, 0, "0")]
        [InlineData(4, 1, "1")]
        [InlineData(4, 7773, "7773")]

        [InlineData(5, 0, "00")]
        [InlineData(5, 1, "25")]
        [InlineData(5, 2, "50")]
        [InlineData(5, 3, "75")]

        public void GetUhfDialFrequencyForPosition_ShouldReturn_ExpectedValues(int dial, uint position, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10C.GetUhfDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(1, 555)]
        [InlineData(5, 4)]
        [InlineData(5, 666)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetUhfDialFrequencyForPosition_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And5(int dial, uint position)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10C.GetUhfDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(0, 0, "")]
        [InlineData(0, 555, "")]

        [InlineData(1, 0, "3")]
        [InlineData(1, 1, "4")]
        [InlineData(1, 2, "5")]
        [InlineData(1, 3, "6")]
        [InlineData(1, 4, "7")]
        [InlineData(1, 5, "8")]
        [InlineData(1, 6, "9")]
        [InlineData(1, 7, "10")]
        [InlineData(1, 8, "11")]
        [InlineData(1, 9, "12")]
        [InlineData(1, 10, "13")]
        [InlineData(1, 11, "14")]
        [InlineData(1, 12, "15")]

        [InlineData(2, 0, "0")]
        [InlineData(2, 1, "1")]
        [InlineData(2, 2, "2")]
        [InlineData(2, 3, "3")]
        [InlineData(2, 4, "4")]
        [InlineData(2, 5, "5")]
        [InlineData(2, 6, "6")]
        [InlineData(2, 7, "7")]
        [InlineData(2, 8, "8")]
        [InlineData(2, 9, "9")]
        [InlineData(2, 10, "10")]
        [InlineData(2, 11, "11")]
        [InlineData(2, 12, "12")]
        [InlineData(2, 13, "13")]

        [InlineData(3, 0, "0")]
        [InlineData(3, 1, "1")]
        [InlineData(3, 2, "2")]
        [InlineData(3, 3, "3")]
        [InlineData(3, 4, "4")]
        [InlineData(3, 5, "5")]
        [InlineData(3, 6, "6")]
        [InlineData(3, 7, "7")]
        [InlineData(3, 8, "8")]
        [InlineData(3, 9, "9")]
        [InlineData(3, 10, "10")]
        [InlineData(3, 11, "11")]
        [InlineData(3, 12, "12")]
        [InlineData(3, 13, "13")]

        [InlineData(4, 0, "00")]
        [InlineData(4, 1, "25")]
        [InlineData(4, 2, "50")]
        [InlineData(4, 3, "75")]

        [InlineData(5, 0, "")]
        [InlineData(6, 666, "")]
        public void GetVhfFmDialFrequencyForPosition_ShouldReturn_ExpectedValues(int dial, uint position, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10C.GetVhfFmDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 13)]
        [InlineData(1, 555)]
        [InlineData(4, 4)]
        [InlineData(4, 666)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetVhfFmDialFrequencyForPosition_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And4(int dial, uint position)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10C.GetVhfFmDialFrequencyForPosition(dial, position));
        }
    }
}
