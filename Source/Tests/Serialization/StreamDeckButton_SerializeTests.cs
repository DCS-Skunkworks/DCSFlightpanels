using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;
using System;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class StreamDeckButton_SerializeTests {

        [Fact]
        public static void StreamDeckButton_ShouldBeSerializable() {
            StreamDeckButton s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            StreamDeckButton d = JsonConvert.DeserializeObject<StreamDeckButton>(serializedObj);

            Assert.True(s.StreamDeckButtonName == d.StreamDeckButtonName);


            //should be not serialized : 
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.False(d.IsVisible);
            //Assert.True(s.Description == d.Description); //deprecated


            RepositorySerialized repo = new();

            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            StreamDeckButton deseralizedObjFromFile = JsonConvert.DeserializeObject<StreamDeckButton>(repo.GetSerializedObjectString(d.GetType()));

            //Should be nice to test the object 's' with 'deseralizedObjFromFile' but since serialization/ deserialization is asymetric we will use the 'd' object 
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static EnumStreamDeckButtonNames GetStreamDeckButtonNameFromInstance(int instanceNbr) {
            EnumStreamDeckButtonNames buttonName;
            Enum.TryParse($"BUTTON{instanceNbr}", out buttonName);
            return buttonName;
        }

        public static EnumStreamDeckPushRotaryNames GetStreamDeckPushRotaryNameFromInstance(int instanceNbr) {
            EnumStreamDeckPushRotaryNames pushRotaryName;
            Enum.TryParse($"PUSHROTARY{instanceNbr}", out pushRotaryName);
            return pushRotaryName;
        }

        private static StreamDeckButton GetObject(int instanceNbr = 1) {
            return new()
            {
                StreamDeckButtonName = GetStreamDeckButtonNameFromInstance(instanceNbr),
                //Face = , /*TODO*/
                //ActionForPress =, /*TODO*/
                //ActionForRelease =, /*TODO*/

                //HasConfig = true, //get only
                //ActionType = EnumStreamDeckActionType.DCSBIOS, //get only
                //FaceType = EnumStreamDeckFaceType.DCSBIOS, //get only


                //should be not serialized:
                // Description = $"rhq opl {instanceNbr}", //deprecated
                IsVisible = true,
            };
        }
    }
}
