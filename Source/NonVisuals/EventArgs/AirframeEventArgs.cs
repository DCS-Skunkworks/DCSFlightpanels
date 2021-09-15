namespace NonVisuals.EventArgs
{
    using ClassLibraryCommon;

    public class AirframeEventArgs : System.EventArgs
    {
        public DCSFPProfile Profile { get; set; }
    }
}
