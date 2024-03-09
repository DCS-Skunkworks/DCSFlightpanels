using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.OSCommand;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class OSCommandBindingPZ69Emulator_SerializeTests {
        [Fact]
        public static void OSCommandBindingPZ69Emulator_ShouldBeSerializable() {
            OSCommandBindingPZ69Emulator s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommandBindingPZ69Emulator d = JsonConvert.DeserializeObject<OSCommandBindingPZ69Emulator>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.RadioPanelPZ69Key, d.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommandBindingPZ69Emulator deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommandBindingPZ69Emulator>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.RadioPanelPZ69Key, deseralizedObjFromFile.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, deseralizedObjFromFile.OSCommandObject);
        }

        private static OSCommandBindingPZ69Emulator GetObject(int instanceNbr = 1) {
            return new()
            {
                RadioPanelPZ69Key = KeyBindingPZ69_SerializeTests.GetRadioPanelPZ69KnobsEmulatorFromInstance(instanceNbr+2),
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr+1)
            };
        }
    }
}
