using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.StreamDeck
{
    using MEF;

    using NonVisuals.Plugin;

    [Serializable]
    public class ActionTypeKey : KeyBinding, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.KeyPress;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;


        public ActionTypeKey(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }


        public new int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                result = (result * 397) ^ base.GetHash();
                return result;
            }
        }

        [JsonIgnore]
        public string ActionDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Key press");
                if (OSKeyPress != null)
                {
                    stringBuilder.Append(" ").Append(OSKeyPress.GetKeyPressInformation());
                }

                return stringBuilder.ToString();
            }
        }


        public bool IsRunning()
        {
            return OSKeyPress.IsRunning();
        }

        public void Execute(CancellationToken threadCancellationToken)
        {
            Common.PlaySoundFile(false, SoundFile, Volume);

            if (!PluginManager.DisableKeyboardAPI)
            {
                OSKeyPress.Execute(threadCancellationToken);
            }

            if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
            {
                PluginManager.Get().PanelEventHandler.PanelEvent(
                    ProfileHandler.SelectedProfile().Description,
                    StreamDeckPanelInstance.HIDInstanceId,
                    (int)StreamDeckCommon.ConvertEnum(_streamDeckPanel.TypeOfPanel),
                    (int)StreamDeckButtonName,
                    true,
                    OSKeyPress.KeySequence);
            }
        }


        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public static HashSet<ActionTypeKey> SetNegators(HashSet<ActionTypeKey> keyBindings)
        {
            return keyBindings;
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set => _streamDeckPanel = value;
        }

        [JsonProperty("SoundFile", Required = Required.Default)]
        public string SoundFile { get; set; }

        [JsonProperty("Volume", Required = Required.Default)]
        public double Volume { get; set; }

        [JsonProperty("Delay", Required = Required.Default)]
        public int Delay { get; set; }

        [Obsolete]
        [JsonProperty("HasSound", Required = Required.Default)]
        public bool HasSound => !string.IsNullOrEmpty(SoundFile) && File.Exists(SoundFile);


        public void PlaySound()
        {
            Common.PlaySoundFile(false, SoundFile, Volume);
        }
    }
}
