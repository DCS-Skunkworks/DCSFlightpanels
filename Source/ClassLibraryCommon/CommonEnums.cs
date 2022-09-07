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

    public enum CopyContentType
    {
        KeyStroke,
        KeySequence,
        DCSBIOS,
        BIPLink,
        OSCommand
    }

    public enum APIModeEnum
    {
        keybd_event = 0,
        SendInput = 1
    }

    public enum GamingPanelVendorEnum
    {
        Saitek = 0x6A3,
        MadCatz = 0x0738,
        Elgato = 0xFD9,
        CockpitMaster = 0x0483,
    }

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
        [Description("StreamDeck")]
        StreamDeck = 0x0060,
        [Description("StreamDeck V2")]
        StreamDeckV2 = 0x006D,
        [Description("StreamDeck MK.2")]
        StreamDeckMK2 = 0x0080,
        [Description("StreamDeck XL")]
        StreamDeckXL = 0x006C,
        [Description("Logitech Farming Side Panel")]
        FarmingPanel = 0x2218
    }
}
