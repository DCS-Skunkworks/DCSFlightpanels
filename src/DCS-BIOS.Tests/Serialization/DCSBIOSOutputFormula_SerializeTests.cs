using DCS_BIOS.Serialized;
using Newtonsoft.Json;
using DCS_BIOS.Tests.Serialization.Common;
using Xunit;
using Tests.Common;

namespace DCS_BIOS.Tests.Serialization {

    /// <summary>
    /// Note : Can't set 'Formula' because on how the set property calls to 'ExtractDCSBIOSOutputsInFormula' 
    /// and ultimately call to 'LoadControls' with 'IsNoFrameLoadedYet' with null DcsAirframe loaded.
    /// </summary>
    public static class DCSBIOSOutputFormula_SerializeTests {
        
        [Fact]
        public static void DCSBIOSOutputFormula_ShouldBeSerializable() {
            DCSBIOSOutputFormula s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSOutputFormula d = JsonConvert.DeserializeObject<DCSBIOSOutputFormula>(serializedObj);

            Assert.Equal(s.FormulaResult, d.FormulaResult);
            Assert.Equal(s.Formula, d.Formula);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSOutputFormula deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSOutputFormula>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static DCSBIOSOutputFormula GetObject(int instanceNbr = 1) {
            return new()
            {
                // Formula = $"Formula528 {instanceNbr}", //can't do that, see notes
                FormulaResult = 789 + instanceNbr,
            };
        }
    }
}
