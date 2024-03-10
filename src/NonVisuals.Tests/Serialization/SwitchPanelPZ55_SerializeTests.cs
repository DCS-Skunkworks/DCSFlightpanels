using System.Linq;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.HID;
using NonVisuals.Panels.Saitek.Panels;
using NonVisuals.Tests.Serialization.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization.Panels {
    public class SwitchPanelPZ55_SerializeTests {
        [Fact]
        public static void SwitchPanelPZ55_ShouldBeSerializable() {
            SwitchPanelPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
           // SwitchPanelPZ55 d = JsonConvert.DeserializeObject<SwitchPanelPZ55>(serializedObj);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            //SwitchPanelPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<SwitchPanelPZ55>(repo.GetSerializedObjectString(s.GetType()));

           // DeepAssert.Equal(s, deseralizedObjFromFile);
           // DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        public static SwitchPanelPZ55 GetObject(int instanceNbr = 1) {
            GamingPanelSkeleton gamingPanelSkeleton = new(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel);
            return new SwitchPanelPZ55(new HIDSkeleton(gamingPanelSkeleton, "FakeHidInstanceForTests"))
            {
                ManualLandingGearLEDs = false,
                ManualLandingGearLEDsColorDown = BIPLight_SerializeTests.GetPanelLEDColorFromInstance(instanceNbr),
                ManualLandingGearLEDsColorUp = BIPLight_SerializeTests.GetPanelLEDColorFromInstance(instanceNbr + 1),
                ManualLandingGearLEDsColorTrans = BIPLight_SerializeTests.GetPanelLEDColorFromInstance(instanceNbr + 2),
                ManualLandingGearTransTimeSeconds = instanceNbr + 6,
                
                DCSBiosBindings = DCSBIOSActionBindingPZ55_SerializeTests.GetObjects(),
                KeyBindingsHashSet = KeyBindingPZ55_SerializeTests.GetObjects(),
                OSCommandList = OSCommandBindingPZ55_SerializeTests.GetObjects().ToList(),
                BIPLinkHashSet = BIPLinkPZ55_SerializeTests.GetObjects()
            };
        }
    }
}
