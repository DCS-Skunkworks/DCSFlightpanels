using System;
using Newtonsoft.Json;

namespace NonVisuals.Radios.SRS
{
    public class SRSRadioReceivingState
    {
        [JsonIgnore]
        public long LastReceviedAt { get; set; }

        public bool IsSecondary { get; set; }
        public bool IsSimultaneous { get; set; }
        public int ReceivedOn { get; set; }

        public string SentBy { get; set; }

        public bool IsReceiving
        {
            get
            {
                return (DateTime.Now.Ticks - LastReceviedAt) < 3500000;
            }
        }
    }
}
