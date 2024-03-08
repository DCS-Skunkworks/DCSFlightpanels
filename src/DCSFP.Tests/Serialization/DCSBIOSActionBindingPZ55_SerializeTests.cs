using System.Collections.Generic;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class DCSBIOSActionBindingPZ55_SerializeTests {
        [Fact]
        public static void DCSBIOSActionBindingPZ55_ShouldBeSerializable() {
            DCSBIOSActionBindingPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSActionBindingPZ55 d = JsonConvert.DeserializeObject<DCSBIOSActionBindingPZ55>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, d.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            Assert.Equal(s.IsSequenced, d.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, d.DCSBIOSInputs);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSActionBindingPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSActionBindingPZ55>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, deseralizedObjFromFile.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            Assert.Equal(s.IsSequenced, deseralizedObjFromFile.IsSequenced);
            DeepAssert.Equal(s.DCSBIOSInputs, deseralizedObjFromFile.DCSBIOSInputs);
        }

        public static HashSet<DCSBIOSActionBindingPZ55> GetObjects() {
            HashSet<DCSBIOSActionBindingPZ55> hashSet = new();
            for (int i = 0; i<3; i++) {
                hashSet.Add(GetObject(i));
            }
            return hashSet;
        }

        private static DCSBIOSActionBindingPZ55 GetObject(int instanceNbr = 1) {
            return new()
            {
                SwitchPanelPZ55Key = KeyBindingPZ55_SerializeTests.GetSwitchPanelPZ55KeysFromInstance(instanceNbr + 4),
                WhenTurnedOn = true,
                Description = $"iiu kkh {instanceNbr}",
                IsSequenced = true,
                DCSBIOSInputs = new(){
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 2),
                    DCSBIOSInput_SerializeTests.GetObject(instanceNbr + 3)
                }
            };
        }
    }
}
