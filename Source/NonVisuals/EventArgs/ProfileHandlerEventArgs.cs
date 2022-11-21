using NonVisuals.Interfaces;

namespace NonVisuals.EventArgs
{
    using System;

    public class ProfileHandlerEventArgs : EventArgs
    {
        public IProfileHandler ProfileHandlerCaller { get; init; }
    }
}
