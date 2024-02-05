using DCS_BIOS;
using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class DCSBIOSInput_SerializeTests {

        [Fact]
        public static void DCSBIOSInput_ShouldBeSerializable() {
            DCSBIOSInput s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSInput d = JsonConvert.DeserializeObject<DCSBIOSInput>(serializedObj);

            Assert.True(s.ControlId == d.ControlId);
            Assert.True(s.Delay == d.Delay);
            DeepAssert.Equal(s.SelectedDCSBIOSInterface, d.SelectedDCSBIOSInterface);
            Assert.True(s.ControlDescription == d.ControlDescription);
            Assert.True(s.ControlType == d.ControlType);
            DeepAssert.Equal(s.DCSBIOSInputInterfaces, d.DCSBIOSInputInterfaces);
            // Assert.False(d.Debug); //deprecated

            RepositorySerialized repo = new();

            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSInput deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSInput>(repo.GetSerializedObjectString(d.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        private static DCSBIOSInput GetObject(int instanceNbr = 1) {
            return new()
            {
                ControlId = $"CtrlId 741{instanceNbr}",
                Delay = 1,
                SelectedDCSBIOSInterface = DCSBIOSInputInterface_SerializeTests.GetObject(1),
                //Debug = true,  //deprecated
                ControlDescription = $"ikf xsd {instanceNbr}",
                ControlType = "xmd rty",
                DCSBIOSInputInterfaces = new() {
                    DCSBIOSInputInterface_SerializeTests.GetObject(1),
                    DCSBIOSInputInterface_SerializeTests.GetObject(2)
                },
            };
        }
    }
}
