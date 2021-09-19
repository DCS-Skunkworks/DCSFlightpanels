namespace NonVisuals.StreamDeck
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using NonVisuals.Interfaces;

    [Serializable]
    public class ActionTypeOS : OSCommandBinding, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.OSCommand;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;


        public ActionTypeOS(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }
        
        public new int GetHash()
        {
            unchecked
            {
                var result = OSCommandObject.GetHash();
                result = (result * 397) ^ _streamDeckButtonName.GetHashCode();
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
                    stringBuilder.Append(" ").Append(OSCommandObject.Name);
                }

                return stringBuilder.ToString();
            }
        }

        public bool IsRunning()
        {
            return OSCommandObject.IsRunning();
        }


        public void Execute(CancellationToken threadCancellationToken)
        {
            Common.PlaySoundFile(false, SoundFile, Volume);
            OSCommandObject.Execute(threadCancellationToken);
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
