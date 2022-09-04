namespace NonVisuals.Interfaces
{
    public interface ICockpitMasterCDUKey
    {
        int Group { get; set; }
        int Mask { get; set; }
        bool IsOn { get; set; }
    }
}
