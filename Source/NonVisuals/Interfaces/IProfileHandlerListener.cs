namespace NonVisuals.Interfaces
{
    using NonVisuals.EventArgs;

    public interface IProfileHandlerListener
    {
        void ProfileLoaded(object sender, ProfileLoadedEventArgs e);
        void ProfileSelected(object sender, AirframeEventArgs e);
    }
}
