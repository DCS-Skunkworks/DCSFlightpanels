using NonVisuals.BindingClasses.Key;
using NonVisuals.StreamDeck.Panels;

namespace NonVisuals.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using Interfaces;
    using Plugin;

    [Serializable]
    public class ActionTypeKey : KeyBindingBase, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
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


        public virtual void Dispose(bool disposing) { }
        public virtual void Dispose() { }

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
                    stringBuilder.Append(' ').Append(OSKeyPress.GetKeyPressInformation());
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
            Common.PlaySoundFile(SoundFile, Volume);

            if (!PluginManager.DisableKeyboardAPI)
            {
                OSKeyPress.Execute(threadCancellationToken);
            }

            if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
            {
                PluginManager.DoEvent(
                    DCSFPProfile.SelectedProfile.Description,
                    StreamDeckPanelInstance.HIDInstance,
                    StreamDeckCommon.ConvertEnum(_streamDeckPanel.TypeOfPanel),
                    (int)StreamDeckButtonName,
                    true,
                    OSKeyPress.KeyPressSequence);
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
            Common.PlaySoundFile(SoundFile, Volume);
        }
    }
}
