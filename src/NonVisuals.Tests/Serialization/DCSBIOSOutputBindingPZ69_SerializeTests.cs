using DCS_BIOS.Tests.Serialization;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.DCSBIOSBindings;
using NonVisuals.Radios;
using NonVisuals.Tests.Serialization.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class DCSBIOSOutputBindingPZ69_SerializeTests {
        
        [Fact]
        public static void DCSBIOSOutputBindingPZ69_ShouldBeSerializable() {
            DCSBIOSOutputBindingPZ69 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            DCSBIOSOutputBindingPZ69 d = JsonConvert.DeserializeObject<DCSBIOSOutputBindingPZ69>(serializedObj);

            Assert.Equal(s.DialPosition, d.DialPosition);
            Assert.Equal(s.PZ69LcdPosition, d.PZ69LcdPosition);
            Assert.Equal(s.HasBinding, d.HasBinding);
            Assert.Equal(s.DCSBIOSOutputObject.ControlId, d.DCSBIOSOutputObject.ControlId);
            Assert.Equal(s.DCSBIOSOutputObject.ControlDescription, d.DCSBIOSOutputObject.ControlDescription);
            Assert.Null(s.DCSBIOSOutputFormulaObject);
            Assert.Null(d.DCSBIOSOutputFormulaObject);
            //state dependant:
            //        CurrentValueAsString


            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            DCSBIOSOutputBindingPZ69 deseralizedObjFromFile = JsonConvert.DeserializeObject<DCSBIOSOutputBindingPZ69>(repo.GetSerializedObjectString(s.GetType()));

            Assert.Equal(s.DialPosition, deseralizedObjFromFile.DialPosition);
            Assert.Equal(s.PZ69LcdPosition, deseralizedObjFromFile.PZ69LcdPosition);
            Assert.Equal(s.HasBinding, deseralizedObjFromFile.HasBinding);
            Assert.Equal(s.DCSBIOSOutputObject.ControlId, deseralizedObjFromFile.DCSBIOSOutputObject.ControlId);
            Assert.Equal(s.DCSBIOSOutputObject.ControlDescription, deseralizedObjFromFile.DCSBIOSOutputObject.ControlDescription);
            Assert.Null(deseralizedObjFromFile.DCSBIOSOutputFormulaObject);
        }

        public static PZ69LCDPosition GetPZ69LCDPositionFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => PZ69LCDPosition.UPPER_ACTIVE_LEFT,
                2 => PZ69LCDPosition.UPPER_STBY_RIGHT,
                3 => PZ69LCDPosition.LOWER_ACTIVE_LEFT,
                4 => PZ69LCDPosition.LOWER_STBY_RIGHT,
                _ => PZ69LCDPosition.UPPER_ACTIVE_LEFT
            };
        }

        public static DCSBIOSOutputBindingPZ69 GetObject(int instanceNbr = 1) {
            return new()
            {
                DialPosition = KeyBindingPZ69DialPosition_SerializeTests.GetPZ69DialPositionFromInstance(instanceNbr),
                PZ69LcdPosition = GetPZ69LCDPositionFromInstance(instanceNbr),
                LimitDecimalPlaces = true,
                NumberFormatInfoDecimals = new System.Globalization.NumberFormatInfo()
                {
                    NumberDecimalSeparator = ".",
                    NumberDecimalDigits = 4
                },
                CurrentValue = 123.4567,
                DCSBIOSOutputObject = DCSBIOSOutput_SerializeTests.GetObject(instanceNbr+1),
                //Can't test DCSBIOSOutputObject & DCSBIOSOutputFormulaObject at the same time
                //DCSBIOSOutputFormulaObject = DCSBIOSOutputFormula_SerializeTests.GetObject(instanceNbr+2),
            };
        }
    }
}
