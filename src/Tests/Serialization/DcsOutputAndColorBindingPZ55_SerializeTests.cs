using DCSFPTests.Serialization.Common;
using NonVisuals.Panels.Saitek;
using Xunit;
using Newtonsoft.Json;
using NonVisuals.Panels.Saitek.Panels;
using DCS_BIOS.Serialized;
using System.Collections.Generic;
using NonVisuals.Panels.StreamDeck;

namespace DCSFPTests.Serialization {
    public class DcsOutputAndColorBindingPZ55_SerializeTests {

        [Fact]
        public static void DcsOutputAndColorBindingPZ55_ShouldBeSerializable() {
            DcsOutputAndColorBindingPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DcsOutputAndColorBindingPZ55 d = JsonConvert.DeserializeObject<DcsOutputAndColorBindingPZ55>(serializedObj,
                //JSonSettings.JsonDefaultSettings
                new JsonSerializerSettings
                 {
                    ContractResolver = new ExcludeObsoletePropertiesResolver(),
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (sender, args) =>
                    {
                        throw new System.Exception($"JSON Serialization Error.{args.ErrorContext.Error.Message}");
                    },
                    Converters = new List<JsonConverter> { new UnknownEnumConverter() }
                 }                
                );

            //Assert.Equal(s.LEDColor, d.LEDColor);
            //Assert.Equal(s.SaitekLEDPosition, d.SaitekLEDPosition);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
          //  repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

         //   DcsOutputAndColorBindingPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<DcsOutputAndColorBindingPZ55>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

          //  DeepAssert.Equal(s, deseralizedObjFromFile);
          //  DeepAssert.Equal(d, deseralizedObjFromFile);
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
                SaitekLEDPosition = new SaitekPanelLEDPosition(GetSwitchPanelPZ55LEDPositionFromInstance(instanceNbr+1)),
                DCSBiosOutputLED = DCSBIOSOutput_SerializeTests.GetObject(instanceNbr + 1)
            };
        }
    }
}
