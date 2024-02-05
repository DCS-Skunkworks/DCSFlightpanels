namespace NonVisuals.Radios.SRS
{
//Class copied from SRS source to better understand serialization
#pragma warning disable CS0649 // Field xxx is never assigned to, and will always have its default value null
    internal class SRSCombinedRadioState
    {
        public SRSPlayerRadioInfo RadioInfo;

        public SRSRadioSendingState RadioSendingState;

        public SRSRadioReceivingState[] RadioReceivingState;

        public int ClientCountConnected;

        public int[] TunedClients;
    }
#pragma warning restore CS0649 // Field xxx is never assigned to, and will always have its default value null
}
