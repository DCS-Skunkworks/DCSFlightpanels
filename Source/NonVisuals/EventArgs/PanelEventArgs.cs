namespace NonVisuals.EventArgs
{
    using System;

    using ClassLibraryCommon;

    public class PanelEventArgs : EventArgs
    {
        public string HidInstance { get; set; }

        public GamingPanelEnum PanelType { get; set; }
    }
}
