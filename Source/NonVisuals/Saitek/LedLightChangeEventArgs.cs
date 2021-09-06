namespace NonVisuals.Saitek
{
    using System;

    public class LedLightChangeEventArgs : EventArgs
    {
        public string UniqueId { get; set; }

        public SaitekPanelLEDPosition LEDPosition { get; set; }

        public PanelLEDColor LEDColor { get; set; }
    }
}
