using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.Key;

using Xunit;

namespace DCSFPTests.Serialization {
    public class KeyBindingFarmingPanel_SerializeTests {
        [Fact]
        public static void KeyBindingFarmingPanel_ShouldBeSerializable() {
            KeyBindingFarmingPanel s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyBindingFarmingPanel d = JsonConvert.DeserializeObject<KeyBindingFarmingPanel>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, d.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, d.OSKeyPress.Description);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyBindingFarmingPanel deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyBindingFarmingPanel>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, deseralizedObjFromFile.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
        }

        private static FarmingPanelMKKeys GetFarmingPanelMKKeysFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => FarmingPanelMKKeys.BUTTON_1,
                2 => FarmingPanelMKKeys.BUTTON_2,
                3 => FarmingPanelMKKeys.BUTTON_3,
                4 => FarmingPanelMKKeys.BUTTON_4,
                5 => FarmingPanelMKKeys.BUTTON_5,
                6 => FarmingPanelMKKeys.BUTTON_6,
                7 => FarmingPanelMKKeys.BUTTON_7,
                8 => FarmingPanelMKKeys.BUTTON_8,
                9 => FarmingPanelMKKeys.BUTTON_9,
                10 => FarmingPanelMKKeys.BUTTON_10,
                11 => FarmingPanelMKKeys.BUTTON_11,
                12 => FarmingPanelMKKeys.BUTTON_12,
                13 => FarmingPanelMKKeys.BUTTON_13,
                14 => FarmingPanelMKKeys.BUTTON_14,
                15 => FarmingPanelMKKeys.BUTTON_15,
                16 => FarmingPanelMKKeys.BUTTON_16,
                17 => FarmingPanelMKKeys.BUTTON_17,
                18 => FarmingPanelMKKeys.BUTTON_18,
                19 => FarmingPanelMKKeys.BUTTON_19,
                20 => FarmingPanelMKKeys.BUTTON_20,
                21 => FarmingPanelMKKeys.BUTTON_21,
                22 => FarmingPanelMKKeys.BUTTON_22,
                23 => FarmingPanelMKKeys.BUTTON_23,
                24 => FarmingPanelMKKeys.BUTTON_24,
                25 => FarmingPanelMKKeys.BUTTON_25,
                26 => FarmingPanelMKKeys.BUTTON_26,
                27 => FarmingPanelMKKeys.BUTTON_27,
                28 => FarmingPanelMKKeys.BUTTON_JOY_RIGHT,
                29 => FarmingPanelMKKeys.BUTTON_JOY_LEFT,
                _ => FarmingPanelMKKeys.BUTTON_1
            };
        }

        private static KeyBindingFarmingPanel GetObject(int instanceNbr = 1) {
            return new()
            {
                FarmingPanelKey = GetFarmingPanelMKKeysFromInstance(instanceNbr),
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true
            };
        }
    }
}
