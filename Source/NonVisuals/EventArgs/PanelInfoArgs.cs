namespace NonVisuals.EventArgs
{
    using System;

    using ClassLibraryCommon;

    public class PanelInfoArgs : EventArgs
    {
        public string HidInstance { get; set; }

        public GamingPanelEnum PanelType { get; set; }
    }
}
