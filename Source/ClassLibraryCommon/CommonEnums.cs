using System.ComponentModel;

namespace ClassLibraryCommon
{

    public enum RadioPanelPZ69Display
    {
        Active,
        Standby,
        UpperActive,
        UpperStandby,
        LowerActive,
        LowerStandby
    }

    public enum KeyPressLength
    {
        //Zero = 0, <-- DCS & keybd_event fungerar inte utan fördröjning mellan tangent tryck & släpp
        //Indefinite = 0,
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

    public enum SaitekPanelsEnum
    {
        Unknown = 0,
        PZ55SwitchPanel = 2,
        PZ69RadioPanel = 4,
        PZ70MultiPanel = 8,
        BackLitPanel = 16,
        TPM = 32
    }

    /*
     * Keyemulator
     * DCS-BIOS Profile
     * Radios : DCS-BIOS || SRS
     */

    public enum DCSAirframe
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
        [Description("AJS37")]
        AJS37,
        [Description("AV8BNA")]
        AV8BNA,
        [Description("Bf-109K-4")]
        Bf109,
        [Description("F-5E-3")]
        F5E,
        [Description("F-86F Sabre")]
        F86F,
        [Description("FA-18C_hornet")]
        FA18C,
        [Description("FC3-CD-SRS")]
        FC3_CD_SRS,
        [Description("FW-190D9")]
        Fw190,
        [Description("Ka-50")]
        Ka50,
        [Description("L-39ZA")]
        L39ZA,
        [Description("M2000C")]
        M2000C,
        [Description("MB-339PAN")]
        MB339,
        [Description("Mi-8MT")]
        Mi8,
        [Description("MiG-21bis")]
        Mig21Bis,
        [Description("MiG-15bis")]
        Mig15Bis,
        [Description("NS430")]
        NS430,
        [Description("P-51D")]
        P51D,
        [Description("SA342L")]
        SA342L,
        [Description("SA342M")]
        SA342M,
        [Description("SA342Mistral")]
        SA342Mistral,
        [Description("SpitfireLFMkIX")]
        SpitfireLFMkIX,
        [Description("TF-51D")]
        TF51D,
        [Description("UH-1H")]
        UH1H

    }

    class CommonEnums
    {
    }
}
