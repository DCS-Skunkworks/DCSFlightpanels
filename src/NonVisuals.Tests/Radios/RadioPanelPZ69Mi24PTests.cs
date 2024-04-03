using DCS_BIOS.misc;
using NonVisuals.Radios;
using Xunit;

namespace NonVisuals.Tests.Radios 
{
    public class RadioPanelPZ69Mi24PTests
    {

        [Theory]
        [InlineData(0, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(0, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(0, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(1, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(1, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(1, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(2, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(2, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(2, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(3, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(3, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(3, 9, DCSBIOSCommand.DCSBIOS_INCREMENT)]

        [InlineData(4, 0, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(4, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(4, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(5, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 1, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(5, 5, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(5, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(6, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 2, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(6, 6, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(6, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(7, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 3, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(7, 7, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(7, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(8, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 4, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(8, 8, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(8, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        [InlineData(9, 0, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 1, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 2, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 3, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 4, DCSBIOSCommand.DCSBIOS_DECREMENT)]
        [InlineData(9, 5, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 6, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 7, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 8, DCSBIOSCommand.DCSBIOS_INCREMENT)]
        [InlineData(9, 9, DCSBIOSCommand.DCSBIOS_DECREMENT)]

        public void GetCommandDirectionFor0To9Dials_ShouldReturn_ExpectedValues(int desiredDialPosition, uint actualDialPosition, string expectedValue)
        {
            Assert.Equal(expectedValue, RadioPanelPZ69Mi24P.GetCommandDirectionFor0To9Dials(desiredDialPosition, actualDialPosition));
        }
    }
}
