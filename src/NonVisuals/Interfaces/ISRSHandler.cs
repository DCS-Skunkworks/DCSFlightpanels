namespace NonVisuals.Interfaces
{
    using NonVisuals.Radios.SRS;

    public interface ISRSHandler
    {
        bool GuardIsOn { get; set; }
        double Frequency { get; set; }
        double Channel { get; set; }
        SRSRadioMode RadioMode { get; set; }
    }
}
