using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.Key;
using Xunit;

namespace DCSFPTests.Serialization {
    public class KeyBindingPZ69_SerializeTests {

        [Fact]
        public static void KeyBindingPZ69_ShouldBeSerializable() {
            KeyBindingPZ69 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyBindingPZ69 d = JsonConvert.DeserializeObject<KeyBindingPZ69>(serializedObj, JSonSettings.JsonDefaultSettings);

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

            KeyBindingPZ69 deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyBindingPZ69>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.RadioPanelPZ69Key, deseralizedObjFromFile.RadioPanelPZ69Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
        }

        public static RadioPanelPZ69KnobsEmulator GetRadioPanelPZ69KnobsEmulatorFromInstance(int instanceNbr) {
            return instanceNbr switch 
            {
               1 => RadioPanelPZ69KnobsEmulator.UpperCOM1,
               2 => RadioPanelPZ69KnobsEmulator.UpperCOM2,
               3 => RadioPanelPZ69KnobsEmulator.UpperNAV1,
               4 => RadioPanelPZ69KnobsEmulator.UpperNAV2,
               5 => RadioPanelPZ69KnobsEmulator.UpperADF,
               6 => RadioPanelPZ69KnobsEmulator.UpperDME,
               7 => RadioPanelPZ69KnobsEmulator.UpperXPDR,
               8 => RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc,
               9 => RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec,
               10 => RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc,
               11 => RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec,
               12 => RadioPanelPZ69KnobsEmulator.UpperFreqSwitch,
               13 => RadioPanelPZ69KnobsEmulator.LowerCOM1,
               14 => RadioPanelPZ69KnobsEmulator.LowerCOM2,
               15 => RadioPanelPZ69KnobsEmulator.LowerNAV1,
               16 => RadioPanelPZ69KnobsEmulator.LowerNAV2,
               17 => RadioPanelPZ69KnobsEmulator.LowerADF,
               18 => RadioPanelPZ69KnobsEmulator.LowerDME,
               19 => RadioPanelPZ69KnobsEmulator.LowerXPDR,
               20 => RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc,
               21 => RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec,
               22 => RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc,
               23 => RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec,
               24 => RadioPanelPZ69KnobsEmulator.LowerFreqSwitch,
               _ => RadioPanelPZ69KnobsEmulator.UpperCOM1,
            };
        }

        private static KeyBindingPZ69 GetObject(int instanceNbr = 1) {
            return new()
            {
                RadioPanelPZ69Key = GetRadioPanelPZ69KnobsEmulatorFromInstance(instanceNbr),
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true
            };
        }
    }
}
