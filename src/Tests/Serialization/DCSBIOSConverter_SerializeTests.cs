using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;
using System;
using Xunit;

namespace DCSFPTests.Serialization {
    public class DCSBIOSConverter_SerializeTests {
        [Fact]
        public static void DCSBIOSConverter_ShouldBeSerializable() {
            DCSBIOSConverter s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSConverter d = JsonConvert.DeserializeObject<DCSBIOSConverter>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.ConverterOutputType, d.ConverterOutputType);
            Assert.Equal(s.Comparator1, d.Comparator1);
            Assert.Equal(s.ReferenceValue1, d.ReferenceValue1);
            Assert.Equal(s.Comparator2, d.Comparator2);
            Assert.Equal(s.ReferenceValue2, d.ReferenceValue2);

            //FaceTypeText, FaceTypeImage, FaceTypeDCSBIOSOverlay
            // are already tested for serialization in their own tests

            //Not serialized dependant of Object state:
            Assert.Null(d.StreamDeckPanelInstance);
            Assert.NotNull(d.Bitmap);
            Assert.NotNull(d.TextFont);
            Assert.NotNull(d.FriendlyInfo);
            Assert.NotNull(d.ImageFileRelativePath);
            Assert.NotNull(d.ButtonTextTemplate);
            Assert.Equal(s.OffsetX, d.OffsetX);
            Assert.Equal(s.OffsetY, d.OffsetY);
            Assert.Equal(s.FontColor, d.FontColor);
            Assert.Equal(s.BackgroundColor, d.BackgroundColor);
            Assert.Throws<Exception>(() => d.DCSBIOSValue);


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSConverter deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSConverter>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);
            Assert.Equal(s.ConverterOutputType, deseralizedObjFromFile.ConverterOutputType);
            Assert.Equal(s.Comparator1, deseralizedObjFromFile.Comparator1);
            Assert.Equal(s.ReferenceValue1, deseralizedObjFromFile.ReferenceValue1);
            Assert.Equal(s.Comparator2, deseralizedObjFromFile.Comparator2);
            Assert.Equal(s.ReferenceValue2, deseralizedObjFromFile.ReferenceValue2);
        }

        private static EnumConverterOutputType GetEnumConverterOutputTypeFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => EnumConverterOutputType.Raw,
                2 => EnumConverterOutputType.Image,
                3 => EnumConverterOutputType.ImageOverlay,
                4 => EnumConverterOutputType.NotSet,
                _ => EnumConverterOutputType.Image
            };
        }
        private static EnumComparator GetEnumComparatorFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => EnumComparator.Equals,
                2 => EnumComparator.NotEquals,
                3 => EnumComparator.LessThan,
                4 => EnumComparator.LessThanEqual,
                5 => EnumComparator.GreaterThan,
                6 => EnumComparator.GreaterThanEqual,
                7 => EnumComparator.Always,
                8 => EnumComparator.NotSet,
                _ => EnumComparator.Equals
            };
        }

        public static DCSBIOSConverter GetObject(int instanceNbr = 1) {
            return new()
            {
                FaceTypeText = FaceTypeText_SerializeTests.GetObject(instanceNbr + 1),
                FaceTypeImage = FaceTypeImage_SerializeTests.GetObject(instanceNbr + 2),
                FaceTypeDCSBIOSOverlay = FaceTypeDCSBIOSOverlay_SerializeTests.GetObject(instanceNbr + 3),
                ConverterOutputType = GetEnumConverterOutputTypeFromInstance(instanceNbr),
                Comparator1 = GetEnumComparatorFromInstance(instanceNbr),
                ReferenceValue1 = 888 + instanceNbr,
                Comparator2 = GetEnumComparatorFromInstance(instanceNbr + 1),
                ReferenceValue2 = 1888 + instanceNbr,
            };
        }
    }
}
