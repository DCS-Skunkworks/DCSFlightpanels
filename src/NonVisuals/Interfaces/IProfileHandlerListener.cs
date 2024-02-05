namespace NonVisuals.Interfaces
{
    using EventArgs;

    public interface IProfileHandlerListener
    {
        void ProfileEvent(object sender, ProfileEventArgs e);
    }
}
