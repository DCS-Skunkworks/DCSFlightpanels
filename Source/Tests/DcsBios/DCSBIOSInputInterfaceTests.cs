using DCS_BIOS;
using DCS_BIOS.Json;
using System;
using Xunit;

namespace Tests.DcsBios
{
    public class DCSBIOSInputInterfaceTests
    {
        [Theory]
        [InlineData("fixed_step", DCSBIOSInputType.FIXED_STEP)]
        [InlineData("set_state", DCSBIOSInputType.SET_STATE)]
        [InlineData("action", DCSBIOSInputType.ACTION)]
        [InlineData("variable_step", DCSBIOSInputType.VARIABLE_STEP)]
        [InlineData("set_string", DCSBIOSInputType.SET_STRING)]
        public void Consume_ShouldSet_4_Properties(string givenControlInterface, DCSBIOSInputType expectedDCSBIOSInputType)
        {
            string controlId = "ThisIsAControlId";
            DCSBIOSControlInput controlInput = new()
            {
                Description = "Test",
                ControlInterface = givenControlInterface,
                MaxValue = 666,
                Argument = "ThisIsAnArgument"
            };
            DCSBIOSInputInterface inputInterface = new();
            inputInterface.Consume(controlId, controlInput);

            Assert.Equal(controlId, inputInterface.ControlId);
            Assert.Equal(controlInput.Description, inputInterface.Description);
            Assert.Equal(expectedDCSBIOSInputType, inputInterface.Interface);
            Assert.Equal(controlInput.Argument, inputInterface.SpecifiedActionArgument);
        }

        [Theory]
        [InlineData("ThisIsAControlId", DCSBIOSFixedStepInput.INC, "ThisIsAControlId INC\n")]
        [InlineData("ThisIsAControlId", DCSBIOSFixedStepInput.DEC, "ThisIsAControlId DEC\n")]
        [InlineData("ThisIsAControlId", null, "ThisIsAControlId INC\n")]
        [InlineData("", DCSBIOSFixedStepInput.DEC, " DEC\n")]
        [InlineData(null, DCSBIOSFixedStepInput.DEC, " DEC\n")]
        [InlineData(null, null, " INC\n")]
        public void GetDCSBIOSCommand_FixedStep_ShouldReturn_ExpectedResult(string controlId, DCSBIOSFixedStepInput? specifiedFixedStepArgument, string expectedResult) {
            DCSBIOSInputInterface inputInterface = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.FIXED_STEP
            };
            if (specifiedFixedStepArgument != null)
                inputInterface.SpecifiedFixedStepArgument = (DCSBIOSFixedStepInput)specifiedFixedStepArgument;

            Assert.Equal(expectedResult, inputInterface.GetDCSBIOSCommand());
        }

        [Theory]
        [InlineData("ThisIsAControlId", (uint)123, "ThisIsAControlId 123\n")]
        [InlineData("ThisIsAControlId", (uint)456, "ThisIsAControlId 456\n")]
        [InlineData("ThisIsAControlId", null, "ThisIsAControlId 0\n")]
        [InlineData("", (uint)789, " 789\n")]
        [InlineData("", null, " 0\n")]
        [InlineData(null, null, " 0\n")]
        public void GetDCSBIOSCommand_SetState_ShouldReturn_ExpectedResult(string controlId, uint? specifiedSetStateArgument, string expectedResult)
        {
            DCSBIOSInputInterface inputInterface = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.SET_STATE
            };
            if (specifiedSetStateArgument != null)
                inputInterface.SpecifiedSetStateArgument = (uint)specifiedSetStateArgument;

            Assert.Equal(expectedResult, inputInterface.GetDCSBIOSCommand());
        }

        [Theory]
        [InlineData("ThisIsAControlId", "AbC", "ThisIsAControlId AbC\n")]
        [InlineData("ThisIsAControlId", "", "ThisIsAControlId \n")]
        [InlineData("ThisIsAControlId", null, "ThisIsAControlId \n")]
        [InlineData("", "AbC", " AbC\n")]
        [InlineData(null, "AbC", " AbC\n")]
        //[InlineData("", "", "  \n")]
        //[InlineData("", null, " 0\n")]
        public void GetDCSBIOSCommand_Action_ShouldReturn_ExpectedResult(string controlId, string specifiedActionArgument, string expectedResult)
        {
            DCSBIOSInputInterface inputInterface = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.ACTION,
                SpecifiedActionArgument = specifiedActionArgument
            };

            Assert.Equal(expectedResult, inputInterface.GetDCSBIOSCommand());
        }

        [Theory]
        [InlineData("ThisIsAControlId", 1, "ThisIsAControlId +1\n")]
        [InlineData("ThisIsAControlId", 0, "ThisIsAControlId 0\n")]
        [InlineData("ThisIsAControlId", -1, "ThisIsAControlId -1\n")]
        [InlineData("ThisIsAControlId", 123, "ThisIsAControlId +123\n")]
        [InlineData("ThisIsAControlId", 456, "ThisIsAControlId +456\n")]
        [InlineData("ThisIsAControlId", -123, "ThisIsAControlId -123\n")]
        [InlineData("ThisIsAControlId", null, "ThisIsAControlId 0\n")]
        [InlineData("", 789, " +789\n")]
        [InlineData("", null, " 0\n")]
        [InlineData(null, null, " 0\n")]
        public void GetDCSBIOSCommand_VariableStep_ShouldReturn_ExpectedResult(string controlId, int? specifiedVariableStepArgument, string expectedResult)
        {
            DCSBIOSInputInterface inputInterface = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.VARIABLE_STEP
            };
            if (specifiedVariableStepArgument != null)
                inputInterface.SpecifiedVariableStepArgument = (int)specifiedVariableStepArgument;

            Assert.Equal(expectedResult, inputInterface.GetDCSBIOSCommand());
        }

        [Fact]
        public void GetDCSBIOSCommand_WithoutAnyPropertiesSet_ReturnsDefaultValueOfFixedStep()
        {
            DCSBIOSInputInterface inputInterface = new();
            Assert.Equal(" INC\n", inputInterface.GetDCSBIOSCommand());
        }

        [Theory]
        [InlineData("", "", DCSBIOSInputType.ACTION)]
        [InlineData("", null, DCSBIOSInputType.ACTION)]
        public void GetDCSBIOSCommand_ShouldThrowException_InThoseCases(string controlId, object argument, DCSBIOSInputType? inputType)
        {
            DCSBIOSInputInterface inputInterface = new()
            {
                ControlId = controlId,
            };

            if (inputType == DCSBIOSInputType.ACTION) 
            {
                inputInterface.SpecifiedActionArgument = (string)argument;
                inputInterface.Interface = DCSBIOSInputType.ACTION;
            }
            Assert.Throws<Exception>(() => inputInterface.GetDCSBIOSCommand());
        }

        [Theory]
        [InlineData("ThisIsAControlId", "1", "ThisIsAControlId 1\n")]
        [InlineData("ThisIsAControlId", "0", "ThisIsAControlId 0\n")]
        [InlineData("ThisIsAControlId", "-1", "ThisIsAControlId -1\n")]
        [InlineData("ThisIsAControlId", "123", "ThisIsAControlId 123\n")]
        [InlineData("ThisIsAControlId", "456", "ThisIsAControlId 456\n")]
        [InlineData("ThisIsAControlId", "-123", "ThisIsAControlId -123\n")]
        [InlineData("ThisIsAControlId", "null", "ThisIsAControlId null\n")]
        public void GetDCSBIOSCommand_Set_String_ShouldReturn_ExpectedResult(string controlId, string? specifiedStringArgument, string expectedResult)
        {
            DCSBIOSInputInterface inputInterface = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.SET_STRING,
                SpecifiedSetStringArgument = specifiedStringArgument
            };

            Assert.Equal(expectedResult, inputInterface.GetDCSBIOSCommand());
        }
    }
}
