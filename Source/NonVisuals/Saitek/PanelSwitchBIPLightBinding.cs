namespace NonVisuals.Saitek
{
    using System;

    using Newtonsoft.Json;

    using NonVisuals.Saitek.Panels;

    [Serializable]
    public abstract class PanelSwitchBIPLightBinding
    {
        public abstract void ImportSettings(string settings);

        public abstract string ExportSettings();


        [JsonProperty("LEDColor", Required = Required.Default)]
        public PanelLEDColor LEDColor { get; set; }

        [JsonProperty("BIPLedPosition", Required = Required.Default)]
        public BIPLedPositionEnum BIPLedPosition { get; set; }

        [JsonProperty("DelayBefore", Required = Required.Default)]
        public BIPLightDelays DelayBefore { get; set; }

        [JsonProperty("BindingHash", Required = Required.Default)]
        public string BindingHash { get; set; }

        [JsonProperty("Separator", Required = Required.Default)]
        public string[] Separator { get; } = { "|" };
    }
}
