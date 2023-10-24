using System;
using NonVisuals.Panels.Saitek.Panels;
using Xunit;

namespace DCSFPTests.NonVisuals
{
    public class BackLitPanelTests
    {
        /*ImagePosition21*/
        [Theory]//             ImagePosition21
        [InlineData("1234567890123a1")]
        [InlineData("_______________")]

        public void GetLedPosition_Throws_FormatException_If13thCharIsNotInt(string inputString)
        {
            Assert.Throws<FormatException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]//             ImagePosition21
        [InlineData("12345678901232a")]
        [InlineData("_______________")]

        public void GetLedPosition_Throws_FormatException_If14thCharIsNotInt(string inputString)
        {
            Assert.Throws<FormatException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]//             ImagePosition21
        [InlineData("_____________1")]
        [InlineData("____________12")]
        [InlineData("___________1a")]
        public void GetLedPosition_Throws_ArgumentOutOfRange_IfStringSmallerThan15CharsLong(string inputString)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]//             ImagePosition21
        [InlineData("______________1aa")]
        [InlineData("______________1_a")]
        [InlineData("______________1_b")]
        [InlineData("______________0_b")]
        [InlineData("______________9_x")]
        public void GetLedPosition_Throws_FormatException_If17thCharIsNotInt(string inputString)
        {
            Assert.Throws<FormatException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]//             ImagePosition21
        [InlineData("_____________11", BIPLedPositionEnum.Position_1_1)]
        [InlineData("aaaaaaaaaaaaa11", BIPLedPositionEnum.Position_1_1)]
        [InlineData("aaaaaaaaaaaaa12", BIPLedPositionEnum.Position_1_2)]
        [InlineData("_____________18", BIPLedPositionEnum.Position_1_8)]
        [InlineData("_____________21", BIPLedPositionEnum.Position_2_1)]
        [InlineData("_____________28", BIPLedPositionEnum.Position_2_8)]
        [InlineData("______x______31", BIPLedPositionEnum.Position_3_1)]
        [InlineData("___________z_34", BIPLedPositionEnum.Position_3_4)]
        [InlineData("__uu_________38", BIPLedPositionEnum.Position_3_8)]
        [InlineData("ImagePosition34", BIPLedPositionEnum.Position_3_4)]
        public void GetLedPosition_ShouldReturn_ExpectedEnumValue(string inputString, BIPLedPositionEnum bipLedPositionEnum)
        {
            Assert.Equal(bipLedPositionEnum, BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]//             ImagePosition21
        [InlineData("_____________19", BIPLedPositionEnum.Position_1_1)]
        [InlineData("_____________10", BIPLedPositionEnum.Position_1_1)]
        [InlineData("_____________20", BIPLedPositionEnum.Position_1_1)]
        [InlineData("_____________29", BIPLedPositionEnum.Position_1_1)]
        [InlineData("_____________91", BIPLedPositionEnum.Position_1_1)]
        public void GetLedPosition_ShouldReturn_Position_1_1_ForUnexpectedValues(string inputString, BIPLedPositionEnum bipLedPositionEnum)
        {
            Assert.Equal(bipLedPositionEnum, BacklitPanelBIP.GetLedPosition(inputString));
        }
    }
}
