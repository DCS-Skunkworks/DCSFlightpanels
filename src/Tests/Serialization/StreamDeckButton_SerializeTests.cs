using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using System;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class StreamDeckButton_SerializeTests {

        [Fact]
        public static void StreamDeckButton_ShouldBeSerializable() {
            StreamDeckButton s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            StreamDeckButton d = JsonConvert.DeserializeObject<StreamDeckButton>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);

            //Face, ActionForPress, ActionForRelease 
            // are already tested for serialization in their own tests

            //Not serialized :
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.False(d.IsVisible);
            //Assert.True(s.Description == d.Description); //deprecated

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            StreamDeckButton deseralizedObjFromFile = JsonConvert.DeserializeObject<StreamDeckButton>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);
        }

        public static EnumStreamDeckButtonNames GetStreamDeckButtonNameFromInstance(int instanceNbr) {
            Enum.TryParse($"BUTTON{instanceNbr}", out EnumStreamDeckButtonNames buttonName);
            return buttonName;
        }

        public static IStreamDeckButtonFace GetFaceFromInstanceNbr(int instanceNbr) {
            return instanceNbr switch
            {
                1 => FaceTypeDCSBIOS_SerializeTests.GetObject(instanceNbr),
                2 => FaceTypeDCSBIOSOverlay_SerializeTests.GetObject(instanceNbr),
                3 => FaceTypeImage_SerializeTests.GetObject(instanceNbr),
                4 => FaceTypeText_SerializeTests.GetObject(instanceNbr),
                _ => FaceTypeDCSBIOS_SerializeTests.GetObject(instanceNbr),
            };
        }

        public static IStreamDeckButtonAction GetActionFromInstanceNbr(int instanceNbr) {
            return instanceNbr switch
            {
                1 => ActionTypeDCSBIOS_SerializeTests.GetObject(instanceNbr),
                2 => ActionTypeKey_SerializeTests.GetObject(instanceNbr),
                3 => ActionTypeLayer_SerializeTests.GetObject(instanceNbr),
                4 => ActionTypeOS_SerializeTests.GetObject(instanceNbr),
                _ => ActionTypeDCSBIOS_SerializeTests.GetObject(instanceNbr),
            };
        }

        private static StreamDeckButton GetObject(int instanceNbr = 1) {
            return new()
            {
                StreamDeckButtonName = GetStreamDeckButtonNameFromInstance(instanceNbr),
                Face = GetFaceFromInstanceNbr(instanceNbr),
                ActionForPress = GetActionFromInstanceNbr(instanceNbr),
                ActionForRelease = GetActionFromInstanceNbr(instanceNbr + 1),

                //HasConfig = true, //get only
                //ActionType = EnumStreamDeckActionType.DCSBIOS, //get only
                //FaceType = EnumStreamDeckFaceType.DCSBIOS, //get only

                //Not serialized :
                // Description = $"rhq opl {instanceNbr}", //deprecated
                IsVisible = false, //do not set this to true during tests
            };
        }
    }
}
