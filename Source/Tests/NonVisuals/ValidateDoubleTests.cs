using NonVisuals;
using System.Windows.Controls;
using Xunit;

namespace Tests.NonVisuals
{
    public class ValidateDoubleTests
    {
        [StaFact]
        public void ValidateDouble_MustReturn_True_IfTextBoxIsEmptyOrNull()
        {
            TextBox textBox = new ();
            Assert.True(textBox.ValidateDouble());
        }

        [StaFact]
        public void ValidateDouble_MustReturn_False_If_Blanks()
        {
            TextBox textBox = new() { Text = "    " };
            Assert.False(textBox.ValidateDouble());
        }

        [StaTheory]
        [InlineData("1")]
        [InlineData("12")]
        [InlineData("123")]
        [InlineData("1.2")]
        [InlineData("1.23")]
        [InlineData("1234.56789")]
        [InlineData("00000.56789")]
        //Comma is the thousands separator, .NET is not picky about where the user puts it but it must be before decimal separator
        [InlineData("1,2.78")]
        [InlineData("1,2,3,4,5,6,.78")]
        [InlineData("1,23")]
        [InlineData("1234,56,789")]
        public void ValidateDouble_MustReturn_True_If_StringIsValidDouble(string inputString)
        {
            TextBox textBox = new() { Text = inputString };
            Assert.True(textBox.ValidateDouble());
        }

        [StaTheory]
        [InlineData("1.1A")]
        [InlineData("A1.1")]
        [InlineData("102.0,0")]
        [InlineData("1.0.0")]
        [InlineData("1,0..0")]
        [InlineData("1,0,.,0.9")]
        [InlineData("1,2,3.4,5,6,7,8")]
        [InlineData("1,2.78,910")]
        public void ValidateDouble_MustReturn_False_If_StringIsInvalidDouble(string inputString)
        {
            TextBox textBox = new() { Text = inputString };
            Assert.False(textBox.ValidateDouble());
        }

    }
}
