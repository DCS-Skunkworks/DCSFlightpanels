namespace MEF
{
    using System.ComponentModel;

    public enum PluginGamingPanelEnum
    {
        [Description("Unknown Panel")]
        Unknown = 0,
        [Description("Saitek PZ55 Switch Panel")]
        PZ55SwitchPanel = 1,
        [Description("Saitek PZ69 Radio Panel")]
        PZ69RadioPanel = 2,
        [Description("Saitek PZ70 Multi Panel")]
        PZ70MultiPanel = 3,
        [Description("Saitek BIP Back Lit Panel")]
        BackLitPanel = 4,
        [Description("Saitek TPM Panel")]
        TPM = 5,
        [Description("StreamDeck Mini")]
        StreamDeckMini = 6,
        [Description("StreamDeck")]
        StreamDeck = 7,
        [Description("StreamDeck V2")]
        StreamDeckV2 = 8,
        [Description("StreamDeck MK.2")]
        StreamDeckMK2 = 9,
        [Description("StreamDeck XL")]
        StreamDeckXL = 10,
        [Description("Logitech Farming Side Panel")]
        FarmingPanel = 11
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
    public enum CurrentUH1HRadioMode
    {
        INTERCOMM = 0,
        VHFCOMM = 2,
        UHF = 4,
        VHFFM = 8,
        VHFNAV = 16,
        ADF = 32
    }

    public enum RadioPanelPZ69KnobsUH1H
    {
        UPPER_VHFCOMM = 0,
        UPPER_UHF = 2,
        UPPER_VHFNAV = 4,
        UPPER_VHFFM = 8,
        UPPER_ADF = 16,
        UPPER_DME = 32,
        UPPER_INTERCOMM = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHFCOMM = 4096,
        LOWER_UHF = 8192,
        LOWER_VHFNAV = 16384,
        LOWER_VHFFM = 32768,
        LOWER_ADF = 65536,
        LOWER_DME = 131072,
        LOWER_INTERCOMM = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
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

    public enum RadioPanelPZ69KnobsSRS
    {
        UPPER_COM1 = 0,   //COM1
        UPPER_COM2 = 2,  //COM2
        UPPER_NAV1 = 4, //NAV1
        UPPER_NAV2 = 8, //NAV2
        UPPER_ADF = 16, //ADF
        UPPER_DME = 32, //DME_
        UPPER_XPDR = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_COM1 = 4096,
        LOWER_COM2 = 8192,
        LOWER_NAV1 = 16384,
        LOWER_NAV2 = 32768,
        LOWER_ADF = 65536,
        LOWER_DME = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentSpitfireLFMkIXRadioMode
    {
        HFRADIO = 0,
        HFRADIO2 = 2,
        IFF = 4,
        NOUSE = 64
    }

    public enum RadioPanelPZ69KnobsSpitfireLFMkIX
    {
        UPPER_HFRADIO = 0,      //COM1
        UPPER_IFF = 2,      //COM2
        UPPER_NO_USE0 = 4,          //NAV1
        UPPER_NO_USE1 = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_HFRADIO = 4096,   //COM1
        LOWER_IFF = 8192,   //COM2
        LOWER_NO_USE0 = 16384,      //NAV1
        LOWER_NO_USE1 = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentSA342RadioMode
    {
        VHFFM = 2,
        VHFAM = 4,
        UHF = 8,
        ADF = 16,
        NADIR = 32,
        NOUSE
    }

    public enum RadioPanelPZ69KnobsSA342
    {
        UPPER_VHFAM = 0,
        UPPER_VHFFM = 2,
        UPPER_UHF = 4,
        UPPER_NAV2 = 8,
        UPPER_ADF = 16,
        UPPER_NADIR = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHFAM = 4096,
        LOWER_VHFFM = 8192,
        LOWER_UHF = 16384,
        LOWER_NAV2 = 32768,
        LOWER_ADF = 65536,
        LOWER_NADIR = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentP51DRadioMode
    {
        VHF = 0,
        NOUSE = 2
    }

    public enum RadioPanelPZ69KnobsP51D
    {
        UPPER_VHF = 0,   //COM1
        UPPER_NO_USE0 = 2,  //COM2
        UPPER_NO_USE1 = 4, //NAV1
        UPPER_NO_USE2 = 8, //NAV2
        UPPER_NO_USE3 = 16, //ADF
        UPPER_NO_USE4 = 32, //DME_
        UPPER_NO_USE5 = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHF = 4096,
        LOWER_NO_USE0 = 8192,
        LOWER_NO_USE1 = 16384,
        LOWER_NO_USE2 = 32768,
        LOWER_NO_USE3 = 65536,
        LOWER_NO_USE4 = 131072,
        LOWER_NO_USE5 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentP47DRadioMode
    {
        HFRADIO = 0,
        HFRADIO2 = 2,
        //IFF = 4,
        NOUSE = 64
    }

    public enum RadioPanelPZ69KnobsP47D
    {
        UPPER_HFRADIO = 0,      //COM1
        UPPER_NO_USE5 = 2, //IFF = 2,      //COM2
        UPPER_NO_USE0 = 4,          //NAV1
        UPPER_NO_USE1 = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_HFRADIO = 4096,   //COM1
        LOWER_NO_USE5 = 8192, //IFF = 8192,   //COM2
        LOWER_NO_USE0 = 16384,      //NAV1
        LOWER_NO_USE1 = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentMiG21BisRadioMode
    {
        Radio = 0,
        RSBN = 2,
        ARC = 4
    }

    public enum RadioPanelPZ69KnobsMiG21Bis
    {
        UpperRadio = 0,
        UpperCom2 = 2,
        UpperRsbn = 4,
        UpperNav2 = 8,
        UpperArc = 16,
        UpperDme = 32,
        UpperXpdr = 64,
        UpperSmallFreqWheelInc = 128,
        UpperSmallFreqWheelDec = 256,
        UpperLargeFreqWheelInc = 512,
        UpperLargeFreqWheelDec = 1024,
        UpperFreqSwitch = 2056,
        LowerRadio = 4096,
        LowerCom2 = 8192,
        LowerRsbn = 16384,
        LowerNav2 = 32768,
        LowerArc = 65536,
        LowerDme = 131072,
        LowerXpdr = 262144,
        LowerSmallFreqWheelInc = 8388608,
        LowerSmallFreqWheelDec = 524288,
        LowerLargeFreqWheelInc = 1048576,
        LowerLargeFreqWheelDec = 2097152,
        LowerFreqSwitch = 4194304
    }

    /*
     *
     */
    public enum CurrentMi8RadioMode
    {
        R863_MANUAL = 0,
        R863_PRESET = 2,
        YADRO1A = 4,
        R828_PRESETS = 8,
        ADF_ARK9 = 16,
        ARK_UD = 32,
        SPU7 = 64,
        NOUSE = 128
    }

    public enum RadioPanelPZ69KnobsMi8
    {
        UPPER_R863_MANUAL = 0,      //COM1
        UPPER_R863_PRESET = 2,      //COM2
        UPPER_YADRO1A = 4,          //NAV1
        UPPER_R828 = 8,             //NAV2
        UPPER_ADF_ARK9 = 16,       //ADF
        UPPER_ARK_UD = 32,          //DME_
        UPPER_SPU7 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_R863_MANUAL = 4096,   //COM1
        LOWER_R863_PRESET = 8192,   //COM2
        LOWER_YADRO1A = 16384,      //NAV1
        LOWER_R828 = 32768,          //NAV2
        LOWER_ADF_ARK9 = 65536,    //ADF
        LOWER_ARK_UD = 131072,      //DME_
        LOWER_SPU7 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentM2000CRadioMode
    {
        VUHF = 0,
        UHF = 2,
        TACAN = 4,
        VOR = 8,
        NOUSE = 32
    }

    public enum RadioPanelPZ69KnobsM2000C
    {
        UPPER_VUHF = 0,      //COM1
        UPPER_UHF = 2,      //COM2
        UPPER_TACAN = 4,          //NAV1
        UPPER_VOR = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VUHF = 4096,   //COM1
        LOWER_UHF = 8192,   //COM2
        LOWER_TACAN = 16384,      //NAV1
        LOWER_VOR = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentKa50RadioMode
    {
        VHF1_R828 = 0,
        VHF2_R800L1 = 2,
        ADF_ARK22 = 4,
        ABRIS = 8,
        DATALINK = 16,
        NOUSE = 32
    }

    public enum RadioPanelPZ69KnobsKa50
    {
        UPPER_VHF1_R828 = 0,   //COM1
        UPPER_VHF2_R800L1 = 2,  //COM2
        UPPER_ABRIS = 4, //NAV1
        UPPER_DATALINK = 8, //NAV2
        UPPER_ADF_ARK22 = 16, //ADF
        UPPER_NO_USE3 = 32, //DME_
        UPPER_NO_USE4 = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHF1_R828 = 4096,
        LOWER_VHF2_R800L1 = 8192,
        LOWER_ABRIS = 16384,
        LOWER_DATALINK = 32768,
        LOWER_ADF_ARK22 = 65536,
        LOWER_NO_USE3 = 131072,
        LOWER_NO_USE4 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentFw190RadioMode
    {
        FUG16ZY = 0,
        IFF = 2,
        HOMING = 4,
        NOUSE = 64
    }

    public enum RadioPanelPZ69KnobsFw190
    {
        UPPER_FUG16ZY = 0,      //COM1
        UPPER_IFF = 2,      //COM2
        UPPER_HOMING = 4,          //NAV1
        UPPER_NO_USE1 = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_FUG16ZY = 4096,   //COM1
        LOWER_IFF = 8192,   //COM2
        LOWER_HOMING = 16384,      //NAV1
        LOWER_NO_USE1 = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentFA18CRadioMode
    {
        COMM2 = 0,
        VHFFM = 2,
        COMM1 = 4,
        TACAN = 8,
        ILS = 16
    }

    public enum RadioPanelPZ69KnobsFA18C
    {
        UPPER_COMM1 = 0,
        UPPER_COMM2 = 2,
        UPPER_VHFFM = 4,
        UPPER_ILS = 8,
        UPPER_TACAN = 16,
        UPPER_DME = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_COMM1 = 4096,
        LOWER_COMM2 = 8192,
        LOWER_VHFFM = 16384,
        LOWER_ILS = 32768,
        LOWER_TACAN = 65536,
        LOWER_DME = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentF86FRadioMode
    {
        ARC27_PRESET = 0,
        ARC27_VOL = 2,
        ARN6 = 4,
        ARN6_MODES = 8,
        ADF_APX6 = 16,
        NOUSE = 32
    }

    public enum RadioPanelPZ69KnobsF86F
    {
        UPPER_ARC27_PRESET = 0,      //COM1
        UPPER_ARC27_VOL = 2,      //COM2
        UPPER_ARN6 = 4,          //NAV1
        UPPER_ARN6_MODES = 8,             //NAV2
        UPPER_ADF_APX6 = 16,       //ADF
        UPPER_NO_USE1 = 32,          //DME_
        UPPER_NO_USE2 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_ARC27_PRESET = 4096,   //COM1
        LOWER_ARC27_VOL = 8192,   //COM2
        LOWER_ARN6 = 16384,      //NAV1
        LOWER_ARN6_MODES = 32768,          //NAV2
        LOWER_ADF_APX6 = 65536,    //ADF
        LOWER_NO_USE1 = 131072,      //DME_
        LOWER_NO_USE2 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentF5ERadioMode
    {
        UHF = 0,
        TACAN = 8,
        NO_USE = 16
    }

    public enum RadioPanelPZ69KnobsF5E
    {
        UPPER_UHF = 0,
        UPPER_NOUSE1 = 2,
        UPPER_NOUSE2 = 4,
        UPPER_NOUSE3 = 8,
        UPPER_TACAN = 16,
        UPPER_NOUSE4 = 32,
        UPPER_NOUSE5 = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_UHF = 4096,
        LOWER_NOUSE1 = 8192,
        LOWER_NOUSE2 = 16384,
        LOWER_NOUSE3 = 32768,
        LOWER_TACAN = 65536,
        LOWER_NOUSE4 = 131072,
        LOWER_NOUSE5 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentF14RadioMode
    {
        UHF = 0,
        VUHF = 2,
        PLT_TACAN = 4,
        RIO_TACAN = 8,
        LINK4 = 16,
        NOUSE = 32
    }

    public enum RadioPanelPZ69KnobsF14B
    {
        UPPER_UHF = 0,
        UPPER_VUHF = 2,
        UPPER_PLT_TACAN = 4,
        UPPER_RIO_TACAN = 8,
        UPPER_ADF = 16,
        UPPER_LINK4 = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_UHF = 4096,
        LOWER_VUHF = 8192,
        LOWER_PLT_TACAN = 16384,
        LOWER_RIO_TACAN = 32768,
        LOWER_ADF = 65536,
        LOWER_LINK4 = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentBf109RadioMode
    {
        FUG16ZY = 0,
        IFF = 2,
        HOMING = 4,
        NOUSE = 64
    }

    public enum RadioPanelPZ69KnobsBf109
    {
        UPPER_FUG16ZY = 0,      //COM1
        UPPER_IFF = 2,      //COM2
        UPPER_HOMING = 4,          //NAV1
        UPPER_NO_USE1 = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_FUG16ZY = 4096,   //COM1
        LOWER_IFF = 8192,   //COM2
        LOWER_HOMING = 16384,      //NAV1
        LOWER_NO_USE1 = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentAV8BNARadioMode
    {
        COMM1 = 0,
        COMM2 = 2,
        NOUSE = 4
    }

    public enum RadioPanelPZ69KnobsAV8BNA
    {
        UPPER_COMM1 = 0,
        UPPER_COMM2 = 2,
        UPPER_NAV1 = 4,
        UPPER_NAV2 = 8,
        UPPER_ADF = 16,
        UPPER_DME = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_COMM1 = 4096,
        LOWER_COMM2 = 8192,
        LOWER_NAV1 = 16384,
        LOWER_NAV2 = 32768,
        LOWER_ADF = 65536,
        LOWER_DME = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentAJS37RadioMode
    {
        FR22 = 0,
        FR24 = 2,
        TILS = 4,
        NOUSE = 64
    }

    public enum RadioPanelPZ69KnobsAJS37
    {
        UPPER_FR22 = 0,      //COM1
        UPPER_FR24 = 2,      //COM2
        UPPER_TILS = 4,          //NAV1
        UPPER_NO_USE0 = 8,             //NAV2
        UPPER_NO_USE1 = 16,       //ADF
        UPPER_NO_USE2 = 32,          //DME_
        UPPER_NO_USE3 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_FR22 = 4096,   //COM1
        LOWER_FR24 = 8192,   //COM2
        LOWER_TILS = 16384,      //NAV1
        LOWER_NO_USE0 = 32768,          //NAV2
        LOWER_NO_USE1 = 65536,    //ADF
        LOWER_NO_USE2 = 131072,      //DME_
        LOWER_NO_USE3 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    /*
     *
     */
    public enum CurrentA10RadioMode
    {
        UHF = 0,
        VHFFM = 2,
        VHFAM = 4,
        TACAN = 8,
        ILS = 16
    }

    public enum RadioPanelPZ69KnobsA10C
    {
        UPPER_VHFAM = 0,
        UPPER_UHF = 2,
        UPPER_VHFFM = 4,
        UPPER_ILS = 8,
        UPPER_TACAN = 16,
        UPPER_DME = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHFAM = 4096,
        LOWER_UHF = 8192,
        LOWER_VHFFM = 16384,
        LOWER_ILS = 32768,
        LOWER_TACAN = 65536,
        LOWER_DME = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentMi24PRadioMode
    {
        R863_MANUAL = 0,
        R863_PRESET = 2,
        YADRO1A = 4,
        R828_PRESETS = 8,
        ADF_ARK15_HIGH = 16,
        DME_ARK15_LOW = 32,
        SPU8 = 64,
        NOUSE = 128
    }

    public enum RadioPanelPZ69KnobsMi24P
    {
        UPPER_R863_MANUAL = 0,      //COM1
        UPPER_R863_PRESET = 2,      //COM2
        UPPER_YADRO1A = 4,          //NAV1
        UPPER_R828 = 8,             //NAV2
        UPPER_ADF_ARK15 = 16,       //ADF
        UPPER_ARK_UD = 32,          //DME_
        UPPER_SPU8 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_R863_MANUAL = 4096,   //COM1
        LOWER_R863_PRESET = 8192,   //COM2
        LOWER_YADRO1A = 16384,      //NAV1
        LOWER_R828 = 32768,          //NAV2
        LOWER_ADF_ARK15 = 65536,    //ADF
        LOWER_ARK_UD = 131072,      //DME_
        LOWER_SPU8 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }
}
