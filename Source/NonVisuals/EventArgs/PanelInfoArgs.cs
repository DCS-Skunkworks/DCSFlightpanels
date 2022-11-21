namespace NonVisuals.EventArgs
{
    using System;

    using ClassLibraryCommon;

    public class PanelInfoArgs : EventArgs
    {
        public string HidInstance { get; init; }

        public GamingPanelEnum PanelType { get; init; }
    }
}
