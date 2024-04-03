using NonVisuals.Radios;
using System;
using DCS_BIOS.misc;
using Xunit;

namespace NonVisuals.Tests.Radios
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
            Assert.Equal(expectedValue, RadioPanelPZ69A10CII.GetUhfDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(1, 555)]
        [InlineData(5, 4)]
        [InlineData(5, 666)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetUhfDialFrequencyForPosition_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And5(int dial, uint position)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10CII.GetUhfDialFrequencyForPosition(dial, position));
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
            Assert.Equal(expectedValue, RadioPanelPZ69A10CII.GetVhfFmDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 13)]
        [InlineData(1, 555)]
        [InlineData(4, 4)]
        [InlineData(4, 666)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetVhfFmDialFrequencyForPosition_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And4(int dial, uint position)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10CII.GetVhfFmDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 0, "108")]
        [InlineData(1, 1, "109")]
        [InlineData(1, 2, "110")]
        [InlineData(1, 3, "111")]

        [InlineData(2, 0, "10")]
        [InlineData(2, 1, "15")]
        [InlineData(2, 2, "30")]
        [InlineData(2, 3, "35")]
        [InlineData(2, 4, "50")]
        [InlineData(2, 5, "55")]
        [InlineData(2, 6, "70")]
        [InlineData(2, 7, "75")]
        [InlineData(2, 8, "90")]
        [InlineData(2, 9, "95")]

        public void GetILSDialFrequencyForPosition_ShouldReturn_ExpectedValues(int dial, uint position, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10CII.GetILSDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 4)]
        [InlineData(1, 555)]
        [InlineData(2, 10)]
        [InlineData(2, 666)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetILSDialFrequencyForPosition_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And4(int dial, uint position)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10CII.GetILSDialFrequencyForPosition(dial, position));
        }

        [Theory]
        [InlineData(1, 108, 0)]
        [InlineData(1, 109, 1)]
        [InlineData(1, 110, 2)]
        [InlineData(1, 111, 3)]

        [InlineData(2, 10, 0)]
        [InlineData(2, 15, 1)]
        [InlineData(2, 30, 2)]
        [InlineData(2, 35, 3)]
        [InlineData(2, 50, 4)]
        [InlineData(2, 55, 5)]
        [InlineData(2, 70, 6)]
        [InlineData(2, 75, 7)]
        [InlineData(2, 90, 8)]
        [InlineData(2, 95, 9)]
        public void GetILSDialPosForFrequency_ShouldReturn_ExpectedValues(int dial, int frequency, int expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10CII.GetILSDialPosForFrequency(dial, frequency));
        }

        [Theory]
        [InlineData(1, -1)]
        [InlineData(1, 107)]
        [InlineData(1, 112)]
        [InlineData(1, 5551)]

        [InlineData(2, -1)]
        [InlineData(2, 9)]
        [InlineData(2, 96)]
        [InlineData(2, 6661)]
        ///previous Non-exhaustive switches that throwed exception is replaced by default handling in switches that also throws a better exception
        public void GetILSDialPosForFrequency_ThrowsException_For_UnexpectedPositionValue_For_DialValue1And4(int dial, int frequency)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RadioPanelPZ69A10CII.GetILSDialPosForFrequency(dial, frequency));
        }

        [Theory]

        [InlineData(0, 0, null)]
        [InlineData(0, 1,  DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 10, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 11, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 12, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        
        [InlineData(1, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 1, null)]
        [InlineData(1, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 10, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 11, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 12, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(2, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 2, null)]
        [InlineData(2, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 10, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 11, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 12, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(3, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 3, null)]
        [InlineData(3, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 10, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 11, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 12, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(4, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 4, null)]
        [InlineData(4, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 10, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 11, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 12, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(5, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 5, null)]
        [InlineData(5, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 10, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 11, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 12, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        
        [InlineData(6, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 6, null)]
        [InlineData(6, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 10, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 11, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 12, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(7, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 7, null)]
        [InlineData(7, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 10, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 11, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 12, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(8, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 8, null)]
        [InlineData(8, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 10, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 11, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 12, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(9, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 9, null)]
        [InlineData(9, 10, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 11, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 12, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(10, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(10, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(10, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(10, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(10, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(10, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(10, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(10, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(10, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(10, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(10, 10, null)]
        [InlineData(10, 11, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(10, 12, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(11, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(11, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(11, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(11, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(11, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(11, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(11, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(11, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(11, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(11, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(11, 10, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(11, 11, null)]
        [InlineData(11, 12, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(12, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(12, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(12, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(12, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(12, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(12, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(12, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(12, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(12, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(12, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(12, 10, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(12, 11, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(12, 12, null)]
        public void GetCommandDirectionForVhfDial1_ShouldReturn_ExpectedValues(int desiredDialPosition, uint actualDialPosition, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10CII.GetCommandDirectionForVhfDial1(desiredDialPosition, actualDialPosition));
        }

        [Theory]
        [InlineData(1, 13)]
        [InlineData(13, 0)]
        [InlineData(555, 666)]
        [InlineData(-1, 12)]
        public void GetCommandDirectionForVhfDial1_ThrowsException_For_UnexpectedValues(int desiredDialPosition, uint actualDialPosition)
        {
            Assert.Throws<Exception>(() => RadioPanelPZ69A10CII.GetCommandDirectionForVhfDial1(desiredDialPosition, actualDialPosition));
        }
        
        [Theory]
        [InlineData(0, 0, null)]
        [InlineData(0, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(1, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 1, null)]
        [InlineData(1, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(2, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 2, null)]
        [InlineData(2, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(3, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 3, null)]
        [InlineData(3, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(4, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 4, null)]
        [InlineData(4, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(5, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 5, null)]
        [InlineData(5, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(6, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 6, null)]
        [InlineData(6, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(7, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 7, null)]
        [InlineData(7, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(8, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 8, null)]
        [InlineData(8, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(9, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 9, null)]
        public void GetCommandDirectionForVhfDial23_ShouldReturn_ExpectedValues(int desiredDialPosition, uint actualDialPosition, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69A10CII.GetCommandDirectionForVhfDial23(desiredDialPosition, actualDialPosition));
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 0)]
        [InlineData(555, 666)]
        [InlineData(-1, 6)]
        public void GetCommandDirectionForVhfDial23_ThrowsException_For_UnexpectedValues(int desiredDialPosition, uint actualDialPosition)
        {
            Assert.Throws<Exception>(() => RadioPanelPZ69A10CII.GetCommandDirectionForVhfDial23(desiredDialPosition, actualDialPosition));
        }
    }
}
