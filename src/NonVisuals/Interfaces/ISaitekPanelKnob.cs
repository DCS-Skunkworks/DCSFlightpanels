namespace NonVisuals.Interfaces
{
    public interface ISaitekPanelKnob
    {
        int Group { get; set; }
        int Mask { get; set; }
        bool IsOn { get; set; }
    }
}
