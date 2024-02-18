using DCSFPTests.Serialization.Common;
using Xunit;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.Key;
using MEF;

namespace DCSFPTests.Serialization {
    public class KeyBindingPZ55_SerializeTests {

        [Fact]
        public static void KeyBindingPZ55_ShouldBeSerializable() {
            KeyBindingPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyBindingPZ55 d = JsonConvert.DeserializeObject<KeyBindingPZ55>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, d.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, d.OSKeyPress.Description);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyBindingPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyBindingPZ55>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, deseralizedObjFromFile.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
        }

        public static SwitchPanelPZ55Keys GetSwitchPanelPZ55KeysFromInstance(int instanceNbr) {
            return instanceNbr switch 
            {
                1 => SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT,
                2 => SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT,
                3 => SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER,
                4 => SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP,
                5 => SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE,
                6 => SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT,
                7 => SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL,
                8 => SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL,
                9 => SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON,
                10 => SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV,
                11 => SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE,
                12 => SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI,
                13 => SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING,
                14 => SwitchPanelPZ55Keys.KNOB_ENGINE_OFF,
                15 => SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT,
                16 => SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT,
                17 => SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH,
                18 => SwitchPanelPZ55Keys.KNOB_ENGINE_START,
                19 => SwitchPanelPZ55Keys.LEVER_GEAR_UP,
                20 => SwitchPanelPZ55Keys.LEVER_GEAR_DOWN,
                _ => SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT
            };
        }

        private static KeyBindingPZ55 GetObject(int instanceNbr = 1) {
            return new()
            {
                SwitchPanelPZ55Key = GetSwitchPanelPZ55KeysFromInstance(instanceNbr),
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true               
            };
        }
    }
}
