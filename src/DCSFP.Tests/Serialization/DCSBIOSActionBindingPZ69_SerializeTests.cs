using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class DCSBIOSActionBindingPZ69_SerializeTests {
        [Fact]
        public static void DCSBIOSActionBindingPZ69_ShouldBeSerializable() {
            DCSBIOSActionBindingPZ69 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSActionBindingPZ69 d = JsonConvert.DeserializeObject<DCSBIOSActionBindingPZ69>(serializedObj, JSonSettings.JsonDefaultSettings);
            
            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.RadioPanelPZ69Knob, d.RadioPanelPZ69Knob);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.IsSequenced, d.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, d.DCSBIOSInputs);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSActionBindingPZ69 deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSActionBindingPZ69>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.RadioPanelPZ69Knob, deseralizedObjFromFile.RadioPanelPZ69Knob);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            Assert.Equal(s.IsSequenced, deseralizedObjFromFile.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, deseralizedObjFromFile.DCSBIOSInputs);
        }

        private static DCSBIOSActionBindingPZ69 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ69DialPosition_SerializeTests.GetPZ69DialPositionFromInstance(instanceNbr+7),
                RadioPanelPZ69Knob = KeyBindingPZ69_SerializeTests.GetRadioPanelPZ69KnobsEmulatorFromInstance(instanceNbr + 3),
                WhenTurnedOn = true,
                Description = $"dds ola {instanceNbr}",
                IsSequenced = true,
                DCSBIOSInputs = new(){
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 3),
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 4)
                }
            };
        }
    }
}
