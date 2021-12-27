using DCS_BIOS;
using DCS_BIOS.Json;
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

            DCSBIOSInputObject inputObject = new();
            DCSBIOSControlInput controlInput = new()
            {
                Description = "Test",
                ControlInterface = givenControlInterface,
                MaxValue = 666,
                Argument = "ThisIsAnArgument"
            };
            inputObject.Consume(controlId, controlInput);

            Assert.Equal(controlId, inputObject.ControlId);
            Assert.Equal(controlInput.Description, inputObject.Description);
            Assert.Equal(expectedDCSBIOSInputType, inputObject.Interface);
            Assert.Equal(controlInput.Argument, inputObject.SpecifiedActionArgument);
        }

    }
}
