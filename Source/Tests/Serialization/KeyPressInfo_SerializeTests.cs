using DCSFPTests.Serialization.Common;
using MEF;
using Newtonsoft.Json;
using NonVisuals.KeyEmulation;

using Xunit;

namespace DCSFPTests.Serialization {

    public static class KeyPressInfo_SerializeTests {

        [Fact]
        public static void KeyPressInfo_ShouldBeSerializable() {
            KeyPressInfo s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            KeyPressInfo d = JsonConvert.DeserializeObject<KeyPressInfo>(serializedObj);

            Assert.True(s.LengthOfBreak == d.LengthOfBreak);
            Assert.True(s.LengthOfKeyPress == d.LengthOfKeyPress);
            DeepAssert.Equal(s.VirtualKeyCodes, d.VirtualKeyCodes);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            KeyPressInfo deseralizedObjFromFile = JsonConvert.DeserializeObject<KeyPressInfo>(repo.GetSerializedObjectString(d.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        private static KeyPressLength GetKeyPressLengthFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => KeyPressLength.ThirtyTwoMilliSec,
                2 => KeyPressLength.HalfSecond,
                3 => KeyPressLength.TwoSeconds,
                4 => KeyPressLength.FiveSecs,
                5 => KeyPressLength.FiftyMilliSec,
                _ => KeyPressLength.SecondAndHalf
            };
        }

        private static VirtualKeyCode GetVirtualKeyCodeFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => VirtualKeyCode.F6,
                2 => VirtualKeyCode.VK_V,
                3 => VirtualKeyCode.LCONTROL,
                4 => VirtualKeyCode.SPACE,
                5 => VirtualKeyCode.DELETE,
                6 => VirtualKeyCode.F1,
                7 => VirtualKeyCode.VK_S,
                8 => VirtualKeyCode.VK_L,
                9 => VirtualKeyCode.END,
                _ => VirtualKeyCode.VK_T
            };
        }

        public static KeyPressInfo GetObject(int instanceNbr = 1) {
            return new()
            {
                LengthOfBreak = GetKeyPressLengthFromInstance(instanceNbr),
                LengthOfKeyPress = GetKeyPressLengthFromInstance(instanceNbr+1),
                VirtualKeyCodes = new() { 
                  GetVirtualKeyCodeFromInstance(instanceNbr)
                , GetVirtualKeyCodeFromInstance(instanceNbr+1)
                , GetVirtualKeyCodeFromInstance(instanceNbr+2)
                , GetVirtualKeyCodeFromInstance(instanceNbr+3)
                }
            };
        }
    }
}
