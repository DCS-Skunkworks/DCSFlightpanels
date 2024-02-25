namespace NonVisuals.Panels.Saitek {
    using ClassLibraryCommon;
    using DCS_BIOS.Serialized;

    public enum PanelLEDColor : byte
    {
        DARK = 0x0,
        GREEN = 0x1,
        YELLOW = 0x2,
        RED = 0x4
    }

    /// <summary>
    /// This is used for mapping a certain DCS-BIOS Control value with a
    /// panel LED.So for example DCS-BIOS GEAR_INDICATOR value of 1
    /// would show a GREEN light on the Switch Panel or the BIP whereas
    /// 0 would show RED.
    /// </summary>

    [SerializeCritical]
    public abstract class DcsOutputAndColorBinding
    {
        public abstract void ImportSettings(string settings);
        public abstract string ExportSettings();

        public PanelLEDColor LEDColor { get; set; }
        public SaitekPanelLEDPosition SaitekLEDPosition { get; set; }

        protected string[] Separator { get; } = { SaitekConstants.SEPARATOR_SYMBOL };

        public DCSBIOSOutput DCSBiosOutputLED { get; set; }
    }
}
