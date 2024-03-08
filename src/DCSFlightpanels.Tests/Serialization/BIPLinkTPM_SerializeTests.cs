using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.BIP;
using Xunit;

namespace DCSFPTests.Serialization {
    public class BIPLinkTPM_SerializeTests {
        [Fact]
        public static void BIPLinkTPM_ShouldBeSerializable() {
            BIPLinkTPM s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLinkTPM d = JsonConvert.DeserializeObject<BIPLinkTPM>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, d.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            DeepAssert.Equal(s.BIPLights, d.BIPLights);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLinkTPM deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLinkTPM>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, deseralizedObjFromFile.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            DeepAssert.Equal(s.BIPLights, deseralizedObjFromFile.BIPLights);
        }

        private static BIPLinkTPM GetObject(int instanceNbr = 1) {
            return new()
            {
                TPMSwitch = KeyBindingTPM_SerializeTests.GetTPMPanelSwitchesFromInstance(instanceNbr + 3),
                WhenTurnedOn = true,
                Description = $"poi kjh {instanceNbr}",
                BIPLights = new()
                {   {instanceNbr, BIPLight_SerializeTests.GetObject(instanceNbr+10) },
                    {instanceNbr+1, BIPLight_SerializeTests.GetObject(instanceNbr+11) },
                    {instanceNbr+2, BIPLight_SerializeTests.GetObject(instanceNbr+12) },
                },

            };
        }
    }
}
