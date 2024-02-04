using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using System.Drawing;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class FaceTypeImage_SerializeTests {

        [Fact]
        public static void FaceTypeImage_ShouldBeSerializable() {
            FaceTypeImage s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            FaceTypeImage d = JsonConvert.DeserializeObject<FaceTypeImage>(serializedObj);

            Assert.True(d.FaceType == EnumStreamDeckFaceType.Image);
            Assert.True(s.FaceType == d.FaceType);
            DeepAssert.Equal(s.TextFont, d.TextFont);
            DeepAssert.Equals(s.FontColor, d.FontColor);
            DeepAssert.Equal(s.BackgroundColor, d.BackgroundColor);

            Assert.True(s.StreamDeckButtonName == d.StreamDeckButtonName);
            Assert.True(s.OffsetX == d.OffsetX);
            Assert.True(s.OffsetY == d.OffsetY);
            Assert.True(s.ImageFile == d.ImageFile);


            //should be not serialized :
            Assert.True(s.ConfigurationOK); //Dependant of object state
            Assert.True(d.ConfigurationOK); //Dependant of object state
            Assert.False(string.IsNullOrEmpty(s.FaceDescription));//Dependant of object state
            Assert.False(string.IsNullOrEmpty(d.FaceDescription));//Dependant of object state

            Assert.NotNull(s.Bitmap); //Dependant of object state
            Assert.NotNull(d.Bitmap); //Dependant of object state
            Assert.True(string.IsNullOrEmpty(d.Text));
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.Null(d.StreamDeckButton);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            FaceTypeImage deseralizedObjFromFile = JsonConvert.DeserializeObject<FaceTypeImage>(repo.GetSerializedObjectString(d.GetType()));

            Assert.True(s.FaceType == deseralizedObjFromFile.FaceType);

            DeepAssert.Equal(s.TextFont, deseralizedObjFromFile.TextFont);
            DeepAssert.Equals(s.FontColor, deseralizedObjFromFile.FontColor);
            DeepAssert.Equal(s.BackgroundColor, deseralizedObjFromFile.BackgroundColor);

            Assert.True(s.StreamDeckButtonName == deseralizedObjFromFile.StreamDeckButtonName);
            Assert.True(s.OffsetX == deseralizedObjFromFile.OffsetX);
            Assert.True(s.OffsetY == deseralizedObjFromFile.OffsetY);
            Assert.True(string.IsNullOrEmpty(deseralizedObjFromFile.Text));
            Assert.True(s.ImageFile == deseralizedObjFromFile.ImageFile);
            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
            Assert.Null(deseralizedObjFromFile.StreamDeckButton);
        }

        private static FaceTypeImage GetObject(int instanceNbr = 1) {
            return new()
            {
                Text = $"wor nmp {instanceNbr}",
                ImageFile = $"xzj kga {instanceNbr}",
                TextFont = SystemFonts.CaptionFont,
                FontColor = Color.MediumVioletRed,
                BackgroundColor = Color.GreenYellow,
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                OffsetX = 30 + instanceNbr,
                OffsetY = 40 + instanceNbr,
                Bitmap = new RepositorySerialized().GetTestImageBitmap(),
                RawBitmap = new RepositorySerialized().GetTestImageBytes(),
            };
        }
    }
}
