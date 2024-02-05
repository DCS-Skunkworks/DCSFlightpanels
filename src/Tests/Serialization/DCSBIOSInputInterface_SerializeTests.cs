using DCS_BIOS;
using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using Xunit;

namespace DCSFPTests.Serialization {

    /// <summary>
    /// Note: SelectedArgumentValue is private set but should reflect the last value passed to one of the 'Specifiedfields*'
    /// </summary>
    public static class DCSBIOSInputInterface_SerializeTests {

        [Fact]
        public static void DCSBIOSInputInterface_ShouldBeSerializable() {
            DCSBIOSInputInterface s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSInputInterface d = JsonConvert.DeserializeObject<DCSBIOSInputInterface>(serializedObj);

            Assert.True(s.ControlId == d.ControlId);
            Assert.True(s.Delay == d.Delay);
            Assert.True(s.Description == d.Description);
            Assert.True(s.MaxValue == d.MaxValue);
            Assert.True(s.Interface == d.Interface);
            Assert.True(s.SuggestedStep == d.SuggestedStep);

            Assert.True(s.SpecifiedActionArgument == d.SpecifiedActionArgument);
            Assert.True(s.SpecifiedSetStateArgument == d.SpecifiedSetStateArgument);
            Assert.True(s.SpecifiedVariableStepArgument == d.SpecifiedVariableStepArgument);
            Assert.True(s.SpecifiedFixedStepArgument == d.SpecifiedFixedStepArgument);
            Assert.True(s.SpecifiedSetStringArgument == d.SpecifiedSetStringArgument);

            Assert.True(s.SelectedArgumentValue == d.SpecifiedSetStringArgument);

            RepositorySerialized repo = new();

            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSInputInterface deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSInputInterface>(repo.GetSerializedObjectString(d.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static DCSBIOSInputInterface GetObject(int instanceNbr = 1) {
            return new()
            {
                ControlId = $"CtrlId 952{instanceNbr}",
                Delay = 1,
                Description = $"urt vdf {instanceNbr}",
                MaxValue = 2,
                Interface = DCSBIOSInputType.SET_STRING,
                SuggestedStep = 3,
                SpecifiedActionArgument = "dov csd",
                SpecifiedSetStateArgument = 4,
                SpecifiedVariableStepArgument = 4 + instanceNbr,
                SpecifiedFixedStepArgument = DCSBIOSFixedStepInput.INC,
                SpecifiedSetStringArgument = "sdd vbn",
            };
        }
    }
}
