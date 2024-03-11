using Newtonsoft.Json;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class BIPLinkPZ70_SerializeTests {
        [Fact]
        public static void BIPLinkPZ70_ShouldBeSerializable() {
            BIPLinkPZ70 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLinkPZ70 d = JsonConvert.DeserializeObject<BIPLinkPZ70>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, d.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            DeepAssert.Equal(s.BIPLights, d.BIPLights);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLinkPZ70 deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLinkPZ70>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, deseralizedObjFromFile.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            DeepAssert.Equal(s.BIPLights, deseralizedObjFromFile.BIPLights);
        }

        private static BIPLinkPZ70 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ70_SerializeTests.GetPZ70DialPositionFromInstance(instanceNbr + 2),
                MultiPanelPZ70Knob = KeyBindingPZ70_SerializeTests.GetMultiPanelPZ70KnobsFromInstance(instanceNbr + 3),
                WhenTurnedOn = true,
                Description = $"btc ojo {instanceNbr}",
                BIPLights = new()
                {   {instanceNbr, BIPLight_SerializeTests.GetObject(instanceNbr+7) },
                    {instanceNbr+1, BIPLight_SerializeTests.GetObject(instanceNbr+8) },
                    {instanceNbr+2, BIPLight_SerializeTests.GetObject(instanceNbr+9) },
                },

            };
        }
    }
}
