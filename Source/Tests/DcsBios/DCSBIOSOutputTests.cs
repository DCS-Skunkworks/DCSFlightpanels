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
            DCSBIOSOutput dcsOutput = new()
            {
                Address = 99,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                Mask = valueToCompare,
                SpecifiedValueUInt = OriginalValue,
                DCSBiosOutputComparison = comparisonType,
            };

            Assert.Equal(expectedResult, dcsOutput.UIntConditionIsMet(99, valueToCompare));
        }

        [Theory]
        [InlineData("AbC", "AbC", false)]
        [InlineData("AbC", "ABC", true)]
        [InlineData("ABC", "AbC", true)]
        [InlineData("AbC", "DeF", true)]
        [InlineData("(-Something Ugly&£µLike That%$^)- ", "(-Something Ugly&£µLike That%$^)- ", false)]
        [InlineData("", "", false)]
        
        public void CheckIfValueHasChanged_WithString(string valueToCompare, string originalValue, bool expectedResult)
        {
            DCSBIOSOutput dcsOutput = new()
            {
                Address = 99,
                DCSBiosOutputType = DCSBiosOutputType.StringType,
                LastStringValue = originalValue, //should set DCSBiosOutputType before assign !
            };

            Assert.Equal(expectedResult, dcsOutput.StringValueHasChanged(99, valueToCompare));
        }
        
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
        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)100, false)] //<-- ?? != compared to CheckForValueMatch_WithUint 

        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)99, true)]
        [InlineData((uint)99, DCSBiosOutputComparison.NotEquals, (uint)100, true)]
        [InlineData((uint)123455, DCSBiosOutputComparison.NotEquals, (uint)123456, true)]
        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)100, false)]
        public void CheckForValueMatchAndChange_WithUint_LastIntValue_Is_OriginalValue(uint valueToCompare, DCSBiosOutputComparison comparisonType, uint OriginalValue, bool expectedResult)
        {
            DCSBIOSOutput dcsOutput = new()
            {
                Address = 99,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                Mask = valueToCompare,
                SpecifiedValueUInt = OriginalValue,
                DCSBiosOutputComparison = comparisonType,
                LastUIntValue = OriginalValue, //<-- != compared to CheckForValueMatch_WithUint 
            };

            Assert.Equal(expectedResult, dcsOutput.UIntConditionIsMet(99, valueToCompare));
            Assert.Equal(valueToCompare, dcsOutput.LastUIntValue);
        }

        [Theory]
        [InlineData((uint)100, DCSBiosOutputComparison.BiggerThan, (uint)99)]
        [InlineData((uint)99, DCSBiosOutputComparison.BiggerThan, (uint)100)]
        [InlineData((uint)123455, DCSBiosOutputComparison.BiggerThan, (uint)123456)]
        [InlineData((uint)100, DCSBiosOutputComparison.BiggerThan, (uint)100)]

        [InlineData((uint)100, DCSBiosOutputComparison.LessThan, (uint)99)]
        [InlineData((uint)99, DCSBiosOutputComparison.LessThan, (uint)100)]
        [InlineData((uint)123455, DCSBiosOutputComparison.LessThan, (uint)123456)]
        [InlineData((uint)100, DCSBiosOutputComparison.LessThan, (uint)100)]

        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)99)]
        [InlineData((uint)99, DCSBiosOutputComparison.Equals, (uint)100)]
        [InlineData((uint)123455, DCSBiosOutputComparison.Equals, (uint)123456)]
        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)100)]

        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)99)]
        [InlineData((uint)99, DCSBiosOutputComparison.NotEquals, (uint)100)]
        [InlineData((uint)123455, DCSBiosOutputComparison.NotEquals, (uint)123456)]
        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)100)]
        public void CheckForValueMatchAndChange_WithUint_LastIntValue_Is_ValueToCompare(uint valueToCompare, DCSBiosOutputComparison comparisonType, uint OriginalValue)
        {
            DCSBIOSOutput dcsOutput = new()
            {
                Address = 99,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                Mask = valueToCompare,
                SpecifiedValueUInt = OriginalValue,
                DCSBiosOutputComparison = comparisonType,
                LastUIntValue = valueToCompare, //<-- != compared to CheckForValueMatch_WithUint 
            };
            //always returns false
            Assert.False(dcsOutput.UIntConditionIsMet(99, valueToCompare));
            Assert.Equal(valueToCompare, dcsOutput.LastUIntValue);
        }

        [Theory]
        [InlineData((uint)100, DCSBiosOutputComparison.BiggerThan, (uint)99, (uint)1, true)]
        [InlineData((uint)99, DCSBiosOutputComparison.BiggerThan, (uint)100, (uint)5, false)]
        [InlineData((uint)123455, DCSBiosOutputComparison.BiggerThan, (uint)123456, (uint)8, false)]
        [InlineData((uint)100, DCSBiosOutputComparison.BiggerThan, (uint)100, (uint)555, false)]

        [InlineData((uint)100, DCSBiosOutputComparison.LessThan, (uint)99, (uint)9991, false)]
        [InlineData((uint)99, DCSBiosOutputComparison.LessThan, (uint)100, (uint)231, true)]
        [InlineData((uint)123455, DCSBiosOutputComparison.LessThan, (uint)123456, (uint)1, true)]
        [InlineData((uint)100, DCSBiosOutputComparison.LessThan, (uint)100, (uint)0, false)]

        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)99, (uint)888, false)]
        [InlineData((uint)99, DCSBiosOutputComparison.Equals, (uint)100, (uint)789, false)]
        [InlineData((uint)123455, DCSBiosOutputComparison.Equals, (uint)123456, (uint)12, false)]
        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)100, (uint)99, true)] //<-- != compared to CheckForValueMatch_WithUint 
        [InlineData((uint)100, DCSBiosOutputComparison.Equals, (uint)100, (uint)101, true)] //<-- != compared to CheckForValueMatch_WithUint 

        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)99, (uint)78121, true)]
        [InlineData((uint)99, DCSBiosOutputComparison.NotEquals, (uint)100, (uint)111, true)]
        [InlineData((uint)123455, DCSBiosOutputComparison.NotEquals, (uint)123456, (uint)2, true)]
        [InlineData((uint)100, DCSBiosOutputComparison.NotEquals, (uint)100, (uint)55, false)]
        public void CheckForValueMatchAndChange_WithUint_LastIntValue_Is_Random(uint valueToCompare, DCSBiosOutputComparison comparisonType, uint OriginalValue, uint lastIntValue, bool expectedResult)
        {
            DCSBIOSOutput dcsOutput = new()
            {
                Address = 99,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                Mask = valueToCompare,
                SpecifiedValueUInt = OriginalValue,
                DCSBiosOutputComparison = comparisonType,
                LastUIntValue = lastIntValue, //<-- != compared to CheckForValueMatch_WithUint 
            };

            Assert.Equal(expectedResult, dcsOutput.UIntConditionIsMet(99, valueToCompare));
            Assert.Equal(valueToCompare, dcsOutput.LastUIntValue);
        }

    }
}
