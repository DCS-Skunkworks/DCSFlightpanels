using DCS_BIOS.Tests.Serialization;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class DCSBIOSActionBindingPZ70_SerializeTests {
        [Fact]
        public static void DCSBIOSActionBindingPZ70_ShouldBeSerializable() {
            DCSBIOSActionBindingPZ70 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSActionBindingPZ70 d = JsonConvert.DeserializeObject<DCSBIOSActionBindingPZ70>(serializedObj, JSonSettings.JsonDefaultSettings);
            
            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, d.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.IsSequenced, d.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, d.DCSBIOSInputs);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSActionBindingPZ70 deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSActionBindingPZ70>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, deseralizedObjFromFile.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            Assert.Equal(s.IsSequenced, deseralizedObjFromFile.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, deseralizedObjFromFile.DCSBIOSInputs);
        }

        private static DCSBIOSActionBindingPZ70 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ70_SerializeTests.GetPZ70DialPositionFromInstance(instanceNbr + 4),
                MultiPanelPZ70Knob = KeyBindingPZ70_SerializeTests.GetMultiPanelPZ70KnobsFromInstance(instanceNbr + 5),
                WhenTurnedOn = true,
                Description = $"ecs ivt {instanceNbr}",
                IsSequenced = true,
                DCSBIOSInputs = new(){
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 5),
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 6)
                }
            };
        }
    }
}
