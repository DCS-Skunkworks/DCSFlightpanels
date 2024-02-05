using NonVisuals.BindingClasses.OSCommand;

namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using Interfaces;
    using Panels;

    [Serializable]
    [SerializeCritical]
    public class ActionTypeOS : OSCommandBindingBase, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.OSCommand;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private EnumStreamDeckPushRotaryNames _streamDeckPushRotaryName;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;

        public ActionTypeOS() {
        }

        public ActionTypeOS(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        public virtual void Dispose(bool disposing) { }
        public virtual void Dispose() { }

        public new int GetHash()
        {
            unchecked
            {
                var result = OSCommandObject.GetHash();
                result = result * 397 ^ _streamDeckButtonName.GetHashCode();
                return result;
            }
        }

        [JsonIgnore]
        public string ActionDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("OS Command");
                if (OSCommandObject != null)
                {
                    stringBuilder.Append(' ').Append(OSCommandObject.Name);
                }

                return stringBuilder.ToString();
            }
        }

        public bool IsRunning()
        {
            return OSCommandObject.IsRunning();
        }


        public void Execute(CancellationToken threadCancellationToken, bool executeOnce = false)
        {
            Common.PlaySoundFile(SoundFile, Volume);
            OSCommandObject.ExecuteCommand(threadCancellationToken);
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }

        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        [JsonProperty("StreamDeckButtonPushRotaryName", Required = Required.Default)]
        public EnumStreamDeckPushRotaryNames StreamDeckPushRotaryName
        {
            get => _streamDeckPushRotaryName;
            set => _streamDeckPushRotaryName = value;
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
