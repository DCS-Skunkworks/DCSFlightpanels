using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using System.Drawing;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class FaceTypeDCSBIOSOverlay_SerializeTests {

        [Fact]
        public static void FaceTypeDCSBIOSOverlay_ShouldBeSerializable() {
            FaceTypeDCSBIOSOverlay s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            FaceTypeDCSBIOSOverlay d = JsonConvert.DeserializeObject<FaceTypeDCSBIOSOverlay>(serializedObj);

            Assert.Equal(EnumStreamDeckFaceType.DCSBIOSOverlay, d.FaceType);
            Assert.Equal(s.FaceType , d.FaceType);
            Assert.Equal(s.ButtonTextTemplate, d.ButtonTextTemplate);
            Assert.Equal(s.BackgroundBitmapPath, d.BackgroundBitmapPath);
            DeepAssert.Equal(s.TextFont, d.TextFont);
            DeepAssert.Equals(s.FontColor, d.FontColor);
            DeepAssert.Equal(s.BackgroundColor, d.BackgroundColor);

            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.OffsetX, d.OffsetX);
            Assert.Equal(s.OffsetY,  d.OffsetY);
            Assert.Equal(s.UIntDcsBiosValue, d.UIntDcsBiosValue);
            Assert.Equal(s.StringDcsBiosValue, d.StringDcsBiosValue);
            Assert.NotNull(d.RawBitmap);

            //Not serialized :
            Assert.True(s.ConfigurationOK); //Dependant of object state
            Assert.True(d.ConfigurationOK); //Dependant of object state
            Assert.False(string.IsNullOrEmpty(s.FaceDescription));//Dependant of object state
            Assert.False(string.IsNullOrEmpty(d.FaceDescription));//Dependant of object state
            Assert.False(string.IsNullOrEmpty(s.ButtonFinalText));
            Assert.True(string.IsNullOrEmpty(d.ButtonFinalText));
            Assert.NotNull(s.BackgroundBitmap);


            Assert.NotNull(s.Bitmap); //Dependant of object state
            Assert.NotNull(d.Bitmap); //Dependant of object state
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.Null(d.StreamDeckButton);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            FaceTypeDCSBIOSOverlay deseralizedObjFromFile = JsonConvert.DeserializeObject<FaceTypeDCSBIOSOverlay>(repo.GetSerializedObjectString(s.GetType()));

            Assert.Equal(s.FaceType, deseralizedObjFromFile.FaceType);
            Assert.Equal(s.ButtonTextTemplate, deseralizedObjFromFile.ButtonTextTemplate);
            Assert.Equal(s.BackgroundBitmapPath, deseralizedObjFromFile.BackgroundBitmapPath);
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
            Assert.Null(deseralizedObjFromFile.BackgroundBitmap);
            Assert.NotNull(deseralizedObjFromFile.RawBitmap);
        }

        public static FaceTypeDCSBIOSOverlay GetObject(int instanceNbr = 1) {
            return new()
            {
                ButtonTextTemplate = $"lyn dct {instanceNbr}",
                ButtonFinalText = $"qzd iuc {instanceNbr}",
                BackgroundBitmapPath = $"rtt uy {instanceNbr}",
                BackgroundBitmap = new RepositorySerialized().GetTestImageBitmap(),
                TextFont = SystemFonts.CaptionFont,
                FontColor = Color.Chartreuse,
                BackgroundColor = Color.Honeydew,
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                OffsetX = 55 + instanceNbr,
                OffsetY = 66 + instanceNbr,
                UIntDcsBiosValue = 3000 + (uint)instanceNbr,
                StringDcsBiosValue = $"wxe ofs {instanceNbr}",
                Bitmap = new RepositorySerialized().GetTestImageBitmap(),
                RawBitmap = new RepositorySerialized().GetTestImageBytes(),
            };
        }
    }
}
