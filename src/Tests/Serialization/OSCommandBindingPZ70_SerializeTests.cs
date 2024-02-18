using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.OSCommand;
using Xunit;


namespace DCSFPTests.Serialization {
    public class OSCommandBindingPZ70_SerializeTests {
        [Fact]
        public static void OSCommandBindingPZ70_ShouldBeSerializable() {
            OSCommandBindingPZ70 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommandBindingPZ70 d = JsonConvert.DeserializeObject<OSCommandBindingPZ70>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, d.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommandBindingPZ70 deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommandBindingPZ70>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, d.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);
        }

        private static OSCommandBindingPZ70 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ70_SerializeTests.GetPZ70DialPositionFromInstance(instanceNbr + 3),
                MultiPanelPZ70Knob = KeyBindingPZ70_SerializeTests.GetMultiPanelPZ70KnobsFromInstance(instanceNbr + 2),
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr + 4)
            };
        }
    }
}
