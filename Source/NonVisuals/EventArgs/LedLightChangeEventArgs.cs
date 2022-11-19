namespace NonVisuals.EventArgs
{
    using System;
    using Panels.Saitek;

    public class LedLightChangeEventArgs : EventArgs
    {
        public string HIDInstance { get; set; }

        public SaitekPanelLEDPosition LEDPosition { get; set; }

        public PanelLEDColor LEDColor { get; set; }
    }
}
