using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using Xunit;

namespace DCSFPTests.Serialization {
    public class ActionTypeDCSBIOS_SerializeTests {
        [Fact]
        public static void ActionTypeDCSBIOS_ShouldBeSerializable() {
            ActionTypeDCSBIOS s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            ActionTypeDCSBIOS d = JsonConvert.DeserializeObject<ActionTypeDCSBIOS>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(EnumStreamDeckActionType.DCSBIOS, s.ActionType);
            Assert.Equal(s.ActionType, d.ActionType);
            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.StreamDeckPushRotaryName, d.StreamDeckPushRotaryName);
            Assert.Equal(s.SoundFile, d.SoundFile);
            Assert.Equal(s.Volume, d.Volume);
            Assert.Equal(s.Delay, d.Delay);
            Assert.Equal(s.Delay, d.Delay);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            Assert.Equal(s.IsSequenced, d.IsSequenced);
            Assert.Equal(s.Description, d.Description);
            DeepAssert.Equal(s.DCSBIOSInputs, d.DCSBIOSInputs);

            //not serialized : 
            Assert.Null(d.StreamDeckPanelInstance);

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            ActionTypeDCSBIOS deseralizedObjFromFile = JsonConvert.DeserializeObject<ActionTypeDCSBIOS>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
            Assert.Null(deseralizedObjFromFile.StreamDeckPanelInstance);
        }

        public static ActionTypeDCSBIOS GetObject(int instanceNbr = 1) {
            return new()
            {
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                StreamDeckPushRotaryName = StreamDeckPushRotary_SerializeTests.GetStreamDeckPushRotaryNameFromInstance(instanceNbr),
                SoundFile = $"ccv vvc {instanceNbr}",
                Volume = 125 + instanceNbr,
                Delay = 555 + instanceNbr,
                WhenTurnedOn = true,
                IsSequenced = true,
                Description = $"jjf llv {instanceNbr}",
                //WhenOnTurnedOn = true, //protected ?!
                DCSBIOSInputs = new(){
                     DCSBIOSInput_SerializeTests.GetObject(instanceNbr),
                     DCSBIOSInput_SerializeTests.GetObject(instanceNbr)
                }
            };
        }
    }
}
