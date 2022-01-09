namespace NonVisuals.Interfaces
{
    using NonVisuals.EventArgs;

    public interface IProfileHandlerListener
    {
        void ProfileEvent(object sender, ProfileEventArgs e);
    }
}
