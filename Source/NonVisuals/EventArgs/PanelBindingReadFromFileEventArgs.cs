namespace NonVisuals.EventArgs
{
    using EventArgs = System.EventArgs;

    public class PanelBindingReadFromFileEventArgs : EventArgs
    {
        public GenericPanelBinding PanelBinding { get; set; }
    }
}
