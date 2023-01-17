using Newtonsoft.Json;

namespace NonVisuals.Radios.SRS
{
    public class SRSRadioSendingState
    {
        [JsonIgnore]
        public long LastSentAt { get; set; }

        public bool IsSending { get; set; }

        public int SendingOn { get; set; }

        public int IsEncrypted { get; set; }
    }
}
