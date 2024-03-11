using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class ActionTypeOS_SerializeTests {

        [Fact]
        public static void ActionTypeOS_ShouldBeSerializable() {
            ActionTypeOS s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            ActionTypeOS d = JsonConvert.DeserializeObject<ActionTypeOS>(serializedObj);

            Assert.Equal(EnumStreamDeckActionType.OSCommand, s.ActionType);
            Assert.Equal(s.ActionType, d.ActionType);
            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.StreamDeckPushRotaryName, d.StreamDeckPushRotaryName);
            Assert.Equal(s.SoundFile, d.SoundFile);
            Assert.Equal(s.Volume, d.Volume);
            Assert.Equal(s.Delay, d.Delay);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //not serialized:
            //ActionDescription
            //StreamDeckPanelInstance

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            ActionTypeOS deseralizedObjFromFile = JsonConvert.DeserializeObject<ActionTypeOS>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static ActionTypeOS GetObject(int instanceNbr = 1) {
            return new()
            {
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                StreamDeckPushRotaryName = StreamDeckPushRotary_SerializeTests.GetStreamDeckPushRotaryNameFromInstance(instanceNbr),
                SoundFile = $"xdl ero {instanceNbr}",
                Volume = 104 + instanceNbr,
                Delay = 1001 + instanceNbr,
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr)
            };
        }
    }
}
