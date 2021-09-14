namespace NonVisuals
{
    using System;

    using ClassLibraryCommon;

    public class AirframeEventArgs : EventArgs
    {
        public DCSFPProfile Profile { get; set; }
    }
}
