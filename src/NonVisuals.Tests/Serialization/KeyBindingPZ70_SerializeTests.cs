using MEF;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.Key;
using NonVisuals.Panels.Saitek.Panels;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class KeyBindingPZ70_SerializeTests
    {
        [Fact]
        public static void KeyBindingPZ70_ShouldBeSerializable() {
            KeyBindingPZ70 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyBindingPZ70 d = JsonConvert.DeserializeObject<KeyBindingPZ70>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, d.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, d.OSKeyPress.Description);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyBindingPZ70 deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyBindingPZ70>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.MultiPanelPZ70Knob, deseralizedObjFromFile.MultiPanelPZ70Knob);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
        }

        public static PZ70DialPosition GetPZ70DialPositionFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => PZ70DialPosition.ALT,
                2 => PZ70DialPosition.VS,
                3 => PZ70DialPosition.IAS,
                4 => PZ70DialPosition.HDG,
                5 => PZ70DialPosition.CRS,
                _ => PZ70DialPosition.ALT
            };
        }

        public static MultiPanelPZ70Knobs GetMultiPanelPZ70KnobsFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => MultiPanelPZ70Knobs.KNOB_ALT,
                2 => MultiPanelPZ70Knobs.KNOB_VS,
                3 => MultiPanelPZ70Knobs.KNOB_IAS,
                4 => MultiPanelPZ70Knobs.KNOB_HDG,
                5 => MultiPanelPZ70Knobs.KNOB_CRS,
                6 => MultiPanelPZ70Knobs.LCD_WHEEL_INC,
                7 => MultiPanelPZ70Knobs.LCD_WHEEL_DEC,
                8 => MultiPanelPZ70Knobs.AUTO_THROTTLE,
                9 => MultiPanelPZ70Knobs.FLAPS_LEVER_UP,
                10 => MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN,
                11 => MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP,
                12 => MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN,
                13 => MultiPanelPZ70Knobs.AP_BUTTON,
                14 => MultiPanelPZ70Knobs.HDG_BUTTON,
                15 => MultiPanelPZ70Knobs.NAV_BUTTON,
                16 => MultiPanelPZ70Knobs.IAS_BUTTON,
                17 => MultiPanelPZ70Knobs.ALT_BUTTON,
                18 => MultiPanelPZ70Knobs.VS_BUTTON,
                19 => MultiPanelPZ70Knobs.APR_BUTTON,
                20 => MultiPanelPZ70Knobs.REV_BUTTON,
                _ => MultiPanelPZ70Knobs.KNOB_ALT
            };
        }

        private static KeyBindingPZ70 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = GetPZ70DialPositionFromInstance(instanceNbr),
                MultiPanelPZ70Knob = GetMultiPanelPZ70KnobsFromInstance(instanceNbr),
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true
            };
        }
    }
}
