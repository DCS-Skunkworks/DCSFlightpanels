using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using System.Drawing;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class FaceTypeDCSBIOS_SerializeTests {

        [Fact]
        public static void FaceTypeDCSBIOS_ShouldBeSerializable() {
            FaceTypeDCSBIOS s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            FaceTypeDCSBIOS d = JsonConvert.DeserializeObject<FaceTypeDCSBIOS>(serializedObj);

            Assert.True(d.FaceType == EnumStreamDeckFaceType.DCSBIOS);
            Assert.True(s.FaceType == d.FaceType);
            Assert.True(s.ButtonTextTemplate == d.ButtonTextTemplate);
            DeepAssert.Equal(s.TextFont, d.TextFont);
            DeepAssert.Equals(s.FontColor, d.FontColor);
            DeepAssert.Equal(s.BackgroundColor, d.BackgroundColor);

            Assert.True(s.StreamDeckButtonName == d.StreamDeckButtonName);
            Assert.True(s.OffsetX == d.OffsetX);
            Assert.True(s.OffsetY == d.OffsetY);
            Assert.True(s.UIntDcsBiosValue == d.UIntDcsBiosValue);
            Assert.True(s.StringDcsBiosValue == d.StringDcsBiosValue);


            //should be not serialized :
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

            FaceTypeDCSBIOS deseralizedObjFromFile = JsonConvert.DeserializeObject<FaceTypeDCSBIOS>(repo.GetSerializedObjectString(d.GetType()));

            Assert.True(s.FaceType == deseralizedObjFromFile.FaceType);
            Assert.True(s.ButtonTextTemplate == deseralizedObjFromFile.ButtonTextTemplate);
            DeepAssert.Equal(s.TextFont, deseralizedObjFromFile.TextFont);
            DeepAssert.Equals(s.FontColor, deseralizedObjFromFile.FontColor);
            DeepAssert.Equal(s.BackgroundColor, deseralizedObjFromFile.BackgroundColor);

            Assert.True(s.StreamDeckButtonName == deseralizedObjFromFile.StreamDeckButtonName);
            Assert.True(s.OffsetX == deseralizedObjFromFile.OffsetX);
            Assert.True(s.OffsetY == deseralizedObjFromFile.OffsetY);
            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
            Assert.Null(deseralizedObjFromFile.StreamDeckButton);
            Assert.True(string.IsNullOrEmpty(deseralizedObjFromFile.ButtonFinalText));
            Assert.True(s.UIntDcsBiosValue == deseralizedObjFromFile.UIntDcsBiosValue);
            Assert.True(s.StringDcsBiosValue == deseralizedObjFromFile.StringDcsBiosValue);
        }

        private static FaceTypeDCSBIOS GetObject(int instanceNbr = 1) {
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
