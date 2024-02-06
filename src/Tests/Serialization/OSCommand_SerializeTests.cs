using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals;
using Xunit;

namespace DCSFPTests.Serialization {
    public class OSCommand_SerializeTests {
        [Fact]
        public static void OSCommand_ShouldBeSerializable() {
            OSCommand s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommand d = JsonConvert.DeserializeObject<OSCommand>(serializedObj);

            Assert.Equal(s.Name, d.Name);
            Assert.Equal(s.Command, d.Command);
            Assert.Equal(s.Arguments, d.Arguments);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommand deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommand>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static OSCommand GetObject(int instanceNbr = 1) {
            return new()
            {
                Name = $"hrv kfp {instanceNbr}",
                Command = $"ggt ooi {instanceNbr}",
                Arguments = $"bvr opm {instanceNbr}"
            };
        }
    }
}
