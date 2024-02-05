using System;
using Newtonsoft.Json;

namespace NonVisuals.Radios.SRS
{
    public class SRSRadioReceivingState
    {
        [JsonIgnore]
        public long LastReceivedAt { get; set; }

        public bool IsSecondary { get; set; }
        public bool IsSimultaneous { get; set; }
        public int ReceivedOn { get; set; }

        public string SentBy { get; set; }

        public bool IsReceiving
        {
            get
            {
                return (DateTime.Now.Ticks - LastReceivedAt) < 3500000;
            }
        }
    }
}
