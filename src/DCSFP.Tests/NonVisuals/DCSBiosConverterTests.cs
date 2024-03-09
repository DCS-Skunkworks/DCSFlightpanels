using System;
using NonVisuals.Panels.StreamDeck;
using Xunit;

namespace DCSFP.Tests.NonVisuals
{
    public class DCSBiosConverterTests
    {
        [Theory]
        [InlineData(EnumComparator.NotSet, "NotSet")]
        [InlineData(EnumComparator.Equals, "==")]
        [InlineData(EnumComparator.NotEquals, "!=")]
        [InlineData(EnumComparator.LessThan, "<")]
        [InlineData(EnumComparator.LessThanEqual, "<=")]
        [InlineData(EnumComparator.GreaterThan, ">")]
        [InlineData(EnumComparator.GreaterThanEqual, ">=")]
        [InlineData(EnumComparator.Always, "Always")]
        public void GetComparatorAsString_MustReturnExpectedValue(EnumComparator comparator, string expectedValue)
        {
            Assert.Equal(expectedValue, DCSBIOSConverter.GetComparatorAsString(comparator));
        }

        [Fact]
        public void GetComparatorAsString_ThrowExeptionForUnexpectedValue()
        {
            Assert.Throws<ArgumentException>(() => DCSBIOSConverter.GetComparatorAsString((EnumComparator)int.MaxValue));
        }

        [Theory]
        [InlineData(EnumConverterOutputType.NotSet, "not set")]
        [InlineData(EnumConverterOutputType.Raw, "raw")]
        [InlineData(EnumConverterOutputType.Image, "image")]
        [InlineData(EnumConverterOutputType.ImageOverlay, "image overlay")]
        [InlineData((EnumConverterOutputType)int.MaxValue, "unknown?")]
        public void GetOutputAsString_MustReturnExpectedValue(EnumConverterOutputType converterOutputType, string expectedValue)
        {
            Assert.Equal(expectedValue, DCSBIOSConverter.GetOutputAsString(converterOutputType));
        }
    }
}
