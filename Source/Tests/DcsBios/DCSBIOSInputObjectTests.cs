using DCS_BIOS;
using DCS_BIOS.Json;
using System;
using Xunit;

namespace Tests.DcsBios
{
    public class DCSBIOSInputObjectTests
    {
        [Theory]
        [InlineData("fixed_step", DCSBIOSInputType.FIXED_STEP)]
        [InlineData("set_state", DCSBIOSInputType.SET_STATE)]
        [InlineData("action", DCSBIOSInputType.ACTION)]
        [InlineData("variable_step", DCSBIOSInputType.VARIABLE_STEP)]
        public void Cosume_ShouldSet_4_Properties(string givenControlInterface, DCSBIOSInputType expectedDCSBIOSInputType)
        {
            string controlId = "ThisIsAControlId";
            DCSBIOSControlInput controlInput = new()
            {
                Description = "Test",
                ControlInterface = givenControlInterface,
                MaxValue = 666,
                Argument = "ThisIsAnArgument"
            };
            DCSBIOSInputObject inputObject = new();
            inputObject.Consume(controlId, controlInput);

            Assert.Equal(controlId, inputObject.ControlId);
            Assert.Equal(controlInput.Description, inputObject.Description);
            Assert.Equal(expectedDCSBIOSInputType, inputObject.Interface);
            Assert.Equal(controlInput.Argument, inputObject.SpecifiedActionArgument);
        }

        [Theory]
        [InlineData("ThisIsAControlId", DCSBIOSFixedStepInput.INC, "ThisIsAControlId INC\n")]
        [InlineData("ThisIsAControlId", DCSBIOSFixedStepInput.DEC, "ThisIsAControlId DEC\n")]
        [InlineData("ThisIsAControlId", null, "ThisIsAControlId INC\n")]
        [InlineData("", DCSBIOSFixedStepInput.DEC, " DEC\n")]
        [InlineData(null, DCSBIOSFixedStepInput.DEC, " DEC\n")]
        [InlineData(null, null, " INC\n")]
        public void GetDCSBIOSCommand_FixedStep_ShouldReturn_ExpectedResult(string controlId, DCSBIOSFixedStepInput? specifiedFixedStepArgument, string expectedResult) {
            DCSBIOSInputObject inputObject = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.FIXED_STEP
            };
            if (specifiedFixedStepArgument != null)
                inputObject.SpecifiedFixedStepArgument = (DCSBIOSFixedStepInput)specifiedFixedStepArgument;

            Assert.Equal(expectedResult, inputObject.GetDCSBIOSCommand());
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
            DCSBIOSInputObject inputObject = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.SET_STATE
            };
            if (specifiedSetStateArgument != null)
                inputObject.SpecifiedSetStateArgument = (uint)specifiedSetStateArgument;

            Assert.Equal(expectedResult, inputObject.GetDCSBIOSCommand());
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
            DCSBIOSInputObject inputObject = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.ACTION,
                SpecifiedActionArgument = specifiedActionArgument
            };

            Assert.Equal(expectedResult, inputObject.GetDCSBIOSCommand());
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
            DCSBIOSInputObject inputObject = new()
            {
                ControlId = controlId,
                Interface = DCSBIOSInputType.VARIABLE_STEP
            };
            if (specifiedVariableStepArgument != null)
                inputObject.SpecifiedVariableStepArgument = (int)specifiedVariableStepArgument;

            Assert.Equal(expectedResult, inputObject.GetDCSBIOSCommand());
        }

        [Fact]
        public void GetDCSBIOSCommand_WithoutAnyPropertiesSet_ReturnsDefaultValueOfFixedStep()
        {
            DCSBIOSInputObject inputObject = new();
            Assert.Equal(" INC\n", inputObject.GetDCSBIOSCommand());
        }

        [Theory]
        [InlineData("", "", DCSBIOSInputType.ACTION)]
        [InlineData("", null, DCSBIOSInputType.ACTION)]
        public void GetDCSBIOSCommand_ShouldThrowException_InThoseCases(string controlId, object argument, DCSBIOSInputType? inputType)
        {
            DCSBIOSInputObject inputObject = new()
            {
                ControlId = controlId,
            };

            if (inputType == DCSBIOSInputType.ACTION) 
            {
                inputObject.SpecifiedActionArgument = (string)argument;
                inputObject.Interface = DCSBIOSInputType.ACTION;
            }
            Assert.Throws<Exception>(() => inputObject.GetDCSBIOSCommand());
        }
    }
}
