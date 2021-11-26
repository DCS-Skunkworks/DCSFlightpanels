namespace NonVisuals.Interfaces
{
    using NonVisuals.EventArgs;

    public interface IProfileHandlerListener
    {
        void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e);
        void ProfileSelected(object sender, AirframeEventArgs e);
    }
}
