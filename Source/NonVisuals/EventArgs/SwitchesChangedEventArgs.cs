namespace NonVisuals.EventArgs
{
    using System;
    using System.Collections.Generic;

    using ClassLibraryCommon;

    public class SwitchesChangedEventArgs : EventArgs
    {
        public string HidInstance { get; set; }

        public GamingPanelEnum PanelType { get; set; }

        public HashSet<object> Switches { get; set; }
    }
}
