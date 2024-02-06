using NonVisuals.BindingClasses.DCSBIOSBindings;

namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.IO;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;
    using Interfaces;
    using Panels;

    [Serializable]
    [SerializeCritical]
    public class ActionTypeDCSBIOS : DCSBIOSActionBindingBase, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.DCSBIOS;
        public bool IsRepeatable() => true;

        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private EnumStreamDeckPushRotaryNames _streamDeckPushRotaryName;

        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;

        public ActionTypeDCSBIOS() {
        }

        public ActionTypeDCSBIOS(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.

        public override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public new void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
        }

        [JsonIgnore]
        public string ActionDescription => Description;


        public int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                foreach (var dcsbiosInput in DCSBIOSInputs)
                {
                    result = result * 397 ^ dcsbiosInput.GetHashCode();
                }

                return result;
            }
        }

        public void Execute(CancellationToken threadCancellationToken, bool executeOnce = false)
        {
            Common.PlaySoundFile(SoundFile, Volume);
            SendDCSBIOSCommands(threadCancellationToken);
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
