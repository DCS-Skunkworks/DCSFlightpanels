﻿using ClassLibraryCommon;

namespace NonVisuals.EventArgs
{
    using EventArgs = System.EventArgs;

    public class ProfileEventArgs : EventArgs
    {
        public GenericPanelBinding PanelBinding { get; init; }

        public ProfileEventEnum ProfileEventType { get; init; }

        public DCSFPProfile DCSProfile { get; set; }
    }

    public enum ProfileEventEnum
    {
        ProfileLoaded,
        ProfileClosed,
        ProfileSettings
    }
}
