namespace NonVisuals.Radios.SRS
{
    internal class SRSCombinedRadioState
    {
        public SRSPlayerRadioInfo RadioInfo;

        public SRSRadioSendingState RadioSendingState;

        public SRSRadioReceivingState[] RadioReceivingState;

        public int ClientCountConnected;

        public int[] TunedClients;
    }
}
