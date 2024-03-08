using DCS_BIOS.Serialized;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using Xunit;

namespace DCSFP.Tests.Serialization
{

    public static class DCSBIOSInput_SerializeTests {

        [Fact]
        public static void DCSBIOSInput_ShouldBeSerializable() {
            DCSBIOSInput s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSInput d = JsonConvert.DeserializeObject<DCSBIOSInput>(serializedObj);

            Assert.Equal(s.ControlId, d.ControlId);
            Assert.Equal(s.Delay, d.Delay);
            DeepAssert.Equal(s.SelectedDCSBIOSInterface, d.SelectedDCSBIOSInterface);
            Assert.Equal(s.ControlDescription, d.ControlDescription);
            DeepAssert.Equal(s.DCSBIOSInputInterfaces, d.DCSBIOSInputInterfaces);
            // Assert.False(d.Debug); //deprecated

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSInput deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSInput>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static DCSBIOSInput GetObject(int instanceNbr = 1) {
            return new()
            {
                ControlId = $"uyt ffs {instanceNbr}",
                Delay = 1,
                SelectedDCSBIOSInterface = DCSBIOSInputInterface_SerializeTests.GetObject(1),
                //Debug = true,  //deprecated
                ControlDescription = $"ikf xsd {instanceNbr}",
                DCSBIOSInputInterfaces = new() {
                    DCSBIOSInputInterface_SerializeTests.GetObject(1),
                    DCSBIOSInputInterface_SerializeTests.GetObject(2)
                },
            };
        }
    }
}
