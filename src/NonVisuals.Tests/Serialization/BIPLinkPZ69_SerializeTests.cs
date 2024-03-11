using Newtonsoft.Json;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class BIPLinkPZ69_SerializeTests {
        [Fact]
        public static void BIPLinkPZ69_ShouldBeSerializable() {
            BIPLinkPZ69 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLinkPZ69 d = JsonConvert.DeserializeObject<BIPLinkPZ69>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.RadioPanelPZ69Knob, d.RadioPanelPZ69Knob);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.Description, d.Description);
            DeepAssert.Equal(s.BIPLights, d.BIPLights);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLinkPZ69 deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLinkPZ69>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.RadioPanelPZ69Knob, deseralizedObjFromFile.RadioPanelPZ69Knob);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.Description, deseralizedObjFromFile.Description);
            DeepAssert.Equal(s.BIPLights, deseralizedObjFromFile.BIPLights);
        }

        private static BIPLinkPZ69 GetObject(int instanceNbr = 1) {
            return new()
            {
                RadioPanelPZ69Knob = KeyBindingPZ69_SerializeTests.GetRadioPanelPZ69KnobsEmulatorFromInstance(instanceNbr + 5),
                WhenTurnedOn = true,
                Description = $"ook bom {instanceNbr}",
                BIPLights = new()
                {   {instanceNbr, BIPLight_SerializeTests.GetObject(instanceNbr+4) },
                    {instanceNbr+1, BIPLight_SerializeTests.GetObject(instanceNbr+5) },
                    {instanceNbr+2, BIPLight_SerializeTests.GetObject(instanceNbr+6) },
                },

            };
        }
    }
}
