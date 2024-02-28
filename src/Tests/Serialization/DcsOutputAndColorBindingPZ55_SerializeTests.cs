using DCSFPTests.Serialization.Common;
using NonVisuals.Panels.Saitek;
using Xunit;
using Newtonsoft.Json;
using NonVisuals.Panels.Saitek.Panels;

namespace DCSFPTests.Serialization {
    public class DcsOutputAndColorBindingPZ55_SerializeTests {

        [Fact]
        public static void DcsOutputAndColorBindingPZ55_ShouldBeSerializable() {
            DcsOutputAndColorBindingPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DcsOutputAndColorBindingPZ55 d = JsonConvert.DeserializeObject<DcsOutputAndColorBindingPZ55>(serializedObj, JSonSettings.JsonDefaultSettings);
               
            Assert.Equal(s.LEDColor, d.LEDColor);
            DeepAssert.Equal(s.SaitekLEDPosition, d.SaitekLEDPosition);
            Assert.Equal(s.SaitekLEDPosition.GetPosition(), d.SaitekLEDPosition.GetPosition());
            Assert.Equal(s.DCSBiosOutputLED.Address, d.DCSBiosOutputLED.Address);
            Assert.Equal(s.DCSBiosOutputLED.DCSBiosOutputType, d.DCSBiosOutputLED.DCSBiosOutputType);
            Assert.Equal(s.DCSBiosOutputLED.MaxLength, d.DCSBiosOutputLED.MaxLength);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DcsOutputAndColorBindingPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<DcsOutputAndColorBindingPZ55>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            DeepAssert.Equal(s.SaitekLEDPosition, deseralizedObjFromFile.SaitekLEDPosition);
            Assert.Equal(s.SaitekLEDPosition.GetPosition(), deseralizedObjFromFile.SaitekLEDPosition.GetPosition());
            Assert.Equal(s.DCSBiosOutputLED.Address, deseralizedObjFromFile.DCSBiosOutputLED.Address);
            Assert.Equal(s.DCSBiosOutputLED.DCSBiosOutputType, deseralizedObjFromFile.DCSBiosOutputLED.DCSBiosOutputType);
            Assert.Equal(s.DCSBiosOutputLED.MaxLength, deseralizedObjFromFile.DCSBiosOutputLED.MaxLength);
        }

        private static SwitchPanelPZ55LEDPosition GetSwitchPanelPZ55LEDPositionFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => SwitchPanelPZ55LEDPosition.UP,
                2 => SwitchPanelPZ55LEDPosition.LEFT,
                3 => SwitchPanelPZ55LEDPosition.RIGHT,
                _ => SwitchPanelPZ55LEDPosition.UP
            };
        }

        public static DcsOutputAndColorBindingPZ55 GetObject(int instanceNbr = 1) {
            return new()
            {
                LEDColor = BIPLight_SerializeTests.GetPanelLEDColorFromInstance(instanceNbr+1),
                SaitekLEDPosition = new SaitekPanelLEDPosition((int)GetSwitchPanelPZ55LEDPositionFromInstance(instanceNbr+1)),
                DCSBiosOutputLED = DCSBIOSOutput_SerializeTests.GetObject(instanceNbr + 1)
            };
        }
    }
}
