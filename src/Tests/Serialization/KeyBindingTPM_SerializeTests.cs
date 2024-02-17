using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.Key;
using Xunit;

namespace DCSFPTests.Serialization {
    public class KeyBindingTPM_SerializeTests {
        [Fact]
        public static void KeyBindingTPM_ShouldBeSerializable() {
            KeyBindingTPM s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyBindingTPM d = JsonConvert.DeserializeObject<KeyBindingTPM>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, d.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, d.OSKeyPress.Description);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyBindingTPM deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyBindingTPM>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.TPMSwitch, deseralizedObjFromFile.TPMSwitch);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
        }

        private static TPMPanelSwitches GetTPMPanelSwitchesFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => TPMPanelSwitches.G1,
                2 => TPMPanelSwitches.G2,
                3 => TPMPanelSwitches.G3,
                4 => TPMPanelSwitches.G4,
                5 => TPMPanelSwitches.G5,
                6 => TPMPanelSwitches.G6,
                7 => TPMPanelSwitches.G7,
                8 => TPMPanelSwitches.G8,
                9 => TPMPanelSwitches.G9,
                _ => TPMPanelSwitches.G1
            };
        }

        private static KeyBindingTPM GetObject(int instanceNbr = 1) {
            return new()
            {
                TPMSwitch = GetTPMPanelSwitchesFromInstance(instanceNbr),
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true
            };
        }
    }
}
