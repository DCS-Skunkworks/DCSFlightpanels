using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class DCSBIOSActionBindingTPM_SerializeTests {
        [Fact]
        public static void DCSBIOSActionBindingTPM_ShouldBeSerializable() {
            DCSBIOSActionBindingTPM s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSActionBindingTPM d = JsonConvert.DeserializeObject<DCSBIOSActionBindingTPM>(serializedObj, JSonSettings.JsonDefaultSettings);
            
            Assert.Equal(s.TPMSwitch, d.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.IsSequenced, d.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, d.DCSBIOSInputs);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSActionBindingTPM deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSActionBindingTPM>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, deseralizedObjFromFile.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            Assert.Equal(s.IsSequenced, deseralizedObjFromFile.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, deseralizedObjFromFile.DCSBIOSInputs);
        }

        private static DCSBIOSActionBindingTPM GetObject(int instanceNbr = 1) {
            return new()
            {
                TPMSwitch = KeyBindingTPM_SerializeTests.GetTPMPanelSwitchesFromInstance(instanceNbr + 7),
                WhenTurnedOn = true,
                Description = $"yea tal {instanceNbr}",
                IsSequenced = true,
                DCSBIOSInputs = new(){
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 9),
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 10)
                }
            };
        }
    }
}
