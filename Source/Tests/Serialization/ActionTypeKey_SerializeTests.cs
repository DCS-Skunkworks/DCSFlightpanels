using DCSFPTests.Serialization.Common;
using Xunit;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;


namespace DCSFPTests.Serialization {
    
    public static class ActionTypeKey_SerializeTests {
     
        [Fact]
        public static void ActionTypeKey_ShouldBeSerializable() {
            ActionTypeKey s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            ActionTypeKey d = JsonConvert.DeserializeObject<ActionTypeKey>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.True(s.StreamDeckButtonName == d.StreamDeckButtonName);
            Assert.True(s.StreamDeckPushRotaryName == d.StreamDeckPushRotaryName);
            DeepAssert.Equal(s.SoundFile, d.SoundFile);
            Assert.True(s.Volume == d.Volume);
            Assert.True(s.Delay == d.Delay);
            

            //not serialized : 
            Assert.Null(d.StreamDeckPanelInstance);
            
            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            ActionTypeKey deseralizedObjFromFile = JsonConvert.DeserializeObject<ActionTypeKey>(repo.GetSerializedObjectString(d.GetType()), JSonSettings.JsonDefaultSettings);
            
            Assert.True(s.StreamDeckButtonName == deseralizedObjFromFile.StreamDeckButtonName);
            Assert.True(s.StreamDeckPushRotaryName == deseralizedObjFromFile.StreamDeckPushRotaryName);
            DeepAssert.Equal(s.SoundFile, deseralizedObjFromFile.SoundFile);
            Assert.True(s.Volume == deseralizedObjFromFile.Volume);
            Assert.True(s.Delay == deseralizedObjFromFile.Delay);
            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
        }

        public static ActionTypeKey GetObject(int instanceNbr = 1) {
            return new ()
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
