namespace NonVisuals.Interfaces
{
    using NonVisuals.EventArgs;

    public interface IProfileHandlerListener
    {
        void PanelSettingsChanged(object sender, PanelEventArgs e);

        void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e);

        void SelectedProfile(object sender, AirframeEventArgs e);
    }
}
