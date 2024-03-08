using System.Drawing;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using Xunit;

namespace DCSFP.Tests.Serialization {

    public static class FaceTypeImage_SerializeTests {

        [Fact]
        public static void FaceTypeImage_ShouldBeSerializable() {
            FaceTypeImage s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            FaceTypeImage d = JsonConvert.DeserializeObject<FaceTypeImage>(serializedObj);

            Assert.Equal(EnumStreamDeckFaceType.Image, d.FaceType);
            Assert.Equal(d.FaceType, s.FaceType);
            DeepAssert.Equal(s.TextFont, d.TextFont);
            DeepAssert.Equals(s.FontColor, d.FontColor);
            DeepAssert.Equal(s.BackgroundColor, d.BackgroundColor);

            Assert.Equal(d.StreamDeckButtonName, s.StreamDeckButtonName);
            Assert.Equal(d.OffsetX, s.OffsetX);
            Assert.Equal(d.OffsetY, s.OffsetY);
            Assert.Equal(d.ImageFile, s.ImageFile);


            //Not serialized :
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

            FaceTypeImage deseralizedObjFromFile = JsonConvert.DeserializeObject<FaceTypeImage>(repo.GetSerializedObjectString(s.GetType()));

            Assert.Equal(s.FaceType, deseralizedObjFromFile.FaceType);

            DeepAssert.Equal(s.TextFont, deseralizedObjFromFile.TextFont);
            DeepAssert.Equals(s.FontColor, deseralizedObjFromFile.FontColor);
            DeepAssert.Equal(s.BackgroundColor, deseralizedObjFromFile.BackgroundColor);

            Assert.Equal(s.StreamDeckButtonName, deseralizedObjFromFile.StreamDeckButtonName);
            Assert.Equal(s.OffsetX, deseralizedObjFromFile.OffsetX);
            Assert.Equal(s.OffsetY, deseralizedObjFromFile.OffsetY);
            Assert.True(string.IsNullOrEmpty(deseralizedObjFromFile.Text));
            Assert.Equal(s.ImageFile, deseralizedObjFromFile.ImageFile);
            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
            Assert.Null(deseralizedObjFromFile.StreamDeckButton);
        }

        public static FaceTypeImage GetObject(int instanceNbr = 1) {
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
