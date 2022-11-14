using NonVisuals.Panels.Saitek.Panels;
using System;
using Xunit;

namespace Tests.NonVisuals
{
    public class BackLitPanelTests
    {

        [Theory]
        [InlineData("")]
        [InlineData("a2")]
        [InlineData("a23")]
        [InlineData("a234")]
        [InlineData("a2345")]
        [InlineData("a23456")]
        [InlineData("a234567")]
        [InlineData("a2345678")]
        [InlineData("a23456789")]
        [InlineData("a234567890")]
        [InlineData("a2345678901")]
        [InlineData("a23456789012")]
        [InlineData("a234567890123")]
        [InlineData("12345678901234")]
        [InlineData("abcdefghijklmn")]
        public void GetLedPosition_Throws_ArgumentOutOfRange_IfStringSmallerThan15CharsLong(string inputString)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]
        [InlineData("12345678901234a")]
        [InlineData("12345678901234_")]
        [InlineData("_______________")]

        public void GetLedPosition_Throws_FormatException_If15thCharIsNotInt(string inputString)
        {
            Assert.Throws<FormatException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]
        [InlineData("______________1")]
        [InlineData("______________12")]
        [InlineData("______________1a")]
        public void GetLedPosition_Throws_ArgumentOutOfRange_IfStringSmallerThan17CharsLong(string inputString)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]
        [InlineData("______________1aa")]
        [InlineData("______________1_a")]
        [InlineData("______________1_b")]
        [InlineData("______________0_b")]
        [InlineData("______________9_x")]
        public void GetLedPosition_Throws_FormatException_If17thCharIsNotInt(string inputString)
        {
            Assert.Throws<FormatException>(() => BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]
        [InlineData("______________1a1", BIPLedPositionEnum.Position_1_1)]
        [InlineData("______________1_1", BIPLedPositionEnum.Position_1_1)]
        [InlineData("aaaaaaaaaaaaaa1b1", BIPLedPositionEnum.Position_1_1)]
        [InlineData("aaaaaaaaaaaaaa1b2", BIPLedPositionEnum.Position_1_2)]
        [InlineData("______________1_8", BIPLedPositionEnum.Position_1_8)]
        [InlineData("______________2_1", BIPLedPositionEnum.Position_2_1)]
        [InlineData("______________2_8", BIPLedPositionEnum.Position_2_8)]
        [InlineData("______x_______3_1", BIPLedPositionEnum.Position_3_1)]
        [InlineData("___________z__3_4", BIPLedPositionEnum.Position_3_4)]
        [InlineData("__uu__________3x8", BIPLedPositionEnum.Position_3_8)]
        [InlineData("ImagePosition_3_4", BIPLedPositionEnum.Position_3_4)]
        public void GetLedPosition_ShouldReturn_ExpectedEnumValue(string inputString, BIPLedPositionEnum bIPLedPositionEnum)
        {
            Assert.Equal(bIPLedPositionEnum, BacklitPanelBIP.GetLedPosition(inputString));
        }

        [Theory]
        [InlineData("______________1_9", BIPLedPositionEnum.Position_1_1)]
        [InlineData("______________1_0", BIPLedPositionEnum.Position_1_1)]
        [InlineData("______________2_0", BIPLedPositionEnum.Position_1_1)]
        [InlineData("______________2_9", BIPLedPositionEnum.Position_1_1)]
        [InlineData("______________9_1", BIPLedPositionEnum.Position_1_1)]
        public void GetLedPosition_ShouldReturn_Position_1_1_ForUnexpectedValues(string inputString, BIPLedPositionEnum bIPLedPositionEnum)
        {
            Assert.Equal(bIPLedPositionEnum, BacklitPanelBIP.GetLedPosition(inputString));
        }
    }
}
