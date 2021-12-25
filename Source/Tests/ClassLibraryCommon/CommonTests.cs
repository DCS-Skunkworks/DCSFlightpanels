using ClassLibraryCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.ClassLibraryCommon
{
    public class CommonTests
    {
        [Theory]
        [InlineData("123","123")]
        [InlineData("123 ABCD", "123 ABCD")]
        [InlineData("", "")]
        [InlineData("RMENU + LCONTROL", "RMENU ")]
        [InlineData("XxxX RMENU + LCONTROL YyY", "XxxX RMENU  YyY")]
        [InlineData("XxxXRMENU + LCONTROLYyY", "XxxXRMENU YyY")]
        [InlineData("LCONTROL + RMENU", " RMENU")]
        [InlineData("XxxX LCONTROL + RMENU YyY", "XxxX  RMENU YyY")]
        [InlineData("XxxXLCONTROL + RMENUYyY", "XxxX RMENUYyY")]
        public void RemoveLControl_ShouldReturn_ExpectedString(string inputString, string expected)
        {
            Assert.Equal(expected, Common.RemoveLControl(inputString));
        }

    }
}
