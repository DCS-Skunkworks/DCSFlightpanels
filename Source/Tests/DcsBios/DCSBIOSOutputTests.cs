using DCS_BIOS;
using System;
using Xunit;

namespace Tests.DcsBios
{
    public class DCSBIOSOutputTests
    {

        [Theory]
        [InlineData((uint)100, DCSBiosOutputComparison.BiggerThan, (uint)99, true)]
        [InlineData((uint)99, DCSBiosOutputComparison.BiggerThan, (uint)100, false)]
        [InlineData((uint)123455, DCSBiosOutputComparison.BiggerThan, (uint)123456, false)]
        [InlineData((uint)100, DCSBiosOutputComparison.BiggerThan, (uint)100, false)]

        [InlineData((uint)100, DCSBiosOutputComparison.LessThan, (uint)99, false)]
        [InlineData((uint)99, DCSBiosOutputComparison.LessThan, (uint)100, true)]
        [InlineData((uint)123455, DCSBiosOutputComparison.LessThan, (uint)123456, true)]
        [InlineData((uint)100, DCSBiosOutputComparison.LessThan, (uint)100, false)]

        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)99, false)]
        [InlineData((uint)99, DCSBiosOutputComparison.Equals, (uint)100, false)]
        [InlineData((uint)123455, DCSBiosOutputComparison.Equals, (uint)123456, false)]
        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)100, true)]

        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)99, true)]
        [InlineData((uint)99, DCSBiosOutputComparison.NotEquals, (uint)100, true)]
        [InlineData((uint)123455, DCSBiosOutputComparison.NotEquals, (uint)123456, true)]
        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)100, false)]
        public void CheckForValueMatch_WithUint(uint valueToCompare, DCSBiosOutputComparison comparisonType, uint OriginalValue, bool expectedResult)
        {
            DCSBIOSOutput dcsOutput = new() {
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                Mask = valueToCompare,
                SpecifiedValueInt = OriginalValue,
                DCSBiosOutputComparison = comparisonType,
            };

            Assert.Equal(expectedResult, dcsOutput.CheckForValueMatch(valueToCompare));
        }

        [Theory]
        [InlineData("AbC", "AbC", true)]
        [InlineData("AbC", "ABC", false)]
        [InlineData("ABC", "AbC", false)]
        [InlineData("AbC", "DeF", false)]
        [InlineData("(-Something Ugly&£µLike That%$^)- ", "(-Something Ugly&£µLike That%$^)- ", true)]
        [InlineData("", "", false)]
        //NullReferenceException [InlineData(null, null, true)]
        public void CheckForValueMatch_WithString(string valueToCompare, string OriginalValue, bool expectedResult)
        {
            DCSBIOSOutput dcsOutput = new()
            {
                DCSBiosOutputType = DCSBiosOutputType.StringType,
                SpecifiedValueString = OriginalValue, //should set DCSBiosOutputType before assign !
            };

            Assert.Equal(expectedResult, dcsOutput.CheckForValueMatch(valueToCompare));
        }

        [Theory]
        [InlineData(null, "AbC")]
        [InlineData(DCSBiosOutputType.IntegerType, "Abc")]
        [InlineData(DCSBiosOutputType.StringType, (uint)123)]
        [InlineData(null, null)]
        [InlineData(DCSBiosOutputType.IntegerType, (float)555.666)]
        [InlineData(DCSBiosOutputType.StringType, true)]
        public void CheckForValueMatch_WithObject_ShouldThrowException_InThoseCases(DCSBiosOutputType? dcsBiosOutputType, object obj)
        {
            DCSBIOSOutput dcsOutput = new();
            if (dcsBiosOutputType != null)
                dcsOutput.DCSBiosOutputType = (DCSBiosOutputType)dcsBiosOutputType;

            if (obj == null)
                Assert.Throws<NullReferenceException>(() => dcsOutput.CheckForValueMatch(obj));
            else
                Assert.Throws<Exception>(() => dcsOutput.CheckForValueMatch(obj));
        }
    }
}
