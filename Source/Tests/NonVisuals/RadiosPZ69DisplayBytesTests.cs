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
        PZ69DisplayBytes _dp = new();
        private const string _zeroes = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
        private const string _eights = "00-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08-08";
        private const string _deights = "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8";
        private const string _blank = "00-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF";
        private const string _values = "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02";

        private byte[] StringToBytes(string text)
        {
            byte[] data = text.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
            return data;
        }

        public static IEnumerable<object[]> SetPositionBlankData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-00-00-00", _zeroes, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-FF-00-00-00-00-00", _zeroes, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-FF", _zeroes, PZ69LCDPosition.LOWER_STBY_RIGHT };

            yield return new object[] { "00-FF-FF-FF-FF-FF-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02", _values, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-FF-FF-FF-FF-FF-02-03-04-05-06-07-08-09-01-02", _values, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-01-02-03-04-05-06-07-08-09-01-FF-FF-FF-FF-FF-07-08-09-01-02", _values, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-FF-FF-FF-FF-FF", _values, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(SetPositionBlankData))]
        public void SetPositionBlank_ShouldSet_5_Blank_Chars_At_Position(string expected, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.SetPositionBlank(ref bytes, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> UnsignedIntegerData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 1, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 10000, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 12345, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 612345, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-00-00-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00", 1, _zeroes, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 10000, _zeroes, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00", 12345, _zeroes, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00", 612345, _zeroes, PZ69LCDPosition.UPPER_STBY_RIGHT };

            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00", 1, _zeroes, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-01-00-00-00-00-00-00-00-00-00", 10000, _zeroes, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-01-02-03-04-05-00-00-00-00-00", 12345, _zeroes, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-06-01-02-03-04-00-00-00-00-00", 612345, _zeroes, PZ69LCDPosition.LOWER_ACTIVE_LEFT };

            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF-01", 1, _zeroes, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-00-00-00-00", 10000, _zeroes, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-02-03-04-05", 12345, _zeroes, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-06-01-02-03-04", 612345, _zeroes, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(UnsignedIntegerData))]
        public void UnsignedInteger_ShouldReturn_ExpectedValue(string expected, uint inputUint, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.UnsignedInteger(ref bytes, inputUint, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> IntegerData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 1, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT};
            yield return new object[] { "00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 10000, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 12345, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            //no sign managed?
            //yield return new object[] { "00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", -1, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT  };
            
            //no sign managed ?
            //yield return new object[] { "00-06-01-02-03-04-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", -12345, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            //Overflow of 1 char to the right (looks like a bug)
            yield return new object[] { "00-01-02-03-04-05-06-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 123456, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            //Overflow of 1 char to the right (not 2) (looks like a bug)
            yield return new object[] { "00-01-02-03-04-05-06-00-00-00-00-00-00-00-00-00-00-00-00-00-00", 1234567, _zeroes, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            //IndexOutOfRangeException. Overflow of 1 char to the right (looks like a bug)
            //yield return new object[] { "00-01-02-03-04-05-06-00-00-00-00-00-00-00-00-00-01-02-03-04-05", 123456, _zeroes, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        /// <summary>
        /// Todo: Replace Integer by UnsignedInteger since they seems to react the same way. Is there a use for sign ?
        /// </summary>
        [Theory]
        [MemberData(nameof(IntegerData))]
        public void Integer_ShouldReturn_ExpectedValue(string expected, int inputInt, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.Integer(ref bytes, inputInt, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> CustomData()
        {
            yield return new object[] { "00-D1-D2-D3-D4-D5-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02", "D1-D2-D3-D4-D5", _values, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-D2-D3-D4-05-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02", "D1-D2-D3-D4", _values, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-D2-D3-D4-D5-06-07-08-09-01-02-03-04-05-06-07-08-09-01-02", "D1-D2-D3-D4-D5-D6", _values, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D1-D2-D3-D4-D5-02-03-04-05-06-07-08-09-01-02", "D1-D2-D3-D4-D5", _values, PZ69LCDPosition.UPPER_STBY_RIGHT };
            yield return new object[] { "00-01-02-03-04-05-06-07-08-09-01-D1-D2-D3-D4-D5-07-08-09-01-02", "D1-D2-D3-D4-D5", _values, PZ69LCDPosition.LOWER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-06-07-08-09-01-02-03-04-05-06-D1-D2-D3-D4-D5", "D1-D2-D3-D4-D5", _values, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(CustomData))]
        public void Custom5Bytes_ShouldInject_TheGiven5BytesMax_AtPosition(string expected, string inputBytes, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            var bytesToInject = StringToBytes(inputBytes);
            _dp.Custom5Bytes(ref bytes, bytesToInject, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> DefaultStringAsItData()
        {
            yield return new object[] { "00-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "123", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "123456", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-02-FF-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2 3", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-01-FF-02-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 1 2 3", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-02-FF-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2 34", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "     ", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "    1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-FF-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1   2", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            //IndexOutOfRangeException. Should maybe return D1-FF-FF-FF-FF ?
            //yield return new object[] { "00-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            yield return new object[] { "00-D1-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1. ", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "    1.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "00001.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-D2-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "0002.1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.2345", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345.6", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-D0-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "   0.1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-D2-D3-D4-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.2.3.4.5.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-D4-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.234.5.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-D3-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12.3.45", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-D8-D8-D8-D8", "1", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-D8-D8-D8", "12", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-D8-D8", "123", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", "12345", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", "123456", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(DefaultStringAsItData))]
        public void DefaultStringAsIt_ShouldReturn_ExpectedValue(string expected, string inputString, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.DefaultStringAsIt(ref bytes, inputString, lcdPosition);
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
            var bytes = StringToBytes(_deights);
            Assert.Throws<FormatException>(() => _dp.DefaultStringAsIt(ref bytes, inputString, PZ69LCDPosition.UPPER_ACTIVE_LEFT));            
        }

        public static IEnumerable<object[]> BytesStringData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //diff. than DefaultStringAsIt
            yield return new object[] { "00-FF-FF-FF-01-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //diff. than DefaultStringAsIt
            yield return new object[] { "00-FF-FF-01-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "123", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //diff. than DefaultStringAsIt
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "123456", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-01-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //diff. than DefaultStringAsIt
            yield return new object[] { "00-01-FF-02-FF-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2 3", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-01-FF-02-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", " 1 2 3", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-02-FF-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1 2 34", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "     ", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "    1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-FF-FF-FF-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1   2", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
           
            yield return new object[] { "00-FF-FF-FF-FF-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-D1-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1. ", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //diff. than DefaultStringAsIt
            yield return new object[] { "00-FF-FF-FF-FF-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "    1.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-00-D1-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "00001.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-00-00-00-D2-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "0002.1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.2345", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12345.6", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-FF-D0-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "   0.1", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            //IndexOutOfRangeException
            //yield return new object[] { "00-D1-D2-D3-D4-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.2.3.4.5.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            //IndexOutOfRangeException
            //yield return new object[] { "00-D1-02-03-D4-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "1.234.5.", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            
            //Not what I expect, should return 01-D2-D3-04-05
            yield return new object[] { "00-01-D2-D3-04-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8", "12.3.45", _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-FF-01", "1", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-FF-01-02", "12", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-FF-FF-01-02-03", "123", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", "12345", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
            yield return new object[] { "00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-01-02-03-04-05", "123456", _deights, PZ69LCDPosition.LOWER_STBY_RIGHT };
        }

        [Theory]
        [MemberData(nameof(BytesStringData))]
        public void BytesStringAsItOrPadLeftBlanks_ShouldReturn_ExpectedValue(string expected, string inputString, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.BytesStringAsItOrPadLeftBlanks(ref bytes, inputString, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }


        public static IEnumerable<object[]> DoubleWithSpecifiedDecimalsPlacesData()
        {
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"     "*/, 1, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-FF-01-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"    1"*/, 12, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-FF-FF-FF-01-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   12"*/, 123, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-FF-FF-01-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  123"*/, 1234, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-FF-01-02-03-04-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1234"*/, 12345, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 123456, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 1234567, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };//??

            yield return new object[] { "00-FF-FF-FF-D1-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.0"*/, 1, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-01-D2-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  12.0"*/, 12, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-01-02-D3-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 123.0"*/, 123, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-D4-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1234.0"*/, 1234, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-D5-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345."*/, 12345, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12345"*/, 123456, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"      "*/, 1.2, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-D1-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.2"*/, 1.2, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-D1-02-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  1.20"*/, 1.2, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-D1-02-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1.200"*/, 1.2, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2000"*/, 1.2, 4, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2000"*/, 1.2, 5, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"      "*/, 1.23, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-D1-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.2"*/, 1.23, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-FF-D1-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  1.23"*/, 1.23, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-FF-D1-02-03-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1.230"*/, 1.23, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-D1-02-03-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2300"*/, 1.23, 4, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-D1-02-03-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2300"*/, 1.23, 5, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            
            yield return new object[] { "00-FF-FF-FF-FF-FF-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"      "*/, 1.01, 0, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //??
            yield return new object[] { "00-FF-FF-FF-D1-02-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"   1.2"*/, 1.23456, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-FF-FF-D1-02-03-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"  1.23"*/, 1.23456, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-FF-D1-02-03-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*" 1.235"*/, 1.23456, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //????
            yield return new object[] { "00-D1-02-03-04-06-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2346"*/, 1.23456, 4, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; //????
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2345"*/, 1.23456, 5, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-D1-02-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.2345"*/, 1.23456, 6, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };

            //More or less good frequencies to display with realistic number of digits and decimals
            yield return new object[] { "00-01-02-D3-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.00"*/, 123.00, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-02-D3-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.00"*/, 123, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-D2-00-05-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.050"*/, 12.050, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.000"*/, 12, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-D2-00-00-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.005"*/, 12.005, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-00-05-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.050"*/, 12.05, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-D2-05-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.500"*/, 12.5, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.45"*/, 123.45, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.40"*/, 123.40, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
            yield return new object[] { "00-01-02-D3-04-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"123.40"*/, 123.4, 2, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
            yield return new object[] { "00-01-D2-03-04-05-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"12.345"*/, 12.345, 3, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT };
        }

        [Theory]
        [MemberData(nameof(DoubleWithSpecifiedDecimalsPlacesData))]
        public void DoubleWithSpecifiedDecimalsPlaces_ShouldReturn_ExpectedValue(string expected, double digits, int decimals, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.DoubleWithSpecifiedDecimalsPlaces(ref bytes, digits, decimals, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }

        public static IEnumerable<object[]> DoubleData()
        {
            yield return new object[] { "00-D1-00-00-00-00-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8-D8"/*"1.0000"*/, 1, _deights, PZ69LCDPosition.UPPER_ACTIVE_LEFT }; 
        }

        [Theory]
        [MemberData(nameof(DoubleData))]
        public void Double_ShouldReturn_ExpectedValue(string expected, double digits, string inputArray, PZ69LCDPosition lcdPosition)
        {
            var bytes = StringToBytes(inputArray);
            _dp.Double(ref bytes, digits, lcdPosition);
            Assert.Equal(expected, BitConverter.ToString(bytes));
        }
    }
}
