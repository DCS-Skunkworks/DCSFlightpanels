namespace NonVisuals.EventArgs
{
    using System;

    public class PanelEventArgs : EventArgs
    {
        public string HidInstance { get; set; }

        public HIDSkeleton HidSkeleton { get; set; }
        
        public PanelEventType EventType { get; set; }

        public string OldHidInstance { get; set; }
    }

    public enum PanelEventType
    {
        /// <summary>
        /// Triggered when a panel is re-inserted and has the same HID ID as before
        /// </summary>
        Attached,
        /// <summary>
        /// Triggered when panel removed
        /// </summary>
        Detached,
        /// <summary>
        /// Triggered when DCSFP has automatically found a panel
        /// </summary>
        Found,
        /// <summary>
        /// This is when the user explicitly searches for panels. Difference is that in this event chain [AllPanelsFound] will not be sent.
        /// </summary>
        ManuallyFound, 
        /// <summary>
        /// Triggered when a Panel has been created
        /// </summary>
        Created,
        /// <summary>
        /// Triggered when a Panel has been disposed
        /// </summary>
        Disposed,
        /// <summary>
        /// Triggered when automatic search for panel has ended
        /// </summary>
        AllPanelsFound
    }
}
