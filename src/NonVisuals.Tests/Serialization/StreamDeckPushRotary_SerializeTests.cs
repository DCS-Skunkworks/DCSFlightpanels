using MEF;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;
using NonVisuals.Tests.Serialization.Common;
using System;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class StreamDeckPushRotary_SerializeTests {

        [Fact]
        public static void StreamDeckPushRotary_ShouldBeSerializable() {
            StreamDeckPushRotary s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            StreamDeckPushRotary d = JsonConvert.DeserializeObject<StreamDeckPushRotary>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.StreamDeckPushRotaryName, d.StreamDeckPushRotaryName);

            //ActionForPress, ActionForRelease 
            // are already tested for serialization in their own tests

            //Not serialized :
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.False(d.IsVisible);
            //Assert.True(s.Description == d.Description); //deprecated

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            StreamDeckPushRotary deseralizedObjFromFile = JsonConvert.DeserializeObject<StreamDeckPushRotary>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);
            Assert.Equal(s.StreamDeckPushRotaryName, deseralizedObjFromFile.StreamDeckPushRotaryName);
        }

        public static EnumStreamDeckPushRotaryNames GetStreamDeckPushRotaryNameFromInstance(int instanceNbr) {
            Enum.TryParse($"PUSHROTARY{instanceNbr}", out EnumStreamDeckPushRotaryNames pushRotaryName);
            return pushRotaryName;
        }

        private static StreamDeckPushRotary GetObject(int instanceNbr = 1) {
            return new()
            {
                StreamDeckPushRotaryName = GetStreamDeckPushRotaryNameFromInstance(instanceNbr + 1),
                ActionForPress = StreamDeckButton_SerializeTests.GetActionFromInstanceNbr(instanceNbr + 2),
                ActionForRelease = StreamDeckButton_SerializeTests.GetActionFromInstanceNbr(instanceNbr + 3),

                //HasConfig = true, //get only
                //ActionType = EnumStreamDeckActionType.DCSBIOS, //get only

                //Not serialized :
                // Description = $"rhq opl {instanceNbr}", //deprecated
                IsVisible = false, //do not set this to true during tests
            };
        }
    }
}
