namespace NonVisuals.EventArgs
{
    using System;

    using ClassLibraryCommon;

    public class PanelEventArgs : EventArgs
    {
        public string HidInstance { get; set; }

        public GamingPanelEnum PanelType { get; set; }

        public PanelEventType EventType { get; set; }

        public string OldHidInstance { get; set; }
    }

    public enum PanelEventType
    {
        Attached,
        Detached,
        Found,
        Created,
        Disposed,
        AllPanelsFound
    }
}
