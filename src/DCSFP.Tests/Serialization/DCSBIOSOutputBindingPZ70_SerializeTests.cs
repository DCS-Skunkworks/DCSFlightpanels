using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class DCSBIOSOutputBindingPZ70_SerializeTests {
        [Fact]
        public static void DCSBIOSOutputBindingPZ70_ShouldBeSerializable() {
            DCSBIOSOutputBindingPZ70 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSOutputBindingPZ70 d = JsonConvert.DeserializeObject<DCSBIOSOutputBindingPZ70>(serializedObj);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.PZ70LCDPosition, d.PZ70LCDPosition);
            Assert.Equal(s.HasBinding, d.HasBinding);
            DeepAssert.Equal(s.DCSBIOSOutputFormulaObject, d.DCSBIOSOutputFormulaObject);
            Assert.Null(s.DCSBIOSOutputObject);
            Assert.Null(d.DCSBIOSOutputObject);
            
            //state dependant:
            //        CurrentValueAsString

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSOutputBindingPZ70 deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSOutputBindingPZ70>(repo.GetSerializedObjectString(s.GetType()));

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.PZ70LCDPosition, deseralizedObjFromFile.PZ70LCDPosition);
            Assert.Equal(s.HasBinding, deseralizedObjFromFile.HasBinding);
            DeepAssert.Equal(s.DCSBIOSOutputFormulaObject, d.DCSBIOSOutputFormulaObject);
            Assert.Null(deseralizedObjFromFile.DCSBIOSOutputObject);
        }

        public static PZ70LCDPosition GetPZ70LCDPositionFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => PZ70LCDPosition.UpperLCD,
                2 => PZ70LCDPosition.LowerLCD,
                _ => PZ70LCDPosition.UpperLCD
            };
        }

        public static DCSBIOSOutputBindingPZ70 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ70_SerializeTests.GetPZ70DialPositionFromInstance(instanceNbr),
                PZ70LCDPosition = GetPZ70LCDPositionFromInstance(instanceNbr),
                LimitDecimalPlaces = true,
                NumberFormatInfoDecimals = new System.Globalization.NumberFormatInfo()
                {
                    NumberDecimalSeparator = ".",
                    NumberDecimalDigits = 2
                },
                CurrentValue = 543.21,
                //DCSBIOSOutputObject = DCSBIOSOutput_SerializeTests.GetObject(instanceNbr + 1),
                //Can't test DCSBIOSOutputObject & DCSBIOSOutputFormulaObject at the same time
                DCSBIOSOutputFormulaObject = DCSBIOSOutputFormula_SerializeTests.GetObject(instanceNbr+2),
            };
        }
    }
}
