using System.Drawing;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using Xunit;

namespace DCSFP.Tests.Serialization {

    public static class FaceTypeDCSBIOS_SerializeTests {

        [Fact]
        public static void FaceTypeDCSBIOS_ShouldBeSerializable() {
            FaceTypeDCSBIOS s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            FaceTypeDCSBIOS d = JsonConvert.DeserializeObject<FaceTypeDCSBIOS>(serializedObj);

            Assert.Equal(EnumStreamDeckFaceType.DCSBIOS, d.FaceType);
            Assert.Equal(s.FaceType, d.FaceType);
            Assert.Equal(s.ButtonTextTemplate, d.ButtonTextTemplate);
            DeepAssert.Equal(s.TextFont, d.TextFont);
            DeepAssert.Equals(s.FontColor, d.FontColor);
            DeepAssert.Equal(s.BackgroundColor, d.BackgroundColor);

            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.OffsetX, d.OffsetX);
            Assert.Equal(s.OffsetY, d.OffsetY);
            Assert.Equal(s.UIntDcsBiosValue, d.UIntDcsBiosValue);
            Assert.Equal(s.StringDcsBiosValue, d.StringDcsBiosValue);


            //Not serialized :
            Assert.True(s.ConfigurationOK); //Dependant of object state
            Assert.True(d.ConfigurationOK); //Dependant of object state
            Assert.False(string.IsNullOrEmpty(s.FaceDescription));//Dependant of object state
            Assert.False(string.IsNullOrEmpty(d.FaceDescription));//Dependant of object state
            Assert.False(string.IsNullOrEmpty(s.ButtonFinalText));
            Assert.True(string.IsNullOrEmpty(d.ButtonFinalText));

            Assert.NotNull(s.Bitmap); //Dependant of object state
            Assert.NotNull(d.Bitmap); //Dependant of object state
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.Null(d.StreamDeckButton);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            FaceTypeDCSBIOS deseralizedObjFromFile = JsonConvert.DeserializeObject<FaceTypeDCSBIOS>(repo.GetSerializedObjectString(s.GetType()));

            Assert.Equal(s.FaceType, deseralizedObjFromFile.FaceType);
            Assert.Equal(s.ButtonTextTemplate, deseralizedObjFromFile.ButtonTextTemplate);
            DeepAssert.Equal(s.TextFont, deseralizedObjFromFile.TextFont);
            DeepAssert.Equals(s.FontColor, deseralizedObjFromFile.FontColor);
            DeepAssert.Equal(s.BackgroundColor, deseralizedObjFromFile.BackgroundColor);

            Assert.Equal(s.StreamDeckButtonName, deseralizedObjFromFile.StreamDeckButtonName);
            Assert.Equal(s.OffsetX, deseralizedObjFromFile.OffsetX);
            Assert.Equal(s.OffsetY, deseralizedObjFromFile.OffsetY);
            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
            Assert.Null(deseralizedObjFromFile.StreamDeckButton);
            Assert.True(string.IsNullOrEmpty(deseralizedObjFromFile.ButtonFinalText));
            Assert.Equal(s.UIntDcsBiosValue, deseralizedObjFromFile.UIntDcsBiosValue);
            Assert.Equal(s.StringDcsBiosValue, deseralizedObjFromFile.StringDcsBiosValue);
        }

        public static FaceTypeDCSBIOS GetObject(int instanceNbr = 1) {
            return new()
            {
                ButtonTextTemplate = $"iuy jbf {instanceNbr}",
                ButtonFinalText = $"fcc dcw {instanceNbr}",
                TextFont = SystemFonts.CaptionFont,
                FontColor = Color.CadetBlue,
                BackgroundColor = Color.DimGray,
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                OffsetX = 5 + instanceNbr,
                OffsetY = 6 + instanceNbr,
                UIntDcsBiosValue = 155 + (uint)instanceNbr,
                StringDcsBiosValue = $"amc prb {instanceNbr}",
                Bitmap = new RepositorySerialized().GetTestImageBitmap(),
                RawBitmap = new RepositorySerialized().GetTestImageBytes(),
            };
        }
    }
}
