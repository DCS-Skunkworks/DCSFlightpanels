using DCS_BIOS.Serialized;
using Newtonsoft.Json;
using DCS_BIOS.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace DCS_BIOS.Tests.Serialization {

    /// <summary>
    /// Note: SelectedArgumentValue is private set but should reflect the last value passed to one of the 'Specifiedfields*'
    /// </summary>
    public static class DCSBIOSInputInterface_SerializeTests {

        [Fact]
        public static void DCSBIOSInputInterface_ShouldBeSerializable() {
            DCSBIOSInputInterface s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSInputInterface d = JsonConvert.DeserializeObject<DCSBIOSInputInterface>(serializedObj);

            Assert.Equal(s.ControlId, d.ControlId);
            Assert.Equal(s.Delay, d.Delay);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.MaxValue, d.MaxValue);
            Assert.Equal(s.Interface, d.Interface);
            Assert.Equal(s.SuggestedStep, d.SuggestedStep);

            Assert.Equal(s.SpecifiedActionArgument, d.SpecifiedActionArgument);
            Assert.Equal(s.SpecifiedSetStateArgument, d.SpecifiedSetStateArgument);
            Assert.Equal(s.SpecifiedVariableStepArgument, d.SpecifiedVariableStepArgument);
            Assert.Equal(s.SpecifiedFixedStepArgument, d.SpecifiedFixedStepArgument);
            Assert.Equal(s.SpecifiedSetStringArgument, d.SpecifiedSetStringArgument);

            Assert.Equal(s.SelectedArgumentValue, d.SpecifiedSetStringArgument);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSInputInterface deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSInputInterface>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static DCSBIOSInputInterface GetObject(int instanceNbr = 1) {
            return new()
            {
                ControlId = $"ijw evv {instanceNbr}",
                Delay = 1,
                Description = $"urt vdf {instanceNbr}",
                MaxValue = 2,
                Interface = DCSBIOSInputType.SET_STRING,
                SuggestedStep = 3,
                SpecifiedActionArgument = $"dov csd {instanceNbr}",
                SpecifiedSetStateArgument = 4,
                SpecifiedVariableStepArgument = 4 + instanceNbr,
                SpecifiedFixedStepArgument = DCSBIOSFixedStepInput.INC,
                SpecifiedSetStringArgument = "sdd vbn",
            };
        }
    }
}
