using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Panels.StreamDeck;
using NonVisuals.Tests.Serialization.Common;
using Tests.Common;
using Xunit;

namespace NonVisuals.Tests.Serialization {
    public class ActionTypeLayer_SerializeTests {
        [Fact]
        public static void ActionTypeLayer_ShouldBeSerializable() {
            ActionTypeLayer s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            ActionTypeLayer d = JsonConvert.DeserializeObject<ActionTypeLayer>(serializedObj);

            Assert.Equal(EnumStreamDeckActionType.LayerNavigation, s.ActionType);
            Assert.Equal(s.ActionType, d.ActionType);
            Assert.Equal(s.NavigationType, d.NavigationType);
            Assert.Equal(s.TargetLayer, d.TargetLayer);
            Assert.Equal(s.RemoteStreamdeckBindingHash, d.RemoteStreamdeckBindingHash);
            Assert.Equal(s.RemoteStreamdeckTargetLayer, d.RemoteStreamdeckTargetLayer);
            Assert.Equal(s.StreamDeckButtonName, d.StreamDeckButtonName);
            Assert.Equal(s.StreamDeckPushRotaryName, d.StreamDeckPushRotaryName);
            Assert.Equal(s.SoundFile, d.SoundFile);
            Assert.Equal(s.Volume, d.Volume);
            Assert.Equal(s.Delay, d.Delay);

            //not serialized:
            //ActionDescription
            //ControlsRemoteStreamDeck
            //StreamDeckPanelInstance

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            ActionTypeLayer deseralizedObjFromFile = JsonConvert.DeserializeObject<ActionTypeLayer>(repo.GetSerializedObjectString(s.GetType()));

            DeepAssert.Equal(s, deseralizedObjFromFile);
            DeepAssert.Equal(d, deseralizedObjFromFile);
        }

        private static LayerNavType GetLayerNavTypeFromInstance(int instanceNbr) {
            return instanceNbr switch
            {
                1 => LayerNavType.None,
                2 => LayerNavType.SwitchToSpecificLayer,
                3 => LayerNavType.Back,
                _ => LayerNavType.Home
            };
        }

        public static ActionTypeLayer GetObject(int instanceNbr = 1) {
            return new()
            {
                NavigationType = GetLayerNavTypeFromInstance(instanceNbr),
                TargetLayer = $"ggy ool {instanceNbr}",
                RemoteStreamdeckBindingHash = $"ttr efb {instanceNbr}",
                RemoteStreamdeckTargetLayer = $"kjq cyn {instanceNbr}",
                StreamDeckButtonName = StreamDeckButton_SerializeTests.GetStreamDeckButtonNameFromInstance(instanceNbr),
                StreamDeckPushRotaryName = StreamDeckPushRotary_SerializeTests.GetStreamDeckPushRotaryNameFromInstance(instanceNbr),
                SoundFile = $"xdl ero {instanceNbr}",
                Volume = 199 + instanceNbr,
                Delay = 141 + instanceNbr
            };
        }
    }
}
