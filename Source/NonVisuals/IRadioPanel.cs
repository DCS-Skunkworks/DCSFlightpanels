namespace NonVisuals
{
    public interface IRadioPanel
    {
        int FrequencyKnobSensitivity { get; set; }
        int SynchSleepTime { get; set; }
        long ResetSyncTimeout { get; set; }
    }
}
