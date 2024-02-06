using DCSFPTests.Serialization.Common;
using Xunit;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;
using NonVisuals.Interfaces;


namespace DCSFPTests.Serialization {

    public static class ActionTypeKey_SerializeTests {

        [Fact]
        public static void ActionTypeKey_ShouldBeSerializable() {
            ActionTypeKey s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            ActionTypeKey d = JsonConvert.DeserializeObject<ActionTypeKey>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(EnumStreamDeckActionType.KeyPress, s.ActionType);
            Assert.Equal(s.ActionType, d.ActionType);
            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.StreamDeckPushRotaryName, d.StreamDeckPushRotaryName);
            Assert.Equal(s.SoundFile, d.SoundFile);
            Assert.Equal(s.Volume, d.Volume);
            Assert.Equal(s.Delay, d.Delay);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.OSKeyPress.Information, d.OSKeyPress.Information);
            Assert.Equal(s.OSKeyPress.Description, d.OSKeyPress.Description);
            Assert.Equal(s.OSKeyPress.Abort, d.OSKeyPress.Abort);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, d.OSKeyPress.KeyPressSequence);


            //not serialized : 
            Assert.Null(d.StreamDeckPanelInstance);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            ActionTypeKey deseralizedObjFromFile = JsonConvert.DeserializeObject<ActionTypeKey>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.StreamDeckButtonName, deseralizedObjFromFile.StreamDeckButtonName);
            Assert.Equal(s.StreamDeckPushRotaryName, deseralizedObjFromFile.StreamDeckPushRotaryName);
            Assert.Equal(s.SoundFile, deseralizedObjFromFile.SoundFile);
            Assert.Equal(s.Volume, deseralizedObjFromFile.Volume);
            Assert.Equal(s.Delay, deseralizedObjFromFile.Delay);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            Assert.Equal(s.OSKeyPress.Information, deseralizedObjFromFile.OSKeyPress.Information);
            Assert.Equal(s.OSKeyPress.Description, deseralizedObjFromFile.OSKeyPress.Description);
            Assert.Equal(s.OSKeyPress.Abort, deseralizedObjFromFile.OSKeyPress.Abort);
            DeepAssert.Equal(s.OSKeyPress.KeyPressSequence, deseralizedObjFromFile.OSKeyPress.KeyPressSequence);

            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
        }

        public static ActionTypeKey GetObject(int instanceNbr = 1) {
            return new()
            {
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                StreamDeckPushRotaryName = StreamDeckButton_SerializeTests.GetStreamDeckPushRotaryNameFromInstance(instanceNbr),
                SoundFile = $"hht iiu {instanceNbr}",
                Volume = 125 + instanceNbr,
                Delay = 555 + instanceNbr,
                OSKeyPress = KeyPress_SerializeTests.GetObject(instanceNbr),
                WhenTurnedOn = true,
            };
        }
    }
}
