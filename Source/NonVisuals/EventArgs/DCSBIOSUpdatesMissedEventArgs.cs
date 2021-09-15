namespace NonVisuals.EventArgs
{
    using ClassLibraryCommon;

    public class DCSBIOSUpdatesMissedEventArgs : System.EventArgs
    {
        public string HidInstance { get; set; }

        public GamingPanelEnum GamingPanelEnum { get; set; }

        public int Count { get; set; }
    }
}
