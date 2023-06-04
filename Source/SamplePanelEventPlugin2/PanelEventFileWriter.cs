namespace SamplePanelEventPlugin2
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using MEF;
    using NonVisuals.Radios.Knobs;

    public class PanelEventFileWriter
    {
        private static PanelEventFileWriter _panelEventFileWriter;
        private readonly string _filePath;
        private readonly object _lockObject = new();
        private readonly string _filename = "DCSFP_Plugin_Output.txt";

        public PanelEventFileWriter()
        {
            _filePath = AppDomain.CurrentDomain.BaseDirectory;
        }


        public static void WriteEventInfoToFile(string profile, string panelHidId, PluginGamingPanelEnum panelId, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence)
        {
            if (_panelEventFileWriter == null)
            {
                _panelEventFileWriter = new PanelEventFileWriter();
            }
            _panelEventFileWriter.WriteInfoToFile(profile, panelHidId, panelId, switchId, pressed, keySequence);
        }

        private void WriteInfoToFile(string profile, string panelHidId, PluginGamingPanelEnum panelId, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence)
        {
            lock (_lockObject)
            {
                File.AppendAllText(_filePath + _filename, GetInfoFromEnums(profile, panelHidId, panelId, switchId, pressed));
                
                if (keySequence == null)
                    return;

                foreach (var keyPressInfo in keySequence)
                {
                    File.AppendAllText(
                        _filePath + _filename,
                        "\t" + 
                        Enum.GetName(typeof(KeyPressLength), keyPressInfo.Value.LengthOfBreak) + " " +
                        Enum.GetName(typeof(KeyPressLength), keyPressInfo.Value.LengthOfKeyPress) + " " +
                        keyPressInfo.Value.VirtualKeyCodesAsString
                        + "\n");
                }
            }
        }

        private static string GetEventString<T>(int switchId, bool pressed)
        {
            return $" {Enum.GetName(typeof(T), switchId)}  {(pressed == false ? "RELEASED" : "PRESSED")}  ";
        }

        private static string GetInfoFromEnums(string profile, string panelHidId, PluginGamingPanelEnum panel, int switchId, bool pressed)
        {
            var result = $"{DateTime.Now:dd.MM.yyyy hh:mm:ss} {profile} {Enum.GetName(typeof(PluginGamingPanelEnum), panel)}  ";

            result += panel switch
            {
                PluginGamingPanelEnum.PZ55SwitchPanel => GetEventString<SwitchPanelPZ55Keys>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel => GetEventString<RadioPanelPZ69KnobsEmulator>(switchId, pressed),
                PluginGamingPanelEnum.PZ70MultiPanel => GetEventString<MultiPanelPZ70Knobs>(switchId, pressed),
                PluginGamingPanelEnum.TPM => GetEventString<TPMPanelSwitches>(switchId, pressed),
                PluginGamingPanelEnum.FarmingPanel => GetEventString<FarmingPanelMKKeys>(switchId, pressed),
                
                PluginGamingPanelEnum.StreamDeckMK2
                or PluginGamingPanelEnum.StreamDeckV2
                or PluginGamingPanelEnum.StreamDeck
                or PluginGamingPanelEnum.StreamDeckMini
                or PluginGamingPanelEnum.StreamDeckXL
                or PluginGamingPanelEnum.StreamDeckPlus => GetEventString<EnumStreamDeckButtonNames>(switchId, pressed),
                
                PluginGamingPanelEnum.BackLitPanel => "BIP Event - ATTN : this panel cannot produce events",

                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_A10C => GetEventString<RadioPanelPZ69KnobsA10C>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_AJS37 => GetEventString<RadioPanelPZ69KnobsAJS37>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_AV8BNA => GetEventString<RadioPanelPZ69KnobsAV8BNA>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_BF109 => GetEventString<RadioPanelPZ69KnobsBf109>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F14B => GetEventString<RadioPanelPZ69KnobsF14B>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F5E => GetEventString<RadioPanelPZ69KnobsF5E>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F86F => GetEventString<RadioPanelPZ69KnobsF86F>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_FA18C => GetEventString<RadioPanelPZ69KnobsFA18C>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_FW190 => GetEventString<RadioPanelPZ69KnobsFw190>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_KA50 => GetEventString<RadioPanelPZ69KnobsKa50>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_M2000C => GetEventString<RadioPanelPZ69KnobsM2000C>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_MI24P => GetEventString<RadioPanelPZ69KnobsMi24P>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_MI8 => GetEventString<RadioPanelPZ69KnobsMi8>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_MIG21BIS => GetEventString<RadioPanelPZ69KnobsMiG21Bis>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_P47D => GetEventString<RadioPanelPZ69KnobsP47D>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_P51D => GetEventString<RadioPanelPZ69KnobsP51D>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_SA342 => GetEventString<RadioPanelPZ69KnobsSA342>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_UH1H => GetEventString<RadioPanelPZ69KnobsUH1H>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_SPITFIRELFMKIX => GetEventString<RadioPanelPZ69KnobsSpitfireLFMkIX>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_AH64D => GetEventString<RadioPanelPZ69KnobsAH64D>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_T45C => GetEventString<RadioPanelPZ69KnobsT45C>(switchId, pressed),

                PluginGamingPanelEnum.Unknown 
                or _ => "Unknown panel."
            };
            return result += $" {panelHidId}\n";
        }
    }
}
