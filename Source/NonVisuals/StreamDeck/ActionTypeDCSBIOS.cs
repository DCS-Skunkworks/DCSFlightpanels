using System;
using System.IO;
using System.Threading;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{

    [Serializable]
    public class ActionTypeDCSBIOS : DCSBIOSActionBindingBase, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.DCSBIOS;
        public bool IsRepeatable() => true;

        private EnumStreamDeckButtonNames _streamDeckButtonName;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;


        public ActionTypeDCSBIOS(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
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
                    result = (result * 397) ^ dcsbiosInput.GetHashCode();
                }
                return result;
            }
        }

        public void Execute(CancellationToken threadCancellationToken)
        {
            Common.PlaySoundFile(false, SoundFile, Volume);
            SendDCSBIOSCommands(threadCancellationToken);
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
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


        public string SoundFile { get; set; }
        public double Volume { get; set; }
        public int Delay { get; set; }
        public bool HasSound => string.IsNullOrEmpty(SoundFile) && File.Exists(SoundFile);
        public void PlaySound()
        {
            Common.PlaySoundFile(false, SoundFile, Volume);
        }
    }
}
