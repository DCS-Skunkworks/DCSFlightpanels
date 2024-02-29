using DCS_BIOS.Serialized;
using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using Xunit;

namespace DCSFPTests.Serialization
{

    public static class DCSBIOSOutput_SerializeTests {

        [Fact]
        public static void DCSBIOSOutput_ShouldBeSerializable() {
            DCSBIOSOutput s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSOutput d = JsonConvert.DeserializeObject<DCSBIOSOutput>(serializedObj);

            Assert.Equal(s.ControlId, d.ControlId);
            Assert.Equal(s.Address, d.Address);
            Assert.Equal(s.Mask, d.Mask);
            Assert.Equal(s.ShiftValue, d.ShiftValue);
            Assert.Equal(s.DCSBiosOutputType, d.DCSBiosOutputType);
            Assert.Equal(s.DCSBiosOutputComparison, d.DCSBiosOutputComparison);
            Assert.Equal(s.ControlDescription, d.ControlDescription);
            Assert.Equal(s.MaxValue, d.MaxValue);
            Assert.Equal(s.MaxLength, d.MaxLength);
            //Assert.Equal(s.ControlType, d.ControlType);

            //Not serialized : 
            Assert.False(s.LastUIntValue == d.LastUIntValue);
            Assert.False(s.LastStringValue == d.LastStringValue);
            Assert.False(s.SpecifiedValueUInt == d.SpecifiedValueUInt);

            //default values test
            Assert.Equal(uint.MaxValue, d.LastUIntValue);
            Assert.Equal((uint)0, d.SpecifiedValueUInt);
            Assert.Equal(string.Empty, d.LastStringValue);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSOutput deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSOutput>(repo.GetSerializedObjectString(s.GetType()));

            //Should be nice to test the object 's' with 'deseralizedObjFromFile' but since serialization/ deserialization is asymetric we will use the 'd' object 
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static DCSBIOSOutput GetObject(int instanceNbr = 1) {
            return new()
            {
                ControlId = $"qqw ggs {instanceNbr}",
                Address = 1,
                Mask = 2,
                ShiftValue = 3,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                DCSBiosOutputComparison = DCSBiosOutputComparison.BiggerThan,
                ControlDescription = $"xsh lkj {instanceNbr}",
                MaxValue = 4,
                MaxLength = 5,
                //ControlType = $"vvt ikj {instanceNbr}",

                //Not serialized : 
                LastUIntValue = 666,
                LastStringValue = "ShouldNotBeSerialized",
                SpecifiedValueUInt = 777
            };
        }
    }
}
