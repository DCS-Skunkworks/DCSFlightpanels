using NonVisuals.Radios;
using NonVisuals.Radios.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.NonVisuals
{
    public class RadiosPZ69DisplayBytesTests
    {
        PZ69DisplayBytes _dp = new PZ69DisplayBytes();
        private const string _zeroes = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
        private const string _eights = "00-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08";
        private const string _blank  = "00-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF";
        private const string _values = "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02";

        private byte[] StringToBytes(string text)
        {
            byte[] data = text.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
            return data;
        }

        public static IEnumerable<object[]> SetPositionBlankData()
        {
            yield return new object[] { PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { PZ69LCDPosition.UPPER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { PZ69LCDPosition.LOWER_ACTIVE_LEFT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-FF-00-00-00-00-00" };
            yield return new object[] { PZ69LCDPosition.LOWER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-FF" };

            yield return new object[] { PZ69LCDPosition.UPPER_ACTIVE_LEFT, _values, "00-FF-FF-FF-FF-FF-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02" };
            yield return new object[] { PZ69LCDPosition.UPPER_STBY_RIGHT, _values, "00-01-02-03-04-05-FF-FF-FF-FF-FF-02-03-04-05-06-07-08-09-01-02" };
            yield return new object[] { PZ69LCDPosition.LOWER_ACTIVE_LEFT, _values, "00-01-02-03-04-05-06-07-08-09-01-FF-FF-FF-FF-FF-07-08-09-01-02" };
            yield return new object[] { PZ69LCDPosition.LOWER_STBY_RIGHT, _values, "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-FF-FF-FF-FF-FF" };
        }

        [Theory]
        [MemberData(nameof(SetPositionBlankData))]
        public void SetPositionBlank_ShouldSet_5_Blank_Chars_At_Position(PZ69LCDPosition lcdPosition, string inputBytes, string expected)
        {
            var bytes = StringToBytes(inputBytes);
            _dp.SetPositionBlank(ref bytes, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> UnsignedIntegerData()
        {
            yield return new object[] { 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 10000, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 12345, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 612345, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };

            yield return new object[] { 1, PZ69LCDPosition.UPPER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 10000, PZ69LCDPosition.UPPER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 12345, PZ69LCDPosition.UPPER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 612345, PZ69LCDPosition.UPPER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00" };

            yield return new object[] { 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00" };
            yield return new object[] { 10000, PZ69LCDPosition.LOWER_ACTIVE_LEFT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-01-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 12345, PZ69LCDPosition.LOWER_ACTIVE_LEFT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-01-02-03-04-05-00-00-00-00-00" };
            yield return new object[] { 612345, PZ69LCDPosition.LOWER_ACTIVE_LEFT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-06-01-02-03-04-00-00-00-00-00" };

            yield return new object[] { 1, PZ69LCDPosition.LOWER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-01" };
            yield return new object[] { 10000, PZ69LCDPosition.LOWER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-00-00-00-00" };
            yield return new object[] { 12345, PZ69LCDPosition.LOWER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-02-03-04-05" };
            yield return new object[] { 612345, PZ69LCDPosition.LOWER_STBY_RIGHT, _zeroes, "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-06-01-02-03-04" };
        }

        [Theory]
        [MemberData(nameof(UnsignedIntegerData))]
        public void UnsignedInteger_ShouldReturn_ExpectedValue(uint input, PZ69LCDPosition lcdPosition, string inputBytes, string expected)
        {
            var bytes = StringToBytes(inputBytes);
            _dp.UnsignedInteger(ref bytes, input, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> IntegerData()
        {
            yield return new object[] { 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { -1, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 10000, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 12345, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { -12345, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
            yield return new object[] { 612345, PZ69LCDPosition.UPPER_ACTIVE_LEFT, _zeroes, "00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00" };
        }

        [Theory]
        [MemberData(nameof(IntegerData))]
        public void Integer_ShouldReturn_ExpectedValue(int input, PZ69LCDPosition lcdPosition, string inputBytes, string expected)
        {
            var bytes = StringToBytes(inputBytes);
            _dp.Integer(ref bytes, input, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

    }
}
