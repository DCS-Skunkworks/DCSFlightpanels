namespace NonVisuals.EventArgs
{
    using System;

    using ClassLibraryCommon;

    public class PanelEventArgs : EventArgs
    {
        public string HidInstance { get; set; }

        public HIDSkeleton HidSkeleton { get; set; }
        
        public PanelEventType EventType { get; set; }

        public string OldHidInstance { get; set; }
    }

    public enum PanelEventType
    {
        Attached,
        Detached,
        Found,
        ManuallyFound, // This is when the user explicitly searches for panels. Difference is that in this event chain [AllPanelsFound] will not be sent.
        Created,
        Disposed,
        AllPanelsFound
    }
}
