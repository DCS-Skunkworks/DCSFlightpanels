namespace NonVisuals.EventArgs
{
    using System;
    using System.Collections.Generic;

    using ClassLibraryCommon;

    public class SwitchesChangedEventArgs : EventArgs
    {
        public string HidInstance { get; init; }

        public GamingPanelEnum PanelType { get; init; }

        public HashSet<object> Switches { get; init; }
    }
}
