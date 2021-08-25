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

    public enum KeyPressLength
    {
        //Zero = 0, <-- DCS & keybd_event does not work without delay between key press & release
        Indefinite = 999999999,
        ThirtyTwoMilliSec = 32,
        FiftyMilliSec = 50,
        HalfSecond = 500,
        Second = 1000,
        SecondAndHalf = 1500,
        TwoSeconds = 2000,
        ThreeSeconds = 3000,
        FourSeconds = 4000,
        FiveSecs = 5000,
        TenSecs = 10000,
        FifteenSecs = 15000,
        TwentySecs = 20000,
        ThirtySecs = 30000,
        FortySecs = 40000,
        SixtySecs = 60000
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
        Elgato = 0xFD9
    }

    public enum GamingPanelEnum
    {
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
        [Description("StreamDeck XL")]
        StreamDeckXL = 0x006C,
        [Description("Logitech Farming Side Panel")]
        FarmingPanel = 0x2218
    }

    /*
     * Description = What is defined (XYZ) in BIOS.protocol.setExportModuleAircrafts({"XYZ"})
     * Value => not used
     */
    public enum DCSAirframex
    {
        [Description("NoFrameLoadedYet")]
        NOFRAMELOADEDYET,
        [Description("KeyEmulator")]
        KEYEMULATOR,
        [Description("KeyEmulator_SRS")]
        KEYEMULATOR_SRS,
        [Description("A-4E-C")]
        A4E,
        [Description("A-10C")]
        A10C,
        [Description("AH-6J")]
        AH6J,
        [Description("AJS37")]
        AJS37,
        [Description("Alphajet")]
        Alphajet,
        [Description("AV8BNA")]
        AV8BNA,
        [Description("Bf-109K-4")]
        Bf109,
        [Description("C-101CC")]
        C101CC,
        [Description("Christen Eagle II")]
        ChristenEagle,
        [Description("Edge540")]
        Edge540,
        [Description("F-5E-3")]
        F5E,
        [Description("F-14B")]
        F14B,
        [Description("F-16C_50")]
        F16C,
        [Description("FA-18C_hornet")]
        FA18C,
        [Description("F-86F Sabre")]
        F86F,
        [Description("FC3-CD-SRS")]
        FC3_CD_SRS,
        [Description("FW-190A8")]
        Fw190a8,
        [Description("FW-190D9")]
        Fw190d9,
        [Description("C-130")]
        Hercules,
        [Description("I-16")]
        I16,
        [Description("JF-17")]
        JF17,
        [Description("Ka-50")]
        Ka50,
        [Description("L-39ZA")]
        L39ZA,
        [Description("M-2000C")]
        M2000C,
        [Description("MB-339PAN")]
        MB339,
        [Description("Mi-8MT")]
        Mi8,
        [Description("MiG-15bis")]
        Mig15bis,
        [Description("MiG-19P")]
        Mig19P,
        [Description("MiG-21Bis")]
        Mig21Bis,
        [Description("NS430")]
        NS430,
        [Description("P-51D")]
        P51D,
        [Description("P-47D")]
        P47D,
        [Description("SA342M")]
        SA342M,
        [Description("SpitfireLFMkIX")]
        SpitfireLFMkIX,
        [Description("UH-1H")]
        UH1H,
        [Description("Yak-52")]
        Yak52
    }

}
