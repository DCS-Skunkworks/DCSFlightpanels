using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.OSCommand;
using Xunit;


namespace DCSFPTests.Serialization {
    public class OSCommandBindingTPM_SerializeTests {
        [Fact]
        public static void OSCommandBindingTPM_ShouldBeSerializable() {
            OSCommandBindingTPM s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommandBindingTPM d = JsonConvert.DeserializeObject<OSCommandBindingTPM>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, d.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommandBindingTPM deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommandBindingTPM>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, deseralizedObjFromFile.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, deseralizedObjFromFile.OSCommandObject);
        }

        private static OSCommandBindingTPM GetObject(int instanceNbr = 1) {
            return new()
            {
                TPMSwitch = KeyBindingTPM_SerializeTests.GetTPMPanelSwitchesFromInstance(instanceNbr + 6),
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr + 2)
            };
        }
    }
}
