using ClassLibraryCommon;

namespace NonVisuals.EventArgs
{
    using EventArgs = System.EventArgs;

    public class ProfileEventArgs : EventArgs
    {
        public GenericPanelBinding PanelBinding { get; set; }

        public ProfileEventEnum ProfileEventType { get; set; }

        public DCSFPModule DCSProfile { get; set; }
    }

    public enum ProfileEventEnum
    {
        ProfileLoaded,
        ProfileClosed,
        ProfileSettings
    }
}
