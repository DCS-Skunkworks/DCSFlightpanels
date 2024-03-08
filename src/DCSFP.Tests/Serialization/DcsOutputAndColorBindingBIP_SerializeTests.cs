using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Panels.Saitek;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class DcsOutputAndColorBindingBIP_SerializeTests {
        [Fact]
        public static void DcsOutputAndColorBindingBIP_ShouldBeSerializable() {
            DcsOutputAndColorBindingBIP s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DcsOutputAndColorBindingBIP d = JsonConvert.DeserializeObject<DcsOutputAndColorBindingBIP>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.LEDColor, d.LEDColor);
            DeepAssert.Equal(s.SaitekLEDPosition, d.SaitekLEDPosition);
            Assert.Equal(s.SaitekLEDPosition.GetPosition(), d.SaitekLEDPosition.GetPosition());
            Assert.Equal(s.DCSBiosOutputLED.Address, d.DCSBiosOutputLED.Address);
            Assert.Equal(s.DCSBiosOutputLED.DCSBiosOutputType, d.DCSBiosOutputLED.DCSBiosOutputType);
            Assert.Equal(s.DCSBiosOutputLED.MaxLength, d.DCSBiosOutputLED.MaxLength);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DcsOutputAndColorBindingBIP deseralizedObjFromFile = JsonConvert.DeserializeObject<DcsOutputAndColorBindingBIP>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            DeepAssert.Equal(s.SaitekLEDPosition, deseralizedObjFromFile.SaitekLEDPosition);
            Assert.Equal(s.SaitekLEDPosition.GetPosition(), deseralizedObjFromFile.SaitekLEDPosition.GetPosition());
            Assert.Equal(s.DCSBiosOutputLED.Address, deseralizedObjFromFile.DCSBiosOutputLED.Address);
            Assert.Equal(s.DCSBiosOutputLED.DCSBiosOutputType, deseralizedObjFromFile.DCSBiosOutputLED.DCSBiosOutputType);
            Assert.Equal(s.DCSBiosOutputLED.MaxLength, deseralizedObjFromFile.DCSBiosOutputLED.MaxLength);
        }

        public static DcsOutputAndColorBindingBIP GetObject(int instanceNbr = 1) {
            return new()
            {
                LEDColor = BIPLight_SerializeTests.GetPanelLEDColorFromInstance(instanceNbr + 2),
                SaitekLEDPosition = new SaitekPanelLEDPosition((int)BIPLight_SerializeTests.GetBIPLedPositionEnumFromInstance(instanceNbr + 2)),
                DCSBiosOutputLED = DCSBIOSOutput_SerializeTests.GetObject(instanceNbr + 2)
            };
        }
    }
}
