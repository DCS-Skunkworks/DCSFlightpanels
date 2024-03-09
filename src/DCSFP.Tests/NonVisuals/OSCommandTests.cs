using NonVisuals;
using Xunit;

namespace DCSFP.Tests.NonVisuals
{
    public class OSCommandTests
    {
        [Fact]
        public void OSCommand_MustBe_Serializable()
        {
            OSCommand osCommand = new("File AA","Arguments BB","Name CC");

            var clone = osCommand.CloneJson();
            Assert.Equal("File AA", clone.Command);
            Assert.Equal("Arguments BB", clone.Arguments);
            Assert.Equal("Name CC", clone.Name);
            Assert.Equal(osCommand.GetHash(), clone.GetHash());
            Assert.NotEqual(osCommand, clone); //references are different

            Assert.IsType<OSCommand>(clone);
        }

        [Theory]
        [InlineData("A", "B", "C")]
        [InlineData("", "", "")]
        [InlineData("", "B", "C")]
        [InlineData(null, "B", "C")]
        [InlineData(null, " B ", " C ")]
        public void Parametrized_Constructor_MustSet_Properties_Accordingly(string command, string argument, string name)
        {
            OSCommand osCommand = new(command, argument, name);

            Assert.Equal(command, osCommand.Command);
            Assert.Equal(argument, osCommand.Arguments);
            Assert.Equal(string.IsNullOrEmpty(name) ? "OS Command" : name, osCommand.Name);
        }

        [Theory]
        [InlineData("",true)]
        [InlineData(null, true)]
        [InlineData(" a", false)]
        [InlineData(" ", false)]
        [InlineData(" ABC ", false)]
        [InlineData(" ABC 123 ", false)]
        public void IsEmpty_MustReturn_True_If_Command_Is_Empty_Or_Null(string commandValue, bool expectedReturn)
        {
            OSCommand osCommand = new()
            {
                Command = commandValue
            };
            Assert.Equal(expectedReturn, osCommand.IsEmpty);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" a")]
        [InlineData(" ")]
        [InlineData(" ABC ")]
        [InlineData(" ABC 123 ")]
        public void Name_MustReturn_Windows_Command_string_If_Name_Is_Empty_Or_Null(string nameValue)
        {
            OSCommand osCommand = new()
            {
                Name = nameValue
            };
            string expected = nameValue;
            if (string.IsNullOrEmpty(nameValue))
                expected = "Windows Command";
            Assert.Equal(expected, osCommand.Name);
        }

        [Theory]
        [InlineData("A", "B","C", @"OSCommand{A\o/B\o/C}")]
        [InlineData(" A ", " B ", " C ", @"OSCommand{ A \o/ B \o/ C }")]
        [InlineData(" A 12", " -+ B ", "* C * ", @"OSCommand{ A 12\o/ -+ B \o/* C * }")]
        [InlineData("", "", "", null)]
        public void ExportString_MustReturn_ExpectedString(string command, string argument, string name, string expected)
        {
            OSCommand osCommand = new(command, argument, name);
            Assert.Equal(expected, osCommand.ExportString());
        }

        [Theory]
        [InlineData(@"OSCommand{A\o/B\o/C}", "A", "B", "C")]
        [InlineData(@"OSCommand{ A \o/ B \o/ C }", " A ", " B ", " C ")]
        [InlineData(@"OSCommand{ A 12\o/ -+ B \o/* C * }", " A 12", " -+ B ", "* C * ")]
        [InlineData("", "", null, "Windows Command")]
        public void ImportString_MustSet_Properties(string stringToImport, string expectedCommand, string expectedArgument, string expectedName)
        {
            OSCommand osCommand = new();
            osCommand.ImportString(stringToImport);
            Assert.Equal(expectedCommand, osCommand.Command);
            Assert.Equal(expectedArgument, osCommand.Arguments);
            Assert.Equal(expectedName, osCommand.Name);
        }

    }
}
