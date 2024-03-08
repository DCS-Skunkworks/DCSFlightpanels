using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Panels.Saitek;
using NonVisuals.Panels.Saitek.Panels;
using System.Windows.Forms;
using Xunit;

namespace DCSFPTests.Serialization {

    public static class BIPLight_SerializeTests {

        [Fact]
        public static void BIPLight_ShouldBeSerializable() {
            BIPLight s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            BIPLight d = JsonConvert.DeserializeObject<BIPLight>(serializedObj);

            Assert.Equal(s.LEDColor, d.LEDColor);
            Assert.Equal(s.BIPLedPosition, d.BIPLedPosition);
            DeepAssert.Equal(s.DelayBefore, d.DelayBefore);
            Assert.Equal(s.BindingHash, d.BindingHash);
            Assert.Equal(new string[] { "|" }, d.Separator);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            BIPLight deseralizedObjFromFile = JsonConvert.DeserializeObject<BIPLight>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static PanelLEDColor GetPanelLEDColorFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => PanelLEDColor.DARK,
                2 => PanelLEDColor.GREEN,
                3 => PanelLEDColor.YELLOW,
                4 => PanelLEDColor.RED,
                _ => PanelLEDColor.GREEN
            };
        }

        public static BIPLedPositionEnum GetBIPLedPositionEnumFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
               1 => BIPLedPositionEnum.Position_1_1,
               2 => BIPLedPositionEnum.Position_1_2,
               3 => BIPLedPositionEnum.Position_1_3,
               4 => BIPLedPositionEnum.Position_1_4,
               5 => BIPLedPositionEnum.Position_1_5,
               6 => BIPLedPositionEnum.Position_1_6,
               7 => BIPLedPositionEnum.Position_1_7,
               8 => BIPLedPositionEnum.Position_1_8,
               9 => BIPLedPositionEnum.Position_2_1,
               10 => BIPLedPositionEnum.Position_2_2,
               11 => BIPLedPositionEnum.Position_2_3,
               12 => BIPLedPositionEnum.Position_2_4,
               13 => BIPLedPositionEnum.Position_2_5,
               14 => BIPLedPositionEnum.Position_2_6,
               15 => BIPLedPositionEnum.Position_2_7,
               16 => BIPLedPositionEnum.Position_2_8,
               17 => BIPLedPositionEnum.Position_3_1,
               18 => BIPLedPositionEnum.Position_3_2,
               19 => BIPLedPositionEnum.Position_3_3,
               20 => BIPLedPositionEnum.Position_3_4,
               21 => BIPLedPositionEnum.Position_3_5,
               22 => BIPLedPositionEnum.Position_3_6,
               23 => BIPLedPositionEnum.Position_3_7,
               24 => BIPLedPositionEnum.Position_3_8,
               _ => BIPLedPositionEnum.Position_1_1
            };
        }

        public static BIPLightDelays GetBIPLightDelaysFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => BIPLightDelays.Zeroms,
                2 => BIPLightDelays.Fiftyms,
                3 => BIPLightDelays.Hundredms,
                4 => BIPLightDelays.TwoHundredms,
                5 => BIPLightDelays.ThreeHundredms,
                6 => BIPLightDelays.ForHundredms,
                7 => BIPLightDelays.FiveHundredms,
                8 => BIPLightDelays.OneSec,
                9 => BIPLightDelays.OneAndHalfSec,
                10 => BIPLightDelays.TwoSec,
                11 => BIPLightDelays.ThreeSec,
                12 => BIPLightDelays.FourSec,
                13 => BIPLightDelays.FiveSec,
                14 => BIPLightDelays.SixSec,
                15 => BIPLightDelays.SevenSec,
                16 => BIPLightDelays.EightSec,
                17 => BIPLightDelays.NineSec,
                18 => BIPLightDelays.TenSec,
                _  => BIPLightDelays.ForHundredms
            };
        }

        public static BIPLight GetObject(int instanceNbr = 1) {
            return new()
            {
                LEDColor = GetPanelLEDColorFromInstance(instanceNbr),
                BIPLedPosition = GetBIPLedPositionEnumFromInstance(instanceNbr),
                DelayBefore = GetBIPLightDelaysFromInstance(instanceNbr),
                BindingHash = $"dre tiy {instanceNbr}"
            };
        }
    }
}
