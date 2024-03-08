using DCSFP.Tests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.Key;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class KeyBindingPZ69DialPosition_SerializeTests {
        [Fact]
        public static void KeyBindingPZ69DialPosition_ShouldBeSerializable() {
            KeyBindingPZ69DialPosition s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyBindingPZ69DialPosition d = JsonConvert.DeserializeObject<KeyBindingPZ69DialPosition>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.RadioPanelPZ69Key, d.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, d.OSKeyPress.Description);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyBindingPZ69DialPosition deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyBindingPZ69DialPosition>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.RadioPanelPZ69Key, deseralizedObjFromFile.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
        }

        public static PZ69DialPosition GetPZ69DialPositionFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => PZ69DialPosition.UpperCOM1,
                2 => PZ69DialPosition.UpperCOM2,
                3 => PZ69DialPosition.UpperNAV1,
                4 => PZ69DialPosition.UpperNAV2,
                5 => PZ69DialPosition.UpperADF,
                6 => PZ69DialPosition.UpperDME,
                7 => PZ69DialPosition.UpperXPDR,
                8 => PZ69DialPosition.LowerCOM1,
                9 => PZ69DialPosition.LowerCOM2,
                10 => PZ69DialPosition.LowerNAV1,
                11 => PZ69DialPosition.LowerNAV2,
                12 => PZ69DialPosition.LowerADF,
                13 => PZ69DialPosition.LowerDME,
                14 => PZ69DialPosition.LowerXPDR,
                15 => PZ69DialPosition.Unknown,
                _ => PZ69DialPosition.UpperCOM1
            };
        }
       
        private static KeyBindingPZ69DialPosition GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = GetPZ69DialPositionFromInstance(instanceNbr),
                RadioPanelPZ69Key = KeyBindingPZ69_SerializeTests.GetRadioPanelPZ69KnobsEmulatorFromInstance(instanceNbr),
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true
            };
        }
    }
}
