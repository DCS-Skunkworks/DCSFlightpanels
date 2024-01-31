using System.ComponentModel;

namespace ClassLibraryCommon
{

    public enum RadioPanelPZ69Display
    {
        UpperActive,
        UpperStandby,
        LowerActive,
        LowerStandby
    }
    
    /// <summary>
    /// Used for identifying what a copy package (user copies setting) contains so that the pasting process is easier.
    /// </summary>
    public enum CopyContentType
    {
        KeyStroke,
        KeySequence,
        DCSBIOS,
        BIPLink,
        OSCommand
    }
    
    /// <summary>
    /// The two Windows API:s supported for creating key emulation.
    /// </summary>
    public enum APIModeEnum
    {
        KeybdEvent = 0,
        SendInput = 1
    }

    /// <summary>
    /// The supported manufacturer's Vendor IDs (VID).
    /// </summary>
    public enum GamingPanelVendorEnum
    {
        Saitek = 0x6A3,
        MadCatz = 0x0738,
        Elgato = 0xFD9,
        CockpitMaster = 0x0483
    }

    /// <summary>
    /// The Product ID (PID) of the supported panels.
    /// </summary>
    public enum GamingPanelEnum
    {
        [Description("Cockpit Master 737 CDU")]
        CDU737 = 0x5b36,
        [Description("Unknown Panel")]
        Unknown = 0,
        [Description("Saitek PZ55 Switch Panel")]
        PZ55SwitchPanel = 0xD67,
        [Description("Saitek PZ69 Radio Panel")]
        PZ69RadioPanel = 0xD05,
        [Description("Saitek PZ70 Multi Panel")]
        PZ70MultiPanel = 0xD06,
        [Description("Saitek BIP Back Lit Panel")]
        BackLitPanel = 0xB4E,
        [Description("Saitek TPM Panel")]
        TPM = 0xB4D,
        [Description("StreamDeck Mini")]
        StreamDeckMini = 0x0063,
        [Description("StreamDeck Mini V2")]
        StreamDeckMiniV2 = 0x0090,
        [Description("StreamDeck")]
        StreamDeck = 0x0060,
        [Description("StreamDeck V2")]
        StreamDeckV2 = 0x006D,
        [Description("StreamDeck MK.2")]
        StreamDeckMK2 = 0x0080,
        [Description("StreamDeck XL")]
        StreamDeckXL = 0x006C,
        [Description("StreamDeck XL Rev2")]
        StreamDeckXLRev2 = 0x008F,
        [Description("StreamDeck Plus")]
        StreamDeckPlus = 0x0084,
        [Description("Logitech Farming Side Panel")]
        FarmingPanel = 0x2218
    }

    public enum Axis
    {
        X,
        Y
    }
}
