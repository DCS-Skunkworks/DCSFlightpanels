using DCS_BIOS;
using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class DCSBIOSOutput_SerializeTests {

        [Fact]
        public static void DCSBIOSOutput_ShouldBeSerializable() {
            DCSBIOSOutput s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSOutput d = JsonConvert.DeserializeObject<DCSBIOSOutput>(serializedObj);

            Assert.True(s.ControlId == d.ControlId);
            Assert.True(s.Address == d.Address);
            Assert.True(s.Mask == d.Mask);
            Assert.True(s.ShiftValue == d.ShiftValue);
            Assert.True(s.DCSBiosOutputType == d.DCSBiosOutputType);
            Assert.True(s.DCSBiosOutputComparison == d.DCSBiosOutputComparison);
            Assert.True(s.ControlDescription == d.ControlDescription);
            Assert.True(s.MaxValue == d.MaxValue);
            Assert.True(s.MaxLength == d.MaxLength);
            Assert.True(s.ControlType == d.ControlType);

            //should be not serialized : 
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

            DCSBIOSOutput deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSOutput>(repo.GetSerializedObjectString(d.GetType()));

            //Should be nice to test the object 's' with 'deseralizedObjFromFile' but since serialization/ deserialization is asymetric we will use the 'd' object 
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        private static DCSBIOSOutput GetObject(int instanceNbr = 1) {
            return new()
            {
                ControlId = $"CtrlId 752{instanceNbr}",
                Address = 1,
                Mask = 2,
                ShiftValue = 3,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                DCSBiosOutputComparison = DCSBiosOutputComparison.BiggerThan,
                ControlDescription = $"xsh lkj {instanceNbr}",
                MaxValue = 4,
                MaxLength = 5,
                ControlType = "type",

                //should be not serialized: 
                LastUIntValue = 666,
                LastStringValue = "ShouldNotBeSerialized",
                SpecifiedValueUInt = 777
            };
        }
    }
}
