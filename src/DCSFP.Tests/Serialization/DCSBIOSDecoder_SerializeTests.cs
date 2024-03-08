using System.Drawing;
using DCS_BIOS.Serialized;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class DCSBIOSDecoder_SerializeTests {

        [Fact]
        public static void DCSBIOSDecoder_ShouldBeSerializable() {
            DCSBIOSDecoder s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSDecoder d = JsonConvert.DeserializeObject<DCSBIOSDecoder>(serializedObj);

            Assert.Equal(s.UseFormula, d.UseFormula);
            Assert.Equal(s.TreatStringAsNumber, d.TreatStringAsNumber);
            Assert.Equal(s.RawFontColor, d.RawFontColor);
            Assert.Equal(s.RawBackgroundColor, d.RawBackgroundColor);
            Assert.Equal(s.LimitDecimalPlaces, d.LimitDecimalPlaces);
            DeepAssert.Equal(s.RawTextFont, d.RawTextFont);
            DeepAssert.Equal(s.FormulaInstance, d.FormulaInstance);
            DeepAssert.Equal(s.DecoderSourceType, d.DecoderSourceType);
            DeepAssert.Equal(s.DecoderOutputType, d.DecoderOutputType);
            DeepAssert.Equal(s.NumberFormatInfoFormula, d.NumberFormatInfoFormula);
            Assert.Equal(s.DCSBIOSConverters[0].Comparator1, d.DCSBIOSConverters[0].Comparator1);
            Assert.Equal(s.DCSBIOSConverters[1].ReferenceValue1, d.DCSBIOSConverters[1].ReferenceValue1);
            Assert.Equal(s.DCSBIOSConverters[2].DCSBIOSValue, d.DCSBIOSConverters[2].DCSBIOSValue);
            Assert.Equal(s.DCSBIOSConverters[3].FriendlyInfo, d.DCSBIOSConverters[3].FriendlyInfo);
            Assert.Equal(s.DCSBIOSOutput.Address, d.DCSBIOSOutput.Address);
            Assert.Equal(s.DCSBIOSOutput.DCSBiosOutputType, d.DCSBIOSOutput.DCSBiosOutputType);
            Assert.Equal(s.DCSBIOSOutput.MaxLength, d.DCSBIOSOutput.MaxLength);

            //-Gets only :
            // FaceDescription
            // ValueUpdated
            //-Obsolete:
            // Formula
            //- Ignore:
            // ImageFiles
            // HasErrors
            // LastFormulaError
            // FormulaResult
            // IsVisible


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSDecoder deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSDecoder>(repo.GetSerializedObjectString(s.GetType()));
            Assert.Equal(s.UseFormula, deseralizedObjFromFile.UseFormula);
            Assert.Equal(s.TreatStringAsNumber, deseralizedObjFromFile.TreatStringAsNumber);
            Assert.Equal(s.RawFontColor, deseralizedObjFromFile.RawFontColor);
            Assert.Equal(s.RawBackgroundColor, deseralizedObjFromFile.RawBackgroundColor);
            Assert.Equal(s.LimitDecimalPlaces, deseralizedObjFromFile.LimitDecimalPlaces);
            DeepAssert.Equal(s.RawTextFont, deseralizedObjFromFile.RawTextFont);
            DeepAssert.Equal(s.FormulaInstance, deseralizedObjFromFile.FormulaInstance);
            DeepAssert.Equal(s.DecoderSourceType, deseralizedObjFromFile.DecoderSourceType);
            DeepAssert.Equal(s.DecoderOutputType, deseralizedObjFromFile.DecoderOutputType);
            DeepAssert.Equal(s.NumberFormatInfoFormula, deseralizedObjFromFile.NumberFormatInfoFormula);
            Assert.Equal(s.DCSBIOSConverters[0].Comparator1, deseralizedObjFromFile.DCSBIOSConverters[0].Comparator1);
            Assert.Equal(s.DCSBIOSConverters[1].ReferenceValue1, deseralizedObjFromFile.DCSBIOSConverters[1].ReferenceValue1);
            Assert.Equal(s.DCSBIOSConverters[2].DCSBIOSValue, deseralizedObjFromFile.DCSBIOSConverters[2].DCSBIOSValue);
            Assert.Equal(s.DCSBIOSConverters[3].FriendlyInfo, deseralizedObjFromFile.DCSBIOSConverters[3].FriendlyInfo);
            Assert.Equal(s.DCSBIOSOutput.Address, deseralizedObjFromFile.DCSBIOSOutput.Address);
            Assert.Equal(s.DCSBIOSOutput.DCSBiosOutputType, deseralizedObjFromFile.DCSBIOSOutput.DCSBiosOutputType);
            Assert.Equal(s.DCSBIOSOutput.MaxLength, deseralizedObjFromFile.DCSBIOSOutput.MaxLength);
        }

        private static DCSBiosOutputType GetDCSBiosOutputTypeFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => DCSBiosOutputType.StringType,
                2 => DCSBiosOutputType.IntegerType,
                3 => DCSBiosOutputType.LED,
                4 => DCSBiosOutputType.ServoOutput,
                5 => DCSBiosOutputType.FloatBuffer,
                6 => DCSBiosOutputType.None,
                _ => DCSBiosOutputType.StringType
            };
        }

        private static EnumDCSBIOSDecoderOutputType GetDCSBIOSDecoderOutputTypeFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => EnumDCSBIOSDecoderOutputType.Raw,
                2 => EnumDCSBIOSDecoderOutputType.Converter,
                _ => EnumDCSBIOSDecoderOutputType.Raw
            };
        }

        private static DCSBIOSDecoder GetObject(int instanceNbr = 1) {
            return new()
            {
                UseFormula = false,
                FormulaInstance = DCSBIOSOutputFormula_SerializeTests.GetObject(instanceNbr),
                DCSBIOSOutput = DCSBIOSOutput_SerializeTests.GetObject(instanceNbr+1),
                DCSBIOSConverters = new() { 
                    DCSBIOSConverter_SerializeTests.GetObject(instanceNbr),
                    DCSBIOSConverter_SerializeTests.GetObject(instanceNbr+1),
                    DCSBIOSConverter_SerializeTests.GetObject(instanceNbr+2),
                    DCSBIOSConverter_SerializeTests.GetObject(instanceNbr+3)
                },
                TreatStringAsNumber = true,
                RawTextFont = new Font(new System.Drawing.FontFamily("Arial"), 15, FontStyle.Bold),
                RawFontColor = System.Drawing.Color.BlanchedAlmond,
                RawBackgroundColor = System.Drawing.Color.Goldenrod,
                DecoderSourceType = GetDCSBiosOutputTypeFromInstance(instanceNbr),
                DecoderOutputType = GetDCSBIOSDecoderOutputTypeFromInstance(instanceNbr),
                DefaultImageFilePath = "ycf lbs",
                LimitDecimalPlaces = true,
                NumberFormatInfoFormula = new()
                {
                    NumberDecimalSeparator = ".",
                    NumberDecimalDigits = 4
                }
            };
        }
    }
}
