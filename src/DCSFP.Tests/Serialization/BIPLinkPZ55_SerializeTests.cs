using System.Collections.Generic;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.BIP;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class BIPLinkPZ55_SerializeTests {

        [Fact]
        public static void BIPLinkPZ55_ShouldBeSerializable() {
            BIPLinkPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLinkPZ55 d = JsonConvert.DeserializeObject<BIPLinkPZ55>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, d.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            DeepAssert.Equal(s.BIPLights, d.BIPLights);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLinkPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLinkPZ55>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, deseralizedObjFromFile.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            DeepAssert.Equal(s.BIPLights, deseralizedObjFromFile.BIPLights);
        }

        public static HashSet<BIPLinkPZ55> GetObjects() {
            HashSet<BIPLinkPZ55> hashSet = new();
            for (int i = 0; i < 3; i++) {
                hashSet.Add(GetObject(i));
            }
            return hashSet;
        }

        private static BIPLinkPZ55 GetObject(int instanceNbr = 1) {
            return new()
            {
                SwitchPanelPZ55Key = KeyBindingPZ55_SerializeTests.GetSwitchPanelPZ55KeysFromInstance(instanceNbr + 4),
                WhenTurnedOn = true,
                Description = $"hgh ido {instanceNbr}",
                BIPLights = new() 
                {   {instanceNbr, BIPLight_SerializeTests.GetObject(instanceNbr+1) },
                    {instanceNbr+1, BIPLight_SerializeTests.GetObject(instanceNbr+2) },
                    {instanceNbr+2, BIPLight_SerializeTests.GetObject(instanceNbr+3) },
                },

            };
        }
    }
}
