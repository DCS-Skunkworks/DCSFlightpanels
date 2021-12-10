namespace NonVisuals.EventArgs
{
    using System;

    using NonVisuals.Saitek;

    public class LedLightChangeEventArgs : EventArgs
    {
        public string HIDInstanceId { get; set; }

        public SaitekPanelLEDPosition LEDPosition { get; set; }

        public PanelLEDColor LEDColor { get; set; }
    }
}
