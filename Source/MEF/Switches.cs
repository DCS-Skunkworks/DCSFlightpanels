namespace MEF
{
    using System.ComponentModel;

    public enum PluginGamingPanelEnum
    {
        [Description("Unknown Panel")]
        Unknown,
        [Description("Saitek PZ55 Switch Panel")]
        PZ55SwitchPanel,
        [Description("Saitek PZ69 Radio Panel")]
        PZ69RadioPanel,
        [Description("Saitek PZ70 Multi Panel")]
        PZ70MultiPanel,
        [Description("Saitek BIP Back Lit Panel")]
        BackLitPanel,
        [Description("Saitek TPM Panel")]
        TPM,
        [Description("StreamDeck Mini")]
        StreamDeckMini,
        [Description("StreamDeck")]
        StreamDeck,
        [Description("StreamDeck V2")]
        StreamDeckV2,
        [Description("StreamDeck MK.2")]
        StreamDeckMK2,
        [Description("StreamDeck XL")]
        StreamDeckXL,
        [Description("Logitech Farming Side Panel")]
        FarmingPanel,
        [Description("Saitek PZ69 Radio PreProg Panel A10C")]
        PZ69RadioPanel_PreProg_A10C,
        [Description("Saitek PZ69 Radio PreProg Panel AJS37")]
        PZ69RadioPanel_PreProg_AJS37,
        [Description("Saitek PZ69 Radio PreProg Panel AV8BNA")]
        PZ69RadioPanel_PreProg_AV8BNA,
        [Description("Saitek PZ69 Radio PreProg Panel Bf109")]
        PZ69RadioPanel_PreProg_BF109,
        [Description("Saitek PZ69 Radio PreProg Panel F14B")]
        PZ69RadioPanel_PreProg_F14B,
        [Description("Saitek PZ69 Radio PreProg Panel F5E")]
        PZ69RadioPanel_PreProg_F5E,
        [Description("Saitek PZ69 Radio PreProg Panel F86F")]
        PZ69RadioPanel_PreProg_F86F,
        [Description("Saitek PZ69 Radio PreProg Panel FA18C")]
        PZ69RadioPanel_PreProg_FA18C,
        [Description("Saitek PZ69 Radio PreProg Panel Fw190")]
        PZ69RadioPanel_PreProg_FW190,
        [Description("Saitek PZ69 Radio PreProg Panel Ka50")]
        PZ69RadioPanel_PreProg_KA50,
        [Description("Saitek PZ69 Radio PreProg Panel M2000C")]
        PZ69RadioPanel_PreProg_M2000C,
        [Description("Saitek PZ69 Radio PreProg Panel MI24P")]
        PZ69RadioPanel_PreProg_MI24P,
        [Description("Saitek PZ69 Radio PreProg Panel MI8")]
        PZ69RadioPanel_PreProg_MI8,
        [Description("Saitek PZ69 Radio PreProg Panel MiG21Bis")]
        PZ69RadioPanel_PreProg_MIG21BIS,
        [Description("Saitek PZ69 Radio PreProg Panel P47D")]
        PZ69RadioPanel_PreProg_P47D,
        [Description("Saitek PZ69 Radio PreProg Panel P51D")]
        PZ69RadioPanel_PreProg_P51D,
        [Description("Saitek PZ69 Radio PreProg Panel SA342")]
        PZ69RadioPanel_PreProg_SA342,
        [Description("Saitek PZ69 Radio PreProg Panel UH1H")]
        PZ69RadioPanel_PreProg_UH1H,
        [Description("Saitek PZ69 Radio PreProg Panel SpitfireLFMkIX")]
        PZ69RadioPanel_PreProg_SPITFIRELFMKIX,

    }

    /*
     *
     */
     
    public enum KeyPressLength
    {
        // Zero = 0, <-- DCS & keybd_event does not work without delay between key press & release
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

    /*
     *
     */
    public enum EnumStreamDeckButtonNames
    {
        BUTTON0_NO_BUTTON,
        BUTTON1,
        BUTTON2,
        BUTTON3,
        BUTTON4,
        BUTTON5,
        BUTTON6,
        BUTTON7,
        BUTTON8,
        BUTTON9,
        BUTTON10,
        BUTTON11,
        BUTTON12,
        BUTTON13,
        BUTTON14,
        BUTTON15,
        BUTTON16,
        BUTTON17,
        BUTTON18,
        BUTTON19,
        BUTTON20,
        BUTTON21,
        BUTTON22,
        BUTTON23,
        BUTTON24,
        BUTTON25,
        BUTTON26,
        BUTTON27,
        BUTTON28,
        BUTTON29,
        BUTTON30,
        BUTTON31,
        BUTTON32
    }

    /*
     *
     */
    public enum TPMPanelSwitches
    {
        G1 = 0,
        G2 = 2,
        G3 = 4,
        G4 = 8,
        G5 = 16,
        G6 = 32,
        G7 = 64,
        G8 = 128,
        G9 = 256
    }

    /*
     *
     */
    public enum SwitchPanelPZ55Keys
    {
        SWITCHKEY_MASTER_BAT = 0,
        SWITCHKEY_MASTER_ALT = 2,
        SWITCHKEY_AVIONICS_MASTER = 4,
        SWITCHKEY_FUEL_PUMP = 8,
        SWITCHKEY_DE_ICE = 16,
        SWITCHKEY_PITOT_HEAT = 32,
        SWITCHKEY_CLOSE_COWL = 64,
        SWITCHKEY_LIGHTS_PANEL = 128,
        SWITCHKEY_LIGHTS_BEACON = 256,
        SWITCHKEY_LIGHTS_NAV = 512,
        SWITCHKEY_LIGHTS_STROBE = 1024,
        SWITCHKEY_LIGHTS_TAXI = 2048,
        SWITCHKEY_LIGHTS_LANDING = 4096,
        KNOB_ENGINE_OFF = 8192,
        KNOB_ENGINE_RIGHT = 16384,
        KNOB_ENGINE_LEFT = 32768,
        KNOB_ENGINE_BOTH = 65536,
        KNOB_ENGINE_START = 131072,
        LEVER_GEAR_UP = 262144,
        LEVER_GEAR_DOWN = 524288
    }

    /*
     *
     */
    public enum MultiPanelPZ70Knobs
    {
        KNOB_ALT = 0,
        KNOB_VS = 2,
        KNOB_IAS = 4,
        KNOB_HDG = 8,
        KNOB_CRS = 16,
        LCD_WHEEL_INC = 32,
        LCD_WHEEL_DEC = 64,
        AUTO_THROTTLE = 512,
        FLAPS_LEVER_UP = 1024,
        FLAPS_LEVER_DOWN = 256,
        PITCH_TRIM_WHEEL_UP = 2056,
        PITCH_TRIM_WHEEL_DOWN = 128,
        AP_BUTTON = 4096,
        HDG_BUTTON = 8192,
        NAV_BUTTON = 16384,
        IAS_BUTTON = 32768,
        ALT_BUTTON = 65536,
        VS_BUTTON = 131072,
        APR_BUTTON = 262144,
        REV_BUTTON = 524288
    }

    /*
     *
     */
    public enum FarmingPanelMKKeys
    {
        BUTTON_1 = 0,
        BUTTON_2 = 1,
        BUTTON_3 = 2,
        BUTTON_4 = 3,
        BUTTON_5 = 4,
        BUTTON_6 = 5,
        BUTTON_7 = 6,
        BUTTON_8 = 7,
        BUTTON_9 = 8,
        BUTTON_10 = 9,
        BUTTON_11 = 10,
        BUTTON_12 = 11,
        BUTTON_13 = 12,
        BUTTON_14 = 13,
        BUTTON_15 = 14,
        BUTTON_16 = 15,
        BUTTON_17 = 16,
        BUTTON_18 = 17,
        BUTTON_19 = 18,
        BUTTON_20 = 19,
        BUTTON_21 = 20,
        BUTTON_22 = 21,
        BUTTON_23 = 22,
        BUTTON_24 = 23,
        BUTTON_25 = 24,
        BUTTON_26 = 25,
        BUTTON_27 = 26,
        BUTTON_JOY_RIGHT = 27,
        BUTTON_JOY_LEFT = 28
    }

    /*
     *
     */
    public enum RadioPanelPZ69KnobsEmulator
    {
        UpperCOM1 = 0,
        UpperCOM2 = 2,
        UpperNAV1 = 4,
        UpperNAV2 = 8,
        UpperADF = 16,
        UpperDME = 32,
        UpperXPDR = 64,
        UpperSmallFreqWheelInc = 128,
        UpperSmallFreqWheelDec = 256,
        UpperLargeFreqWheelInc = 512,
        UpperLargeFreqWheelDec = 1024,
        UpperFreqSwitch = 2056,
        LowerCOM1 = 4096,
        LowerCOM2 = 8192,
        LowerNAV1 = 16384,
        LowerNAV2 = 32768,
        LowerADF = 65536,
        LowerDME = 131072,
        LowerXPDR = 262144,
        LowerSmallFreqWheelInc = 8388608,
        LowerSmallFreqWheelDec = 524288,
        LowerLargeFreqWheelInc = 1048576,
        LowerLargeFreqWheelDec = 2097152,
        LowerFreqSwitch = 4194304
    }

    public enum PZ69DialPosition
    {
        UpperCOM1 = 0,
        UpperCOM2 = 2,
        UpperNAV1 = 4,
        UpperNAV2 = 8,
        UpperADF = 16,
        UpperDME = 32,
        UpperXPDR = 64,
        LowerCOM1 = 4096,
        LowerCOM2 = 8192,
        LowerNAV1 = 16384,
        LowerNAV2 = 32768,
        LowerADF = 65536,
        LowerDME = 131072,
        LowerXPDR = 262144,
        Unknown = 0x80000
    }

    /*
     *
     */
    public enum CurrentSRSRadioMode
    {
        COM1 = 0,
        COM2 = 2,
        NAV1 = 4,
        NAV2 = 8,
        ADF = 16,
        DME = 32,
        XPDR = 64
    }
 
}
