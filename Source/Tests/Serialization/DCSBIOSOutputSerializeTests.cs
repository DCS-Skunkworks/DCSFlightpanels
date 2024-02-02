using DCS_BIOS;
using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;
using Xunit;

namespace DCSFPTests.Serialization {
    public class DCSBIOSOutputSerializeTests {
        private JsonSerializerSettings _jsonSettings = new()
        {
            ContractResolver = new ExcludeObsoletePropertiesResolver(),
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) => {
                throw new System.Exception($"JSON Serialization Error.{args.ErrorContext.Error.Message}");
            }
        };

        [Fact]
        public void DCSBIOSOutput_ShouldBeSeriazable() {
            DCSBIOSOutput s = new()
            {
                ControlId = "123",
                Address = 1,
                Mask = 2,
                ShiftValue = 3,
                DCSBiosOutputType = DCSBiosOutputType.IntegerType,
                DCSBiosOutputComparison = DCSBiosOutputComparison.BiggerThan,
                ControlDescription = "abc",
                MaxValue = 4,
                MaxLength = 5,
                ControlType = "type",

                //should be not serialized: 
                LastUIntValue = 666,
                LastStringValue = "ShouldNotBeSerialized",
                SpecifiedValueUInt = 777
            };

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, _jsonSettings);
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
            repo.SaveSerializedObjectToFile(d.GetType(), serializedObj);

            DCSBIOSOutput deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSOutput>(repo.GetSerializedObjectString(d.GetType()));

            //Should be nice to test the object 's' with 'deseralizedObjFromFile' but since serialization/ deserialization is asymetric we will use the 'd' object 
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }
    }
}
