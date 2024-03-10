using DCS_BIOS.Tests.Serialization;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class DCSBIOSActionBindingFarmingPanel_SerializeTests {
        [Fact]
        public static void DCSBIOSActionBindingFarmingPanel_ShouldBeSerializable() {
            DCSBIOSActionBindingFarmingPanel s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSActionBindingFarmingPanel d = JsonConvert.DeserializeObject<DCSBIOSActionBindingFarmingPanel>(serializedObj, JSonSettings.JsonDefaultSettings);
            
            Assert.Equal(s.FarmingPanelKey, d.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.IsSequenced, d.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, d.DCSBIOSInputs);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSActionBindingFarmingPanel deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSActionBindingFarmingPanel>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, deseralizedObjFromFile.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            Assert.Equal(s.IsSequenced, deseralizedObjFromFile.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, deseralizedObjFromFile.DCSBIOSInputs);
        }

        private static DCSBIOSActionBindingFarmingPanel GetObject(int instanceNbr = 1) {
            return new()
            {
                FarmingPanelKey = KeyBindingFarmingPanel_SerializeTests.GetFarmingPanelMKKeysFromInstance(instanceNbr + 9),
                WhenTurnedOn = true,
                Description = $"dkk aqk {instanceNbr}",
                IsSequenced = true,
                DCSBIOSInputs = new(){
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 7),
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 8)
                }
            };
        }
    }
}
