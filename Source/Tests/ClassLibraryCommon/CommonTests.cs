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


        [Theory]
        [InlineData(@"\\testX", @"\\testX", @".\")]
        [InlineData(@"\\testX\1", @"\\testX\1", @".\")]
        [InlineData(@"\\testX", @"\\testY", @"file:\\testy\")]
        [InlineData(@"\\testX\1", @"\\testY\1", @"file:\\testy\1")]
        [InlineData(@"\\testX\1\2", @"\\testX\1", @"..\1")]
        [InlineData(@"\\testX\1\2\3", @"\\testX\1", @"..\..\1")]
        [InlineData(@"\\testX\1\2\3", @"\\testX\1\2", @"..\2")]
        [InlineData(@"\\testX\1\2\3\", @"\\testX\1\2\", @"..\")]
        [InlineData(@"\\testX\1\2\3", @"\\testX\1\2\", @".\")]
        [InlineData(@"\\testX\1\2\3\", @"\\testX\1\2", @"..\..\2")]
        [InlineData(@"\\testX\1\2\3\file.txt", @"\\testX\1\2\file.txt", @"..\file.txt")]
        [InlineData(@"\\testX\1", @"\\testX\1\2", @"1\2")]
        [InlineData(@"\\testX\1", @"\\testX\1\2\3", @"1\2\3")]
        [InlineData(@"\\testX\1\2", @"\\testX\1\2\3", @"2\3")]
        [InlineData(@"\\testX\1\2\file.txt", @"\\testX\1\2\3\file.txt", @"3\file.txt")]
        public void GetRelativePath_ShouldReturn_Expectedvalue(string relativeTo, string path, string expected)
        {
            Assert.Equal(expected, Common.GetRelativePath(relativeTo, path));
        }
        
    }
}
