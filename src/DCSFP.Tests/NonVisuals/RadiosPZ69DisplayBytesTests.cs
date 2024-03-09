using System;
using System.Collections.Generic;
using System.Linq;
using NonVisuals.Radios;
using NonVisuals.Radios.Misc;
using Xunit;

namespace DCSFP.Tests.NonVisuals
{
    public class RadiosPZ69DisplayBytesTests
    {
        private const string ZEROES = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
        private const string DEIGHTS = "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8";
        private const string VALUES = "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02";

        private static byte[] StringToBytes(string text)
        {
            byte[] data = text.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
            return data;
        }

        public static IEnumerable<object[]> SetPositionBlankData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", ZEROES, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-00-00-00", ZEROES, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-FF-00-00-00-00-00", ZEROES, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-FF", ZEROES, PZ69LCDPosition.LOWER_STBY_RIGHT };

            yield return new object[] { "00-FF-FF-FF-FF-FF-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02", VALUES, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-FF-FF-FF-FF-FF-02-03-04-05-06-07-08-09-01-02", VALUES, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-01-02-03-04-05-06-07-08-09-01-FF-FF-FF-FF-FF-07-08-09-01-02", VALUES, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-FF-FF-FF-FF-FF", VALUES, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(SetPositionBlankData))]
        public void SetPositionBlank_ShouldSet_5_Blank_Chars_At_Position(string expected, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            PZ69DisplayBytes.SetPositionBlank(ref bytes, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> UnsignedIntegerData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 1, ZEROES, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 10000, ZEROES, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 12345, ZEROES, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 612345, ZEROES, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-00-00-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00", 1, ZEROES, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 10000, ZEROES, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00", 12345, ZEROES, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00", 612345, ZEROES, PZ69LCDPosition.UPPER_STBY_RIGHT };

            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00", 1, ZEROES, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-01-00-00-00-00-00-00-00-00-00", 10000, ZEROES, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-01-02-03-04-05-00-00-00-00-00", 12345, ZEROES, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-06-01-02-03-04-00-00-00-00-00", 612345, ZEROES, PZ69LCDPosition.LOWER_ACTIVE_LEFT };

            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-01", 1, ZEROES, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-00-00-00-00", 10000, ZEROES, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-02-03-04-05", 12345, ZEROES, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-06-01-02-03-04", 612345, ZEROES, PZ69LCDPosition.LOWER_STBY_RIGHT };

            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-FF-01", 1, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-01-02", 12, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-01-02-03", 123, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-01-02-03-04", 1234, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", 12345, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-00-00-00-00", 10000, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-00-00-00-05", 10005, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-00-00-00-05", 100056, DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };

            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-FF-01-D8-D8-D8-D8-D8", 1, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-01-02-D8-D8-D8-D8-D8", 12, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-01-02-03-D8-D8-D8-D8-D8", 123, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-01-02-03-04-D8-D8-D8-D8-D8", 1234, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05-D8-D8-D8-D8-D8", 12345, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-00-00-00-00-D8-D8-D8-D8-D8", 10000, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-00-00-00-05-D8-D8-D8-D8-D8", 10005, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-00-00-00-05-D8-D8-D8-D8-D8", 100056, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-09-09-09-09-09-D8-D8-D8-D8-D8", 99999, DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
        }

        [Theory]
        [MemberData(nameof(UnsignedIntegerData))]
        public void UnsignedInteger_ShouldReturn_ExpectedValue(string expected, uint inputUint, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            PZ69DisplayBytes.UnsignedInteger(ref bytes, inputUint, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> DefaultStringAsItData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-01-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-01-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "123", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "123456", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-01-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-02-FF-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2 3", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-01-FF-02-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 1 2 3", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-02-FF-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2 34", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "     ", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "    1", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-FF-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1   2", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            //IndexOutOfRangeException. Should maybe return D1-FF-FF-FF-FF ?
            //yield return new object[] { "00-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-FF-FF-D1-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1. ", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "    1.", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "00001.", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-D2-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "0002.1", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.2345", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345.", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345.6", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-D0-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "   0.1", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-D2-D3-D4-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.2.3.4.5.", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-D4-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.234.5.", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-D3-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12.3.45", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-FF-01", "1", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-01-02", "12", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-01-02-03", "123", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", "12345", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", "123456", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };

            //Mig 21bis special Tests
            yield return new object[] { "00-FF-01-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 1  1", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-FF-01-FF-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 1  2", DEIGHTS, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-02-FF-FF-01-D8-D8-D8-D8-D8", " 2  1", DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-02-FF-FF-02", " 2  2", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-FF-03-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 3  1", DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-FF-03-FF-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 3  2", DEIGHTS, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-04-FF-FF-01-D8-D8-D8-D8-D8", " 4  1", DEIGHTS, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-04-FF-FF-02", " 4  2", DEIGHTS, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(DefaultStringAsItData))]
        public void DefaultStringAsIt_ShouldReturn_ExpectedValue(string expected, string inputString, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            PZ69DisplayBytes.DefaultStringAsIs(ref bytes, inputString, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        [Theory]
        [InlineData("12a45")]
        [InlineData("a")]
        [InlineData("something")]
        [InlineData(" _ ")]
        [InlineData("/")]
        [InlineData(".....")] //For me this should not throw but perhaps return FF-FF-FF-FF-FF ?
        [InlineData("1..2345")]
        public void DefaultStringAsIt_InvalidChars_OrCombination_ShouldThrow_FormatException(string inputString)
        {
            var bytes = StringToBytes(DEIGHTS);
            Assert.Throws<FormatException>(() => PZ69DisplayBytes.DefaultStringAsIs(ref bytes, inputString, PZ69LCDPosition.UPPER_ACTIVE_LEFT));            
        }

        public static IEnumerable<object[]> DoubleWithSpecifiedDecimalsPlacesData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"     "*/, 1, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"    1"*/, 12, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-FF-FF-FF-01-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   12"*/, 123, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-FF-FF-01-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  123"*/, 1234, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-FF-01-02-03-04-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1234"*/, 12345, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 123456, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 1234567, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??

            yield return new object[] { "00-FF-FF-FF-D1-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.0"*/, 1, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-01-D2-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  12.0"*/, 12, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-01-02-D3-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 123.0"*/, 123, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-D4-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1234.0"*/, 1234, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345."*/, 12345, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 123456, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"      "*/, 1.2, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-D1-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.2"*/, 1.2, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-D1-02-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  1.20"*/, 1.2, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-D1-02-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1.200"*/, 1.2, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2000"*/, 1.2, 4, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2000"*/, 1.2, 5, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"      "*/, 1.23, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-D1-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.2"*/, 1.23, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-D1-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  1.23"*/, 1.23, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-D1-02-03-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1.230"*/, 1.23, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-D1-02-03-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2300"*/, 1.23, 4, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-D1-02-03-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2300"*/, 1.23, 5, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"      "*/, 1.01, 0, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-D1-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.2"*/, 1.23456, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-FF-FF-D1-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  1.23"*/, 1.23456, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-FF-D1-02-03-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1.235"*/, 1.23456, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //????
            yield return new object[] { "00-D1-02-03-04-06-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2346"*/, 1.23456, 4, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //????
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2345"*/, 1.23456, 5, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2345"*/, 1.23456, 6, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            //More or less good frequencies to display with realistic number of digits and decimals
            yield return new object[] { "00-01-02-D3-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.00"*/, 123.00, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-02-D3-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.00"*/, 123, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-D2-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.000"*/, 12, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-D2-00-00-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.005"*/, 12.005, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-00-05-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.050"*/, 12.05, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-05-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.500"*/, 12.5, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.45"*/, 123.45, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.40"*/, 123.4, 2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-D2-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.345"*/, 12.345, 3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
        }

        [Theory]
        [MemberData(nameof(DoubleWithSpecifiedDecimalsPlacesData))]
        public void DoubleWithSpecifiedDecimalsPlaces_ShouldReturn_ExpectedValue(string expected, double digits, int decimals, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            PZ69DisplayBytes.DoubleWithSpecifiedDecimalsPlaces(ref bytes, digits, decimals, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> DoubleJustifyLeftData()
        {
            yield return new object[] { "00-D1-00-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.0000"*/, 1, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.000"*/, 12, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.00"*/, 123, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-D4-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1234.0"*/, 1234, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345."*/, 12345, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D6-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12346."*/, 12346, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-01-02-03-04-06-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12346"*/, 123467, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??

            yield return new object[] { "00-D1-02-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2000"*/, 1.2, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2300"*/, 1.23, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2340"*/, 1.234, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2345"*/, 1.2345, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-06-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2346"*/, 1.23456, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-D1-02-03-04-06-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2346"*/, 1.234567, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??

            yield return new object[] { "00-01-D2-03-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.300"*/, 12.3, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-03-04-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.340"*/, 12.34, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.345"*/, 12.345, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.346"*/, 12.3456, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.346"*/, 12.34567, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-01-02-03-D4-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1234.5"*/, 1234.5, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-D4-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1234.5"*/, 1234.56, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 12345.6789, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.40"*/, 123.40, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.45"*/, 123.45, DEIGHTS, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
        }

        [Theory]
        [MemberData(nameof(DoubleJustifyLeftData))]
        public void Double_ShouldReturn_ExpectedValue(string expected, double digits, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            PZ69DisplayBytes.DoubleJustifyLeft(ref bytes, digits, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }
    }
}
