using System.Collections.Generic;
using DCSFP.Tests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.OSCommand;
using Xunit;

namespace DCSFP.Tests.Serialization {
    public class OSCommandBindingPZ55_SerializeTests {
        [Fact]
        public static void OSCommandBindingPZ55_ShouldBeSerializable() {
            OSCommandBindingPZ55 s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommandBindingPZ55 d = JsonConvert.DeserializeObject<OSCommandBindingPZ55>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, d.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommandBindingPZ55 deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommandBindingPZ55>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.SwitchPanelPZ55Key, deseralizedObjFromFile.SwitchPanelPZ55Key);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, deseralizedObjFromFile.OSCommandObject);
        }

        public static HashSet<OSCommandBindingPZ55> GetObjects() {
            HashSet<OSCommandBindingPZ55> hashSet = new();
            for (int i = 0; i < 3; i++) {
                hashSet.Add(GetObject(i));
            }
            return hashSet;
        }

        private static OSCommandBindingPZ55 GetObject(int instanceNbr = 1) {
            return new()
            {
                SwitchPanelPZ55Key = KeyBindingPZ55_SerializeTests.GetSwitchPanelPZ55KeysFromInstance(instanceNbr+4),
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr+5)
            };
        }
    }
}
