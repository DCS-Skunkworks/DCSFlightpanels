namespace NonVisuals.EventArgs
{
    using EventArgs = System.EventArgs;

    public class ProfileLoadedEventArgs : EventArgs
    {
        public GenericPanelBinding PanelBinding { get; set; }
    }
}
