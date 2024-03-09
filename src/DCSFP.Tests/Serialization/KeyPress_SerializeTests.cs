using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.KeyEmulation;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public static class KeyPress_SerializeTests {

        [Fact]
        public static void KeyPress_ShouldBeSerializable() {
            KeyPress s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyPress d = JsonConvert.DeserializeObject<KeyPress>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.Information, d.Information);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.Abort, d.Abort);
            DeepAssert.Equal(s.KeyPressSequence, d.KeyPressSequence);

            //not serialized : 
            Assert.Empty(d.NegatorOSKeyPresses);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyPress deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyPress>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.Information, deseralizedObjFromFile.Information);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            Assert.Equal(s.Abort, deseralizedObjFromFile.Abort);
            DeepAssert.Equal(s.KeyPressSequence, deseralizedObjFromFile.KeyPressSequence);
            Assert.Empty(deseralizedObjFromFile.NegatorOSKeyPresses);
        }

        public static KeyPress GetObject(int instanceNbr = 1) {
            return new()
            {
                Information = $"ecw azs {instanceNbr}",
                Description = $"wws zze {instanceNbr}",
                Abort = true,
                KeyPressSequence = new() {
                    { instanceNbr, KeyPressInfo_SerializeTests.GetObject(instanceNbr) },
                    { instanceNbr+1, KeyPressInfo_SerializeTests.GetObject(instanceNbr+1)}
                }
            };
        }
    }
}
