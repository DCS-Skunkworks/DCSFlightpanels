using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.OSCommand;
using Xunit;

namespace DCSFPTests.Serialization {
    public class OSCommandBindingPZ69FullEmulator_SerializeTests {
        [Fact]
        public static void OSCommandBindingPZ69FullEmulator_ShouldBeSerializable() {
            OSCommandBindingPZ69FullEmulator s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommandBindingPZ69FullEmulator d = JsonConvert.DeserializeObject<OSCommandBindingPZ69FullEmulator>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.RadioPanelPZ69Key, d.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommandBindingPZ69FullEmulator deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommandBindingPZ69FullEmulator>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.RadioPanelPZ69Key, deseralizedObjFromFile.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);
        }

        private static OSCommandBindingPZ69FullEmulator GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ69DialPosition_SerializeTests.GetPZ69DialPositionFromInstance(instanceNbr+5),
                RadioPanelPZ69Key = KeyBindingPZ69_SerializeTests.GetRadioPanelPZ69KnobsEmulatorFromInstance(instanceNbr+6),
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr+7)
            };
        }
    }
}
