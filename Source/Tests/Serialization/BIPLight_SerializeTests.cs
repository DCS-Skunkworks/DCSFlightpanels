using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Panels.Saitek;
using NonVisuals.Panels.Saitek.Panels;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class BIPLight_SerializeTests {
      
        [Fact]
        public static void BIPLight_ShouldBeSerializable() {
            BIPLight s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLight d = JsonConvert.DeserializeObject<BIPLight>(serializedObj);

            Assert.True(s.LEDColor == d.LEDColor);
            Assert.True(s.BIPLedPosition == d.BIPLedPosition);
            DeepAssert.Equal(s.DelayBefore, d.DelayBefore);
            Assert.True(s.BindingHash == d.BindingHash);
            Assert.Equal(new string[] { "|" }, d.Separator);

            RepositorySerialized repo = new();

            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLight deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLight>(repo.GetSerializedObjectString(d.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        private static BIPLight GetObject(int instanceNbr = 1) {
           return new()
            {
                LEDColor = PanelLEDColor.YELLOW,
                BIPLedPosition = BIPLedPositionEnum.Position_2_6,
                DelayBefore = BIPLightDelays.FiveHundredms,
                BindingHash = $"BL Hash {instanceNbr}"
            };
        }
    }
}
