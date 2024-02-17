using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using System.Drawing;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class FaceTypeText_SerializeTests {

        [Fact]
        public static void FaceTypeText_ShouldBeSerializable() {
            FaceTypeText s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            FaceTypeText d = JsonConvert.DeserializeObject<FaceTypeText>(serializedObj);

            Assert.Equal(EnumStreamDeckFaceType.Text, d.FaceType);
            Assert.Equal(s.FaceType, d.FaceType);
            Assert.True(s.ButtonTextTemplate == d.ButtonTextTemplate);
            DeepAssert.Equal(s.TextFont, d.TextFont);
            DeepAssert.Equals(s.FontColor, d.FontColor);
            DeepAssert.Equal(s.BackgroundColor, d.BackgroundColor);

            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.OffsetX, d.OffsetX);
            Assert.Equal(s.OffsetY, d.OffsetY);


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

            FaceTypeText deseralizedObjFromFile = JsonConvert.DeserializeObject<FaceTypeText>(repo.GetSerializedObjectString(s.GetType()));

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

        }

        public static FaceTypeText GetObject(int instanceNbr = 1) {
            return new()
            {
                ButtonTextTemplate = $"ltn qae {instanceNbr}",
                ButtonFinalText = $"rth kiv {instanceNbr}",
                TextFont = new Font(new FontFamily("Arial"), 12, FontStyle.Italic),
                FontColor = Color.AliceBlue,
                BackgroundColor = Color.Violet,
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                OffsetX = 10 + instanceNbr,
                OffsetY = 20 + instanceNbr,
                Bitmap = new RepositorySerialized().GetTestImageBitmap(),
                RawBitmap = new RepositorySerialized().GetTestImageBytes(),
            };
        }
    }
}
