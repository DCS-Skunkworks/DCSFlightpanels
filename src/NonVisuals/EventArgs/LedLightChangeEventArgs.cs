namespace NonVisuals.EventArgs
{
    using System;
    using Panels.Saitek;

    public class LedLightChangeEventArgs : EventArgs
    {
        public string HIDInstance { get; init; }

        public SaitekPanelLEDPosition LEDPosition { get; init; }

        public PanelLEDColor LEDColor { get; init; }
    }
}
