namespace NonVisuals.EventArgs
{
    using ClassLibraryCommon;

    public class DCSBIOSUpdatesMissedEventArgs : System.EventArgs
    {
        public string HidInstance { get; set; }

        public GamingPanelEnum GamingPanelEnum { get; init; }

        public int Count { get; init; }
    }
}
