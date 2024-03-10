using Newtonsoft.Json;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class BIPLinkFarmingPanel_SerializeTests {
        [Fact]
        public static void BIPLinkFarmingPanel_ShouldBeSerializable() {
            BIPLinkFarmingPanel s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLinkFarmingPanel d = JsonConvert.DeserializeObject<BIPLinkFarmingPanel>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, d.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            DeepAssert.Equal(s.BIPLights, d.BIPLights);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLinkFarmingPanel deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLinkFarmingPanel>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, deseralizedObjFromFile.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            DeepAssert.Equal(s.BIPLights, deseralizedObjFromFile.BIPLights);
        }

        private static BIPLinkFarmingPanel GetObject(int instanceNbr = 1) {
            return new()
            {
                FarmingPanelKey = KeyBindingFarmingPanel_SerializeTests.GetFarmingPanelMKKeysFromInstance(instanceNbr + 3),
                WhenTurnedOn = true,
                Description = $"rfv nht {instanceNbr}",
                BIPLights = new()
                {   {instanceNbr, BIPLight_SerializeTests.GetObject(instanceNbr+1) },
                    {instanceNbr+1, BIPLight_SerializeTests.GetObject(instanceNbr+3) },
                    {instanceNbr+2, BIPLight_SerializeTests.GetObject(instanceNbr+5) },
                },

            };
        }
    }
}
